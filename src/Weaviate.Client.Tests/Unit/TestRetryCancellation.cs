using System.Net;

namespace Weaviate.Client.Tests.Unit;

public class TestRetryCancellation
{
    private sealed class Counting503Handler : DelegatingHandler
    {
        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Attempts++;
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("{\"error\":\"unavailable\"}"),
                }
            );
        }
    }

    [Fact]
    public async Task CancellationDuringBackoff_AbortsFurtherRetries()
    {
        var countingHandler = new Counting503Handler();
        var retryPolicy = new RetryPolicy
        {
            MaxRetries = 5,
            InitialDelay = TimeSpan.FromMilliseconds(500), // long enough to allow cancellation
            BackoffMultiplier = 2.0,
            RetryOn = RetryOn.ServiceUnavailable,
        };
        var config = new ClientConfiguration(RestPort: 1234, RetryPolicy: retryPolicy);
        // Create a no-op gRPC client to avoid meta fetch during construction (Meta is accessed when real gRPC client is created).
        var noOpGrpc = new Weaviate.Client.Grpc.WeaviateGrpcClient(
            Weaviate.Client.Tests.Unit.Mocks.NoOpGrpcChannel.Create()
        );
        using var retryClient = new WeaviateClient(
            configuration: config,
            httpMessageHandler: countingHandler,
            logger: null,
            grpcClient: noOpGrpc
        );
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // cancel during first backoff sleep

        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            retryClient.GetMeta(cts.Token)
        );
        Assert.NotNull(ex);
        Assert.Equal(1, countingHandler.Attempts); // Only the initial attempt should have occurred (no retry after cancellation during backoff)
    }
}

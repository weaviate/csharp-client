using System.Net;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The test retry cancellation class
/// </summary>
public class TestRetryCancellation
{
    /// <summary>
    /// The counting 503 handler class
    /// </summary>
    /// <seealso cref="DelegatingHandler"/>
    private sealed class Counting503Handler : DelegatingHandler
    {
        /// <summary>
        /// Gets or sets the value of the attempts
        /// </summary>
        public int Attempts { get; private set; }

        /// <summary>
        /// Sends the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the http response message</returns>
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

    /// <summary>
    /// Tests that cancellation during backoff aborts further retries
    /// </summary>
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
        var noOpGrpc = new Grpc.WeaviateGrpcClient(
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

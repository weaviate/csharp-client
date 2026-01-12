using System.Net;
using System.Text;
using System.Text.Json;
using Weaviate.Client.Rest;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The test well known class
/// </summary>
public class TestWellKnown
{
    /// <summary>
    /// The fake handler class
    /// </summary>
    /// <seealso cref="HttpMessageHandler"/>
    private class FakeHandler : HttpMessageHandler
    {
        /// <summary>
        /// The responder
        /// </summary>
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        /// <summary>
        /// Gets the value of the requests
        /// </summary>
        public List<HttpRequestMessage> Requests { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeHandler"/> class
        /// </summary>
        /// <param name="responder">The responder</param>
        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

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
            // Intercept requests to the Meta endpoint and return a dummy response
            if (
                request.RequestUri != null
                && request.RequestUri.AbsolutePath.EndsWith(WeaviateEndpoints.Meta())
                && request.Method == HttpMethod.Get
            )
            {
                var dummyMeta = new
                {
                    hostname = "http://localhost:8080",
                    version = "1.28.0",
                    modules = new { },
                    grpcMaxMessageSize = 10485760UL,
                };

                var json = JsonSerializer.Serialize(dummyMeta);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = content }
                );
            }

            Requests.Add(request);
            return Task.FromResult(_responder(request));
        }
    }

    /// <summary>
    /// Tests that live returns true on 200
    /// </summary>
    [Fact]
    public async Task Live_ReturnsTrue_On200()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        Assert.True(await client.IsLive(TestContext.Current.CancellationToken));
        Assert.Equal(
            "v1/.well-known/live",
            handler.Requests.Single().RequestUri!.AbsolutePath.TrimStart('/')
        );
    }

    /// <summary>
    /// Tests that is ready returns true on 200
    /// </summary>
    [Fact]
    public async Task IsReady_ReturnsTrue_On200()
    {
        var handler = new FakeHandler(req =>
            req.RequestUri!.AbsolutePath.EndsWith("ready")
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : new HttpResponseMessage(HttpStatusCode.InternalServerError)
        );

        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        Assert.True(await client.IsReady(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Tests that wait until ready succeeds before timeout
    /// </summary>
    [Fact]
    public async Task WaitUntilReady_SucceedsBeforeTimeout()
    {
        int calls = 0;
        var handler = new FakeHandler(req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("ready"))
            {
                calls++;
                if (calls < 3)
                {
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        var result = await client.WaitUntilReady(
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(10),
            TestContext.Current.CancellationToken
        );
        Assert.True(result);
        Assert.True(calls >= 3);
    }

    /// <summary>
    /// Tests that wait until ready times out
    /// </summary>
    [Fact]
    public async Task WaitUntilReady_TimesOut()
    {
        var handler = new FakeHandler(req => new HttpResponseMessage(
            HttpStatusCode.ServiceUnavailable
        ));
        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        var result = await client.WaitUntilReady(
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(10),
            TestContext.Current.CancellationToken
        );
        Assert.False(result);
    }

    /// <summary>
    /// Tests that wait until ready cancellation
    /// </summary>
    [Fact]
    public async Task WaitUntilReady_Cancellation()
    {
        var cts = new CancellationTokenSource();
        int calls = 0;
        var handler = new FakeHandler(req =>
        {
            calls++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });
        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        var task = client.WaitUntilReady(
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(10),
            cts.Token
        );
        cts.CancelAfter(50);
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        Assert.True(calls > 0);
    }
}

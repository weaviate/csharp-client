using System.Net;
using System.Text;
using System.Text.Json;
using Weaviate.Client.Rest;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public class TestWellKnown
{
    private class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public List<HttpRequestMessage> Requests { get; } = new();

        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

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

    [Fact]
    public async Task Live_ReturnsTrue_On200()
    {
        var handler = new FakeHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        Assert.True(await client.Live(TestContext.Current.CancellationToken));
        Assert.Equal(
            "v1/.well-known/live",
            handler.Requests.Single().RequestUri!.AbsolutePath.TrimStart('/')
        );
    }

    [Fact]
    public async Task IsReady_ReturnsTrue_On200()
    {
        var handler = new FakeHandler(req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("ready"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        Assert.True(await client.IsReady(TestContext.Current.CancellationToken));
    }

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
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        var result = await client.WaitUntilReady(
            TimeSpan.FromSeconds(2),
            TestContext.Current.CancellationToken,
            TimeSpan.FromMilliseconds(10)
        );
        Assert.True(result);
        Assert.True(calls >= 3);
    }

    [Fact]
    public async Task WaitUntilReady_TimesOut()
    {
        var handler = new FakeHandler(req => new HttpResponseMessage(
            HttpStatusCode.ServiceUnavailable
        ));
        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        var result = await client.WaitUntilReady(
            TimeSpan.FromMilliseconds(100),
            TestContext.Current.CancellationToken,
            TimeSpan.FromMilliseconds(10)
        );
        Assert.False(result);
    }

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
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(noOpChannel);
        var client = new WeaviateClient(new ClientConfiguration(), handler, grpcClient: grpcClient);
        var task = client.WaitUntilReady(
            TimeSpan.FromSeconds(5),
            cts.Token,
            TimeSpan.FromMilliseconds(10)
        );
        cts.CancelAfter(50);
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        Assert.True(calls > 0);
    }
}

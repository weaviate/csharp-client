using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Weaviate.Client;
using Xunit;

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
            Requests.Add(request);
            return Task.FromResult(_responder(request));
        }
    }

    [Fact]
    public async Task Live_ReturnsTrue_On200()
    {
        var handler = new FakeHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        var client = new WeaviateClient(new ClientConfiguration(), handler);
        Assert.True(await client.Live());
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
        var client = new WeaviateClient(new ClientConfiguration(), handler);
        Assert.True(await client.IsReady());
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
        var client = new WeaviateClient(new ClientConfiguration(), handler);
        var result = await client.WaitUntilReady(
            TimeSpan.FromSeconds(2),
            CancellationToken.None,
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
        var client = new WeaviateClient(new ClientConfiguration(), handler);
        var result = await client.WaitUntilReady(
            TimeSpan.FromMilliseconds(100),
            CancellationToken.None,
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
        var client = new WeaviateClient(new ClientConfiguration(), handler);
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

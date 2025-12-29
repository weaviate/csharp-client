#if ENABLE_INTERNAL_TESTS
using Weaviate.Client.Grpc;
using Weaviate.Client.Grpc.Protobuf.V1;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public class TestGrpcCancellation
{
    private static WeaviateGrpcClient CreateGrpcClientWithSearchDelay(TimeSpan delay)
    {
        var channel = NoOpGrpcChannel.Create(
            customHandler: null,
            customAsyncHandler: async (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    await Task.Delay(delay, ct);
                    ct.ThrowIfCancellationRequested();
                    var reply = new SearchReply();
                    return Helpers.CreateGrpcResponse(reply);
                }
                return null;
            }
        );
        return new WeaviateGrpcClient(channel);
    }

    [Fact]
    public async Task Search_PreCancelledToken_Throws()
    {
        var grpcClient = CreateGrpcClientWithSearchDelay(TimeSpan.FromMilliseconds(200));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var ex = await Assert.ThrowsAsync<WeaviateServerException>(() =>
            grpcClient.FetchObjects("TestCollection", cancellationToken: cts.Token)
        );

        // Verify the inner exception is an RpcException with Cancelled status
        Assert.IsType<global::Grpc.Core.RpcException>(ex.InnerException);
        var rpcEx = (global::Grpc.Core.RpcException)ex.InnerException!;
        Assert.Equal(global::Grpc.Core.StatusCode.Cancelled, rpcEx.StatusCode);
    }

    [Fact]
    public async Task Search_CancellationDuringDelay_Throws()
    {
        var grpcClient = CreateGrpcClientWithSearchDelay(TimeSpan.FromMilliseconds(250));
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // cancel during artificial server delay

        var ex = await Assert.ThrowsAsync<WeaviateServerException>(() =>
            grpcClient.FetchObjects("TestCollection", cancellationToken: cts.Token)
        );

        // Verify the inner exception is an RpcException with Cancelled status
        Assert.IsType<global::Grpc.Core.RpcException>(ex.InnerException);
        var rpcEx = (global::Grpc.Core.RpcException)ex.InnerException!;
        Assert.Equal(global::Grpc.Core.StatusCode.Cancelled, rpcEx.StatusCode);
    }

    [Fact]
    public async Task Search_NoCancellation_Succeeds()
    {
        var grpcClient = CreateGrpcClientWithSearchDelay(TimeSpan.FromMilliseconds(10));
        var reply = await grpcClient.FetchObjects(
            "TestCollection",
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(reply);
    }
}
#endif

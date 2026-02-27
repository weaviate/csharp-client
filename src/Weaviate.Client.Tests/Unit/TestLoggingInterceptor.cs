using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Weaviate.Client.Grpc;
using Weaviate.Client.Grpc.Protobuf.V1;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public class TestLoggingInterceptor
{
    /// <summary>
    /// Creates a WeaviateGrpcClient with logging enabled, using a channel that responds to Search.
    /// </summary>
    private static (WeaviateGrpcClient GrpcClient, TestLogger<LoggingInterceptor> Logger) Build()
    {
        var logger = new TestLogger<LoggingInterceptor>();
        var loggerFactory = new TestLoggerFactory<LoggingInterceptor>(logger);

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    var reply = new SearchReply { Collection = "TestCollection" };
                    return Task.FromResult<HttpResponseMessage?>(Helpers.CreateGrpcResponse(reply));
                }
                return Task.FromResult<HttpResponseMessage?>(null);
            }
        );

        var grpcClient = new WeaviateGrpcClient(
            channel,
            loggerFactory: loggerFactory,
            logRequests: true
        );
        return (grpcClient, logger);
    }

    [Fact]
    public async Task LogsGrpcMethodNameOnCall()
    {
        var (grpcClient, logger) = Build();

        await grpcClient.FetchObjects(
            "TestCollection",
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Contains(
            logger.Entries,
            e => e.Message.Contains("Search") || e.Message.Contains("gRPC")
        );
    }

    [Fact]
    public async Task LogsResponseStatusCode()
    {
        var (grpcClient, logger) = Build();

        await grpcClient.FetchObjects(
            "TestCollection",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Should contain some status indicator (OK or status code)
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug);
    }

    [Fact]
    public async Task LogsElapsedTimeOnCompletion()
    {
        var (grpcClient, logger) = Build();

        await grpcClient.FetchObjects(
            "TestCollection",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // At least one log entry should mention elapsed time
        Assert.Contains(logger.Entries, e => e.Message.Contains("ms"));
    }

    [Fact]
    public async Task LogsWarningOnGrpcError()
    {
        var logger = new TestLogger<LoggingInterceptor>();
        var loggerFactory = new TestLoggerFactory<LoggingInterceptor>(logger);

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    throw new RpcException(
                        new Status(StatusCode.Unavailable, "Service unavailable")
                    );
                }
                return Task.FromResult<HttpResponseMessage?>(null);
            }
        );

        var grpcClient = new WeaviateGrpcClient(
            channel,
            loggerFactory: loggerFactory,
            logRequests: true
        );

        await Assert.ThrowsAsync<WeaviateServerException>(() =>
            grpcClient.FetchObjects(
                "TestCollection",
                cancellationToken: TestContext.Current.CancellationToken
            )
        );

        // Error path should log a Warning
        Assert.Contains(logger.Entries, e => e.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task DoesNotLogWhenLoggingDisabled()
    {
        var logger = new TestLogger<LoggingInterceptor>();
        var loggerFactory = new TestLoggerFactory<LoggingInterceptor>(logger);

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    var reply = new SearchReply { Collection = "TestCollection" };
                    return Task.FromResult<HttpResponseMessage?>(Helpers.CreateGrpcResponse(reply));
                }
                return Task.FromResult<HttpResponseMessage?>(null);
            }
        );

        // logRequests: false — no logging interceptor should be added
        var grpcClient = new WeaviateGrpcClient(
            channel,
            loggerFactory: loggerFactory,
            logRequests: false
        );

        await grpcClient.FetchObjects(
            "TestCollection",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Logger should have received NO entries about gRPC calls
        Assert.Empty(logger.Entries);
    }
}

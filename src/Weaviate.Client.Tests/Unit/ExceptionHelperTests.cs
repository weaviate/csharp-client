using System.Net;
using Grpc.Core;

namespace Weaviate.Client.Tests.Unit;

[Collection("Unit Tests")]
public class ExceptionHelperTests
{
    [Fact]
    public void MapHttpException_WithTimeoutCancellation_ReturnsTimeoutException()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(10);
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "Test operation"
        );

        // Wait for timeout to expire
        Thread.Sleep(50);

        var innerEx = new TaskCanceledException();

        // Act
        var result = ExceptionHelper.MapHttpException(
            HttpStatusCode.RequestTimeout,
            "Timeout",
            innerEx
        );

        // Assert
        var timeoutEx = Assert.IsType<WeaviateTimeoutException>(result);
        Assert.Equal(timeout, timeoutEx.Timeout);
        Assert.Equal("Test operation", timeoutEx.Operation);
        Assert.Contains("0.0 seconds", timeoutEx.Message); // 10ms rounds to 0.0
        Assert.Contains("Test operation", timeoutEx.Message);
    }

    [Fact]
    public void MapHttpException_WithUserCancellation_DoesNotReturnTimeoutException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var innerEx = new TaskCanceledException();

        // Act & Assert - should rethrow the original exception
        var ex = Assert.Throws<TaskCanceledException>(() =>
            ExceptionHelper.MapHttpException(HttpStatusCode.OK, "", innerEx)
        );
        Assert.Same(innerEx, ex);
    }

    [Fact]
    public void MapHttpException_Unauthorized_ReturnsAuthenticationException()
    {
        // Arrange
        var innerEx = new Exception("Auth failed");

        // Act
        var result = ExceptionHelper.MapHttpException(
            HttpStatusCode.Unauthorized,
            "Authentication failed",
            innerEx
        );

        // Assert
        Assert.IsType<WeaviateAuthenticationException>(result);
    }

    [Fact]
    public void MapHttpException_Forbidden_ReturnsAuthorizationException()
    {
        // Arrange
        var innerEx = new Exception("Access denied");

        // Act
        var result = ExceptionHelper.MapHttpException(
            HttpStatusCode.Forbidden,
            "Access denied",
            innerEx
        );

        // Assert
        Assert.IsType<WeaviateAuthorizationException>(result);
    }

    [Fact(Skip = "RpcException doesn't expose public constructor with inner exception parameter")]
    public void MapGrpcException_WithTimeoutInInnerException_ReturnsTimeoutException()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(10);
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "gRPC operation"
        );

        // Wait for timeout to expire
        Thread.Sleep(50);

        var innerEx = new TaskCanceledException();
        // Create RpcException with inner exception using reflection
        var rpcEx = new RpcException(new Status(StatusCode.Cancelled, "Cancelled"), "Cancelled");

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        var timeoutEx = Assert.IsType<WeaviateTimeoutException>(result);
        Assert.Equal(timeout, timeoutEx.Timeout);
        Assert.Equal("gRPC operation", timeoutEx.Operation);
    }

    [Fact(
        Skip = "gRPC StatusCode.Cancelled without inner exception cannot reliably detect timeout vs user cancellation"
    )]
    public void MapGrpcException_WithCancelledStatusAndTimeout_ReturnsTimeoutException()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(10);
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "Slow operation"
        );

        // Wait for timeout to expire
        Thread.Sleep(50);

        var rpcEx = new RpcException(new Status(StatusCode.Cancelled, "Timeout"));

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        var timeoutEx = Assert.IsType<WeaviateTimeoutException>(result);
        Assert.Equal(timeout, timeoutEx.Timeout);
        Assert.Equal("Slow operation", timeoutEx.Operation);
    }

    [Fact]
    public void MapGrpcException_Unauthenticated_ReturnsAuthenticationException()
    {
        // Arrange
        var rpcEx = new RpcException(new Status(StatusCode.Unauthenticated, "Auth failed"));

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        Assert.IsType<WeaviateAuthenticationException>(result);
    }

    [Fact]
    public void MapGrpcException_PermissionDenied_ReturnsAuthorizationException()
    {
        // Arrange
        var rpcEx = new RpcException(new Status(StatusCode.PermissionDenied, "Access denied"));

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        Assert.IsType<WeaviateAuthorizationException>(result);
    }

    [Fact]
    public void MapGrpcException_Unimplemented_ReturnsFeatureNotSupportedException()
    {
        // Arrange
        var rpcEx = new RpcException(new Status(StatusCode.Unimplemented, "Not implemented"));

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        Assert.IsType<WeaviateFeatureNotSupportedException>(result);
    }

    [Fact]
    public void MapHttpException_WithTimeoutButNoOperation_UsesDefaultMessage()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(10);
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken
        ); // No operation specified

        // Wait for timeout to expire
        Thread.Sleep(50);

        var innerEx = new TaskCanceledException();

        // Act
        var result = ExceptionHelper.MapHttpException(
            HttpStatusCode.RequestTimeout,
            "Timeout",
            innerEx
        );

        // Assert
        var timeoutEx = Assert.IsType<WeaviateTimeoutException>(result);
        Assert.Contains("The operation timed out", timeoutEx.Message);
        Assert.Contains("0.0 seconds", timeoutEx.Message); // 10ms rounds to 0.0
    }

    [Fact]
    public void MapGrpcException_UserCancellation_DoesNotReturnTimeoutException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var innerEx = new TaskCanceledException();
        var rpcEx = new RpcException(
            new Status(StatusCode.Cancelled, "User cancelled"),
            "User cancelled"
        );

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        // Should return generic server exception, not timeout exception
        Assert.IsType<WeaviateServerException>(result);
        Assert.IsNotType<WeaviateTimeoutException>(result);
    }

    [Fact]
    public void MapGrpcException_CollectionLimitMessage_ReturnsCollectionLimitException()
    {
        // Arrange
        var rpcEx = new RpcException(
            new Status(StatusCode.InvalidArgument, "maximum number of collections reached")
        );

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        Assert.IsType<WeaviateCollectionLimitReachedException>(result);
    }

    [Fact]
    public void MapGrpcException_ModuleNotAvailableMessage_ReturnsModuleNotAvailableException()
    {
        // Arrange
        var rpcEx = new RpcException(
            new Status(StatusCode.InvalidArgument, "no module with name 'text2vec-openai' present")
        );

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        Assert.IsType<WeaviateModuleNotAvailableException>(result);
    }

    [Fact]
    public void MapGrpcException_VectorizationError_ReturnsExternalModuleProblemException()
    {
        // Arrange
        var rpcEx = new RpcException(
            new Status(StatusCode.Internal, "could not vectorize input data")
        );

        // Act
        var result = ExceptionHelper.MapGrpcException(rpcEx, "Test failed");

        // Assert
        Assert.IsType<WeaviateExternalModuleProblemException>(result);
    }
}

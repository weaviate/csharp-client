using System.Net;
using Grpc.Core;
using Weaviate.Client.Internal;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The exception helper tests class
/// </summary>
public class ExceptionHelperTests
{
    /// <summary>
    /// Tests that map http exception with timeout cancellation returns timeout exception
    /// </summary>
    [Fact]
    public async Task MapHttpException_WithTimeoutCancellation_ReturnsTimeoutException()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(10);
        using var cts = new CancellationTokenSource(timeout);

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cts.Token,
            TestContext.Current.CancellationToken
        );

        _ = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: linkedCts.Token,
            operation: "Test operation"
        );

        // Wait for timeout to expire
        await Task.FromResult(cts.Token.WaitHandle.WaitOne());

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

    /// <summary>
    /// Tests that map http exception with user cancellation does not return timeout exception
    /// </summary>
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

    /// <summary>
    /// Tests that map http exception unauthorized returns authentication exception
    /// </summary>
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

    /// <summary>
    /// Tests that map http exception forbidden returns authorization exception
    /// </summary>
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

    /// <summary>
    /// Tests that map grpc exception unauthenticated returns authentication exception
    /// </summary>
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

    /// <summary>
    /// Tests that map grpc exception permission denied returns authorization exception
    /// </summary>
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

    /// <summary>
    /// Tests that map grpc exception unimplemented returns feature not supported exception
    /// </summary>
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

    /// <summary>
    /// Tests that map http exception with timeout but no operation uses default message
    /// </summary>
    [Fact]
    public void MapHttpException_WithTimeoutButNoOperation_UsesDefaultMessage()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(10);
        _ = TimeoutHelper.GetCancellationToken(
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

    /// <summary>
    /// Tests that map grpc exception user cancellation does not return timeout exception
    /// </summary>
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

    /// <summary>
    /// Tests that map grpc exception collection limit message returns collection limit exception
    /// </summary>
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

    /// <summary>
    /// Tests that map grpc exception module not available message returns module not available exception
    /// </summary>
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

    /// <summary>
    /// Tests that map grpc exception vectorization error returns external module problem exception
    /// </summary>
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

    /// <summary>
    /// Tests that map http exception bad request returns bad request exception
    /// </summary>
    [Fact]
    public void MapHttpException_BadRequest_ReturnsBadRequestException()
    {
        // Arrange
        var innerEx = new Rest.WeaviateUnexpectedStatusCodeException(
            HttpStatusCode.BadRequest,
            new HashSet<HttpStatusCode> { HttpStatusCode.OK },
            "Bad request"
        );

        // Act
        var result = ExceptionHelper.MapHttpException(
            HttpStatusCode.BadRequest,
            "Bad request",
            innerEx
        );

        // Assert
        Assert.IsType<WeaviateBadRequestException>(result);
    }

    /// <summary>
    /// Tests that map http exception unprocessable entity returns unprocessable entity exception
    /// </summary>
    [Fact]
    public void MapHttpException_UnprocessableEntity_ReturnsUnprocessableEntityException()
    {
        // Arrange
        var innerEx = new Rest.WeaviateUnexpectedStatusCodeException(
            HttpStatusCode.UnprocessableEntity,
            new HashSet<HttpStatusCode> { HttpStatusCode.OK },
            "Unprocessable entity"
        );

        // Act
        var result = ExceptionHelper.MapHttpException(
            HttpStatusCode.UnprocessableEntity,
            "Unprocessable entity",
            innerEx
        );

        // Assert
        Assert.IsType<WeaviateUnprocessableEntityException>(result);
    }

    /// <summary>
    /// Tests that map http exception unprocessable entity with backup conflict message returns backup conflict exception
    /// </summary>
    [Fact]
    public void MapHttpException_UnprocessableEntityWithBackupConflictMessage_ReturnsBackupConflictException()
    {
        // Arrange
        var innerEx = new Rest.WeaviateUnexpectedStatusCodeException(
            HttpStatusCode.UnprocessableEntity,
            new HashSet<HttpStatusCode> { HttpStatusCode.OK },
            "backup concurrent-backup-123 already in progress"
        );

        // Act
        var result = ExceptionHelper.MapHttpException(
            HttpStatusCode.UnprocessableEntity,
            "backup concurrent-backup-123 already in progress",
            innerEx
        );

        // Assert
        var backupConflictEx = Assert.IsType<WeaviateBackupConflictException>(result);
        Assert.Contains("already in progress", backupConflictEx.Message);
    }

    /// <summary>
    /// Tests that map http exception unprocessable entity with restore conflict message returns backup conflict exception
    /// </summary>
    [Fact]
    public void MapHttpException_UnprocessableEntityWithRestoreConflictMessage_ReturnsBackupConflictException()
    {
        // Arrange
        var innerEx = new Rest.WeaviateUnexpectedStatusCodeException(
            HttpStatusCode.UnprocessableEntity,
            new HashSet<HttpStatusCode> { HttpStatusCode.OK },
            "restoration restore-456 already in progress"
        );

        // Act
        var result = ExceptionHelper.MapHttpException(
            HttpStatusCode.UnprocessableEntity,
            "restoration restore-456 already in progress",
            innerEx
        );

        // Assert
        var backupConflictEx = Assert.IsType<WeaviateBackupConflictException>(result);
        Assert.Contains("already in progress", backupConflictEx.Message);
    }

    /// <summary>
    /// Tests that map http exception internal server error with conflict message does not return backup conflict exception
    /// </summary>
    [Fact]
    public void MapHttpException_InternalServerErrorWithConflictMessage_DoesNotReturnBackupConflictException()
    {
        // Arrange - 500 status code should NOT trigger backup conflict, only 422
        var innerEx = new Rest.WeaviateUnexpectedStatusCodeException(
            HttpStatusCode.InternalServerError,
            new HashSet<HttpStatusCode> { HttpStatusCode.OK },
            "backup already in progress"
        );

        // Act & Assert
        // Should throw the original exception since it's not recognized
        var ex = Assert.Throws<Rest.WeaviateUnexpectedStatusCodeException>(() =>
            ExceptionHelper.MapHttpException(
                HttpStatusCode.InternalServerError,
                "backup already in progress",
                innerEx
            )
        );
        Assert.Same(innerEx, ex);
    }
}

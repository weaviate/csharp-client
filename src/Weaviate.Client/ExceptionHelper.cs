using System.Net;

namespace Weaviate.Client;

/// <summary>
/// Helper class to centralize exception mapping logic for both REST and gRPC APIs.
/// Provides consistent exception handling across different protocols.
/// </summary>
internal static class ExceptionHelper
{
    /// <summary>
    /// Maps an HTTP status code and error message to the appropriate Weaviate exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="errorMessage">The error message from the server</param>
    /// <param name="innerException">The original exception</param>
    /// <param name="resourceType">Optional resource type for context</param>
    /// <returns>The appropriate WeaviateException based on status code and message</returns>
    public static WeaviateException MapHttpException(
        HttpStatusCode statusCode,
        string errorMessage,
        Exception innerException,
        ResourceType resourceType = ResourceType.Unknown
    )
    {
        // Check for timeout first
        if (TimeoutHelper.IsTimeoutCancellation(innerException))
        {
            var timeout = TimeoutHelper.GetTimeout();
            var operation = TimeoutHelper.GetOperation();
            return new WeaviateTimeoutException(timeout, operation, innerException);
        }

        // Check status code first
        WeaviateException? exception = statusCode switch
        {
            HttpStatusCode.BadRequest => new WeaviateBadRequestException(null, innerException),
            HttpStatusCode.Unauthorized => new WeaviateAuthenticationException(
                null,
                innerException
            ),
            HttpStatusCode.Forbidden => new WeaviateAuthorizationException(null, innerException),
            HttpStatusCode.NotFound => new WeaviateNotFoundException(
                (Rest.WeaviateUnexpectedStatusCodeException)innerException,
                resourceType
            ),
            HttpStatusCode.Conflict => new WeaviateConflictException(
                $"Conflict accessing {resourceType}",
                innerException
            ),
            _ => null,
        };

        if (exception != null)
        {
            return exception;
        }

        // For 422 and 500, try to map based on error message
        if (
            statusCode == HttpStatusCode.UnprocessableEntity
            || statusCode == HttpStatusCode.InternalServerError
        )
        {
            var messageBasedException = MapErrorMessage(errorMessage, innerException);
            if (messageBasedException != null)
            {
                return messageBasedException;
            }
        }

        if (statusCode == HttpStatusCode.UnprocessableEntity)
        {
            return new WeaviateUnprocessableEntityException(null, innerException);
        }

        // Re-throw the original exception if we can't map it
        throw innerException;
    }

    /// <summary>
    /// Maps a gRPC RpcException to the appropriate Weaviate exception.
    /// </summary>
    /// <param name="rpcException">The gRPC RpcException</param>
    /// <param name="defaultMessage">The default error message if no specific exception applies</param>
    /// <returns>The appropriate WeaviateException based on gRPC status and message</returns>
    public static WeaviateException MapGrpcException(
        global::Grpc.Core.RpcException rpcException,
        string defaultMessage
    )
    {
        // Check for timeout first (gRPC wraps cancellation in RpcException)
        if (
            rpcException.InnerException != null
            && TimeoutHelper.IsTimeoutCancellation(rpcException.InnerException)
        )
        {
            var timeout = TimeoutHelper.GetTimeout();
            var operation = TimeoutHelper.GetOperation();
            return new WeaviateTimeoutException(timeout, operation, rpcException);
        }

        // gRPC also uses StatusCode.Cancelled for timeouts
        if (
            rpcException.StatusCode == global::Grpc.Core.StatusCode.Cancelled
            && TimeoutHelper.IsTimeoutCancellation(rpcException)
        )
        {
            var timeout = TimeoutHelper.GetTimeout();
            var operation = TimeoutHelper.GetOperation();
            return new WeaviateTimeoutException(timeout, operation, rpcException);
        }

        // Check status code first
        switch (rpcException.StatusCode)
        {
            case global::Grpc.Core.StatusCode.Unauthenticated:
                return new WeaviateAuthenticationException(null, rpcException);

            case global::Grpc.Core.StatusCode.PermissionDenied:
                return new WeaviateAuthorizationException(null, rpcException);

            case global::Grpc.Core.StatusCode.Unimplemented:
                // This is typically used for feature not supported scenarios
                return new WeaviateFeatureNotSupportedException(null, rpcException);
        }

        // Parse error message for specific error types
        var errorMessage = rpcException.Status.Detail ?? rpcException.Message;
        var messageBasedException = MapErrorMessage(errorMessage, rpcException);

        if (messageBasedException != null)
        {
            return messageBasedException;
        }

        // Default to generic server exception
        return new WeaviateServerException(defaultMessage, rpcException);
    }

    /// <summary>
    /// Maps error messages to specific exceptions regardless of protocol.
    /// Returns null if no specific exception type matches.
    /// </summary>
    private static WeaviateException? MapErrorMessage(string errorMessage, Exception innerException)
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            return null;
        }

        // Check for collection limit error
        if (
            errorMessage.Contains(
                "maximum number of collections",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return new WeaviateCollectionLimitReachedException(null, innerException);
        }

        // Check for module not available error
        if (errorMessage.Contains("no module with name", StringComparison.OrdinalIgnoreCase))
        {
            return new WeaviateModuleNotAvailableException(null, innerException);
        }

        // Check for vectorization/module execution errors
        if (errorMessage.Contains("could not vectorize", StringComparison.OrdinalIgnoreCase))
        {
            return new WeaviateExternalModuleProblemException(null, innerException);
        }

        return null;
    }
}

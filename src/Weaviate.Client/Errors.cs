using Weaviate.Client.Grpc;
using Weaviate.Client.Rest;

namespace Weaviate.Client;

public class WeaviateException : Exception
{
    public WeaviateException(string message)
        : base(message) { }

    protected WeaviateException(Exception innerException)
        : base(innerException.Message, innerException) { }

    protected WeaviateException(string? message, Exception? innerException = null)
        : base(message, innerException) { }
}

public class WeaviateClientException : WeaviateException
{
    public WeaviateClientException(string message)
        : base(message) { }

    public WeaviateClientException(Exception innerException)
        : base(innerException) { }

    public WeaviateClientException(string? message = null, Exception? innerException = null)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an operation times out.
/// This indicates the operation exceeded the configured timeout duration.
/// </summary>
public class WeaviateTimeoutException : WeaviateClientException
{
    public TimeSpan? Timeout { get; }
    public string? Operation { get; }

    public const string DefaultMessage = "The operation timed out.";

    public WeaviateTimeoutException(
        TimeSpan? timeout = null,
        string? operation = null,
        Exception? innerException = null
    )
        : base(BuildMessage(timeout, operation), innerException)
    {
        Timeout = timeout;
        Operation = operation;
    }

    private static string BuildMessage(TimeSpan? timeout, string? operation)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(operation))
        {
            parts.Add(operation);
        }
        else
        {
            parts.Add("The operation");
        }

        parts.Add("timed out");

        if (timeout.HasValue)
        {
            parts.Add(
                $"after {timeout.Value.TotalSeconds.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)} seconds"
            );
        }

        return string.Join(" ", parts) + ".";
    }
}

public class WeaviateServerException : WeaviateException
{
    public WeaviateServerException(string message)
        : base(message) { }

    public WeaviateServerException(Exception innerException)
        : base(innerException) { }

    public WeaviateServerException(string? message = null, Exception? innerException = null)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a requested server feature or RPC method is not supported by the connected
/// Weaviate instance (e.g. gRPC Aggregate method missing on older versions / builds).
/// </summary>
public class WeaviateFeatureNotSupportedException : WeaviateServerException
{
    public const string DefaultMessage =
        "The requested feature is not supported by the connected Weaviate server version.";

    public WeaviateFeatureNotSupportedException(
        string? message = null,
        Exception? innerException = null
    )
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when a backup or restore operation cannot be started because another one is already in progress.
/// The Weaviate server only allows one backup or restore operation at a time.
/// </summary>
public class WeaviateBackupConflictException : WeaviateServerException
{
    public const string DefaultMessage =
        "A backup or restore operation is already in progress. Only one backup or restore operation can be performed at a time.";

    public WeaviateBackupConflictException(Exception innerException)
        : base(DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when attempting to create a resource that already exists (HTTP 409 Conflict).
/// </summary>
public class WeaviateConflictException : WeaviateServerException
{
    public WeaviateConflictException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

public class WeaviateNotFoundException : WeaviateServerException
{
    private const string DefaultMessage = "The requested resource was not found in Weaviate.";

    public ResourceType? ResourceType { get; }

    internal WeaviateNotFoundException(
        ResourceType resourceType = Client.ResourceType.Unknown,
        IDictionary<string, object>? data = null
    )
        : base(DefaultMessage)
    {
        ResourceType = resourceType;
        if (data != null)
        {
            foreach (var kvp in data)
            {
                this.Data.Add(kvp.Key, kvp.Value);
            }
        }
    }

    internal WeaviateNotFoundException(
        WeaviateRestServerException restException,
        ResourceType resourceType = Client.ResourceType.Unknown
    )
        : base(DefaultMessage, restException)
    {
        ResourceType = resourceType;
    }

    internal WeaviateNotFoundException(
        WeaviateGrpcServerException grpcException,
        ResourceType resourceType = Client.ResourceType.Unknown
    )
        : base(DefaultMessage, grpcException)
    {
        ResourceType = resourceType;
    }

    internal WeaviateNotFoundException(
        Rest.WeaviateUnexpectedStatusCodeException restException,
        ResourceType resourceType = Client.ResourceType.Unknown
    )
        : base(DefaultMessage, restException)
    {
        ResourceType = resourceType;
    }

    public override string ToString()
    {
        return $"{base.ToString()} ResourceType: {ResourceType}.";
    }
}

/// <summary>
/// Exception thrown when authentication fails (HTTP 401 Unauthorized or gRPC UNAUTHENTICATED).
/// This indicates that the request lacks valid authentication credentials.
/// </summary>
public class WeaviateAuthenticationException : WeaviateServerException
{
    public const string DefaultMessage =
        "Authentication failed. Please check your API key or authentication credentials.";

    public WeaviateAuthenticationException(string? message = null, Exception? innerException = null)
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when authorization fails (HTTP 403 Forbidden or gRPC PERMISSION_DENIED).
/// This indicates that the authenticated user does not have permission to perform the requested operation.
/// </summary>
public class WeaviateAuthorizationException : WeaviateServerException
{
    public const string DefaultMessage =
        "Authorization failed. You do not have permission to perform this operation.";

    public WeaviateAuthorizationException(string? message = null, Exception? innerException = null)
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when the server cannot process the request due to a client error (HTTP 400 Bad Request).
/// This can be caused by a malformed request, invalid parameters, or a schema validation error.
/// </summary>
public class WeaviateBadRequestException : WeaviateServerException
{
    public const string DefaultMessage =
        "The request is invalid. Please check the request parameters and schema.";

    public WeaviateBadRequestException(string? message = null, Exception? innerException = null)
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when the server understands the content type and syntax of the request,
/// but it is unable to process the contained instructions (HTTP 422 Unprocessable Entity).
/// </summary>
public class WeaviateUnprocessableEntityException : WeaviateServerException
{
    public const string DefaultMessage =
        "The server is unable to process the request. Please check the request content.";

    public WeaviateUnprocessableEntityException(
        string? message = null,
        Exception? innerException = null
    )
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when a collection limit has been reached (HTTP 422 Unprocessable Entity).
/// This typically occurs when trying to create more collections than allowed by the server configuration.
/// </summary>
public class WeaviateCollectionLimitReachedException : WeaviateUnprocessableEntityException
{
    public new const string DefaultMessage =
        "Collection limit reached. Cannot create more collections than allowed by the server configuration.";

    public WeaviateCollectionLimitReachedException(
        string? message = null,
        Exception? innerException = null
    )
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when a required module is not available or enabled (HTTP 422 Unprocessable Entity).
/// This occurs when attempting to use a feature that requires a module that is not configured on the server.
/// </summary>
public class WeaviateModuleNotAvailableException : WeaviateUnprocessableEntityException
{
    public new const string DefaultMessage =
        "Required module is not available or enabled on the Weaviate server. Please check the server's module configuration.";

    public WeaviateModuleNotAvailableException(
        string? message = null,
        Exception? innerException = null
    )
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when an external module encounters a problem (HTTP 500 Internal Server Error).
/// This typically indicates an issue with a vectorizer, generative module, or other external service integration.
/// </summary>
public class WeaviateExternalModuleProblemException : WeaviateServerException
{
    public const string DefaultMessage =
        "An external module encountered a problem. This may be related to vectorizers, generative modules, or other external service integrations.";

    public WeaviateExternalModuleProblemException(
        string? message = null,
        Exception? innerException = null
    )
        : base(message ?? DefaultMessage, innerException) { }
}

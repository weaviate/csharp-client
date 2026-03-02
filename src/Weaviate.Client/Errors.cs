using Weaviate.Client.Grpc;
using Weaviate.Client.Rest;

namespace Weaviate.Client;

/// <summary>
/// The weaviate exception class
/// </summary>
/// <seealso cref="Exception"/>
public class WeaviateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    public WeaviateException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateException"/> class
    /// </summary>
    /// <param name="innerException">The inner exception</param>
    protected WeaviateException(Exception innerException)
        : base(innerException.Message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    protected WeaviateException(string? message, Exception? innerException = null)
        : base(message, innerException) { }
}

/// <summary>
/// The weaviate client exception class
/// </summary>
/// <seealso cref="WeaviateException"/>
public class WeaviateClientException : WeaviateException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateClientException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    public WeaviateClientException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateClientException"/> class
    /// </summary>
    /// <param name="innerException">The inner exception</param>
    public WeaviateClientException(Exception innerException)
        : base(innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateClientException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateClientException(string? message = null, Exception? innerException = null)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an operation times out.
/// This indicates the operation exceeded the configured timeout duration.
/// </summary>
public class WeaviateTimeoutException : WeaviateClientException
{
    /// <summary>
    /// Gets the value of the timeout
    /// </summary>
    public TimeSpan? Timeout { get; }

    /// <summary>
    /// Gets the value of the operation
    /// </summary>
    public string? Operation { get; }

    /// <summary>
    /// The default message
    /// </summary>
    public const string DefaultMessage = "The operation timed out.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateTimeoutException"/> class
    /// </summary>
    /// <param name="timeout">The timeout</param>
    /// <param name="operation">The operation</param>
    /// <param name="innerException">The inner exception</param>
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

    /// <summary>
    /// Builds the message using the specified timeout
    /// </summary>
    /// <param name="timeout">The timeout</param>
    /// <param name="operation">The operation</param>
    /// <returns>The string</returns>
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

/// <summary>
/// Marks a method as requiring a minimum Weaviate server version.
/// Used by <see cref="WeaviateClient.EnsureVersion"/> at runtime and by integration test helpers
/// to automatically determine which server version a test requires.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RequiresWeaviateVersionAttribute : Attribute
{
    /// <summary>
    /// Gets the minimum required server version.
    /// </summary>
    public Version MinimumVersion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresWeaviateVersionAttribute"/> class.
    /// </summary>
    /// <param name="major">Major version component.</param>
    /// <param name="minor">Minor version component.</param>
    /// <param name="patch">Patch version component.</param>
    public RequiresWeaviateVersionAttribute(int major, int minor, int patch = 0)
    {
        MinimumVersion = new Version(major, minor, patch);
    }
}

/// <summary>
/// Exception thrown when an operation requires a minimum Weaviate server version that is not met.
/// This indicates the connected Weaviate server is too old to support the requested feature.
/// </summary>
public class WeaviateVersionMismatchException : WeaviateClientException
{
    /// <summary>
    /// Gets the minimum server version required by the operation.
    /// </summary>
    public Version RequiredVersion { get; }

    /// <summary>
    /// Gets the actual server version that was connected.
    /// </summary>
    public Version ActualVersion { get; }

    /// <summary>
    /// Gets the name of the operation that requires the higher version.
    /// </summary>
    public string? Operation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateVersionMismatchException"/> class.
    /// </summary>
    /// <param name="operation">The name of the operation that requires a higher version.</param>
    /// <param name="requiredVersion">The minimum server version required.</param>
    /// <param name="actualVersion">The actual server version connected.</param>
    public WeaviateVersionMismatchException(
        string? operation,
        Version requiredVersion,
        Version actualVersion
    )
        : base(BuildMessage(operation, requiredVersion, actualVersion))
    {
        Operation = operation;
        RequiredVersion = requiredVersion;
        ActualVersion = actualVersion;
    }

    private static string BuildMessage(
        string? operation,
        Version requiredVersion,
        Version actualVersion
    )
    {
        var prefix = string.IsNullOrEmpty(operation) ? "This operation" : operation;
        return $"{prefix} requires Weaviate server version {requiredVersion} or later, but connected server is version {actualVersion}.";
    }
}

/// <summary>
/// The weaviate server exception class
/// </summary>
/// <seealso cref="WeaviateException"/>
public class WeaviateServerException : WeaviateException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateServerException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    public WeaviateServerException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateServerException"/> class
    /// </summary>
    /// <param name="innerException">The inner exception</param>
    public WeaviateServerException(Exception innerException)
        : base(innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateServerException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateServerException(string? message = null, Exception? innerException = null)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a requested server feature or RPC method is not supported by the connected
/// Weaviate instance (e.g. gRPC Aggregate method missing on older versions / builds).
/// </summary>
public class WeaviateFeatureNotSupportedException : WeaviateServerException
{
    /// <summary>
    /// The default message
    /// </summary>
    public const string DefaultMessage =
        "The requested feature is not supported by the connected Weaviate server version.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateFeatureNotSupportedException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
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
    /// <summary>
    /// The default message
    /// </summary>
    public const string DefaultMessage =
        "A backup or restore operation is already in progress. Only one backup or restore operation can be performed at a time.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateBackupConflictException"/> class
    /// </summary>
    /// <param name="innerException">The inner exception</param>
    public WeaviateBackupConflictException(Exception innerException)
        : base(DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when attempting to create a resource that already exists (HTTP 409 Conflict).
/// </summary>
public class WeaviateConflictException : WeaviateServerException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateConflictException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateConflictException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

/// <summary>
/// The weaviate not found exception class
/// </summary>
/// <seealso cref="WeaviateServerException"/>
public class WeaviateNotFoundException : WeaviateServerException
{
    /// <summary>
    /// The default message
    /// </summary>
    private const string DefaultMessage = "The requested resource was not found in Weaviate.";

    /// <summary>
    /// Gets the value of the resource type
    /// </summary>
    public ResourceType? ResourceType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateNotFoundException"/> class
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <param name="data">The data</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateNotFoundException"/> class
    /// </summary>
    /// <param name="restException">The rest exception</param>
    /// <param name="resourceType">The resource type</param>
    internal WeaviateNotFoundException(
        WeaviateRestServerException restException,
        ResourceType resourceType = Client.ResourceType.Unknown
    )
        : base(DefaultMessage, restException)
    {
        ResourceType = resourceType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateNotFoundException"/> class
    /// </summary>
    /// <param name="grpcException">The grpc exception</param>
    /// <param name="resourceType">The resource type</param>
    internal WeaviateNotFoundException(
        WeaviateGrpcServerException grpcException,
        ResourceType resourceType = Client.ResourceType.Unknown
    )
        : base(DefaultMessage, grpcException)
    {
        ResourceType = resourceType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateNotFoundException"/> class
    /// </summary>
    /// <param name="restException">The rest exception</param>
    /// <param name="resourceType">The resource type</param>
    internal WeaviateNotFoundException(
        WeaviateUnexpectedStatusCodeException restException,
        ResourceType resourceType = Client.ResourceType.Unknown
    )
        : base(DefaultMessage, restException)
    {
        ResourceType = resourceType;
    }

    /// <summary>
    /// Returns the string
    /// </summary>
    /// <returns>The string</returns>
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
    /// <summary>
    /// The default message
    /// </summary>
    public const string DefaultMessage =
        "Authentication failed. Please check your API key or authentication credentials.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateAuthenticationException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateAuthenticationException(string? message = null, Exception? innerException = null)
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when authorization fails (HTTP 403 Forbidden or gRPC PERMISSION_DENIED).
/// This indicates that the authenticated user does not have permission to perform the requested operation.
/// </summary>
public class WeaviateAuthorizationException : WeaviateServerException
{
    /// <summary>
    /// The default message
    /// </summary>
    public const string DefaultMessage =
        "Authorization failed. You do not have permission to perform this operation.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateAuthorizationException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateAuthorizationException(string? message = null, Exception? innerException = null)
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when the server cannot process the request due to a client error (HTTP 400 Bad Request).
/// This can be caused by a malformed request, invalid parameters, or a schema validation error.
/// </summary>
public class WeaviateBadRequestException : WeaviateServerException
{
    /// <summary>
    /// The default message
    /// </summary>
    public const string DefaultMessage =
        "The request is invalid. Please check the request parameters and schema.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateBadRequestException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateBadRequestException(string? message = null, Exception? innerException = null)
        : base(message ?? DefaultMessage, innerException) { }
}

/// <summary>
/// Exception thrown when the server understands the content type and syntax of the request,
/// but it is unable to process the contained instructions (HTTP 422 Unprocessable Entity).
/// </summary>
public class WeaviateUnprocessableEntityException : WeaviateServerException
{
    /// <summary>
    /// The default message
    /// </summary>
    public const string DefaultMessage =
        "The server is unable to process the request. Please check the request content.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateUnprocessableEntityException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
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
    /// <summary>
    /// The default message
    /// </summary>
    public new const string DefaultMessage =
        "Collection limit reached. Cannot create more collections than allowed by the server configuration.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateCollectionLimitReachedException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
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
    /// <summary>
    /// The default message
    /// </summary>
    public new const string DefaultMessage =
        "Required module is not available or enabled on the Weaviate server. Please check the server's module configuration.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateModuleNotAvailableException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
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
    /// <summary>
    /// The default message
    /// </summary>
    public const string DefaultMessage =
        "An external module encountered a problem. This may be related to vectorizers, generative modules, or other external service integrations.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateExternalModuleProblemException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateExternalModuleProblemException(
        string? message = null,
        Exception? innerException = null
    )
        : base(message ?? DefaultMessage, innerException) { }
}

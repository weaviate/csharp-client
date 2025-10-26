using Weaviate.Client.gRPC;
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

public class WeaviateNotFoundException : WeaviateServerException
{
    private const string DefaultMessage = "The requested resource was not found in Weaviate.";

    public ResourceType? ResourceType { get; }

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

    public override string ToString()
    {
        return $"{base.ToString()} ResourceType: {ResourceType}.";
    }
}

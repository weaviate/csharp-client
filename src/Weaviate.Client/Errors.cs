namespace Weaviate.Client;

public class WeaviateException : Exception
{
    protected WeaviateException(Exception innerException)
        : base(innerException.Message, innerException) { }

    protected WeaviateException(string? message, Exception? innerException = null)
        : base(message, innerException) { }
}

public class WeaviateClientException : WeaviateException
{
    public WeaviateClientException(string? message)
        : base(message) { }

    public WeaviateClientException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

public class WeaviateServerException : WeaviateException
{
    public WeaviateServerException(string? message)
        : base(message) { }

    public WeaviateServerException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a backup or restore operation cannot be started because another one is already in progress.
/// The Weaviate server only allows one backup or restore operation at a time.
/// </summary>
public class WeaviateBackupConflictException : WeaviateServerException
{
    public WeaviateBackupConflictException(string? message)
        : base(message) { }

    public WeaviateBackupConflictException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

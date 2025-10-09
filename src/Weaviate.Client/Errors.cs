namespace Weaviate.Client;

[Serializable]
public class WeaviateException : Exception
{
    protected WeaviateException(Exception innerException)
        : base(innerException.Message, innerException) { }

    protected WeaviateException(string? message, Exception? innerException = null)
        : base(message, innerException) { }
}

[Serializable]
public class WeaviateClientException : WeaviateException
{
    public WeaviateClientException(string? message)
        : base(message) { }

    public WeaviateClientException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

[Serializable]
public class WeaviateServerException : WeaviateException
{
    public WeaviateServerException(string? message)
        : base(message) { }

    public WeaviateServerException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

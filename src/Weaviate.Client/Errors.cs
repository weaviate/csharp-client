namespace Weaviate.Client;

public class WeaviateException : Exception
{
    public WeaviateException(Exception innerException)
        : base(innerException.Message, innerException) { }

    public WeaviateException(string? message, Exception? innerException = null)
        : base(message, innerException) { }
}

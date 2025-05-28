namespace Weaviate.Client;

public class WeaviateException : Exception
{
    public WeaviateException() { }

    public WeaviateException(string? message)
        : base(message) { }
}

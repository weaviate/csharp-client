namespace Weaviate.Client.Rest;

internal class WeaviateRestException : WeaviateException
{
    public WeaviateRestException(Exception? innerException = null)
        : base("An error occurred in the Weaviate REST API", innerException) { }
}

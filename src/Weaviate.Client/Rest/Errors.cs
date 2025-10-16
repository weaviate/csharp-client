namespace Weaviate.Client.Rest;

public class WeaviateRestException : WeaviateClientException
{
    public WeaviateRestException(Exception? innerException = null)
        : base("An error occurred in the Weaviate REST API", innerException) { }
}

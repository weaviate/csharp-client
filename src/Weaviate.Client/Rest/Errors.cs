using System.Net;

namespace Weaviate.Client.Rest;

public class WeaviateRestException : WeaviateClientException
{
    public WeaviateRestException(
        string? message = null,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null
    )
        : base("An error occurred in the Weaviate REST API", innerException) { }
}

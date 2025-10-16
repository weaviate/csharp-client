using System.Net;

namespace Weaviate.Client.Rest;

public class WeaviateRestClientException : WeaviateClientException
{
    public WeaviateRestClientException(string? message = null, Exception? innerException = null)
        : base(
            $"An error occurred processing the response from the Weaviate REST API: {message}",
            innerException
        ) { }
}

public class WeaviateRestServerException : WeaviateServerException
{
    public WeaviateRestServerException(
        string? message = null,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null
    )
        : base(
            $"An error occurred in the Weaviate REST API: [{statusCode}] {message}",
            innerException
        )
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }
}

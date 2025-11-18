using System.Net;

namespace Weaviate.Client.Rest;

public class WeaviateRestClientException : WeaviateClientException
{
    private const string DefaultMessage =
        "An error occurred processing the response from the Weaviate REST API.";

    public WeaviateRestClientException(Exception innerException)
        : base(DefaultMessage, innerException) { }

    public WeaviateRestClientException(string message = DefaultMessage)
        : base(
            message != DefaultMessage ? string.Join(" ", DefaultMessage, message) : DefaultMessage
        ) { }

    public WeaviateRestClientException(string message, Exception innerException)
        : base(
            message != DefaultMessage ? string.Join(" ", DefaultMessage, message) : DefaultMessage,
            innerException
        ) { }
}

public class WeaviateRestServerException : WeaviateServerException
{
    private const string DefaultMessage =
        "An error occurred in the server while processing the request.";

    public WeaviateRestServerException(
        HttpStatusCode? statusCode = null,
        Exception? innerException = null
    )
        : base(DefaultMessage, innerException)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }

    public override string ToString()
    {
        var status = StatusCode.HasValue ? $" StatusCode: {StatusCode.Value}." : string.Empty;

        return $"Server-Side Error: {base.ToString()}{status}";
    }
}

/// <summary>
/// Exception thrown for invalid enum wire-format values.
/// </summary>
public class InvalidEnumWireFormatException : WeaviateClientException
{
    public InvalidEnumWireFormatException(string message)
        : base(message) { }
}

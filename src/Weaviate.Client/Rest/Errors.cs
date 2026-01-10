using System.Net;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client exception class
/// </summary>
/// <seealso cref="WeaviateClientException"/>
internal class WeaviateRestClientException : WeaviateClientException
{
    /// <summary>
    /// The default message
    /// </summary>
    private const string DefaultMessage =
        "An error occurred processing the response from the Weaviate REST API.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateRestClientException"/> class
    /// </summary>
    /// <param name="innerException">The inner exception</param>
    public WeaviateRestClientException(Exception innerException)
        : base(DefaultMessage, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateRestClientException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    public WeaviateRestClientException(string message = DefaultMessage)
        : base(
            message != DefaultMessage ? string.Join(" ", DefaultMessage, message) : DefaultMessage
        ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateRestClientException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateRestClientException(string message, Exception innerException)
        : base(
            message != DefaultMessage ? string.Join(" ", DefaultMessage, message) : DefaultMessage,
            innerException
        ) { }
}

/// <summary>
/// The weaviate rest server exception class
/// </summary>
/// <seealso cref="WeaviateServerException"/>
internal class WeaviateRestServerException : WeaviateServerException
{
    /// <summary>
    /// The default message
    /// </summary>
    private const string DefaultMessage =
        "An error occurred in the server while processing the request.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateRestServerException"/> class
    /// </summary>
    /// <param name="statusCode">The status code</param>
    /// <param name="innerException">The inner exception</param>
    public WeaviateRestServerException(
        HttpStatusCode? statusCode = null,
        Exception? innerException = null
    )
        : base(DefaultMessage, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the value of the status code
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Returns the string
    /// </summary>
    /// <returns>The string</returns>
    public override string ToString()
    {
        var status = StatusCode.HasValue ? $" StatusCode: {StatusCode.Value}." : string.Empty;

        return $"Server-Side Error: {base.ToString()}{status}";
    }
}

/// <summary>
/// Exception thrown for invalid enum wire-format values.
/// </summary>
internal class InvalidEnumWireFormatException : WeaviateClientException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidEnumWireFormatException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    public InvalidEnumWireFormatException(string message)
        : base(message) { }
}

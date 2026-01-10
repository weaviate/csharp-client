namespace Weaviate.Client.Grpc;

/// <summary>
/// The weaviate grpc server exception class
/// </summary>
/// <seealso cref="WeaviateServerException"/>
internal class WeaviateGrpcServerException : WeaviateServerException
{
    /// <summary>
    /// The default message
    /// </summary>
    private const string DefaultMessage =
        "An error occurred in the server while processing the request.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateGrpcServerException"/> class
    /// </summary>
    /// <param name="innerException">The inner exception</param>
    public WeaviateGrpcServerException(Exception? innerException = null)
        : base(DefaultMessage, innerException) { }

    /// <summary>
    /// Returns the string
    /// </summary>
    /// <returns>The string</returns>
    public override string ToString()
    {
        return $"Server-Side Error: {base.ToString()}";
    }
}

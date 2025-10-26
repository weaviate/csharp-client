namespace Weaviate.Client.gRPC;

public class WeaviateGrpcServerException : WeaviateServerException
{
    private const string DefaultMessage =
        "An error occurred in the server while processing the request.";

    public WeaviateGrpcServerException(Exception? innerException = null)
        : base(DefaultMessage, innerException) { }

    public override string ToString()
    {
        return $"Server-Side Error: {base.ToString()}";
    }
}

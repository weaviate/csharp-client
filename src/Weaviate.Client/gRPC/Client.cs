namespace Weaviate.Client.Grpc;

public class WeaviateGrpcClient(WeaviateClient client) : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
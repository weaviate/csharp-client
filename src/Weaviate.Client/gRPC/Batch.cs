using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public partial class WeaviateGrpcClient
{
    internal async Task<BatchObjectsReply> InsertMany(IEnumerable<BatchObject> objects)
    {
        var request = new BatchObjectsRequest { Objects = { objects } };

        BatchObjectsReply reply = await _grpcClient.BatchObjectsAsync(request);

        return reply;
    }
}

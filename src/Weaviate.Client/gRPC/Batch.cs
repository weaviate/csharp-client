using Weaviate.Client.Models;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    internal async Task<Grpc.Protobuf.V1.BatchDeleteReply> DeleteMany(
        string collection,
        Filter filter,
        bool dryRun,
        bool verbose,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = new Grpc.Protobuf.V1.BatchDeleteRequest
        {
            Collection = collection,
            DryRun = dryRun,
            Verbose = verbose,
            Filters = filter.InternalFilter,
            Tenant = tenant ?? string.Empty,
        };

        if (consistencyLevel.HasValue)
        {
            request.ConsistencyLevel = MapConsistencyLevel(consistencyLevel.Value);
        }
        var reply = await _grpcClient.BatchDeleteAsync(
            request,
            CreateCallOptions(cancellationToken)
        );

        return reply;
    }

    internal async Task<Grpc.Protobuf.V1.BatchObjectsReply> InsertMany(
        IEnumerable<Grpc.Protobuf.V1.BatchObject> objects,
        CancellationToken cancellationToken = default
    )
    {
        var request = new Grpc.Protobuf.V1.BatchObjectsRequest { Objects = { objects } };

        var reply = await _grpcClient.BatchObjectsAsync(
            request,
            CreateCallOptions(cancellationToken)
        );

        return reply;
    }
}

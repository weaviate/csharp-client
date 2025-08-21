using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    internal async Task<BatchDeleteReply> DeleteMany(
        string collection,
        Filter filter,
        bool dryRun,
        bool verbose,
        string? tenant = null,
        ConsistencyLevel? consistencyLevel = null
    )
    {
        var request = new BatchDeleteRequest
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

        BatchDeleteReply reply = await _grpcClient.BatchDeleteAsync(
            request,
            headers: _defaultHeaders
        );

        return reply;
    }

    internal async Task<BatchObjectsReply> InsertMany(IEnumerable<BatchObject> objects)
    {
        var request = new BatchObjectsRequest { Objects = { objects } };

        BatchObjectsReply reply = await _grpcClient.BatchObjectsAsync(
            request,
            headers: _defaultHeaders
        );

        return reply;
    }
}

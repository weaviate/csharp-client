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
        ConsistencyLevels? consistencyLevel = null,
        CancellationToken cancellationToken = default
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

        try
        {
            BatchDeleteReply reply = await _grpcClient.BatchDeleteAsync(
                request,
                CreateCallOptions(cancellationToken)
            );

            return reply;
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            // Use centralized exception mapping helper
            throw ExceptionHelper.MapGrpcException(ex, "Batch delete request failed");
        }
    }

    internal async Task<BatchObjectsReply> InsertMany(
        IEnumerable<BatchObject> objects,
        CancellationToken cancellationToken = default
    )
    {
        var request = new BatchObjectsRequest { Objects = { objects } };

        try
        {
            BatchObjectsReply reply = await _grpcClient.BatchObjectsAsync(
                request,
                CreateCallOptions(cancellationToken)
            );

            return reply;
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            // Use centralized exception mapping helper
            throw ExceptionHelper.MapGrpcException(ex, "Batch insert request failed");
        }
    }
}

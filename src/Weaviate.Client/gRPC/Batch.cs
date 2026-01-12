using Weaviate.Client.Models;

namespace Weaviate.Client.Grpc;

/// <summary>
/// The weaviate grpc client class
/// </summary>
internal partial class WeaviateGrpcClient
{
    /// <summary>
    /// Deletes the many using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="filter">The filter</param>
    /// <param name="dryRun">The dry run</param>
    /// <param name="verbose">The verbose</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="consistencyLevel">The consistency level</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the grpc protobuf batch delete reply</returns>
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
        try
        {
            var reply = await _grpcClient.BatchDeleteAsync(
                request,
                CreateCallOptions(cancellationToken)
            );

            return reply;
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            // Use centralized exception mapping helper
            throw Internal.ExceptionHelper.MapGrpcException(ex, "Batch delete request failed");
        }
    }

    /// <summary>
    /// Inserts the many using the specified objects
    /// </summary>
    /// <param name="objects">The objects</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the grpc protobuf batch objects reply</returns>
    internal async Task<Grpc.Protobuf.V1.BatchObjectsReply> InsertMany(
        IEnumerable<Grpc.Protobuf.V1.BatchObject> objects,
        CancellationToken cancellationToken = default
    )
    {
        var request = new Grpc.Protobuf.V1.BatchObjectsRequest { Objects = { objects } };

        try
        {
            var reply = await _grpcClient.BatchObjectsAsync(
                request,
                CreateCallOptions(cancellationToken)
            );

            return reply;
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            // Use centralized exception mapping helper
            throw Internal.ExceptionHelper.MapGrpcException(ex, "Batch insert request failed");
        }
    }
}

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
    internal async Task<Protobuf.V1.BatchDeleteReply> DeleteMany(
        string collection,
        Filter filter,
        bool dryRun,
        bool verbose,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = new Protobuf.V1.BatchDeleteRequest
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
    internal async Task<Protobuf.V1.BatchObjectsReply> InsertMany(
        IEnumerable<Protobuf.V1.BatchObject> objects,
        CancellationToken cancellationToken = default
    )
    {
        var request = new Protobuf.V1.BatchObjectsRequest { Objects = { objects } };

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

    /// <summary>
    /// Starts a bidirectional streaming call for server-side batching.
    /// </summary>
    /// <param name="consistencyLevel">The consistency level</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A batch stream wrapper that abstracts away protobuf types</returns>
    internal async Task<BatchStreamWrapper> StartBatchStream(
        ConsistencyLevels? consistencyLevel = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var stream = _grpcClient.BatchStream(CreateCallOptions(cancellationToken));

            // Send the Start message with optional consistency level
            var startRequest = new Protobuf.V1.BatchStreamRequest
            {
                Start = new Protobuf.V1.BatchStreamRequest.Types.Start(),
            };

            if (consistencyLevel.HasValue)
            {
                startRequest.Start.ConsistencyLevel = MapConsistencyLevel(consistencyLevel.Value);
            }

            await stream.RequestStream.WriteAsync(startRequest, cancellationToken);

            return new BatchStreamWrapper(stream);
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            throw Internal.ExceptionHelper.MapGrpcException(ex, "Batch stream start failed");
        }
    }
}

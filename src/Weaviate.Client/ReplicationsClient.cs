using Weaviate.Client.Models;

namespace Weaviate.Client;

public class ReplicationsClient
{
    private readonly Rest.WeaviateRestClient _restClient;

    internal ReplicationsClient(Rest.WeaviateRestClient restClient)
    {
        _restClient = restClient;
    }

    /// <summary>
    /// Get details of a specific replication operation by ID
    /// </summary>
    /// <param name="id">The unique identifier of the replication operation</param>
    /// <param name="includeHistory">Whether to include status history</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The replication operation, or null if not found</returns>
    public async Task<ReplicationOperation?> Get(
        Guid id,
        bool includeHistory = false,
        CancellationToken cancellationToken = default
    )
    {
        var dto = await _restClient.ReplicationDetailsAsync(id, includeHistory, cancellationToken);
        return dto is null ? null : ToModel(dto);
    }

    /// <summary>
    /// List all replication operations with optional filters
    /// </summary>
    /// <param name="collection">Filter by collection name</param>
    /// <param name="shard">Filter by shard name</param>
    /// <param name="targetNode">Filter by target node name</param>
    /// <param name="includeHistory">Whether to include status history</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of replication operations matching the filters</returns>
    public async Task<IEnumerable<ReplicationOperation>> List(
        string? collection = null,
        string? shard = null,
        string? targetNode = null,
        bool includeHistory = false,
        CancellationToken cancellationToken = default
    )
    {
        var dtos = await _restClient.ListReplicationsAsync(
            collection,
            shard,
            targetNode,
            includeHistory,
            cancellationToken
        );
        return dtos.Select(ToModel);
    }

    /// <summary>
    /// List all replication operations (includes status history)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all replication operations</returns>
    public async Task<IEnumerable<ReplicationOperation>> ListAll(
        CancellationToken cancellationToken = default
    )
    {
        return await List(includeHistory: true, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Cancel a replication operation
    /// </summary>
    /// <param name="id">The unique identifier of the operation to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task Cancel(Guid id, CancellationToken cancellationToken = default)
    {
        await _restClient.CancelReplicationAsync(id, cancellationToken);
    }

    /// <summary>
    /// Delete a replication operation
    /// </summary>
    /// <param name="id">The unique identifier of the operation to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _restClient.DeleteReplicationAsync(id, cancellationToken);
    }

    /// <summary>
    /// Delete all replication operations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteAll(CancellationToken cancellationToken = default)
    {
        await _restClient.DeleteAllReplicationsAsync(cancellationToken);
    }

    internal static ReplicationOperation ToModel(
        Rest.Dto.ReplicationReplicateDetailsReplicaResponse dto
    )
    {
        return new ReplicationOperation(
            Id: dto.Id,
            Collection: dto.Collection ?? string.Empty,
            Shard: dto.Shard ?? string.Empty,
            SourceNode: dto.SourceNode ?? string.Empty,
            TargetNode: dto.TargetNode ?? string.Empty,
            Type: ParseReplicationType(dto.Type),
            Status: ParseStatus(dto.Status),
            WhenStartedUnixMs: dto.WhenStartedUnixMs,
            Uncancelable: dto.Uncancelable,
            ScheduledForCancel: dto.ScheduledForCancel,
            ScheduledForDelete: dto.ScheduledForDelete,
            StatusHistory: dto.StatusHistory?.Select(ParseStatus).ToList()
        );
    }

    private static ReplicationType ParseReplicationType(
        Rest.Dto.ReplicationReplicateDetailsReplicaResponseType type
    )
    {
        return type == Rest.Dto.ReplicationReplicateDetailsReplicaResponseType.MOVE
            ? ReplicationType.Move
            : ReplicationType.Copy;
    }

    private static ReplicationOperationStatus ParseStatus(
        Rest.Dto.ReplicationReplicateDetailsReplicaStatus? status
    )
    {
        if (status is null)
        {
            return new ReplicationOperationStatus(ReplicationOperationState.Registered, null, null);
        }

        return new ReplicationOperationStatus(
            State: ParseState(status.State),
            WhenStartedUnixMs: status.WhenStartedUnixMs,
            Errors: status.Errors?.Select(ParseError).ToList()
        );
    }

    private static ReplicationOperationState ParseState(
        Rest.Dto.ReplicationReplicateDetailsReplicaStatusState? state
    )
    {
        return state switch
        {
            Rest.Dto.ReplicationReplicateDetailsReplicaStatusState.REGISTERED =>
                ReplicationOperationState.Registered,
            Rest.Dto.ReplicationReplicateDetailsReplicaStatusState.HYDRATING =>
                ReplicationOperationState.Hydrating,
            Rest.Dto.ReplicationReplicateDetailsReplicaStatusState.FINALIZING =>
                ReplicationOperationState.Finalizing,
            Rest.Dto.ReplicationReplicateDetailsReplicaStatusState.DEHYDRATING =>
                ReplicationOperationState.Dehydrating,
            Rest.Dto.ReplicationReplicateDetailsReplicaStatusState.READY =>
                ReplicationOperationState.Ready,
            Rest.Dto.ReplicationReplicateDetailsReplicaStatusState.CANCELLED =>
                ReplicationOperationState.Cancelled,
            _ => ReplicationOperationState.Registered,
        };
    }

    private static ReplicationOperationError ParseError(
        Rest.Dto.ReplicationReplicateDetailsReplicaStatusError error
    )
    {
        return new ReplicationOperationError(
            WhenErroredUnixMs: error.WhenErroredUnixMs ?? 0,
            Message: error.Message ?? string.Empty
        );
    }
}

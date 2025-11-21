using Weaviate.Client.Models;

namespace Weaviate.Client;

public class ClusterClient
{
    private readonly Rest.WeaviateRestClient _client;
    private NodesClient? _nodes;
    private ReplicationsClient? _replications;

    internal ClusterClient(Rest.WeaviateRestClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Access to cluster nodes management
    /// </summary>
    public NodesClient Nodes => _nodes ??= new NodesClient(_client);

    /// <summary>
    /// Access to replication operations management
    /// </summary>
    public ReplicationsClient Replications => _replications ??= new ReplicationsClient(_client);

    /// <summary>
    /// Start a replication operation asynchronously.
    /// Returns a tracker that can be used to monitor status or wait for completion.
    /// </summary>
    /// <param name="request">The replication request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tracker for the replication operation</returns>
    public async Task<ReplicationOperationTracker> Replicate(
        ReplicateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var dto = new Rest.Dto.ReplicationReplicateReplicaRequest
        {
            Collection = request.Collection,
            Shard = request.Shard,
            SourceNode = request.SourceNode,
            TargetNode = request.TargetNode,
            Type =
                request.Type == ReplicationType.Move
                    ? Rest.Dto.ReplicationReplicateReplicaRequestType.MOVE
                    : Rest.Dto.ReplicationReplicateReplicaRequestType.COPY,
        };

        var response = await _client.ReplicateAsync(dto);
        var operationId = response.Id;

        // Fetch initial status
        var initialDetails = await _client.ReplicationDetailsAsync(
            operationId,
            cancellationToken: cancellationToken
        );
        if (initialDetails is null)
        {
            throw new WeaviateClientException(
                "Replication operation was created but could not be retrieved"
            );
        }

        var initialOperation = ReplicationsClient.ToModel(initialDetails);

        return new ReplicationOperationTracker(
            initialOperation,
            async (ct) =>
            {
                var details = await _client.ReplicationDetailsAsync(
                    operationId,
                    cancellationToken: ct == default ? cancellationToken : ct
                );
                return details is null
                    ? throw new WeaviateNotFoundException(
                        ResourceType.Replication,
                        new Dictionary<string, object> { ["operationId"] = operationId }
                    )
                    : ReplicationsClient.ToModel(details);
            },
            async (ct) =>
                await _client.CancelReplicationAsync(
                    operationId,
                    ct == default ? cancellationToken : ct
                )
        );
    }

    /// <summary>
    /// Start a replication operation and wait for completion synchronously.
    /// This method blocks until the replication finishes.
    /// </summary>
    /// <param name="request">The replication request parameters</param>
    /// <param name="timeout">Optional timeout; if null uses default from ReplicationClientConfig</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The completed replication operation</returns>
    public async Task<ReplicationOperation> ReplicateSync(
        ReplicateRequest request,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var operation = await Replicate(request, cancellationToken);
        return await operation.WaitForCompletion(timeout, cancellationToken);
    }
}

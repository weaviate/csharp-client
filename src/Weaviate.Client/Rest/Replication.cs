using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    /// <summary>
    /// Initiate a replica movement operation
    /// </summary>
    internal async Task<Dto.ReplicationReplicateReplicaResponse> ReplicateAsync(
        Dto.ReplicationReplicateReplicaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Replicate(),
            request,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        await response.EnsureExpectedStatusCodeAsync([200], "replicate");
        return await response.Content.ReadFromJsonAsync<Dto.ReplicationReplicateReplicaResponse>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken
            ) ?? throw new WeaviateRestClientException();
    }

    /// <summary>
    /// Get details of a specific replication operation
    /// </summary>
    internal async Task<Dto.ReplicationReplicateDetailsReplicaResponse?> ReplicationDetailsAsync(
        Guid id,
        bool includeHistory = false,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.ReplicationDetails(id, includeHistory),
            cancellationToken
        );
        await response.EnsureExpectedStatusCodeAsync([200, 404], "replication details");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        return await response.Content.ReadFromJsonAsync<Dto.ReplicationReplicateDetailsReplicaResponse>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken
            ) ?? throw new WeaviateRestClientException();
    }

    /// <summary>
    /// List replication operations with optional filters
    /// </summary>
    internal async Task<
        IEnumerable<Dto.ReplicationReplicateDetailsReplicaResponse>
    > ListReplicationsAsync(
        string? collection = null,
        string? shard = null,
        string? targetNode = null,
        bool includeHistory = false,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.ReplicationList(collection, shard, targetNode, includeHistory),
            cancellationToken
        );
        await response.EnsureExpectedStatusCodeAsync([200], "list replications");
        return await response.Content.ReadFromJsonAsync<
                IEnumerable<Dto.ReplicationReplicateDetailsReplicaResponse>
            >(WeaviateRestClient.RestJsonSerializerOptions, cancellationToken)
            ?? throw new WeaviateRestClientException();
    }

    /// <summary>
    /// Cancel a replication operation
    /// </summary>
    internal async Task CancelReplicationAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsync(
            WeaviateEndpoints.ReplicationCancel(id),
            content: null,
            cancellationToken
        );
        await response.EnsureExpectedStatusCodeAsync([204], "cancel replication");
    }

    /// <summary>
    /// Delete a replication operation
    /// </summary>
    internal async Task DeleteReplicationAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.ReplicationDelete(id),
            cancellationToken
        );
        await response.EnsureExpectedStatusCodeAsync([204], "delete replication");
    }

    /// <summary>
    /// Delete all replication operations
    /// </summary>
    internal async Task DeleteAllReplicationsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.Replicate(),
            cancellationToken
        );
        await response.EnsureExpectedStatusCodeAsync([204], "delete all replications");
    }
}

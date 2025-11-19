using Weaviate.Client.Models;
using Weaviate.Client.Rest;

namespace Weaviate.Client;

/// <summary>
/// Client for querying Weaviate cluster nodes information.
/// </summary>
public class NodesClient
{
    private readonly WeaviateRestClient _client;

    internal NodesClient(WeaviateRestClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Lists all nodes in the cluster with minimal information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An array of cluster node information.</returns>
    public async Task<ClusterNode[]> List(CancellationToken cancellationToken = default)
    {
        var nodes = await _client.Nodes(null, "minimal", cancellationToken);
        if (nodes == null)
            return Array.Empty<ClusterNode>();

        return nodes.Where(n => n != null).Select(n => n.ToModel()).ToArray();
    }

    /// <summary>
    /// Lists all nodes in the cluster with verbose information including statistics and shard details.
    /// </summary>
    /// <param name="collection">Optional collection name to filter shard information.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An array of verbose cluster node information.</returns>
    public async Task<ClusterNodeVerbose[]> ListVerbose(
        string? collection = null,
        CancellationToken cancellationToken = default
    )
    {
        var nodes = await _client.Nodes(collection, "verbose", cancellationToken);
        if (nodes == null)
            return Array.Empty<ClusterNodeVerbose>();

        return nodes.Where(n => n != null).Select(n => n.ToVerboseModel()).ToArray();
    }
}

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
    /// Lists all nodes in the cluster.
    /// </summary>
    /// <param name="detailLevel">The level of detail to retrieve (Minimal or Verbose).</param>
    /// <param name="collection">Optional collection name to filter shard information. Only applicable when detailLevel is Verbose.</param>
    /// <returns>An array of cluster node information with the specified level of detail.</returns>
    public async Task<ClusterNode[]> List(
        NodeDetailLevel detailLevel = NodeDetailLevel.Minimal,
        string? collection = null
    )
    {
        var verbosityString = detailLevel switch
        {
            NodeDetailLevel.Minimal => "minimal",
            NodeDetailLevel.Verbose => "verbose",
            _ => "minimal",
        };

        var nodes = await _client.Nodes(collection, verbosityString);
        if (nodes == null)
            return Array.Empty<ClusterNode>();

        return nodes.Where(n => n != null).Select(n => n.ToModel(detailLevel)).ToArray();
    }
}

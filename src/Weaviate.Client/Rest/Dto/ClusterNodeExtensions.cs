using Weaviate.Client.Models;

namespace Weaviate.Client.Rest.Dto;

/// <summary>
/// Extension methods for NodeStatus DTO to convert to model objects.
/// </summary>
public partial class NodeStatus
{
    /// <summary>
    /// Converts this NodeStatus DTO to a ClusterNode model.
    /// </summary>
    /// <param name="detailLevel">The level of detail to include in the conversion.</param>
    /// <returns>A ClusterNode model object.</returns>
    public ClusterNode ToModel(NodeDetailLevel detailLevel)
    {
        var node = new ClusterNode
        {
            DetailLevel = detailLevel,
            GitHash = GitHash ?? string.Empty,
            Name = Name ?? string.Empty,
            Status = Status?.ToString() ?? "Unknown",
            Version = Version ?? string.Empty,
        };

        if (detailLevel == NodeDetailLevel.Verbose)
        {
            return node with
            {
                Stats = Stats?.ToModel(),
                Shards = Shards
                    ?.Where(s =>
                        s != null
                        && !string.IsNullOrEmpty(s.Class)
                        && !string.IsNullOrEmpty(s.Name)
                    )
                    .Select(s => s.ToModel())
                    .ToArray(),
            };
        }

        return node;
    }
}

/// <summary>
/// Extension methods for NodeStats DTO to convert to model objects.
/// </summary>
public partial class NodeStats
{
    /// <summary>
    /// Converts this NodeStats DTO to a NodeStats model.
    /// </summary>
    /// <returns>A NodeStats model object.</returns>
    public ClusterNode.NodeStats ToModel()
    {
        return new ClusterNode.NodeStats
        {
            ObjectCount = (int)(ObjectCount ?? 0),
            ShardCount = (int)(ShardCount ?? 0),
        };
    }
}

/// <summary>
/// Extension methods for NodeShardStatus DTO to convert to model objects.
/// </summary>
public partial class NodeShardStatus
{
    /// <summary>
    /// Converts this NodeShardStatus DTO to a Shard model.
    /// </summary>
    /// <returns>A Shard model object.</returns>
    public ClusterNode.Shard ToModel()
    {
        return new ClusterNode.Shard
        {
            Collection = Class!,
            Name = Name!,
            Node = Name!, // Using shard name as node name
            ObjectCount = (int)(ObjectCount ?? 0),
            VectorIndexingStatus = ParseVectorIndexingStatus(VectorIndexingStatus),
            VectorQueueLength = (int)(VectorQueueLength ?? 0),
            Compressed = ParseBooleanValue(Compressed),
            Loaded = Loaded,
        };
    }

    private static VectorIndexingStatus ParseVectorIndexingStatus(object? status)
    {
        if (status == null)
            return Models.VectorIndexingStatus.Ready;

        var statusString = status.ToString()?.ToUpperInvariant();
        return statusString switch
        {
            "READONLY" => Models.VectorIndexingStatus.ReadOnly,
            "INDEXING" => Models.VectorIndexingStatus.Indexing,
            "READY" => Models.VectorIndexingStatus.Ready,
            _ => Models.VectorIndexingStatus.Ready,
        };
    }

    private static bool ParseBooleanValue(object? value)
    {
        if (value == null)
            return false;

        if (value is bool boolValue)
            return boolValue;

        var stringValue = value.ToString()?.ToLowerInvariant();
        return stringValue is "true" or "1" or "yes";
    }
}

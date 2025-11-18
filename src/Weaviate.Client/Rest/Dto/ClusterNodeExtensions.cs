using Weaviate.Client.Models;

namespace Weaviate.Client.Rest.Dto;

/// <summary>
/// Extension methods for NodeStatus DTO to convert to model objects.
/// </summary>
internal partial record NodeStatus
{
    /// <summary>
    /// Converts this NodeStatus DTO to a ClusterNode model.
    /// </summary>
    /// <returns>A ClusterNode model object.</returns>
    public ClusterNode ToModel()
    {
        return new ClusterNode
        {
            GitHash = GitHash ?? string.Empty,
            Name = Name ?? string.Empty,
            StatusRaw = Status?.ToString() ?? "Unknown",
            Version = Version ?? string.Empty,
        };
    }

    /// <summary>
    /// Converts this NodeStatus DTO to a ClusterNodeVerbose model.
    /// </summary>
    /// <returns>A ClusterNodeVerbose model object.</returns>
    public ClusterNodeVerbose ToVerboseModel()
    {
        return new ClusterNodeVerbose
        {
            GitHash = GitHash ?? string.Empty,
            Name = Name ?? string.Empty,
            StatusRaw = Status?.ToString() ?? "Unknown",
            Version = Version ?? string.Empty,
            Stats = Stats?.ToModel() ?? new ClusterNodeVerbose.NodeStats
            {
            ObjectCount = 0,
            ShardCount = 0,
            },
            Shards = Shards
            ?.Where(s =>
                s != null
                && !string.IsNullOrEmpty(s.Class)
                && !string.IsNullOrEmpty(s.Name)
            )
            .Select(s => s.ToModel())
            .ToArray() ?? [],
        };
    }
}

/// <summary>
/// Extension methods for NodeStats DTO to convert to model objects.
/// </summary>
internal partial record NodeStats
{
    /// <summary>
    /// Converts this NodeStats DTO to a NodeStats model.
    /// </summary>
    /// <returns>A NodeStats model object.</returns>
    public ClusterNodeVerbose.NodeStats ToModel()
    {
        return new ClusterNodeVerbose.NodeStats
        {
            ObjectCount = (int)(ObjectCount ?? 0),
            ShardCount = (int)(ShardCount ?? 0),
        };
    }
}

/// <summary>
/// Extension methods for NodeShardStatus DTO to convert to model objects.
/// </summary>
internal partial record NodeShardStatus
{
    /// <summary>
    /// Converts this NodeShardStatus DTO to a Shard model.
    /// </summary>
    /// <returns>A Shard model object.</returns>
    public ClusterNodeVerbose.Shard ToModel()
    {
        return new ClusterNodeVerbose.Shard
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

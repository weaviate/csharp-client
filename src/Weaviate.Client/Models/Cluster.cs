using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// Represents the status of a node in the Weaviate cluster.
/// </summary>
public enum NodeStatus
{
    Unknown,
    Healthy,
    Unhealthy,
    Unavailable,
    Timeout,
}

public static class NodeStatusExtensions
{
    public static NodeStatus ToNodeStatus(this string? status)
    {
        return status?.ToUpperInvariant() switch
        {
            "HEALTHY" => NodeStatus.Healthy,
            "UNHEALTHY" => NodeStatus.Unhealthy,
            "UNAVAILABLE" => NodeStatus.Unavailable,
            "TIMEOUT" => NodeStatus.Timeout,
            _ => NodeStatus.Unknown,
        };
    }
}

/// <summary>
/// Represents a node in the Weaviate cluster with basic information.
/// </summary>
public record ClusterNode
{
    /// <summary>
    /// The git hash of the Weaviate build.
    /// </summary>
    public required string GitHash { get; init; }

    /// <summary>
    /// The unique name of the node.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Current status of the node in raw string form (e.g., "HEALTHY", "UNHEALTHY").
    /// </summary>
    public required string StatusRaw { get; init; }

    /// <summary>
    /// Current status of the node as an enum value.
    /// </summary>
    public NodeStatus Status => StatusRaw.ToNodeStatus();

    /// <summary>
    /// Weaviate version running on the node.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Returns a string representation of the cluster node information.
    /// </summary>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Node: {Name}");
        sb.AppendLine($"  Version: {Version}");
        sb.AppendLine($"  Status: {StatusRaw}");
        sb.AppendLine($"  Git Hash: {GitHash}");
        return sb.ToString().TrimEnd();
    }
}

/// <summary>
/// Represents a node in the Weaviate cluster with detailed information including statistics and shards.
/// </summary>
public record ClusterNodeVerbose : ClusterNode
{
    /// <summary>
    /// Aggregate statistics for the node.
    /// </summary>
    public required NodeStats Stats { get; init; }

    /// <summary>
    /// Array of shard information.
    /// </summary>
    public required Shard[] Shards { get; init; }

    /// <summary>
    /// Returns a string representation of the cluster node information.
    /// </summary>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(base.ToString());
        sb.AppendLine($"  Total Objects: {Stats.ObjectCount}");
        sb.AppendLine($"  Total Shards: {Stats.ShardCount}");

        if (Shards.Length > 0)
        {
            sb.AppendLine("  Shards:");
            foreach (var shard in Shards)
            {
                sb.AppendLine($"    - {shard.Collection}/{shard.Name}");
                sb.AppendLine($"      Objects: {shard.ObjectCount}");
                sb.AppendLine($"      Status: {shard.VectorIndexingStatus}");
                sb.AppendLine($"      Compressed: {shard.Compressed}");
                sb.AppendLine($"      Queue Length: {shard.VectorQueueLength}");
                if (shard.Loaded.HasValue)
                {
                    sb.AppendLine($"      Loaded: {shard.Loaded.Value}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Statistics for a node or collection.
    /// </summary>
    public record NodeStats
    {
        /// <summary>
        /// Total number of objects.
        /// </summary>
        [JsonPropertyName("object_count")]
        public required int ObjectCount { get; init; }

        /// <summary>
        /// Total number of shards.
        /// </summary>
        [JsonPropertyName("shard_count")]
        public required int ShardCount { get; init; }

        /// <summary>
        /// Returns a string representation of the statistics.
        /// </summary>
        public override string ToString()
        {
            return $"Objects: {ObjectCount}, Shards: {ShardCount}";
        }
    }

    /// <summary>
    /// Represents a shard of a collection in the cluster.
    /// </summary>
    public record Shard
    {
        /// <summary>
        /// The name of the collection this shard belongs to.
        /// </summary>
        public required string Collection { get; init; }

        /// <summary>
        /// The name of the shard.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// The name of the node hosting this shard.
        /// </summary>
        public required string Node { get; init; }

        /// <summary>
        /// Number of objects in this shard.
        /// </summary>
        [JsonPropertyName("object_count")]
        public required int ObjectCount { get; init; }

        /// <summary>
        /// The status of the vector indexing process.
        /// </summary>
        [JsonPropertyName("vector_indexing_status")]
        public required VectorIndexingStatus VectorIndexingStatus { get; init; }

        /// <summary>
        /// The length of the vector indexing queue.
        /// </summary>
        [JsonPropertyName("vector_queue_length")]
        public required int VectorQueueLength { get; init; }

        /// <summary>
        /// Whether the shard is compressed.
        /// </summary>
        public required bool Compressed { get; init; }

        /// <summary>
        /// Whether the shard is loaded. Not present in versions &lt; 1.24.x
        /// </summary>
        public bool? Loaded { get; init; }

        /// <summary>
        /// Returns a string representation of the shard information.
        /// </summary>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{Collection}/{Name}");
            sb.AppendLine($"  Node: {Node}");
            sb.AppendLine($"  Objects: {ObjectCount}");
            sb.AppendLine($"  Status: {VectorIndexingStatus}");
            sb.AppendLine($"  Compressed: {Compressed}");
            sb.AppendLine($"  Queue Length: {VectorQueueLength}");
            if (Loaded.HasValue)
            {
                sb.AppendLine($"  Loaded: {Loaded.Value}");
            }
            return sb.ToString().TrimEnd();
        }
    }
}

public enum VectorIndexingStatus
{
    [EnumMember(Value = "READONLY")]
    ReadOnly,

    [EnumMember(Value = "INDEXING")]
    Indexing,

    [EnumMember(Value = "READY")]
    Ready,
}

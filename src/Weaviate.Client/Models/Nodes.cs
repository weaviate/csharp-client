using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public class MinimalNode
{
    /// <summary>
    /// The properties of a single node in the cluster.
    /// </summary>
    public required string GitHash { get; init; }
    public required string Name { get; init; }
    public required string Status { get; init; }
    public required string Version { get; init; }
}

public class VerboseNode : MinimalNode
{
    public required Shard[] Shards { get; init; }
    public required Stats? Stats { get; init; }
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

public class Shard
{
    /// <summary>
    /// The properties of a single shard of a collection.
    /// </summary>
    public required string Collection { get; init; }
    public required string Name { get; init; }
    public required string Node { get; init; }

    [JsonPropertyName("object_count")]
    public required int ObjectCount { get; init; }

    [JsonPropertyName("vector_indexing_status")]
    public required VectorIndexingStatus VectorIndexingStatus { get; init; }

    [JsonPropertyName("vector_queue_length")]
    public required int VectorQueueLength { get; init; }

    public required bool Compressed { get; init; }

    /// <summary>
    /// Not present in versions < 1.24.x
    /// </summary>
    public bool? Loaded { get; init; }
}

public class Stats
{
    /// <summary>
    /// The statistics of a collection.
    /// </summary>
    [JsonPropertyName("object_count")]
    public required int ObjectCount { get; init; }

    [JsonPropertyName("shard_count")]
    public required int ShardCount { get; init; }
}

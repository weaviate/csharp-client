
using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest.Dto;

public enum DeletionStrategy
{
    NoAutomatedResolution,
    DeleteOnConflict,
    TimeBasedResolution
}

/// <summary>
/// ReplicationConfig Configure how replication is executed in a cluster.
/// </summary>
public class ReplicationConfig
{
    /// <summary>
    /// Enable asynchronous replication (default: false).
    /// </summary>
    [JsonPropertyName("asyncEnabled")]
    public bool AsyncEnabled { get; set; } = false;

    /// <summary>
    /// Conflict resolution strategy for deleted objects.
    /// Enum: [NoAutomatedResolution DeleteOnConflict TimeBasedResolution]
    /// </summary>
    [JsonPropertyName("deletionStrategy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DeletionStrategy? DeletionStrategy { get; set; }

    /// <summary>
    /// Number of times a class is replicated (default: 1).
    /// </summary>
    [JsonPropertyName("factor")]
    public long Factor { get; set; } = 1;
}

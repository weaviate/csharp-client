namespace Weaviate.Client.Models;

using System.Text.Json.Serialization;

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
    public bool AsyncEnabled { get; set; } = false;

    /// <summary>
    /// Conflict resolution strategy for deleted objects.
    /// Enum: [NoAutomatedResolution DeleteOnConflict TimeBasedResolution]
    /// </summary>
    public DeletionStrategy? DeletionStrategy { get; set; }

    /// <summary>
    /// Number of times a class is replicated (default: 1).
    /// </summary>
    public long Factor { get; set; } = 1;
}

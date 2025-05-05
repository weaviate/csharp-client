namespace Weaviate.Client.Models;

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
    private static readonly Lazy<ReplicationConfig> _default = new(() => new());

    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    public static ReplicationConfig Default => _default.Value;

    /// <summary>
    /// Enable asynchronous replication (default: false).
    /// </summary>
    public bool AsyncEnabled { get; set; } = false;

    /// <summary>
    /// Conflict resolution strategy for deleted objects.
    /// Enum: [NoAutomatedResolution DeleteOnConflict TimeBasedResolution]
    /// </summary>
    public DeletionStrategy? DeletionStrategy { get; set; } = Models.DeletionStrategy.NoAutomatedResolution;

    /// <summary>
    /// Number of times a class is replicated (default: 1).
    /// </summary>
    public int Factor { get; set; } = 1;
}

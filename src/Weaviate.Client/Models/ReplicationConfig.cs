namespace Weaviate.Client.Models;

/// <summary>
/// The deletion strategy enum
/// </summary>
public enum DeletionStrategy
{
    /// <summary>
    /// The no automated resolution deletion strategy
    /// </summary>
    NoAutomatedResolution,

    /// <summary>
    /// The delete on conflict deletion strategy
    /// </summary>
    DeleteOnConflict,

    /// <summary>
    /// The time based resolution deletion strategy
    /// </summary>
    TimeBasedResolution,
}

/// <summary>
/// ReplicationConfig Configure how replication is executed in a cluster.
/// </summary>
public record ReplicationConfig : IEquatable<ReplicationConfig>
{
    /// <summary>
    /// The default
    /// </summary>
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
    public DeletionStrategy? DeletionStrategy { get; set; } =
        Models.DeletionStrategy.NoAutomatedResolution;

    /// <summary>
    /// Number of times a class is replicated (default: 1).
    /// </summary>
    public int Factor { get; set; } = 1;
}

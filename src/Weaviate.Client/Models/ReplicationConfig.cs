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
/// Fine-grained configuration parameters for asynchronous replication.
/// All fields are optional; omitted fields use server defaults.
/// Requires Weaviate 1.36 or later.
/// </summary>
public record ReplicationAsyncConfig
{
    /// <summary>Maximum number of async replication workers.</summary>
    public long? MaxWorkers { get; set; }

    /// <summary>Height of the hashtree used for diffing.</summary>
    public long? HashtreeHeight { get; set; }

    /// <summary>Base frequency in milliseconds at which async replication runs diff calculations.</summary>
    public long? Frequency { get; set; }

    /// <summary>Frequency in milliseconds at which async replication runs while propagation is active.</summary>
    public long? FrequencyWhilePropagating { get; set; }

    /// <summary>Interval in milliseconds at which liveness of target nodes is checked.</summary>
    public long? AliveNodesCheckingFrequency { get; set; }

    /// <summary>Interval in seconds at which async replication logs its status.</summary>
    public long? LoggingFrequency { get; set; }

    /// <summary>Maximum number of object keys included in a single diff batch.</summary>
    public long? DiffBatchSize { get; set; }

    /// <summary>Timeout in seconds for computing a diff against a single node.</summary>
    public long? DiffPerNodeTimeout { get; set; }

    /// <summary>Overall timeout in seconds for the pre-propagation phase.</summary>
    public long? PrePropagationTimeout { get; set; }

    /// <summary>Timeout in seconds for propagating a batch of changes to a node.</summary>
    public long? PropagationTimeout { get; set; }

    /// <summary>Maximum number of objects to propagate in a single async replication run.</summary>
    public long? PropagationLimit { get; set; }

    /// <summary>Delay in milliseconds before newly added or updated objects are propagated.</summary>
    public long? PropagationDelay { get; set; }

    /// <summary>Maximum number of concurrent propagation workers.</summary>
    public long? PropagationConcurrency { get; set; }

    /// <summary>Number of objects to include in a single propagation batch.</summary>
    public long? PropagationBatchSize { get; set; }
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
    /// Fine-grained parameters for asynchronous replication.
    /// Requires Weaviate 1.36 or later; ignored by older servers.
    /// </summary>
    public ReplicationAsyncConfig? AsyncConfig { get; set; }

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

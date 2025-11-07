namespace Weaviate.Client.Models;

/// <summary>
/// Type of replication operation
/// </summary>
public enum ReplicationType
{
    /// <summary>
    /// Copy the shard to the target node, keeping the source replica
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "COPY")]
    Copy,

    /// <summary>
    /// Move the shard to the target node, removing the source replica after successful transfer
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "MOVE")]
    Move,
}

/// <summary>
/// State of a replication operation
/// </summary>
public enum ReplicationOperationState
{
    /// <summary>
    /// Operation has been registered but not yet started
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "REGISTERED")]
    Registered,

    /// <summary>
    /// Operation is copying data to the target node
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "HYDRATING")]
    Hydrating,

    /// <summary>
    /// Operation is finalizing the replication
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "FINALIZING")]
    Finalizing,

    /// <summary>
    /// Operation is removing data from the source node (MOVE operations only)
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "DEHYDRATING")]
    Dehydrating,

    /// <summary>
    /// Operation has completed successfully
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "READY")]
    Ready,

    /// <summary>
    /// Operation was cancelled
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "CANCELLED")]
    Cancelled,
}

/// <summary>
/// Error that occurred during a replication operation
/// </summary>
public record ReplicationOperationError(
    /// <summary>
    /// Unix timestamp in milliseconds when the error occurred
    /// </summary>
    long WhenErroredUnixMs,
    /// <summary>
    /// Human-readable error message
    /// </summary>
    string Message
)
{
    /// <summary>
    /// When the error occurred
    /// </summary>
    public DateTimeOffset WhenErrored => DateTimeOffset.FromUnixTimeMilliseconds(WhenErroredUnixMs);
}

/// <summary>
/// Status of a replication operation at a point in time
/// </summary>
public record ReplicationOperationStatus(
    /// <summary>
    /// The operational state
    /// </summary>
    ReplicationOperationState State,
    /// <summary>
    /// Unix timestamp in milliseconds when this state was entered
    /// </summary>
    long? WhenStartedUnixMs,
    /// <summary>
    /// Errors encountered in this state, if any
    /// </summary>
    IReadOnlyList<ReplicationOperationError>? Errors
)
{
    /// <summary>
    /// When this state was entered
    /// </summary>
    public DateTimeOffset? WhenStarted =>
        WhenStartedUnixMs.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(WhenStartedUnixMs.Value)
            : null;
}

/// <summary>
/// Complete details of a replication operation
/// </summary>
public record ReplicationOperation(
    /// <summary>
    /// Unique identifier of the operation
    /// </summary>
    Guid Id,
    /// <summary>
    /// Name of the collection being replicated
    /// </summary>
    string Collection,
    /// <summary>
    /// Name of the shard being replicated
    /// </summary>
    string Shard,
    /// <summary>
    /// Name of the source node
    /// </summary>
    string SourceNode,
    /// <summary>
    /// Name of the target node
    /// </summary>
    string TargetNode,
    /// <summary>
    /// Type of replication (COPY or MOVE)
    /// </summary>
    ReplicationType Type,
    /// <summary>
    /// Current status of the operation
    /// </summary>
    ReplicationOperationStatus Status,
    /// <summary>
    /// Unix timestamp in milliseconds when the operation started
    /// </summary>
    long? WhenStartedUnixMs,
    /// <summary>
    /// Whether the operation is uncancelable
    /// </summary>
    bool? Uncancelable,
    /// <summary>
    /// Whether the operation is scheduled for cancellation
    /// </summary>
    bool? ScheduledForCancel,
    /// <summary>
    /// Whether the operation is scheduled for deletion
    /// </summary>
    bool? ScheduledForDelete,
    /// <summary>
    /// Historical sequence of statuses the operation has transitioned through
    /// </summary>
    IReadOnlyList<ReplicationOperationStatus>? StatusHistory
)
{
    /// <summary>
    /// When the operation started
    /// </summary>
    public DateTimeOffset? WhenStarted =>
        WhenStartedUnixMs.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(WhenStartedUnixMs.Value)
            : null;

    /// <summary>
    /// True if the operation has completed (successfully or not)
    /// </summary>
    public bool IsCompleted =>
        Status.State is ReplicationOperationState.Ready or ReplicationOperationState.Cancelled;

    /// <summary>
    /// True if the operation completed successfully
    /// </summary>
    public bool IsSuccessful => Status.State == ReplicationOperationState.Ready;

    /// <summary>
    /// True if the operation was cancelled
    /// </summary>
    public bool IsCancelled => Status.State == ReplicationOperationState.Cancelled;
}

/// <summary>
/// Request to initiate a replication operation
/// </summary>
public record ReplicateRequest(
    /// <summary>
    /// Name of the collection to replicate
    /// </summary>
    string Collection,
    /// <summary>
    /// Name of the shard to replicate
    /// </summary>
    string Shard,
    /// <summary>
    /// Name of the source node hosting the replica
    /// </summary>
    string SourceNode,
    /// <summary>
    /// Name of the target node where the replica will be created
    /// </summary>
    string TargetNode,
    /// <summary>
    /// Type of replication operation (defaults to COPY)
    /// </summary>
    ReplicationType Type = ReplicationType.Copy
);

/// <summary>
/// Configuration for replication client polling behavior
/// </summary>
public class ReplicationClientConfig
{
    /// <summary>
    /// Default configuration instance
    /// </summary>
    public static ReplicationClientConfig Default { get; } = new();

    /// <summary>
    /// How often to poll for status updates during async operations
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Default timeout for waiting for operation completion
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);
}

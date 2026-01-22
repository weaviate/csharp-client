namespace Weaviate.Client.Models;

/// <summary>
/// Type of replication operation
/// </summary>
public enum ReplicationType
{
    /// <summary>
    /// Copy the shard to the target node, keeping the source replica
    /// </summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("COPY")]
    Copy,

    /// <summary>
    /// Move the shard to the target node, removing the source replica after successful transfer
    /// </summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("MOVE")]
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
    [System.Text.Json.Serialization.JsonStringEnumMemberName("REGISTERED")]
    Registered,

    /// <summary>
    /// Operation is copying data to the target node
    /// </summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("HYDRATING")]
    Hydrating,

    /// <summary>
    /// Operation is finalizing the replication
    /// </summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("FINALIZING")]
    Finalizing,

    /// <summary>
    /// Operation is removing data from the source node (MOVE operations only)
    /// </summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("DEHYDRATING")]
    Dehydrating,

    /// <summary>
    /// Operation has completed successfully
    /// </summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("READY")]
    Ready,

    /// <summary>
    /// Operation was cancelled
    /// </summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("CANCELLED")]
    Cancelled,
}

/// <summary>
/// Error that occurred during a replication operation
/// </summary>
/// <param name="WhenErroredUnixMs">Unix timestamp in milliseconds when the error occurred</param>
/// <param name="Message">Human-readable error message</param>
public record ReplicationOperationError(long WhenErroredUnixMs, string Message)
{
    /// <summary>
    /// When the error occurred
    /// </summary>
    public DateTimeOffset WhenErrored => DateTimeOffset.FromUnixTimeMilliseconds(WhenErroredUnixMs);
}

/// <summary>
/// Status of a replication operation at a point in time
/// </summary>
/// <param name="State">The operational state</param>
/// <param name="WhenStartedUnixMs">Unix timestamp in milliseconds when this state was entered</param>
/// <param name="Errors">Errors encountered in this state, if any</param>
public record ReplicationOperationStatus(
    ReplicationOperationState State,
    long? WhenStartedUnixMs,
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
/// <param name="Id">Unique identifier of the operation</param>
/// <param name="Collection">Name of the collection being replicated</param>
/// <param name="Shard">Name of the shard being replicated</param>
/// <param name="SourceNode">Name of the source node</param>
/// <param name="TargetNode">Name of the target node</param>
/// <param name="Type">Type of replication (COPY or MOVE)</param>
/// <param name="Status">Current status of the operation</param>
/// <param name="WhenStartedUnixMs">Unix timestamp in milliseconds when the operation started</param>
/// <param name="Uncancelable">Whether the operation is uncancelable</param>
/// <param name="ScheduledForCancel">Whether the operation is scheduled for cancellation</param>
/// <param name="ScheduledForDelete">Whether the operation is scheduled for deletion</param>
/// <param name="StatusHistory">Historical sequence of statuses the operation has transitioned through</param>
public record ReplicationOperation(
    Guid Id,
    string Collection,
    string Shard,
    string SourceNode,
    string TargetNode,
    ReplicationType Type,
    ReplicationOperationStatus Status,
    long? WhenStartedUnixMs,
    bool? Uncancelable,
    bool? ScheduledForCancel,
    bool? ScheduledForDelete,
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
/// <param name="Collection">Name of the collection to replicate</param>
/// <param name="Shard">Name of the shard to replicate</param>
/// <param name="SourceNode">Name of the source node hosting the replica</param>
/// <param name="TargetNode">Name of the target node where the replica will be created</param>
/// <param name="Type">Type of replication operation (defaults to COPY)</param>
public record ReplicateRequest(
    string Collection,
    string Shard,
    string SourceNode,
    string TargetNode,
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

namespace Weaviate.Client.Models;

/// <summary>
/// Represents the possible status values for a shard.
/// </summary>
public enum ShardStatus
{
    /// <summary>
    /// The shard is ready to serve requests.
    /// </summary>
    Ready,

    /// <summary>
    /// The shard is in read-only mode.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// The shard is indexing data.
    /// </summary>
    Indexing,
}

/// <summary>
/// Information about a collection shard, including its name, status, and statistics.
/// </summary>
public record ShardInfo
{
    /// <summary>
    /// Gets or sets the name of the shard.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the shard (e.g., "READY", "READONLY", "INDEXING").
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the vector queue for the shard.
    /// </summary>
    public int? VectorQueueSize { get; set; }

    /// <summary>
    /// Gets the parsed status as a ShardStatus enum.
    /// Returns null if the status cannot be parsed.
    /// </summary>
    public ShardStatus? StatusValue =>
        Status?.ToUpperInvariant() switch
        {
            "READY" => ShardStatus.Ready,
            "READONLY" => ShardStatus.ReadOnly,
            "INDEXING" => ShardStatus.Indexing,
            _ => null,
        };
}

/// <summary>
/// Extension methods for ShardStatus enum.
/// </summary>
internal static class ShardStatusExtensions
{
    /// <summary>
    /// Converts a ShardStatus enum to its string representation for the API.
    /// </summary>
    internal static string ToApiString(this ShardStatus status)
    {
        return status switch
        {
            ShardStatus.Ready => "READY",
            ShardStatus.ReadOnly => "READONLY",
            ShardStatus.Indexing => "INDEXING",
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown shard status value"
            ),
        };
    }
}

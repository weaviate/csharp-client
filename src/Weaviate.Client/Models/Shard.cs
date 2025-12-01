using System.Runtime.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// Represents the possible status values for a shard.
/// </summary>
public enum ShardStatus
{
    /// <summary>
    /// The shard is ready to serve requests.
    /// </summary>
    [EnumMember(Value = "READY")]
    Ready,

    /// <summary>
    /// The shard is in read-only mode.
    /// </summary>
    [EnumMember(Value = "READONLY")]
    ReadOnly,

    /// <summary>
    /// The shard is indexing data.
    /// </summary>
    [EnumMember(Value = "INDEXING")]
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
    /// Gets or sets the status of the shard.
    /// </summary>
    public ShardStatus Status { get; set; } = ShardStatus.Ready;

    /// <summary>
    /// Gets or sets the size of the vector queue for the shard.
    /// </summary>
    public int? VectorQueueSize { get; set; }
}

/// <summary>
/// Extension methods for ShardStatus enum.
/// </summary>
internal static class ShardStatusExtensions
{
    /// <summary>
    /// Converts a ShardStatus enum to its string representation for the API.
    /// </summary>
    internal static string ToApiString(this ShardStatus status) =>
        WeaviateExtensions.ToEnumMemberString(status);

    /// <summary>
    /// Parses a string value to a ShardStatus enum.
    /// </summary>
    internal static ShardStatus ParseStatus(string? value) =>
        string.IsNullOrEmpty(value)
            ? ShardStatus.Ready
            : WeaviateExtensions.FromEnumMemberString<ShardStatus>(value);
}

namespace Weaviate.Client.Models;

/// <summary>
/// Configuration for BackupClient operations, including default polling behavior
/// </summary>
public record BackupClientConfig
{
    /// <summary>
    /// Default polling interval for checking backup status.
    /// Default: 250ms
    /// </summary>
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Default timeout for waiting for completion of backup operations.
    /// Default: 10 minutes
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Default configuration instance
    /// </summary>
    public static BackupClientConfig Default { get; } = new();
}

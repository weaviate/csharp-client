namespace Weaviate.Client.Models;

/// <summary>
/// Configuration for export client operations (polling interval, timeout).
/// </summary>
public record ExportClientConfig
{
    /// <summary>
    /// Default polling interval for checking export status.
    /// Default: 250ms
    /// </summary>
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Default timeout for waiting for completion of export operations.
    /// Default: 10 minutes
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Default configuration instance.
    /// </summary>
    public static ExportClientConfig Default { get; } = new();
}

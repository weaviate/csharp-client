using Weaviate.Client.Internal;

namespace Weaviate.Client.Models;

/// <summary>
/// Specifies the file format for export operations.
/// </summary>
public enum ExportFileFormat
{
    /// <summary>
    /// Apache Parquet format
    /// </summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("parquet")]
    Parquet,
}

/// <summary>
/// Represents the status of an export operation.
/// </summary>
public enum ExportStatus
{
    /// <summary>
    /// The status is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// The export has started.
    /// </summary>
    Started,

    /// <summary>
    /// The export is transferring data.
    /// </summary>
    Transferring,

    /// <summary>
    /// The export completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The export failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The export was canceled.
    /// </summary>
    Canceled,
}

/// <summary>
/// Provides extension methods for parsing <see cref="ExportStatus"/> values from strings.
/// </summary>
public static class ExportStatusExtensions
{
    /// <summary>
    /// Converts a string status to an <see cref="ExportStatus"/> value.
    /// </summary>
    /// <param name="status">The status string to parse.</param>
    /// <returns>The parsed <see cref="ExportStatus"/> value.</returns>
    public static ExportStatus ToExportStatus(this string? status)
    {
        return status?.ToUpperInvariant() switch
        {
            "STARTED" => ExportStatus.Started,
            "TRANSFERRING" => ExportStatus.Transferring,
            "SUCCESS" => ExportStatus.Success,
            "FAILED" => ExportStatus.Failed,
            "CANCELED" => ExportStatus.Canceled,
            _ => ExportStatus.Unknown,
        };
    }
}

/// <summary>
/// Represents an export as returned by create/status operations.
/// </summary>
public record Export(
    string Id,
    BackupBackend Backend,
    string? Path,
    string StatusRaw,
    string[]? Collections,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? Error
)
{
    /// <summary>
    /// Parsed export status
    /// </summary>
    public ExportStatus Status => StatusRaw.ToExportStatus();

    /// <summary>
    /// Per-shard export progress. Outer key is collection, inner key is shard name.
    /// </summary>
    public Dictionary<string, Dictionary<string, ShardProgress>>? ShardStatus { get; init; }

    /// <summary>
    /// Time taken in milliseconds, available after completion.
    /// </summary>
    public int? TookInMs { get; init; }
}

/// <summary>
/// Represents the progress of a single shard export.
/// </summary>
public record ShardProgress(
    string StatusRaw,
    int ObjectsExported,
    string? Error,
    string? SkipReason
);

/// <summary>
/// Represents the options for creating an export operation.
/// </summary>
public record ExportCreateRequest(
    string Id,
    BackupBackend Backend,
    ExportFileFormat FileFormat = ExportFileFormat.Parquet,
    AutoArray<string>? IncludeCollections = null,
    AutoArray<string>? ExcludeCollections = null
);

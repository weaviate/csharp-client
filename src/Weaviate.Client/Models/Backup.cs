namespace Weaviate.Client.Models;

public enum BackupStatus
{
    Unknown,
    Started,
    Transferring,
    Transferred,
    Success,
    Failed,
    Canceled,
}

/// <summary>
/// Compression level for backup operations
/// </summary>
public enum BackupCompressionLevel
{
    /// <summary>
    /// Default compression level
    /// </summary>
    DefaultCompression,

    /// <summary>
    /// Optimized for speed
    /// </summary>
    BestSpeed,

    /// <summary>
    /// Optimized for compression ratio
    /// </summary>
    BestCompression,
}

/// <summary>
/// Backend storage type for backups
/// </summary>
public enum BackupStorage
{
    /// <summary>
    /// Local filesystem storage
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "filesystem")]
    Filesystem,

    /// <summary>
    /// Amazon S3 storage
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "s3")]
    S3,

    /// <summary>
    /// Google Cloud Storage
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "gcs")]
    GCS,

    /// <summary>
    /// Azure Blob Storage
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "azure")]
    Azure,
}

public static class BackupStatusExtensions
{
    public static BackupStatus ToBackupStatus(this string? status)
    {
        return status?.ToUpperInvariant() switch
        {
            "STARTED" => BackupStatus.Started,
            "TRANSFERRING" => BackupStatus.Transferring,
            "TRANSFERRED" => BackupStatus.Transferred,
            "SUCCESS" => BackupStatus.Success,
            "FAILED" => BackupStatus.Failed,
            "CANCELED" => BackupStatus.Canceled,
            _ => BackupStatus.Unknown,
        };
    }
}

/// <summary>
/// Represents a backup as returned by list/status operations
/// </summary>
public record Backup(
    string Id,
    string Backend,
    string? Bucket,
    string? Path,
    string StatusRaw,
    string[]? Classes,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? Error
)
{
    public BackupStatus Status => StatusRaw.ToBackupStatus();
}

/// <summary>
/// Options for creating a backup
/// </summary>
public record BackupCreateRequest(
    string Id,
    IEnumerable<string>? Include = null,
    IEnumerable<string>? Exclude = null,
    BackupConfig? Config = null
);

/// <summary>
/// Options for restoring a backup
/// </summary>
public record BackupRestoreRequest(
    IEnumerable<string>? Include = null,
    IEnumerable<string>? Exclude = null,
    IDictionary<string, string>? NodeMapping = null,
    RestoreConfig? Config = null,
    bool? OverwriteAlias = null
);

public record BackupConfig(
    string? Endpoint = null,
    string? Bucket = null,
    string? Path = null,
    int? CPUPercentage = null,
    int? ChunkSize = null,
    BackupCompressionLevel? CompressionLevel = null
);

public record RestoreConfig(
    string? Endpoint = null,
    string? Bucket = null,
    string? Path = null,
    int? CPUPercentage = null,
    string? RolesOptions = null,
    string? UsersOptions = null
);

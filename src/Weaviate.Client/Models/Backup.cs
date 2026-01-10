namespace Weaviate.Client.Models;

/// <summary>
/// The backup status enum
/// </summary>
public enum BackupStatus
{
    /// <summary>
    /// The unknown backup status
    /// </summary>
    Unknown,

    /// <summary>
    /// The started backup status
    /// </summary>
    Started,

    /// <summary>
    /// The transferring backup status
    /// </summary>
    Transferring,

    /// <summary>
    /// The transferred backup status
    /// </summary>
    Transferred,

    /// <summary>
    /// The success backup status
    /// </summary>
    Success,

    /// <summary>
    /// The failed backup status
    /// </summary>
    Failed,

    /// <summary>
    /// The canceled backup status
    /// </summary>
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

    /// <summary>
    /// Zstd compressed with default (balanced) parameters
    /// </summary>
    ZstdDefaultCompression,

    /// <summary>
    /// Zstd compressed optimized for best speed
    /// </summary>
    ZstdBestSpeed,

    /// <summary>
    /// Zstd compressed optimized for best compression ratio
    /// </summary>
    ZstdBestCompression,

    /// <summary>
    /// No compression
    /// </summary>
    NoCompression,
}

/// <summary>
/// Backend storage provider type for backups
/// </summary>
public enum BackupStorageProvider
{
    /// <summary>
    /// No backend specified
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "none")]
    None,

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

/// <summary>
/// Base class for backup backend configurations
/// </summary>
public abstract record BackupBackend
{
    /// <summary>
    /// The backend storage provider
    /// </summary>
    public abstract BackupStorageProvider Provider { get; }

    /// <summary>
    /// Optional path for the backup location
    /// </summary>
    public abstract string? Path { get; }

    /// <summary>
    /// Creates an empty backend (no backend specified)
    /// </summary>
    public static BackupBackend Empty() => new EmptyBackend();

    /// <summary>
    /// Creates a filesystem backend
    /// </summary>
    public static BackupBackend Filesystem(string? path = null) => new FilesystemBackend(path);

    /// <summary>
    /// Creates an S3 backend
    /// </summary>
    public static BackupBackend S3(string? bucket = null, string? path = null) =>
        new ObjectStorageBackend(BackupStorageProvider.S3, bucket, path);

    /// <summary>
    /// Creates a GCS backend
    /// </summary>
    public static BackupBackend GCS(string? bucket = null, string? path = null) =>
        new ObjectStorageBackend(BackupStorageProvider.GCS, bucket, path);

    /// <summary>
    /// Creates an Azure backend
    /// </summary>
    public static BackupBackend Azure(string? bucket = null, string? path = null) =>
        new ObjectStorageBackend(BackupStorageProvider.Azure, bucket, path);
}

/// <summary>
/// Empty backend configuration (no backend specified)
/// </summary>
internal record EmptyBackend() : BackupBackend
{
    /// <summary>
    /// Gets the value of the provider
    /// </summary>
    public override BackupStorageProvider Provider => BackupStorageProvider.None;

    /// <summary>
    /// Gets the value of the path
    /// </summary>
    public override string? Path => null;
}

/// <summary>
/// Filesystem backend configuration for backups
/// </summary>
public record FilesystemBackend : BackupBackend
{
    /// <summary>
    /// Optional path for the backup location
    /// </summary>
    public override string? Path { get; }

    /// <summary>
    /// The backend provider, always Filesystem
    /// </summary>
    public override BackupStorageProvider Provider => BackupStorageProvider.Filesystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesystemBackend"/> class
    /// </summary>
    /// <param name="path">The path</param>
    public FilesystemBackend(string? path = null)
    {
        Path = path;
    }
}

/// <summary>
/// Object storage backend configuration (S3, GCS, Azure, etc.)
/// </summary>
public record ObjectStorageBackend : BackupBackend
{
    /// <summary>
    /// The backend storage provider
    /// </summary>
    public override BackupStorageProvider Provider { get; }

    /// <summary>
    /// The bucket name for object storage
    /// </summary>
    public string? Bucket { get; }

    /// <summary>
    /// Optional path within the bucket
    /// </summary>
    public override string? Path { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectStorageBackend"/> class
    /// </summary>
    /// <param name="provider">The provider</param>
    /// <param name="bucket">The bucket</param>
    /// <param name="path">The path</param>
    public ObjectStorageBackend(
        BackupStorageProvider provider,
        string? bucket = null,
        string? path = null
    )
    {
        Provider = provider;
        Bucket = bucket;
        Path = path;
    }
}

/// <summary>
/// The backup status extensions class
/// </summary>
public static class BackupStatusExtensions
{
    /// <summary>
    /// Returns the backup status using the specified status
    /// </summary>
    /// <param name="status">The status</param>
    /// <returns>The backup status</returns>
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
    BackupBackend Backend,
    string StatusRaw,
    string[]? Classes,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? Error
)
{
    /// <summary>
    /// Gets the value of the status
    /// </summary>
    public BackupStatus Status => StatusRaw.ToBackupStatus();
}

/// <summary>
/// Options for creating a backup
/// </summary>
public record BackupCreateRequest(
    string Id,
    BackupBackend Backend,
    AutoArray<string>? IncludeCollections = null,
    AutoArray<string>? ExcludeCollections = null,
    int? CPUPercentage = null,
    BackupCompressionLevel? CompressionLevel = null
);

/// <summary>
/// Options for restoring a backup
/// </summary>
public record BackupRestoreRequest(
    string Id,
    BackupBackend Backend,
    AutoArray<string>? IncludeCollections = null,
    AutoArray<string>? ExcludeCollections = null,
    IDictionary<string, string>? NodeMapping = null,
    int? CPUPercentage = null,
    RolesRestoreOption? RolesOptions = null,
    UserRestoreOption? UsersOptions = null,
    bool? OverwriteAlias = null
);

/// <summary>
/// Options for restore behavior of users
/// </summary>
public enum UserRestoreOption
{
    /// <summary>
    /// The no restore user restore option
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "noRestore")]
    NoRestore,

    /// <summary>
    /// The all user restore option
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "all")]
    All,
}

/// <summary>
/// Options for restore behavior of roles
/// </summary>
public enum RolesRestoreOption
{
    /// <summary>
    /// The no restore roles restore option
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "noRestore")]
    NoRestore,

    /// <summary>
    /// The all roles restore option
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "all")]
    All,
}

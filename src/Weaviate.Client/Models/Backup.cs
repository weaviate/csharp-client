namespace Weaviate.Client.Models;

/// <summary>
/// Represents a backup as returned by list/status operations
/// </summary>
public record Backup(
    string Id,
    string Backend,
    string? Bucket,
    string? Path,
    string Status,
    string[]? Classes,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? Error
);

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
    string? CompressionLevel = null
);

public record RestoreConfig(
    string? Endpoint = null,
    string? Bucket = null,
    string? Path = null,
    int? CPUPercentage = null,
    string? RolesOptions = null,
    string? UsersOptions = null
);

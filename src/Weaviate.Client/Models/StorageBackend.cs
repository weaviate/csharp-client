namespace Weaviate.Client.Models;

/// <summary>
/// Shared base for storage backend configurations used by Weaviate sub-clients that
/// persist data to external storage (backup, export, …). Concrete subtypes
/// (<see cref="FilesystemBackend"/>, <see cref="ObjectStorageBackend"/>, …) describe a
/// specific storage location; the per-feature abstract types <see cref="BackupBackend"/>
/// and <see cref="ExportBackend"/> serve as discoverable factory namespaces with
/// feature-appropriate naming, and inherit from this type so the same concrete instances
/// can be passed to either feature's API.
/// </summary>
public abstract record StorageBackend
{
    /// <summary>
    /// The backend storage provider (filesystem, S3, GCS, Azure, …).
    /// </summary>
    public abstract BackupStorageProvider Provider { get; }

    /// <summary>
    /// Optional path within the storage location.
    /// </summary>
    public abstract string? Path { get; }
}

using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class WeaviateClient
{
    private BackupClient? _backups;

    public BackupClient Backups => _backups ??= new(this);
}

public class BackupClient
{
    private readonly WeaviateClient _client;

    /// <summary>
    /// Static configuration used for all backup operations.
    /// Can be modified to change default polling behavior.
    /// </summary>
    public static BackupClientConfig Config { get; set; } = BackupClientConfig.Default;

    internal BackupClient(WeaviateClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Start creating a backup asynchronously.
    /// Returns a BackupCreateOperation that can be used to track status or wait for completion.
    /// </summary>
    /// <example>
    /// // Filesystem backend
    /// var operation = await client.Backups.Create(new BackupCreateRequest(
    ///     "my-backup-id",
    ///     new FilesystemBackend(path: "/backups")
    /// ));
    ///
    /// // S3 backend
    /// var operation = await client.Backups.Create(new BackupCreateRequest(
    ///     "my-backup-id",
    ///     ObjectStorageBackend.S3(bucket: "my-bucket")
    /// ));
    /// </example>
    public async Task<BackupCreateOperation> Create(
        Models.BackupCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var restRequest = BuildBackupCreateRequest(request);
        var response = await _client.RestClient.BackupCreate(
            request.Backend.Provider,
            restRequest,
            cancellationToken
        );
        var model = ToModel(response);

        return new BackupCreateOperation(
            model,
            async (ct) => await GetStatus(request.Backend, model.Id, ct),
            async (ct) => await Cancel(request.Backend, model.Id, ct)
        );
    }

    /// <summary>
    /// Create a backup and wait synchronously for completion.
    /// This method blocks until the backup operation finishes.
    /// </summary>
    public async Task<Backup> CreateSync(
        Models.BackupCreateRequest request,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var operation = await Create(request, cancellationToken);
        return await operation.WaitForCompletion(timeout, cancellationToken);
    }

    private Rest.Dto.BackupCreateRequest BuildBackupCreateRequest(
        Models.BackupCreateRequest request
    )
    {
        var backend = request.Backend;
        var bucket = backend is ObjectStorageBackend osb ? osb.Bucket : null;
        var path = backend.Path;

        return new Rest.Dto.BackupCreateRequest
        {
            Id = request.Id,
            Include = request.Include?.ToList(),
            Exclude = request.Exclude?.ToList(),
            Config = request.Config is null
                ? null
                : new Rest.Dto.BackupConfig
                {
                    Endpoint = request.Config.Endpoint,
                    Bucket = bucket,
                    Path = path,
                    CPUPercentage = request.Config.CPUPercentage,
                    ChunkSize = request.Config.ChunkSize,
                    CompressionLevel = request.Config.CompressionLevel switch
                    {
                        Models.BackupCompressionLevel.BestSpeed => Rest.Dto
                            .BackupConfigCompressionLevel
                            .BestSpeed,
                        Models.BackupCompressionLevel.BestCompression => Rest.Dto
                            .BackupConfigCompressionLevel
                            .BestCompression,
                        Models.BackupCompressionLevel.DefaultCompression => Rest.Dto
                            .BackupConfigCompressionLevel
                            .DefaultCompression,
                        null => Rest.Dto.BackupConfigCompressionLevel.DefaultCompression,
                        _ => Rest.Dto.BackupConfigCompressionLevel.DefaultCompression,
                    },
                },
        };
    }

    /// <summary>
    /// List existing backups for a backend provider
    /// </summary>
    /// <param name="provider">The backup storage provider to list backups from.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An enumerable collection of backups.</returns>
    public async Task<IEnumerable<Backup>> List(
        BackupStorageProvider provider,
        CancellationToken cancellationToken = default
    )
    {
        var list = await _client.RestClient.BackupList(provider, cancellationToken);
        return list.Select(ToModelListItem) ?? Array.Empty<Backup>();
    }

    /// <summary>
    /// Get creation status for a backup
    /// </summary>
    /// <param name="backend">The backup backend to check status from.</param>
    /// <param name="id">The backup ID to check status for.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The backup status information.</returns>
    public async Task<Backup> GetStatus(
        BackupBackend backend,
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var bucket = backend is ObjectStorageBackend osb ? osb.Bucket : null;
        var path = backend.Path;

        var status = await _client.RestClient.BackupStatus(
            backend.Provider,
            id,
            bucket,
            path,
            cancellationToken
        );
        return ToModel(status, backend);
    }

    /// <summary>
    /// Cancel a running backup
    /// </summary>
    public Task Cancel(
        BackupBackend backend,
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var bucket = backend is ObjectStorageBackend osb ? osb.Bucket : null;
        var path = backend.Path;

        return _client.RestClient.BackupCancel(
            backend.Provider,
            id,
            bucket,
            path,
            cancellationToken
        );
    }

    /// <summary>
    /// Start restoring a backup asynchronously.
    /// Returns a BackupRestoreOperation that can be used to track status or wait for completion.
    /// </summary>
    /// <example>
    /// // Filesystem backend
    /// var operation = await client.Backups.Restore(new BackupRestoreRequest(
    ///     "my-backup-id",
    ///     new FilesystemBackend(path: "/backups"),
    ///     Include: new[] { "Article" }
    /// ));
    ///
    /// // S3 backend
    /// var operation = await client.Backups.Restore(new BackupRestoreRequest(
    ///     "my-backup-id",
    ///     ObjectStorageBackend.S3(bucket: "my-bucket")
    /// ));
    /// </example>
    public async Task<BackupRestoreOperation> Restore(
        Models.BackupRestoreRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var restRequest = BuildBackupRestoreRequest(request);
        var response = await _client.RestClient.BackupRestore(
            request.Backend.Provider,
            request.Id,
            restRequest,
            cancellationToken
        );
        var model = ToModel(response);

        return new BackupRestoreOperation(
            model,
            async (ct) => await GetRestoreStatus(request.Backend, model.Id, ct),
            async (ct) => await Cancel(request.Backend, model.Id, ct)
        );
    }

    /// <summary>
    /// Restore a backup and wait synchronously for completion.
    /// This method blocks until the restore operation finishes.
    /// </summary>
    public async Task<Backup> RestoreSync(
        Models.BackupRestoreRequest request,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var operation = await Restore(request, cancellationToken);
        return await operation.WaitForCompletion(timeout, cancellationToken);
    }

    private Rest.Dto.BackupRestoreRequest BuildBackupRestoreRequest(
        Models.BackupRestoreRequest request
    )
    {
        var backend = request.Backend;
        var bucket = backend is ObjectStorageBackend osb ? osb.Bucket : null;
        var path = backend.Path;

        return new Rest.Dto.BackupRestoreRequest
        {
            Include = request.Include?.ToList(),
            Exclude = request.Exclude?.ToList(),
            Node_mapping = request.NodeMapping is null
                ? null
                : new Dictionary<string, string>(request.NodeMapping),
            Config = request.Config is null
                ? null
                : new Rest.Dto.RestoreConfig
                {
                    Endpoint = request.Config.Endpoint,
                    Bucket = bucket,
                    Path = path,
                    CPUPercentage = request.Config.CPUPercentage,
                    RolesOptions = request.Config.RolesOptions switch
                    {
                        "all" => Rest.Dto.RestoreConfigRolesOptions.All,
                        _ => Rest.Dto.RestoreConfigRolesOptions.NoRestore,
                    },
                    UsersOptions = request.Config.UsersOptions switch
                    {
                        "all" => Rest.Dto.RestoreConfigUsersOptions.All,
                        _ => Rest.Dto.RestoreConfigUsersOptions.NoRestore,
                    },
                },
            OverwriteAlias = request.OverwriteAlias,
        };
    }

    /// <summary>
    /// Get status for a restore operation
    /// </summary>
    /// <param name="backend">The backup backend to check restore status from.</param>
    /// <param name="id">The backup ID to check restore status for.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The backup restore status information.</returns>
    public async Task<Backup> GetRestoreStatus(
        BackupBackend backend,
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var bucket = backend is ObjectStorageBackend osb ? osb.Bucket : null;
        var path = backend.Path;

        var status = await _client.RestClient.BackupRestoreStatus(
            backend.Provider,
            id,
            bucket,
            path,
            cancellationToken
        );
        return ToModel(status, backend);
    }

    private static BackupBackend ParseBackend(string? backendStr, string? bucket, string? path)
    {
        var provider = backendStr?.ToLowerInvariant() switch
        {
            "filesystem" => BackupStorageProvider.Filesystem,
            "s3" => BackupStorageProvider.S3,
            "gcs" => BackupStorageProvider.GCS,
            "azure" => BackupStorageProvider.Azure,
            _ => BackupStorageProvider.None,
        };

        if (provider == BackupStorageProvider.None)
            return BackupBackend.Empty();

        return provider == BackupStorageProvider.Filesystem
            ? new FilesystemBackend(path)
            : new ObjectStorageBackend(provider, bucket, path);
    }

    private static Backup ToModel(Rest.Dto.BackupCreateResponse dto) =>
        new(
            dto.Id ?? string.Empty,
            ParseBackend(dto.Backend, dto.Bucket, dto.Path),
            dto.Status?.ToString() ?? string.Empty,
            dto.Classes?.ToArray(),
            null,
            null,
            dto.Error
        );

    private static Backup ToModel(Rest.Dto.BackupCreateStatusResponse dto, BackupBackend backend) =>
        new(
            dto.Id ?? string.Empty,
            backend,
            dto.Status?.ToString() ?? string.Empty,
            null,
            dto.StartedAt,
            dto.CompletedAt,
            dto.Error
        );

    private static Backup ToModel(Rest.Dto.BackupRestoreResponse dto) =>
        new(
            dto.Id ?? string.Empty,
            ParseBackend(dto.Backend, null, dto.Path),
            dto.Status?.ToString() ?? string.Empty,
            dto.Classes?.ToArray(),
            null,
            null,
            dto.Error
        );

    private static Backup ToModel(
        Rest.Dto.BackupRestoreStatusResponse dto,
        BackupBackend backend
    ) =>
        new(
            dto.Id ?? string.Empty,
            backend,
            dto.Status?.ToString() ?? string.Empty,
            null,
            null,
            null,
            dto.Error
        );

    private static Backup ToModelListItem(Rest.Dto.Anonymous3 dto) =>
        new(
            dto.Id ?? string.Empty,
            BackupBackend.Empty(), // List endpoint doesn't provide backend info
            dto.Status?.ToString() ?? string.Empty,
            dto.Classes?.ToArray(),
            dto.StartedAt,
            dto.CompletedAt,
            null
        );
}

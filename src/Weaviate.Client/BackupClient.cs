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
    /// Bucket and path should be supplied via <see cref="Models.BackupCreateRequest.Config"/> when needed.
    /// </summary>
    /// <example>
    /// // Start backup and check status later
    /// var operation = await client.Backups.Create(BackupStorage.Filesystem, request);
    ///
    /// // Or await it directly for completion
    /// var result = await client.Backups.Create(BackupStorage.Filesystem, request);
    /// </example>
    public async Task<BackupCreateOperation> Create(
        BackupStorage backend,
        Models.BackupCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var restRequest = BuildBackupCreateRequest(request);
        var response = await _client.RestClient.BackupCreate(backend, restRequest);
        var model = ToModel(response);

        var cfgBucket = request.Config?.Bucket;
        var cfgPath = request.Config?.Path;

        return new BackupCreateOperation(
            model,
            async () => await GetStatus(backend, model.Id, cfgBucket, cfgPath),
            async () => await Cancel(backend, model.Id, cfgBucket, cfgPath)
        );
    }

    /// <summary>
    /// Create a backup and wait synchronously for completion.
    /// This method blocks until the backup operation finishes.
    /// </summary>
    public async Task<Backup> CreateSync(
        BackupStorage backend,
        Models.BackupCreateRequest request,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var operation = await Create(backend, request, cancellationToken);
        return await operation.WaitForCompletion(timeout, cancellationToken);
    }

    private Rest.Dto.BackupCreateRequest BuildBackupCreateRequest(
        Models.BackupCreateRequest request
    )
    {
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
                    Bucket = request.Config.Bucket,
                    Path = request.Config.Path,
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
    /// List existing backups for a backend
    /// </summary>
    public async Task<IEnumerable<Backup>> List(BackupStorage backend)
    {
        var list = await _client.RestClient.BackupList(backend);
        return list.Select(ToModelListItem) ?? Array.Empty<Backup>();
    }

    /// <summary>
    /// Get creation status for a backup
    /// </summary>
    public async Task<Backup> GetStatus(
        BackupStorage backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var status = await _client.RestClient.BackupStatus(backend, id, bucket, path);
        return ToModel(status);
    }

    /// <summary>
    /// Cancel a running backup
    /// </summary>
    public Task Cancel(
        BackupStorage backend,
        string id,
        string? bucket = null,
        string? path = null
    ) => _client.RestClient.BackupCancel(backend, id, bucket, path);

    /// <summary>
    /// Start restoring a backup asynchronously.
    /// Returns a BackupRestoreOperation that can be used to track status or wait for completion.
    /// Bucket and path should be supplied via <see cref="Models.BackupRestoreRequest.Config"/> when needed.
    /// </summary>
    /// <example>
    /// // Start restore and check status later
    /// var operation = await client.Backups.Restore(BackupStorage.Filesystem, backupId, request);
    ///
    /// // Or await it directly for completion
    /// var result = await client.Backups.Restore(BackupStorage.Filesystem, backupId, request);
    /// </example>
    public async Task<BackupRestoreOperation> Restore(
        BackupStorage backend,
        string id,
        Models.BackupRestoreRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var restRequest = BuildBackupRestoreRequest(request);
        var response = await _client.RestClient.BackupRestore(backend, id, restRequest);
        var model = ToModel(response);

        var cfgBucket = request.Config?.Bucket;
        var cfgPath = request.Config?.Path;

        return new BackupRestoreOperation(
            model,
            async () => await GetRestoreStatus(backend, model.Id, cfgBucket, cfgPath),
            async () => await Cancel(backend, model.Id, cfgBucket, cfgPath)
        );
    }

    /// <summary>
    /// Restore a backup and wait synchronously for completion.
    /// This method blocks until the restore operation finishes.
    /// </summary>
    public async Task<Backup> RestoreSync(
        BackupStorage backend,
        string id,
        Models.BackupRestoreRequest request,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var operation = await Restore(backend, id, request, cancellationToken);
        return await operation.WaitForCompletion(timeout, cancellationToken);
    }

    private Rest.Dto.BackupRestoreRequest BuildBackupRestoreRequest(
        Models.BackupRestoreRequest request
    )
    {
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
                    Bucket = request.Config.Bucket,
                    Path = request.Config.Path,
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
    /// Get restore status
    /// </summary>
    public async Task<Backup> GetRestoreStatus(
        BackupStorage backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var status = await _client.RestClient.BackupRestoreStatus(backend, id, bucket, path);
        return ToModel(status);
    }

    private static Backup ToModel(Rest.Dto.BackupCreateResponse dto) =>
        new(
            dto.Id ?? string.Empty,
            dto.Backend ?? string.Empty,
            dto.Bucket,
            dto.Path,
            dto.Status?.ToString() ?? string.Empty,
            dto.Classes?.ToArray(),
            null,
            null,
            dto.Error
        );

    private static Backup ToModel(Rest.Dto.BackupCreateStatusResponse dto) =>
        new(
            dto.Id ?? string.Empty,
            dto.Backend ?? string.Empty,
            null,
            dto.Path,
            dto.Status?.ToString() ?? string.Empty,
            null,
            dto.StartedAt,
            dto.CompletedAt,
            dto.Error
        );

    private static Backup ToModel(Rest.Dto.BackupRestoreResponse dto) =>
        new(
            dto.Id ?? string.Empty,
            dto.Backend ?? string.Empty,
            null,
            dto.Path,
            dto.Status?.ToString() ?? string.Empty,
            dto.Classes?.ToArray(),
            null,
            null,
            dto.Error
        );

    private static Backup ToModel(Rest.Dto.BackupRestoreStatusResponse dto) =>
        new(
            dto.Id ?? string.Empty,
            dto.Backend ?? string.Empty,
            null,
            dto.Path,
            dto.Status?.ToString() ?? string.Empty,
            null,
            null,
            null,
            dto.Error
        );

    private static Backup ToModelListItem(Rest.Dto.Anonymous3 dto) =>
        new(
            dto.Id ?? string.Empty,
            string.Empty,
            null,
            null,
            dto.Status?.ToString() ?? string.Empty,
            dto.Classes?.ToArray(),
            dto.StartedAt,
            dto.CompletedAt,
            null
        );
}

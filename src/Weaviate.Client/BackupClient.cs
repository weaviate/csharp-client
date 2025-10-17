using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class WeaviateClient
{
    public BackupClient Backups => new(this);
}

public class BackupClient
{
    private readonly WeaviateClient _client;

    internal BackupClient(WeaviateClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Start creating a backup
    /// </summary>
    public async Task<Backup> Create(
        BackupStorage backend,
        Models.BackupCreateRequest request,
        bool waitForCompletion = false,
        TimeSpan? pollInterval = null,
        TimeSpan? timeout = null,
        string? bucket = null,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var dto = new Models.BackupCreateRequest(
            request.Id,
            request.Include,
            request.Exclude,
            request.Config
        );
        var restRequest = new Rest.Dto.BackupCreateRequest
        {
            Id = dto.Id,
            Include = dto.Include?.ToList(),
            Exclude = dto.Exclude?.ToList(),
            Config = dto.Config is null
                ? null
                : new Rest.Dto.BackupConfig
                {
                    Endpoint = dto.Config.Endpoint,
                    Bucket = dto.Config.Bucket,
                    Path = dto.Config.Path,
                    CPUPercentage = dto.Config.CPUPercentage,
                    ChunkSize = dto.Config.ChunkSize,
                    CompressionLevel = dto.Config.CompressionLevel switch
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

        var response = await _client.RestClient.BackupCreate(backend, restRequest);
        var model = ToModel(response);
        if (!waitForCompletion)
        {
            return model;
        }
        return await WaitForCreateCompletion(
            backend,
            model.Id,
            pollInterval,
            timeout,
            bucket,
            path,
            cancellationToken
        );
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
    /// Start restoring a backup
    /// </summary>
    public async Task<Backup> Restore(
        BackupStorage backend,
        string id,
        Models.BackupRestoreRequest request,
        bool waitForCompletion = false,
        TimeSpan? pollInterval = null,
        TimeSpan? timeout = null,
        string? bucket = null,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var restRequest = new Rest.Dto.BackupRestoreRequest
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
        var response = await _client.RestClient.BackupRestore(backend, id, restRequest);
        if (!waitForCompletion)
        {
            var model = ToModel(response);
            return model;
        }
        return await WaitForRestoreCompletion(
            backend,
            id,
            pollInterval,
            timeout,
            bucket,
            path,
            cancellationToken
        );
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

    private static readonly HashSet<BackupStatus> TerminalStatuses = new()
    {
        BackupStatus.Success,
        BackupStatus.Failed,
        BackupStatus.Canceled,
    };

    private async Task<Backup> WaitForCreateCompletion(
        BackupStorage backend,
        string id,
        TimeSpan? pollInterval,
        TimeSpan? timeout,
        string? bucket,
        string? path,
        CancellationToken ct
    )
    {
        pollInterval ??= TimeSpan.FromMilliseconds(250);
        timeout ??= TimeSpan.FromMinutes(10);
        var start = DateTime.UtcNow;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var status = await _client.RestClient.BackupStatus(backend, id, bucket, path);
            var model = ToModel(status);
            if (TerminalStatuses.Contains(model.StatusRaw.ToBackupStatus()))
            {
                return model;
            }
            if (DateTime.UtcNow - start > timeout)
            {
                throw new TimeoutException(
                    $"Backup create did not finish within {timeout}. Last status={model.StatusRaw}"
                );
            }
            await Task.Delay(pollInterval.Value, ct);
        }
    }

    private async Task<Backup> WaitForRestoreCompletion(
        BackupStorage backend,
        string id,
        TimeSpan? pollInterval,
        TimeSpan? timeout,
        string? bucket,
        string? path,
        CancellationToken ct
    )
    {
        pollInterval ??= TimeSpan.FromMilliseconds(250);
        timeout ??= TimeSpan.FromMinutes(10);
        var start = DateTime.UtcNow;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var status = await _client.RestClient.BackupRestoreStatus(backend, id, bucket, path);
            var model = ToModel(status);
            if (TerminalStatuses.Contains(model.StatusRaw.ToBackupStatus()))
            {
                return model;
            }
            if (DateTime.UtcNow - start > timeout)
            {
                throw new TimeoutException(
                    $"Backup restore did not finish within {timeout}. Last status={model.StatusRaw}"
                );
            }
            await Task.Delay(pollInterval.Value, ct);
        }
    }
}

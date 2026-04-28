using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Adds export property to WeaviateClient
/// </summary>
public partial class WeaviateClient
{
    private ExportClient? _exports;

    /// <summary>
    /// A client for managing collection exports.
    /// </summary>
    public ExportClient Export => _exports ??= new(this);
}

/// <summary>
/// Provides export operations for Weaviate collections — creation, status tracking, and cancellation.
/// </summary>
public class ExportClient
{
    private readonly WeaviateClient _client;

    /// <summary>
    /// Static configuration used for all export operations.
    /// </summary>
    public static ExportClientConfig Config { get; set; } = ExportClientConfig.Default;

    internal ExportClient(WeaviateClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Start creating an export asynchronously.
    /// Returns an ExportOperation that can be used to track status or wait for completion.
    /// </summary>
    [RequiresWeaviateVersion(1, 37, 0)]
    public async Task<ExportOperation> Create(
        ExportCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureVersion<ExportClient>();

        var restRequest = BuildExportCreateRequest(request);
        var response = await _client.RestClient.ExportCreate(
            request.Backend.Provider,
            restRequest,
            cancellationToken
        );
        var model = ToModel(response);

        return new ExportOperation(
            model,
            async (ct) => await GetStatus(request.Backend, model.Id, ct),
            async (ct) => await Cancel(request.Backend, model.Id, ct)
        );
    }

    /// <summary>
    /// Create an export and wait synchronously for completion.
    /// </summary>
    [RequiresWeaviateVersion(1, 37, 0)]
    public async Task<Export> CreateSync(
        ExportCreateRequest request,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureVersion<ExportClient>();
        await using var operation = await Create(request, cancellationToken);
        return await operation.WaitForCompletion(timeout, cancellationToken);
    }

    /// <summary>
    /// Get status for an export
    /// </summary>
    [RequiresWeaviateVersion(1, 37, 0)]
    public async Task<Export> GetStatus(
        BackupBackend backend,
        string id,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureVersion<ExportClient>();

        var status = await _client.RestClient.ExportGetStatus(
            backend.Provider,
            id,
            cancellationToken
        );
        return ToModel(status, backend);
    }

    /// <summary>
    /// Cancel a running export. Returns <c>true</c> if the cancel request was accepted,
    /// <c>false</c> if the server responded 409 Conflict (export already in a terminal
    /// state and cannot be canceled). Throws <see cref="WeaviateNotFoundException"/>
    /// if the export id is unknown.
    /// </summary>
    [RequiresWeaviateVersion(1, 37, 0)]
    public async Task<bool> Cancel(
        BackupBackend backend,
        string id,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureVersion<ExportClient>();

        return await _client.RestClient.ExportCancel(backend.Provider, id, cancellationToken);
    }

    private static Rest.Dto.ExportCreateRequest BuildExportCreateRequest(
        ExportCreateRequest request
    )
    {
        return new Rest.Dto.ExportCreateRequest
        {
            Id = request.Id,
            File_format = request.FileFormat switch
            {
                ExportFileFormat.Parquet => Rest.Dto.ExportCreateRequestFile_format.Parquet,
                _ => Rest.Dto.ExportCreateRequestFile_format.Parquet,
            },
            Include = request.IncludeCollections?.ToList(),
            Exclude = request.ExcludeCollections?.ToList(),
        };
    }

    private static Export ToModel(Rest.Dto.ExportCreateResponse dto) =>
        new(
            dto.Id ?? string.Empty,
            ParseBackend(dto.Backend),
            dto.Path,
            dto.Status?.ToString() ?? string.Empty,
            dto.Classes?.ToArray(),
            dto.StartedAt,
            null,
            null
        );

    private static Export ToModel(Rest.Dto.ExportStatusResponse dto, BackupBackend backend) =>
        new(
            dto.Id ?? string.Empty,
            backend,
            dto.Path,
            dto.Status?.ToString() ?? string.Empty,
            dto.Classes?.ToArray(),
            dto.StartedAt,
            dto.CompletedAt,
            dto.Error
        )
        {
            ShardStatus = dto.ShardStatus?.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                    kvp.Value.ToDictionary(
                        inner => inner.Key,
                        inner => new ShardProgress(
                            inner.Value.Status?.ToString() ?? string.Empty,
                            (int)(inner.Value.ObjectsExported ?? 0),
                            inner.Value.Error,
                            inner.Value.SkipReason
                        )
                    )
            ),
            TookInMs = dto.TookInMs.HasValue ? (int)dto.TookInMs.Value : null,
        };

    private static BackupBackend ParseBackend(string? backendStr)
    {
        var provider = backendStr?.ToLowerInvariant() switch
        {
            "filesystem" => BackupStorageProvider.Filesystem,
            "s3" => BackupStorageProvider.S3,
            "gcs" => BackupStorageProvider.GCS,
            "azure" => BackupStorageProvider.Azure,
            _ => BackupStorageProvider.None,
        };
        return provider == BackupStorageProvider.None ? BackupBackend.Empty()
            : provider == BackupStorageProvider.Filesystem ? new FilesystemBackend()
            : new ObjectStorageBackend(provider);
    }
}

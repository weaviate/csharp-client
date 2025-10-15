using System.Net.Http.Json;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

public partial class WeaviateRestClient
{
    internal async Task<Dto.BackupCreateResponse> BackupCreate(
        string backend,
        Dto.BackupCreateRequest request
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Backups(backend),
            request,
            options: RestJsonSerializerOptions
        );
        await response.EnsureExpectedStatusCodeAsync([200], "create backup");
        return await response.Content.ReadFromJsonAsync<Dto.BackupCreateResponse>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();
    }

    internal async Task<Dto.BackupListResponse> BackupList(string backend)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Backups(backend));
        await response.EnsureExpectedStatusCodeAsync([200], "list backups");
        return await response.Content.ReadFromJsonAsync<Dto.BackupListResponse>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();
    }

    internal async Task<Dto.BackupCreateStatusResponse> BackupStatus(
        string backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.BackupStatus(backend, id, bucket, path)
        );
        await response.EnsureExpectedStatusCodeAsync([200], "backup status");
        return await response.Content.ReadFromJsonAsync<Dto.BackupCreateStatusResponse>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();
    }

    internal async Task BackupCancel(
        string backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.BackupStatus(backend, id, bucket, path)
        );
        await response.EnsureExpectedStatusCodeAsync([200, 204], "backup cancel");
    }

    internal async Task<Dto.BackupRestoreResponse> BackupRestore(
        string backend,
        string id,
        Dto.BackupRestoreRequest request
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.BackupRestore(backend, id),
            request,
            options: RestJsonSerializerOptions
        );
        await response.EnsureExpectedStatusCodeAsync([200], "backup restore");
        return await response.Content.ReadFromJsonAsync<Dto.BackupRestoreResponse>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();
    }

    internal async Task<Dto.BackupRestoreStatusResponse> BackupRestoreStatus(
        string backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.BackupRestoreStatus(backend, id, bucket, path)
        );
        await response.EnsureExpectedStatusCodeAsync([200], "backup restore status");
        return await response.Content.ReadFromJsonAsync<Dto.BackupRestoreStatusResponse>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();
    }
}

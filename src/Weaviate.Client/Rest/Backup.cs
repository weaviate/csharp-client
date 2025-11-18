using System.Net;
using System.Net.Http.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    internal async Task<Dto.BackupCreateResponse> BackupCreate(
        BackupStorageProvider backend,
        Dto.BackupCreateRequest request
    )
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                WeaviateEndpoints.Backups(backend.ToEnumMemberString()!),
                request,
                options: RestJsonSerializerOptions
            );
            await response.EnsureExpectedStatusCodeAsync([200], "create backup");
            return await response.Content.ReadFromJsonAsync<Dto.BackupCreateResponse>(
                    WeaviateRestClient.RestJsonSerializerOptions
                ) ?? throw new WeaviateRestClientException();
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.UnprocessableEntity
                && ex.Message.Contains("already in progress")
            )
        {
            throw new WeaviateBackupConflictException(ex);
        }
    }

    internal async Task<List<Dto.Anonymous3>> BackupList(BackupStorageProvider backend)
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.Backups(backend.ToEnumMemberString()!)
        );
        await response.EnsureExpectedStatusCodeAsync([200], "list backups");
        return await response.Content.ReadFromJsonAsync<List<Dto.Anonymous3>>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestClientException();
    }

    internal async Task<Dto.BackupCreateStatusResponse> BackupStatus(
        BackupStorageProvider backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.BackupStatus(backend.ToEnumMemberString()!, id, bucket, path)
        );
        await response.EnsureExpectedStatusCodeAsync([200], "backup status");
        return await response.Content.ReadFromJsonAsync<Dto.BackupCreateStatusResponse>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestClientException();
    }

    internal async Task BackupCancel(
        BackupStorageProvider backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.BackupStatus(backend.ToEnumMemberString()!, id, bucket, path)
        );
        await response.EnsureExpectedStatusCodeAsync([200, 204], "backup cancel");
    }

    internal async Task<Dto.BackupRestoreResponse> BackupRestore(
        BackupStorageProvider backend,
        string id,
        Dto.BackupRestoreRequest request
    )
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                WeaviateEndpoints.BackupRestore(backend.ToEnumMemberString()!, id),
                request,
                options: RestJsonSerializerOptions
            );
            await response.EnsureExpectedStatusCodeAsync([200], "backup restore");
            return await response.Content.ReadFromJsonAsync<Dto.BackupRestoreResponse>(
                    WeaviateRestClient.RestJsonSerializerOptions
                ) ?? throw new WeaviateRestClientException();
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.UnprocessableEntity
                && ex.Message.Contains("already in progress")
            )
        {
            throw new WeaviateBackupConflictException(ex);
        }
    }

    internal async Task<Dto.BackupRestoreStatusResponse> BackupRestoreStatus(
        BackupStorageProvider backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.BackupRestoreStatus(backend.ToEnumMemberString()!, id, bucket, path)
        );
        await response.EnsureExpectedStatusCodeAsync([200], "backup restore status");
        return await response.Content.ReadFromJsonAsync<Dto.BackupRestoreStatusResponse>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestClientException();
    }
}

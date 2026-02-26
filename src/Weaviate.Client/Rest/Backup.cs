using System.Net;
using System.Net.Http.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client class
/// </summary>
internal partial class WeaviateRestClient
{
    /// <summary>
    /// Backups the create using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto backup create response</returns>
    internal async Task<Dto.BackupCreateResponse> BackupCreate(
        BackupStorageProvider backend,
        Dto.BackupCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Backups(backend.ToEnumMemberString()!),
            request,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "create backup",
            ResourceType.Backup
        );
        return await response.DecodeAsync<Dto.BackupCreateResponse>(cancellationToken);
    }

    /// <summary>
    /// Backups the list using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing a list of dto anonymous 3</returns>
    internal async Task<List<Dto.Anonymous3>> BackupList(
        BackupStorageProvider backend,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.Backups(backend.ToEnumMemberString()!),
            cancellationToken
        );
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "list backups",
            ResourceType.Backup
        );
        return await response.DecodeAsync<List<Dto.Anonymous3>>(cancellationToken);
    }

    /// <summary>
    /// Backups the status using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <param name="bucket">The bucket</param>
    /// <param name="path">The path</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto backup create status response</returns>
    internal async Task<Dto.BackupCreateStatusResponse> BackupStatus(
        BackupStorageProvider backend,
        string id,
        string? bucket = null,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.BackupStatus(backend.ToEnumMemberString()!, id, bucket, path),
            cancellationToken
        );
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "backup status",
            ResourceType.Backup
        );
        return await response.DecodeAsync<Dto.BackupCreateStatusResponse>(cancellationToken);
    }

    /// <summary>
    /// Backups the cancel using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <param name="bucket">The bucket</param>
    /// <param name="path">The path</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task BackupCancel(
        BackupStorageProvider backend,
        string id,
        string? bucket = null,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.BackupStatus(backend.ToEnumMemberString()!, id, bucket, path),
            cancellationToken
        );
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                HttpStatusCode.NoContent,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "backup cancel",
            ResourceType.Backup
        );
    }

    /// <summary>
    /// Backups the restore using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto backup restore response</returns>
    internal async Task<Dto.BackupRestoreResponse> BackupRestore(
        BackupStorageProvider backend,
        string id,
        Dto.BackupRestoreRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.BackupRestore(backend.ToEnumMemberString()!, id),
            request,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "backup restore",
            ResourceType.Backup
        );
        return await response.DecodeAsync<Dto.BackupRestoreResponse>(cancellationToken);
    }

    /// <summary>
    /// Cancels a running restore operation by issuing DELETE to /backups/{backend}/{id}/restore.
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <param name="bucket">The bucket</param>
    /// <param name="path">The path</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task BackupRestoreCancel(
        BackupStorageProvider backend,
        string id,
        string? bucket = null,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.BackupRestoreStatus(backend.ToEnumMemberString()!, id, bucket, path),
            cancellationToken
        );
        await response.ManageStatusCode(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "backup restore cancel",
            ResourceType.Backup
        );
    }

    /// <summary>
    /// Backups the restore status using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <param name="bucket">The bucket</param>
    /// <param name="path">The path</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto backup restore status response</returns>
    internal async Task<Dto.BackupRestoreStatusResponse> BackupRestoreStatus(
        BackupStorageProvider backend,
        string id,
        string? bucket = null,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.BackupRestoreStatus(backend.ToEnumMemberString()!, id, bucket, path),
            cancellationToken
        );
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "backup restore status",
            ResourceType.Backup
        );
        return await response.DecodeAsync<Dto.BackupRestoreStatusResponse>(cancellationToken);
    }
}

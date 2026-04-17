using System.Net;
using System.Net.Http.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    internal async Task<Dto.ExportCreateResponse> ExportCreate(
        BackupStorageProvider backend,
        Dto.ExportCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Exports(backend.ToEnumMemberString()!),
            request,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        await response.ManageStatusCode([HttpStatusCode.OK], "create export", ResourceType.Export);
        return await response.DecodeAsync<Dto.ExportCreateResponse>(cancellationToken);
    }

    internal async Task<Dto.ExportStatusResponse> ExportGetStatus(
        BackupStorageProvider backend,
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.ExportStatus(backend.ToEnumMemberString()!, id),
            cancellationToken
        );
        await response.ManageStatusCode([HttpStatusCode.OK], "export status", ResourceType.Export);
        return await response.DecodeAsync<Dto.ExportStatusResponse>(cancellationToken);
    }

    internal async Task ExportCancel(
        BackupStorageProvider backend,
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.ExportStatus(backend.ToEnumMemberString()!, id),
            cancellationToken
        );
        await response.ManageStatusCode(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "export cancel",
            ResourceType.Export
        );
    }
}

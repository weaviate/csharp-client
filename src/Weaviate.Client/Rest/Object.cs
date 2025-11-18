using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    internal async Task<Dto.Object> ObjectInsert(
        Dto.Object data,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Objects(),
            data,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        await response.EnsureExpectedStatusCodeAsync([200], "insert object");

        return await response.Content.ReadFromJsonAsync<Dto.Object>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            ) ?? throw new WeaviateRestClientException();
    }

    internal async Task<Dto.Object> ObjectReplace(
        string collectionName,
        Dto.Object data,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(data.Id, nameof(data.Id));

        var response = await _httpClient.PutAsJsonAsync(
            WeaviateEndpoints.CollectionObject(collectionName, data.Id!.Value),
            data,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        await response.EnsureExpectedStatusCodeAsync([200], "replace object");

        return await response.Content.ReadFromJsonAsync<Dto.Object>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            ) ?? throw new WeaviateRestClientException();
    }

    internal async Task DeleteObject(
        string collectionName,
        Guid id,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        var url = WeaviateEndpoints.CollectionObject(collectionName, id, tenant);
        var response = await _httpClient.DeleteAsync(url, cancellationToken);

        await response.EnsureExpectedStatusCodeAsync([204, 404], "delete object");
    }
}

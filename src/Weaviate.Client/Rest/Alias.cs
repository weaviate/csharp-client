using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

public partial class WeaviateRestClient
{
    internal async Task<IEnumerable<Dto.Alias>> CollectionAliasesGet(string? collectionName)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Aliases(collectionName));

        await response.EnsureExpectedStatusCodeAsync([200], "get aliases");

        var aliasesResponse =
            await response.Content.ReadFromJsonAsync<Dto.AliasResponse>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();

        return aliasesResponse.Aliases ?? Array.Empty<Dto.Alias>();
    }

    internal async Task<Dto.Alias> CollectionAliasesPost(Dto.Alias data)
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Aliases(),
            data,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "create alias");

        return await response.Content.ReadFromJsonAsync<Dto.Alias>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();
    }

    internal async Task<Dto.Alias?> AliasGet(string aliasName)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Alias(aliasName));

        return await response.EnsureExpectedStatusCodeAsync([200, 404], "get alias") switch
        {
            HttpStatusCode.NotFound => null,
            _ => await response.Content.ReadFromJsonAsync<Dto.Alias>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException(),
        };
    }

    internal async Task<Dto.Alias> AliasPut(string alias, string targetCollection)
    {
        var response = await _httpClient.PutAsJsonAsync(
            WeaviateEndpoints.Alias(alias),
            new { @class = targetCollection },
            options: RestJsonSerializerOptions
        );

        var statusCode = await response.EnsureExpectedStatusCodeAsync([200, 404], "update alias");

        return statusCode switch
        {
            HttpStatusCode.NotFound => throw new WeaviateRestException(
                "Alias not found",
                statusCode
            ),
            _ => await response.Content.ReadFromJsonAsync<Dto.Alias>(
                WeaviateRestClient.RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException(),
        };
    }

    internal async Task<bool> AliasDelete(string aliasName)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.Alias(aliasName));

        var statusCode = await response.EnsureExpectedStatusCodeAsync([204], "delete alias");

        return statusCode == HttpStatusCode.NoContent;
    }
}

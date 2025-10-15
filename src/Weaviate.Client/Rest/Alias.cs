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
            await response.Content.ReadFromJsonAsync<Dto.AliasResponse>()
            ?? throw new WeaviateRestException();

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

        return await response.Content.ReadFromJsonAsync<Dto.Alias>()
            ?? throw new WeaviateRestException();
    }

    internal async Task<Dto.Alias> AliasGet(string aliasName)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Alias(aliasName));

        await response.EnsureExpectedStatusCodeAsync([200], "get alias");

        return await response.Content.ReadFromJsonAsync<Dto.Alias>()
            ?? throw new WeaviateRestException();
    }

    internal async Task<Dto.Alias> AliasPut(string alias, string targetCollection)
    {
        var response = await _httpClient.PutAsJsonAsync(
            WeaviateEndpoints.Alias(alias),
            new { @class = targetCollection }
        );

        await response.EnsureExpectedStatusCodeAsync([200], "update alias");

        return await response.Content.ReadFromJsonAsync<Dto.Alias>()
            ?? throw new WeaviateRestException();
    }

    internal async Task AliasDelete(string aliasName)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.Alias(aliasName));

        await response.EnsureExpectedStatusCodeAsync([200, 204], "delete alias");
    }
}

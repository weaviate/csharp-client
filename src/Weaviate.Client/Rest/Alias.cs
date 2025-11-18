using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    internal async Task<IEnumerable<Dto.Alias>> CollectionAliasesGet(
        string? collectionName,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.Aliases(collectionName),
            cancellationToken
        );

        await response.EnsureExpectedStatusCodeAsync([200], "get aliases");

        var aliasesResponse =
            await response.Content.ReadFromJsonAsync<Dto.AliasResponse>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            ) ?? throw new WeaviateRestClientException();

        return aliasesResponse.Aliases ?? Array.Empty<Dto.Alias>();
    }

    internal async Task<Dto.Alias> CollectionAliasesPost(
        Dto.Alias data,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Aliases(),
            data,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        await response.EnsureExpectedStatusCodeAsync([200], "create alias");

        return await response.Content.ReadFromJsonAsync<Dto.Alias>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            ) ?? throw new WeaviateRestClientException();
    }

    internal async Task<Dto.Alias?> AliasGet(
        string aliasName,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.Alias(aliasName),
            cancellationToken
        );

        return await response.EnsureExpectedStatusCodeAsync([200, 404], "get alias") switch
        {
            HttpStatusCode.NotFound => null,
            _ => await response.Content.ReadFromJsonAsync<Dto.Alias>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            ) ?? throw new WeaviateRestClientException(),
        };
    }

    internal async Task<Dto.Alias> AliasPut(
        string alias,
        string targetCollection,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PutAsJsonAsync(
            WeaviateEndpoints.Alias(alias),
            new { @class = targetCollection },
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        var statusCode = await response.EnsureExpectedStatusCodeAsync([200, 404], "update alias");

        return statusCode switch
        {
            HttpStatusCode.NotFound => throw new WeaviateNotFoundException(
                new WeaviateRestServerException(statusCode),
                resourceType: ResourceType.Alias
            ),
            _ => await response.Content.ReadFromJsonAsync<Dto.Alias>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            ) ?? throw new WeaviateRestClientException(),
        };
    }

    internal async Task<bool> AliasDelete(
        string aliasName,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.Alias(aliasName),
            cancellationToken
        );

        var statusCode = await response.EnsureExpectedStatusCodeAsync([204], "delete alias");

        return statusCode == HttpStatusCode.NoContent;
    }
}

using System.Net;
using System.Net.Http.Json;

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

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest, // 400
                // HttpStatusCode.Unauthorized, // 401
                // HttpStatusCode.Forbidden, // 403
                // HttpStatusCode.NotFound, // 404
                // HttpStatusCode.Conflict, // 409
                // HttpStatusCode.InternalServerError, // 500
            ],
            "get aliases",
            ResourceType.Alias
        );

        var aliasesResponse = await response.DecodeAsync<Dto.AliasResponse>(cancellationToken);

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

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest, // 400
                // HttpStatusCode.Unauthorized, // 401
                // HttpStatusCode.Forbidden, // 403
                // HttpStatusCode.NotFound, // 404
                // HttpStatusCode.Conflict, // 409
                // HttpStatusCode.InternalServerError, // 500
            ],
            "create alias",
            ResourceType.Alias
        );

        return await response.DecodeAsync<Dto.Alias>(cancellationToken);
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

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                HttpStatusCode.NotFound, // 404
                // HttpStatusCode.BadRequest, // 400
                // HttpStatusCode.Unauthorized, // 401
                // HttpStatusCode.Forbidden, // 403
                // HttpStatusCode.Conflict, // 409
                // HttpStatusCode.InternalServerError, // 500
            ],
            "get alias",
            ResourceType.Alias
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await response.DecodeAsync<Dto.Alias>(cancellationToken);
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

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest, // 400
                // HttpStatusCode.Unauthorized, // 401
                // HttpStatusCode.Forbidden, // 403
                // HttpStatusCode.NotFound, // 404
                // HttpStatusCode.Conflict, // 409
                // HttpStatusCode.InternalServerError, // 500
            ],
            "update alias",
            ResourceType.Alias
        );

        return await response.DecodeAsync<Dto.Alias>(cancellationToken);
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

        await response.ManageStatusCode(
            [
                HttpStatusCode.NoContent,
                HttpStatusCode.NotFound, // 404
                // HttpStatusCode.BadRequest, // 400
                // HttpStatusCode.Unauthorized, // 401
                // HttpStatusCode.Forbidden, // 403
                // HttpStatusCode.Conflict, // 409
                // HttpStatusCode.InternalServerError, // 500
            ],
            "delete alias",
            ResourceType.Alias
        );

        return response.StatusCode switch
        {
            HttpStatusCode.NoContent => true,
            _ => false,
        };
    }
}

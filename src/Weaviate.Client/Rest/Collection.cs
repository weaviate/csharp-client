using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    internal async Task<Dto.Schema?> CollectionList(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.Collection(),
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
            "collection list",
            ResourceType.Collection
        );

        var contents = await response.Content.ReadFromJsonAsync<Dto.Schema>(
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        return contents;
    }

    internal async Task<Dto.Class?> CollectionGet(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.Collection(name),
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
            "collection get",
            ResourceType.Collection
        );

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        var contents = await response.Content.ReadFromJsonAsync<Dto.Class>(
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        if (contents is null)
        {
            throw new WeaviateRestClientException();
        }

        return contents;
    }

    internal async Task CollectionDelete(string name, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.Collection(name),
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
            "collection delete",
            ResourceType.Collection
        );
    }

    internal async Task<Dto.Class> CollectionCreate(
        Dto.Class collection,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Collection(),
            collection,
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
            "collection create",
            ResourceType.Collection
        );

        var contents = await response.Content.ReadFromJsonAsync<Dto.Class>(
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        if (contents is null)
        {
            throw new WeaviateRestClientException();
        }

        return contents;
    }

    internal async Task<Dto.Class> CollectionUpdate(
        string collectionName,
        Dto.Class collection,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PutAsJsonAsync(
            WeaviateEndpoints.Collection(collectionName),
            collection,
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
            "collection update",
            ResourceType.Collection
        );

        var contents =
            await response.Content.ReadFromJsonAsync<Dto.Class>(
                options: RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            ) ?? throw new WeaviateRestServerException();

        return contents;
    }

    internal async Task CollectionAddProperty(
        string collectionName,
        Property property,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.CollectionProperties(collectionName);

        var response = await _httpClient.PostAsJsonAsync(
            path,
            property,
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
            "collection property add",
            ResourceType.Collection
        );
    }

    internal async Task<bool> CollectionExists(
        object collectionName,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.Collection();

        var response = await _httpClient.GetAsync(path, cancellationToken);

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
            "collection exists",
            ResourceType.Collection
        );

        var schema = await response.Content.ReadFromJsonAsync<Schema>(
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        return schema?.Classes?.Any(c => c.Class1 is not null && c.Class1!.Equals(collectionName))
            ?? false;
    }
}

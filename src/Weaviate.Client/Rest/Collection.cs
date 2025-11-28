using System.Net;
using System.Net.Http.Json;
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

        return await response.DecodeAsync<Dto.Schema>(cancellationToken);
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

        return await response.DecodeAsync<Dto.Class>(cancellationToken);
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

        return await response.DecodeAsync<Dto.Class>(cancellationToken);
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

        return await response.DecodeAsync<Dto.Class>(cancellationToken);
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

        var schema = await response.DecodeAsync<Schema>(cancellationToken);

        return schema?.Classes?.Any(c => c.Class1 is not null && c.Class1!.Equals(collectionName))
            ?? false;
    }

    internal async Task<IList<Dto.ShardStatusGetResponse>> CollectionGetShards(
        string collectionName,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.CollectionShards(collectionName),
            cancellationToken
        );

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest, // 400
                // HttpStatusCode.Unauthorized, // 401
                // HttpStatusCode.Forbidden, // 403
                // HttpStatusCode.NotFound, // 404
                // HttpStatusCode.InternalServerError, // 500
            ],
            "collection get shards",
            ResourceType.Collection
        );

        return await response.DecodeAsync<IList<Dto.ShardStatusGetResponse>>(cancellationToken)
            ?? new List<Dto.ShardStatusGetResponse>();
    }

    internal async Task<Dto.ShardStatusGetResponse?> CollectionGetShard(
        string collectionName,
        string shardName,
        CancellationToken cancellationToken = default
    )
    {
        // Note: The API doesn't support GET for a single shard.
        // We must GET all shards and filter by name.
        var allShards = await CollectionGetShards(collectionName, cancellationToken);
        return allShards.FirstOrDefault(s => s.Name == shardName);
    }

    internal async Task<Dto.ShardStatusGetResponse> CollectionUpdateShard(
        string collectionName,
        string shardName,
        Dto.ShardStatus status,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PutAsJsonAsync(
            WeaviateEndpoints.CollectionShard(collectionName, shardName),
            status,
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
                // HttpStatusCode.UnprocessableEntity, // 422
                // HttpStatusCode.InternalServerError, // 500
            ],
            "collection update shard",
            ResourceType.Collection
        );

        // The PUT response only returns {"status": "..."} without name or other fields.
        // We need to fetch the full shard info by getting all shards and filtering.
        return await CollectionGetShard(collectionName, shardName, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Failed to retrieve shard '{shardName}' after update"
            );
    }
}

using System.Net;
using System.Net.Http.Json;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client class
/// </summary>
internal partial class WeaviateRestClient
{
    /// <summary>
    /// Collections the list using the specified cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto schema</returns>
    internal async Task<Schema?> CollectionList(CancellationToken cancellationToken = default)
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

        return await response.DecodeAsync<Schema>(cancellationToken);
    }

    /// <summary>
    /// Collections the get using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto class</returns>
    internal async Task<Class?> CollectionGet(
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

        return await response.DecodeAsync<Class>(cancellationToken);
    }

    /// <summary>
    /// Collections the delete using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="cancellationToken">The cancellation token</param>
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

    /// <summary>
    /// Collections the create using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto class</returns>
    internal async Task<Class> CollectionCreate(
        Class collection,
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

        return await response.DecodeAsync<Class>(cancellationToken);
    }

    /// <summary>
    /// Collections the create raw using the specified json
    /// </summary>
    /// <param name="json">The json</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto class</returns>
    internal async Task<Class> CollectionCreateRaw(
        string json,
        CancellationToken cancellationToken = default
    )
    {
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            WeaviateEndpoints.Collection(),
            content,
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
            "collection create (raw JSON)",
            ResourceType.Collection
        );

        return await response.DecodeAsync<Class>(cancellationToken);
    }

    /// <summary>
    /// Collections the update using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="collection">The collection</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto class</returns>
    internal async Task<Class> CollectionUpdate(
        string collectionName,
        Class collection,
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

        return await response.DecodeAsync<Class>(cancellationToken);
    }

    /// <summary>
    /// Collections the add property using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="property">The property</param>
    /// <param name="cancellationToken">The cancellation token</param>
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

    /// <summary>
    /// Drops a specific inverted index from a collection property.
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="indexName">The index to drop</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task CollectionDeletePropertyIndex(
        string collectionName,
        string propertyName,
        Dto.IndexName indexName,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.CollectionPropertyIndex(
            collectionName,
            propertyName,
            indexName.ToEnumMemberString()
        );

        var response = await _httpClient.DeleteAsync(path, cancellationToken);

        await response.ManageStatusCode(
            [HttpStatusCode.OK],
            "collection property index delete",
            ResourceType.Collection
        );
    }

    /// <summary>
    /// Deletes the vector index for a named vector in a collection.
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="vectorIndexName">The vector index name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task CollectionDeleteVectorIndex(
        string collectionName,
        string vectorIndexName,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.CollectionVectorIndex(
            collectionName,
            vectorIndexName
        );

        var response = await _httpClient.DeleteAsync(path, cancellationToken);

        await response.ManageStatusCode(
            [HttpStatusCode.OK],
            "collection vector index delete",
            ResourceType.Collection
        );
    }

    /// <summary>
    /// Collections the exists using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the bool</returns>
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

    /// <summary>
    /// Collections the get shards using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing a list of dto shard status get response</returns>
    internal async Task<IList<ShardStatusGetResponse>> CollectionGetShards(
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

        return await response.DecodeAsync<IList<ShardStatusGetResponse>>(cancellationToken)
            ?? new List<ShardStatusGetResponse>();
    }

    /// <summary>
    /// Collections the get shard using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="shardName">The shard name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto shard status get response</returns>
    internal async Task<ShardStatusGetResponse?> CollectionGetShard(
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

    /// <summary>
    /// Collections the update shard using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="shardName">The shard name</param>
    /// <param name="status">The status</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="InvalidOperationException">Failed to retrieve shard '{shardName}' after update</exception>
    /// <returns>A task containing the dto shard status get response</returns>
    internal async Task<ShardStatusGetResponse> CollectionUpdateShard(
        string collectionName,
        string shardName,
        ShardStatus status,
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

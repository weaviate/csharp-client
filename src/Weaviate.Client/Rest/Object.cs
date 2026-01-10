using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client class
/// </summary>
internal partial class WeaviateRestClient
{
    /// <summary>
    /// Objects the insert using the specified data
    /// </summary>
    /// <param name="data">The data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto object</returns>
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
            "insert object",
            ResourceType.Object
        );

        return await response.DecodeAsync<Dto.Object>(cancellationToken);
    }

    /// <summary>
    /// Objects the update using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="data">The data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto object</returns>
    internal async Task<Dto.Object> ObjectUpdate(
        string collectionName,
        Dto.Object data,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(data.Id, nameof(data.Id));

        var response = await _httpClient.PatchAsJsonAsync(
            WeaviateEndpoints.CollectionObject(collectionName, data.Id!.Value),
            data,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
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
            "update object",
            ResourceType.Object
        );

        // Only try to deserialize if there's content
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return data; // Return the input data or null, depending on your needs
        }

        return await response.DecodeAsync<Dto.Object>(cancellationToken);
    }

    /// <summary>
    /// Objects the replace using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="data">The data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto object</returns>
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
            "replace object",
            ResourceType.Object
        );

        return await response.DecodeAsync<Dto.Object>(cancellationToken);
    }

    /// <summary>
    /// Deletes the object using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="id">The id</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task DeleteObject(
        string collectionName,
        Guid id,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        var url = WeaviateEndpoints.CollectionObject(collectionName, id, tenant);
        var response = await _httpClient.DeleteAsync(url, cancellationToken);

        await response.ManageStatusCode(
            [
                HttpStatusCode.NoContent,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "delete object",
            ResourceType.Object
        );
    }
}

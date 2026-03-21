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
    /// References the add using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="from">The from</param>
    /// <param name="fromProperty">The from property</param>
    /// <param name="to">The to</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task ReferenceAdd(
        string collectionName,
        Guid from,
        string fromProperty,
        Guid to,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.Reference(collectionName, from, fromProperty, tenant);

        var beacons = Internal.ObjectHelper.MakeBeacons([to]);
        var reference = beacons.First();

        var response = await _httpClient.PostAsJsonAsync(
            path,
            reference,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.InternalServerError,
            ],
            "reference add",
            ResourceType.Reference
        );
    }

    /// <summary>
    /// References the replace using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="from">The from</param>
    /// <param name="fromProperty">The from property</param>
    /// <param name="to">The to</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task ReferenceReplace(
        string collectionName,
        Guid from,
        string fromProperty,
        Guid[] to,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.Reference(collectionName, from, fromProperty, tenant);

        var beacons = Internal.ObjectHelper.MakeBeacons(to);
        var reference = beacons;

        var response = await _httpClient.PutAsJsonAsync(
            path,
            reference,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.InternalServerError,
            ],
            "reference replace",
            ResourceType.Reference
        );
    }

    /// <summary>
    /// References the delete using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="from">The from</param>
    /// <param name="fromProperty">The from property</param>
    /// <param name="to">The to</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task ReferenceDelete(
        string collectionName,
        Guid from,
        string fromProperty,
        Guid to,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.Reference(collectionName, from, fromProperty, tenant);

        var beacons = Internal.ObjectHelper.MakeBeacons([to]);
        var reference = beacons.First();

        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        request.Content = JsonContent.Create(
            reference,
            mediaType: null,
            options: RestJsonSerializerOptions
        );

        var response = await _httpClient.SendAsync(request, cancellationToken);

        await response.ManageStatusCode(
            [
                HttpStatusCode.NoContent,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.InternalServerError,
            ],
            "reference delete",
            ResourceType.Reference
        );
    }

    /// <summary>
    /// References the add many using the specified references
    /// </summary>
    /// <param name="collectionName">The collection name of the source objects (fallback when DataReference.FromCollection is not set)</param>
    /// <param name="references">The references</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="consistencyLevel">The consistency level</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the batch reference response array</returns>
    internal async Task<BatchReferenceResponse[]> ReferenceAddMany(
        string collectionName,
        Models.DataReference[] references,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        CancellationToken cancellationToken = default
    )
    {
        var batchRefs = references.SelectMany(r =>
        {
            var effectiveCollection = r.FromCollection ?? collectionName;
            var sourceBeacon =
                r.Beacon ?? $"weaviate://localhost/{effectiveCollection}/{r.From}/{r.FromProperty}";
            return r.To.Select(toUuid => new BatchReference
            {
                From = new Uri(sourceBeacon),
                To = new Uri($"weaviate://localhost/{toUuid}"),
                Tenant = tenant ?? default!,
            });
        });

        var path = WeaviateEndpoints.ReferencesAdd(consistencyLevel);

        var response = await _httpClient.PostAsJsonAsync(
            path,
            batchRefs,
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
            "reference add many",
            ResourceType.Reference
        );

        return await response.DecodeAsync<BatchReferenceResponse[]>(cancellationToken);
    }
}

using System.Net;
using System.Net.Http.Json;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
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

        var beacons = ObjectHelper.MakeBeacons(to);
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

        var beacons = ObjectHelper.MakeBeacons(to);
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

        var beacons = ObjectHelper.MakeBeacons(to);
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

    internal async Task<BatchReferenceResponse[]> ReferenceAddMany(
        string collectionName,
        Models.DataReference[] references,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        CancellationToken cancellationToken = default
    )
    {
        var batchRefs = references.SelectMany(r =>
            ObjectHelper
                .MakeBeacons(r.To)
                .SelectMany(b => b.Values)
                .Select(beacon => new BatchReference
                {
                    From = new Uri(
                        ObjectHelper.MakeBeaconSource(collectionName, r.From, r.FromProperty)
                    ),
                    To = new Uri(beacon),
                    Tenant = tenant ?? default!,
                })
        );

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

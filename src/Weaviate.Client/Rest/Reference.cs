using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

public partial class WeaviateRestClient
{
    internal async Task ReferenceAdd(
        string collectionName,
        Guid from,
        string fromProperty,
        Guid to,
        string? tenant = null
    )
    {
        var path = WeaviateEndpoints.Reference(collectionName, from, fromProperty, tenant);

        var beacons = ObjectHelper.MakeBeacons(to);
        var reference = beacons.First();

        var response = await _httpClient.PostAsJsonAsync(
            path,
            reference,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "reference add");
    }

    internal async Task ReferenceReplace(
        string collectionName,
        Guid from,
        string fromProperty,
        Guid[] to,
        string? tenant = null
    )
    {
        var path = WeaviateEndpoints.Reference(collectionName, from, fromProperty, tenant);

        var beacons = ObjectHelper.MakeBeacons(to);
        var reference = beacons;

        var response = await _httpClient.PutAsJsonAsync(
            path,
            reference,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "reference replace");
    }

    internal async Task ReferenceDelete(
        string collectionName,
        Guid from,
        string fromProperty,
        Guid to,
        string? tenant = null
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

        var response = await _httpClient.SendAsync(request);

        await response.EnsureExpectedStatusCodeAsync([200], "reference delete");
    }

    internal async Task<BatchReferenceResponse[]> ReferenceAddMany(
        string collectionName,
        Models.DataReference[] references,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null
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
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "reference add many");

        return await response.Content.ReadFromJsonAsync<BatchReferenceResponse[]>(
                RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();
    }
}

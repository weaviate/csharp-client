using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

public partial class WeaviateRestClient
{
    internal async Task<Dto.Schema?> CollectionList()
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Collection());

        await response.EnsureExpectedStatusCodeAsync([200], "collection list");

        var contents = await response.Content.ReadFromJsonAsync<Dto.Schema>(
            options: RestJsonSerializerOptions
        );

        return contents;
    }

    internal async Task<Dto.Class?> CollectionGet(string name)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Collection(name));

        await response.EnsureExpectedStatusCodeAsync([200], "collection get");

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        var contents = await response.Content.ReadFromJsonAsync<Dto.Class>(
            options: RestJsonSerializerOptions
        );

        if (contents is null)
        {
            throw new WeaviateRestClientException();
        }

        return contents;
    }

    internal async Task CollectionDelete(string name)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.Collection(name));

        await response.EnsureExpectedStatusCodeAsync([200], "collection delete");
    }

    internal async Task<Dto.Class> CollectionCreate(Dto.Class collection)
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Collection(),
            collection,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "collection create");

        var contents = await response.Content.ReadFromJsonAsync<Dto.Class>(
            options: RestJsonSerializerOptions
        );

        if (contents is null)
        {
            throw new WeaviateRestClientException();
        }

        return contents;
    }

    internal async Task<Dto.Class> CollectionUpdate(string collectionName, Dto.Class collection)
    {
        var response = await _httpClient.PutAsJsonAsync(
            WeaviateEndpoints.Collection(collectionName),
            collection,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "collection update");

        var contents =
            await response.Content.ReadFromJsonAsync<Dto.Class>(options: RestJsonSerializerOptions)
            ?? throw new WeaviateRestServerException();

        return contents;
    }

    internal async Task CollectionAddProperty(string collectionName, Property property)
    {
        var path = WeaviateEndpoints.CollectionProperties(collectionName);

        var response = await _httpClient.PostAsJsonAsync(
            path,
            property,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "collection property add");
    }

    internal async Task<bool> CollectionExists(object collectionName)
    {
        var path = WeaviateEndpoints.Collection();

        var response = await _httpClient.GetAsync(path);

        await response.EnsureExpectedStatusCodeAsync([200], "collection property add");

        var schema = await response.Content.ReadFromJsonAsync<Schema>(
            options: RestJsonSerializerOptions
        );

        return schema?.Classes?.Any(c => c.Class1 is not null && c.Class1!.Equals(collectionName))
            ?? false;
    }
}

namespace Weaviate.Client;

using Weaviate.Client.Models;
using Weaviate.Client.Rest.Models;

internal static class WeaviateClientExtensions
{
    internal static Collection ToCollectionDefinition(this CollectionGeneric collection)
    {
        return new Collection()
        {
            Name = collection.Class,
            Description = collection.Description,
        };
    }
}

public struct CollectionsClient
{
    private readonly WeaviateClient _client;

    internal CollectionsClient(WeaviateClient client)
    {
        _client = client;
    }

    public async Task<CollectionClient> Create(Collection collection)
    {
        var data = new CollectionGeneric()
        {
            Class = collection.Name,
            Description = collection.Description,
        };
        var response = await _client.RestClient.CollectionCreate(data);

        return new CollectionClient(_client, response.ToCollectionDefinition());
    }

    public async Task Delete(string name)
    {
        await _client.RestClient.CollectionDelete(name);
    }

    public CollectionClient this[string name] => Get(name).Result;

    public async Task<CollectionClient> Get(string name)
    {
        var response = await _client.RestClient.CollectionGet(name);

        var collection = new CollectionClient(_client, response.ToCollectionDefinition());

        return collection;
    }

    public async IAsyncEnumerable<Collection> List()
    {
        var response = await _client.RestClient.CollectionList();

        foreach (var c in response.Collections)
        {
            yield return c.ToCollectionDefinition();
        }
    }
}

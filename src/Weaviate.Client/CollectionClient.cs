using Weaviate.Client.Models;

namespace Weaviate.Client;

public class CollectionClient
{
    private readonly WeaviateClient _client;
    private readonly string _collectionName;
    private Collection? _collection;

    internal CollectionClient(WeaviateClient client, Collection collection)
        : this(client, collection.Name)
    {
        _collection = collection;
    }

    internal CollectionClient(WeaviateClient client, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _client = client;
        _collectionName = name;
    }

    public async Task Delete()
    {
        await _client.Collections.Delete(_collectionName);
    }

    public async Task<CollectionClient> Get()
    {
        var response = await _client.RestClient.CollectionGet(_collectionName);

        _collection = response.ToCollectionDefinition();

        return new CollectionClient(_client, _collection);
    }
}
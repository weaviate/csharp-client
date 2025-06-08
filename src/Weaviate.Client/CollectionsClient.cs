namespace Weaviate.Client;

public struct CollectionsClient
{
    private readonly WeaviateClient _client;

    internal CollectionsClient(WeaviateClient client)
    {
        _client = client;
    }

    public async Task<CollectionClient<dynamic>> Create(Models.Collection collection)
    {
        var response = await _client.RestClient.CollectionCreate(collection.ToDto());

        return new CollectionClient<dynamic>(_client, response.ToModel());
    }

    public async Task<CollectionClient<TData>> Create<TData>(Models.Collection collection)
    {
        var response = await _client.RestClient.CollectionCreate(collection.ToDto());

        return new CollectionClient<TData>(_client, response.ToModel());
    }

    public async Task Delete(string collectionName)
    {
        ArgumentException.ThrowIfNullOrEmpty(collectionName);

        await _client.RestClient.CollectionDelete(collectionName);
    }

    public async IAsyncEnumerable<Models.Collection> List()
    {
        var response = await _client.RestClient.CollectionList();

        foreach (var c in response?.Classes ?? Enumerable.Empty<Rest.Dto.Class>())
        {
            yield return c.ToModel();
        }
    }

    public CollectionClient<dynamic> Use(string name)
    {
        return new CollectionClient<dynamic>(_client, name);
    }

    public CollectionClient<TData> Use<TData>(string? name = null)
    {
        name = name ?? typeof(TData).Name;

        return new CollectionClient<TData>(_client, name);
    }
}

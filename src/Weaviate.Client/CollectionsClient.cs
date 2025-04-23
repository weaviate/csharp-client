namespace Weaviate.Client;

public struct CollectionsClient
{
    private readonly WeaviateClient _client;

    internal CollectionsClient(WeaviateClient client)
    {
        _client = client;
    }

    public async Task<CollectionClient<TData>> Create<TData>(Action<Models.Collection> collectionConfigurator)
    {
        var collection = new Models.Collection();

        collectionConfigurator(collection);

        var response = await _client.RestClient.CollectionCreate(collection.ToDto());

        return new CollectionClient<TData>(_client, response.ToModel());
    }

    public async IAsyncEnumerable<Models.Collection> List()
    {
        var response = await _client.RestClient.CollectionList();

        foreach (var c in response.Collections)
        {
            yield return c.ToModel();
        }
    }

    public CollectionClient<TData> Use<TData>(string? name = null)
    {
        name = name ?? typeof(TData).Name;

        return new CollectionClient<TData>(_client, name);
    }
}

using Weaviate.Client.Models;

namespace Weaviate.Client;

public record CollectionsClient
{
    private readonly WeaviateClient _client;

    internal CollectionsClient(WeaviateClient client)
    {
        _client = client;
    }

    public async Task<CollectionClient<dynamic>> Create(
        Models.CollectionConfig collection,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client.RestClient.CollectionCreate(
            collection.ToDto(),
            cancellationToken
        );

        return new CollectionClient<dynamic>(_client, response.ToModel());
    }

    public async Task<CollectionClient<TData>> Create<TData>(
        Models.CollectionConfig collection,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client.RestClient.CollectionCreate(
            collection.ToDto(),
            cancellationToken
        );

        return new CollectionClient<TData>(_client, response.ToModel());
    }

    public async Task Delete(string collectionName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(collectionName);

        await _client.RestClient.CollectionDelete(collectionName, cancellationToken);
    }

    public async Task DeleteAll(CancellationToken cancellationToken = default)
    {
        var list = await List(cancellationToken).Select(l => l.Name).ToListAsync(cancellationToken);

        var tasks = list.Select(name => Delete(name, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task<bool> Exists(
        string collectionName,
        CancellationToken cancellationToken = default
    )
    {
        return await _client.RestClient.CollectionExists(collectionName, cancellationToken);
    }

    public async Task<CollectionConfig?> Export(
        string collectionName,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client.RestClient.CollectionGet(collectionName, cancellationToken);

        if (response is null)
        {
            return null;
        }

        return response.ToModel();
    }

    public async IAsyncEnumerable<Models.CollectionConfig> List(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken = default
    )
    {
        var response = await _client.RestClient.CollectionList(cancellationToken);

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

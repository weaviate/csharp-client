using Weaviate.Client.Models;

namespace Weaviate.Client;

public record CollectionsClient
{
    private readonly WeaviateClient _client;

    internal CollectionsClient(WeaviateClient client)
    {
        _client = client;
    }

    public async Task<CollectionClient> Create(
        Models.CollectionConfig collection,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client.RestClient.CollectionCreate(
            collection.ToDto(),
            cancellationToken
        );

        return new CollectionClient(_client, response.ToModel());
    }

    public async Task<Typed.TypedCollectionClient<T>> Create<T>(
        Models.CollectionConfig collection,
        bool validateType = false,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        var response = await _client.RestClient.CollectionCreate(
            collection.ToDto(),
            cancellationToken
        );

        var innerClient = new CollectionClient(_client, response.ToModel());

        return await innerClient.AsTyped<T>(validateType, cancellationToken);
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

    public CollectionClient Use(string name)
    {
        return new CollectionClient(_client, name);
    }

    /// <summary>
    /// Creates a strongly-typed collection client for accessing a specific collection.
    /// The collection client provides type-safe data and query operations.
    /// </summary>
    /// <typeparam name="T">The C# type representing objects in this collection.</typeparam>
    /// <param name="name">The name of the collection.</param>
    /// <param name="validateType">If true, validates that type T is compatible with the collection schema on construction. Default is false for performance.</param>
    /// <returns>A TypedCollectionClient that provides strongly-typed operations.</returns>
    /// <exception cref="InvalidOperationException">Thrown if validateType is true and validation fails with errors.</exception>
    public async Task<Typed.TypedCollectionClient<T>> Use<T>(string name, bool validateType = false)
        where T : class, new()
    {
        var innerClient = Use(name);

        return await innerClient.AsTyped<T>(validateType);
    }
}

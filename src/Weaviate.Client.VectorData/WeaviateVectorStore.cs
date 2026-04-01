using System.Runtime.CompilerServices;
using Microsoft.Extensions.VectorData;

namespace Weaviate.Client.VectorData;

/// <summary>
/// Weaviate implementation of <see cref="VectorStore"/>.
/// Wraps a <see cref="WeaviateClient"/> and provides access to vector store collections.
/// </summary>
public class WeaviateVectorStore : VectorStore
{
    private readonly WeaviateClient _client;
    private readonly WeaviateVectorStoreOptions? _options;

    /// <summary>
    /// Initializes a new instance of <see cref="WeaviateVectorStore"/>.
    /// </summary>
    /// <param name="client">The Weaviate client to use.</param>
    /// <param name="options">Optional configuration options.</param>
    public WeaviateVectorStore(WeaviateClient client, WeaviateVectorStoreOptions? options = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options;
    }

    /// <inheritdoc />
    public override VectorStoreCollection<TKey, TRecord> GetCollection<TKey, TRecord>(
        string name,
        VectorStoreCollectionDefinition? definition = null
    )
    {
        var collectionOptions = _options?.CollectionOptionsFactory?.Invoke(name);

        return new WeaviateVectorStoreCollection<TKey, TRecord>(
            _client,
            name,
            definition,
            collectionOptions
        );
    }

    /// <inheritdoc />
    public override VectorStoreCollection<object, Dictionary<string, object?>> GetDynamicCollection(
        string name,
        VectorStoreCollectionDefinition definition
    )
    {
        var collectionOptions = _options?.CollectionOptionsFactory?.Invoke(name);

        return new WeaviateVectorStoreCollection<object, Dictionary<string, object?>>(
            _client,
            name,
            definition,
            collectionOptions
        );
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<string> ListCollectionNamesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (
            var config in _client.Collections.List(cancellationToken).ConfigureAwait(false)
        )
        {
            yield return config.Name;
        }
    }

    /// <inheritdoc />
    public override Task<bool> CollectionExistsAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return _client.Collections.Exists(name, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task EnsureCollectionDeletedAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        if (await CollectionExistsAsync(name, cancellationToken).ConfigureAwait(false))
        {
            await _client.Collections.Delete(name, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey != null)
            return null;

        if (serviceType == typeof(VectorStoreCollectionMetadata))
        {
            return new VectorStoreCollectionMetadata { VectorStoreSystemName = "weaviate" };
        }

        return null;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // No unmanaged resources to dispose — the underlying WeaviateClient is owned by the caller.
    }
}

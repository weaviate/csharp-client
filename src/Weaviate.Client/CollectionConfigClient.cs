using Weaviate.Client.Cache;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Manages the configuration of a specific collection in the Weaviate instance.
/// Provides methods to get, update, and modify the collection's schema, properties, and vectors.
/// </summary>
public class CollectionConfigClient
{
    /// <summary>
    /// The client
    /// </summary>
    private readonly WeaviateClient _client;

    /// <summary>
    /// The collection name
    /// </summary>
    private readonly string _collectionName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionConfigClient"/> class
    /// </summary>
    /// <param name="client">The client</param>
    /// <param name="collectionName">The collection name</param>
    internal CollectionConfigClient(WeaviateClient client, string collectionName)
    {
        _client = client;
        _collectionName = collectionName;
    }

    /// <summary>
    /// Gets the cached collection config, fetching from server if needed.
    /// </summary>
    /// <param name="schemaCache">Optional schema cache. If null, uses SchemaCache.Default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection config, or null if not found.</returns>
    public async Task<CollectionConfig?> GetCachedConfig(
        SchemaCache? schemaCache = null,
        CancellationToken cancellationToken = default
    )
    {
        var cache = schemaCache ?? Cache.SchemaCache.Default;
        return await cache.GetOrFetch(
            _collectionName,
            async () =>
            {
                var client = _client;
                return await client.Collections.Export(_collectionName, cancellationToken);
            }
        );
    }

    /// <summary>
    /// Adds a reference property to the collection.
    /// </summary>
    /// <param name="referenceProperty">The reference property to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task AddReference(
        Reference referenceProperty,
        CancellationToken cancellationToken = default
    )
    {
        var dto = referenceProperty.ToDto();

        await _client.RestClient.CollectionAddProperty(_collectionName, dto, cancellationToken);
    }

    /// <summary>
    /// Adds a property to the collection.
    /// </summary>
    /// <param name="p">The property to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task AddProperty(Property p, CancellationToken cancellationToken = default)
    {
        await _client.RestClient.CollectionAddProperty(
            _collectionName,
            p.ToDto(),
            cancellationToken
        );
    }

    // Add new named vectors
    /// <summary>
    /// Adds a new named vector to the collection.
    /// </summary>
    /// <param name="vector">The vector configuration to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task AddVector(VectorConfig vector, CancellationToken cancellationToken = default)
    {
        // 1. Fetch the collection config
        var collection =
            await _client.Collections.Export(_collectionName, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Collection '{_collectionName}' does not exist."
            );

        // 2. Add the named vector
        collection.VectorConfig.Add(vector);

        var dto = collection.ToDto();

        // 3. PUT to /schema
        await _client.RestClient.CollectionUpdate(_collectionName, dto, cancellationToken);
    }

    // Proxied property updates

    /// <summary>
    /// Updates the collection configuration.
    /// </summary>
    /// <param name="c">Action to update the collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<CollectionConfigExport> Update(
        Action<CollectionUpdate> c,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Fetch the collection config
        var collection =
            await _client.Collections.Export(_collectionName, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Collection '{_collectionName}' does not exist."
            );

        // 2. Apply everything that is not null over a  collection export
        c(new CollectionUpdate(collection));

        // 3. PUT to /schema
        var result = await _client.RestClient.CollectionUpdate(
            _collectionName,
            collection.ToDto(),
            cancellationToken
        );

        return result.ToModel();
    }

    /// <summary>
    /// Retrieves the collection configuration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection configuration.</returns>
    public async Task<CollectionConfigExport> Get(CancellationToken cancellationToken = default)
    {
        var response =
            await _client.RestClient.CollectionGet(_collectionName, cancellationToken)
            ?? throw new WeaviateClientException(
                new InvalidOperationException($"Collection '{_collectionName}' does not exist.")
            );

        return response.ToModel();
    }

    /// <summary>
    /// Gets all shards for this collection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of shard information for the collection.</returns>
    public async Task<IList<ShardInfo>> GetShards(CancellationToken cancellationToken = default)
    {
        var shards = await _client.RestClient.CollectionGetShards(
            _collectionName,
            cancellationToken
        );

        return shards.Select(s => s.ToModel()).ToList();
    }

    /// <summary>
    /// Gets information about a specific shard.
    /// </summary>
    /// <param name="shardName">The name of the shard to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Information about the specified shard, or null if not found.</returns>
    public async Task<ShardInfo?> GetShard(
        string shardName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(shardName);

        var shard = await _client.RestClient.CollectionGetShard(
            _collectionName,
            shardName,
            cancellationToken
        );

        return shard?.ToModel();
    }

    /// <summary>
    /// Updates the status of one or more shards.
    /// </summary>
    /// <param name="status">The new status to set for the shards.</param>
    /// <param name="shardNames">The names of the shards to update.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of updated shard information.</returns>
    public async Task<IList<ShardInfo>> UpdateShardStatus(
        ShardStatus status,
        Internal.AutoArray<string>? shardNames = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(shardNames);

        if (!shardNames.Any())
        {
            throw new ArgumentException(
                "At least one shard name must be provided.",
                nameof(shardNames)
            );
        }

        var shardStatus = new Rest.Dto.ShardStatus { Status = status.ToApiString() };

        var tasks = shardNames.Select(shardName =>
            _client.RestClient.CollectionUpdateShard(
                _collectionName,
                shardName,
                shardStatus,
                cancellationToken
            )
        );

        var results = await Task.WhenAll(tasks);

        return results.Select(r => r.ToModel()).ToList();
    }
}

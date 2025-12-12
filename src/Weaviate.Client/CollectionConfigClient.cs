using Weaviate.Client.Cache;
using Weaviate.Client.Models;

namespace Weaviate.Client;

public class CollectionConfigClient
{
    private readonly WeaviateClient _client;
    private readonly string _collectionName;

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

    public async Task AddReference(Reference referenceProperty)
    {
        var dto = referenceProperty.ToDto();

        await _client.RestClient.CollectionAddProperty(_collectionName, dto);
    }

    public async Task AddProperty(Property p)
    {
        await _client.RestClient.CollectionAddProperty(_collectionName, p.ToDto());
    }

    // Add new named vectors
    public async Task AddVector(VectorConfig vector)
    {
        // 1. Fetch the collection config
        var collection =
            await _client.Collections.Export(_collectionName)
            ?? throw new InvalidOperationException(
                $"Collection '{_collectionName}' does not exist."
            );

        // 2. Add the named vector
        collection.VectorConfig.Add(vector);

        var dto = collection.ToDto();

        // 3. PUT to /schema
        await _client.RestClient.CollectionUpdate(_collectionName, dto);
    }

    // Proxied property updates

    public async Task<CollectionConfigExport> Update(Action<CollectionUpdate> c)
    {
        // 1. Fetch the collection config
        var collection =
            await _client.Collections.Export(_collectionName)
            ?? throw new InvalidOperationException(
                $"Collection '{_collectionName}' does not exist."
            );

        // 2. Apply everything that is not null over a  collection export
        c(new CollectionUpdate(collection));

        // 3. PUT to /schema
        var result = await _client.RestClient.CollectionUpdate(_collectionName, collection.ToDto());

        return result.ToModel();
    }

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
    /// <returns>A list of updated shard information.</returns>
    public async Task<IList<ShardInfo>> UpdateShardStatus(
        ShardStatus status,
        params string[] shardNames
    )
    {
        ArgumentNullException.ThrowIfNull(shardNames);

        if (shardNames.Length == 0)
        {
            throw new ArgumentException(
                "At least one shard name must be provided.",
                nameof(shardNames)
            );
        }

        var shardStatus = new Rest.Dto.ShardStatus { Status = status.ToApiString() };

        var tasks = shardNames.Select(shardName =>
            _client.RestClient.CollectionUpdateShard(_collectionName, shardName, shardStatus)
        );

        var results = await Task.WhenAll(tasks);

        return results.Select(r => r.ToModel()).ToList();
    }
}

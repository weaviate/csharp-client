using System.Collections.ObjectModel;
using Weaviate.Client.Cache;

namespace Weaviate.Client.Models;

public record InvertedIndexConfigUpdate(InvertedIndexConfig WrappedConfig)
{
    public Bm25ConfigUpdate Bm25 => new(WrappedConfig.Bm25 ??= BM25Config.Default);
    public int CleanupIntervalSeconds
    {
        get => WrappedConfig.CleanupIntervalSeconds;
        set => WrappedConfig.CleanupIntervalSeconds = value;
    }
    public StopwordsConfigUpdate Stopwords =>
        new(WrappedConfig.Stopwords ??= StopwordConfig.Default);
}

public record Bm25ConfigUpdate(BM25Config WrappedBm25)
{
    public double? B
    {
        get => WrappedBm25.B;
        set => WrappedBm25.B = value ?? BM25Config.Default.B;
    }

    public double? K1
    {
        get => WrappedBm25.K1;
        set => WrappedBm25.K1 = value ?? BM25Config.Default.K1;
    }
}

public record StopwordsConfigUpdate(StopwordConfig WrappedStopwords)
{
    public List<string> Additions
    {
        get => WrappedStopwords.Additions;
        set => WrappedStopwords.Additions = value ?? new List<string>();
    }
    public StopwordConfig.Presets Preset
    {
        get => WrappedStopwords.Preset;
        set => WrappedStopwords.Preset = value;
    }
    public List<string> Removals
    {
        get => WrappedStopwords.Removals;
        set => WrappedStopwords.Removals = value ?? new List<string>();
    }
}

public partial record PropertyUpdate(Property WrappedProperty)
{
    public string Name => WrappedProperty.Name;
    public string? Description
    {
        get => WrappedProperty.Description;
        set => WrappedProperty.Description = value;
    }
}

public partial record CollectionUpdate(CollectionConfig WrappedCollection)
{
    public string Name => WrappedCollection.Name;
    public string Description
    {
        get => WrappedCollection.Description;
        set => WrappedCollection.Description = value;
    }

    // Define properties of the collection.
    public ReadOnlyDictionary<string, PropertyUpdate> Properties { get; } =
        WrappedCollection
            .Properties.ToDictionary(p => p.Name, p => new PropertyUpdate(p))
            .AsReadOnly();
    public ReadOnlyCollection<PropertyUpdate> References { get; } =
        WrappedCollection.References.Select(r => new PropertyUpdate(r)).ToList().AsReadOnly();

    // inverted index config
    public InvertedIndexConfigUpdate InvertedIndexConfig
    {
        get => new(WrappedCollection.InvertedIndexConfig ??= Models.InvertedIndexConfig.Default);
    }

    // Configuration specific to modules in a collection context.
    public ModuleConfigList? ModuleConfig // TODO Considering removing this, or making it internal.
    {
        get => WrappedCollection.ModuleConfig ??= new();
    }

    public IRerankerConfig? RerankerConfig
    {
        get => WrappedCollection.RerankerConfig;
        set => WrappedCollection.RerankerConfig = value;
    }

    public IGenerativeConfig? GenerativeConfig
    {
        get => WrappedCollection.GenerativeConfig;
        set => WrappedCollection.GenerativeConfig = value;
    }

    // multi tenancy config
    public MultiTenancyConfigUpdate MultiTenancyConfig =>
        new(WrappedCollection.MultiTenancyConfig ?? Models.MultiTenancyConfig.Default);

    // replication config
    public ReplicationConfigUpdate ReplicationConfig = new(
        WrappedCollection.ReplicationConfig ?? Models.ReplicationConfig.Default
    );

    // Manage how the index should be sharded and distributed in the cluster
    public ShardingConfig? ShardingConfig
    {
        get => WrappedCollection.ShardingConfig;
    }

    // Configure named vectors. Either use this field or `vectorizer`, `vectorIndexType`, and `vectorIndexConfig` fields. Available from `v1.24.0`.
    public ReadOnlyDictionary<string, VectorConfigUpdate> VectorConfig { get; } =
        WrappedCollection
            .VectorConfig.Select(vc => new VectorConfigUpdate(WrappedCollection, vc.Value))
            .ToDictionary(vc => vc.Name, vc => vc)
            .AsReadOnly();

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(
            WrappedCollection.ToDto(),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Description);
        hash.Add(Properties);
        hash.Add(References);
        hash.Add(InvertedIndexConfig);
        hash.Add(ModuleConfig);
        hash.Add(MultiTenancyConfig);
        hash.Add(ReplicationConfig);
        hash.Add(ShardingConfig);
        hash.Add(VectorConfig);
        return hash.ToHashCode();
    }
}

public class VectorConfigUpdate(
    CollectionConfig WrappedCollection,
    VectorConfig WrappedVectorConfig
)
{
    public string Name
    {
        get => WrappedVectorConfig.Name;
        set
        {
            var vc = WrappedCollection.VectorConfig[WrappedVectorConfig.Name];
            WrappedCollection.VectorConfig.Remove(vc.Name);
            WrappedVectorConfig.Name = value;
            WrappedCollection.VectorConfig.Add(WrappedVectorConfig);
        }
    }

    public VectorIndexConfigUpdate VectorIndexConfig
    {
        get => new(WrappedVectorConfig.VectorIndexConfig);
    }
}

public class VectorIndexConfigUpdate(VectorIndexConfig? WrappedVectorIndexConfig)
{
    public void UpdateHNSW(Action<VectorIndexConfigUpdateHNSW> c)
    {
        VectorIndex.HNSW hnsw =
            WrappedVectorIndexConfig as VectorIndex.HNSW
            ?? throw new InvalidOperationException("VectorIndexConfig is not of type HNSW.");

        c(new(hnsw));
    }

    public void UpdateFlat(Action<VectorIndexConfigUpdateFlat> c)
    {
        VectorIndex.Flat flat =
            WrappedVectorIndexConfig as VectorIndex.Flat
            ?? throw new InvalidOperationException("VectorIndexConfig is not of type Flat.");

        c(new(flat));
    }

    public void UpdateDynamic(Action<VectorIndexConfigUpdateDynamic> c)
    {
        VectorIndex.Dynamic dynamic =
            WrappedVectorIndexConfig as VectorIndex.Dynamic
            ?? throw new InvalidOperationException("VectorIndexConfig is not of type Dynamic.");

        c(new(dynamic));
    }
}

public class VectorIndexConfigUpdateHNSW(VectorIndex.HNSW WrappedHNSW)
{
    public int? DynamicEfFactor
    {
        get => WrappedHNSW.DynamicEfFactor;
        set => WrappedHNSW.DynamicEfFactor = value;
    }

    public int? DynamicEfMax
    {
        get => WrappedHNSW.DynamicEfMax;
        set => WrappedHNSW.DynamicEfMax = value;
    }

    public int? DynamicEfMin
    {
        get => WrappedHNSW.DynamicEfMin;
        set => WrappedHNSW.DynamicEfMin = value;
    }

    public int? Ef
    {
        get => WrappedHNSW.Ef;
        set => WrappedHNSW.Ef = value;
    }

    public VectorIndexConfig.VectorIndexFilterStrategy? FilterStrategy
    {
        get => WrappedHNSW.FilterStrategy;
        set => WrappedHNSW.FilterStrategy = value;
    }

    public int? FlatSearchCutoff
    {
        get => WrappedHNSW.FlatSearchCutoff;
        set => WrappedHNSW.FlatSearchCutoff = value;
    }

    public VectorIndexConfig.QuantizerConfigBase? Quantizer
    {
        get => WrappedHNSW.Quantizer;
        set => WrappedHNSW.Quantizer = value;
    }

    public long? VectorCacheMaxObjects
    {
        get => WrappedHNSW.VectorCacheMaxObjects;
        set => WrappedHNSW.VectorCacheMaxObjects = value;
    }
}

public class VectorIndexConfigUpdateDynamic(VectorIndex.Dynamic WrappedDynamic)
{
    public VectorIndexConfig.VectorDistance? Distance
    {
        get => WrappedDynamic.Distance;
        set => WrappedDynamic.Distance = value;
    }
    public int? Threshold
    {
        get => WrappedDynamic.Threshold;
        set => WrappedDynamic.Threshold = value;
    }

    public VectorIndexConfigUpdateHNSW? Hnsw
    {
        get => new(WrappedDynamic.Hnsw ?? new VectorIndex.HNSW());
    }
    public VectorIndexConfigUpdateFlat? Flat
    {
        get => new(WrappedDynamic.Flat ?? new VectorIndex.Flat());
    }
}

public class VectorIndexConfigUpdateFlat(VectorIndex.Flat WrappedFlat)
{
    public VectorIndexConfig.QuantizerConfigFlat? Quantizer
    {
        get => (VectorIndexConfig.QuantizerConfigFlat?)WrappedFlat.Quantizer;
        set => WrappedFlat.Quantizer = value;
    }

    public long? VectorCacheMaxObjects
    {
        get => WrappedFlat.VectorCacheMaxObjects;
        set => WrappedFlat.VectorCacheMaxObjects = value;
    }
}

public class ReplicationConfigUpdate(ReplicationConfig WrappedReplicationConfig)
{
    public bool AsyncEnabled
    {
        get => WrappedReplicationConfig.AsyncEnabled;
        set => WrappedReplicationConfig.AsyncEnabled = value;
    }

    public DeletionStrategy? DeletionStrategy
    {
        get => WrappedReplicationConfig.DeletionStrategy;
        set => WrappedReplicationConfig.DeletionStrategy = value;
    }

    public int Factor
    {
        get => WrappedReplicationConfig.Factor;
        set => WrappedReplicationConfig.Factor = value;
    }
}

public class MultiTenancyConfigUpdate(MultiTenancyConfig WrappedMultiTenancyConfig)
{
    public bool AutoTenantCreation
    {
        get => WrappedMultiTenancyConfig.AutoTenantCreation;
        set => WrappedMultiTenancyConfig.AutoTenantCreation = value;
    }

    public bool AutoTenantActivation
    {
        get => WrappedMultiTenancyConfig.AutoTenantActivation;
        set => WrappedMultiTenancyConfig.AutoTenantActivation = value;
    }
}

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

    internal async Task AddReference(Reference referenceProperty)
    {
        var p = (Property)referenceProperty;

        var dto = new Rest.Dto.Property() { Name = p.Name, DataType = [.. p.DataType] };

        await _client.RestClient.CollectionAddProperty(_collectionName, dto);
    }

    public async Task AddProperty(Property p)
    {
        await _client.RestClient.CollectionAddProperty(
            _collectionName,
            new Rest.Dto.Property()
            {
                Name = p.Name,
                DataType = [.. p.DataType],
                Description = p.Description,
                IndexFilterable = p.IndexFilterable,
#pragma warning disable CS0612 // Type or member is obsolete
                IndexInverted = p.IndexInverted,
#pragma warning restore CS0612 // Type or member is obsolete
                IndexRangeFilters = p.IndexRangeFilters,
                IndexSearchable = p.IndexSearchable,
                Tokenization = (Rest.Dto.PropertyTokenization?)p.PropertyTokenization,
            }
        );
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

    public async Task<CollectionConfig> Update(Action<CollectionUpdate> c)
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

    public async Task<CollectionConfig?> Get(CancellationToken cancellationToken = default)
    {
        var response = await _client.RestClient.CollectionGet(_collectionName, cancellationToken);

        return response?.ToModel();
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

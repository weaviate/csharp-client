namespace Weaviate.Client.Models;

public class Collection : CollectionBase<object, object, object> { }

public class CollectionBase<TModuleConfig, TShardingConfig, TVectorIndexConfig>
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    // Define properties of the collection.
    public IList<Property> Properties { get; set; } = new List<Property>();

    // inverted index config
    public InvertedIndexConfig? InvertedIndexConfig { get; set; }

    // Configuration specific to modules in a collection context.
    public TModuleConfig? ModuleConfig { get; set; }

    // multi tenancy config
    public MultiTenancyConfig? MultiTenancyConfig { get; set; }

    // replication config
    public ReplicationConfig? ReplicationConfig { get; set; }

    // Manage how the index should be sharded and distributed in the cluster
    public TShardingConfig? ShardingConfig { get; set; }

    // Configure named vectors. Either use this field or `vectorizer`, `vectorIndexType`, and `vectorIndexConfig` fields. Available from `v1.24.0`.
    public IDictionary<string, VectorConfig> VectorConfig { get; set; } = default!;

    // Vector-index config, that is specific to the type of index selected in vectorIndexType
    [Obsolete("Use `VectorConfig` instead")]
    public TVectorIndexConfig? VectorIndexConfig { get; set; }

    // Name of the vector index to use, eg. (HNSW)
    [Obsolete("Use `VectorConfig` instead")]
    public string VectorIndexType { get; set; } = default!;

    [Obsolete("Use `VectorConfig` instead")]
    public string Vectorizer { get; set; } = "";
}

namespace Weaviate.Client.Rest.Dto;

public class CollectionGeneric : CollectionBase<object, object, object>
{
}

public class CollectionBase<TModuleConfig, TShardingConfig, TVectorIndexConfig>
{
    // Name of the class (a.k.a. 'collection') (required). Multiple words should be concatenated in CamelCase, e.g. `ArticleAuthor`.
    public required string Class { get; set; }

    // Description of the collection for metadata purposes.
    public string Description { get; set; } = "";

    // Define properties of the collection.
    public IList<Property> Properties { get; set; } = default!;

    // Configure named vectors. Either use this field or `vectorizer`, `vectorIndexType`, and `vectorIndexConfig` fields. Available from `v1.24.0`.
    public IDictionary<string, VectorConfig> VectorConfig { get; set; } = default!;

    // inverted index config
    public InvertedIndexConfig? InvertedIndexConfig { get; set; }

    // Configuration specific to modules in a collection context.
    // TODO Switch to IDictionary<string, TModuleConfig>
    public TModuleConfig? ModuleConfig { get; set; }

    // multi tenancy config
    public MultiTenancyConfig? MultiTenancyConfig { get; set; }

    // replication config
    public ReplicationConfig? ReplicationConfig { get; set; }

    // Manage how the index should be sharded and distributed in the cluster
    public TShardingConfig? ShardingConfig { get; set; }

    // Vector-index config, that is specific to the type of index selected in vectorIndexType
    public TVectorIndexConfig? VectorIndexConfig { get; set; }

    // Name of the vector index to use, eg. (HNSW)
    public string VectorIndexType { get; set; } = "";

    public string Vectorizer { get; set; } = "none";
}

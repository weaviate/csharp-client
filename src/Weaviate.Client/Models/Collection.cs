namespace Weaviate.Client.Models;

public partial record Collection : IEquatable<Collection>
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    // Define properties of the collection.
    public List<Property> Properties { get; set; } = [];
    public List<ReferenceProperty> References { get; set; } = [];

    // inverted index config
    public InvertedIndexConfig? InvertedIndexConfig { get; set; }

    // Configuration specific to modules in a collection context.
    // TODO Considering removing this
    public ModuleConfigList? ModuleConfig { get; set; }

    public IRerankerConfig? RerankerConfig { get; set; }

    public IGenerativeConfig? GenerativeConfig { get; set; }

    // multi tenancy config
    public MultiTenancyConfig? MultiTenancyConfig { get; set; }

    // replication config
    public ReplicationConfig? ReplicationConfig { get; set; }

    // Manage how the index should be sharded and distributed in the cluster
    public ShardingConfig? ShardingConfig { get; set; }

    // Configure named vectors. Either use this field or `vectorizer`, `vectorIndexType`, and `vectorIndexConfig` fields. Available from `v1.24.0`.
    public VectorConfigList VectorConfig { get; set; } = default!;

    // Vector-index config, that is specific to the type of index selected in vectorIndexType
    [Obsolete("Use `VectorConfig` instead")]
    public object? VectorIndexConfig { get; set; }

    // Name of the vector index to use, eg. (HNSW)
    [Obsolete("Use `VectorConfig` instead")]
    public string VectorIndexType { get; set; } = default!;

    [Obsolete("Use `VectorConfig` instead")]
    public string Vectorizer { get; set; } = "";

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(
            this.ToDto(),
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

    public virtual bool Equals(Collection? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Name == other.Name
            && Description == other.Description
            && Properties.SequenceEqual(other.Properties)
            && References.SequenceEqual(other.References)
            && EqualityComparer<InvertedIndexConfig?>.Default.Equals(
                InvertedIndexConfig,
                other.InvertedIndexConfig
            )
            && EqualityComparer<IDictionary<string, object>?>.Default.Equals(
                ModuleConfig,
                other.ModuleConfig
            )
            && EqualityComparer<MultiTenancyConfig?>.Default.Equals(
                MultiTenancyConfig,
                other.MultiTenancyConfig
            )
            && EqualityComparer<ReplicationConfig?>.Default.Equals(
                ReplicationConfig,
                other.ReplicationConfig
            )
            && EqualityComparer<ShardingConfig?>.Default.Equals(
                ShardingConfig ?? ShardingConfig.Zero,
                other.ShardingConfig ?? ShardingConfig.Zero
            )
            && EqualityComparer<VectorConfigList>.Default.Equals(VectorConfig, other.VectorConfig)
            && true;
    }
}

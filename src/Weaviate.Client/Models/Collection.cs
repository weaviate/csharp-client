namespace Weaviate.Client.Models;

public abstract record CollectionConfigCommon
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    // Define properties of the collection.
    public Property[] Properties { get; set; } = [];
    public Reference[] References { get; set; } = [];

    // inverted index config
    public InvertedIndexConfig? InvertedIndexConfig { get; set; }

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

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Description);
        hash.Add(Properties);
        hash.Add(References);
        hash.Add(InvertedIndexConfig);
        hash.Add(MultiTenancyConfig);
        hash.Add(ReplicationConfig);
        hash.Add(ShardingConfig);
        hash.Add(VectorConfig);
        return hash.ToHashCode();
    }

    public virtual bool Equals(CollectionConfig? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Name != other.Name)
            return false;

        if (Description != other.Description)
            return false;

        if (!Properties.SequenceEqual(other.Properties))
            return false;

        if (!References.SequenceEqual(other.References))
            return false;

        if (
            !EqualityComparer<InvertedIndexConfig?>.Default.Equals(
                InvertedIndexConfig,
                other.InvertedIndexConfig
            )
        )
            return false;

        if (
            !EqualityComparer<MultiTenancyConfig?>.Default.Equals(
                MultiTenancyConfig,
                other.MultiTenancyConfig
            )
        )
            return false;

        if (
            !EqualityComparer<ReplicationConfig?>.Default.Equals(
                ReplicationConfig,
                other.ReplicationConfig
            )
        )
            return false;

        if (
            !EqualityComparer<ShardingConfig?>.Default.Equals(
                ShardingConfig ?? ShardingConfig.Zero,
                other.ShardingConfig ?? ShardingConfig.Zero
            )
        )
            return false;

        if (!EqualityComparer<VectorConfigList>.Default.Equals(VectorConfig, other.VectorConfig))
            return false;

        return true;
    }
}

public partial record CollectionCreateParams : CollectionConfigCommon
{
    public static CollectionCreateParams FromCollectionConfig(CollectionConfig config)
    {
        return new CollectionCreateParams
        {
            Name = config.Name,
            Description = config.Description,
            Properties = config.Properties,
            References = config.References,
            InvertedIndexConfig = config.InvertedIndexConfig,
            MultiTenancyConfig = config.MultiTenancyConfig,
            ReplicationConfig = config.ReplicationConfig,
            ShardingConfig = config.ShardingConfig,
            VectorConfig = config.VectorConfig,
            GenerativeConfig = config.GenerativeConfig,
            RerankerConfig = config.RerankerConfig,
        };
    }
}

public partial record CollectionConfig : CollectionConfigCommon
{
    internal CollectionConfig()
        : base() { }

    public static CollectionConfig FromCollectionCreate(CollectionCreateParams config)
    {
        return new CollectionConfig
        {
            Name = config.Name,
            Description = config.Description,
            Properties = config.Properties,
            References = config.References,
            InvertedIndexConfig = config.InvertedIndexConfig,
            MultiTenancyConfig = config.MultiTenancyConfig,
            ReplicationConfig = config.ReplicationConfig,
            ShardingConfig = config.ShardingConfig,
            VectorConfig = config.VectorConfig,
            GenerativeConfig = config.GenerativeConfig,
            RerankerConfig = config.RerankerConfig,
        };
    }

    // Configuration specific to modules in a collection context.
    public ModuleConfigList? ModuleConfig { get; set; }

    // Vector-index config, that is specific to the type of index selected in vectorIndexType
    public object? VectorIndexConfig { get; internal set; }

    // Name of the vector index to use, eg. (HNSW)
    public string VectorIndexType { get; internal set; } = default!;

    public string Vectorizer { get; internal set; } = "";

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(ModuleConfig);
        hash.Add(VectorIndexConfig);
        hash.Add(VectorIndexType);
        hash.Add(Vectorizer);
        return hash.ToHashCode();
    }

    public override bool Equals(CollectionConfig? other)
    {
        if (!base.Equals(other))
            return false;

        if (other is null)
            return false;

        if (!EqualityComparer<ModuleConfigList?>.Default.Equals(ModuleConfig, other.ModuleConfig))
            return false;

        if (!VectorIndexConfig?.Equals(other.VectorIndexConfig) ?? other.VectorIndexConfig != null)
            return false;

        if (VectorIndexType != other.VectorIndexType)
            return false;

        if (Vectorizer != other.Vectorizer)
            return false;

        return true;
    }
}

namespace Weaviate.Client.Models;

/// <summary>
/// The collection config common
/// </summary>
public abstract record CollectionConfigCommon
{
    /// <summary>
    /// Gets or sets the value of the name
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string Description { get; set; } = "";

    // Define properties of the collection.
    /// <summary>
    /// Gets or sets the value of the properties
    /// </summary>
    public Property[] Properties { get; set; } = [];

    /// <summary>
    /// Gets or sets the value of the references
    /// </summary>
    public Reference[] References { get; set; } = [];

    // inverted index config
    /// <summary>
    /// Gets or sets the value of the inverted index config
    /// </summary>
    public InvertedIndexConfig? InvertedIndexConfig { get; set; }

    /// <summary>
    /// Gets or sets the value of the reranker config
    /// </summary>
    public IRerankerConfig? RerankerConfig { get; set; }

    /// <summary>
    /// Gets or sets the value of the generative config
    /// </summary>
    public IGenerativeConfig? GenerativeConfig { get; set; }

    // multi tenancy config
    /// <summary>
    /// Gets or sets the value of the multi tenancy config
    /// </summary>
    public MultiTenancyConfig? MultiTenancyConfig { get; set; }

    // replication config
    /// <summary>
    /// Gets or sets the value of the replication config
    /// </summary>
    public ReplicationConfig? ReplicationConfig { get; set; }

    // Manage how the index should be sharded and distributed in the cluster
    /// <summary>
    /// Gets or sets the value of the sharding config
    /// </summary>
    public ShardingConfig? ShardingConfig { get; set; }

    // Configure named vectors. Either use this field or `vectorizer`, `vectorIndexType`, and `vectorIndexConfig` fields. Available from `v1.24.0`.
    /// <summary>
    /// Gets or sets the value of the vector config
    /// </summary>
    public VectorConfigList VectorConfig { get; set; } = default!;

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>The int</returns>
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

    /// <summary>
    /// Equalses the other
    /// </summary>
    /// <param name="other">The other</param>
    /// <returns>The bool</returns>
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

/// <summary>
/// The collection create params
/// </summary>
public partial record CollectionCreateParams : CollectionConfigCommon { }

/// <summary>
/// The collection config export
/// </summary>
public partial record CollectionConfigExport : CollectionConfig
{
    /// <summary>
    /// Returns the collection config create params
    /// </summary>
    /// <exception cref="WeaviateClientException">Cannot convert CollectionConfigExport with legacy settings to CollectionCreateParams.</exception>
    /// <returns>The collection create params</returns>
    public CollectionCreateParams ToCollectionConfigCreateParams()
    {
        if (this.VectorIndexType != "" || this.VectorIndexConfig != null || this.Vectorizer != "")
        {
            throw new WeaviateClientException(
                "Cannot convert CollectionConfigExport with legacy settings to CollectionCreateParams."
            );
        }

        return new CollectionCreateParams
        {
            Name = this.Name,
            Description = this.Description,
            Properties = this.Properties,
            References = this.References,
            InvertedIndexConfig = this.InvertedIndexConfig,
            MultiTenancyConfig = this.MultiTenancyConfig,
            ReplicationConfig = this.ReplicationConfig,
            ShardingConfig = this.ShardingConfig,
            VectorConfig = this.VectorConfig,
            GenerativeConfig = this.GenerativeConfig,
            RerankerConfig = this.RerankerConfig,
        };
    }
}

/// <summary>
/// The collection config
/// </summary>
public partial record CollectionConfig : CollectionConfigCommon
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionConfig"/> class
    /// </summary>
    internal CollectionConfig()
        : base() { }

    /// <summary>
    /// Creates the collection create using the specified config
    /// </summary>
    /// <param name="config">The config</param>
    /// <returns>The collection config</returns>
    internal static CollectionConfig FromCollectionCreate(CollectionCreateParams config)
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
    /// <summary>
    /// Gets or sets the value of the module config
    /// </summary>
    public ModuleConfigList? ModuleConfig { get; set; }

    // Vector-index config, that is specific to the type of index selected in vectorIndexType
    /// <summary>
    /// Gets or sets the value of the vector index config
    /// </summary>
    public object? VectorIndexConfig { get; internal set; }

    // Name of the vector index to use, eg. (HNSW)
    /// <summary>
    /// Gets or sets the value of the vector index type
    /// </summary>
    public string VectorIndexType { get; internal set; } = default!;

    /// <summary>
    /// Gets or sets the value of the vectorizer
    /// </summary>
    public string Vectorizer { get; internal set; } = "";

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>The int</returns>
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

    /// <summary>
    /// Equalses the other
    /// </summary>
    /// <param name="other">The other</param>
    /// <returns>The bool</returns>
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

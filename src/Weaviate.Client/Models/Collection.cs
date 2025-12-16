namespace Weaviate.Client.Models;

/// <summary>
/// Base class for collection configuration, containing common settings shared between creation and update operations.
/// </summary>
/// <remarks>
/// A collection in Weaviate is similar to a table in traditional databases or a collection in MongoDB.
/// It defines the schema (properties and references), indexing behavior, and various configurations
/// for storing and searching data objects.
/// </remarks>
public abstract record CollectionConfigCommon
{
    /// <summary>
    /// Gets or sets the name of the collection. Must be unique within the Weaviate instance.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the description of the collection, explaining its purpose and contents.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the properties (data fields) defined for this collection.
    /// Properties define the schema for data objects stored in the collection.
    /// </summary>
    public Property[] Properties { get; set; } = [];

    /// <summary>
    /// Gets or sets the cross-references to other collections.
    /// References enable relationships between objects in different collections.
    /// </summary>
    public Reference[] References { get; set; } = [];

    /// <summary>
    /// Gets or sets the inverted index configuration for filtering and keyword search.
    /// The inverted index is used for BM25 keyword search and filtering operations.
    /// </summary>
    public InvertedIndexConfig? InvertedIndexConfig { get; set; }

    /// <summary>
    /// Gets or sets the reranker configuration for improving search result quality.
    /// Rerankers can reorder search results using more sophisticated models.
    /// </summary>
    public IRerankerConfig? RerankerConfig { get; set; }

    /// <summary>
    /// Gets or sets the generative AI configuration for RAG (Retrieval-Augmented Generation).
    /// Enables integration with LLMs for generating text based on search results.
    /// </summary>
    public IGenerativeConfig? GenerativeConfig { get; set; }

    /// <summary>
    /// Gets or sets the multi-tenancy configuration.
    /// Multi-tenancy enables data isolation for different tenants within the same collection.
    /// </summary>
    public MultiTenancyConfig? MultiTenancyConfig { get; set; }

    /// <summary>
    /// Gets or sets the replication configuration for data redundancy and high availability.
    /// Controls how many copies of data are maintained across the cluster.
    /// </summary>
    public ReplicationConfig? ReplicationConfig { get; set; }

    /// <summary>
    /// Gets or sets the sharding configuration, controlling how data is distributed across cluster nodes.
    /// Sharding enables horizontal scaling of data storage and query performance.
    /// </summary>
    public ShardingConfig? ShardingConfig { get; set; }

    /// <summary>
    /// Gets or sets the named vector configurations for this collection.
    /// Supports multiple vector spaces per collection (available from Weaviate v1.24.0).
    /// Use this for multi-vector setups, or use the legacy single-vector configuration.
    /// </summary>
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

public partial record CollectionCreateParams : CollectionConfigCommon { }

public partial record CollectionConfigExport : CollectionConfig
{
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

public partial record CollectionConfig : CollectionConfigCommon
{
    internal CollectionConfig()
        : base() { }

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

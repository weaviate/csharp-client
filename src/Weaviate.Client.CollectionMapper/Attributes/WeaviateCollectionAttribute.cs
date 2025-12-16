namespace Weaviate.Client.CollectionMapper.Attributes;

/// <summary>
/// Defines a Weaviate collection schema on a C# class.
/// Use this attribute to specify collection-level configuration.
/// </summary>
/// <example>
/// <code>
/// // Basic collection
/// [WeaviateCollection("Articles", Description = "Blog articles")]
/// public class Article { }
///
/// // Multi-tenant collection
/// [WeaviateCollection("Products",
///     MultiTenancyEnabled = true,
///     AutoTenantCreation = true,
///     AutoTenantActivation = true)]
/// public class Product { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class WeaviateCollectionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the collection name. If not specified, the class name will be used.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the collection description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether multi-tenancy is enabled for this collection.
    /// WARNING: This is immutable and cannot be changed after collection creation.
    /// If not set, multi-tenancy will be disabled (default).
    /// </summary>
    public bool? MultiTenancyEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether tenants should be automatically created when referenced.
    /// Only applies if MultiTenancyEnabled is true.
    /// This setting can be updated after collection creation.
    /// </summary>
    public bool? AutoTenantCreation { get; set; }

    /// <summary>
    /// Gets or sets whether tenants should be automatically activated when created.
    /// Only applies if MultiTenancyEnabled is true.
    /// This setting can be updated after collection creation.
    /// </summary>
    public bool? AutoTenantActivation { get; set; }

    #region Sharding Configuration

    /// <summary>
    /// Gets or sets the desired number of shards.
    /// WARNING: This is immutable and cannot be changed after collection creation.
    /// Set to -1 to use Weaviate default (1).
    /// </summary>
    public int ShardingDesiredCount { get; set; } = -1;

    /// <summary>
    /// Gets or sets the virtual shards per physical shard.
    /// Set to -1 to use Weaviate default (128).
    /// </summary>
    public int ShardingVirtualPerPhysical { get; set; } = -1;

    /// <summary>
    /// Gets or sets the desired virtual shard count.
    /// Set to -1 to use Weaviate default (128).
    /// </summary>
    public int ShardingDesiredVirtualCount { get; set; } = -1;

    /// <summary>
    /// Gets or sets the sharding key (property name to shard on).
    /// Default: "_id" (if not specified)
    /// </summary>
    public string? ShardingKey { get; set; }

    #endregion

    #region Replication Configuration

    /// <summary>
    /// Gets or sets the replication factor (number of replicas).
    /// WARNING: This is immutable and cannot be changed after collection creation.
    /// Set to -1 to use Weaviate default (1, no replication).
    /// </summary>
    public int ReplicationFactor { get; set; } = -1;

    /// <summary>
    /// Gets or sets whether asynchronous replication is enabled.
    /// Set to true to enable, false to disable.
    /// </summary>
    public bool ReplicationAsyncEnabled { get; set; } = false;

    #endregion

    #region Advanced Configuration

    /// <summary>
    /// Gets or sets the name of a static method that provides complete control over CollectionConfig.
    /// This is an escape hatch for advanced scenarios not covered by attributes.
    /// The method signature must be: static CollectionConfig MethodName(CollectionConfig prebuilt)
    /// The method receives a pre-built config with all attribute properties already set.
    /// If ConfigMethodClass is not specified, the method is looked up in the same class.
    /// </summary>
    /// <example>
    /// <code>
    /// // Same class
    /// [WeaviateCollection(
    ///     "Articles",
    ///     CollectionConfigMethod = nameof(CustomizeConfig)
    /// )]
    /// public class Article
    /// {
    ///     public static CollectionConfig CustomizeConfig(
    ///         CollectionConfig prebuilt)
    ///     {
    ///         // Customize any aspect of the config
    ///         prebuilt.InvertedIndexConfig = new InvertedIndexConfig
    ///         {
    ///             Bm25 = new BM25Config { K1 = 1.5f, B = 0.75f }
    ///         };
    ///         return prebuilt;
    ///     }
    /// }
    ///
    /// // Different class (type-safe)
    /// [WeaviateCollection(
    ///     "Products",
    ///     CollectionConfigMethod = nameof(CollectionConfigurations.CustomizeProducts),
    ///     ConfigMethodClass = typeof(CollectionConfigurations)
    /// )]
    /// public class Product { }
    /// </code>
    /// </example>
    public string? CollectionConfigMethod { get; set; }

    /// <summary>
    /// Gets or sets the class containing the CollectionConfigMethod.
    /// If not specified, the method is looked up in the same class.
    /// This provides compile-time type safety when referencing methods in different classes.
    /// </summary>
    /// <example>
    /// <code>
    /// [WeaviateCollection(
    ///     "Articles",
    ///     CollectionConfigMethod = nameof(CollectionConfigurations.CustomizeArticles),
    ///     ConfigMethodClass = typeof(CollectionConfigurations)
    /// )]
    /// public class Article { }
    /// </code>
    /// </example>
    public Type? ConfigMethodClass { get; set; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateCollectionAttribute"/> class.
    /// </summary>
    public WeaviateCollectionAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateCollectionAttribute"/> class with a name.
    /// </summary>
    /// <param name="name">The collection name.</param>
    public WeaviateCollectionAttribute(string name)
    {
        Name = name;
    }
}

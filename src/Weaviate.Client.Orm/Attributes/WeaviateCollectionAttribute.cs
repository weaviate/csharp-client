namespace Weaviate.Client.Orm.Attributes;

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

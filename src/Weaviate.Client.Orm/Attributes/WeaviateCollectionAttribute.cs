namespace Weaviate.Client.Orm.Attributes;

/// <summary>
/// Defines a Weaviate collection schema on a C# class.
/// Use this attribute to specify collection-level configuration.
/// </summary>
/// <example>
/// <code>
/// [WeaviateCollection("Articles", Description = "Blog articles")]
/// public class Article
/// {
///     [Property(DataType.Text)]
///     public string Title { get; set; }
/// }
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

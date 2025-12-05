namespace Weaviate.Client.Orm.Attributes;

/// <summary>
/// Defines a cross-reference to another Weaviate collection.
/// The property can be a single object, a Guid (ID only), or a List of objects.
/// </summary>
/// <example>
/// <code>
/// // Single reference with full object
/// [Reference("Category")]
/// public Category? Category { get; set; }
///
/// // Reference with ID only
/// [Reference("Author")]
/// public Guid? AuthorId { get; set; }
///
/// // Multi-reference
/// [Reference("Article")]
/// public List&lt;Article&gt;? RelatedArticles { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ReferenceAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the target collection.
    /// </summary>
    public string TargetCollection { get; }

    /// <summary>
    /// Gets or sets the reference description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceAttribute"/> class.
    /// </summary>
    /// <param name="targetCollection">The target collection name.</param>
    public ReferenceAttribute(string targetCollection)
    {
        TargetCollection = targetCollection;
    }
}

using Weaviate.Client.Models;

namespace Weaviate.Client.Orm.Attributes;

/// <summary>
/// Defines a Weaviate property on a C# class property.
/// This attribute is required for all properties that should be stored in Weaviate.
/// </summary>
/// <example>
/// <code>
/// // Property name inferred from C# property name (Title → title)
/// [Property(DataType.Text, Description = "Article title")]
/// public string Title { get; set; }
///
/// // Custom property name in Weaviate schema
/// [Property(DataType.Text, Name = "article_title")]
/// public string Title { get; set; }
///
/// [Property(DataType.Int)]
/// public int ViewCount { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class PropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the Weaviate data type for this property.
    /// </summary>
    public DataType DataType { get; }

    /// <summary>
    /// Gets or sets the property name in the Weaviate schema.
    /// If not specified, the C# property name will be converted to camelCase.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the property description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
    /// </summary>
    /// <param name="dataType">The Weaviate data type.</param>
    public PropertyAttribute(DataType dataType)
    {
        DataType = dataType;
    }
}

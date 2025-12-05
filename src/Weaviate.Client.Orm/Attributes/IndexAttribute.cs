namespace Weaviate.Client.Orm.Attributes;

/// <summary>
/// Configures indexing options for a Weaviate property.
/// Use this attribute to control filtering, searching, and range filtering capabilities.
/// </summary>
/// <example>
/// <code>
/// [Property(DataType.Text)]
/// [Index(Filterable = true, Searchable = true)]
/// public string Title { get; set; }
///
/// [Property(DataType.Int)]
/// [Index(Filterable = true, RangeFilters = true)]
/// public int Price { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class IndexAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether this property can be used in filters.
    /// </summary>
    public bool Filterable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property supports full-text search.
    /// Only applicable to text properties.
    /// </summary>
    public bool Searchable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property supports range filters (greater than, less than, etc).
    /// Typically used for numeric and date properties.
    /// </summary>
    public bool RangeFilters { get; set; }
}

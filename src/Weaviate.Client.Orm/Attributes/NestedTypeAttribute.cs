namespace Weaviate.Client.Orm.Attributes;

/// <summary>
/// Specifies the nested type for Object or ObjectArray properties.
/// Use this with DataType.Object or DataType.ObjectArray to define the structure of nested objects.
/// </summary>
/// <example>
/// <code>
/// [Property(DataType.Object)]
/// [NestedType(typeof(Address))]
/// public Address ShippingAddress { get; set; }
///
/// [Property(DataType.ObjectArray)]
/// [NestedType(typeof(Comment))]
/// public List&lt;Comment&gt; Comments { get; set; }
///
/// // The nested type itself
/// public class Address
/// {
///     [Property(DataType.Text)]
///     public string Street { get; set; }
///
///     [Property(DataType.Text)]
///     public string City { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class NestedTypeAttribute : Attribute
{
    /// <summary>
    /// Gets the nested type.
    /// </summary>
    public Type NestedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestedTypeAttribute"/> class.
    /// </summary>
    /// <param name="nestedType">The nested type.</param>
    public NestedTypeAttribute(Type nestedType)
    {
        NestedType = nestedType;
    }
}

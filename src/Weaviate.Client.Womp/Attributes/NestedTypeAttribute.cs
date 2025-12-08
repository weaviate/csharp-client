namespace Weaviate.Client.Womp.Attributes;

/// <summary>
/// Specifies the concrete type for polymorphic Object or ObjectArray properties.
/// In most cases, the nested type is automatically inferred from the property type.
/// This attribute is only needed for polymorphic scenarios where the property type is a base class or interface.
/// </summary>
/// <remarks>
/// <para>
/// The ORM automatically infers nested types from property types:
/// - For DataType.Object: Uses the property type directly
/// - For DataType.ObjectArray: Extracts T from List&lt;T&gt;, IList&lt;T&gt;, IEnumerable&lt;T&gt;, etc.
/// </para>
/// <para>
/// Use this attribute for polymorphism when:
/// - Your property is declared as a base class but you want to use a derived type's schema
/// - Your property is declared as an interface but you want to use a concrete implementation's schema
/// - You need to specify which concrete type to use for schema generation in inheritance hierarchies
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Automatic type inference (most common case)
/// [Property(DataType.Object)]
/// public Address ShippingAddress { get; set; }  // Uses Address type directly
///
/// [Property(DataType.ObjectArray)]
/// public List&lt;Comment&gt; Comments { get; set; }  // Uses Comment type from List&lt;T&gt;
///
/// // Polymorphic scenario - property is interface, specify concrete type
/// [Property(DataType.Object)]
/// [NestedType(typeof(EmailAddress))]  // Schema uses EmailAddress, not IContactInfo
/// public IContactInfo ContactInfo { get; set; }
///
/// // Polymorphic scenario - property is base class, specify derived type
/// [Property(DataType.ObjectArray)]
/// [NestedType(typeof(PremiumUser))]  // Schema uses PremiumUser, not BaseUser
/// public List&lt;BaseUser&gt; Users { get; set; }
///
/// // The concrete types
/// public interface IContactInfo { }
/// public class EmailAddress : IContactInfo
/// {
///     [Property(DataType.Text)]
///     public string Email { get; set; }
/// }
///
/// public class BaseUser { }
/// public class PremiumUser : BaseUser
/// {
///     [Property(DataType.Text)]
///     public string MembershipLevel { get; set; }
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

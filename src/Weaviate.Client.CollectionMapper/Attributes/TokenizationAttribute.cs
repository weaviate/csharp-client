using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Attributes;

/// <summary>
/// Specifies the tokenization strategy for a text property.
/// Only applicable to properties with DataType.Text or DataType.TextArray.
/// </summary>
/// <example>
/// <code>
/// [Property(DataType.Text)]
/// [Tokenization(PropertyTokenization.Word)]
/// public string Title { get; set; }
///
/// [Property(DataType.Text)]
/// [Tokenization(PropertyTokenization.Field)]
/// public string Email { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class TokenizationAttribute : Attribute
{
    /// <summary>
    /// Gets the tokenization strategy.
    /// </summary>
    public PropertyTokenization Tokenization { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenizationAttribute"/> class.
    /// </summary>
    /// <param name="tokenization">The tokenization strategy.</param>
    public TokenizationAttribute(PropertyTokenization tokenization)
    {
        Tokenization = tokenization;
    }
}

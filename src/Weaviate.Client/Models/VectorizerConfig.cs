using System.Reflection;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// The vectorizer config
/// </summary>
public abstract record VectorizerConfig : IEquatable<VectorizerConfig>
{
    /// <summary>
    /// The identifier
    /// </summary>
    private readonly string _identifier;

    /// <summary>
    /// The source properties
    /// </summary>
    protected HashSet<string> _sourceProperties = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorizerConfig"/> class
    /// </summary>
    /// <exception cref="InvalidOperationException">VectorizerConfig derived type {GetType().Name} must have a [Vectorizer] attribute.</exception>
    protected VectorizerConfig()
    {
        var attribute =
            GetType().GetCustomAttribute<VectorizerAttribute>()
            ?? throw new InvalidOperationException(
                $"VectorizerConfig derived type {GetType().Name} must have a [Vectorizer] attribute."
            );
        _identifier = attribute.Identifier;
    }

    /// <summary>
    /// Gets or sets the value of the source properties
    /// </summary>
    [JsonConverter(typeof(JsonConverterEmptyCollectionAsNull))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("properties")]
    public ICollection<string>? SourceProperties
    {
        get { return _sourceProperties.Count == 0 ? null : _sourceProperties; }
        set
        {
            if (value == null)
            {
                _sourceProperties.Clear();
                return;
            }

            _sourceProperties = [.. value];
        }
    }

    /// <summary>
    /// Gets the value of the identifier
    /// </summary>
    [JsonIgnore()]
    public string Identifier => _identifier;

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>The int</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_identifier);
        foreach (var property in _sourceProperties)
        {
            hash.Add(property);
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Equalses the other
    /// </summary>
    /// <param name="other">The other</param>
    /// <returns>The bool</returns>
    public virtual bool Equals(VectorizerConfig? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return _identifier == other._identifier
            && _sourceProperties.SetEquals(other._sourceProperties);
    }

    /// <summary>
    /// Returns the dto
    /// </summary>
    /// <returns>A dictionary of string and object</returns>
    public virtual Dictionary<string, object> ToDto()
    {
        return new() { [_identifier] = this };
    }
}

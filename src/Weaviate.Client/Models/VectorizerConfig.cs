using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public abstract record VectorizerConfig : IEquatable<VectorizerConfig>
{
    private readonly string _identifier;
    protected HashSet<string> _sourceProperties = new();

    protected VectorizerConfig(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        _identifier = identifier;
    }

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

    [JsonIgnore()]
    public string Identifier => _identifier;

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

    public virtual bool Equals(VectorizerConfig? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return _identifier == other._identifier
            && _sourceProperties.SetEquals(other._sourceProperties);
    }

    public virtual Dictionary<string, object> ToDto()
    {
        return new() { [_identifier] = this };
    }
}

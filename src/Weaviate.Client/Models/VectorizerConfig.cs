using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public abstract record VectorizerConfig : IEquatable<VectorizerConfig>
{
    private readonly string _identifier;
    protected HashSet<string> _properties = new();

    protected VectorizerConfig(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        _identifier = identifier;
    }

    [JsonConverter(typeof(JsonConverterEmptyCollectionAsNull))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<string>? Properties
    {
        get { return _properties.Count == 0 ? null : _properties; }
        set
        {
            if (value == null)
            {
                _properties.Clear();
                return;
            }

            _properties = [.. value];
        }
    }

    [JsonIgnore()]
    public string Identifier => _identifier;

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_identifier);
        foreach (var property in _properties)
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

        return _identifier == other._identifier && _properties.SetEquals(other._properties);
    }

    public virtual Dictionary<string, object?> ToDto()
    {
        return new() { [_identifier] = this };
    }
}

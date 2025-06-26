using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public abstract record VectorizerConfig
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

    public virtual Dictionary<string, object?> ToDto()
    {
        return new() { [_identifier] = this };
    }
}

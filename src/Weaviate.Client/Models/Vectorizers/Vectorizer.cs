using System.Text.Json.Serialization;

namespace Weaviate.Client.Models.Vectorizers;

public abstract partial record VectorizerConfig
{
    protected readonly string _identifier;
    protected HashSet<string> _properties = new();

    protected VectorizerConfig(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        _identifier = identifier;
    }

    [JsonConverter(typeof(JsonConverterEmptyCollectionAsNull))]
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

    public virtual Dictionary<string, object?> ToDto()
    {
        return new() { [_identifier] = this };
    }
}

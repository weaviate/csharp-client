using Weaviate.Client.Models;

namespace Weaviate.Client.VectorData.Mapping;

/// <summary>
/// Maps between <see cref="Dictionary{String, Object}"/> (dynamic collection records)
/// and Weaviate's storage format.
/// </summary>
internal sealed class DynamicRecordMapper : IWeaviateRecordMapper<Dictionary<string, object?>>
{
    private const string KeyField = "Key";
    private readonly IReadOnlyList<string> _dataPropertyNames;
    private readonly IReadOnlyList<string> _vectorPropertyNames;

    public DynamicRecordMapper(
        IReadOnlyList<string>? dataPropertyNames = null,
        IReadOnlyList<string>? vectorPropertyNames = null
    )
    {
        _dataPropertyNames = dataPropertyNames ?? [];
        _vectorPropertyNames = vectorPropertyNames ?? [];
    }

    public (Guid? Key, IDictionary<string, object?> Properties, Vectors? Vectors) MapToWeaviate(
        Dictionary<string, object?> record
    )
    {
        Guid? key = null;
        if (record.TryGetValue(KeyField, out var keyValue) && keyValue != null)
        {
            key = keyValue switch
            {
                Guid g => g,
                string s => Guid.Parse(s),
                _ => throw new InvalidOperationException(
                    $"Dynamic record key must be Guid or string, got {keyValue.GetType().Name}."
                ),
            };
        }

        var properties = new Dictionary<string, object?>();
        Vectors? vectors = null;

        foreach (var kvp in record)
        {
            if (kvp.Key == KeyField)
                continue;

            // Check if this is a vector property
            if (kvp.Value is float[] floatArray)
            {
                vectors ??= new Vectors();
                vectors.Add(new NamedVector(kvp.Key, (Vector)floatArray));
            }
            else if (kvp.Value is ReadOnlyMemory<float> rom)
            {
                vectors ??= new Vectors();
                vectors.Add(new NamedVector(kvp.Key, (Vector)rom.ToArray()));
            }
            else
            {
                properties[kvp.Key] = kvp.Value;
            }
        }

        return (key, properties, vectors);
    }

    public Dictionary<string, object?> MapFromWeaviate(WeaviateObject weaviateObject)
    {
        var record = new Dictionary<string, object?>();

        if (weaviateObject.UUID.HasValue)
        {
            record[KeyField] = weaviateObject.UUID.Value.ToString();
        }

        foreach (var kvp in weaviateObject.Properties)
        {
            record[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in weaviateObject.Vectors)
        {
            float[] floats = kvp.Value;
            record[kvp.Key] = floats;
        }

        return record;
    }

    public IReadOnlyList<string> GetStoragePropertyNames() => _dataPropertyNames;

    public IReadOnlyList<string> GetVectorPropertyNames() => _vectorPropertyNames;
}

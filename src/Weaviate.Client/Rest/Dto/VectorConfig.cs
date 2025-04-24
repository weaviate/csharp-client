using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest.Dto;

public class VectorConfig
{
    /// <summary>
    /// Vector-index config, that is specific to the type of index selected in vectorIndexType.
    /// </summary>
    [JsonPropertyName("vectorIndexConfig")]
    public IDictionary<string, object> VectorIndexConfig { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Name of the vector index to use, eg. (HNSW).
    /// </summary>
    [JsonPropertyName("vectorIndexType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? VectorIndexType { get; set; }

    /// <summary>
    /// Configuration of a specific vectorizer used by this vector.
    /// </summary>
    [JsonPropertyName("vectorizer")]
    public IDictionary<string, object> Vectorizer { get; set; } = new Dictionary<string, object>();
}
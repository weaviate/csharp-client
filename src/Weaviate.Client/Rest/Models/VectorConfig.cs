namespace Weaviate.Client.Rest.Models;

using System;
using System.Text.Json.Serialization;

public class VectorConfig
{
    /// <summary>
    /// Vector-index config, that is specific to the type of index selected in vectorIndexType.
    /// </summary>
    [JsonPropertyName("vectorIndexConfig")]
    public IDictionary<string, object> VectorIndexConfig { get; set; }

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
    public IDictionary<string, object> Vectorizer { get; set; }
}
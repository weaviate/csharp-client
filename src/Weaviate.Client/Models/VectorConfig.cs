namespace Weaviate.Client.Models;

public class VectorConfig
{
    /// <summary>
    /// Vector-index config, that is specific to the type of index selected in vectorIndexType.
    /// </summary>
    public object? VectorIndexConfig { get; set; }

    /// <summary>
    /// Name of the vector index to use, eg. (HNSW).
    /// </summary>
    public string? VectorIndexType { get; set; }

    /// <summary>
    /// Configuration of a specific vectorizer used by this vector.
    /// </summary>
    public object? Vectorizer { get; set; }
}
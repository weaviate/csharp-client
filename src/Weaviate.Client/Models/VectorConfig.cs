using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Models;

public record VectorConfig
{
    /// <summary>
    /// Vector-index config, that is specific to the type of index selected in vectorIndexType.
    /// </summary>
    public VectorIndexConfig VectorIndexConfig { get; set; } = VectorIndexConfig.Default;

    /// <summary>
    /// Name of the vector index to use, eg. (HNSW).
    /// </summary>
    public string? VectorIndexType => VectorIndexConfig?.Identifier;

    /// <summary>
    /// Configuration of a specific vectorizer used by this vector.
    /// </summary>
    public VectorizerConfig? Vectorizer { get; set; }
}

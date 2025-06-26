namespace Weaviate.Client.Models;

/// <summary>
/// Represents a vector configuration with name, vectorizer, and index settings.
/// </summary>
public record VectorConfig
{
    /// <param name="Name">The name of the vector configuration.</param>
    /// <param name="Vectorizer">Configuration of a specific vectorizer used by this vector.</param>
    /// <param name="VectorIndexConfig">Vector-index config, that is specific to the type of index selected in vectorIndexType.</param>
    public VectorConfig(
        string name,
        VectorizerConfig? vectorizer = null,
        VectorIndexConfig? vectorIndexConfig = null
    )
    {
        Name = name;
        Vectorizer = vectorizer ?? new Vectorizer.None();
        VectorIndexConfig = vectorIndexConfig ?? new VectorIndex.HNSW();
    }

    /// <summary>
    /// Name of the vector index to use, eg. (HNSW).
    /// </summary>
    public string? VectorIndexType => VectorIndexConfig?.Type;

    /// <summary>
    /// Name of the vector configuration.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Configuration of a specific vectorizer used by this vector.
    /// </summary>
    public VectorizerConfig? Vectorizer { get; }

    /// <summary>
    /// Vector-index config, that is specific to the type of index selected in vectorIndexType.
    /// </summary>
    public VectorIndexConfig? VectorIndexConfig { get; }
}

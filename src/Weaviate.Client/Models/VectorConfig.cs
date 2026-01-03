namespace Weaviate.Client.Models;

/// <summary>
/// Represents a vector configuration with name, vectorizer, and index settings.
/// </summary>
public record VectorConfig : IEquatable<VectorConfig>
{
    /// <param name="name">The name of the vector configuration.</param>
    /// <param name="vectorizer">Configuration of a specific vectorizer used by this vector.</param>
    /// <param name="vectorIndexConfig">Vector-index config, that is specific to the type of index selected in vectorIndexType.</param>
    internal VectorConfig(
        string name,
        VectorizerConfig? vectorizer = null,
        VectorIndexConfig? vectorIndexConfig = null
    )
    {
        Name = name;
        Vectorizer = vectorizer ?? new Vectorizer.SelfProvided();
        VectorIndexConfig = vectorIndexConfig;
    }

    /// <summary>
    /// Name of the vector index to use, eg. (HNSW).
    /// </summary>
    public string? VectorIndexType => VectorIndexConfig?.Type;

    /// <summary>
    /// Name of the vector configuration.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Configuration of a specific vectorizer used by this vector.
    /// </summary>
    public VectorizerConfig? Vectorizer { get; }

    /// <summary>
    /// Vector-index config, that is specific to the type of index selected in vectorIndexType.
    /// </summary>
    public VectorIndexConfig? VectorIndexConfig { get; }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public virtual bool Equals(VectorConfig? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Name == other.Name
            && EqualityComparer<VectorizerConfig?>.Default.Equals(Vectorizer, other.Vectorizer)
            && EqualityComparer<VectorIndexConfig?>.Default.Equals(
                VectorIndexConfig,
                other.VectorIndexConfig
            );
    }
}

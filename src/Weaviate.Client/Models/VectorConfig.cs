namespace Weaviate.Client.Models;

/// <summary>
/// Represents a vector configuration with name, vectorizer, and index settings.
/// </summary>
public record VectorConfig : IEquatable<VectorConfig>
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
        Vectorizer = vectorizer ?? new Vectorizer.SelfProvided();
        VectorIndexConfig = vectorIndexConfig;
    }

    public static implicit operator VectorConfig(VectorizerConfig vectorizer)
    {
        return Configure.Vector(vectorizer: vectorizer);
    }

    public static implicit operator VectorConfig(string name)
    {
        return Configure.Vector(
            name: name,
            vectorizer: (Weaviate.Client.Models.VectorizerConfig?)null
        );
    }

    public static implicit operator VectorConfig((string name, VectorizerConfig vectorizer) entry)
    {
        return Configure.Vector(name: entry.name, vectorizer: entry.vectorizer);
    }

    public static implicit operator VectorConfig(
        (string name, VectorizerConfig vectorizer, VectorIndexConfig vectorIndexConfig) entry
    )
    {
        return Configure.Vector(
            name: entry.name,
            vectorizer: entry.vectorizer,
            index: entry.vectorIndexConfig
        );
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

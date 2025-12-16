namespace Weaviate.Client.CollectionMapper.Attributes;

/// <summary>
/// Configures Muvera encoding for multi-vector configurations.
/// Only applicable to multi-vector properties (float[,] or float[][]).
/// Muvera encoding provides compression for multi-vector embeddings (e.g., ColBERT).
/// </summary>
/// <example>
/// <code>
/// // Multi-vector with Muvera encoding (ColBERT-style)
/// [Vector&lt;Vectorizer.SelfProvided&gt;()]
/// [VectorIndex&lt;VectorIndex.HNSW&gt;(Distance = VectorDistance.Cosine)]
/// [Encoding(KSim = 4, DProjections = 16, Repetitions = 10)]
/// public float[,]? ColBERTEmbedding { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class EncodingAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the k-similarity parameter for Muvera encoding.
    /// Default: 4
    /// </summary>
    public double? KSim { get; set; }

    /// <summary>
    /// Gets or sets the number of dimension projections for Muvera encoding.
    /// Default: 16
    /// </summary>
    public double? DProjections { get; set; }

    /// <summary>
    /// Gets or sets the number of repetitions for Muvera encoding.
    /// Default: 10
    /// </summary>
    public double? Repetitions { get; set; }
}

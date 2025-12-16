using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Attributes;

/// <summary>
/// Base class for vector index attributes. Used for runtime type inspection.
/// Do not use this directly - use VectorIndex&lt;TConfig&gt; instead.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class VectorIndexAttributeBase : Attribute
{
    /// <summary>
    /// Gets the vector index config type.
    /// </summary>
    public abstract Type IndexConfigType { get; }

    /// <summary>
    /// Gets or sets the distance metric for vector similarity.
    /// </summary>
    public VectorDistance? Distance { get; set; }
}

/// <summary>
/// Configures the vector index for a vector property.
/// Use the generic type parameter to specify the index type (HNSW, Flat, or Dynamic).
/// </summary>
/// <typeparam name="TIndexConfig">The index configuration type (VectorIndex.HNSW, VectorIndex.Flat, or VectorIndex.Dynamic).</typeparam>
/// <example>
/// <code>
/// // HNSW index with custom parameters
/// [Vector&lt;Vectorizer.Text2VecOpenAI&gt;(...)]
/// [VectorIndex&lt;VectorIndex.HNSW&gt;(
///     Distance = VectorDistance.Cosine,
///     EfConstruction = 256,
///     MaxConnections = 64)]
/// public float[]? ContentEmbedding { get; set; }
///
/// // Flat index
/// [Vector&lt;Vectorizer.SelfProvided&gt;()]
/// [VectorIndex&lt;VectorIndex.Flat&gt;(Distance = VectorDistance.Dot)]
/// public float[]? CustomVector { get; set; }
///
/// // Dynamic index that switches from Flat to HNSW
/// [Vector&lt;Vectorizer.Text2VecCohere&gt;(...)]
/// [VectorIndex&lt;VectorIndex.Dynamic&gt;(Threshold = 5000)]
/// public float[]? TitleVector { get; set; }
/// </code>
/// </example>
public class VectorIndexAttribute<TIndexConfig> : VectorIndexAttributeBase
    where TIndexConfig : VectorIndexConfig
{
    /// <inheritdoc/>
    public override Type IndexConfigType => typeof(TIndexConfig);

    // HNSW-specific properties
    /// <summary>
    /// Gets or sets the efConstruction parameter for HNSW index.
    /// Higher values improve index quality but increase build time. Default: 128
    /// Only applicable to HNSW and Dynamic indexes.
    /// </summary>
    public int? EfConstruction { get; set; }

    /// <summary>
    /// Gets or sets the ef parameter for HNSW index query time.
    /// Higher values improve recall but increase query time. Default: -1 (dynamic)
    /// Only applicable to HNSW and Dynamic indexes.
    /// </summary>
    public int? Ef { get; set; }

    /// <summary>
    /// Gets or sets the maxConnections parameter for HNSW index.
    /// Number of connections per node. Default: 32
    /// Only applicable to HNSW and Dynamic indexes.
    /// </summary>
    public int? MaxConnections { get; set; }

    /// <summary>
    /// Gets or sets the dynamic efMin parameter for HNSW index.
    /// Minimum ef value for dynamic ef. Default: 100
    /// Only applicable to HNSW and Dynamic indexes.
    /// </summary>
    public int? DynamicEfMin { get; set; }

    /// <summary>
    /// Gets or sets the dynamic efMax parameter for HNSW index.
    /// Maximum ef value for dynamic ef. Default: 500
    /// Only applicable to HNSW and Dynamic indexes.
    /// </summary>
    public int? DynamicEfMax { get; set; }

    /// <summary>
    /// Gets or sets the dynamic efFactor parameter for HNSW index.
    /// Multiplier for dynamic ef calculation. Default: 8
    /// Only applicable to HNSW and Dynamic indexes.
    /// </summary>
    public int? DynamicEfFactor { get; set; }

    /// <summary>
    /// Gets or sets the flat search cutoff for HNSW index.
    /// Below this threshold, use brute-force search. Default: 40000
    /// Only applicable to HNSW and Dynamic indexes.
    /// </summary>
    public int? FlatSearchCutoff { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of vectors to cache in memory.
    /// Default: unlimited. Applicable to HNSW, Flat, and Dynamic indexes.
    /// </summary>
    public long? VectorCacheMaxObjects { get; set; }

    // Dynamic-specific properties
    /// <summary>
    /// Gets or sets the threshold for Dynamic index switching.
    /// Number of objects before switching from Flat to HNSW. Default: 10000
    /// Only applicable to Dynamic index.
    /// </summary>
    public int? Threshold { get; set; }
}

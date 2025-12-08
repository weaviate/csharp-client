using Weaviate.Client.Models;

namespace Weaviate.Client.Womp.Attributes;

/// <summary>
/// Base class for quantizer attributes. Used for runtime type inspection.
/// Do not use this directly - use concrete quantizer attributes (QuantizerBQ, QuantizerPQ, etc.) instead.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class QuantizerAttribute : Attribute
{
    /// <summary>
    /// Gets the quantizer type.
    /// </summary>
    public abstract Type QuantizerType { get; }

    /// <summary>
    /// Gets or sets the rescore limit for quantized searches.
    /// Number of candidates to rescore with full precision. Default varies by quantizer.
    /// </summary>
    public int? RescoreLimit { get; set; }
}

/// <summary>
/// Configures Binary Quantization (BQ) for vector compression.
/// Compresses vectors to 1 bit per dimension. Very fast, significant memory savings.
/// Works well with cosine distance.
/// </summary>
/// <example>
/// <code>
/// [Vector&lt;Vectorizer.Text2VecOpenAI&gt;(Model = "text-embedding-ada-002")]
/// [VectorIndex&lt;VectorIndex.HNSW&gt;(Distance = VectorDistance.Cosine)]
/// [QuantizerBQ(RescoreLimit = 200, Cache = true)]
/// public float[]? ContentEmbedding { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class QuantizerBQ : QuantizerAttribute
{
    /// <inheritdoc/>
    public override Type QuantizerType => typeof(VectorIndex.Quantizers.BQ);

    /// <summary>
    /// Gets or sets whether to cache quantized vectors.
    /// Default: false
    /// </summary>
    public bool? Cache { get; set; }
}

/// <summary>
/// Configures Product Quantization (PQ) for vector compression.
/// Good balance of compression and accuracy using clustering.
/// Note: PQ does not support the RescoreLimit parameter (use BQ, SQ, or RQ if you need rescoring).
/// </summary>
/// <example>
/// <code>
/// [Vector&lt;Vectorizer.Text2VecCohere&gt;(Model = "embed-multilingual-v3.0")]
/// [VectorIndex&lt;VectorIndex.HNSW&gt;(Distance = VectorDistance.Cosine)]
/// [QuantizerPQ(
///     Segments = 96,
///     Centroids = 256,
///     EncoderType = PQEncoderType.Kmeans,
///     EncoderDistribution = PQEncoderDistribution.LogNormal)]
/// public float[]? TitleVector { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class QuantizerPQ : QuantizerAttribute
{
    /// <inheritdoc/>
    public override Type QuantizerType => typeof(VectorIndex.Quantizers.PQ);

    /// <summary>
    /// Gets or sets the number of segments for PQ.
    /// Default: 0 (auto)
    /// </summary>
    public int? Segments { get; set; }

    /// <summary>
    /// Gets or sets the number of centroids for PQ.
    /// Default: 256
    /// </summary>
    public int? Centroids { get; set; }

    /// <summary>
    /// Gets or sets whether to use bit compression for PQ.
    /// Default: false
    /// </summary>
    public bool? BitCompression { get; set; }

    /// <summary>
    /// Gets or sets the training limit for quantization.
    /// Default: 100000
    /// </summary>
    public int? TrainingLimit { get; set; }

    /// <summary>
    /// Gets or sets the encoder type for Product Quantization.
    /// Default: Tile (faster training)
    /// </summary>
    public PQEncoderType EncoderType { get; set; } = PQEncoderType.Tile;

    /// <summary>
    /// Gets or sets the encoder distribution for Product Quantization.
    /// Default: LogNormal (better for most embeddings)
    /// </summary>
    public PQEncoderDistribution EncoderDistribution { get; set; } =
        PQEncoderDistribution.LogNormal;
}

/// <summary>
/// Configures Scalar Quantization (SQ) for vector compression.
/// Compresses vectors to 8 bits per dimension. Simple and effective.
/// </summary>
/// <example>
/// <code>
/// [Vector&lt;Vectorizer.SelfProvided&gt;()]
/// [VectorIndex&lt;VectorIndex.HNSW&gt;(Distance = VectorDistance.L2Squared)]
/// [QuantizerSQ(RescoreLimit = 100, TrainingLimit = 100000)]
/// public float[]? CustomVector { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class QuantizerSQ : QuantizerAttribute
{
    /// <inheritdoc/>
    public override Type QuantizerType => typeof(VectorIndex.Quantizers.SQ);

    /// <summary>
    /// Gets or sets the training limit for quantization.
    /// Default: 100000
    /// </summary>
    public int? TrainingLimit { get; set; }
}

/// <summary>
/// Configures Residual Quantization (RQ) for vector compression.
/// Advanced compression technique. Best compression-accuracy tradeoff for large datasets.
/// </summary>
/// <example>
/// <code>
/// [Vector&lt;Vectorizer.Text2VecTransformers&gt;(Model = "sentence-transformers/all-MiniLM-L6-v2")]
/// [VectorIndex&lt;VectorIndex.HNSW&gt;(Distance = VectorDistance.Cosine)]
/// [QuantizerRQ(RescoreLimit = 150, Cache = true, Bits = 8)]
/// public float[]? SentenceEmbedding { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class QuantizerRQ : QuantizerAttribute
{
    /// <inheritdoc/>
    public override Type QuantizerType => typeof(VectorIndex.Quantizers.RQ);

    /// <summary>
    /// Gets or sets whether to cache quantized vectors.
    /// Default: false
    /// </summary>
    public bool? Cache { get; set; }

    /// <summary>
    /// Gets or sets the number of bits for RQ quantization.
    /// </summary>
    public int? Bits { get; set; }
}

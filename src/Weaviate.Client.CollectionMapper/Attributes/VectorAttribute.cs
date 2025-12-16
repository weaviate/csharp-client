using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Attributes;

/// <summary>
/// Vector index types supported by Weaviate.
/// </summary>
public enum VectorIndexType
{
    /// <summary>
    /// HNSW (Hierarchical Navigable Small World) index - default and recommended for most use cases.
    /// Provides excellent recall with good performance.
    /// </summary>
    Hnsw,

    /// <summary>
    /// Flat index - brute-force exact search.
    /// Best for small datasets or when exact results are required.
    /// </summary>
    Flat,

    /// <summary>
    /// Dynamic index - automatically switches from Flat to HNSW based on object count.
    /// Best for collections that start small but may grow large.
    /// </summary>
    Dynamic,
}

/// <summary>
/// Distance metrics for vector similarity.
/// </summary>
public enum VectorDistance
{
    /// <summary>
    /// Cosine similarity - measures angle between vectors (1 - cosine distance).
    /// Range: 0 (identical) to 2 (opposite). Most common choice.
    /// </summary>
    Cosine,

    /// <summary>
    /// Dot product - measures magnitude and direction.
    /// Useful for embeddings that are already normalized.
    /// </summary>
    Dot,

    /// <summary>
    /// L2-squared (Euclidean distance squared) - measures straight-line distance.
    /// Faster than regular L2 distance.
    /// </summary>
    L2Squared,

    /// <summary>
    /// Hamming distance - measures bit differences.
    /// Only for binary vectors.
    /// </summary>
    Hamming,
}

/// <summary>
/// Quantizer types for vector compression.
/// </summary>
public enum QuantizerType
{
    /// <summary>
    /// Binary Quantization - compresses to 1 bit per dimension.
    /// Very fast, significant memory savings, works well with cosine distance.
    /// </summary>
    BQ,

    /// <summary>
    /// Product Quantization - sophisticated compression using clustering.
    /// Good balance of compression and accuracy.
    /// </summary>
    PQ,

    /// <summary>
    /// Scalar Quantization - compresses to 8 bits per dimension.
    /// Simple and effective compression.
    /// </summary>
    SQ,

    /// <summary>
    /// Residual Quantization - advanced compression technique.
    /// Best compression-accuracy tradeoff for large datasets.
    /// </summary>
    RQ,
}

/// <summary>
/// Encoder types for Product Quantization.
/// </summary>
public enum PQEncoderType
{
    /// <summary>
    /// K-means clustering encoder - more accurate but slower training.
    /// </summary>
    Kmeans,

    /// <summary>
    /// Tile encoder - faster training, good for most use cases (default).
    /// </summary>
    Tile,
}

/// <summary>
/// Distribution types for Product Quantization encoder.
/// </summary>
public enum PQEncoderDistribution
{
    /// <summary>
    /// Log-normal distribution - better for most embeddings (default).
    /// </summary>
    LogNormal,

    /// <summary>
    /// Normal (Gaussian) distribution - alternative distribution model.
    /// </summary>
    Normal,
}

/// <summary>
/// Base class for vector attributes. Used for runtime type inspection.
/// Do not use this directly - use VectorAttribute&lt;TVectorizer&gt; instead.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class VectorAttributeBase : Attribute
{
    /// <summary>
    /// Gets the vectorizer type.
    /// </summary>
    public abstract Type VectorizerType { get; }

    /// <summary>
    /// Gets or sets the vector name in the collection schema.
    /// If not specified, the property name (converted to camelCase) will be used.
    /// Useful when working with existing collections that have specific vector names.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the properties to include in vectorization.
    /// Use nameof() for compile-time safety: [nameof(Title), nameof(Content)]
    /// </summary>
    public string[]? SourceProperties { get; set; }

    /// <summary>
    /// Gets or sets whether to include the collection name in vectorization.
    /// </summary>
    public bool VectorizeCollectionName { get; set; }
}

/// <summary>
/// Defines a named vector on a property. The property name becomes the vector name,
/// and the property value will contain the vector embeddings after retrieval.
/// </summary>
/// <typeparam name="TVectorizer">The vectorizer type (e.g., Text2VecOpenAI, Multi2VecClip).</typeparam>
/// <example>
/// <code>
/// // Simple text vectorization
/// [Vector&lt;Vectorizer.Text2VecOpenAI&gt;(
///     Model = "text-embedding-ada-002",
///     SourceProperties = [nameof(Title), nameof(Content)]
/// )]
/// public float[]? TitleContentEmbedding { get; set; }
///
/// // Self-provided vector
/// [Vector&lt;Vectorizer.SelfProvided&gt;()]
/// public float[]? CustomEmbedding { get; set; }
///
/// // Multi-vector (ColBERT-style)
/// [Vector&lt;Vectorizer.SelfProvided&gt;()]
/// public float[,]? ColBERTEmbedding { get; set; }
/// </code>
/// </example>
public class VectorAttribute<TVectorizer> : VectorAttributeBase
    where TVectorizer : VectorizerConfig
{
    /// <inheritdoc/>
    public override Type VectorizerType => typeof(TVectorizer);

    // Common text vectorizer properties
    /// <summary>
    /// Gets or sets the model name. Applicable to most text vectorizers.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the embedding dimensions. Applicable to some vectorizers.
    /// </summary>
    public int? Dimensions { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the vectorizer API. Applicable to some vectorizers.
    /// </summary>
    public string? BaseURL { get; set; }

    // Multi-modal vectorizer properties
    /// <summary>
    /// Gets or sets the text fields for multi-modal vectorization.
    /// </summary>
    public string[]? TextFields { get; set; }

    /// <summary>
    /// Gets or sets the image fields for multi-modal vectorization.
    /// </summary>
    public string[]? ImageFields { get; set; }

    /// <summary>
    /// Gets or sets the video fields for multi-modal vectorization.
    /// </summary>
    public string[]? VideoFields { get; set; }

    // Ref2Vec properties
    /// <summary>
    /// Gets or sets the reference properties for Ref2Vec vectorization.
    /// </summary>
    public string[]? ReferenceProperties { get; set; }

    // Advanced configuration
    /// <summary>
    /// Gets or sets a custom configuration builder type for complex vectorizer setups.
    /// The type must implement IVectorConfigBuilder&lt;TVectorizer&gt;.
    /// </summary>
    public Type? ConfigBuilder { get; set; }

    /// <summary>
    /// Gets or sets the name of a static method that configures the vectorizer.
    /// The method signature must be: static TVectorizer MethodName(string vectorName, TVectorizer prebuilt)
    /// The method receives the vector name and a pre-built vectorizer with properties from the attribute already set.
    /// If ConfigMethodClass is not specified, the method is looked up in the same class as the property.
    /// </summary>
    /// <example>
    /// <code>
    /// // Same class
    /// [Vector&lt;Vectorizer.Text2VecOpenAI&gt;(
    ///     Model = "text-embedding-3-small",
    ///     ConfigMethod = nameof(ConfigureContentVector)
    /// )]
    /// public float[]? ContentEmbedding { get; set; }
    ///
    /// public static Vectorizer.Text2VecOpenAI ConfigureContentVector(
    ///     string vectorName,
    ///     Vectorizer.Text2VecOpenAI prebuilt)
    /// {
    ///     prebuilt.Type = "text";
    ///     prebuilt.VectorizeCollectionName = false;
    ///     return prebuilt;
    /// }
    ///
    /// // Different class (type-safe)
    /// [Vector&lt;Vectorizer.Text2VecOpenAI&gt;(
    ///     Model = "text-embedding-3-small",
    ///     ConfigMethod = nameof(VectorConfigurations.ConfigureOpenAI),
    ///     ConfigMethodClass = typeof(VectorConfigurations)
    /// )]
    /// public float[]? TitleEmbedding { get; set; }
    /// </code>
    /// </example>
    public string? ConfigMethod { get; set; }

    /// <summary>
    /// Gets or sets the class containing the ConfigMethod.
    /// If not specified, the method is looked up in the same class as the property.
    /// This provides compile-time type safety when referencing methods in different classes.
    /// </summary>
    /// <example>
    /// <code>
    /// [Vector&lt;Vectorizer.Text2VecOpenAI&gt;(
    ///     ConfigMethod = nameof(VectorConfigurations.ConfigureOpenAI),
    ///     ConfigMethodClass = typeof(VectorConfigurations)
    /// )]
    /// public float[]? ContentEmbedding { get; set; }
    /// </code>
    /// </example>
    public Type? ConfigMethodClass { get; set; }
}

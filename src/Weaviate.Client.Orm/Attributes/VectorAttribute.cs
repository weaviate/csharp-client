using Weaviate.Client.Models;

namespace Weaviate.Client.Orm.Attributes;

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
}

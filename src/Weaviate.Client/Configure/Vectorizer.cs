using Weaviate.Client.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// The factory class
/// </summary>
internal static partial class Factory
{
    /// <summary>
    /// Gets the value of the vectorizer
    /// </summary>
    public static VectorizerFactory Vectorizer { get; } = new VectorizerFactory();

    /// <summary>
    /// Gets the value of the vectorizer multi
    /// </summary>
    public static VectorizerFactoryMulti VectorizerMulti { get; } = new VectorizerFactoryMulti();

    /// <summary>
    /// Gets the value of the generative provider
    /// </summary>
    public static GenerativeProviderFactory GenerativeProvider { get; } =
        new GenerativeProviderFactory();

    /// <summary>
    /// Gets the value of the generative
    /// </summary>
    public static GenerativeConfigFactory Generative { get; } = new GenerativeConfigFactory();

    /// <summary>
    /// Gets the value of the reranker
    /// </summary>
    public static RerankerConfigFactory Reranker { get; } = new RerankerConfigFactory();
}

/// <summary>
/// Provides static access to generative provider configuration for Weaviate generative queries.
/// </summary>
public static partial class Generate
{
    /// <summary>
    /// Gets the generative provider factory for configuring generative providers.
    /// </summary>
    public static GenerativeProviderFactory Provider => Factory.GenerativeProvider;
}

/// <summary>
/// Provides static access to generative and reranker configuration factories for Weaviate.
/// </summary>
public static partial class Configure
{
    /// <summary>
    /// Gets the generative configuration factory.
    /// </summary>
    public static GenerativeConfigFactory Generative => Factory.Generative;

    /// <summary>
    /// Gets the reranker configuration factory.
    /// </summary>
    public static RerankerConfigFactory Reranker => Factory.Reranker;

    // Note: `vectorizer` is now required on public overloads to make the
    // caller explicitly choose the vectorizer instead of relying on an
    // implicit default.

    /// <summary>
    /// Vectors the vectorizer
    /// </summary>
    /// <param name="vectorizer">The vectorizer</param>
    /// <param name="name">The name</param>
    /// <param name="index">The index</param>
    /// <param name="quantizer">The quantizer</param>
    /// <param name="sourceProperties">The source properties</param>
    /// <returns>The vector config</returns>
    internal static VectorConfig Vector(
        VectorizerConfig vectorizer,
        string? name = null,
        VectorIndexConfig? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        params string[] sourceProperties
    )
    {
        name ??= "default";
        index ??= new VectorIndex.HNSW();

        return new(
            name: name,
            vectorizer: vectorizer with
            {
                SourceProperties = sourceProperties,
            },
            vectorIndexConfig: EnrichVectorIndexConfig(index, quantizer)
        );
    }

    /// <summary>
    /// Multis the vector using the specified vectorizer
    /// </summary>
    /// <param name="vectorizer">The vectorizer</param>
    /// <param name="name">The name</param>
    /// <param name="indexConfig">The index config</param>
    /// <param name="quantizerConfig">The quantizer config</param>
    /// <param name="encoding">The encoding</param>
    /// <param name="sourceProperties">The source properties</param>
    /// <exception cref="WeaviateClientException"></exception>
    /// <exception cref="WeaviateClientException"></exception>
    /// <returns>The vector config</returns>
    internal static VectorConfig MultiVector(
        VectorizerConfig vectorizer,
        string? name = null,
        VectorIndex.HNSW? indexConfig = null,
        VectorIndexConfig.QuantizerConfigBase? quantizerConfig = null,
        VectorIndexConfig.EncodingConfig? encoding = null,
        params string[] sourceProperties
    )
    {
        name ??= "default";
        indexConfig ??= new VectorIndex.HNSW()
        {
            MultiVector = new VectorIndexConfig.MultiVectorConfig(),
        };

        indexConfig.MultiVector ??= new VectorIndexConfig.MultiVectorConfig();

        if (quantizerConfig is not null && indexConfig.Quantizer is not null)
        {
            throw new WeaviateClientException(
                new InvalidOperationException(
                    "Quantizer is already set on the indexConfig. Please provide either the quantizerConfig or set it on the indexConfig, not both."
                )
            );
        }

        if (encoding is not null && indexConfig.MultiVector.Encoding is not null)
        {
            throw new WeaviateClientException(
                new InvalidOperationException(
                    "Encoding is already set on the indexConfig.MultiVector. Please provide either the encoding parameter or set it on the indexConfig.MultiVector, not both."
                )
            );
        }

        indexConfig.MultiVector.Encoding ??= encoding;

        // vectorizer is required by callers; do not provide a default here so missing
        // vectorizer is a compile-time error for callers that omit it.

        return new(
            name,
            vectorizer: vectorizer with
            {
                SourceProperties = sourceProperties,
            },
            vectorIndexConfig: quantizerConfig is null
                ? indexConfig
                : indexConfig with
                {
                    Quantizer = quantizerConfig,
                }
        );
    }

    /// <summary>
    /// Vectors the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="vectorizer">The vectorizer</param>
    /// <param name="index">The index</param>
    /// <param name="quantizer">The quantizer</param>
    /// <param name="sourceProperties">The source properties</param>
    /// <returns>The vector config</returns>
    public static VectorConfig Vector<TVectorizer>(
        string name,
        Func<VectorizerFactory, TVectorizer> vectorizer,
        VectorIndexConfig? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        params string[] sourceProperties
    )
        where TVectorizer : VectorizerConfig =>
        Vector(vectorizer(Factory.Vectorizer), name, index, quantizer, sourceProperties);

    /// <summary>
    /// Vectors the vectorizer
    /// </summary>
    /// <param name="vectorizer">The vectorizer</param>
    /// <param name="index">The index</param>
    /// <param name="quantizer">The quantizer</param>
    /// <param name="sourceProperties">The source properties</param>
    /// <returns>The vector config</returns>
    public static VectorConfig Vector<TVectorizer>(
        Func<VectorizerFactory, TVectorizer> vectorizer,
        VectorIndexConfig? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        params string[] sourceProperties
    )
        where TVectorizer : VectorizerConfig =>
        Vector(vectorizer(Factory.Vectorizer), null, index, quantizer, sourceProperties);

    /// <summary>
    /// Multis the vector using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="vectorizer">The vectorizer</param>
    /// <param name="index">The index</param>
    /// <param name="quantizer">The quantizer</param>
    /// <param name="encoding">The encoding</param>
    /// <param name="sourceProperties">The source properties</param>
    /// <returns>The vector config</returns>
    public static VectorConfig MultiVector<TVectorizer>(
        string name,
        Func<VectorizerFactoryMulti, TVectorizer> vectorizer,
        VectorIndex.HNSW? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        VectorIndexConfig.EncodingConfig? encoding = null,
        params string[] sourceProperties
    )
        where TVectorizer : VectorizerConfig =>
        MultiVector(
            vectorizer(Factory.VectorizerMulti),
            name,
            index,
            quantizer,
            encoding,
            sourceProperties
        );

    /// <summary>
    /// Multis the vector using the specified vectorizer
    /// </summary>
    /// <param name="vectorizer">The vectorizer</param>
    /// <param name="index">The index</param>
    /// <param name="quantizer">The quantizer</param>
    /// <param name="encoding">The encoding</param>
    /// <param name="sourceProperties">The source properties</param>
    /// <returns>The vector config</returns>
    public static VectorConfig MultiVector<TVectorizer>(
        Func<VectorizerFactoryMulti, TVectorizer> vectorizer,
        VectorIndex.HNSW? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        VectorIndexConfig.EncodingConfig? encoding = null,
        params string[] sourceProperties
    )
        where TVectorizer : VectorizerConfig =>
        MultiVector(
            vectorizer(Factory.VectorizerMulti),
            null,
            index,
            quantizer,
            encoding,
            sourceProperties
        );

    /// <summary>
    /// Enriches the provided <see cref="VectorIndexConfig"/> instance with the specified quantizer configuration,
    /// if applicable. The method updates the quantizer property of the index configuration based on its concrete type.
    /// </summary>
    /// <param name="indexConfig">
    /// The vector index configuration to enrich. If <c>null</c>, the method returns <c>null</c>.
    /// </param>
    /// <param name="quantizerConfig">
    /// The quantizer configuration to apply. If <c>null</c>, the original <paramref name="indexConfig"/> is returned unchanged.
    /// </param>
    /// <returns>
    /// The enriched <see cref="VectorIndexConfig"/> instance with the quantizer configuration applied, or the original
    /// <paramref name="indexConfig"/> if no enrichment was possible.
    /// </returns>
    internal static VectorIndexConfig? EnrichVectorIndexConfig(
        VectorIndexConfig? indexConfig,
        VectorIndexConfig.QuantizerConfigBase? quantizerConfig
    )
    {
        if (indexConfig is null)
            return null;

        if (quantizerConfig is null)
            return indexConfig;

        if (indexConfig is VectorIndex.HNSW hnsw)
        {
            if (hnsw.Quantizer != null)
            {
                throw new WeaviateClientException(
                    "HNSW index already has a quantizer configured. Overwriting is not allowed."
                );
            }

            return hnsw with
            {
                Quantizer = quantizerConfig,
            };
        }

        if (indexConfig is VectorIndex.Flat flat)
        {
            if (flat.Quantizer != null)
            {
                throw new WeaviateClientException(
                    "Flat index already has a quantizer configured. Overwriting is not allowed."
                );
            }

            // Only set the Quantizer if it's of type BQ, as Flat supports only BQ quantization.
            if (quantizerConfig is VectorIndex.Quantizers.BQ bq)
            {
                flat.Quantizer = bq;
            }
            else
            {
                throw new WeaviateClientException(
                    "Flat index supports only BQ quantization. Provided quantizer is of type: "
                        + quantizerConfig.GetType().Name
                );
            }
            return flat;
        }

        // Handle the case where the index configuration is of type Dynamic,
        // which may contain both HNSW and Flat sub-configurations.
        if (indexConfig is VectorIndex.Dynamic)
        {
            throw new WeaviateClientException(
                "Dynamic Index must specify quantizers in their respective Vector Index Configurations."
            );
        }

        return indexConfig;
    }
}

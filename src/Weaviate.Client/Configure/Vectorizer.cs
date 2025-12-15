using Weaviate.Client.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static partial class Factory
{
    public static VectorizerFactory Vectorizer { get; } = new VectorizerFactory();
    public static VectorizerFactoryMulti VectorizerMulti { get; } = new VectorizerFactoryMulti();
    public static GenerativeProviderFactory GenerativeProvider { get; } =
        new GenerativeProviderFactory();
    public static GenerativeConfigFactory Generative { get; } = new GenerativeConfigFactory();
    public static RerankerConfigFactory Reranker { get; } = new RerankerConfigFactory();
}

public static partial class Configure
{
    public static GenerativeProviderFactory GenerativeProvider => Factory.GenerativeProvider;
    public static GenerativeConfigFactory Generative => Factory.Generative;
    public static RerankerConfigFactory Reranker => Factory.Reranker;

    public static VectorConfig Vector(string? name = null) =>
        Vector(name, (VectorizerConfig?)null, null, null);

    internal static VectorConfig Vector(
        string? name = null,
        VectorizerConfig? vectorizer = null,
        VectorIndexConfig? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        params string[] sourceProperties
    )
    {
        name ??= "default";
        vectorizer ??= new Models.Vectorizer.SelfProvided();
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

    internal static VectorConfig MultiVector(
        string? name = null,
        VectorizerConfig? vectorizer = null,
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

        vectorizer ??= new Models.Vectorizer.SelfProvided();

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

    public static VectorConfig Vector(
        string name,
        Func<VectorizerFactory, VectorizerConfig>? vectorizer = null,
        VectorIndexConfig? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        params string[] sourceProperties
    ) =>
        Vector(
            name,
            vectorizer is null ? null : vectorizer(Factory.Vectorizer),
            index,
            quantizer,
            sourceProperties
        );

    public static VectorConfig Vector(
        Func<VectorizerFactory, VectorizerConfig>? vectorizer = null,
        VectorIndexConfig? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        params string[] sourceProperties
    ) =>
        Vector(
            null,
            vectorizer is null ? null : vectorizer(Factory.Vectorizer),
            index,
            quantizer,
            sourceProperties
        );

    public static VectorConfig MultiVector(
        string name,
        Func<VectorizerFactoryMulti, VectorizerConfig>? vectorizer = null,
        VectorIndex.HNSW? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        VectorIndexConfig.EncodingConfig? encoding = null,
        params string[] sourceProperties
    ) =>
        MultiVector(
            name,
            vectorizer is null ? null : vectorizer(Factory.VectorizerMulti),
            index,
            quantizer,
            encoding,
            sourceProperties
        );

    public static VectorConfig MultiVector(
        Func<VectorizerFactoryMulti, VectorizerConfig>? vectorizer = null,
        VectorIndex.HNSW? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        VectorIndexConfig.EncodingConfig? encoding = null,
        params string[] sourceProperties
    ) =>
        MultiVector(
            null,
            vectorizer is null ? null : vectorizer(Factory.VectorizerMulti),
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

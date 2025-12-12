using Weaviate.Client.Models;

namespace Weaviate.Client;

public static partial class Configure
{
    public static VectorConfig Vector(string? name = null) =>
        Vector(name, (VectorizerConfig?)null, null, null);

    public static VectorConfig Vector(
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

    public static VectorConfig Vector(
        string? name = null,
        Func<VectorizerFactory, VectorizerConfig>? vectorizer = null,
        VectorIndexConfig? index = null,
        VectorIndexConfig.QuantizerConfigBase? quantizer = null,
        params string[] sourceProperties
    ) =>
        Vector(
            name,
            vectorizer is null ? null : vectorizer(Vectorizer),
            index,
            quantizer,
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

    public static VectorizerFactory Vectorizer { get; } = new VectorizerFactory();
}

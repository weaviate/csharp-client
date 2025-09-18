using Weaviate.Client.Models;

namespace Weaviate.Client;

public static partial class Configure
{
    public static class Vectors
    {
        public static VectorConfig SelfProvided(
            string name = "default",
            VectorIndexConfig? indexConfig = null,
            VectorIndexConfig.QuantizerConfig? quantizerConfig = null
        ) =>
            new VectorConfigBuilder(new Vectorizer.SelfProvided()).New(
                name,
                indexConfig,
                quantizerConfig
            );

        public class VectorConfigBuilder(VectorizerConfig Config)
        {
            public VectorConfig New(
                string name = "default",
                VectorIndexConfig? indexConfig = null,
                VectorIndexConfig.QuantizerConfig? quantizerConfig = null,
                params string[] sourceProperties
            ) =>
                new(
                    name,
                    vectorizer: Config with
                    {
                        SourceProperties = sourceProperties,
                    },
                    vectorIndexConfig: EnrichVectorIndexConfig(indexConfig, quantizerConfig)
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
            private static VectorIndexConfig? EnrichVectorIndexConfig(
                VectorIndexConfig? indexConfig,
                VectorIndexConfig.QuantizerConfig? quantizerConfig
            )
            {
                if (indexConfig is null)
                    return null;

                if (quantizerConfig is null)
                    return indexConfig;

                if (indexConfig is VectorIndex.HNSW hnsw)
                {
                    return hnsw with { Quantizer = quantizerConfig };
                }

                if (indexConfig is VectorIndex.Flat flat)
                {
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
                if (indexConfig is VectorIndex.Dynamic dynamic)
                {
                    // If the Dynamic config has an HNSW sub-config, set its Quantizer directly.
                    if (dynamic.Hnsw is not null)
                        dynamic.Hnsw.Quantizer = quantizerConfig;

                    // If the Dynamic config has a Flat sub-config and the quantizer is of type BQ,
                    // set the Flat's Quantizer property.
                    if (dynamic.Flat is not null && quantizerConfig is VectorIndex.Quantizers.BQ)
                        dynamic.Flat.Quantizer = quantizerConfig as VectorIndex.Quantizers.BQ;

                    // Return the enriched Dynamic config.
                    return dynamic;
                }

                return indexConfig;
            }
        }

        public static VectorConfigBuilder Img2VecNeural(string[] imageFields) =>
            new(new Vectorizer.Img2VecNeural { ImageFields = imageFields });

        public static VectorConfigBuilder Text2VecContextionary(
            bool? vectorizeCollectionName = false
        ) =>
            new(
                new Vectorizer.Text2VecContextionary()
                {
                    VectorizeClassName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecWeaviate(
            string? baseURL = null,
            int? dimensions = null,
            string? model = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecWeaviate
                {
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    Model = model,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2VecClip(
            string[]? imageFields = null,
            string? inferenceUrl = null,
            string[]? textFields = null,
            bool? vectorizeCollectionName = null,
            Vectorizer.Multi2VecWeights? weights = null
        ) =>
            new(
                new Vectorizer.Multi2VecClip
                {
                    ImageFields = imageFields,
                    InferenceUrl = inferenceUrl,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = weights,
                }
            );

        public static VectorConfigBuilder Multi2VecCohere(
            string? baseURL = null,
            string[]? imageFields = null,
            string? model = null,
            string[]? textFields = null,
            string? truncate = null,
            bool? vectorizeCollectionName = null,
            Vectorizer.Multi2VecCohereWeights? weights = null
        ) =>
            new(
                new Vectorizer.Multi2VecCohere
                {
                    BaseURL = baseURL,
                    ImageFields = imageFields,
                    Model = model,
                    TextFields = textFields,
                    Truncate = truncate,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = weights,
                }
            );

        public static VectorConfigBuilder Multi2VecBind(
            string[]? audioFields = null,
            string[]? depthFields = null,
            string[]? imageFields = null,
            string[]? imuFields = null,
            string[]? textFields = null,
            string[]? thermalFields = null,
            string[]? videoFields = null,
            bool? vectorizeCollectionName = null,
            Vectorizer.Multi2VecBindWeights? weights = null
        ) =>
            new(
                new Vectorizer.Multi2VecBind
                {
                    AudioFields = audioFields,
                    DepthFields = depthFields,
                    ImageFields = imageFields,
                    IMUFields = imuFields,
                    TextFields = textFields,
                    ThermalFields = thermalFields,
                    VideoFields = videoFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = weights,
                }
            );

        public static VectorConfigBuilder Multi2VecGoogle(
            string projectId,
            string location,
            string[]? imageFields = null,
            string[]? textFields = null,
            string[]? videoFields = null,
            int? videoIntervalSeconds = null,
            string? modelId = null,
            int? dimensions = null,
            bool? vectorizeCollectionName = null,
            Vectorizer.Multi2VecGoogleWeights? weights = null
        ) =>
            new(
                new Vectorizer.Multi2VecGoogle
                {
                    ProjectId = projectId,
                    Location = location,
                    ImageFields = imageFields,
                    TextFields = textFields,
                    VideoFields = videoFields,
                    VideoIntervalSeconds = videoIntervalSeconds,
                    ModelId = modelId,
                    Dimensions = dimensions,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = weights,
                }
            );

        public static VectorConfigBuilder Multi2VecVoyageAI(
            string? baseURL = null,
            string[]? imageFields = null,
            string? model = null,
            string[]? textFields = null,
            bool? truncate = null,
            bool? vectorizeCollectionName = null,
            Vectorizer.Multi2VecVoyageAIWeights? weights = null
        ) =>
            new(
                new Vectorizer.Multi2VecVoyageAI
                {
                    BaseURL = baseURL,
                    ImageFields = imageFields,
                    Model = model,
                    TextFields = textFields,
                    Truncate = truncate,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = weights,
                }
            );

        public static VectorConfigBuilder Ref2VecCentroid(
            string[] referenceProperties,
            string method = "mean"
        ) =>
            new(
                new Vectorizer.Ref2VecCentroid
                {
                    ReferenceProperties = referenceProperties,
                    Method = method,
                }
            );

        public static VectorConfigBuilder Text2VecAWS(
            string region,
            string service,
            string? endpoint = null,
            string? model = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecAWS
                {
                    Region = region,
                    Service = service,
                    Endpoint = endpoint,
                    Model = model,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecAzureOpenAI(
            string deploymentId,
            string resourceName,
            string? baseURL = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecAzureOpenAI
                {
                    DeploymentId = deploymentId,
                    ResourceName = resourceName,
                    BaseURL = baseURL,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecCohere(
            string? baseURL = null,
            string? model = null,
            string? truncate = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecCohere
                {
                    BaseURL = baseURL,
                    Model = model,
                    Truncate = truncate,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecDatabricks(
            string endpoint,
            string? instruction = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecDatabricks
                {
                    Endpoint = endpoint,
                    Instruction = instruction,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecGPT4All(bool? vectorizeCollectionName = null) =>
            new(
                new Vectorizer.Text2VecGPT4All { VectorizeCollectionName = vectorizeCollectionName }
            );

        public static VectorConfigBuilder Text2VecHuggingFace(
            string? endpointURL = null,
            string? model = null,
            string? passageModel = null,
            string? queryModel = null,
            bool? useCache = null,
            bool? useGPU = null,
            bool? waitForModel = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecHuggingFace
                {
                    EndpointURL = endpointURL,
                    Model = model,
                    PassageModel = passageModel,
                    QueryModel = queryModel,
                    UseCache = useCache,
                    UseGPU = useGPU,
                    WaitForModel = waitForModel,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecNvidia(
            string? baseURL = null,
            string? model = null,
            bool? truncate = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecNvidia
                {
                    BaseURL = baseURL,
                    Model = model,
                    Truncate = truncate,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2VecNvidia(
            string? baseURL = null,
            string? model = null,
            string[]? properties = null,
            bool? truncation = null
        ) =>
            new(
                new Vectorizer.Multi2VecNvidia
                {
                    BaseURL = baseURL,
                    Model = model,
                    SourceProperties = properties,
                    Truncation = truncation,
                }
            );

        public static VectorConfigBuilder Text2VecMistral(
            string? baseURL = null,
            string? model = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecMistral
                {
                    BaseURL = baseURL,
                    Model = model,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecOllama(
            string? apiEndpoint = null,
            string? model = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecOllama
                {
                    ApiEndpoint = apiEndpoint,
                    Model = model,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecOpenAI(
            string? baseURL = null,
            int? dimensions = null,
            string? model = null,
            string? modelVersion = null,
            string? type = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecOpenAI
                {
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    Model = model,
                    ModelVersion = modelVersion,
                    Type = type,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecGoogle(
            string? apiEndpoint = null,
            string? modelId = null,
            string? projectId = null,
            string? titleProperty = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecGoogle
                {
                    ApiEndpoint = apiEndpoint,
                    ModelId = modelId,
                    ProjectId = projectId,
                    TitleProperty = titleProperty,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecTransformers(
            string? inferenceUrl = null,
            string? passageInferenceUrl = null,
            string? queryInferenceUrl = null,
            string? poolingStrategy = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecTransformers
                {
                    InferenceUrl = inferenceUrl,
                    PassageInferenceUrl = passageInferenceUrl,
                    QueryInferenceUrl = queryInferenceUrl,
                    PoolingStrategy = poolingStrategy,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecVoyageAI(
            string? baseURL = null,
            string? model = null,
            bool? truncate = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecVoyageAI
                {
                    BaseURL = baseURL,
                    Model = model,
                    Truncate = truncate,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );
    }
}

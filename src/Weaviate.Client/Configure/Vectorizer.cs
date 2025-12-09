using Weaviate.Client.Models;
using static Weaviate.Client.Models.Vectorizer;

namespace Weaviate.Client;

public record WeightedField(string Name, double Weight)
{
    public static implicit operator WeightedField((string Name, double Weight) tuple) =>
        new(tuple.Name, tuple.Weight);
};

public class WeightedFields : List<WeightedField>
{
    public WeightedFields() { }

    public WeightedFields(IEnumerable<WeightedField> fields)
        : base(fields) { }

    public WeightedFields(params WeightedField[] fields)
        : base(fields) { }

    public string[] FieldNames => [.. this.Select(f => f.Name)];
    public double[] Weights => [.. this.Select(f => f.Weight)];

    public static implicit operator string[]?(WeightedFields? weightedFields) =>
        weightedFields?.FieldNames;
}

public static partial class Configure
{
    public static class Vectors
    {
        public static VectorConfigBuilder SelfProvided() => new(new Vectorizer.SelfProvided());

        public class VectorConfigBuilder(VectorizerConfig Config)
        {
            public VectorConfig New(string name = "default", params string[] sourceProperties) =>
                new(
                    name,
                    vectorizer: Config with
                    {
                        SourceProperties = sourceProperties,
                    },
                    vectorIndexConfig: null
                );

            public VectorConfig New(
                string name,
                VectorIndex.HNSW? indexConfig,
                VectorIndexConfig.QuantizerConfigBase? quantizerConfig = null,
                params string[] sourceProperties
            ) =>
                new(
                    name: string.IsNullOrEmpty(name) ? "default" : name,
                    vectorizer: Config with
                    {
                        SourceProperties = sourceProperties,
                    },
                    vectorIndexConfig: EnrichVectorIndexConfig(indexConfig, quantizerConfig)
                );

            public VectorConfig New(
                string name,
                VectorIndex.Flat? indexConfig,
                VectorIndexConfig.QuantizerConfigFlat? quantizerConfig = null,
                params string[] sourceProperties
            ) =>
                new(
                    name: string.IsNullOrEmpty(name) ? "default" : name,
                    vectorizer: Config with
                    {
                        SourceProperties = sourceProperties,
                    },
                    vectorIndexConfig: EnrichVectorIndexConfig(indexConfig, quantizerConfig)
                );

            public VectorConfig New(
                string name,
                VectorIndex.Dynamic? indexConfig,
                params string[] sourceProperties
            ) =>
                new(
                    name: string.IsNullOrEmpty(name) ? "default" : name,
                    vectorizer: Config with
                    {
                        SourceProperties = sourceProperties,
                    },
                    vectorIndexConfig: indexConfig
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

        public static VectorConfigBuilder Img2VecNeural(string[] imageFields) =>
            new(new Vectorizer.Img2VecNeural { ImageFields = imageFields });

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

        public static VectorConfigBuilder Multi2VecAWS(
            string? region = null,
            string? model = null,
            int? dimensions = null,
            WeightedFields? imageFields = null,
            WeightedFields? textFields = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2VecAWS
                {
                    Region = region,
                    Model = model,
                    Dimensions = dimensions,
                    ImageFields = imageFields,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
                }
            );

        public static VectorConfigBuilder Multi2VecAWS(
            string? region = null,
            string? model = null,
            int? dimensions = null,
            string[]? imageFields = null,
            string[]? textFields = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2VecAWS
                {
                    Region = region,
                    Model = model,
                    Dimensions = dimensions,
                    ImageFields = imageFields,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2VecClip(
            string? inferenceUrl = null,
            WeightedFields? imageFields = null,
            WeightedFields? textFields = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2VecClip
                {
                    ImageFields = imageFields,
                    InferenceUrl = inferenceUrl,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
                }
            );

        public static VectorConfigBuilder Multi2VecClip(
            string? inferenceUrl = null,
            string[]? imageFields = null,
            string[]? textFields = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2VecClip
                {
                    ImageFields = imageFields,
                    InferenceUrl = inferenceUrl,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2VecCohere(
            string? baseURL = null,
            WeightedFields? imageFields = null,
            string? model = null,
            int? dimensions = null,
            WeightedFields? textFields = null,
            string? truncate = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2VecCohere
                {
                    BaseURL = baseURL,
                    ImageFields = imageFields,
                    Model = model,
                    Dimensions = dimensions,
                    TextFields = textFields,
                    Truncate = truncate,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
                }
            );

        public static VectorConfigBuilder Multi2VecCohere(
            string? baseURL = null,
            string[]? imageFields = null,
            string? model = null,
            int? dimensions = null,
            string[]? textFields = null,
            string? truncate = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2VecCohere
                {
                    BaseURL = baseURL,
                    ImageFields = imageFields,
                    Model = model,
                    Dimensions = dimensions,
                    TextFields = textFields,
                    Truncate = truncate,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2VecBind(
            WeightedFields? audioFields = null,
            WeightedFields? depthFields = null,
            WeightedFields? imageFields = null,
            WeightedFields? imuFields = null,
            WeightedFields? textFields = null,
            WeightedFields? thermalFields = null,
            WeightedFields? videoFields = null,
            bool? vectorizeCollectionName = null
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
                    Weights = VectorizerWeights.FromWeightedFields(
                        audioFields,
                        depthFields,
                        imageFields,
                        imuFields,
                        textFields,
                        thermalFields,
                        videoFields
                    ),
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
            bool? vectorizeCollectionName = null
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
                }
            );

        public static VectorConfigBuilder Multi2VecGoogle(
            string projectId,
            string location,
            WeightedFields? imageFields = null,
            WeightedFields? textFields = null,
            WeightedFields? videoFields = null,
            int? videoIntervalSeconds = null,
            string? model = null,
            int? dimensions = null,
            bool? vectorizeCollectionName = null
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
                    ModelId = model,
                    Dimensions = dimensions,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = VectorizerWeights.FromWeightedFields(
                        imageFields,
                        textFields,
                        videoFields
                    ),
                }
            );

        public static VectorConfigBuilder Multi2VecGoogle(
            string projectId,
            string location,
            string[]? imageFields = null,
            string[]? textFields = null,
            string[]? videoFields = null,
            int? videoIntervalSeconds = null,
            string? model = null,
            int? dimensions = null,
            bool? vectorizeCollectionName = null
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
                    ModelId = model,
                    Dimensions = dimensions,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2VecVoyageAI(
            string? baseURL = null,
            WeightedFields? imageFields = null,
            string? model = null,
            WeightedFields? textFields = null,
            bool? truncate = null,
            bool? vectorizeCollectionName = null
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
                    Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
                }
            );

        public static VectorConfigBuilder Multi2VecVoyageAI(
            string? baseURL = null,
            string[]? imageFields = null,
            string? model = null,
            string[]? textFields = null,
            bool? truncate = null,
            bool? vectorizeCollectionName = null
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
            string? targetModel = null,
            string? targetVariant = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecAWS
                {
                    Region = region,
                    Service = service,
                    Endpoint = endpoint,
                    Model = model,
                    TargetModel = targetModel,
                    TargetVariant = targetVariant,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecAzureOpenAI(
            string deploymentId,
            string resourceName,
            string? baseURL = null,
            int? dimensions = null,
            string? model = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecAzureOpenAI
                {
                    DeploymentId = deploymentId,
                    ResourceName = resourceName,
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    Model = model,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecCohere(
            string? baseURL = null,
            string? model = null,
            int? dimensions = null,
            string? truncate = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecCohere
                {
                    BaseURL = baseURL,
                    Model = model,
                    Dimensions = dimensions,
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
            bool? truncate = null
        ) =>
            new(
                new Vectorizer.Multi2VecNvidia
                {
                    BaseURL = baseURL,
                    Model = model,
                    SourceProperties = properties,
                    Truncate = truncate,
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

        public static VectorConfigBuilder Text2VecModel2Vec(
            string? inferenceURL = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecModel2Vec
                {
                    InferenceURL = inferenceURL,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecMorph(
            string? baseURL = null,
            string? model = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecMorph
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

        public static VectorConfigBuilder Text2VecGoogleVertex(
            string? apiEndpoint = null,
            string? model = null,
            string? projectId = null,
            string? titleProperty = null,
            int? dimensions = null,
            string? taskType = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecGoogle
                {
                    ApiEndpoint = apiEndpoint,
                    Model = model,
                    ProjectId = projectId,
                    TitleProperty = titleProperty,
                    Dimensions = dimensions,
                    TaskType = taskType,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecGoogleGemini(
            string? model = null,
            string? titleProperty = null,
            int? dimensions = null,
            string? taskType = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecGoogle
                {
                    ApiEndpoint = null,
                    Model = model,
                    ProjectId = null,
                    TitleProperty = titleProperty,
                    Dimensions = dimensions,
                    TaskType = taskType,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );


        public static VectorConfigBuilder Text2VecTransformers(
            string? inferenceUrl = null,
            string? passageInferenceUrl = null,
            string? queryInferenceUrl = null,
            string? poolingStrategy = null,
            int? dimensions = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecTransformers
                {
                    InferenceUrl = inferenceUrl,
                    PassageInferenceUrl = passageInferenceUrl,
                    QueryInferenceUrl = queryInferenceUrl,
                    PoolingStrategy = poolingStrategy,
                    Dimensions = dimensions,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecVoyageAI(
            string? baseURL = null,
            string? model = null,
            bool? truncate = null,
            int? dimensions = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecVoyageAI
                {
                    BaseURL = baseURL,
                    Model = model,
                    Truncate = truncate,
                    Dimensions = dimensions,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Text2VecJinaAI(
            string? model = null,
            string? baseURL = null,
            int? dimensions = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecJinaAI
                {
                    Model = model,
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2VecJinaAI(
            string? model = null,
            string? baseURL = null,
            int? dimensions = null,
            string[]? imageFields = null,
            string[]? textFields = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2VecJinaAI
                {
                    Model = model,
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    ImageFields = imageFields,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2VecJinaAI(
            string? model = null,
            string? baseURL = null,
            int? dimensions = null,
            WeightedFields? imageFields = null,
            WeightedFields? textFields = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2VecJinaAI
                {
                    Model = model,
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    ImageFields = imageFields,
                    TextFields = textFields,
                    Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );
    }
}

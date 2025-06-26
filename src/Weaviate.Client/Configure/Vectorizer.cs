using Weaviate.Client.Models;

namespace Weaviate.Client;

public static partial class Configure
{
    public static class Vectors
    {
        public static VectorConfig SelfProvided(string name = "default") => new(name);

        public class VectorConfigBuilder(VectorizerConfig Config)
        {
            public VectorConfig New(
                string name,
                VectorIndexConfig? indexConfig = null,
                params string[] properties
            ) =>
                new(
                    name,
                    vectorizer: Config with
                    {
                        Properties = properties,
                    },
                    vectorIndexConfig: indexConfig
                );
        }

        public static VectorConfigBuilder Img2VecNeural(string[] imageFields) =>
            new(new Vectorizer.Img2VecNeural { ImageFields = imageFields });

        public static VectorConfigBuilder Text2VecContextionary(bool? vectorizeClassName = null) =>
            new(new Vectorizer.Text2VecContextionary() { VectorizeClassName = vectorizeClassName });

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

        [Obsolete("Use Multi2VecGoogle instead.")]
        public static VectorConfigBuilder Multi2VecPalm(
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
                new Vectorizer.Multi2VecPalm
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

        public static VectorConfigBuilder Multi2VecJinaAI(
            string? baseURL = null,
            int? dimensions = null,
            string[]? imageFields = null,
            string? model = null,
            string[]? textFields = null,
            bool? vectorizeCollectionName = null,
            Vectorizer.Multi2VecJinaAIWeights? weights = null
        ) =>
            new(
                new Vectorizer.Multi2VecJinaAI
                {
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    ImageFields = imageFields,
                    Model = model,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = weights,
                }
            );

        public static VectorConfigBuilder Multi2VecVoyageAI(
            string? baseURL = null,
            string[]? imageFields = null,
            string? model = null,
            string? outputEncoding = null,
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
                    OutputEncoding = outputEncoding,
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
            bool? truncate = null,
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

        public static VectorConfigBuilder Text2VecJinaAI(
            string? model = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecJinaAI
                {
                    Model = model,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        [Obsolete("Use Text2VecJinaAI instead.")]
        public static VectorConfigBuilder Text2VecJinaConfig(
            string? model = null,
            bool? vectorizeCollectionName = null
        ) => new(new Vectorizer.Text2VecJinaConfig());

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

        [Obsolete("Use Text2VecGoogle instead.")]
        public static VectorConfigBuilder Text2VecPalm(
            string? apiEndpoint = null,
            string? modelId = null,
            string? projectId = null,
            string? titleProperty = null,
            bool? vectorizeCollectionName = null
        ) => new(new Vectorizer.Text2VecPalm());

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

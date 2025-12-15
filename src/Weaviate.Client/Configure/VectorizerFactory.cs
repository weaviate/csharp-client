using Weaviate.Client.Models;
using static Weaviate.Client.Models.Vectorizer;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

public class VectorizerFactory
{
    internal VectorizerFactory() { }

#pragma warning disable CA1822 // Mark members as static
    public VectorizerConfig SelfProvided() => new Models.Vectorizer.SelfProvided();

    public VectorizerConfig Img2VecNeural(string[] imageFields) =>
        new Models.Vectorizer.Img2VecNeural { ImageFields = imageFields };

    public VectorizerConfig Text2VecWeaviate(
        string? baseURL = null,
        int? dimensions = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecWeaviate
        {
            BaseURL = baseURL,
            Dimensions = dimensions,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2VecAWSBedrock(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? region = null,
        string? model = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecAWS
        {
            Region = region,
            Model = model,
            Dimensions = dimensions,
            ImageFields = imageFields,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
        };

    public VectorizerConfig Multi2VecAWSBedrock(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? region = null,
        string? model = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecAWS
        {
            Region = region,
            Model = model,
            Dimensions = dimensions,
            ImageFields = imageFields,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2VecClip(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? inferenceUrl = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecClip
        {
            ImageFields = imageFields,
            InferenceUrl = inferenceUrl,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
        };

    public VectorizerConfig Multi2VecClip(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? inferenceUrl = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecClip
        {
            ImageFields = imageFields,
            InferenceUrl = inferenceUrl,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2VecCohere(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? baseURL = null,
        string? model = null,
        int? dimensions = null,
        string? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecCohere
        {
            BaseURL = baseURL,
            ImageFields = imageFields,
            Model = model,
            Dimensions = dimensions,
            TextFields = textFields,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
        };

    public VectorizerConfig Multi2VecCohere(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? baseURL = null,
        string? model = null,
        int? dimensions = null,
        string? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecCohere
        {
            BaseURL = baseURL,
            ImageFields = imageFields,
            Model = model,
            Dimensions = dimensions,
            TextFields = textFields,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2VecBind(
        WeightedFields imageFields,
        WeightedFields textFields,
        WeightedFields audioFields,
        WeightedFields depthFields,
        WeightedFields imuFields,
        WeightedFields thermalFields,
        WeightedFields videoFields,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecBind
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
                imageFields,
                textFields,
                audioFields,
                depthFields,
                imuFields,
                thermalFields,
                videoFields
            ),
        };

    public VectorizerConfig Multi2VecBind(
        string[]? imageFields = null,
        string[]? textFields = null,
        string[]? audioFields = null,
        string[]? depthFields = null,
        string[]? imuFields = null,
        string[]? thermalFields = null,
        string[]? videoFields = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecBind
        {
            AudioFields = audioFields,
            DepthFields = depthFields,
            ImageFields = imageFields,
            IMUFields = imuFields,
            TextFields = textFields,
            ThermalFields = thermalFields,
            VideoFields = videoFields,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2VecGoogle(
        string projectId,
        string location,
        WeightedFields imageFields,
        WeightedFields textFields,
        WeightedFields videoFields,
        int? videoIntervalSeconds = null,
        string? model = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecGoogle
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
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields, videoFields),
        };

    public VectorizerConfig Multi2VecGoogle(
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
        new Models.Vectorizer.Multi2VecGoogle
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
        };

    public VectorizerConfig Multi2VecVoyageAI(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? baseURL = null,
        string? model = null,
        bool? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecVoyageAI
        {
            BaseURL = baseURL,
            ImageFields = imageFields,
            Model = model,
            TextFields = textFields,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
        };

    public VectorizerConfig Multi2VecVoyageAI(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? baseURL = null,
        string? model = null,
        bool? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecVoyageAI
        {
            BaseURL = baseURL,
            ImageFields = imageFields,
            Model = model,
            TextFields = textFields,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Ref2VecCentroid(string[] referenceProperties, string method = "mean") =>
        new Models.Vectorizer.Ref2VecCentroid
        {
            ReferenceProperties = referenceProperties,
            Method = method,
        };

    public VectorizerConfig Text2VecAWSBedrock(
        string region,
        string model,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecAWS
        {
            Region = region,
            Service = "bedrock",
            Endpoint = null,
            Model = model,
            TargetModel = null,
            TargetVariant = null,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecAWSSagemaker(
        string region,
        string endpoint,
        string? targetModel = null,
        string? targetVariant = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecAWS
        {
            Region = region,
            Service = "sagemaker",
            Endpoint = endpoint,
            Model = null,
            TargetModel = targetModel,
            TargetVariant = targetVariant,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecAzureOpenAI(
        string deploymentId,
        string resourceName,
        string? baseURL = null,
        int? dimensions = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecAzureOpenAI
        {
            DeploymentId = deploymentId,
            ResourceName = resourceName,
            BaseURL = baseURL,
            Dimensions = dimensions,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecCohere(
        string? baseURL = null,
        string? model = null,
        int? dimensions = null,
        string? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecCohere
        {
            BaseURL = baseURL,
            Model = model,
            Dimensions = dimensions,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecDatabricks(
        string endpoint,
        string? instruction = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecDatabricks
        {
            Endpoint = endpoint,
            Instruction = instruction,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecHuggingFace(
        string? endpointURL = null,
        string? model = null,
        string? passageModel = null,
        string? queryModel = null,
        bool? useCache = null,
        bool? useGPU = null,
        bool? waitForModel = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecHuggingFace
        {
            EndpointURL = endpointURL,
            Model = model,
            PassageModel = passageModel,
            QueryModel = queryModel,
            UseCache = useCache,
            UseGPU = useGPU,
            WaitForModel = waitForModel,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecNvidia(
        string? baseURL = null,
        string? model = null,
        bool? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecNvidia
        {
            BaseURL = baseURL,
            Model = model,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2VecNvidia(
        string? baseURL = null,
        string? model = null,
        string[]? properties = null,
        bool? truncate = null
    ) =>
        new Models.Vectorizer.Multi2VecNvidia
        {
            BaseURL = baseURL,
            Model = model,
            SourceProperties = properties,
            Truncate = truncate,
        };

    public VectorizerConfig Text2VecMistral(
        string? baseURL = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecMistral
        {
            BaseURL = baseURL,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecModel2Vec(
        string? inferenceURL = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecModel2Vec
        {
            InferenceURL = inferenceURL,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecMorph(
        string? baseURL = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecMorph
        {
            BaseURL = baseURL,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecOllama(
        string? apiEndpoint = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecOllama
        {
            ApiEndpoint = apiEndpoint,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecOpenAI(
        string? baseURL = null,
        int? dimensions = null,
        string? model = null,
        string? modelVersion = null,
        string? type = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecOpenAI
        {
            BaseURL = baseURL,
            Dimensions = dimensions,
            Model = model,
            ModelVersion = modelVersion,
            Type = type,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecGoogleVertex(
        string? apiEndpoint = null,
        string? model = null,
        string? projectId = null,
        string? titleProperty = null,
        int? dimensions = null,
        string? taskType = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecGoogle
        {
            ApiEndpoint = apiEndpoint,
            Model = model,
            ProjectId = projectId,
            TitleProperty = titleProperty,
            Dimensions = dimensions,
            TaskType = taskType,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecGoogleGemini(
        string? model = null,
        string? titleProperty = null,
        int? dimensions = null,
        string? taskType = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecGoogle
        {
            ApiEndpoint = "generativelanguage.googleapis.com",
            Model = model,
            ProjectId = null,
            TitleProperty = titleProperty,
            Dimensions = dimensions,
            TaskType = taskType,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecTransformers(
        string? inferenceUrl = null,
        string? passageInferenceUrl = null,
        string? queryInferenceUrl = null,
        string? poolingStrategy = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecTransformers
        {
            InferenceUrl = inferenceUrl,
            PassageInferenceUrl = passageInferenceUrl,
            QueryInferenceUrl = queryInferenceUrl,
            PoolingStrategy = poolingStrategy,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecVoyageAI(
        string? baseURL = null,
        string? model = null,
        bool? truncate = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecVoyageAI
        {
            BaseURL = baseURL,
            Model = model,
            Truncate = truncate,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Text2VecJinaAI(
        string? model = null,
        string? baseURL = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2VecJinaAI
        {
            Model = model,
            BaseURL = baseURL,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2VecJinaAI(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? model = null,
        string? baseURL = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecJinaAI
        {
            Model = model,
            BaseURL = baseURL,
            Dimensions = dimensions,
            ImageFields = imageFields,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2VecJinaAI(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? model = null,
        string? baseURL = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2VecJinaAI
        {
            Model = model,
            BaseURL = baseURL,
            Dimensions = dimensions,
            ImageFields = imageFields,
            TextFields = textFields,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
            VectorizeCollectionName = vectorizeCollectionName,
        };
#pragma warning restore CA1822 // Mark members as static
}

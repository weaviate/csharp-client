using Weaviate.Client.Models;
using static Weaviate.Client.Models.Vectorizer;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides factory methods for creating various vectorizer configuration objects used in Weaviate.
/// </summary>
public class VectorizerFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VectorizerFactory"/> class
    /// </summary>
    internal VectorizerFactory() { }

#pragma warning disable CA1822 // Mark members as static
    /// <summary>
    /// Creates a configuration for a self-provided vectorizer.
    /// </summary>
    /// <returns>SelfProvided vectorizer configuration.</returns>
    public VectorizerConfig SelfProvided() => new SelfProvided();

    /// <summary>
    /// Creates a configuration for the Img2VecNeural vectorizer.
    /// </summary>
    /// <param name="imageFields">Array of image field names.</param>
    /// <returns>Img2VecNeural vectorizer configuration.</returns>
    public VectorizerConfig Img2VecNeural(string[] imageFields) =>
        new Img2VecNeural { ImageFields = imageFields };

    /// <summary>
    /// Creates a configuration for the Text2VecWeaviate vectorizer.
    /// </summary>
    /// <param name="baseURL">Optional base URL for the model.</param>
    /// <param name="dimensions">Number of vector dimensions.</param>
    /// <param name="model">Model name to use.</param>
    /// <param name="vectorizeCollectionName">Whether to vectorize the collection name.</param>
    /// <returns>Text2VecWeaviate vectorizer configuration.</returns>
    public VectorizerConfig Text2VecWeaviate(
        string? baseURL = null,
        int? dimensions = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecWeaviate
        {
            BaseURL = baseURL,
            Dimensions = dimensions,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Creates a configuration for the Multi2VecAWSBedrock vectorizer using weighted fields.
    /// </summary>
    /// <param name="imageFields">Weighted image fields.</param>
    /// <param name="textFields">Weighted text fields.</param>
    /// <param name="region">AWS region.</param>
    /// <param name="model">Model name to use.</param>
    /// <param name="dimensions">Number of vector dimensions.</param>
    /// <param name="vectorizeCollectionName">Whether to vectorize the collection name.</param>
    /// <returns>Multi2VecAWSBedrock vectorizer configuration.</returns>
    public VectorizerConfig Multi2VecAWSBedrock(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? region = null,
        string? model = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecAWS
        {
            Region = region,
            Model = model,
            Dimensions = dimensions,
            ImageFields = imageFields,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
        };

    /// <summary>
    /// Creates a configuration for the Multi2VecAWSBedrock vectorizer using string arrays.
    /// </summary>
    /// <param name="imageFields">Array of image field names.</param>
    /// <param name="textFields">Array of text field names.</param>
    /// <param name="region">AWS region.</param>
    /// <param name="model">Model name to use.</param>
    /// <param name="dimensions">Number of vector dimensions.</param>
    /// <param name="vectorizeCollectionName">Whether to vectorize the collection name.</param>
    /// <returns>Multi2VecAWSBedrock vectorizer configuration.</returns>
    public VectorizerConfig Multi2VecAWSBedrock(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? region = null,
        string? model = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecAWS
        {
            Region = region,
            Model = model,
            Dimensions = dimensions,
            ImageFields = imageFields,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Creates a configuration for the Multi2VecClip vectorizer using weighted fields.
    /// </summary>
    /// <param name="imageFields">Weighted image fields.</param>
    /// <param name="textFields">Weighted text fields.</param>
    /// <param name="inferenceUrl">Inference URL for the model.</param>
    /// <param name="vectorizeCollectionName">Whether to vectorize the collection name.</param>
    /// <returns>Multi2VecClip vectorizer configuration.</returns>
    public VectorizerConfig Multi2VecClip(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? inferenceUrl = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecClip
        {
            ImageFields = imageFields,
            InferenceUrl = inferenceUrl,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
        };

    /// <summary>
    /// Creates a configuration for the Multi2VecClip vectorizer using string arrays.
    /// </summary>
    /// <param name="imageFields">Array of image field names.</param>
    /// <param name="textFields">Array of text field names.</param>
    /// <param name="inferenceUrl">Inference URL for the model.</param>
    /// <param name="vectorizeCollectionName">Whether to vectorize the collection name.</param>
    /// <returns>Multi2VecClip vectorizer configuration.</returns>
    public VectorizerConfig Multi2VecClip(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? inferenceUrl = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecClip
        {
            ImageFields = imageFields,
            InferenceUrl = inferenceUrl,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Creates a configuration for the Multi2VecCohere vectorizer using weighted fields.
    /// </summary>
    /// <param name="imageFields">Weighted image fields.</param>
    /// <param name="textFields">Weighted text fields.</param>
    /// <param name="baseURL">Optional base URL for the model.</param>
    /// <param name="model">Model name to use.</param>
    /// <param name="dimensions">Number of vector dimensions.</param>
    /// <param name="truncate">Truncation strategy.</param>
    /// <param name="vectorizeCollectionName">Whether to vectorize the collection name.</param>
    /// <returns>Multi2VecCohere vectorizer configuration.</returns>
    public VectorizerConfig Multi2VecCohere(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? baseURL = null,
        string? model = null,
        int? dimensions = null,
        string? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecCohere
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

    /// <summary>
    /// Creates a configuration for the Multi2VecCohere vectorizer using string arrays.
    /// </summary>
    /// <param name="imageFields">Array of image field names.</param>
    /// <param name="textFields">Array of text field names.</param>
    /// <param name="baseURL">Optional base URL for the model.</param>
    /// <param name="model">Model name to use.</param>
    /// <param name="dimensions">Number of vector dimensions.</param>
    /// <param name="truncate">Truncation strategy.</param>
    /// <param name="vectorizeCollectionName">Whether to vectorize the collection name.</param>
    /// <returns>Multi2VecCohere vectorizer configuration.</returns>
    public VectorizerConfig Multi2VecCohere(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? baseURL = null,
        string? model = null,
        int? dimensions = null,
        string? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecCohere
        {
            BaseURL = baseURL,
            ImageFields = imageFields,
            Model = model,
            Dimensions = dimensions,
            TextFields = textFields,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Creates a configuration for the Multi2VecBind vectorizer using weighted fields for multiple modalities.
    /// </summary>
    /// <param name="imageFields">Weighted image fields.</param>
    /// <param name="textFields">Weighted text fields.</param>
    /// <param name="audioFields">Weighted audio fields.</param>
    /// <param name="depthFields">Weighted depth fields.</param>
    /// <param name="imuFields">Weighted IMU fields.</param>
    /// <param name="thermalFields">Weighted thermal fields.</param>
    /// <param name="videoFields">Weighted video fields.</param>
    /// <param name="vectorizeCollectionName">Whether to vectorize the collection name.</param>
    /// <returns>Multi2VecBind vectorizer configuration.</returns>
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
        new Multi2VecBind
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

    /// <summary>
    /// Multis the 2 vec bind using the specified image fields
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="audioFields">The audio fields</param>
    /// <param name="depthFields">The depth fields</param>
    /// <param name="imuFields">The imu fields</param>
    /// <param name="thermalFields">The thermal fields</param>
    /// <param name="videoFields">The video fields</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
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
        new Multi2VecBind
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

    /// <summary>
    /// Multis the 2 vec google using the specified project id
    /// </summary>
    /// <param name="projectId">The project id</param>
    /// <param name="location">The location</param>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="videoFields">The video fields</param>
    /// <param name="videoIntervalSeconds">The video interval seconds</param>
    /// <param name="model">The model</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
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
        new Multi2VecGoogle
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

    /// <summary>
    /// Multis the 2 vec google using the specified project id
    /// </summary>
    /// <param name="projectId">The project id</param>
    /// <param name="location">The location</param>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="videoFields">The video fields</param>
    /// <param name="videoIntervalSeconds">The video interval seconds</param>
    /// <param name="model">The model</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
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
        new Multi2VecGoogle
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

    /// <summary>
    /// Multi2Vec Google Gemini configuration (using Google AI Studio/Gemini API)
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="videoFields">The video fields</param>
    /// <param name="apiEndpoint">The API endpoint</param>
    /// <param name="videoIntervalSeconds">The video interval seconds</param>
    /// <param name="model">The model</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Multi2VecGoogleGemini(
        WeightedFields imageFields,
        WeightedFields textFields,
        WeightedFields videoFields,
        string? apiEndpoint = null,
        int? videoIntervalSeconds = null,
        string? model = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecGoogleGemini
        {
            ApiEndpoint = apiEndpoint ?? "generativelanguage.googleapis.com",
            ImageFields = imageFields,
            TextFields = textFields,
            VideoFields = videoFields,
            VideoIntervalSeconds = videoIntervalSeconds,
            ModelId = model,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields, videoFields),
        };

    /// <summary>
    /// Multi2Vec Google Gemini configuration (using Google AI Studio/Gemini API)
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="videoFields">The video fields</param>
    /// <param name="apiEndpoint">The API endpoint</param>
    /// <param name="videoIntervalSeconds">The video interval seconds</param>
    /// <param name="model">The model</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Multi2VecGoogleGemini(
        string[]? imageFields = null,
        string[]? textFields = null,
        string[]? videoFields = null,
        string? apiEndpoint = null,
        int? videoIntervalSeconds = null,
        string? model = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecGoogleGemini
        {
            ApiEndpoint = apiEndpoint ?? "generativelanguage.googleapis.com",
            ImageFields = imageFields,
            TextFields = textFields,
            VideoFields = videoFields,
            VideoIntervalSeconds = videoIntervalSeconds,
            ModelId = model,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Multis the 2 vec voyage ai using the specified image fields
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="videoFields">The video fields</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="model">The model</param>
    /// <param name="truncate">The truncate</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Multi2VecVoyageAI(
        WeightedFields imageFields,
        WeightedFields textFields,
        WeightedFields videoFields,
        string? baseURL = null,
        int? dimensions = null,
        string? model = null,
        bool? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecVoyageAI
        {
            BaseURL = baseURL,
            Dimensions = dimensions,
            ImageFields = imageFields,
            Model = model,
            TextFields = textFields,
            VideoFields = videoFields,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(
                imageFields,
                textFields,
                videoFields: videoFields
            ),
        };

    /// <summary>
    /// Multis the 2 vec voyage ai using the specified image fields
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="videoFields">The video fields</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="model">The model</param>
    /// <param name="truncate">The truncate</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Multi2VecVoyageAI(
        string[]? imageFields = null,
        string[]? textFields = null,
        string[]? videoFields = null,
        string? baseURL = null,
        int? dimensions = null,
        string? model = null,
        bool? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecVoyageAI
        {
            BaseURL = baseURL,
            Dimensions = dimensions,
            ImageFields = imageFields,
            VideoFields = videoFields,
            Model = model,
            TextFields = textFields,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Refs the 2 vec centroid using the specified reference properties
    /// </summary>
    /// <param name="referenceProperties">The reference properties</param>
    /// <param name="method">The method</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Ref2VecCentroid(string[] referenceProperties, string method = "mean") =>
        new Ref2VecCentroid { ReferenceProperties = referenceProperties, Method = method };

    /// <summary>
    /// Texts the 2 vec aws bedrock using the specified region
    /// </summary>
    /// <param name="region">The region</param>
    /// <param name="model">The model</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecAWSBedrock(
        string region,
        string model,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecAWS
        {
            Region = region,
            Service = "bedrock",
            Endpoint = null,
            Model = model,
            TargetModel = null,
            TargetVariant = null,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec aws sagemaker using the specified region
    /// </summary>
    /// <param name="region">The region</param>
    /// <param name="endpoint">The endpoint</param>
    /// <param name="targetModel">The target model</param>
    /// <param name="targetVariant">The target variant</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecAWSSagemaker(
        string region,
        string endpoint,
        string? targetModel = null,
        string? targetVariant = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecAWS
        {
            Region = region,
            Service = "sagemaker",
            Endpoint = endpoint,
            Model = null,
            TargetModel = targetModel,
            TargetVariant = targetVariant,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec azure open ai using the specified deployment id
    /// </summary>
    /// <param name="deploymentId">The deployment id</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="model">The model</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecAzureOpenAI(
        string deploymentId,
        string resourceName,
        string? baseURL = null,
        int? dimensions = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecAzureOpenAI
        {
            DeploymentId = deploymentId,
            ResourceName = resourceName,
            BaseURL = baseURL,
            Dimensions = dimensions,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec cohere using the specified base url
    /// </summary>
    /// <param name="baseURL">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="truncate">The truncate</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecCohere(
        string? baseURL = null,
        string? model = null,
        int? dimensions = null,
        string? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecCohere
        {
            BaseURL = baseURL,
            Model = model,
            Dimensions = dimensions,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec databricks using the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint</param>
    /// <param name="instruction">The instruction</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecDatabricks(
        string endpoint,
        string? instruction = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecDatabricks
        {
            Endpoint = endpoint,
            Instruction = instruction,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec hugging face using the specified endpoint url
    /// </summary>
    /// <param name="endpointURL">The endpoint url</param>
    /// <param name="model">The model</param>
    /// <param name="passageModel">The passage model</param>
    /// <param name="queryModel">The query model</param>
    /// <param name="useCache">The use cache</param>
    /// <param name="useGPU">The use gpu</param>
    /// <param name="waitForModel">The wait for model</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
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
        new Text2VecHuggingFace
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

    /// <summary>
    /// Texts the 2 vec nvidia using the specified base url
    /// </summary>
    /// <param name="baseURL">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="truncate">The truncate</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecNvidia(
        string? baseURL = null,
        string? model = null,
        bool? truncate = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecNvidia
        {
            BaseURL = baseURL,
            Model = model,
            Truncate = truncate,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Multis the 2 vec nvidia using the specified base url
    /// </summary>
    /// <param name="baseURL">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="properties">The properties</param>
    /// <param name="truncate">The truncate</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Multi2VecNvidia(
        string? baseURL = null,
        string? model = null,
        string[]? properties = null,
        bool? truncate = null
    ) =>
        new Multi2VecNvidia
        {
            BaseURL = baseURL,
            Model = model,
            SourceProperties = properties,
            Truncate = truncate,
        };

    /// <summary>
    /// Texts the 2 vec mistral using the specified base url
    /// </summary>
    /// <param name="baseURL">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecMistral(
        string? baseURL = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecMistral
        {
            BaseURL = baseURL,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec model 2 vec using the specified inference url
    /// </summary>
    /// <param name="inferenceURL">The inference url</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecModel2Vec(
        string? inferenceURL = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecModel2Vec
        {
            InferenceURL = inferenceURL,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec morph using the specified base url
    /// </summary>
    /// <param name="baseURL">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecMorph(
        string? baseURL = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecMorph
        {
            BaseURL = baseURL,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec ollama using the specified api endpoint
    /// </summary>
    /// <param name="apiEndpoint">The api endpoint</param>
    /// <param name="model">The model</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecOllama(
        string? apiEndpoint = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecOllama
        {
            ApiEndpoint = apiEndpoint,
            Model = model,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec open ai using the specified base url
    /// </summary>
    /// <param name="baseURL">The base url</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="model">The model</param>
    /// <param name="modelVersion">The model version</param>
    /// <param name="type">The type</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecOpenAI(
        string? baseURL = null,
        int? dimensions = null,
        string? model = null,
        string? modelVersion = null,
        string? type = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecOpenAI
        {
            BaseURL = baseURL,
            Dimensions = dimensions,
            Model = model,
            ModelVersion = modelVersion,
            Type = type,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec google vertex using the specified api endpoint
    /// </summary>
    /// <param name="apiEndpoint">The api endpoint</param>
    /// <param name="model">The model</param>
    /// <param name="projectId">The project id</param>
    /// <param name="titleProperty">The title property</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="taskType">The task type</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecGoogleVertex(
        string? apiEndpoint = null,
        string? model = null,
        string? projectId = null,
        string? titleProperty = null,
        int? dimensions = null,
        string? taskType = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecGoogle
        {
            ApiEndpoint = apiEndpoint,
            Model = model,
            ProjectId = projectId,
            TitleProperty = titleProperty,
            Dimensions = dimensions,
            TaskType = taskType,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec google gemini using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <param name="titleProperty">The title property</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="taskType">The task type</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecGoogleGemini(
        string? model = null,
        string? titleProperty = null,
        int? dimensions = null,
        string? taskType = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecGoogle
        {
            ApiEndpoint = "generativelanguage.googleapis.com",
            Model = model,
            ProjectId = null,
            TitleProperty = titleProperty,
            Dimensions = dimensions,
            TaskType = taskType,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec transformers using the specified inference url
    /// </summary>
    /// <param name="inferenceUrl">The inference url</param>
    /// <param name="passageInferenceUrl">The passage inference url</param>
    /// <param name="queryInferenceUrl">The query inference url</param>
    /// <param name="poolingStrategy">The pooling strategy</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecTransformers(
        string? inferenceUrl = null,
        string? passageInferenceUrl = null,
        string? queryInferenceUrl = null,
        string? poolingStrategy = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecTransformers
        {
            InferenceUrl = inferenceUrl,
            PassageInferenceUrl = passageInferenceUrl,
            QueryInferenceUrl = queryInferenceUrl,
            PoolingStrategy = poolingStrategy,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec voyage ai using the specified base url
    /// </summary>
    /// <param name="baseURL">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="truncate">The truncate</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecVoyageAI(
        string? baseURL = null,
        string? model = null,
        bool? truncate = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecVoyageAI
        {
            BaseURL = baseURL,
            Model = model,
            Truncate = truncate,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Texts the 2 vec jina ai using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Text2VecJinaAI(
        string? model = null,
        string? baseURL = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Text2VecJinaAI
        {
            Model = model,
            BaseURL = baseURL,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Multis the 2 vec jina ai using the specified image fields
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="model">The model</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Multi2VecJinaAI(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? model = null,
        string? baseURL = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecJinaAI
        {
            Model = model,
            BaseURL = baseURL,
            Dimensions = dimensions,
            ImageFields = imageFields,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    /// <summary>
    /// Multis the 2 vec jina ai using the specified image fields
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="model">The model</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig Multi2VecJinaAI(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? model = null,
        string? baseURL = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Multi2VecJinaAI
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

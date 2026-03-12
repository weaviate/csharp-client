using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// The vectorizer class
/// </summary>
public static class Vectorizer
{
    /// <summary>
    /// Unified weights configuration for multi-media vectorizers.
    /// All fields are optional and will be omitted from JSON when null.
    /// </summary>
    internal record VectorizerWeights
    {
        /// <summary>
        /// Creates the weighted fields using the specified image fields
        /// </summary>
        /// <param name="imageFields">The image fields</param>
        /// <param name="textFields">The text fields</param>
        /// <param name="audioFields">The audio fields</param>
        /// <param name="depthFields">The depth fields</param>
        /// <param name="imuFields">The imu fields</param>
        /// <param name="thermalFields">The thermal fields</param>
        /// <param name="videoFields">The video fields</param>
        /// <returns>The vectorizer weights</returns>
        public static VectorizerWeights FromWeightedFields(
            WeightedFields? imageFields = null,
            WeightedFields? textFields = null,
            WeightedFields? audioFields = null,
            WeightedFields? depthFields = null,
            WeightedFields? imuFields = null,
            WeightedFields? thermalFields = null,
            WeightedFields? videoFields = null
        ) =>
            new()
            {
                ImageFields = imageFields?.Weights,
                TextFields = textFields?.Weights,
                AudioFields = audioFields?.Weights,
                DepthFields = depthFields?.Weights,
                IMUFields = imuFields?.Weights,
                ThermalFields = thermalFields?.Weights,
                VideoFields = videoFields?.Weights,
            };

        /// <summary>
        /// Gets or sets the value of the audio fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? AudioFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the depth fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? DepthFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the imu fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? IMUFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the thermal fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? ThermalFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the video fields
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? VideoFields { get; set; } = null;
    }

    /// <summary>
    /// The self provided
    /// </summary>
    [Vectorizer("none", VectorType.Both)]
    public record SelfProvided : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelfProvided"/> class
        /// </summary>
        [JsonConstructor]
        internal SelfProvided() { }
    }

    /// <summary>
    /// The configuration for image vectorization using a neural network module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("img2vec-neural")]
    public record Img2VecNeural : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Img2VecNeural"/> class
        /// </summary>
        [JsonConstructor]
        internal Img2VecNeural() { }

        /// <summary>
        /// The image fields used when vectorizing. This is a required field and must match the property fields of the collection that are defined as DataType.BLOB.
        /// </summary>
        public required string[] ImageFields { get; set; }
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the AWS module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-aws")]
    public record Multi2VecAWS : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecAWS"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecAWS() { }

        /// <summary>
        /// Gets or sets the value of the region
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the CLIP module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-clip")]
    public record Multi2VecClip : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecClip"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecClip() { }

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the inference url
        /// </summary>
        public string? InferenceUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Cohere module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-cohere")]
    public record Multi2VecCohere : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecCohere"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecCohere() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the truncate
        /// </summary>
        public string? Truncate { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Bind module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-bind")]
    public record Multi2VecBind : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecBind"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecBind() { }

        /// <summary>
        /// Gets or sets the value of the audio fields
        /// </summary>
        public string[]? AudioFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the depth fields
        /// </summary>
        public string[]? DepthFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the imu fields
        /// </summary>
        public string[]? IMUFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the thermal fields
        /// </summary>
        public string[]? ThermalFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the video fields
        /// </summary>
        public string[]? VideoFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Google module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-palm")]
    public record Multi2VecGoogle : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecGoogle"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecGoogle() { }

        /// <summary>
        /// Gets or sets the value of the project id
        /// </summary>
        public required string ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the value of the location
        /// </summary>
        public required string Location { get; set; }

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the video fields
        /// </summary>
        public string[]? VideoFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the audio fields
        /// </summary>
        public string[]? AudioFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the video interval seconds
        /// </summary>
        public int? VideoIntervalSeconds { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model id
        /// </summary>
        public string? ModelId { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// Deprecated. Use Multi2VecGoogle instead.
    /// </summary>
    public record Multi2VecPalm : Multi2VecGoogle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecPalm"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecPalm() { }
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Google Gemini module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-google-gemini")]
    public record Multi2VecGoogleGemini : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecGoogleGemini"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecGoogleGemini() { }

        /// <summary>
        /// Gets or sets the value of the api endpoint
        /// </summary>
        public string? ApiEndpoint { get; set; } = "generativelanguage.googleapis.com";

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the video fields
        /// </summary>
        public string[]? VideoFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the audio fields
        /// </summary>
        public string[]? AudioFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the video interval seconds
        /// </summary>
        public int? VideoIntervalSeconds { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        [JsonPropertyName("modelId")]
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-jinaai")]
    public record Multi2VecJinaAI : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecJinaAI"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecJinaAI() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media multi-vector vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2multivec-jinaai", VectorType.MultiVector)]
    public record Multi2MultiVecJinaAI : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2MultiVecJinaAI"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2MultiVecJinaAI() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media multi-vector vectorization using the Weaviate module.
    /// Accepts only image fields. See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2multivec-weaviate", VectorType.MultiVector)]
    public record Multi2MultiVecWeaviate : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2MultiVecWeaviate"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2MultiVecWeaviate() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the image fields for vectorization
        /// </summary>
        public string[]? ImageFields { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the VoyageAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-voyageai")]
    public record Multi2VecVoyageAI : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecVoyageAI"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecVoyageAI() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the image fields
        /// </summary>
        public string[]? ImageFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the text fields
        /// </summary>
        public string[]? TextFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the video fields
        /// </summary>
        public string[]? VideoFields { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the truncate
        /// </summary>
        public bool? Truncate { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the weights
        /// </summary>
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for reference-based vectorization using the centroid method.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("ref2vec-centroid")]
    public record Ref2VecCentroid : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ref2VecCentroid"/> class
        /// </summary>
        [JsonConstructor]
        internal Ref2VecCentroid() { }

        /// <summary>
        /// The properties used as reference points for vectorization.
        /// </summary>
        public required string[] ReferenceProperties { get; set; }

        /// <summary>
        /// The method used to calculate the centroid.
        /// </summary>
        public string Method { get; set; } = "mean";
    }

    /// <summary>
    /// The configuration for text vectorization using the AWS module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-aws")]
    public record Text2VecAWS : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecAWS"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecAWS() { }

        /// <summary>
        /// Gets or sets the value of the region
        /// </summary>
        public required string Region { get; set; }

        /// <summary>
        /// Gets or sets the value of the service
        /// </summary>
        public required string Service { get; set; }

        /// <summary>
        /// Gets or sets the value of the endpoint
        /// </summary>
        public string? Endpoint { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the target model
        /// </summary>
        public string? TargetModel { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the target variant
        /// </summary>
        public string? TargetVariant { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the OpenAI module with Azure.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-azure-openai")]
    public record Text2VecAzureOpenAI : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecAzureOpenAI"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecAzureOpenAI() { }

        /// <summary>
        /// Gets or sets the value of the deployment id
        /// </summary>
        public required string DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets the value of the resource name
        /// </summary>
        public required string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Cohere module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-cohere")]
    public record Text2VecCohere : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecCohere"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecCohere() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the truncate
        /// </summary>
        public string? Truncate { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Databricks module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-databricks")]
    public record Text2VecDatabricks : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecDatabricks"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecDatabricks() { }

        /// <summary>
        /// Gets or sets the value of the endpoint
        /// </summary>
        public required string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the value of the instruction
        /// </summary>
        public string? Instruction { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the HuggingFace module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-huggingface")]
    public record Text2VecHuggingFace : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecHuggingFace"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecHuggingFace() { }

        /// <summary>
        /// Gets or sets the value of the endpoint url
        /// </summary>
        public string? EndpointURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the passage model
        /// </summary>
        public string? PassageModel { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the query model
        /// </summary>
        public string? QueryModel { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the use cache
        /// </summary>
        public bool? UseCache { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the use gpu
        /// </summary>
        public bool? UseGPU { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the wait for model
        /// </summary>
        public bool? WaitForModel { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-jinaai")]
    public record Text2VecJinaAI : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecJinaAI"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecJinaAI() { }

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text multi-vector vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2multivec-jinaai", VectorType.MultiVector)]
    public record Text2MultiVecJinaAI : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2MultiVecJinaAI"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2MultiVecJinaAI() { }

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Nvidia module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-nvidia")]
    public record Text2VecNvidia : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecNvidia"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecNvidia() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the truncate
        /// </summary>
        public bool? Truncate { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The multi vec nvidia
    /// </summary>
    [Vectorizer("multi2vec-nvidia")]
    public record Multi2VecNvidia : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Multi2VecNvidia"/> class
        /// </summary>
        [JsonConstructor]
        internal Multi2VecNvidia() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the truncate
        /// </summary>
        public bool? Truncate { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Mistral module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-mistral")]
    public record Text2VecMistral : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecMistral"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecMistral() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Model2Vec module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-model2vec")]
    public record Text2VecModel2Vec : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecModel2Vec"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecModel2Vec() { }

        /// <summary>
        /// Gets or sets the value of the inference url
        /// </summary>
        public string? InferenceURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Morph module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-morph")]
    public record Text2VecMorph : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecMorph"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecMorph() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Ollama module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-ollama")]
    public record Text2VecOllama : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecOllama"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecOllama() { }

        /// <summary>
        /// Gets or sets the value of the api endpoint
        /// </summary>
        public string? ApiEndpoint { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the OpenAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-openai")]
    public record Text2VecOpenAI : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecOpenAI"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecOpenAI() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model version
        /// </summary>
        public string? ModelVersion { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the type
        /// </summary>
        public string? Type { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Google module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-google")]
    public record Text2VecGoogle : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecGoogle"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecGoogle() { }

        /// <summary>
        /// Gets or sets the value of the api endpoint
        /// </summary>
        public string? ApiEndpoint { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the project id
        /// </summary>
        public string? ProjectId { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the title property
        /// </summary>
        public string? TitleProperty { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the task type
        /// </summary>
        public string? TaskType { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// Deprecated. Use Text2VecGoogle instead.
    /// </summary>
    public record Text2VecPalm : Text2VecGoogle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecPalm"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecPalm() { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Transformers module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-transformers")]
    public record Text2VecTransformers : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecTransformers"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecTransformers() { }

        /// <summary>
        /// Gets or sets the value of the inference url
        /// </summary>
        public string? InferenceUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the passage inference url
        /// </summary>
        public string? PassageInferenceUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the query inference url
        /// </summary>
        public string? QueryInferenceUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the pooling strategy
        /// </summary>
        public string? PoolingStrategy { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the VoyageAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-voyageai")]
    public record Text2VecVoyageAI : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecVoyageAI"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecVoyageAI() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the truncate
        /// </summary>
        public bool? Truncate { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using Weaviate's self-hosted text-based embedding models.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-weaviate")]
    public record Text2VecWeaviate : VectorizerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Text2VecWeaviate"/> class
        /// </summary>
        [JsonConstructor]
        internal Text2VecWeaviate() { }

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the dimensions
        /// </summary>
        [JsonConverter(typeof(FlexibleConverter<int>))]
        public int? Dimensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? Model { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the vectorize collection name
        /// </summary>
        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }
}

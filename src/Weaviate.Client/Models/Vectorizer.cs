using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public static class Vectorizer
{
    /// <summary>
    /// Unified weights configuration for multi-media vectorizers.
    /// All fields are optional and will be omitted from JSON when null.
    /// </summary>
    internal record VectorizerWeights
    {
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
                AudioFields = audioFields?.Weights,
                DepthFields = depthFields?.Weights,
                ImageFields = imageFields?.Weights,
                IMUFields = imuFields?.Weights,
                TextFields = textFields?.Weights,
                ThermalFields = thermalFields?.Weights,
                VideoFields = videoFields?.Weights,
            };

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? AudioFields { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? DepthFields { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? ImageFields { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? IMUFields { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? TextFields { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? ThermalFields { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[]? VideoFields { get; set; } = null;
    }

    [Vectorizer("none", VectorType.Both)]
    public record SelfProvided : VectorizerConfig
    {
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
        [JsonConstructor]
        internal Multi2VecAWS() { }

        public string? Region { get; set; }
        public string? Model { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string[]? TextFields { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the CLIP module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-clip")]
    public record Multi2VecClip : VectorizerConfig
    {
        [JsonConstructor]
        internal Multi2VecClip() { }

        public string[]? ImageFields { get; set; } = null;
        public string? InferenceUrl { get; set; } = null;
        public string[]? TextFields { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Cohere module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-cohere")]
    public record Multi2VecCohere : VectorizerConfig
    {
        [JsonConstructor]
        internal Multi2VecCohere() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string? Model { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public string? Truncate { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Bind module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-bind")]
    public record Multi2VecBind : VectorizerConfig
    {
        [JsonConstructor]
        internal Multi2VecBind() { }

        public string[]? AudioFields { get; set; } = null;
        public string[]? DepthFields { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string[]? IMUFields { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public string[]? ThermalFields { get; set; } = null;
        public string[]? VideoFields { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Google module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-palm")]
    public record Multi2VecGoogle : VectorizerConfig
    {
        [JsonConstructor]
        internal Multi2VecGoogle() { }

        public required string ProjectId { get; set; }
        public required string Location { get; set; }
        public string[]? ImageFields { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public string[]? VideoFields { get; set; } = null;
        public int? VideoIntervalSeconds { get; set; } = null;
        public string? ModelId { get; set; } = null;
        public int? Dimensions { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// Deprecated. Use Multi2VecGoogle instead.
    /// </summary>
    public record Multi2VecPalm : Multi2VecGoogle
    {
        [JsonConstructor]
        internal Multi2VecPalm() { }
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-jinaai")]
    public record Multi2VecJinaAI : VectorizerConfig
    {
        [JsonConstructor]
        internal Multi2VecJinaAI() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string? Model { get; set; } = null;
        public string[]? TextFields { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media multi-vector vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2multivec-jinaai", VectorType.MultiVector)]
    public record Multi2MultiVecJinaAI : VectorizerConfig
    {
        [JsonConstructor]
        internal Multi2MultiVecJinaAI() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        internal VectorizerWeights? Weights { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the VoyageAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("multi2vec-voyageai")]
    public record Multi2VecVoyageAI : VectorizerConfig
    {
        [JsonConstructor]
        internal Multi2VecVoyageAI() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string? Model { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public bool? Truncate { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
        internal VectorizerWeights? Weights { get; set; } = null;
    }

    /// <summary>
    /// The configuration for reference-based vectorization using the centroid method.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("ref2vec-centroid")]
    public record Ref2VecCentroid : VectorizerConfig
    {
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
        [JsonConstructor]
        internal Text2VecAWS() { }

        public required string Region { get; set; }
        public required string Service { get; set; }
        public string? Endpoint { get; set; } = null;
        public string? Model { get; set; } = null;
        public string? TargetModel { get; set; } = null;
        public string? TargetVariant { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecAzureOpenAI() { }

        public required string DeploymentId { get; set; }
        public required string ResourceName { get; set; }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string? Model { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecCohere() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string? Truncate { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecDatabricks() { }

        public required string Endpoint { get; set; }
        public string? Instruction { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecHuggingFace() { }

        public string? EndpointURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public string? PassageModel { get; set; } = null;
        public string? QueryModel { get; set; } = null;
        public bool? UseCache { get; set; } = null;
        public bool? UseGPU { get; set; } = null;
        public bool? WaitForModel { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecJinaAI() { }

        public string? Model { get; set; } = null;

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public int? Dimensions { get; set; } = null;

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
        [JsonConstructor]
        internal Text2MultiVecJinaAI() { }

        public string? Model { get; set; } = null;

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public int? Dimensions { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecNvidia() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? Truncate { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    [Vectorizer("multi2vec-nvidia")]
    public record Multi2VecNvidia : VectorizerConfig
    {
        [JsonConstructor]
        internal Multi2VecNvidia() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? Truncate { get; set; } = null;
    }

    /// <summary>
    /// The configuration for text vectorization using the Mistral module.
    /// See the documentation for detailed usage.
    /// </summary>
    [Vectorizer("text2vec-mistral")]
    public record Text2VecMistral : VectorizerConfig
    {
        [JsonConstructor]
        internal Text2VecMistral() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecModel2Vec() { }

        public string? InferenceURL { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecMorph() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecOllama() { }

        public string? ApiEndpoint { get; set; } = null;
        public string? Model { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecOpenAI() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string? Model { get; set; } = null;
        public string? ModelVersion { get; set; } = null;
        public string? Type { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecGoogle() { }

        public string? ApiEndpoint { get; set; } = null;
        public string? Model { get; set; } = null;
        public string? ProjectId { get; set; } = null;
        public string? TitleProperty { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string? TaskType { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    /// <summary>
    /// Deprecated. Use Text2VecGoogle instead.
    /// </summary>
    public record Text2VecPalm : Text2VecGoogle
    {
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
        [JsonConstructor]
        internal Text2VecTransformers() { }

        public string? InferenceUrl { get; set; } = null;
        public string? PassageInferenceUrl { get; set; } = null;
        public string? QueryInferenceUrl { get; set; } = null;
        public string? PoolingStrategy { get; set; } = null;
        public int? Dimensions { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecVoyageAI() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? Truncate { get; set; } = null;
        public int? Dimensions { get; set; } = null;

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
        [JsonConstructor]
        internal Text2VecWeaviate() { }

        [JsonPropertyName("baseUrl")]
        public string? BaseURL { get; set; } = null;

        [JsonConverter(typeof(FlexibleConverter<int>))]
        public int? Dimensions { get; set; } = null;

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? Model { get; set; } = null;

        [JsonPropertyName("vectorizeClassName")]
        public bool? VectorizeCollectionName { get; set; } = null;
    }
}

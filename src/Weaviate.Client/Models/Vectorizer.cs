using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public static partial class Vectorizer
{
    public partial record SelfProvided { }

    public partial record Img2VecNeural
    {
        /// <summary>
        /// The image fields used when vectorizing. This is a required field and must match the property fields of the collection that are defined as DataType.BLOB.
        /// </summary>
        public required string[] ImageFields { get; set; }
    }

    public partial record Multi2VecField
    {
        /// <summary>
        /// The name of the field to be used when performing multi-media vectorization.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The weight of the field when performing multi-media vectorization.
        /// </summary>
        public double? Weight { get; set; } = null;
    }

    public partial record Multi2VecWeights
    {
        public double[]? ImageFields { get; set; } = null;
        public double[]? TextFields { get; set; } = null;
    }

    public partial record Multi2VecClip
    {
        public string[]? ImageFields { get; set; } = null;
        public string? InferenceUrl { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
        public Multi2VecWeights? Weights { get; set; } = null;
    }

    public partial record Multi2VecCohereWeights
    {
        public double[]? ImageFields { get; set; } = null;
        public double[]? TextFields { get; set; } = null;
    }

    public partial record Multi2VecCohere
    {
        public string? BaseURL { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string? Model { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public string? Truncate { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
        public Multi2VecCohereWeights? Weights { get; set; } = null;
    }

    public partial record Multi2VecBindWeights
    {
        public double[]? AudioFields { get; set; } = null;
        public double[]? DepthFields { get; set; } = null;
        public double[]? ImageFields { get; set; } = null;
        public double[]? IMUFields { get; set; } = null;
        public double[]? TextFields { get; set; } = null;
        public double[]? ThermalFields { get; set; } = null;
        public double[]? VideoFields { get; set; } = null;
    }

    public partial record Multi2VecBind
    {
        public string[]? AudioFields { get; set; } = null;
        public string[]? DepthFields { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string[]? IMUFields { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public string[]? ThermalFields { get; set; } = null;
        public string[]? VideoFields { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
        public Multi2VecBindWeights? Weights { get; set; } = null;
    }

    public partial record Multi2VecGoogleWeights
    {
        public double[]? ImageFields { get; set; } = null;
        public double[]? TextFields { get; set; } = null;
        public double[]? VideoFields { get; set; } = null;
    }

    public partial record Multi2VecGoogle
    {
        public required string ProjectId { get; set; }
        public required string Location { get; set; }
        public string[]? ImageFields { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public string[]? VideoFields { get; set; } = null;
        public int? VideoIntervalSeconds { get; set; } = null;
        public string? ModelId { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
        public Multi2VecGoogleWeights? Weights { get; set; } = null;
    }

    public partial record Multi2VecJinaAIWeights
    {
        public double[]? ImageFields { get; set; } = null;
        public double[]? TextFields { get; set; } = null;
    }

    public partial record Multi2VecJinaAI
    {
        public string? BaseURL { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string? Model { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
        public Multi2VecJinaAIWeights? Weights { get; set; } = null;
    }

    public partial record Multi2VecVoyageAIWeights
    {
        public double[]? ImageFields { get; set; } = null;
        public double[]? TextFields { get; set; } = null;
    }

    public partial record Multi2VecVoyageAI
    {
        public string? BaseURL { get; set; } = null;
        public string[]? ImageFields { get; set; } = null;
        public string? Model { get; set; } = null;
        public string[]? TextFields { get; set; } = null;
        public bool? Truncate { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
        public Multi2VecVoyageAIWeights? Weights { get; set; } = null;
    }

    public partial record Ref2VecCentroid
    {
        /// <summary>
        /// The properties used as reference points for vectorization.
        /// </summary>
        public required string[] ReferenceProperties { get; set; }

        /// <summary>
        /// The method used to calculate the centroid.
        /// </summary>
        public string Method { get; set; } = "mean";
    }

    public partial record Text2VecAWS
    {
        public required string Region { get; set; }
        public required string Service { get; set; }
        public string? Endpoint { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecAzureOpenAI
    {
        public required string DeploymentId { get; set; }
        public required string ResourceName { get; set; }
        public string? BaseURL { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecCohere
    {
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string? Truncate { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecContextionary
    {
        public bool? VectorizeClassName { get; set; } = false;
    }

    public partial record Text2VecDatabricks
    {
        public required string Endpoint { get; set; }
        public string? Instruction { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecHuggingFace
    {
        public string? EndpointURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public string? PassageModel { get; set; } = null;
        public string? QueryModel { get; set; } = null;
        public bool? UseCache { get; set; } = null;
        public bool? UseGPU { get; set; } = null;
        public bool? WaitForModel { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecJinaAI
    {
        public string? Model { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    [Obsolete("Use Text2VecJinaAIConfig instead.")]
    public partial record Text2VecJinaConfig
    {
        // Inherits all properties from Text2VecJinaAIConfig
    }

    public partial record Text2VecNvidia
    {
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? Truncate { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Multi2VecNvidia
    {
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? Truncation { get; set; } = null;
    }

    public partial record Text2VecMistral
    {
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecModel2Vec
    {
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecOllama
    {
        public string? ApiEndpoint { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecOpenAI
    {
        public string? BaseURL { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public string? Model { get; set; } = null;
        public string? ModelVersion { get; set; } = null;
        public string? Type { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecGoogle
    {
        public string? ApiEndpoint { get; set; } = null;
        public string? Model { get; set; } = null;
        public string? ProjectId { get; set; } = null;
        public string? TitleProperty { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecTransformers
    {
        public string? InferenceUrl { get; set; } = null;
        public string? PassageInferenceUrl { get; set; } = null;
        public string? QueryInferenceUrl { get; set; } = null;
        public string? PoolingStrategy { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecVoyageAI
    {
        public string? BaseURL { get; set; } = null;
        public string? Model { get; set; } = null;
        public bool? Truncate { get; set; } = null;
        public int? Dimensions { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }

    public partial record Text2VecWeaviate
    {
        public string? BaseURL { get; set; } = null;

        [JsonConverter(typeof(FlexibleConverter<int>))]
        public int? Dimensions { get; set; } = null;

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? Model { get; set; } = null;
        public bool? VectorizeCollectionName { get; set; } = null;
    }
}

using System.Text.Json.Serialization;

namespace Weaviate.Client.Models.Vectorizers;

/// <summary>
/// The base configuration for all vectorization modules.
/// </summary>
public abstract record VectorizerConfig
{
    protected readonly string _identifier;
    private HashSet<string>? _properties = null;

    protected VectorizerConfig(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        _identifier = identifier;
    }

    public VectorizerConfig ForProperties(params string[] properties)
    {
        _properties = properties.Length != 0 ? [.. properties] : null;

        return this;
    }

    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? Properties => _properties;

    public virtual Dictionary<string, object?> ToDto()
    {
        return new() { [_identifier] = this };
    }
}

public record NoneConfig() : VectorizerConfig("none")
{
    public override Dictionary<string, object?> ToDto()
    {
        return new() { [_identifier] = this };
    }
};

/// <summary>
/// The configuration for image vectorization using a neural network module.
/// See the documentation for detailed usage.
/// </summary>
public record Img2VecNeuralConfig : VectorizerConfig
{
    public Img2VecNeuralConfig()
        : base("img2vec-neural") { }

    /// <summary>
    /// The image fields used when vectorizing. This is a required field and must match the property fields of the collection that are defined as DataType.BLOB.
    /// </summary>
    public required string[] ImageFields { get; set; }
}

/// <summary>
/// The field configuration for multi-media vectorization.
/// </summary>
public record Multi2VecField
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

/// <summary>
/// The weights configuration for multi-media vectorization.
/// </summary>
public record Multi2VecWeights
{
    public double[]? ImageFields { get; set; } = null;
    public double[]? TextFields { get; set; } = null;
}

/// <summary>
/// The configuration for multi-media vectorization using the CLIP module.
/// See the documentation for detailed usage.
/// </summary>
public record Multi2VecClipConfig : VectorizerConfig
{
    public Multi2VecClipConfig()
        : base("multi2vec-clip") { }

    public string[]? ImageFields { get; set; } = null;
    public string? InferenceUrl { get; set; } = null;
    public string[]? TextFields { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
    public Multi2VecWeights? Weights { get; set; } = null;
}

/// <summary>
/// The weights configuration for Cohere multi-media vectorization.
/// </summary>
public record Multi2VecCohereWeights
{
    public double[]? ImageFields { get; set; } = null;
    public double[]? TextFields { get; set; } = null;
}

/// <summary>
/// The configuration for multi-media vectorization using the Cohere module.
/// See the documentation for detailed usage.
/// </summary>
public record Multi2VecCohereConfig : VectorizerConfig
{
    public Multi2VecCohereConfig()
        : base("multi2vec-cohere") { }

    public string? BaseURL { get; set; } = null;
    public string[]? ImageFields { get; set; } = null;
    public string? Model { get; set; } = null;
    public string[]? TextFields { get; set; } = null;
    public string? Truncate { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
    public Multi2VecCohereWeights? Weights { get; set; } = null;
}

/// <summary>
/// The weights configuration for Bind multi-media vectorization.
/// </summary>
public record Multi2VecBindWeights
{
    public double[]? AudioFields { get; set; } = null;
    public double[]? DepthFields { get; set; } = null;
    public double[]? ImageFields { get; set; } = null;
    public double[]? IMUFields { get; set; } = null;
    public double[]? TextFields { get; set; } = null;
    public double[]? ThermalFields { get; set; } = null;
    public double[]? VideoFields { get; set; } = null;
}

/// <summary>
/// The configuration for multi-media vectorization using the Bind module.
/// See the documentation for detailed usage.
/// </summary>
public record Multi2VecBindConfig : VectorizerConfig
{
    public Multi2VecBindConfig()
        : base("multi2vec-bind") { }

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

/// <summary>
/// The weights configuration for Google multi-media vectorization.
/// </summary>
public record Multi2VecGoogleWeights
{
    public double[]? ImageFields { get; set; } = null;
    public double[]? TextFields { get; set; } = null;
    public double[]? VideoFields { get; set; } = null;
}

/// <summary>
/// The configuration for multi-media vectorization using the Google module.
/// See the documentation for detailed usage.
/// </summary>
public record Multi2VecGoogleConfig : VectorizerConfig
{
    public Multi2VecGoogleConfig()
        : base("multi2vec-palm") { }

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

/// <summary>
/// Deprecated. Use Multi2VecGoogleConfig instead.
/// </summary>
[Obsolete("Use Multi2VecGoogleConfig instead.")]
public record Multi2VecPalmConfig : Multi2VecGoogleConfig
{
    // Inherits all properties from Multi2VecGoogleConfig
}

/// <summary>
/// The weights configuration for JinaAI multi-media vectorization.
/// </summary>
public record Multi2VecJinaAIWeights
{
    public double[]? ImageFields { get; set; } = null;
    public double[]? TextFields { get; set; } = null;
}

/// <summary>
/// The configuration for multi-media vectorization using the Jina module.
/// See the documentation for detailed usage.
/// </summary>
public record Multi2VecJinaAIConfig : VectorizerConfig
{
    public Multi2VecJinaAIConfig()
        : base("multi2vec-jinaai") { }

    public string? BaseURL { get; set; } = null;
    public int? Dimensions { get; set; } = null;
    public string[]? ImageFields { get; set; } = null;
    public string? Model { get; set; } = null;
    public string[]? TextFields { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
    public Multi2VecJinaAIWeights? Weights { get; set; } = null;
}

/// <summary>
/// The weights configuration for VoyageAI multi-media vectorization.
/// </summary>
public record Multi2VecVoyageAIWeights
{
    public double[]? ImageFields { get; set; } = null;
    public double[]? TextFields { get; set; } = null;
}

/// <summary>
/// The configuration for multi-media vectorization using the VoyageAI module.
/// See the documentation for detailed usage.
/// </summary>
public record Multi2VecVoyageAIConfig : VectorizerConfig
{
    public Multi2VecVoyageAIConfig()
        : base("multi2vec-voyageai") { }

    public string? BaseURL { get; set; } = null;
    public string[]? ImageFields { get; set; } = null;
    public string? Model { get; set; } = null;
    public string? OutputEncoding { get; set; } = null;
    public string[]? TextFields { get; set; } = null;
    public bool? Truncate { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
    public Multi2VecVoyageAIWeights? Weights { get; set; } = null;
}

/// <summary>
/// The configuration for reference-based vectorization using the centroid method.
/// See the documentation for detailed usage.
/// </summary>
public record Ref2VecCentroidConfig : VectorizerConfig
{
    public Ref2VecCentroidConfig()
        : base("ref2vec-centroid") { }

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
public record Text2VecAWSConfig : VectorizerConfig
{
    public Text2VecAWSConfig()
        : base("text2vec-aws") { }

    public required string Region { get; set; }
    public required string Service { get; set; }
    public string? Endpoint { get; set; } = null;
    public string? Model { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the OpenAI module with Azure.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecAzureOpenAIConfig : VectorizerConfig
{
    public Text2VecAzureOpenAIConfig()
        : base("text2vec-azure-openai") { }

    public required string DeploymentId { get; set; }
    public required string ResourceName { get; set; }
    public string? BaseURL { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the Cohere module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecCohereConfig : VectorizerConfig
{
    public Text2VecCohereConfig()
        : base("text2vec-cohere") { }

    public string? BaseURL { get; set; } = null;
    public string? Model { get; set; } = null;
    public bool? Truncate { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the Contextionary module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecContextionaryConfig : VectorizerConfig
{
    public Text2VecContextionaryConfig()
        : base("text2vec-contextionary") { }

    public bool? VectorizeClassName { get; set; } = false;
}

/// <summary>
/// The configuration for text vectorization using the Databricks module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecDatabricksConfig : VectorizerConfig
{
    public Text2VecDatabricksConfig()
        : base("text2vec-databricks") { }

    public required string Endpoint { get; set; }
    public string? Instruction { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the GPT-4-All module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecGPT4AllConfig : VectorizerConfig
{
    public Text2VecGPT4AllConfig()
        : base("text2vec-gpt4all") { }

    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the HuggingFace module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecHuggingFaceConfig : VectorizerConfig
{
    public Text2VecHuggingFaceConfig()
        : base("text2vec-huggingface") { }

    public string? EndpointURL { get; set; } = null;
    public string? Model { get; set; } = null;
    public string? PassageModel { get; set; } = null;
    public string? QueryModel { get; set; } = null;
    public bool? UseCache { get; set; } = null;
    public bool? UseGPU { get; set; } = null;
    public bool? WaitForModel { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the Jina module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecJinaAIConfig : VectorizerConfig
{
    public Text2VecJinaAIConfig()
        : base("text2vec-jinaai") { }

    public string? Model { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// Deprecated. Use Text2VecJinaAIConfig instead.
/// </summary>
[Obsolete("Use Text2VecJinaAIConfig instead.")]
public record Text2VecJinaConfig : Text2VecJinaAIConfig
{
    // Inherits all properties from Text2VecJinaAIConfig
}

/// <summary>
/// The configuration for text vectorization using the Nvidia module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecNvidiaConfig : VectorizerConfig
{
    public Text2VecNvidiaConfig()
        : base("text2vec-nvidia") { }

    public string? BaseURL { get; set; } = null;
    public string? Model { get; set; } = null;
    public bool? Truncate { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the Mistral module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecMistralConfig : VectorizerConfig
{
    public Text2VecMistralConfig()
        : base("text2vec-mistral") { }

    public string? BaseURL { get; set; } = null;
    public string? Model { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the Ollama module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecOllamaConfig : VectorizerConfig
{
    public Text2VecOllamaConfig()
        : base("text2vec-ollama") { }

    public string? ApiEndpoint { get; set; } = null;
    public string? Model { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the OpenAI module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecOpenAIConfig : VectorizerConfig
{
    public Text2VecOpenAIConfig()
        : base("text2vec-openai") { }

    public string? BaseURL { get; set; } = null;
    public int? Dimensions { get; set; } = null;
    public string? Model { get; set; } = null;
    public string? ModelVersion { get; set; } = null;
    public string? Type { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// Deprecated. Use Text2VecGoogleConfig instead.
/// </summary>
[Obsolete("Use Text2VecGoogleConfig instead.")]
public record Text2VecPalmConfig : Text2VecGoogleConfig
{
    // Inherits all properties from Text2VecGoogleConfig
}

/// <summary>
/// The configuration for text vectorization using the Google module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecGoogleConfig : VectorizerConfig
{
    public Text2VecGoogleConfig()
        : base("text2vec-palm") { }

    public string? ApiEndpoint { get; set; } = null;
    public string? ModelId { get; set; } = null;
    public string? ProjectId { get; set; } = null;
    public string? TitleProperty { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the Transformers module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecTransformersConfig : VectorizerConfig
{
    public Text2VecTransformersConfig()
        : base("text2vec-transformers") { }

    public string? InferenceUrl { get; set; } = null;
    public string? PassageInferenceUrl { get; set; } = null;
    public string? QueryInferenceUrl { get; set; } = null;
    public string? PoolingStrategy { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using the VoyageAI module.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecVoyageAIConfig : VectorizerConfig
{
    public Text2VecVoyageAIConfig()
        : base("text2vec-voyageai") { }

    public string? BaseURL { get; set; } = null;
    public string? Model { get; set; } = null;
    public bool? Truncate { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

/// <summary>
/// The configuration for text vectorization using Weaviate's self-hosted text-based embedding models.
/// See the documentation for detailed usage.
/// </summary>
public record Text2VecWeaviateConfig : VectorizerConfig
{
    public Text2VecWeaviateConfig()
        : base("text2vec-weaviate") { }

    public string? BaseURL { get; set; } = null;

    [JsonConverter(typeof(FlexibleConverter<int>))]
    public int? Dimensions { get; set; } = null;

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Model { get; set; } = null;
    public bool? VectorizeCollectionName { get; set; } = null;
}

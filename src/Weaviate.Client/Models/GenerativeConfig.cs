using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// Defines a generative AI configuration.
/// </summary>
public interface IGenerativeConfig
{
    /// <summary>
    /// Gets the type identifier for the generative configuration.
    /// </summary>
    [JsonIgnore]
    string Type { get; }
}

/// <summary>
/// Contains generative AI configuration options for various providers.
/// </summary>
public static class GenerativeConfig
{
    /// <summary>
    /// Represents a custom generative AI configuration.
    /// </summary>
    public record Custom : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Custom"/> class.
        /// </summary>
        [JsonConstructor]
        internal Custom() { }

        /// <summary>
        /// Gets or sets the type identifier for the custom configuration.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the custom configuration object.
        /// </summary>
        public object Config { get; set; } = new { };
    }

    /// <summary>
    /// Base class for OpenAI-based generative AI configurations.
    /// </summary>
    public abstract record OpenAIBase : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIBase"/> class.
        /// </summary>
        [JsonConstructor]
        internal OpenAIBase() { }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the API endpoint.
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the frequency penalty to reduce repetition.
        /// </summary>
        public int? FrequencyPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the presence penalty to encourage topic diversity.
        /// </summary>
        public int? PresencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the API version to use.
        /// </summary>
        public string? ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the reasoning effort level for reasoning models.
        /// </summary>
        public string? ReasoningEffort { get; set; }

        /// <summary>
        /// Gets or sets the verbosity level for model outputs.
        /// </summary>
        public string? Verbosity { get; set; }

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public abstract string Type { get; }
    }

    /// <summary>
    /// Configuration for AWS generative AI services.
    /// </summary>
    public record AWS : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AWS"/> class.
        /// </summary>
        [JsonConstructor]
        internal AWS() { }

        /// <summary>
        /// The type value for AWS configuration.
        /// </summary>
        public const string TypeValue = "generative-aws";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the AWS region for the service.
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the AWS service name (e.g., "bedrock" or "sagemaker").
        /// </summary>
        public string Service { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL for the service.
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the target model name for SageMaker.
        /// </summary>
        public string? TargetModel { get; set; }

        /// <summary>
        /// Gets or sets the target model variant for SageMaker.
        /// </summary>
        public string? TargetVariant { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public string[]? StopSequences { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public double? TopK { get; set; }
    }

    /// <summary>
    /// Configuration for Anthropic generative AI provider.
    /// </summary>
    public record Anthropic : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Anthropic"/> class.
        /// </summary>
        [JsonConstructor]
        internal Anthropic() { }

        /// <summary>
        /// The type value for Anthropic configuration.
        /// </summary>
        public const string TypeValue = "generative-anthropic";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the base URL for the Anthropic API endpoint.
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public string[]? StopSequences { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public int? TopK { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }
    }

    /// <summary>
    /// Configuration for Anyscale generative AI provider.
    /// </summary>
    public record Anyscale : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Anyscale"/> class.
        /// </summary>
        [JsonConstructor]
        internal Anyscale() { }

        /// <summary>
        /// The type value for Anyscale configuration.
        /// </summary>
        public const string TypeValue = "generative-anyscale";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the base URL for the Anyscale API endpoint.
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }
    }

    /// <summary>
    /// Configuration for Cohere generative AI provider.
    /// </summary>
    public record Cohere : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cohere"/> class.
        /// </summary>
        [JsonConstructor]
        internal Cohere() { }

        /// <summary>
        /// The type value for Cohere configuration.
        /// </summary>
        public const string TypeValue = "generative-cohere";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public int? K { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the Cohere API endpoint.
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public string[]? StopSequences { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }
    }

    /// <summary>
    /// Configuration for Databricks generative AI provider.
    /// </summary>
    public record Databricks : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Databricks"/> class.
        /// </summary>
        [JsonConstructor]
        internal Databricks() { }

        /// <summary>
        /// The type value for Databricks configuration.
        /// </summary>
        public const string TypeValue = "generative-databricks";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the endpoint URL for the Databricks service.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public int? TopK { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }
    }

    /// <summary>
    /// Configuration for FriendliAI generative AI provider.
    /// </summary>
    public record FriendliAI : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FriendliAI"/> class.
        /// </summary>
        [JsonConstructor]
        internal FriendliAI() { }

        /// <summary>
        /// The type value for FriendliAI configuration.
        /// </summary>
        public const string TypeValue = "generative-friendliai";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the base URL for the FriendliAI API endpoint.
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }
    }

    /// <summary>
    /// Configuration for Mistral generative AI provider.
    /// </summary>
    public record Mistral : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mistral"/> class.
        /// </summary>
        [JsonConstructor]
        internal Mistral() { }

        /// <summary>
        /// The type value for Mistral configuration.
        /// </summary>
        public const string TypeValue = "generative-mistral";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the base URL for the Mistral API endpoint.
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }
    }

    /// <summary>
    /// Configuration for Nvidia generative AI provider.
    /// </summary>
    public record Nvidia : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Nvidia"/> class.
        /// </summary>
        [JsonConstructor]
        internal Nvidia() { }

        /// <summary>
        /// The type value for Nvidia configuration.
        /// </summary>
        public const string TypeValue = "generative-nvidia";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the base URL for the Nvidia API endpoint.
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }
    }

    /// <summary>
    /// Configuration for Ollama generative AI provider.
    /// </summary>
    public record Ollama : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ollama"/> class.
        /// </summary>
        [JsonConstructor]
        internal Ollama() { }

        /// <summary>
        /// The type value for Ollama configuration.
        /// </summary>
        public const string TypeValue = "generative-ollama";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the API endpoint URL for the Ollama service.
        /// </summary>
        public string? ApiEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }
    }

    /// <summary>
    /// Configuration for OpenAI generative AI provider.
    /// </summary>
    public record OpenAI : OpenAIBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAI"/> class.
        /// </summary>
        [JsonConstructor]
        internal OpenAI() { }

        /// <summary>
        /// The type value for OpenAI configuration.
        /// </summary>
        public const string TypeValue = "generative-openai";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public override string Type => TypeValue;
    }

    /// <summary>
    /// Configuration for Azure OpenAI generative AI provider.
    /// </summary>
    public record AzureOpenAI : OpenAIBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureOpenAI"/> class.
        /// </summary>
        [JsonConstructor]
        internal AzureOpenAI() { }

        /// <summary>
        /// The type value for Azure OpenAI configuration.
        /// </summary>
        public const string TypeValue = "generative-azure-openai";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public override string Type => TypeValue;

        /// <summary>
        /// Gets or sets the Azure resource name.
        /// </summary>
        public string ResourceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the deployment ID in Azure.
        /// </summary>
        public string DeploymentId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Configuration for Google Vertex AI generative AI provider.
    /// </summary>
    public record GoogleVertex : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleVertex"/> class.
        /// </summary>
        [JsonConstructor]
        internal GoogleVertex() { }

        /// <summary>
        /// The type value for Google Vertex AI configuration.
        /// </summary>
        public const string TypeValue = "generative-google-vertex";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the API endpoint URL for Google Vertex AI.
        /// </summary>
        public string? ApiEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of output tokens to generate.
        /// </summary>
        public int? MaxOutputTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the Google Cloud project ID.
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the endpoint ID for the model.
        /// </summary>
        public string? EndpointId { get; set; }

        /// <summary>
        /// Gets or sets the Google Cloud region.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public int? TopK { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }
    }

    /// <summary>
    /// Configuration for Google Gemini generative AI provider.
    /// </summary>
    public record GoogleGemini : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleGemini"/> class.
        /// </summary>
        [JsonConstructor]
        internal GoogleGemini() { }

        /// <summary>
        /// The type value for Google Gemini configuration.
        /// </summary>
        public const string TypeValue = "generative-google-gemini";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the maximum number of output tokens to generate.
        /// </summary>
        public int? MaxOutputTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public int? TopK { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }
    }

    /// <summary>
    /// Configuration for xAI generative AI provider.
    /// </summary>
    public record XAI : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XAI"/> class.
        /// </summary>
        [JsonConstructor]
        internal XAI() { }

        /// <summary>
        /// The type value for xAI configuration.
        /// </summary>
        public const string TypeValue = "generative-xai";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the base URL for the xAI API endpoint.
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }
    }

    /// <summary>
    /// Configuration for Contextual AI generative AI provider.
    /// </summary>
    public record ContextualAI : IGenerativeConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualAI"/> class.
        /// </summary>
        [JsonConstructor]
        internal ContextualAI() { }

        /// <summary>
        /// The type value for Contextual AI configuration.
        /// </summary>
        public const string TypeValue = "generative-contextualai";

        /// <summary>
        /// Gets the type identifier for the configuration.
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of new tokens to generate.
        /// </summary>
        public long? MaxNewTokens { get; set; }

        /// <summary>
        /// Gets or sets the system prompt to guide generation.
        /// </summary>
        public string? SystemPrompt { get; set; }

        /// <summary>
        /// Gets or sets whether to avoid generating commentary.
        /// </summary>
        public bool? AvoidCommentary { get; set; }

        /// <summary>
        /// Gets or sets the knowledge base entries to use during generation.
        /// </summary>
        public string[]? Knowledge { get; set; }
    }
}

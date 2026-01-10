namespace Weaviate.Client.Models.Generative;

/// <summary>
/// Contains generative AI provider configurations for various services.
/// </summary>
public static class Providers
{
    /// <summary>
    /// Configuration for Anthropic generative AI provider.
    /// </summary>
    public record Anthropic() : Weaviate.Client.Models.GenerativeProvider("anthropic")
    {
        /// <summary>
        /// Gets or sets the base URL for the Anthropic API endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

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
        public long? TopK { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public List<string>? StopSequences { get; set; }

        /// <summary>
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }
    }

    /// <summary>
    /// Configuration for Anyscale generative AI provider.
    /// </summary>
    public record Anyscale() : Weaviate.Client.Models.GenerativeProvider("anyscale")
    {
        /// <summary>
        /// Gets or sets the base URL for the Anyscale API endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

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
    /// Configuration for AWS Bedrock generative AI provider.
    /// </summary>
    public record AWSBedrock() : Weaviate.Client.Models.GenerativeProvider("aws")
    {
        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the AWS region for the service.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL for the service.
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }
        // TODO - add top_k, top_p & stop_sequences here when added to server-side proto
        // Check the latest available version of `grpc/proto/v1/generative.proto` (see GenerativeAWS) in the server repo
    }

    /// <summary>
    /// Configuration for AWS SageMaker generative AI provider.
    /// </summary>
    public record AWSSagemaker() : Weaviate.Client.Models.GenerativeProvider("aws")
    {
        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the AWS region for the service.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL for the service.
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the target model name in SageMaker.
        /// </summary>
        public string? TargetModel { get; set; }

        /// <summary>
        /// Gets or sets the target model variant in SageMaker.
        /// </summary>
        public string? TargetVariant { get; set; }

        /// <summary>
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }
        // TODO - add top_k, top_p & stop_sequences here when added to server-side proto
        // Check the latest available version of `grpc/proto/v1/generative.proto` (see GenerativeAWS) in the server repo
    }

    /// <summary>
    /// Configuration for Cohere generative AI provider.
    /// </summary>
    public record Cohere() : Weaviate.Client.Models.GenerativeProvider("cohere")
    {
        /// <summary>
        /// Gets or sets the base URL for the Cohere API endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the frequency penalty to reduce repetition.
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public long? K { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? P { get; set; }

        /// <summary>
        /// Gets or sets the presence penalty to encourage topic diversity.
        /// </summary>
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public List<string>? StopSequences { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }
    }

    /// <summary>
    /// Configuration for Dummy generative AI provider for testing purposes.
    /// </summary>
    public record Dummy() : Weaviate.Client.Models.GenerativeProvider("dummy") { }

    /// <summary>
    /// Configuration for Mistral generative AI provider.
    /// </summary>
    public record Mistral() : Weaviate.Client.Models.GenerativeProvider("mistral")
    {
        /// <summary>
        /// Gets or sets the base URL for the Mistral API endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

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
    public record Ollama() : Weaviate.Client.Models.GenerativeProvider("ollama")
    {
        /// <summary>
        /// Gets or sets the API endpoint URL for the Ollama service.
        /// </summary>
        public string? ApiEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }
    }

    /// <summary>
    /// Configuration for Azure OpenAI generative AI provider.
    /// </summary>
    public record AzureOpenAI() : Weaviate.Client.Models.GenerativeProvider("azure-openai")
    {
        /// <summary>
        /// Specifies the reasoning effort level for reasoning models.
        /// </summary>
        public enum ReasoningEffortLevel
        {
            /// <summary>
            /// Unspecified reasoning effort level.
            /// </summary>
            Unspecified = 0,

            /// <summary>
            /// Minimal reasoning effort.
            /// </summary>
            Minimal = 1,

            /// <summary>
            /// Low reasoning effort.
            /// </summary>
            Low = 2,

            /// <summary>
            /// Medium reasoning effort.
            /// </summary>
            Medium = 3,

            /// <summary>
            /// High reasoning effort.
            /// </summary>
            High = 4,
        }

        /// <summary>
        /// Specifies the verbosity level for model outputs.
        /// </summary>
        public enum VerbosityLevel
        {
            /// <summary>
            /// Unspecified verbosity level.
            /// </summary>
            Unspecified = 0,

            /// <summary>
            /// Low verbosity.
            /// </summary>
            Low = 1,

            /// <summary>
            /// Medium verbosity.
            /// </summary>
            Medium = 2,

            /// <summary>
            /// High verbosity.
            /// </summary>
            High = 3,
        }

        /// <summary>
        /// Gets or sets the frequency penalty to reduce repetition.
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the number of completions to generate.
        /// </summary>
        public long? N { get; set; }

        /// <summary>
        /// Gets or sets the presence penalty to encourage topic diversity.
        /// </summary>
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public List<string>? Stop { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the Azure OpenAI endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the API version to use.
        /// </summary>
        public string? ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the Azure resource name.
        /// </summary>
        public string? ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the deployment ID in Azure.
        /// </summary>
        public string? DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets whether this is an Azure deployment.
        /// </summary>
        public bool? IsAzure { get; set; }

        /// <summary>
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }

        /// <summary>
        /// Gets or sets the reasoning effort level for reasoning models.
        /// </summary>
        public ReasoningEffortLevel? ReasoningEffort { get; set; }

        /// <summary>
        /// Gets or sets the verbosity level for model outputs.
        /// </summary>
        public VerbosityLevel? Verbosity { get; set; }
    }

    /// <summary>
    /// Configuration for OpenAI generative AI provider.
    /// </summary>
    public record OpenAI() : Weaviate.Client.Models.GenerativeProvider("openai")
    {
        /// <summary>
        /// Specifies the reasoning effort level for reasoning models.
        /// </summary>
        public enum ReasoningEffortLevel
        {
            /// <summary>
            /// Unspecified reasoning effort level.
            /// </summary>
            Unspecified = 0,

            /// <summary>
            /// Minimal reasoning effort.
            /// </summary>
            Minimal = 1,

            /// <summary>
            /// Low reasoning effort.
            /// </summary>
            Low = 2,

            /// <summary>
            /// Medium reasoning effort.
            /// </summary>
            Medium = 3,

            /// <summary>
            /// High reasoning effort.
            /// </summary>
            High = 4,
        }

        /// <summary>
        /// Specifies the verbosity level for model outputs.
        /// </summary>
        public enum VerbosityLevel
        {
            /// <summary>
            /// Unspecified verbosity level.
            /// </summary>
            Unspecified = 0,

            /// <summary>
            /// Low verbosity.
            /// </summary>
            Low = 1,

            /// <summary>
            /// Medium verbosity.
            /// </summary>
            Medium = 2,

            /// <summary>
            /// High verbosity.
            /// </summary>
            High = 3,
        }

        /// <summary>
        /// Gets or sets the frequency penalty to reduce repetition.
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the number of completions to generate.
        /// </summary>
        public long? N { get; set; }

        /// <summary>
        /// Gets or sets the presence penalty to encourage topic diversity.
        /// </summary>
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public List<string>? Stop { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the OpenAI API endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the API version to use.
        /// </summary>
        public string? ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        public string? ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the deployment ID.
        /// </summary>
        public string? DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets whether this is an Azure deployment.
        /// </summary>
        public bool? IsAzure { get; set; }

        /// <summary>
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }

        /// <summary>
        /// Gets or sets the reasoning effort level for reasoning models.
        /// </summary>
        public ReasoningEffortLevel? ReasoningEffort { get; set; }

        /// <summary>
        /// Gets or sets the verbosity level for model outputs.
        /// </summary>
        public VerbosityLevel? Verbosity { get; set; }
    }

    /// <summary>
    /// Configuration for Google Vertex AI generative AI provider.
    /// </summary>
    public record GoogleVertex() : Weaviate.Client.Models.GenerativeProvider("google-vertex")
    {
        /// <summary>
        /// Gets or sets the frequency penalty to reduce repetition.
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the presence penalty to encourage topic diversity.
        /// </summary>
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public long? TopK { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public List<string>? StopSequences { get; set; }

        /// <summary>
        /// Gets or sets the API endpoint URL for Google Vertex AI.
        /// </summary>
        public string? ApiEndpoint { get; set; }

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
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }
    }

    /// <summary>
    /// Configuration for Google Gemini generative AI provider.
    /// </summary>
    public record GoogleGemini() : Weaviate.Client.Models.GenerativeProvider("google-gemini")
    {
        /// <summary>
        /// Gets or sets the frequency penalty to reduce repetition.
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the presence penalty to encourage topic diversity.
        /// </summary>
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the top-k sampling parameter.
        /// </summary>
        public long? TopK { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public List<string>? StopSequences { get; set; }

        /// <summary>
        /// Gets or sets the API endpoint URL for Google Gemini.
        /// </summary>
        public string? ApiEndpoint { get; set; }

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
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }
    }

    /// <summary>
    /// Configuration for Databricks generative AI provider.
    /// </summary>
    public record Databricks() : Weaviate.Client.Models.GenerativeProvider("databricks")
    {
        /// <summary>
        /// Gets or sets the endpoint URL for the Databricks service.
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the frequency penalty to reduce repetition.
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Gets or sets whether to include log probabilities in the response.
        /// </summary>
        public bool? LogProbs { get; set; }

        /// <summary>
        /// Gets or sets the number of top log probabilities to return.
        /// </summary>
        public long? TopLogProbs { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the number of completions to generate.
        /// </summary>
        public long? N { get; set; }

        /// <summary>
        /// Gets or sets the presence penalty to encourage topic diversity.
        /// </summary>
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the sequences where generation should stop.
        /// </summary>
        public List<string>? Stop { get; set; }

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
    /// Configuration for FriendliAI generative AI provider.
    /// </summary>
    public record FriendliAI() : Weaviate.Client.Models.GenerativeProvider("friendliai")
    {
        /// <summary>
        /// Gets or sets the base URL for the FriendliAI API endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the model identifier to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the temperature for controlling randomness in generation.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the number of completions to generate.
        /// </summary>
        public long? N { get; set; }

        /// <summary>
        /// Gets or sets the top-p (nucleus) sampling parameter.
        /// </summary>
        public double? TopP { get; set; }
    }

    /// <summary>
    /// Configuration for Nvidia generative AI provider.
    /// </summary>
    public record Nvidia() : Weaviate.Client.Models.GenerativeProvider("nvidia")
    {
        /// <summary>
        /// Gets or sets the base URL for the Nvidia API endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

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
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }
    }

    /// <summary>
    /// Configuration for xAI generative AI provider.
    /// </summary>
    public record XAI() : Weaviate.Client.Models.GenerativeProvider("xai")
    {
        /// <summary>
        /// Gets or sets the base URL for the xAI API endpoint.
        /// </summary>
        public string? BaseUrl { get; set; }

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
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public long? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the list of base64-encoded images to include in the prompt.
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the list of property names containing images.
        /// </summary>
        public List<string>? ImageProperties { get; set; }
    }

    /// <summary>
    /// Configuration for Contextual AI generative AI provider.
    /// </summary>
    public record ContextualAI() : Weaviate.Client.Models.GenerativeProvider("contextualai")
    {
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

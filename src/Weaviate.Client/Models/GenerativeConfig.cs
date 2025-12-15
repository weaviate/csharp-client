using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public interface IGenerativeConfig
{
    [JsonIgnore]
    string Type { get; }
}

public static class GenerativeConfig
{
    public record Custom : IGenerativeConfig
    {
        [JsonConstructor]
        internal Custom() { }

        public required string Type { get; set; }

        public dynamic? Config { get; set; } = new { };
    }

    public abstract record OpenAIBase : IGenerativeConfig
    {
        [JsonConstructor]
        internal OpenAIBase() { }

        public string? Model { get; set; }
        public string? BaseURL { get; set; }
        public int? FrequencyPenalty { get; set; }
        public int? MaxTokens { get; set; }
        public int? PresencePenalty { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public string? ApiVersion { get; set; }
        public string? ReasoningEffort { get; set; }
        public string? Verbosity { get; set; }
        public abstract string Type { get; }
    }

    public record AWS : IGenerativeConfig
    {
        [JsonConstructor]
        internal AWS() { }

        public const string TypeValue = "generative-aws";
        public string Type => TypeValue;

        public string Region { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Endpoint { get; set; }
        public string? TargetModel { get; set; }
        public string? TargetVariant { get; set; }
        public int? MaxTokens { get; set; }
        public string[]? StopSequences { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public double? TopK { get; set; }
    }

    public record Anthropic : IGenerativeConfig
    {
        [JsonConstructor]
        internal Anthropic() { }

        public const string TypeValue = "generative-anthropic";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public string[]? StopSequences { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record Anyscale : IGenerativeConfig
    {
        [JsonConstructor]
        internal Anyscale() { }

        public const string TypeValue = "generative-anyscale";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record Cohere : IGenerativeConfig
    {
        [JsonConstructor]
        internal Cohere() { }

        public const string TypeValue = "generative-cohere";
        public string Type => TypeValue;

        public int? K { get; set; }
        public string? Model { get; set; }
        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string[]? StopSequences { get; set; }
        public double? Temperature { get; set; }
    }

    public record Databricks : IGenerativeConfig
    {
        [JsonConstructor]
        internal Databricks() { }

        public const string TypeValue = "generative-databricks";
        public string Type => TypeValue;

        public string Endpoint { get; set; } = string.Empty;
        public int? MaxTokens { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record FriendliAI : IGenerativeConfig
    {
        [JsonConstructor]
        internal FriendliAI() { }

        public const string TypeValue = "generative-friendliai";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record Mistral : IGenerativeConfig
    {
        [JsonConstructor]
        internal Mistral() { }

        public const string TypeValue = "generative-mistral";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record Nvidia : IGenerativeConfig
    {
        [JsonConstructor]
        internal Nvidia() { }

        public const string TypeValue = "generative-nvidia";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
    }

    public record Ollama : IGenerativeConfig
    {
        [JsonConstructor]
        internal Ollama() { }

        public const string TypeValue = "generative-ollama";
        public string Type => TypeValue;

        public string? ApiEndpoint { get; set; }
        public string? Model { get; set; }
    }

    public record OpenAI : OpenAIBase
    {
        [JsonConstructor]
        internal OpenAI() { }

        public const string TypeValue = "generative-openai";
        public override string Type => TypeValue;
    }

    public record AzureOpenAI : OpenAIBase
    {
        [JsonConstructor]
        internal AzureOpenAI() { }

        public const string TypeValue = "generative-azure-openai";
        public override string Type => TypeValue;

        public string ResourceName { get; set; } = string.Empty;
        public string DeploymentId { get; set; } = string.Empty;
    }

    public record GoogleVertex : IGenerativeConfig
    {
        [JsonConstructor]
        internal GoogleVertex() { }

        public const string TypeValue = "generative-google-vertex";
        public string Type => TypeValue;

        public string? ApiEndpoint { get; set; }
        public int? MaxOutputTokens { get; set; }
        public string? Model { get; set; }
        public string? ProjectId { get; set; }
        public string? EndpointId { get; set; }
        public string? Region { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record GoogleGemini : IGenerativeConfig
    {
        [JsonConstructor]
        internal GoogleGemini() { }

        public const string TypeValue = "generative-google-gemini";
        public string Type => TypeValue;

        public int? MaxOutputTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record XAI : IGenerativeConfig
    {
        [JsonConstructor]
        internal XAI() { }

        public const string TypeValue = "generative-xai";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
    }

    public record ContextualAI : IGenerativeConfig
    {
        [JsonConstructor]
        internal ContextualAI() { }

        public const string TypeValue = "generative-contextualai";
        public string Type => TypeValue;

        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public long? MaxNewTokens { get; set; }
        public string? SystemPrompt { get; set; }
        public bool? AvoidCommentary { get; set; }
        public string[]? Knowledge { get; set; }
    }
}

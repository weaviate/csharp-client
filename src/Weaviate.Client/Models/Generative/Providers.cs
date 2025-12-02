namespace Weaviate.Client.Models.Generative;

public static class Providers
{
    public record Anthropic() : Weaviate.Client.Models.GenerativeProvider("anthropic")
    {
        public string? BaseUrl { get; set; }
        public long? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public long? TopK { get; set; }
        public double? TopP { get; set; }
        public List<string>? StopSequences { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? ImageProperties { get; set; }
    }

    public record Anyscale() : Weaviate.Client.Models.GenerativeProvider("anyscale")
    {
        public string? BaseUrl { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record AWS() : Weaviate.Client.Models.GenerativeProvider("aws")
    {
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public string? Service { get; set; }
        public string? Region { get; set; }
        public string? Endpoint { get; set; }
        public string? TargetModel { get; set; }
        public string? TargetVariant { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? ImageProperties { get; set; }
        public long? MaxTokens { get; set; }
    }

    public record Cohere() : Weaviate.Client.Models.GenerativeProvider("cohere")
    {
        public string? BaseUrl { get; set; }
        public double? FrequencyPenalty { get; set; }
        public long? MaxTokens { get; set; }
        public string? Model { get; set; }
        public long? K { get; set; }
        public double? P { get; set; }
        public double? PresencePenalty { get; set; }
        public List<string>? StopSequences { get; set; }
        public double? Temperature { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? ImageProperties { get; set; }
    }

    public record Dummy() : Weaviate.Client.Models.GenerativeProvider("dummy") { }

    public record Mistral() : Weaviate.Client.Models.GenerativeProvider("mistral")
    {
        public string? BaseUrl { get; set; }
        public long? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
    }

    public record Ollama() : Weaviate.Client.Models.GenerativeProvider("ollama")
    {
        public string? ApiEndpoint { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? ImageProperties { get; set; }
    }

    public record OpenAI() : Weaviate.Client.Models.GenerativeProvider("openai")
    {
        public enum ReasoningEffortLevel
        {
            Unspecified = 0,
            Minimal = 1,
            Low = 2,
            Medium = 3,
            High = 4,
        }

        public enum VerbosityLevel
        {
            Unspecified = 0,
            Low = 1,
            Medium = 2,
            High = 3,
        }

        public double? FrequencyPenalty { get; set; }
        public long? MaxTokens { get; set; }
        public string? Model { get; set; }
        public long? N { get; set; }
        public double? PresencePenalty { get; set; }
        public List<string>? Stop { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public string? BaseUrl { get; set; }
        public string? ApiVersion { get; set; }
        public string? ResourceName { get; set; }
        public string? DeploymentId { get; set; }
        public bool? IsAzure { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? ImageProperties { get; set; }
        public ReasoningEffortLevel? ReasoningEffort { get; set; }
        public VerbosityLevel? Verbosity { get; set; }
    }

    public record Google() : Weaviate.Client.Models.GenerativeProvider("google")
    {
        public double? FrequencyPenalty { get; set; }
        public long? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? PresencePenalty { get; set; }
        public double? Temperature { get; set; }
        public long? TopK { get; set; }
        public double? TopP { get; set; }
        public List<string>? StopSequences { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? ProjectId { get; set; }
        public string? EndpointId { get; set; }
        public string? Region { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? ImageProperties { get; set; }
    }

    public record Databricks() : Weaviate.Client.Models.GenerativeProvider("databricks")
    {
        public string? Endpoint { get; set; }
        public string? Model { get; set; }
        public double? FrequencyPenalty { get; set; }
        public bool? LogProbs { get; set; }
        public long? TopLogProbs { get; set; }
        public long? MaxTokens { get; set; }
        public long? N { get; set; }
        public double? PresencePenalty { get; set; }
        public List<string>? Stop { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
    }

    public record FriendliAI() : Weaviate.Client.Models.GenerativeProvider("friendliai")
    {
        public string? BaseUrl { get; set; }
        public string? Model { get; set; }
        public long? MaxTokens { get; set; }
        public double? Temperature { get; set; }
        public long? N { get; set; }
        public double? TopP { get; set; }
    }

    public record Nvidia() : Weaviate.Client.Models.GenerativeProvider("nvidia")
    {
        public string? BaseUrl { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public long? MaxTokens { get; set; }
    }

    public record XAI() : Weaviate.Client.Models.GenerativeProvider("xai")
    {
        public string? BaseUrl { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public long? MaxTokens { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? ImageProperties { get; set; }
    }

    public record ContextualAI() : Weaviate.Client.Models.GenerativeProvider("contextualai")
    {
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public long? MaxNewTokens { get; set; }
        public string? SystemPrompt { get; set; }
        public string? AvoidCommentary { get; set; }
        public string[]? Knowledge { get; set; }
    }
}

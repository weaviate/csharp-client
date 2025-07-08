using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public interface IGenerativeConfig
{
    [JsonIgnore]
    string Type { get; }
}

public static class Generative
{
    public record Custom : IGenerativeConfig
    {
        public required string Type { get; set; }

        public dynamic? Config { get; set; } = new { };
    }

    public abstract record OpenAIConfigBase : IGenerativeConfig
    {
        public string? BaseURL { get; set; }
        public int? FrequencyPenaltyProperty { get; set; }
        public int? MaxTokensProperty { get; set; }
        public int? PresencePenaltyProperty { get; set; }
        public double? TemperatureProperty { get; set; }
        public double? TopPProperty { get; set; }
        public abstract string Type { get; }
    }

    public record AWSConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-aws";
        public string Type => TypeValue;

        public string Region { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Endpoint { get; set; }
    }

    public record AnthropicConfig : IGenerativeConfig
    {
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

    public record AnyscaleConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-anyscale";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record CohereConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-cohere";
        public string Type => TypeValue;

        public int? KProperty { get; set; }
        public string? Model { get; set; }
        public int? MaxTokensProperty { get; set; }
        public string? ReturnLikelihoodsProperty { get; set; }
        public string[]? StopSequencesProperty { get; set; }
        public double? TemperatureProperty { get; set; }
    }

    public record DatabricksConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-databricks";
        public string Type => TypeValue;

        public string Endpoint { get; set; } = string.Empty;
        public int? MaxTokens { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record FriendliAIConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-friendliai";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record MistralConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-mistral";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record NvidiaConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-nvidia";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record OllamaConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-ollama";
        public string Type => TypeValue;

        public string? ApiEndpoint { get; set; }
        public string? Model { get; set; }
    }

    public record OpenAIConfig : OpenAIConfigBase
    {
        public const string TypeValue = "generative-openai";
        public override string Type => TypeValue;

        public string? Model { get; set; }
    }

    public record AzureOpenAIConfig : OpenAIConfigBase
    {
        public const string TypeValue = "generative-azure-openai";
        public override string Type => TypeValue;

        public string ResourceName { get; set; } = string.Empty;
        public string DeploymentId { get; set; } = string.Empty;
    }

    public record GoogleConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-google";
        public string Type => TypeValue;

        public string? ApiEndpoint { get; set; }
        public int? MaxOutputTokens { get; set; }
        public string? ModelId { get; set; }
        public string? ProjectId { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record XAIConfig : IGenerativeConfig
    {
        public const string TypeValue = "generative-xai";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
    }
}

namespace Weaviate.Client.Models;

public abstract record GenerativeConfig
{
    public abstract string Type { get; }
}

public static class Generative
{
    public record Custom<T>(string TypeValue) : GenerativeConfig
    {
        public override string Type => TypeValue;

        public T? Config { get; set; }
    }

    public abstract record OpenAIConfigBase : GenerativeConfig
    {
        public string? BaseURL { get; set; }
        public int? FrequencyPenaltyProperty { get; set; }
        public int? MaxTokensProperty { get; set; }
        public int? PresencePenaltyProperty { get; set; }
        public double? TemperatureProperty { get; set; }
        public double? TopPProperty { get; set; }
    }

    public record AWSConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-aws";
        public override string Type => TypeValue;

        public string Region { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Endpoint { get; set; }
    }

    public record AnthropicConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-anthropic";
        public override string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public string[]? StopSequences { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record AnyscaleConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-anyscale";
        public override string Type => TypeValue;

        public string? BaseURL { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record CohereConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-cohere";
        public override string Type => TypeValue;

        public int? KProperty { get; set; }
        public string? Model { get; set; }
        public int? MaxTokensProperty { get; set; }
        public string? ReturnLikelihoodsProperty { get; set; }
        public string[]? StopSequencesProperty { get; set; }
        public double? TemperatureProperty { get; set; }
    }

    public record DatabricksConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-databricks";
        public override string Type => TypeValue;

        public string Endpoint { get; set; } = string.Empty;
        public int? MaxTokens { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record FriendliAIConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-friendliai";
        public override string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record MistralConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-mistral";
        public override string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record NvidiaConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-nvidia";
        public override string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
    }

    public record OllamaConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-ollama";
        public override string Type => TypeValue;

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

    public record GoogleConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-google";
        public override string Type => TypeValue;

        public string? ApiEndpoint { get; set; }
        public int? MaxOutputTokens { get; set; }
        public string? ModelId { get; set; }
        public string? ProjectId { get; set; }
        public double? Temperature { get; set; }
        public int? TopK { get; set; }
        public double? TopP { get; set; }
    }

    public record XAIConfig : GenerativeConfig
    {
        public const string TypeValue = "generative-xai";
        public override string Type => TypeValue;

        public string? BaseURL { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
    }
}

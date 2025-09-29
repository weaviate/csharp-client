using System.Text.Json;

namespace Weaviate.Client.Models;

internal static class GenerativeConfigSerialization
{
    internal static IGenerativeConfig? Factory(string? type, object? config)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);

        if (config is JsonElement vic)
        {
            var result = type switch
            {
                Generative.AnthropicConfig.TypeValue => (IGenerativeConfig?)
                    JsonSerializer.Deserialize<Generative.AnthropicConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.AnyscaleConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.AnyscaleConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.AWSConfig.TypeValue => JsonSerializer.Deserialize<Generative.AWSConfig>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Generative.CohereConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.CohereConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.DatabricksConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.DatabricksConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.FriendliAIConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.FriendliAIConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.MistralConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.MistralConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.OpenAIConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.OpenAIConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.AzureOpenAIConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.AzureOpenAIConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.GoogleConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.GoogleConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.XAIConfig.TypeValue => JsonSerializer.Deserialize<Generative.XAIConfig>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Generative.NvidiaConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.NvidiaConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Generative.OllamaConfig.TypeValue =>
                    JsonSerializer.Deserialize<Generative.OllamaConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                _ => new Generative.Custom
                {
                    Type = type,
                    Config = ObjectHelper.ConvertJsonElement(
                        (
                            (dynamic?)
                                JsonSerializer.Deserialize<System.Dynamic.ExpandoObject>(
                                    vic.GetRawText(),
                                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                                )
                        )?.config
                    ),
                },
            };

            return result;
        }

        throw new WeaviateClientException("Unable to create GenerativeConfig");
    }
}

using System.Text.Json;

namespace Weaviate.Client.Models;

internal static class GenerativeConfigSerialization
{
    internal static IGenerativeConfig? Factory(string? type, object? config)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);

        if (config is JsonElement vic)
        {
            var text = vic.GetRawText();

            var result = type switch
            {
                GenerativeConfig.Anthropic.TypeValue => (IGenerativeConfig?)
                    JsonSerializer.Deserialize<GenerativeConfig.Anthropic>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.Anyscale.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Anyscale>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.AWS.TypeValue => JsonSerializer.Deserialize<GenerativeConfig.AWS>(
                    text,
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                GenerativeConfig.Cohere.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Cohere>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.Databricks.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Databricks>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.FriendliAI.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.FriendliAI>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.Mistral.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Mistral>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.OpenAI.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.OpenAI>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.AzureOpenAI.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.AzureOpenAI>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.GoogleGemini.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.GoogleGemini>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.GoogleVertex.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.GoogleVertex>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.XAI.TypeValue => JsonSerializer.Deserialize<GenerativeConfig.XAI>(
                    text,
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                GenerativeConfig.Nvidia.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Nvidia>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.Ollama.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Ollama>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                _ => new GenerativeConfig.Custom
                {
                    Type = type,
                    Config =
                        ObjectHelper.ConvertJsonElement(
                            (
                                JsonSerializer.Deserialize<JsonElement>(
                                    text,
                                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                                )
                            ).TryGetProperty("config", out var configElement)
                                ? configElement
                                : null
                        ) ?? new { },
                },
            };

            return result;
        }

        throw new WeaviateClientException("Unable to create GenerativeConfig");
    }
}

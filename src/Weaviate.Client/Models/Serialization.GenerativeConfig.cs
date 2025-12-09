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
                GenerativeConfig.Anthropic.TypeValue => (IGenerativeConfig?)
                    JsonSerializer.Deserialize<GenerativeConfig.Anthropic>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.Anyscale.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Anyscale>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.AWS.TypeValue => JsonSerializer.Deserialize<GenerativeConfig.AWS>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                GenerativeConfig.Cohere.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Cohere>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.Databricks.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Databricks>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.FriendliAI.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.FriendliAI>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.Mistral.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Mistral>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.OpenAI.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.OpenAI>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.AzureOpenAI.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.AzureOpenAI>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.GoogleGemini.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.GoogleGemini>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.GoogleVertex.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.GoogleVertex>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.XAI.TypeValue => JsonSerializer.Deserialize<GenerativeConfig.XAI>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                GenerativeConfig.Nvidia.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Nvidia>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                GenerativeConfig.Ollama.TypeValue =>
                    JsonSerializer.Deserialize<GenerativeConfig.Ollama>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                _ => new GenerativeConfig.Custom
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

using System.Text.Json;

namespace Weaviate.Client.Models;

internal static class RerankerConfigSerialization
{
    internal static IRerankerConfig? Factory(string? type, object? config)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);

        if (config is JsonElement vic)
        {
            var text = vic.GetRawText();

            var result = type switch
            {
                Reranker.Transformers.TypeValue => (IRerankerConfig?)
                    JsonSerializer.Deserialize<Reranker.Transformers>(
                        text,
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Reranker.Cohere.TypeValue => JsonSerializer.Deserialize<Reranker.Cohere>(
                    text,
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Reranker.VoyageAI.TypeValue => JsonSerializer.Deserialize<Reranker.VoyageAI>(
                    text,
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Reranker.JinaAI.TypeValue => JsonSerializer.Deserialize<Reranker.JinaAI>(
                    text,
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Reranker.Nvidia.TypeValue => JsonSerializer.Deserialize<Reranker.Nvidia>(
                    text,
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Reranker.None.TypeValue => JsonSerializer.Deserialize<Reranker.None>(
                    text,
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                _ => new Reranker.Custom
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

        throw new WeaviateClientException("Unable to create RerankerConfig");
    }
}

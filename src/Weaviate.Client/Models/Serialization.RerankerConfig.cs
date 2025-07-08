using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

internal static class RerankerConfigSerialization
{
    internal static RerankerConfig? Factory(string? type, object? config)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);

        if (config is JsonElement vic)
        {
            var result = type switch
            {
                Reranker.TransformersConfig.TypeValue => (RerankerConfig?)
                    JsonSerializer.Deserialize<Reranker.TransformersConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Reranker.CohereConfig.TypeValue =>
                    JsonSerializer.Deserialize<Reranker.CohereConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Reranker.VoyageAIConfig.TypeValue =>
                    JsonSerializer.Deserialize<Reranker.VoyageAIConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Reranker.JinaAIConfig.TypeValue =>
                    JsonSerializer.Deserialize<Reranker.JinaAIConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Reranker.NvidiaConfig.TypeValue =>
                    JsonSerializer.Deserialize<Reranker.NvidiaConfig>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Reranker.NoneConfig.TypeValue => JsonSerializer.Deserialize<Reranker.NoneConfig>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                _ => new Reranker.Custom<dynamic>(type),
            };

            return result;
        }

        throw new WeaviateException("Unable to create RerankerConfig");
    }
}

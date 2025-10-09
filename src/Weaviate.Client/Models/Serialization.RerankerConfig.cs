using System.Text.Json;

namespace Weaviate.Client.Models;

internal static class RerankerConfigSerialization
{
    internal static IRerankerConfig? Factory(string? type, object? config)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);

        if (config is JsonElement vic)
        {
            var result = type switch
            {
                Reranker.Transformers.TypeValue => (IRerankerConfig?)
                    JsonSerializer.Deserialize<Reranker.Transformers>(
                        vic.GetRawText(),
                        Rest.WeaviateRestClient.RestJsonSerializerOptions
                    ),
                Reranker.Cohere.TypeValue => JsonSerializer.Deserialize<Reranker.Cohere>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Reranker.VoyageAI.TypeValue => JsonSerializer.Deserialize<Reranker.VoyageAI>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Reranker.JinaAI.TypeValue => JsonSerializer.Deserialize<Reranker.JinaAI>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Reranker.Nvidia.TypeValue => JsonSerializer.Deserialize<Reranker.Nvidia>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                Reranker.None.TypeValue => JsonSerializer.Deserialize<Reranker.None>(
                    vic.GetRawText(),
                    Rest.WeaviateRestClient.RestJsonSerializerOptions
                ),
                _ => new Reranker.Custom
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

        throw new WeaviateClientException("Unable to create RerankerConfig");
    }
}

namespace Weaviate.Client;

public static class WeaviateClientBuilderExtensions
{
    public static WeaviateClientBuilder WithOpenAI(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-OpenAI-Api-Key", apiKey);

    public static WeaviateClientBuilder WithCohere(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Cohere-Api-Key", apiKey);

    public static WeaviateClientBuilder WithJinaAI(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Jinaai-Api-Key", apiKey);

    public static WeaviateClientBuilder WithHuggingFace(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Huggingface-Api-Key", apiKey);

    public static WeaviateClientBuilder WithVoyageAI(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Voyageai-Api-Key", apiKey);

    public static WeaviateClientBuilder WithMistral(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Mistral-Api-Key", apiKey);

    public static WeaviateClientBuilder WithAWS(
        this WeaviateClientBuilder builder,
        string accessKey,
        string secretKey
    )
    {
        builder.WithHeader("X-Aws-Access-Key", accessKey);
        builder.WithHeader("X-Aws-Secret-Key", secretKey);
        return builder;
    }
}

namespace Weaviate.Client;

/// <summary>
/// The weaviate client builder extensions class
/// </summary>
public static class WeaviateClientBuilderExtensions
{
    /// <summary>
    /// Adds the open ai using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="apiKey">The api key</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder WithOpenAI(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-OpenAI-Api-Key", apiKey);

    /// <summary>
    /// Adds the cohere using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="apiKey">The api key</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder WithCohere(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Cohere-Api-Key", apiKey);

    /// <summary>
    /// Adds the jina ai using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="apiKey">The api key</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder WithJinaAI(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Jinaai-Api-Key", apiKey);

    /// <summary>
    /// Adds the hugging face using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="apiKey">The api key</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder WithHuggingFace(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Huggingface-Api-Key", apiKey);

    /// <summary>
    /// Adds the voyage ai using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="apiKey">The api key</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder WithVoyageAI(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Voyageai-Api-Key", apiKey);

    /// <summary>
    /// Adds the mistral using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="apiKey">The api key</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder WithMistral(
        this WeaviateClientBuilder builder,
        string apiKey
    ) => builder.WithHeader("X-Mistral-Api-Key", apiKey);

    /// <summary>
    /// Adds the aws using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="accessKey">The access key</param>
    /// <param name="secretKey">The secret key</param>
    /// <returns>The builder</returns>
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

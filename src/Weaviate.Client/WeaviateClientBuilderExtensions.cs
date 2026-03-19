namespace Weaviate.Client;

/// <summary>
/// The weaviate client builder extensions class
/// </summary>
public static class WeaviateClientBuilderExtensions
{
    /// <summary>
    /// Sets the <c>X-Weaviate-Client-Integration</c> header to identify a higher-level library
    /// built on top of the core client.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="integrationValue">
    /// An integration identifier in <c>name/version</c> format, e.g.
    /// <c>weaviate-client-csharp-managed/1.0.0</c>. Multiple tokens can be space-separated.
    /// </param>
    /// <returns>The builder for method chaining.</returns>
    public static WeaviateClientBuilder WithIntegration(
        this WeaviateClientBuilder builder,
        string integrationValue
    )
    {
        if (integrationValue.Any(char.IsWhiteSpace))
            throw new ArgumentException(
                "Integration value must not contain whitespace.",
                nameof(integrationValue)
            );
        return builder.WithHeader(WeaviateDefaults.IntegrationHeader, integrationValue);
    }

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

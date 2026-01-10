using Weaviate.Client.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Factory for creating reranker configurations.
/// </summary>
public class RerankerConfigFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RerankerConfigFactory"/> class
    /// </summary>
    internal RerankerConfigFactory() { }

#pragma warning disable CA1822 // Mark members as static

    /// <summary>
    /// Creates a reranker configuration for Transformers.
    /// </summary>
    /// <returns>A new <see cref="IRerankerConfig"/> instance configured for Transformers.</returns>
    public IRerankerConfig Transformers() => new Reranker.Transformers();

    /// <summary>
    /// Creates a reranker configuration for Cohere.
    /// </summary>
    /// <param name="model">The model to use.</param>
    /// <returns>A new <see cref="IRerankerConfig"/> instance configured for Cohere.</returns>
    public IRerankerConfig Cohere(string? model = null) => new Reranker.Cohere { Model = model };

    /// <summary>
    /// Creates a reranker configuration for ContextualAI.
    /// </summary>
    /// <param name="model">The model to use.</param>
    /// <param name="instruction">The instruction for reranking.</param>
    /// <param name="topN">The number of results to return.</param>
    /// <returns>A new <see cref="IRerankerConfig"/> instance configured for ContextualAI.</returns>
    public IRerankerConfig ContextualAI(
        string? model = null,
        string? instruction = null,
        int? topN = null
    ) =>
        new Reranker.ContextualAI
        {
            Model = model,
            Instruction = instruction,
            TopN = topN,
        };

    /// <summary>
    /// Creates a reranker configuration for VoyageAI.
    /// </summary>
    /// <param name="model">The model to use.</param>
    /// <returns>A new <see cref="IRerankerConfig"/> instance configured for VoyageAI.</returns>
    public IRerankerConfig VoyageAI(string? model = null) =>
        new Reranker.VoyageAI { Model = model };

    /// <summary>
    /// Creates a reranker configuration for JinaAI.
    /// </summary>
    /// <param name="model">The model to use.</param>
    /// <returns>A new <see cref="IRerankerConfig"/> instance configured for JinaAI.</returns>
    public IRerankerConfig JinaAI(string? model = null) => new Reranker.JinaAI { Model = model };

    /// <summary>
    /// Creates a reranker configuration for Nvidia.
    /// </summary>
    /// <param name="baseURL">The base URL for the Nvidia API.</param>
    /// <param name="model">The model to use.</param>
    /// <returns>A new <see cref="IRerankerConfig"/> instance configured for Nvidia.</returns>
    public IRerankerConfig Nvidia(string? baseURL = null, string? model = null) =>
        new Reranker.Nvidia { BaseURL = baseURL, Model = model };

    /// <summary>
    /// Creates a custom reranker configuration.
    /// </summary>
    /// <param name="type">The type of the custom reranker.</param>
    /// <param name="config">The configuration object for the custom reranker.</param>
    /// <returns>A new <see cref="IRerankerConfig"/> instance configured for a custom reranker.</returns>
    public IRerankerConfig Custom(string type, object? config = null) =>
        new Reranker.Custom { Type = type, Config = config ?? new { } };

    /// <summary>
    /// Creates a reranker configuration indicating no reranker should be used.
    /// </summary>
    /// <returns>A new <see cref="IRerankerConfig"/> instance configured for no reranker.</returns>
    public IRerankerConfig None() => new Reranker.None();
#pragma warning restore CA1822 // Mark members as static
}

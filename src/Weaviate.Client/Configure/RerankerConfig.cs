using Weaviate.Client.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

public class RerankerConfigFactory
{
    internal RerankerConfigFactory() { }

#pragma warning disable CA1822 // Mark members as static
    public IRerankerConfig Transformers() => new Reranker.Transformers();

    public IRerankerConfig Cohere(string? model = null) => new Reranker.Cohere { Model = model };

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

    public IRerankerConfig VoyageAI(string? model = null) =>
        new Reranker.VoyageAI { Model = model };

    public IRerankerConfig JinaAI(string? model = null) => new Reranker.JinaAI { Model = model };

    public IRerankerConfig Nvidia(string? baseURL = null, string? model = null) =>
        new Reranker.Nvidia { BaseURL = baseURL, Model = model };

    public IRerankerConfig Custom(string type, object? config = null) =>
        new Reranker.Custom { Type = type, Config = config };

    public IRerankerConfig None() => new Reranker.None();
#pragma warning restore CA1822 // Mark members as static
}

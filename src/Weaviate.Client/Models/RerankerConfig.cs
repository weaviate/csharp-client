using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

public interface IRerankerConfig
{
    [JsonIgnore]
    string Type { get; }
}

public static class Reranker
{
    public record Transformers : IRerankerConfig
    {
        public const string TypeValue = "reranker-transformers";
        public string Type => TypeValue;
    }

    public record Cohere : IRerankerConfig
    {
        public const string TypeValue = "reranker-cohere";
        public string Type => TypeValue;

        public string? Model { get; set; }

        // Helper constants for known models
        public static class Models
        {
            public const string RerankEnglishV2 = "rerank-english-v2.0";
            public const string RerankMultilingualV2 = "rerank-multilingual-v2.0";
        }
    }

    public record ContextualAI : IRerankerConfig
    {
        public const string TypeValue = "reranker-contextualai";
        public string Type => TypeValue;

        public string? Model { get; set; }
        public string? Instruction { get; set; }
        public int? TopN { get; set; }
    }

    public record VoyageAI : IRerankerConfig
    {
        public const string TypeValue = "reranker-voyageai";
        public string Type => TypeValue;

        public string? Model { get; set; }

        public static class Models
        {
            public const string Rerank25 = "rerank-2.5";
            public const string Rerank25Lite = "rerank-2.5-lite";
            public const string Rerank2 = "rerank-2";
            public const string Rerank2Lite = "rerank-2-lite";
            public const string Rerank1 = "rerank-1";
            public const string RerankLite1 = "rerank-lite-1";
        }
    }

    public record JinaAI : IRerankerConfig
    {
        public const string TypeValue = "reranker-jinaai";
        public string Type => TypeValue;

        public string? Model { get; set; }

        public static class Models
        {
            public const string JinaRerankerV2BaseMultilingual =
                "jina-reranker-v2-base-multilingual";
            public const string JinaRerankerV1BaseEn = "jina-reranker-v1-base-en";
            public const string JinaRerankerV1TurboEn = "jina-reranker-v1-turbo-en";
            public const string JinaRerankerV1TinyEn = "jina-reranker-v1-tiny-en";
            public const string JinaColbertV1En = "jina-colbert-v1-en";
        }
    }

    public record Nvidia : IRerankerConfig
    {
        public const string TypeValue = "reranker-nvidia";
        public string Type => TypeValue;

        public string? BaseURL { get; set; }
        public string? Model { get; set; }

        public static class Models
        {
            public const string NvidiaRerankQaMistral4B = "nvidia/rerank-qa-mistral-4b";
        }
    }

    public record Custom : IRerankerConfig
    {
        public required string Type { get; init; }

        public dynamic? Config { get; set; }
    }

    // Special case for "none" - no configuration needed
    public record None : IRerankerConfig
    {
        public const string TypeValue = "none";
        public string Type => TypeValue;
    }
}

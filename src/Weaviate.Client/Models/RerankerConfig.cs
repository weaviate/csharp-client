namespace Weaviate.Client.Models;

// TODO Ask: Is it worth using string to allow use of custom models in reranker configs, or if an Enum is enough.

// Base record for all reranker configurations
public abstract record RerankerConfig
{
    public abstract string Type { get; }
}

public static class Reranker
{
    public record TransformersConfig : RerankerConfig
    {
        public const string TypeValue = "reranker-transformers";
        public override string Type => TypeValue;
    }

    public record CohereConfig : RerankerConfig
    {
        public const string TypeValue = "reranker-cohere";
        public override string Type => TypeValue;

        public string? Model { get; set; }

        // Helper constants for known models
        public static class Models
        {
            public const string RerankEnglishV2 = "rerank-english-v2.0";
            public const string RerankMultilingualV2 = "rerank-multilingual-v2.0";
        }
    }

    public record VoyageAIConfig : RerankerConfig
    {
        public const string TypeValue = "reranker-voyageai";
        public override string Type => TypeValue;

        public string? BaseURL { get; set; }
        public string? Model { get; set; }

        public static class Models
        {
            public const string RerankLite1 = "rerank-lite-1";
        }
    }

    public record JinaAIConfig : RerankerConfig
    {
        public const string TypeValue = "reranker-jinaai";
        public override string Type => TypeValue;

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

    public record NvidiaConfig : RerankerConfig
    {
        public const string TypeValue = "reranker-nvidia";
        public override string Type => TypeValue;

        public string? BaseURL { get; set; }
        public string? Model { get; set; }

        public static class Models
        {
            public const string NvidiaRerankQaMistral4B = "nvidia/rerank-qa-mistral-4b";
        }
    }

    public record Custom<T>(string TypeValue) : RerankerConfig
    {
        public override string Type => TypeValue;

        public T? Config { get; set; }
    }

    // Special case for "none" - no configuration needed
    public record NoneConfig : RerankerConfig
    {
        public const string TypeValue = "none";
        public override string Type => TypeValue;
    }
}

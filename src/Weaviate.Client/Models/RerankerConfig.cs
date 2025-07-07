namespace Weaviate.Client.Models;

// TODO Ask: Is it worth using string to allow use of custom models in reranker configs, or if an Enum is enough.

// Base class for all reranker configurations
public abstract class RerankerConfig
{
    public abstract string Type { get; }
}

// Specific configuration classes
public class RerankerTransformersConfig : RerankerConfig
{
    public const string TypeValue = "reranker-transformers";
    public override string Type => TypeValue;

    // Empty config class - could add properties later
}

public class RerankerCohereConfig : RerankerConfig
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

public class RerankerVoyageAIConfig : RerankerConfig
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

public class RerankerJinaAIConfig : RerankerConfig
{
    public const string TypeValue = "reranker-jinaai";
    public override string Type => TypeValue;

    public string? Model { get; set; }

    public static class Models
    {
        public const string JinaRerankerV2BaseMultilingual = "jina-reranker-v2-base-multilingual";
        public const string JinaRerankerV1BaseEn = "jina-reranker-v1-base-en";
        public const string JinaRerankerV1TurboEn = "jina-reranker-v1-turbo-en";
        public const string JinaRerankerV1TinyEn = "jina-reranker-v1-tiny-en";
        public const string JinaColbertV1En = "jina-colbert-v1-en";
    }
}

public class RerankerNvidiaConfig : RerankerConfig
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

// Special case for "none" - no configuration needed
public class RerankerNoneConfig : RerankerConfig
{
    public const string TypeValue = "none";
    public override string Type => TypeValue;
}

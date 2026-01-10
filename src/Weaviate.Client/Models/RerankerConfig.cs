using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// The reranker config interface
/// </summary>
public interface IRerankerConfig
{
    /// <summary>
    /// Gets the value of the type
    /// </summary>
    [JsonIgnore]
    string Type { get; }
}

/// <summary>
/// The reranker class
/// </summary>
public static class Reranker
{
    /// <summary>
    /// The transformers
    /// </summary>
    public record Transformers : IRerankerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Transformers"/> class
        /// </summary>
        [JsonConstructor]
        internal Transformers() { }

        /// <summary>
        /// The type value
        /// </summary>
        public const string TypeValue = "reranker-transformers";

        /// <summary>
        /// Gets the value of the type
        /// </summary>
        public string Type => TypeValue;
    }

    /// <summary>
    /// The cohere
    /// </summary>
    public record Cohere : IRerankerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cohere"/> class
        /// </summary>
        [JsonConstructor]
        internal Cohere() { }

        /// <summary>
        /// The type value
        /// </summary>
        public const string TypeValue = "reranker-cohere";

        /// <summary>
        /// Gets the value of the type
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; }

        // Helper constants for known models
        /// <summary>
        /// The models class
        /// </summary>
        public static class Models
        {
            /// <summary>
            /// The rerank english
            /// </summary>
            public const string RerankEnglishV2 = "rerank-english-v2.0";

            /// <summary>
            /// The rerank multilingual
            /// </summary>
            public const string RerankMultilingualV2 = "rerank-multilingual-v2.0";
        }
    }

    /// <summary>
    /// The contextual ai
    /// </summary>
    public record ContextualAI : IRerankerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualAI"/> class
        /// </summary>
        [JsonConstructor]
        internal ContextualAI() { }

        /// <summary>
        /// The type value
        /// </summary>
        public const string TypeValue = "reranker-contextualai";

        /// <summary>
        /// Gets the value of the type
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the value of the instruction
        /// </summary>
        public string? Instruction { get; set; }

        /// <summary>
        /// Gets or sets the value of the top n
        /// </summary>
        public int? TopN { get; set; }
    }

    /// <summary>
    /// The voyage ai
    /// </summary>
    public record VoyageAI : IRerankerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoyageAI"/> class
        /// </summary>
        [JsonConstructor]
        internal VoyageAI() { }

        /// <summary>
        /// The type value
        /// </summary>
        public const string TypeValue = "reranker-voyageai";

        /// <summary>
        /// Gets the value of the type
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The models class
        /// </summary>
        public static class Models
        {
            /// <summary>
            /// The rerank 25
            /// </summary>
            public const string Rerank25 = "rerank-2.5";

            /// <summary>
            /// The rerank 25 lite
            /// </summary>
            public const string Rerank25Lite = "rerank-2.5-lite";

            /// <summary>
            /// The rerank
            /// </summary>
            public const string Rerank2 = "rerank-2";

            /// <summary>
            /// The rerank lite
            /// </summary>
            public const string Rerank2Lite = "rerank-2-lite";

            /// <summary>
            /// The rerank
            /// </summary>
            public const string Rerank1 = "rerank-1";

            /// <summary>
            /// The rerank lite
            /// </summary>
            public const string RerankLite1 = "rerank-lite-1";
        }
    }

    /// <summary>
    /// The jina ai
    /// </summary>
    public record JinaAI : IRerankerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JinaAI"/> class
        /// </summary>
        [JsonConstructor]
        internal JinaAI() { }

        /// <summary>
        /// The type value
        /// </summary>
        public const string TypeValue = "reranker-jinaai";

        /// <summary>
        /// Gets the value of the type
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The models class
        /// </summary>
        public static class Models
        {
            /// <summary>
            /// The jina reranker base multilingual
            /// </summary>
            public const string JinaRerankerV2BaseMultilingual =
                "jina-reranker-v2-base-multilingual";

            /// <summary>
            /// The jina reranker base en
            /// </summary>
            public const string JinaRerankerV1BaseEn = "jina-reranker-v1-base-en";

            /// <summary>
            /// The jina reranker turbo en
            /// </summary>
            public const string JinaRerankerV1TurboEn = "jina-reranker-v1-turbo-en";

            /// <summary>
            /// The jina reranker tiny en
            /// </summary>
            public const string JinaRerankerV1TinyEn = "jina-reranker-v1-tiny-en";

            /// <summary>
            /// The jina colbert en
            /// </summary>
            public const string JinaColbertV1En = "jina-colbert-v1-en";
        }
    }

    /// <summary>
    /// The nvidia
    /// </summary>
    public record Nvidia : IRerankerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Nvidia"/> class
        /// </summary>
        [JsonConstructor]
        internal Nvidia() { }

        /// <summary>
        /// The type value
        /// </summary>
        public const string TypeValue = "reranker-nvidia";

        /// <summary>
        /// Gets the value of the type
        /// </summary>
        public string Type => TypeValue;

        /// <summary>
        /// Gets or sets the value of the base url
        /// </summary>
        public string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the value of the model
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The models class
        /// </summary>
        public static class Models
        {
            /// <summary>
            /// The nvidia rerank qa mistral
            /// </summary>
            public const string NvidiaRerankQaMistral4B = "nvidia/rerank-qa-mistral-4b";
        }
    }

    /// <summary>
    /// The custom
    /// </summary>
    public record Custom : IRerankerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Custom"/> class
        /// </summary>
        [JsonConstructor]
        internal Custom() { }

        /// <summary>
        /// Gets or inits the value of the type
        /// </summary>
        public required string Type { get; init; }

        /// <summary>
        /// Gets or sets the value of the config
        /// </summary>
        public object Config { get; set; } = new { };
    }

    // Special case for "none" - no configuration needed
    /// <summary>
    /// The none
    /// </summary>
    public record None : IRerankerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="None"/> class
        /// </summary>
        [JsonConstructor]
        internal None() { }

        /// <summary>
        /// The type value
        /// </summary>
        public const string TypeValue = "none";

        /// <summary>
        /// Gets the value of the type
        /// </summary>
        public string Type => TypeValue;
    }
}

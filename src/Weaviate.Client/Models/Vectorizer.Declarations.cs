namespace Weaviate.Client.Models;

public static partial class Vectorizer
{
    // All record constructors for derived types
    public partial record SelfProvided : VectorizerConfig
    {
        public const string IdentifierValue = "none";

        public SelfProvided()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for image vectorization using a neural network module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Img2VecNeural : VectorizerConfig
    {
        public const string IdentifierValue = "img2vec-neural";

        public Img2VecNeural()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The field configuration for multi-media vectorization.
    /// </summary>
    // This record is not derived from VectorizerConfig, but its declaration is included here
    // as it defines the structure for Multi2VecField. Its properties will be in Records.cs.
    public partial record Multi2VecField
    {
        // Implicit constructor used
    }

    /// <summary>
    /// The weights configuration for multi-media vectorization.
    /// </summary>
    public partial record Multi2VecWeights
    {
        // Implicit constructor used
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the CLIP module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Multi2VecClip : VectorizerConfig
    {
        public const string IdentifierValue = "multi2vec-clip";

        public Multi2VecClip()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The weights configuration for Cohere multi-media vectorization.
    /// </summary>
    public partial record Multi2VecCohereWeights
    {
        // Implicit constructor used
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Cohere module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Multi2VecCohere : VectorizerConfig
    {
        public const string IdentifierValue = "multi2vec-cohere";

        public Multi2VecCohere()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The weights configuration for Bind multi-media vectorization.
    /// </summary>
    public partial record Multi2VecBindWeights
    {
        // Implicit constructor used
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Bind module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Multi2VecBind : VectorizerConfig
    {
        public const string IdentifierValue = "multi2vec-bind";

        public Multi2VecBind()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The weights configuration for Google multi-media vectorization.
    /// </summary>
    public partial record Multi2VecGoogleWeights
    {
        // Implicit constructor used
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Google module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Multi2VecGoogle : VectorizerConfig
    {
        public const string IdentifierValue = "multi2vec-palm";

        public Multi2VecGoogle()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// Deprecated. Use Multi2VecGoogleConfig instead.
    /// </summary>
    public partial record Multi2VecPalm : Multi2VecGoogle
    {
        // Inherits constructor from Multi2VecGoogleConfig
    }

    /// <summary>
    /// The weights configuration for JinaAI multi-media vectorization.
    /// </summary>
    public partial record Multi2VecJinaAIWeights
    {
        // Implicit constructor used
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Multi2VecJinaAI : VectorizerConfig
    {
        public const string IdentifierValue = "multi2vec-jinaai";

        public Multi2VecJinaAI()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The weights configuration for VoyageAI multi-media vectorization.
    /// </summary>
    public partial record Multi2VecVoyageAIWeights
    {
        // Implicit constructor used
    }

    /// <summary>
    /// The configuration for multi-media vectorization using the VoyageAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Multi2VecVoyageAI : VectorizerConfig
    {
        public const string IdentifierValue = "multi2vec-voyageai";

        public Multi2VecVoyageAI()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for reference-based vectorization using the centroid method.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Ref2VecCentroid : VectorizerConfig
    {
        public const string IdentifierValue = "ref2vec-centroid";

        public Ref2VecCentroid()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the AWS module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecAWS : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-aws";

        public Text2VecAWS()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the OpenAI module with Azure.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecAzureOpenAI : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-azure-openai";

        public Text2VecAzureOpenAI()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Cohere module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecCohere : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-cohere";

        public Text2VecCohere()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Contextionary module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecContextionary : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-contextionary";

        public Text2VecContextionary()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Databricks module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecDatabricks : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-databricks";

        public Text2VecDatabricks()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the GPT-4-All module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecGPT4All : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-gpt4all";

        public Text2VecGPT4All()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the HuggingFace module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecHuggingFace : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-huggingface";

        public Text2VecHuggingFace()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecJinaAI : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-jinaai";

        public Text2VecJinaAI()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// Deprecated. Use Text2VecJinaAIConfig instead.
    /// </summary>
    public partial record Text2VecJinaConfig : Text2VecJinaAI
    {
        // Inherits constructor from Text2VecJinaAIConfig
    }

    /// <summary>
    /// The configuration for text vectorization using the Nvidia module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecNvidia : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-nvidia";

        public Text2VecNvidia()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Mistral module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecMistral : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-mistral";

        public Text2VecMistral()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Ollama module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecOllama : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-ollama";

        public Text2VecOllama()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the OpenAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecOpenAI : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-openai";

        public Text2VecOpenAI()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// Deprecated. Use Text2VecGoogleConfig instead.
    /// </summary>
    public partial record Text2VecPalm : Text2VecGoogle
    {
        // Inherits constructor from Multi2VecGoogleConfig
    }

    /// <summary>
    /// The configuration for text vectorization using the Google module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecGoogle : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-palm";

        public Text2VecGoogle()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Transformers module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecTransformers : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-transformers";

        public Text2VecTransformers()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using the VoyageAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecVoyageAI : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-voyageai";

        public Text2VecVoyageAI()
            : base(IdentifierValue) { }
    }

    /// <summary>
    /// The configuration for text vectorization using Weaviate's self-hosted text-based embedding models.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecWeaviate : VectorizerConfig
    {
        public const string IdentifierValue = "text2vec-weaviate";

        public Text2VecWeaviate()
            : base(IdentifierValue) { }
    }
}

namespace Weaviate.Client.Models.Vectorizers;

public abstract partial record VectorizerConfig
{
    // All record constructors for derived types
    public partial record None : VectorizerConfig
    {
        public None()
            : base("none") { }
    }

    /// <summary>
    /// The configuration for image vectorization using a neural network module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Img2VecNeural : VectorizerConfig
    {
        public Img2VecNeural()
            : base("img2vec-neural") { }
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
        public Multi2VecClip()
            : base("multi2vec-clip") { }
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
        public Multi2VecCohere()
            : base("multi2vec-cohere") { }
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
        public Multi2VecBind()
            : base("multi2vec-bind") { }
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
        public Multi2VecGoogle()
            : base("multi2vec-palm") { }
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
        public Multi2VecJinaAI()
            : base("multi2vec-jinaai") { }
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
        public Multi2VecVoyageAI()
            : base("multi2vec-voyageai") { }
    }

    /// <summary>
    /// The configuration for reference-based vectorization using the centroid method.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Ref2VecCentroid : VectorizerConfig
    {
        public Ref2VecCentroid()
            : base("ref2vec-centroid") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the AWS module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecAWS : VectorizerConfig
    {
        public Text2VecAWS()
            : base("text2vec-aws") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the OpenAI module with Azure.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecAzureOpenAI : VectorizerConfig
    {
        public Text2VecAzureOpenAI()
            : base("text2vec-azure-openai") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Cohere module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecCohere : VectorizerConfig
    {
        public Text2VecCohere()
            : base("text2vec-cohere") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Contextionary module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecContextionary : VectorizerConfig
    {
        public Text2VecContextionary(bool? vectorizeClassName = null)
            : base("text2vec-contextionary")
        {
            VectorizeClassName = vectorizeClassName;
        }
    }

    /// <summary>
    /// The configuration for text vectorization using the Databricks module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecDatabricks : VectorizerConfig
    {
        public Text2VecDatabricks()
            : base("text2vec-databricks") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the GPT-4-All module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecGPT4All : VectorizerConfig
    {
        public Text2VecGPT4All()
            : base("text2vec-gpt4all") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the HuggingFace module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecHuggingFace : VectorizerConfig
    {
        public Text2VecHuggingFace()
            : base("text2vec-huggingface") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Jina module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecJinaAI : VectorizerConfig
    {
        public Text2VecJinaAI()
            : base("text2vec-jinaai") { }
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
        public Text2VecNvidia()
            : base("text2vec-nvidia") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Mistral module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecMistral : VectorizerConfig
    {
        public Text2VecMistral()
            : base("text2vec-mistral") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Ollama module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecOllama : VectorizerConfig
    {
        public Text2VecOllama()
            : base("text2vec-ollama") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the OpenAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecOpenAI : VectorizerConfig
    {
        public Text2VecOpenAI()
            : base("text2vec-openai") { }
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
        public Text2VecGoogle()
            : base("text2vec-palm") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the Transformers module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecTransformers : VectorizerConfig
    {
        public Text2VecTransformers()
            : base("text2vec-transformers") { }
    }

    /// <summary>
    /// The configuration for text vectorization using the VoyageAI module.
    /// See the documentation for detailed usage.
    /// </summary>
    public partial record Text2VecVoyageAI : VectorizerConfig
    {
        public Text2VecVoyageAI()
            : base("text2vec-voyageai") { }
    }

    /// <summary>
    /// The configuration for text vectorization using Weaviate's self-hosted text-based embedding models.
    /// See the documentation for detailed usage.
    /// </summary>
        public partial record Text2VecWeaviate : VectorizerConfig
    {
        public bool? VectorizeClassName { get; init; }

        public Text2VecWeaviate(bool? vectorizeClassName = null)
            : base("text2vec-weaviate")
        {
            VectorizeClassName = vectorizeClassName;
        }
    }
}

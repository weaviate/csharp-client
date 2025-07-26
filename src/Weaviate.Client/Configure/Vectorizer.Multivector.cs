using Weaviate.Client.Models;

namespace Weaviate.Client;

public static partial class Configure
{
    public static class MultiVectors
    {
        public static VectorConfig SelfProvided(
            string name = "default",
            VectorIndex.HNSW? indexConfig = null
        )
        {
            return new VectorConfigBuilder(new Vectorizer.SelfProvided()).New(name, indexConfig);
        }

        public class VectorConfigBuilder(VectorizerConfig Config)
        {
            public VectorConfig New(
                string name,
                VectorIndex.HNSW? indexConfig = null,
                params string[] properties
            )
            {
                indexConfig ??= new VectorIndex.HNSW()
                {
                    MultiVector = new VectorIndexConfig.MultiVectorConfig(),
                };

                indexConfig.MultiVector ??= new VectorIndexConfig.MultiVectorConfig();

                return new(
                    name,
                    vectorizer: Config with
                    {
                        Properties = properties,
                    },
                    vectorIndexConfig: indexConfig
                );
            }
        }

        public static VectorConfigBuilder Multi2VecJinaAI(
            string? baseURL = null,
            int? dimensions = null,
            string[]? imageFields = null,
            string? model = null,
            string[]? textFields = null,
            bool? vectorizeCollectionName = null,
            Vectorizer.Multi2VecJinaAIWeights? weights = null
        ) =>
            new(
                new Vectorizer.Multi2VecJinaAI
                {
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    ImageFields = imageFields,
                    Model = model,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = weights,
                }
            );

        public static VectorConfigBuilder Text2VecJinaAI(
            string? model = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2VecJinaAI
                {
                    Model = model,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );
    }
}

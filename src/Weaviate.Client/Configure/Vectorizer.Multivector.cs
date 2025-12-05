using Weaviate.Client.Models;
using static Weaviate.Client.Models.VectorIndexConfig;
using static Weaviate.Client.Models.Vectorizer;

namespace Weaviate.Client;

public static partial class Configure
{
    public static class MultiVectors
    {
        public static VectorConfigBuilder SelfProvided() => new(new Vectorizer.SelfProvided());

        public class VectorConfigBuilder(VectorizerConfig Config)
        {
            public VectorConfig New(
                string name = "default",
                VectorIndex.HNSW? indexConfig = null,
                QuantizerConfigBase? quantizerConfig = null,
                params string[] sourceProperties
            )
            {
                indexConfig ??= new VectorIndex.HNSW()
                {
                    MultiVector = new VectorIndexConfig.MultiVectorConfig(),
                };

                indexConfig.MultiVector ??= new VectorIndexConfig.MultiVectorConfig();

                if (quantizerConfig is not null && indexConfig.Quantizer is not null)
                {
                    throw new WeaviateClientException(
                        new InvalidOperationException(
                            "Quantizer is already set on the indexConfig. Please provide either the quantizerConfig or set it on the indexConfig, not both."
                        )
                    );
                }

                return new(
                    name,
                    vectorizer: Config with
                    {
                        SourceProperties = sourceProperties,
                    },
                    vectorIndexConfig: quantizerConfig is null
                        ? indexConfig
                        : indexConfig with
                        {
                            Quantizer = quantizerConfig,
                        }
                );
            }
        }

        public static VectorConfigBuilder Text2MultiVecJinaAI(
            string? model = null,
            string? baseURL = null,
            int? dimensions = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Text2MultiVecJinaAI
                {
                    Model = model,
                    BaseURL = baseURL,
                    Dimensions = dimensions,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2MultiVecJinaAI(
            string? baseURL = null,
            string? model = null,
            string[]? imageFields = null,
            string[]? textFields = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2MultiVecJinaAI
                {
                    BaseURL = baseURL,
                    Model = model,
                    ImageFields = imageFields,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                }
            );

        public static VectorConfigBuilder Multi2MultiVecJinaAI(
            string? baseURL = null,
            string? model = null,
            WeightedFields? imageFields = null,
            WeightedFields? textFields = null,
            bool? vectorizeCollectionName = null
        ) =>
            new(
                new Vectorizer.Multi2MultiVecJinaAI
                {
                    BaseURL = baseURL,
                    Model = model,
                    ImageFields = imageFields,
                    TextFields = textFields,
                    VectorizeCollectionName = vectorizeCollectionName,
                    Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
                }
            );
    }
}

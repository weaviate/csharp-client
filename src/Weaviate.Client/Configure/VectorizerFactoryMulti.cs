using Weaviate.Client.Models;
using static Weaviate.Client.Models.VectorIndexConfig;
using static Weaviate.Client.Models.Vectorizer;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

public class VectorizerFactoryMulti
{
    internal VectorizerFactoryMulti() { }

#pragma warning disable CA1822 // Mark members as static
    public VectorizerConfig SelfProvided() => new Models.Vectorizer.SelfProvided();

    public VectorizerConfig Text2MultiVecJinaAI(
        string? model = null,
        string? baseURL = null,
        int? dimensions = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Text2MultiVecJinaAI
        {
            Model = model,
            BaseURL = baseURL,
            Dimensions = dimensions,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2MultiVecJinaAI(
        string[]? imageFields = null,
        string[]? textFields = null,
        string? baseURL = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2MultiVecJinaAI
        {
            BaseURL = baseURL,
            Model = model,
            ImageFields = imageFields,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
        };

    public VectorizerConfig Multi2MultiVecJinaAI(
        WeightedFields imageFields,
        WeightedFields textFields,
        string? baseURL = null,
        string? model = null,
        bool? vectorizeCollectionName = null
    ) =>
        new Models.Vectorizer.Multi2MultiVecJinaAI
        {
            BaseURL = baseURL,
            Model = model,
            ImageFields = imageFields,
            TextFields = textFields,
            VectorizeCollectionName = vectorizeCollectionName,
            Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields),
        };
#pragma warning restore CA1822 // Mark members as static
}

using Weaviate.Client.Models;
using static Weaviate.Client.Models.VectorIndexConfig;
using static Weaviate.Client.Models.Vectorizer;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// The vectorizer factory multi class
/// </summary>
public class VectorizerFactoryMulti
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VectorizerFactoryMulti"/> class
    /// </summary>
    internal VectorizerFactoryMulti() { }

#pragma warning disable CA1822 // Mark members as static
    /// <summary>
    /// Selfs the provided
    /// </summary>
    /// <returns>The vectorizer config</returns>
    public VectorizerConfig SelfProvided() => new Models.Vectorizer.SelfProvided();

    /// <summary>
    /// Texts the 2 multi vec jina ai using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="dimensions">The dimensions</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
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

    /// <summary>
    /// Multis the 2 multi vec jina ai using the specified image fields
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
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

    /// <summary>
    /// Multis the 2 multi vec jina ai using the specified image fields
    /// </summary>
    /// <param name="imageFields">The image fields</param>
    /// <param name="textFields">The text fields</param>
    /// <param name="baseURL">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="vectorizeCollectionName">The vectorize collection name</param>
    /// <returns>The vectorizer config</returns>
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

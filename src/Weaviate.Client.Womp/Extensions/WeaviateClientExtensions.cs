using Weaviate.Client.Womp.Schema;

namespace Weaviate.Client.Womp.Extensions;

/// <summary>
/// Extension methods for WeaviateClient to support ORM operations.
/// </summary>
public static class WeaviateClientExtensions
{
    /// <summary>
    /// Creates a Weaviate collection from a C# class decorated with ORM attributes.
    /// The class must have a [WeaviateCollection] attribute or the class name will be used as the collection name.
    /// </summary>
    /// <typeparam name="T">The class type representing the collection schema.</typeparam>
    /// <param name="collections">The collections interface.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A CollectionClient for the newly created collection.</returns>
    /// <example>
    /// <code>
    /// [WeaviateCollection("Articles")]
    /// public class Article
    /// {
    ///     [Property(DataType.Text)]
    ///     public string Title { get; set; }
    ///
    ///     [Vector&lt;Vectorizer.Text2VecOpenAI&gt;(Model = "ada-002")]
    ///     public float[]? Embedding { get; set; }
    /// }
    ///
    /// var collection = await client.Collections.CreateFromClass&lt;Article&gt;();
    /// </code>
    /// </example>
    public static async Task<CollectionClient> CreateFromClass<T>(
        this CollectionsClient collections,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var config = CollectionSchemaBuilder.FromClass<T>();
        return await collections.Create(config, cancellationToken);
    }
}

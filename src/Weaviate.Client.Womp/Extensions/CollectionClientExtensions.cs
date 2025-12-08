using Weaviate.Client.Womp.Query;

namespace Weaviate.Client.Womp.Extensions;

/// <summary>
/// Extension methods for CollectionClient to support ORM operations.
/// </summary>
public static class CollectionClientExtensions
{
    /// <summary>
    /// Gets a type-safe ORM query client for LINQ-style queries.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="collection">The collection client.</param>
    /// <returns>An ORM query client for building type-safe queries.</returns>
    /// <example>
    /// <code>
    /// var results = await collection.Query&lt;Article&gt;()
    ///     .Where(a => a.WordCount > 100)
    ///     .NearText("technology")
    ///     .WithReferences(a => a.Category)
    ///     .Limit(10)
    ///     .ExecuteAsync();
    /// </code>
    /// </example>
    public static WompQueryClient<T> Query<T>(this CollectionClient collection)
        where T : class, new()
    {
        return new WompQueryClient<T>(collection);
    }
}

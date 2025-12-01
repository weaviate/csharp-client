using Weaviate.Client.Typed;
using Weaviate.Client.Validation;

namespace Weaviate.Client;

/// <summary>
/// Extension methods for CollectionClient to support typed client operations.
/// </summary>
public static class CollectionClientExtensions
{
    /// <summary>
    /// Converts an untyped CollectionClient to a strongly-typed TypedCollectionClient.
    /// This allows you to work with compile-time type safety for data and query operations.
    /// </summary>
    /// <typeparam name="T">The C# type representing objects in this collection.</typeparam>
    /// <param name="collectionClient">The untyped CollectionClient to wrap.</param>
    /// <param name="validateType">
    /// If true, validates that type T is compatible with the collection schema on construction.
    /// Default is false for performance. Set to true during development to catch schema mismatches early.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the validation operation.</param>
    /// <returns>A TypedCollectionClient that provides strongly-typed operations.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if validateType is true and validation fails with errors.
    /// </exception>
    /// <example>
    /// <code>
    /// // Without validation (faster, default)
    /// var typedArticles = await articlesCollection.AsTyped&lt;Article&gt;();
    ///
    /// // With validation (safer, recommended during development)
    /// var typedArticles = await articlesCollection.AsTyped&lt;Article&gt;(validateType: true);
    ///
    /// // Use the typed client
    /// var article = new Article { Title = "Hello", Content = "World" };
    /// await typedArticles.Data.Insert(article);
    /// </code>
    /// </example>
    public static async Task<TypedCollectionClient<T>> AsTyped<T>(
        this CollectionClient collectionClient,
        bool validateType = false,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(collectionClient);

        if (validateType)
        {
            var schema = await collectionClient.Config.GetCachedConfig(
                cancellationToken: cancellationToken
            );

            schema.ValidateTypeOrThrow<T>();
        }

        return AsTyped<T>(collectionClient);
    }

    /// <summary>
    /// Converts an untyped CollectionClient to a strongly-typed TypedCollectionClient.
    /// This allows you to work with compile-time type safety for data and query operations.
    /// </summary>
    /// <typeparam name="T">The C# type representing objects in this collection.</typeparam>
    /// <param name="collectionClient">The untyped CollectionClient to wrap.</param>
    /// <param name="validateType">
    /// If true, validates that type T is compatible with the collection schema on construction.
    /// Default is false for performance. Set to true during development to catch schema mismatches early.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the validation operation.</param>
    /// <returns>A TypedCollectionClient that provides strongly-typed operations.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if validateType is true and validation fails with errors.
    /// </exception>
    /// <example>
    /// <code>
    /// // Without validation (faster, default)
    /// var typedArticles = await articlesCollection.AsTyped&lt;Article&gt;();
    ///
    /// // With validation (safer, recommended during development)
    /// var typedArticles = await articlesCollection.AsTyped&lt;Article&gt;(validateType: true);
    ///
    /// // Use the typed client
    /// var article = new Article { Title = "Hello", Content = "World" };
    /// await typedArticles.Data.Insert(article);
    /// </code>
    /// </example>
    public static TypedCollectionClient<T> AsTyped<T>(this CollectionClient collectionClient)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(collectionClient);

        return new TypedCollectionClient<T>(collectionClient);
    }
}

using System.Runtime.CompilerServices;
using Weaviate.Client.Cache;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;
using Weaviate.Client.Validation;

namespace Weaviate.Client.Typed;

/// <summary>
/// Strongly-typed collection client that provides type-safe operations.
/// This is the main entry point for working with a specific collection using strongly-typed objects.
/// </summary>
/// <typeparam name="T">The C# type representing objects in this collection.</typeparam>
public class TypedCollectionClient<T>
    where T : class, new()
{
    private readonly CollectionClient _collectionClient;
    private readonly TypedDataClient<T> _dataClient;
    private readonly TypedQueryClient<T> _queryClient;
    private readonly TypedGenerateClient<T> _generateClient;

    /// <summary>
    /// Creates a new typed collection client wrapping an untyped CollectionClient.
    /// </summary>
    /// <param name="collectionClient">The underlying CollectionClient to wrap.</param>
    public TypedCollectionClient(CollectionClient collectionClient)
    {
        ArgumentNullException.ThrowIfNull(collectionClient);

        _collectionClient = collectionClient;
        _dataClient = new TypedDataClient<T>(collectionClient.Data);
        _queryClient = new TypedQueryClient<T>(collectionClient.Query);
        _generateClient = new TypedGenerateClient<T>(collectionClient.Generate);
    }

    /// <summary>
    /// The collection name.
    /// </summary>
    public string Name => _collectionClient.Name;

    /// <summary>
    /// The tenant name for multi-tenancy support.
    /// </summary>
    public string? Tenant => _collectionClient.Tenant;

    /// <summary>
    /// The consistency level for operations.
    /// </summary>
    public ConsistencyLevels? ConsistencyLevel => _collectionClient.ConsistencyLevel;

    /// <summary>
    /// The Weaviate server version.
    /// </summary>
    public System.Version? WeaviateVersion => _collectionClient.WeaviateVersion;

    /// <summary>
    /// Strongly-typed data operations (CRUD).
    /// </summary>
    public TypedDataClient<T> Data => _dataClient;

    /// <summary>
    /// Strongly-typed query operations (search, filter, etc.).
    /// </summary>
    public TypedQueryClient<T> Query => _queryClient;

    /// <summary>
    /// Strongly-typed generative AI query operations (RAG - Retrieval-Augmented Generation).
    /// Combines search with LLM-generated content for each result or the entire result set.
    /// </summary>
    public TypedGenerateClient<T> Generate => _generateClient;

    /// <summary>
    /// Aggregate operations. Returns untyped AggregateClient since aggregates return metrics, not objects.
    /// </summary>
    public AggregateClient Aggregate => _collectionClient.Aggregate;

    /// <summary>
    /// Collection configuration operations.
    /// </summary>
    public CollectionConfigClient Config => _collectionClient.Config;

    /// <summary>
    /// Accesses the underlying untyped CollectionClient.
    /// Useful when you need to perform operations not yet available in the typed wrapper.
    /// </summary>
    public CollectionClient Untyped => _collectionClient;

    /// <summary>
    /// Deletes this collection from Weaviate.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public async Task Delete(CancellationToken cancellationToken = default)
    {
        await _collectionClient.Delete(cancellationToken);
    }

    /// <summary>
    /// Returns the total count of objects in this collection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The total number of objects in the collection.</returns>
    public async Task<ulong> Count(CancellationToken cancellationToken = default)
    {
        return await _collectionClient.Count(cancellationToken);
    }

    /// <summary>
    /// Creates a new typed collection client pinned to a specific tenant.
    /// </summary>
    /// <param name="tenant">The tenant name.</param>
    /// <returns>A new TypedCollectionClient pinned to the specified tenant.</returns>
    public TypedCollectionClient<T> WithTenant(string tenant)
    {
        var newCollectionClient = _collectionClient.WithTenant(tenant);
        return new TypedCollectionClient<T>(newCollectionClient);
    }

    /// <summary>
    /// Creates a new typed collection client with a specific consistency level.
    /// </summary>
    /// <param name="consistencyLevel">The consistency level to use.</param>
    /// <returns>A new TypedCollectionClient with the specified consistency level.</returns>
    public TypedCollectionClient<T> WithConsistencyLevel(ConsistencyLevels consistencyLevel)
    {
        var newCollectionClient = _collectionClient.WithConsistencyLevel(consistencyLevel);
        return new TypedCollectionClient<T>(newCollectionClient);
    }

    /// <summary>
    /// Iterates over all objects in the collection, returning strongly-typed objects.
    /// Uses cursor-based pagination for efficient iteration over large collections.
    /// </summary>
    /// <param name="after">Start iteration after this object ID.</param>
    /// <param name="cacheSize">Number of objects to fetch per page.</param>
    /// <param name="returnMetadata">Metadata to include in results.</param>
    /// <param name="includeVectors">Whether to include vectors.</param>
    /// <param name="returnProperties">Properties to return.</param>
    /// <param name="returnReferences">References to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of strongly-typed objects.</returns>
    public async IAsyncEnumerable<WeaviateObject<T>> Iterator(
        Guid? after = null,
        uint cacheSize = CollectionClient.ITERATOR_CACHE_SIZE,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (
            var obj in _collectionClient.Iterator(
                after,
                cacheSize,
                returnMetadata,
                includeVectors,
                returnProperties,
                returnReferences,
                cancellationToken
            )
        )
        {
            yield return obj.ToTyped<T>();
        }
    }

    /// <summary>
    /// Validates that the C# type T is compatible with this collection's schema.
    /// Checks property names, types, and array handling.
    /// </summary>
    /// <param name="typeValidator">Optional type validator instance. If null, uses TypeValidator.Default.</param>
    /// <param name="schemaCache">Optional schema cache instance. If null, uses SchemaCache.Default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A validation result containing any errors and warnings.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the collection schema cannot be fetched.</exception>
    public async Task<ValidationResult> ValidateType(
        TypeValidator? typeValidator = null,
        SchemaCache? schemaCache = null,
        CancellationToken cancellationToken = default
    )
    {
        var schema = await Config.GetCachedConfig(schemaCache, cancellationToken);

        return schema.ValidateType<T>(typeValidator);
    }
}

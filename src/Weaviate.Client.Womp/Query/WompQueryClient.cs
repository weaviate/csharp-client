using System.Linq.Expressions;
using Weaviate.Client.Models;
using Weaviate.Client.Typed;
using Weaviate.Client.Womp.Internal;
using Weaviate.Client.Womp.Mapping;

namespace Weaviate.Client.Womp.Query;

/// <summary>
/// Fluent query builder for type-safe LINQ-style queries.
/// Provides a declarative API for building Weaviate queries with compile-time type safety.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
/// <example>
/// <code>
/// var results = await collection.Query&lt;Article&gt;()
///     .Where(a => a.WordCount > 100)
///     .NearText("technology", vector: a => a.Embedding)
///     .WithReferences(a => a.Category)
///     .Limit(10)
///     .ExecuteAsync();
/// </code>
/// </example>
public class WompQueryClient<T>
    where T : class, new()
{
    private readonly CollectionClient _collection;
    private readonly TypedQueryClient<T> _typedClient;

    // Query state
    private Filter? _filter;
    private uint? _limit;
    private readonly List<string> _includeVectors = new();
    private readonly List<string> _includeReferences = new();
    private OneOrManyOf<Sort>? _sort;
    private Rerank? _rerank = null;
    private OneOrManyOf<string>? _returnProperties;
    private MetadataQuery? _returnMetadata;

    // Search state
    private SearchMode _searchMode = SearchMode.Fetch;
    private object? _searchTarget;
    private TargetVectors? _targetVectors;
    private float? _distance;
    private float? _certainty;
    private float? _alpha; // For hybrid search

    internal WompQueryClient(CollectionClient collection)
    {
        _collection = collection;
        _typedClient = new TypedQueryClient<T>(collection.Query);
    }

    #region Filter Methods

    /// <summary>
    /// Filters results using a type-safe lambda expression.
    /// Multiple Where calls are combined with AND logic.
    /// </summary>
    /// <param name="predicate">The filter predicate (e.g., a => a.Age > 18).</param>
    /// <returns>The query builder for chaining.</returns>
    /// <example>
    /// <code>
    /// .Where(a => a.WordCount > 100)
    /// .Where(a => a.PublishedAt > DateTime.Now.AddDays(-7))
    /// </code>
    /// </example>
    public WompQueryClient<T> Where(Expression<Func<T, bool>> predicate)
    {
        var filter = ExpressionToFilterConverter.Convert(predicate);
        _filter = _filter == null ? filter : Filter.And(_filter, filter);
        return this;
    }

    #endregion

    #region Vector Search Methods

    /// <summary>
    /// Performs a near text search using text-to-vector conversion.
    /// </summary>
    /// <param name="text">The search text.</param>
    /// <param name="vector">Optional: specify which named vector to use.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <returns>The query builder for chaining.</returns>
    public WompQueryClient<T> NearText(
        string text,
        Expression<Func<T, object>>? vector = null,
        float? certainty = null,
        float? distance = null
    )
    {
        _searchMode = SearchMode.NearText;
        _searchTarget = text;
        if (vector != null)
        {
            var vectorName = GetVectorName(vector);
            _targetVectors = new TargetVectors();
            _targetVectors.Add(vectorName);
        }
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    /// <summary>
    /// Performs a near vector search using a provided vector.
    /// </summary>
    /// <param name="vectorValues">The vector to search with.</param>
    /// <param name="vector">Optional: specify which named vector to use.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <returns>The query builder for chaining.</returns>
    public WompQueryClient<T> NearVector(
        float[] vectorValues,
        Expression<Func<T, object>>? vector = null,
        float? certainty = null,
        float? distance = null
    )
    {
        _searchMode = SearchMode.NearVector;
        _searchTarget = vectorValues;
        if (vector != null)
        {
            var vectorName = GetVectorName(vector);
            _targetVectors = new TargetVectors();
            _targetVectors.Add(vectorName);
        }
        _certainty = certainty;
        _distance = distance;
        return this;
    }

    /// <summary>
    /// Performs a hybrid search combining BM25 keyword search with vector search.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="vector">Optional: specify which named vector to use.</param>
    /// <param name="alpha">Balance between keyword (0) and vector (1) search. Default 0.5.</param>
    /// <returns>The query builder for chaining.</returns>
    public WompQueryClient<T> Hybrid(
        string query,
        Expression<Func<T, object>>? vector = null,
        float? alpha = null
    )
    {
        _searchMode = SearchMode.Hybrid;
        _searchTarget = query;
        if (vector != null)
        {
            var vectorName = GetVectorName(vector);
            _targetVectors = new TargetVectors();
            _targetVectors.Add(vectorName);
        }
        _alpha = alpha;
        return this;
    }

    #endregion

    #region Result Control Methods

    /// <summary>
    /// Limits the number of results returned.
    /// </summary>
    /// <param name="limit">Maximum number of results.</param>
    /// <returns>The query builder for chaining.</returns>
    public WompQueryClient<T> Limit(uint limit)
    {
        _limit = limit;
        return this;
    }

    /// <summary>
    /// Sorts results by a property.
    /// </summary>
    /// <typeparam name="TProp">The property type.</typeparam>
    /// <param name="property">The property selector.</param>
    /// <param name="descending">Sort in descending order.</param>
    /// <returns>The query builder for chaining.</returns>
    public WompQueryClient<T> Sort<TProp>(
        Expression<Func<T, TProp>> property,
        bool descending = false
    )
    {
        var propName = PropertyHelper.GetPropertyName(property);
        var camelName = PropertyHelper.ToCamelCase(propName);
        var sort = Models.Sort.ByProperty(camelName);
        _sort = descending ? sort.Descending() : sort.Ascending();
        return this;
    }

    #endregion

    #region Include Methods

    /// <summary>
    /// Includes named vectors in the results.
    /// Vectors will be populated in the corresponding properties.
    /// </summary>
    /// <param name="vectors">The vector properties to include.</param>
    /// <returns>The query builder for chaining.</returns>
    /// <example>
    /// <code>
    /// .WithVectors(a => a.TitleEmbedding, a => a.ContentEmbedding)
    /// </code>
    /// </example>
    public WompQueryClient<T> WithVectors(params Expression<Func<T, object>>[] vectors)
    {
        foreach (var vector in vectors)
        {
            var vectorName = GetVectorName(vector);
            if (!_includeVectors.Contains(vectorName))
            {
                _includeVectors.Add(vectorName);
            }
        }
        return this;
    }

    /// <summary>
    /// Includes cross-references in the results.
    /// References will be expanded and populated.
    /// </summary>
    /// <param name="references">The reference properties to include.</param>
    /// <returns>The query builder for chaining.</returns>
    /// <example>
    /// <code>
    /// .WithReferences(a => a.Category, a => a.Author)
    /// </code>
    /// </example>
    public WompQueryClient<T> WithReferences(params Expression<Func<T, object>>[] references)
    {
        foreach (var reference in references)
        {
            var refName = PropertyHelper.GetPropertyName(reference);
            var camelName = PropertyHelper.ToCamelCase(refName);
            if (!_includeReferences.Contains(camelName))
            {
                _includeReferences.Add(camelName);
            }
        }
        return this;
    }

    /// <summary>
    /// Specifies which properties to return.
    /// If not called, all properties are returned.
    /// </summary>
    /// <param name="selector">Property selector.</param>
    /// <returns>The query builder for chaining.</returns>
    public WompQueryClient<T> Select(Expression<Func<T, object>> selector)
    {
        var properties = PropertyHelper.GetCamelCasePropertyNames(selector);
        _returnProperties = properties.ToArray();
        return this;
    }

    /// <summary>
    /// Includes metadata in results (distance, certainty, creation time, etc.).
    /// </summary>
    /// <param name="metadata">Metadata options to include.</param>
    /// <returns>The query builder for chaining.</returns>
    public WompQueryClient<T> WithMetadata(MetadataQuery metadata)
    {
        _returnMetadata = metadata;
        return this;
    }

    #endregion

    #region Execution Methods

    /// <summary>
    /// Executes the query and returns typed results.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Enumerable of typed objects.</returns>
    public async Task<IEnumerable<T>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteQueryAsync(cancellationToken);

        // Use ORM object mapper to populate vectors and references
        return result.Objects.Select(WompObjectMapper.FromWeaviateObject);
    }

    /// <summary>
    /// Executes the query and returns the full typed result with metadata.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Full result set with metadata.</returns>
    public async Task<Models.WeaviateResult<Models.Typed.WeaviateObject<T>>> ExecuteWithMetadataAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteQueryAsync(cancellationToken);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Executes the query based on the search mode.
    /// </summary>
    private async Task<Models.WeaviateResult<Models.Typed.WeaviateObject<T>>> ExecuteQueryAsync(
        CancellationToken cancellationToken
    )
    {
        // Build references list
        IList<QueryReference>? queryReferences =
            _includeReferences.Count > 0
                ? _includeReferences.Select(name => new QueryReference(name)).ToList()
                : null;

        // Build vector include query
        VectorQuery? vectorInclude =
            _includeVectors.Count > 0 ? new VectorQuery(_includeVectors) : null;

        return _searchMode switch
        {
            SearchMode.Fetch => await _typedClient.FetchObjects(
                limit: _limit,
                filters: _filter,
                sort: _sort,
                rerank: _rerank,
                returnProperties: _returnProperties,
                returnReferences: queryReferences,
                returnMetadata: _returnMetadata,
                includeVectors: vectorInclude,
                cancellationToken: cancellationToken
            ),

            SearchMode.NearText => await _typedClient.NearText(
                text: (OneOrManyOf<string>)_searchTarget!,
                limit: _limit,
                certainty: _certainty,
                distance: _distance,
                filters: _filter,
                targetVector: _targetVectors,
                returnProperties: _returnProperties,
                returnReferences: queryReferences,
                returnMetadata: _returnMetadata,
                includeVectors: vectorInclude,
                cancellationToken: cancellationToken
            ),

            SearchMode.NearVector => await _typedClient.NearVector(
                vector: (float[])_searchTarget!,
                limit: _limit,
                certainty: _certainty,
                distance: _distance,
                filters: _filter,
                targetVector: _targetVectors,
                returnProperties: _returnProperties,
                returnReferences: queryReferences,
                returnMetadata: _returnMetadata,
                includeVectors: vectorInclude,
                cancellationToken: cancellationToken
            ),

            SearchMode.Hybrid => await _typedClient.Hybrid(
                query: (string)_searchTarget!,
                limit: _limit,
                alpha: _alpha,
                filters: _filter,
                targetVector: _targetVectors,
                returnProperties: _returnProperties,
                returnReferences: queryReferences,
                returnMetadata: _returnMetadata,
                includeVectors: vectorInclude,
                cancellationToken: cancellationToken
            ),

            _ => throw new NotSupportedException($"Search mode {_searchMode} is not supported"),
        };
    }

    /// <summary>
    /// Extracts the vector name from a property selector expression.
    /// </summary>
    private static string GetVectorName(Expression<Func<T, object>> vectorExpr)
    {
        var propName = PropertyHelper.GetPropertyName(vectorExpr);
        return PropertyHelper.ToCamelCase(propName);
    }

    #endregion

    /// <summary>
    /// Search mode enumeration.
    /// </summary>
    private enum SearchMode
    {
        Fetch,
        NearText,
        NearVector,
        Hybrid,
    }
}

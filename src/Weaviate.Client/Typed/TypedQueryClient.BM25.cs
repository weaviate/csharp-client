using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

public partial class TypedQueryClient<T>
{
    /// <summary>
    /// Performs a BM25 keyword search with group-by aggregation.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="searchFields">Fields to search in.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="searchOperator">BM25 search operator (AND/OR).</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="after">Cursor for pagination.</param>
    /// <param name="consistencyLevel">Consistency level for the query.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? searchOperator = null,
        Rerank? rerank = null,
        Guid? after = null,
        ConsistencyLevels? consistencyLevel = null,
        AutoArray<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.BM25(
            query: query,
            groupBy: groupBy,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            searchOperator: searchOperator,
            rerank: rerank,
            after: after,
            consistencyLevel: consistencyLevel,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a BM25 keyword search.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="searchFields">Fields to search in.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="searchOperator">BM25 search operator (AND/OR).</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="after">Cursor for pagination.</param>
    /// <param name="consistencyLevel">Consistency level for the query.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> BM25(
        string query,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? searchOperator = null,
        Rerank? rerank = null,
        Guid? after = null,
        ConsistencyLevels? consistencyLevel = null,
        AutoArray<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.BM25(
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            searchOperator: searchOperator,
            rerank: rerank,
            after: after,
            consistencyLevel: consistencyLevel,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }
}

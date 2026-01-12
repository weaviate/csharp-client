using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The query client class
/// </summary>
public partial class QueryClient
{
    /// <summary>
    /// Bms the 25 using the specified query
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="searchFields">The search fields</param>
    /// <param name="filters">The filters</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="searchOperator">The search operator</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="after">The after</param>
    /// <param name="consistencyLevel">The consistency level</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the group by result</returns>
    public async Task<GroupByResult> BM25(
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
    ) =>
        await _grpc.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            searchOperator: searchOperator,
            groupBy: groupBy,
            rerank: rerank,
            after: after,
            tenant: _collectionClient.Tenant,
            consistencyLevel: consistencyLevel ?? _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            returnProperties: returnProperties,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>
    /// Bms the 25 using the specified query
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="searchFields">The search fields</param>
    /// <param name="filters">The filters</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="searchOperator">The search operator</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="after">The after</param>
    /// <param name="consistencyLevel">The consistency level</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the weaviate result</returns>
    public async Task<WeaviateResult> BM25(
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
    ) =>
        await _grpc.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            searchOperator: searchOperator,
            groupBy: null,
            rerank: rerank,
            after: after,
            tenant: _collectionClient.Tenant,
            consistencyLevel: consistencyLevel ?? _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            returnProperties: returnProperties,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
}

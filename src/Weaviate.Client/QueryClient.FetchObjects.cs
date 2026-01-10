using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The query client class
/// </summary>
public partial class QueryClient
{
    #region Objects
    /// <summary>
    /// Fetches the objects using the specified group by
    /// </summary>
    /// <param name="groupBy">The group by</param>
    /// <param name="after">The after</param>
    /// <param name="limit">The limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="sort">The sort</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the group by result</returns>
    public async Task<GroupByResult> FetchObjects(
        Models.GroupByRequest groupBy,
        Guid? after = null,
        uint? limit = null,
        Filter? filters = null,
        AutoArray<Sort>? sort = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        return await _grpc.FetchObjects(
            _collectionName,
            after: after,
            limit: limit,
            rerank: rerank,
            filters: filters,
            sort: sort,
            groupBy: groupBy,
            tenant: _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// Fetches the objects using the specified after
    /// </summary>
    /// <param name="after">The after</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="filters">The filters</param>
    /// <param name="sort">The sort</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the weaviate result</returns>
    public async Task<WeaviateResult> FetchObjects(
        Guid? after = null,
        uint? limit = null,
        uint? offset = null,
        Filter? filters = null,
        AutoArray<Sort>? sort = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.FetchObjects(
            _collectionName,
            after: after,
            limit: limit,
            offset: offset,
            rerank: rerank,
            filters: filters,
            sort: sort,
            tenant: _collectionClient.Tenant,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata?.Disable(MetadataOptions.Certainty),
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>
    /// Fetches the object by id using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The first object</returns>
    public async Task<WeaviateObject?> FetchObjectByID(
        Guid id,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var searchReply = await _grpc.FetchObjects(
            _collectionName,
            returnProperties: returnProperties,
            filters: Filter.UUID.IsEqual(id),
            tenant: _collectionClient.Tenant,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata?.Disable(MetadataOptions.Certainty),
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

        WeaviateResult? result = searchReply;

        WeaviateObject? firstObject = result.SingleOrDefault();

        return firstObject;
    }

    /// <summary>
    /// Fetches the objects by i ds using the specified ids
    /// </summary>
    /// <param name="ids">The ids</param>
    /// <param name="limit">The limit</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="filters">The filters</param>
    /// <param name="sort">The sort</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the weaviate result</returns>
    public async Task<WeaviateResult> FetchObjectsByIDs(
        HashSet<Guid> ids,
        uint? limit = null,
        Rerank? rerank = null,
        Filter? filters = null,
        AutoArray<Sort>? sort = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.FetchObjects(
            _collectionName,
            limit: limit,
            filters: filters != null
                ? Filter.UUID.IsEqual(ids.First()) & filters
                : Filter.AnyOf([.. ids.Select(id => Filter.UUID.IsEqual(id))]),
            sort: sort,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata?.Disable(MetadataOptions.Certainty),
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    #endregion
}

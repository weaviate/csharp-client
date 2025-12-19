using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class QueryClient
{
    #region Objects
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
            filters: Filter.ID.IsEqual(id),
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
                ? Filter.ID.IsEqual(ids.First()) & filters
                : Filter.AnyOf([.. ids.Select(id => Filter.ID.IsEqual(id))]),
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

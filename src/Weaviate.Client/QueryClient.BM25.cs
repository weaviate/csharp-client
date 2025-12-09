using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class QueryClient
{
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
        OneOrManyOf<string>? returnProperties = null,
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
        OneOrManyOf<string>? returnProperties = null,
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

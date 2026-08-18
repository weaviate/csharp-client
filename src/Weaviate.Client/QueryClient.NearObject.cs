using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The query client class
/// </summary>
public partial class QueryClient
{
    /// <summary>
    /// Nears the object using the specified near object
    /// </summary>
    /// <param name="nearObject">The near object</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="targets">The targets</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the weaviate result</returns>
    public async Task<WeaviateResult> NearObject(
        Guid nearObject,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        Boost? boost = null,
        TargetVectors.FactoryFn? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearObject(
            _collectionClient.Name,
            objectID: nearObject,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: null,
            rerank: rerank,
            boost: boost,
            singlePrompt: null,
            groupedTask: null,
            targetVector: targets?.Invoke(new TargetVectors.Builder()),
            tenant: _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>
    /// Nears the object using the specified near object
    /// </summary>
    /// <param name="nearObject">The near object</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="targets">The targets</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the group by result</returns>
    public async Task<GroupByResult> NearObject(
        Guid nearObject,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        Boost? boost = null,
        TargetVectors.FactoryFn? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearObject(
            _collectionClient.Name,
            objectID: nearObject,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: groupBy,
            rerank: rerank,
            boost: boost,
            singlePrompt: null,
            groupedTask: null,
            targetVector: targets?.Invoke(new TargetVectors.Builder()),
            tenant: _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
}

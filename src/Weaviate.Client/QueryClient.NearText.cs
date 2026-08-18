using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The query client class
/// </summary>
public partial class QueryClient
{
    /// <summary>Performs a near-text search using the specified parameters.</summary>
    /// <param name="query">The search text.</param>
    /// <param name="certainty">Certainty threshold for the search: the minimum similarity a result must reach. If not specified, no threshold is applied.</param>
    /// <param name="distance">Distance threshold for the search: the maximum distance a result may have from the query vector. If not specified, no threshold is applied.</param>
    /// <param name="moveTo">Move-to configuration.</param>
    /// <param name="moveAway">Move-away configuration.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearText(
        AutoArray<string> query,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        Boost? boost = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearText(
            _collectionClient.Name,
            [.. query],
            distance: distance,
            certainty: certainty,
            limit: limit,
            moveTo: moveTo,
            moveAway: moveAway,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            targetVector: null,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            boost: boost,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>Performs a near-text search with group-by using the specified parameters.</summary>
    /// <param name="query">The search text.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="certainty">Certainty threshold for the search: the minimum similarity a result must reach. If not specified, no threshold is applied.</param>
    /// <param name="distance">Distance threshold for the search: the maximum distance a result may have from the query vector. If not specified, no threshold is applied.</param>
    /// <param name="moveTo">Move-to configuration.</param>
    /// <param name="moveAway">Move-away configuration.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>Grouped search results.</returns>
    public async Task<GroupByResult> NearText(
        AutoArray<string> query,
        GroupByRequest groupBy,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        Boost? boost = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearText(
            _collectionClient.Name,
            [.. query],
            groupBy: groupBy,
            distance: distance,
            certainty: certainty,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            boost: boost,
            targetVector: null,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    // Lambda builder overload
    /// <summary>
    /// Performs a near-text search using a lambda builder for NearTextInput.
    /// Allows specifying target vectors with combination methods (Sum, Average, ManualWeights, etc.)
    /// using a fluent syntax.
    /// </summary>
    /// <example>
    /// await collection.Query.NearText(
    ///     q => q(["search query"], certainty: 0.7f)
    ///         .Sum("title", "description")
    /// )
    /// </example>
    /// <param name="query">Lambda builder for creating NearTextInput with target vectors.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearText(
        NearTextInput.FactoryFn query,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        Boost? boost = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var input = query(VectorInputBuilderFactories.CreateNearTextBuilder());
        return await _grpc.SearchNearText(
            _collectionClient.Name,
            [.. input.Query],
            distance: input.Distance,
            certainty: input.Certainty,
            limit: limit,
            moveTo: input.MoveTo,
            moveAway: input.MoveAway,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            targetVector: input.TargetVectors,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            boost: boost,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    // Lambda builder overload with GroupBy
    /// <summary>
    /// Performs a near-text search with group-by using a lambda builder for NearTextInput.
    /// </summary>
    /// <param name="query">Lambda builder for creating NearTextInput with target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>Grouped search results.</returns>
    public async Task<GroupByResult> NearText(
        NearTextInput.FactoryFn query,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        Boost? boost = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var input = query(VectorInputBuilderFactories.CreateNearTextBuilder());
        return await _grpc.SearchNearText(
            _collectionClient.Name,
            [.. input.Query],
            groupBy: groupBy,
            distance: input.Distance,
            certainty: input.Certainty,
            moveTo: input.MoveTo,
            moveAway: input.MoveAway,
            limit: limit,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            boost: boost,
            targetVector: input.TargetVectors,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    }
}

/// <summary>
/// Extension methods for QueryClient NearText search with NearTextInput.
/// </summary>
public static class QueryClientNearTextExtensions
{
    /// <summary>
    /// Performs a near-text search using a NearTextInput record.
    /// </summary>
    /// <param name="client">The client to run the search on.</param>
    /// <param name="query">The near-text input.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public static async Task<WeaviateResult> NearText(
        this QueryClient client,
        NearTextInput query,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        Boost? boost = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        // If input has target vectors, use the lambda builder overload
        if (query.TargetVectors != null)
        {
            return await client.NearText(
                _ => query,
                filters: filters,
                limit: limit,
                offset: offset,
                diversitySelection: diversitySelection,
                autoLimit: autoLimit,
                rerank: rerank,
                boost: boost,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata,
                includeVectors: includeVectors,
                cancellationToken: cancellationToken
            );
        }

        // Otherwise use the base method
        return await client.NearText(
            query: query.Query,
            certainty: query.Certainty,
            distance: query.Distance,
            moveTo: query.MoveTo,
            moveAway: query.MoveAway,
            limit: limit,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            boost: boost,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Performs a near-text search with group-by using a NearTextInput record.
    /// </summary>
    /// <param name="client">The client to run the search on.</param>
    /// <param name="query">The near-text input.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public static async Task<GroupByResult> NearText(
        this QueryClient client,
        NearTextInput query,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        Boost? boost = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        // If input has target vectors, use the lambda builder overload
        if (query.TargetVectors != null)
        {
            return await client.NearText(
                _ => query,
                groupBy,
                filters: filters,
                limit: limit,
                offset: offset,
                diversitySelection: diversitySelection,
                autoLimit: autoLimit,
                rerank: rerank,
                boost: boost,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata,
                includeVectors: includeVectors,
                cancellationToken: cancellationToken
            );
        }

        // Otherwise use the base method
        return await client.NearText(
            query: query.Query,
            groupBy: groupBy,
            certainty: query.Certainty,
            distance: query.Distance,
            moveTo: query.MoveTo,
            moveAway: query.MoveAway,
            limit: limit,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            boost: boost,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
    }
}

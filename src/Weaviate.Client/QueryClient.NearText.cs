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
    /// <param name="certainty">Certainty threshold for the search.</param>
    /// <param name="distance">Distance threshold for the search.</param>
    /// <param name="moveTo">Move-to configuration.</param>
    /// <param name="moveAway">Move-away configuration.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearText(
        AutoArray<string> query,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
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
            autoLimit: autoLimit,
            targetVector: null,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
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
    /// <param name="certainty">Certainty threshold for the search.</param>
    /// <param name="distance">Distance threshold for the search.</param>
    /// <param name="moveTo">Move-to configuration.</param>
    /// <param name="moveAway">Move-away configuration.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
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
            autoLimit: autoLimit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
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
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearText(
        NearTextInput.FactoryFn query,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
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
            autoLimit: autoLimit,
            targetVector: input.TargetVectors,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
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
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Grouped search results.</returns>
    public async Task<GroupByResult> NearText(
        NearTextInput.FactoryFn query,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
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
            autoLimit: autoLimit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
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
    public static async Task<WeaviateResult> NearText(
        this QueryClient client,
        NearTextInput query,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
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
                autoLimit: autoLimit,
                rerank: rerank,
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
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
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
    public static async Task<GroupByResult> NearText(
        this QueryClient client,
        NearTextInput query,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
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
                autoLimit: autoLimit,
                rerank: rerank,
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
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
    }
}

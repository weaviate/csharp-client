using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Aggregate near text with grouping.
    /// </summary>
    /// <param name="query">Text query</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="moveTo">Move towards concept</param>
    /// <param name="moveAway">Move away from concept</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targets">Target vectors</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    /// <summary>
    /// Aggregate near text.
    /// </summary>
    /// <param name="query">Text query</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="moveTo">Move towards concept</param>
    /// <param name="moveAway">Move away from concept</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearText(
        AutoArray<string> query,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query.ToArray(),
            certainty,
            distance,
            limit,
            moveTo,
            moveAway,
            filters,
            null, // No GroupByRequest for NearText
            null,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near text.
    /// </summary>
    /// <param name="query">Text query</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="moveTo">Move towards concept</param>
    /// <param name="moveAway">Move away from concept</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targets">Target vectors</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    /// <summary>
    /// Aggregate near text with grouping.
    /// </summary>
    /// <param name="query">Text query</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="moveTo">Move towards concept</param>
    /// <param name="moveAway">Move away from concept</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> NearText(
        AutoArray<string> query,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query.ToArray(),
            certainty,
            distance,
            limit,
            moveTo,
            moveAway,
            filters,
            groupBy,
            null,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near text with grouping using a NearTextInput record.
    /// </summary>
    /// <param name="query">Near-text input containing query text, target vectors, certainty, distance, and move parameters.</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    /// <summary>
    /// Aggregate near text with grouping using a NearTextInput record.
    /// </summary>
    /// <param name="query">Near-text input containing query text, target vectors, certainty, distance, and move parameters.</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> NearText(
        NearTextInput query,
        Aggregate.GroupBy? groupBy,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query.Query.ToArray(),
            query.Certainty.HasValue ? (double?)query.Certainty.Value : null,
            query.Distance.HasValue ? (double?)query.Distance.Value : null,
            limit,
            query.MoveTo,
            query.MoveAway,
            filters,
            groupBy,
            query.TargetVectors,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );
        return AggregateGroupByResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near text using a NearTextInput record.
    /// </summary>
    /// <param name="query">Near-text input containing query text, target vectors, certainty, distance, and move parameters.</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    /// <summary>
    /// Aggregate near text using a NearTextInput record.
    /// </summary>
    /// <param name="query">Near-text input containing query text, target vectors, certainty, distance, and move parameters.</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearText(
        NearTextInput query,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query.Query.ToArray(),
            query.Certainty.HasValue ? (double?)query.Certainty.Value : null,
            query.Distance.HasValue ? (double?)query.Distance.Value : null,
            limit,
            query.MoveTo,
            query.MoveAway,
            filters,
            null, // No GroupByRequest
            query.TargetVectors,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );
        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near text using a lambda builder for NearTextInput.
    /// </summary>
    /// <param name="query">Lambda builder for creating NearTextInput with target vectors.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="filters">Filters to apply.</param>
    /// <param name="totalCount">Whether to include total count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="returnMetrics">Metrics to return.</param>
    /// <returns>Aggregate result.</returns>
    public async Task<AggregateResult> NearText(
        NearTextInput.FactoryFn query,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await NearText(
            query(VectorInputBuilderFactories.CreateNearTextBuilder()),
            limit,
            filters,
            totalCount,
            cancellationToken,
            returnMetrics
        );

    /// <summary>
    /// Aggregate near text with grouping using a lambda builder for NearTextInput.
    /// </summary>
    /// <param name="query">Lambda builder for creating NearTextInput with target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="filters">Filters to apply.</param>
    /// <param name="totalCount">Whether to include total count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="returnMetrics">Metrics to return.</param>
    /// <returns>Aggregate group-by result.</returns>
    public async Task<AggregateGroupByResult> NearText(
        NearTextInput.FactoryFn query,
        Aggregate.GroupBy? groupBy,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await NearText(
            query(VectorInputBuilderFactories.CreateNearTextBuilder()),
            groupBy,
            limit,
            filters,
            totalCount,
            cancellationToken,
            returnMetrics
        );
}

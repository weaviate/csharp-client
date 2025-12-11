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
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> NearText(
        OneOrManyOf<string> query,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
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
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
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
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearText(
        OneOrManyOf<string> query,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
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
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }
}

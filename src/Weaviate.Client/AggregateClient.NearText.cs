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
    public async Task<AggregateGroupByResult> NearText(
        AutoArray<string> query,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        TargetVectors? targets = null,
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
            targets,
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
    /// <param name="targets">Target vectors</param>
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
        TargetVectors? targets = null,
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
            targets,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }
}

/// <summary>
/// Extension methods for AggregateClient NearText with lambda target vector builders.
/// </summary>
public static class AggregateClientNearTextExtensions
{
    /// <summary>
    /// Aggregate near text with grouping using a lambda to build target vectors.
    /// </summary>
    public static async Task<AggregateGroupByResult> NearText(
        this AggregateClient client,
        AutoArray<string> query,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        TargetVectors.FactoryFn? targets = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await client.NearText(
            query: query,
            groupBy: groupBy,
            certainty: certainty,
            distance: distance,
            limit: limit,
            moveTo: moveTo,
            moveAway: moveAway,
            filters: filters,
            targets: targets?.Invoke(new TargetVectors.Builder()),
            totalCount: totalCount,
            cancellationToken: cancellationToken,
            returnMetrics: returnMetrics
        );

    /// <summary>
    /// Aggregate near text using a lambda to build target vectors.
    /// </summary>
    public static async Task<AggregateResult> NearText(
        this AggregateClient client,
        AutoArray<string> query,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        TargetVectors.FactoryFn? targets = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await client.NearText(
            query: query,
            certainty: certainty,
            distance: distance,
            limit: limit,
            moveTo: moveTo,
            moveAway: moveAway,
            filters: filters,
            targets: targets?.Invoke(new TargetVectors.Builder()),
            totalCount: totalCount,
            cancellationToken: cancellationToken,
            returnMetrics: returnMetrics
        );
}

using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Aggregate near object.
    /// </summary>
    /// <param name="nearObject">Object ID to search near</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearObject(
        Guid nearObject,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearObject(
            _collectionClient.Name,
            objectID: nearObject,
            certainty: certainty,
            distance: distance,
            limit: limit,
            filters: filters,
            groupBy: null,
            targetVector: targetVector,
            totalCount: totalCount,
            _collectionClient.Tenant,
            metrics: returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near object with grouping.
    /// </summary>
    /// <param name="nearObject">Object ID to search near</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="returnMetrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> NearObject(
        Guid nearObject,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearObject(
            _collectionClient.Name,
            objectID: nearObject,
            certainty: certainty,
            distance: distance,
            limit: limit,
            filters: filters,
            groupBy: groupBy,
            targetVector: targetVector,
            totalCount: totalCount,
            _collectionClient.Tenant,
            metrics: returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

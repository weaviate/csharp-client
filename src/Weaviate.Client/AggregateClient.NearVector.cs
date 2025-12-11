using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Aggregate near vector with grouping.
    /// </summary>
    /// <param name="vector">Vector to search near</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="return_metrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> NearVector(
        Vectors vector,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? return_metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearVector(
            _collectionName,
            vector,
            certainty,
            distance,
            limit,
            filters,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            return_metrics ?? [],
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near vector.
    /// </summary>
    /// <param name="vector">Vector to search near</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="return_metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearVector(
        Vectors vector,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? return_metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearVector(
            _collectionName,
            vector,
            certainty,
            distance,
            limit,
            filters,
            null, // No GroupByRequest for NearVector
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            return_metrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }
}

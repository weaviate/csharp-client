using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Aggregate over all objects in the collection.
    /// </summary>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="return_metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> OverAll(
        bool totalCount = true,
        Filter? filters = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? return_metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            filters,
            null, // No GroupByRequest for OverAll
            totalCount,
            _collectionClient.Tenant,
            return_metrics ?? [],
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate over all objects in the collection with grouping.
    /// </summary>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="return_metrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> OverAll(
        Aggregate.GroupBy groupBy,
        bool totalCount = true,
        Filter? filters = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? return_metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            filters,
            groupBy,
            totalCount,
            _collectionClient.Tenant,
            return_metrics ?? [],
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

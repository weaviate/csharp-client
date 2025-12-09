using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Aggregate near media.
    /// </summary>
    /// <param name="media">Media data</param>
    /// <param name="mediaType">Type of media</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearMedia(
        byte[] media,
        NearMediaType mediaType,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? metrics = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            mediaType,
            certainty,
            distance,
            limit,
            filters,
            null,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near media with grouping.
    /// </summary>
    /// <param name="media">Media data</param>
    /// <param name="mediaType">Type of media</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> NearMedia(
        byte[] media,
        NearMediaType mediaType,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            mediaType,
            certainty,
            distance,
            limit,
            filters,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

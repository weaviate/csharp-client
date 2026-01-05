using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Performs a near-media aggregation using media embeddings.
    /// </summary>
    /// <example>
    /// // Simple image aggregation
    /// await collection.Aggregate.NearMedia(m => m.Image(imageBytes));
    ///
    /// // With target vectors and metrics
    /// await collection.Aggregate.NearMedia(
    ///     m => m.Video(videoBytes, certainty: 0.8f).Sum("v1", "v2"),
    ///     returnMetrics: new[] { Aggregate.Metric.Mean, Aggregate.Metric.Count }
    /// );
    /// </example>
    /// <param name="media">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
    /// <param name="filters">Filters to apply to the aggregation.</param>
    /// <param name="limit">Maximum number of results to aggregate.</param>
    /// <param name="totalCount">Whether to include total count in the result.</param>
    /// <param name="returnMetrics">Metrics to calculate and return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregation results.</returns>
    public async Task<AggregateResult> NearMedia(
        NearMediaInput.FactoryFn media,
        Filter? filters = null,
        uint? limit = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? returnMetrics = null,
        CancellationToken cancellationToken = default
    )
    {
        var input = media(VectorInputBuilderFactories.CreateNearMediaBuilder());
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            input.Media,
            input.Type,
            input.Certainty,
            input.Distance,
            limit,
            filters,
            null,
            input.TargetVectors,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Performs a near-media aggregation with group-by.
    /// </summary>
    /// <example>
    /// // Image aggregation with grouping
    /// await collection.Aggregate.NearMedia(
    ///     m => m.Image(imageBytes).Sum("visual", "semantic"),
    ///     groupBy: new Aggregate.GroupBy("category"),
    ///     returnMetrics: new[] { Aggregate.Metric.Count }
    /// );
    /// </example>
    /// <param name="media">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the aggregation.</param>
    /// <param name="limit">Maximum number of results to aggregate.</param>
    /// <param name="totalCount">Whether to include total count in the result.</param>
    /// <param name="returnMetrics">Metrics to calculate and return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Grouped aggregation results.</returns>
    public async Task<AggregateGroupByResult> NearMedia(
        NearMediaInput.FactoryFn media,
        Aggregate.GroupBy? groupBy,
        Filter? filters = null,
        uint? limit = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? returnMetrics = null,
        CancellationToken cancellationToken = default
    )
    {
        var input = media(VectorInputBuilderFactories.CreateNearMediaBuilder());
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            input.Media,
            input.Type,
            input.Certainty,
            input.Distance,
            limit,
            filters,
            groupBy,
            input.TargetVectors,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

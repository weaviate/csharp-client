using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Aggregate near vector.
    /// </summary>
    public async Task<AggregateResult> NearVector(
        VectorSearchInput vectors,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearVector(
            _collectionName,
            vectors,
            certainty,
            distance,
            limit,
            filters,
            null, // No GroupByRequest
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );
        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near vector with grouping.
    /// </summary>
    public async Task<AggregateGroupByResult> NearVector(
        VectorSearchInput vectors,
        Aggregate.GroupBy groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearVector(
            _collectionName,
            vectors,
            certainty,
            distance,
            limit,
            filters,
            groupBy,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );
        return AggregateGroupByResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near vector using lambda builder.
    /// </summary>
    public async Task<AggregateResult> NearVector(
        VectorSearchInput.FactoryFn vectors,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await NearVector(
            vectors(new VectorSearchInput.Builder()),
            certainty,
            distance,
            limit,
            filters,
            totalCount,
            cancellationToken,
            returnMetrics
        );

    /// <summary>
    /// Aggregate near vector with grouping using lambda builder.
    /// </summary>
    public async Task<AggregateGroupByResult> NearVector(
        VectorSearchInput.FactoryFn vectors,
        Aggregate.GroupBy groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await NearVector(
            vectors(new VectorSearchInput.Builder()),
            groupBy,
            certainty,
            distance,
            limit,
            filters,
            totalCount,
            cancellationToken,
            returnMetrics
        );

    /// <summary>
    /// Aggregate near vector search using a NearVectorInput record.
    /// </summary>
    /// <param name="input">Near-vector input containing vector, certainty, and distance.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="filters">Filters to apply.</param>
    /// <param name="totalCount">Whether to include total count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="returnMetrics">Metrics to return.</param>
    /// <returns>Aggregate result.</returns>
    public async Task<AggregateResult> NearVector(
        NearVectorInput input,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await NearVector(
            vectors: input.Vector,
            certainty: input.Certainty.HasValue ? (double?)input.Certainty.Value : null,
            distance: input.Distance.HasValue ? (double?)input.Distance.Value : null,
            limit: limit,
            filters: filters,
            totalCount: totalCount,
            cancellationToken: cancellationToken,
            returnMetrics: returnMetrics
        );

    /// <summary>
    /// Aggregate near vector search with group-by using a NearVectorInput record.
    /// </summary>
    /// <param name="input">Near-vector input containing vector, certainty, and distance.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="filters">Filters to apply.</param>
    /// <param name="totalCount">Whether to include total count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="returnMetrics">Metrics to return.</param>
    /// <returns>Aggregate group-by result.</returns>
    public async Task<AggregateGroupByResult> NearVector(
        NearVectorInput input,
        Aggregate.GroupBy groupBy,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await NearVector(
            vectors: input.Vector,
            groupBy: groupBy,
            certainty: input.Certainty.HasValue ? (double?)input.Certainty.Value : null,
            distance: input.Distance.HasValue ? (double?)input.Distance.Value : null,
            limit: limit,
            filters: filters,
            totalCount: totalCount,
            cancellationToken: cancellationToken,
            returnMetrics: returnMetrics
        );

    /// <summary>
    /// Aggregate near vector search using a lambda builder for NearVectorInput.
    /// </summary>
    /// <param name="inputBuilder">Lambda builder for creating NearVectorInput with target vectors.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="filters">Filters to apply.</param>
    /// <param name="totalCount">Whether to include total count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="returnMetrics">Metrics to return.</param>
    /// <returns>Aggregate result.</returns>
    public async Task<AggregateResult> NearVector(
        NearVectorInput.FactoryFn vectors,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await NearVector(
            vectors(VectorInputBuilderFactories.CreateNearVectorBuilder()),
            limit,
            filters,
            totalCount,
            cancellationToken,
            returnMetrics
        );

    /// <summary>
    /// Aggregate near vector search with group-by using a lambda builder for NearVectorInput.
    /// </summary>
    /// <param name="inputBuilder">Lambda builder for creating NearVectorInput with target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="filters">Filters to apply.</param>
    /// <param name="totalCount">Whether to include total count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="returnMetrics">Metrics to return.</param>
    /// <returns>Aggregate group-by result.</returns>
    public async Task<AggregateGroupByResult> NearVector(
        NearVectorInput.FactoryFn vectors,
        Aggregate.GroupBy groupBy,
        uint? limit = null,
        Filter? filters = null,
        bool totalCount = true,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? returnMetrics = null
    ) =>
        await NearVector(
            vectors(VectorInputBuilderFactories.CreateNearVectorBuilder()),
            groupBy,
            limit,
            filters,
            totalCount,
            cancellationToken,
            returnMetrics
        );
}

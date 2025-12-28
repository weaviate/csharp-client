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
        Func<VectorSearchInput.Builder, VectorSearchInput> vectors,
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
        Func<VectorSearchInput.Builder, VectorSearchInput> vectors,
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
}

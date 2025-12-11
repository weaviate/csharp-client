using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Aggregate using hybrid search.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="alpha">Alpha value for hybrid search</param>
    /// <param name="vectors">Vectors for search</param>
    /// <param name="queryProperties">Properties to query</param>
    /// <param name="objectLimit">Object limit</param>
    /// <param name="bm25Operator">BM25 operator</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="maxVectorDistance">Maximum vector distance</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> Hybrid(
        string? query = null,
        float alpha = 0.7f,
        Vectors? vectors = null,
        string[]? queryProperties = null,
        uint? objectLimit = null,
        BM25Operator? bm25Operator = null,
        Filter? filters = null,
        string? targetVector = null,
        float? maxVectorDistance = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? metrics = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.AggregateHybrid(
            _collectionName,
            query,
            alpha,
            vectors,
            queryProperties,
            bm25Operator,
            targetVector,
            maxVectorDistance,
            filters,
            null,
            objectLimit,
            totalCount,
            _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate using hybrid search with grouping.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="alpha">Alpha value for hybrid search</param>
    /// <param name="vectors">Vectors for search</param>
    /// <param name="queryProperties">Properties to query</param>
    /// <param name="objectLimit">Object limit</param>
    /// <param name="bm25Operator">BM25 operator</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="maxVectorDistance">Maximum vector distance</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> Hybrid(
        string? query,
        Aggregate.GroupBy groupBy,
        float alpha = 0.7f,
        Vectors? vectors = null,
        string[]? queryProperties = null,
        uint? objectLimit = null,
        BM25Operator? bm25Operator = null,
        Filter? filters = null,
        string? targetVector = null,
        float? maxVectorDistance = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? metrics = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.AggregateHybrid(
            _collectionName,
            query,
            alpha,
            vectors,
            queryProperties,
            bm25Operator,
            targetVector,
            maxVectorDistance,
            filters,
            groupBy,
            objectLimit,
            totalCount,
            _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

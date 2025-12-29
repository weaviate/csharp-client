using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class AggregateClient
{
    /// <summary>
    /// Aggregate using hybrid search.
    /// </summary>
    public Task<AggregateResult> Hybrid(
        string query,
        float alpha = 0.7f,
        string[]? queryProperties = null,
        uint? objectLimit = null,
        BM25Operator? bm25Operator = null,
        Filter? filters = null,
        float? maxVectorDistance = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? returnMetrics = null,
        CancellationToken cancellationToken = default
    ) =>
        Hybrid(
            query: query,
            vectors: null,
            alpha: alpha,
            queryProperties: queryProperties,
            objectLimit: objectLimit,
            bm25Operator: bm25Operator,
            filters: filters,
            maxVectorDistance: maxVectorDistance,
            totalCount: totalCount,
            returnMetrics: returnMetrics,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Aggregate using hybrid search.
    /// </summary>
    public async Task<AggregateResult> Hybrid(
        string? query,
        HybridVectorInput? vectors,
        float alpha = 0.7f,
        string[]? queryProperties = null,
        uint? objectLimit = null,
        BM25Operator? bm25Operator = null,
        Filter? filters = null,
        float? maxVectorDistance = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? returnMetrics = null,
        CancellationToken cancellationToken = default
    )
    {
        if (query is null && vectors is null)
        {
            throw new ArgumentException(
                "At least one of 'query' or 'vectors' must be provided for hybrid search."
            );
        }

        var result = await _client.GrpcClient.AggregateHybrid(
            _collectionName,
            query,
            alpha,
            vectors,
            queryProperties,
            bm25Operator,
            maxVectorDistance,
            filters,
            null,
            objectLimit,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate using hybrid search with grouping.
    /// </summary>
    public Task<AggregateGroupByResult> Hybrid(
        string query,
        Aggregate.GroupBy groupBy,
        float alpha = 0.7f,
        string[]? queryProperties = null,
        uint? objectLimit = null,
        BM25Operator? bm25Operator = null,
        Filter? filters = null,
        float? maxVectorDistance = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? returnMetrics = null,
        CancellationToken cancellationToken = default
    ) =>
        Hybrid(
            query: query,
            vectors: null,
            groupBy: groupBy,
            alpha: alpha,
            queryProperties: queryProperties,
            objectLimit: objectLimit,
            bm25Operator: bm25Operator,
            filters: filters,
            maxVectorDistance: maxVectorDistance,
            totalCount: totalCount,
            returnMetrics: returnMetrics,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Aggregate using hybrid search with grouping.
    /// </summary>
    public async Task<AggregateGroupByResult> Hybrid(
        string? query,
        HybridVectorInput? vectors,
        Aggregate.GroupBy groupBy,
        float alpha = 0.7f,
        string[]? queryProperties = null,
        uint? objectLimit = null,
        BM25Operator? bm25Operator = null,
        Filter? filters = null,
        float? maxVectorDistance = null,
        bool totalCount = true,
        IEnumerable<Aggregate.Metric>? returnMetrics = null,
        CancellationToken cancellationToken = default
    )
    {
        if (query is null && vectors is null)
        {
            throw new ArgumentException(
                "At least one of 'query' or 'vectors' must be provided for hybrid search."
            );
        }

        var result = await _client.GrpcClient.AggregateHybrid(
            _collectionName,
            query,
            alpha,
            vectors,
            queryProperties,
            bm25Operator,
            maxVectorDistance,
            filters,
            groupBy,
            objectLimit,
            totalCount,
            _collectionClient.Tenant,
            returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

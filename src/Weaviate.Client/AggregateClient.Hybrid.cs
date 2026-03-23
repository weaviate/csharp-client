using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The aggregate client class
/// </summary>
public partial class AggregateClient
{
    /// <summary>
    /// Aggregate using hybrid search.
    /// </summary>
    public Task<AggregateResult> Hybrid(
        string query,
        float? alpha = null,
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
            vectors: (HybridVectorInput?)null,
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
        float? alpha = null,
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
            query: query,
            alpha: alpha,
            vectors: vectors,
            queryProperties: queryProperties,
            bm25Operator: bm25Operator,
            maxVectorDistance: maxVectorDistance,
            filter: filters,
            groupBy: null,
            objectLimit: objectLimit,
            totalCount: totalCount,
            tenant: _collectionClient.Tenant,
            metrics: returnMetrics,
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
            vectors: (HybridVectorInput?)null,
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
            query: query,
            alpha: alpha,
            vectors: vectors,
            queryProperties: queryProperties,
            bm25Operator: bm25Operator,
            maxVectorDistance: maxVectorDistance,
            filter: filters,
            groupBy: groupBy,
            objectLimit: objectLimit,
            totalCount: totalCount,
            tenant: _collectionClient.Tenant,
            metrics: returnMetrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

/// <summary>
/// Extension methods for AggregateClient Hybrid search with lambda vector builders.
/// </summary>
public static class AggregateClientHybridExtensions
{
    /// <summary>
    /// Aggregate using hybrid search with a lambda to build HybridVectorInput.
    /// This allows chaining NearVector or NearText configuration with target vectors.
    /// </summary>
    /// <example>
    /// await collection.Aggregate.Hybrid(
    ///     "test",
    ///     v => v.NearVector().ManualWeights(
    ///         ("title", 1.2, new[] { 1f, 2f }),
    ///         ("description", 0.8, new[] { 3f, 4f })
    ///     )
    /// );
    /// </example>
    public static async Task<AggregateResult> Hybrid(
        this AggregateClient client,
        string query,
        HybridVectorInput.FactoryFn vectors,
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
        await client.Hybrid(
            query: query,
            vectors: vectors(VectorInputBuilderFactories.CreateHybridBuilder()),
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
    /// Aggregate using hybrid search with grouping and a lambda to build HybridVectorInput.
    /// This allows chaining NearVector or NearText configuration with target vectors.
    /// </summary>
    public static async Task<AggregateGroupByResult> Hybrid(
        this AggregateClient client,
        string query,
        HybridVectorInput.FactoryFn vectors,
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
        await client.Hybrid(
            query: query,
            vectors: vectors(VectorInputBuilderFactories.CreateHybridBuilder()),
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
}

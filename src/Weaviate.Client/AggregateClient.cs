using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient
{
    public AggregateClient Aggregate => new(this);
}

public partial class AggregateClient
{
    private readonly CollectionClient _collectionClient;
    private WeaviateClient _client => _collectionClient.Client;
    private string _collectionName => _collectionClient.Name;

    internal AggregateClient(CollectionClient collectionClient)
    {
        _collectionClient = collectionClient;
    }

    /// <summary>
    /// Creates a cancellation token with query-specific timeout configuration.
    /// Uses QueryTimeout if configured, falls back to DefaultTimeout, then to WeaviateDefaults.QueryTimeout.
    /// </summary>
    private CancellationToken CreateTimeoutCancellationToken(CancellationToken userToken = default)
    {
        var effectiveTimeout =
            _client.QueryTimeout ?? _client.DefaultTimeout ?? WeaviateDefaults.QueryTimeout;
        return TimeoutHelper.GetCancellationToken(effectiveTimeout, userToken);
    }

    /// <summary>
    /// Aggregate over all objects in the collection.
    /// </summary>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> OverAll(
        bool totalCount = true,
        Filter? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            filters,
            null, // No GroupByRequest for OverAll
            totalCount,
            tenant ?? _collectionClient.Tenant,
            metrics,
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
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> OverAll(
        Aggregate.GroupBy groupBy,
        bool totalCount = true,
        Filter? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            filters,
            groupBy,
            totalCount,
            tenant ?? _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

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
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
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
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
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
            tenant ?? _collectionClient.Tenant,
            metrics,
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
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearVector(
        Vectors vector,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
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
            tenant ?? _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near text with grouping.
    /// </summary>
    /// <param name="query">Text query</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="moveTo">Move towards concept</param>
    /// <param name="moveAway">Move away from concept</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> NearText(
        OneOrManyOf<string> query,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query.ToArray(),
            certainty,
            distance,
            limit,
            moveTo,
            moveAway,
            filters,
            groupBy,
            targetVector,
            totalCount,
            tenant ?? _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near text.
    /// </summary>
    /// <param name="query">Text query</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="moveTo">Move towards concept</param>
    /// <param name="moveAway">Move away from concept</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearText(
        OneOrManyOf<string> query,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query.ToArray(),
            certainty,
            distance,
            limit,
            moveTo,
            moveAway,
            filters,
            null, // No GroupByRequest for NearText
            targetVector,
            totalCount,
            tenant ?? _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

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
    /// <param name="tenant">Tenant name</param>
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
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
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
            tenant ?? _collectionClient.Tenant,
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
    /// <param name="tenant">Tenant name</param>
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
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
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
            tenant ?? _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

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
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearObject(
        Guid nearObject,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
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
            tenant ?? _collectionClient.Tenant,
            metrics: metrics,
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
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
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
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
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
            tenant ?? _collectionClient.Tenant,
            metrics: metrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near image.
    /// </summary>
    /// <param name="media">Image data</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Aggregate result</returns>
    public async Task<AggregateResult> NearImage(
        byte[] media,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Image,
            certainty,
            distance,
            limit,
            filters,
            null,
            targetVector,
            totalCount,
            tenant ?? _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateResult.FromGrpcReply(result);
    }

    /// <summary>
    /// Aggregate near image with grouping.
    /// </summary>
    /// <param name="media">Image data</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="totalCount">Whether to include total count</param>
    /// <param name="tenant">Tenant name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="metrics">Metrics to aggregate</param>
    /// <returns>Grouped aggregate result</returns>
    public async Task<AggregateGroupByResult> NearImage(
        byte[] media,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        CancellationToken cancellationToken = default,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Image,
            certainty,
            distance,
            limit,
            filters,
            groupBy,
            targetVector,
            totalCount,
            tenant ?? _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

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
    /// <param name="tenant">Tenant name</param>
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
        string? tenant = null,
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
            null,
            targetVector,
            totalCount,
            tenant ?? _collectionClient.Tenant,
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
    /// <param name="tenant">Tenant name</param>
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
        string? tenant = null,
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
            tenant ?? _collectionClient.Tenant,
            metrics,
            cancellationToken: cancellationToken
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

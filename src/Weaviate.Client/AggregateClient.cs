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

    public async Task<AggregateResult> OverAll(
        bool totalCount = true,
        Filter? filters = null,
        string? tenant = null,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            filters,
            null, // No GroupByRequest for OverAll
            totalCount,
            tenant ?? _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> OverAll(
        Aggregate.GroupBy groupBy,
        bool totalCount = true,
        Filter? filters = null,
        string? tenant = null,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            filters,
            groupBy,
            totalCount,
            tenant ?? _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

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
        params Aggregate.Metric[] metrics
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
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearVector(
        Vectors vector,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        params Aggregate.Metric[] metrics
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
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> NearText(
        string[] query,
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
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query,
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
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearText(
        string[] query,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query,
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
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

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
        params Aggregate.Metric[] metrics
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
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

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
        params Aggregate.Metric[] metrics
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
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearObject(
        Guid nearObject,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        params Aggregate.Metric[] metrics
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
            metrics: metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

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
        params Aggregate.Metric[] metrics
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
            metrics: metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearImage(
        byte[] media,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filters = null,
        TargetVectors? targetVector = null,
        bool totalCount = true,
        string? tenant = null,
        params Aggregate.Metric[] metrics
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
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

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
        params Aggregate.Metric[] metrics
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
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

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
        params Aggregate.Metric[] metrics
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
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

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
        params Aggregate.Metric[] metrics
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
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

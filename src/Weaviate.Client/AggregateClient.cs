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
        Filter? filter = null,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            filter,
            null, // No GroupByRequest for OverAll
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> OverAll(
        Aggregate.GroupBy? groupBy,
        bool totalCount = true,
        Filter? filter = null,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            filter,
            groupBy,
            totalCount,
            _collectionClient.Tenant,
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
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearVector(
            _collectionName,
            vector,
            certainty,
            distance,
            limit,
            filter,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearVector(
        Vectors vector,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearVector(
            _collectionName,
            vector,
            certainty,
            distance,
            limit,
            filter,
            null, // No GroupByRequest for NearVector
            targetVector,
            totalCount,
            _collectionClient.Tenant,
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
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
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
            filter,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
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
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
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
            filter,
            null, // No GroupByRequest for NearText
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> Hybrid(
        string? query = null,
        float alpha = 0.7f,
        AbstractVectorData? vectors = null,
        string[]? queryProperties = null,
        BM25Operator? bm25Operator = null,
        Filter? filter = null,
        string? targetVector = null,
        float? maxVectorDistance = null,
        bool totalCount = true,
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
            filter,
            null, // No GroupBy for AggregateResult
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> Hybrid(
        string? query,
        Aggregate.GroupBy groupBy,
        float alpha = 0.7f,
        AbstractVectorData? vectors = null,
        string[]? queryProperties = null,
        uint? objectLimit = null,
        BM25Operator? bm25Operator = null,
        Filter? filter = null,
        string? targetVector = null,
        float? maxVectorDistance = null,
        bool totalCount = true,
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
            filter,
            groupBy,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearImage(
        byte[] media,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? groupBy = null,
        string[]? targetVector = null,
        bool totalCount = true,
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
            filter,
            null,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
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
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
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
            filter,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearVideo(
        byte[] media,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Video,
            certainty,
            distance,
            limit,
            filter,
            null,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> NearVideo(
        byte[] media,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Video,
            certainty,
            distance,
            limit,
            filter,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearAudio(
        byte[] media,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Audio,
            certainty,
            distance,
            limit,
            filter,
            null,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> NearAudio(
        byte[] media,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Audio,
            certainty,
            distance,
            limit,
            filter,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearDepth(
        byte[] media,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Depth,
            certainty,
            distance,
            limit,
            filter,
            null,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> NearDepth(
        byte[] media,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Depth,
            certainty,
            distance,
            limit,
            filter,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearThermal(
        byte[] media,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Thermal,
            certainty,
            distance,
            limit,
            filter,
            null,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> NearThermal(
        byte[] media,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.Thermal,
            certainty,
            distance,
            limit,
            filter,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearIMU(
        byte[] media,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.IMU,
            certainty,
            distance,
            limit,
            filter,
            null,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> NearIMU(
        byte[] media,
        Aggregate.GroupBy? groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Aggregate.Metric[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearMedia(
            _collectionName,
            media,
            NearMediaType.IMU,
            certainty,
            distance,
            limit,
            filter,
            groupBy,
            targetVector,
            totalCount,
            _collectionClient.Tenant,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }
}

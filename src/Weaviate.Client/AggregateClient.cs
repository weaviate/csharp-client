using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient<TData>
{
    public AggregateClient<TData> Aggregate => new(this);
}

public partial class AggregateClient<TData>
{
    private readonly CollectionClient<TData> _collectionClient;
    private WeaviateClient _client => _collectionClient.Client;
    private string _collectionName => _collectionClient.Name;

    internal AggregateClient(CollectionClient<TData> collectionClient)
    {
        _collectionClient = collectionClient;
    }

    internal async Task<AggregateResult> OverAll(
        bool totalCount = true,
        Filter? filter = null,
        params MetricRequest[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            totalCount,
            null, // No GroupByRequest for OverAll
            filter,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    internal async Task<AggregateGroupByResult> OverAll(
        GroupByRequest? groupByRequest,
        bool totalCount = true,
        Filter? filter = null,
        params MetricRequest[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateOverAll(
            _collectionName,
            totalCount,
            groupByRequest,
            filter,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    internal async Task<AggregateGroupByResult> NearVector(
        float[] vector,
        GroupByRequest? groupByRequest,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params MetricRequest[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearVector(
            _collectionName,
            vector,
            certainty,
            distance,
            limit,
            filter,
            groupByRequest,
            targetVector,
            totalCount,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    internal async Task<AggregateResult> NearVector(
        float[] vector,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params MetricRequest[] metrics
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
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    internal async Task<AggregateGroupByResult> NearText(
        string[] query,
        GroupByRequest? groupByRequest,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params MetricRequest[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query,
            certainty,
            distance,
            limit,
            filter,
            groupByRequest,
            targetVector,
            totalCount,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    internal async Task<AggregateResult> NearText(
        string[] query,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params MetricRequest[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateNearText(
            _collectionName,
            query,
            certainty,
            distance,
            limit,
            filter,
            null, // No GroupByRequest for NearText
            targetVector,
            totalCount,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }

    internal async Task<AggregateGroupByResult> Hybrid(
        GroupByRequest? groupByRequest,
        string? query = null,
        float[]? vector = null,
        double? alpha = 0.7,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params MetricRequest[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateHybrid(
            _collectionName,
            query,
            vector,
            alpha,
            certainty,
            distance,
            limit,
            filter,
            groupByRequest,
            targetVector,
            totalCount,
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    internal async Task<AggregateResult> Hybrid(
        string? query = null,
        float[]? vector = null,
        double? alpha = 0.7,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params MetricRequest[] metrics
    )
    {
        var result = await _client.GrpcClient.AggregateHybrid(
            _collectionName,
            query,
            vector,
            alpha,
            certainty,
            distance,
            limit,
            filter,
            null,
            targetVector,
            totalCount,
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }
}

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
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateGroupByResult> NearVector(
        float[] vector,
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
            metrics
        );

        return AggregateGroupByResult.FromGrpcReply(result);
    }

    public async Task<AggregateResult> NearVector(
        float[] vector,
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
            metrics
        );

        return AggregateResult.FromGrpcReply(result);
    }
}

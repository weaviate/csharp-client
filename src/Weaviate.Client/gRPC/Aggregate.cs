using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public partial class WeaviateGrpcClient
{
    internal async Task<AggregateReply> Aggregate(AggregateRequest request)
    {
        AggregateReply reply = await _grpcClient.AggregateAsync(request);

        return reply;
    }

    internal async Task<AggregateReply> AggregateOverAll(
        string collection,
        bool totalCount,
        GroupByRequest? groupByRequest,
        Filter? filter,
        params MetricRequest[] metrics
    )
    {
        var request = new AggregateRequest();

        return await Aggregate(request);
    }

    internal async Task<AggregateReply> AggregateHybrid(
        string collectionName,
        string? query,
        float[]? vector,
        double? alpha,
        float? certainty,
        float? distance,
        uint? limit,
        Filter? filter,
        GroupByRequest? groupByRequest,
        string[]? targetVector,
        bool totalCount,
        MetricRequest[] metrics
    )
    {
        var request = new AggregateRequest();

        return await Aggregate(request);
    }

    internal async Task<AggregateReply> AggregateNearText(
        string collectionName,
        string[] query,
        float? certainty,
        float? distance,
        uint? limit,
        Filter? filter,
        GroupByRequest? groupByRequest,
        string[]? targetVector,
        bool totalCount,
        MetricRequest[] metrics
    )
    {
        var request = new AggregateRequest();

        return await Aggregate(request);
    }

    internal async Task<AggregateReply> AggregateNearVector(
        string collectionName,
        float[] vector,
        float? certainty,
        float? distance,
        uint? limit,
        Filter? filter,
        GroupByRequest? groupByRequest,
        string[]? targetVector,
        bool totalCount,
        MetricRequest[] metrics
    )
    {
        var request = new AggregateRequest();

        return await Aggregate(request);
    }
}

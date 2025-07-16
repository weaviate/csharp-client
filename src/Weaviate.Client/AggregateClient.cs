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
        GroupByRequest? groupByRequest = null,
        Filter? filter = null,
        params Metric[] metrics
    )
    {
        await Task.CompletedTask;

        // TODO Placeholder
        return new AggregateResult();
    }

    internal async Task<AggregateGroupByResult> NearVector(
        float[] vector,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        GroupByRequest? groupByRequest = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Metric[] metrics
    )
    {
        await Task.CompletedTask;

        // TODO Placeholder
        return new AggregateGroupByResult();
    }

    internal async Task<AggregateResult> NearVector(
        float[] vector,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Metric[] metrics
    )
    {
        await Task.CompletedTask;

        // TODO Placeholder
        return new AggregateResult();
    }

    internal async Task<AggregateGroupByResult> NearText(
        string[] query,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        GroupByRequest? groupByRequest = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Metric[] metrics
    )
    {
        await Task.CompletedTask;

        // TODO Placeholder
        return new AggregateGroupByResult();
    }

    internal async Task<AggregateResult> NearText(
        string[] query,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Metric[] metrics
    )
    {
        await Task.CompletedTask;

        // TODO Placeholder
        return new AggregateResult();
    }

    internal async Task<AggregateGroupByResult> Hybrid(
        string? query = null,
        float[]? vector = null,
        double? alpha = 0.7,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        Filter? filter = null,
        GroupByRequest? groupByRequest = null,
        string[]? targetVector = null,
        bool totalCount = true,
        params Metric[] metrics
    )
    {
        await Task.CompletedTask;

        // TODO Placeholder
        return new AggregateGroupByResult();
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
        params Metric[] metrics
    )
    {
        await Task.CompletedTask;

        // TODO Placeholder
        return new AggregateResult();
    }
}

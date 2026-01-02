using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying AggregateClient vector search syntax with aggregation.
/// </summary>
[Collection("Unit Tests")]
public class TestAggregateVectorSyntax : IAsyncLifetime
{
    private const string CollectionName = "TestCollection";

    private Func<Grpc.Protobuf.V1.AggregateRequest?> _getRequest = null!;
    private CollectionClient _collection = null!;

    public ValueTask InitializeAsync()
    {
        var (client, getRequest) = MockGrpcClient.CreateWithAggregateCapture();
        _getRequest = getRequest;
        _collection = client.Collections.Use(CollectionName);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    #region AggregateClient.NearVector Tests

    [Fact]
    public async Task Aggregate_NearVector_WithMetrics_ProducesValidRequest()
    {
        // Act
        await _collection.Aggregate.NearVector(
            new[] { 1f, 2f, 3f },
            returnMetrics: [Metrics.ForProperty("price").Number(mean: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
    }

    [Fact]
    public async Task Aggregate_NearVector_WithGroupBy_ProducesValidRequest()
    {
        // Act
        await _collection.Aggregate.NearVector(
            new[] { 1f, 2f, 3f },
            groupBy: new Aggregate.GroupBy("category", Limit: 10),
            returnMetrics: [Metrics.ForProperty("price").Number(mean: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("category", request.GroupBy.Property);
    }

    [Fact]
    public async Task Aggregate_NearVector_LambdaBuilder_ProducesValidRequest()
    {
        // Act - Lambda builder with multi-target
        await _collection.Aggregate.NearVector(
            v => v.Average(("title", new[] { 1f, 2f }), ("desc", new[] { 3f, 4f })),
            returnMetrics: [Metrics.ForProperty("price").Number(mean: true, count: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
    }

    #endregion

    #region AggregateClient.NearText Tests

    [Fact]
    public async Task Aggregate_NearText_WithMetrics_ProducesValidRequest()
    {
        // Act
        await _collection.Aggregate.NearText(
            new NearTextInput("banana"),
            returnMetrics: [Metrics.ForProperty("price").Number(mean: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal("banana", request.NearText.Query[0]);
    }

    [Fact]
    public async Task Aggregate_NearText_WithTargetVectors_AndGroupBy_ProducesValidRequest()
    {
        // Act
        await _collection.Aggregate.NearText(
            new NearTextInput("banana", TargetVectors: TargetVectors.Sum("title", "description")),
            groupBy: new Aggregate.GroupBy("category", Limit: 5),
            returnMetrics: [Metrics.ForProperty("price").Number(mean: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.NotNull(request.NearText.Targets);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("category", request.GroupBy.Property);
    }

    #endregion

    #region AggregateClient.Hybrid Tests

    [Fact]
    public async Task Aggregate_Hybrid_TextOnly_WithMetrics_ProducesValidRequest()
    {
        // Act
        await _collection.Aggregate.Hybrid(
            "search query",
            returnMetrics: [Metrics.ForProperty("price").Number(mean: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        // AggregateRequest structure validated - hybrid search parameters handled internally
    }

    [Fact]
    public async Task Aggregate_Hybrid_WithVectors_AndGroupBy_ProducesValidRequest()
    {
        // Act
        await _collection.Aggregate.Hybrid(
            "search query",
            vectors: new[] { 1f, 2f, 3f },
            groupBy: new Aggregate.GroupBy("category", Limit: 10),
            returnMetrics: [Metrics.ForProperty("price").Number(mean: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("category", request.GroupBy.Property);
    }

    [Fact]
    public async Task Aggregate_Hybrid_NearTextInput_WithMetrics_ProducesValidRequest()
    {
        // Act
        await _collection.Aggregate.Hybrid(
            query: null,
            vectors: new NearTextInput("banana", TargetVectors: new[] { "title" }),
            returnMetrics: [Metrics.ForProperty("price").Number(mean: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        // AggregateRequest structure validated - NearText parameters handled internally
    }

    #endregion
}

using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying the NearText search input syntax using NearTextInput.FactoryFn lambda builders.
/// Target vectors MUST be specified via lambda builders - there is no separate targets parameter.
/// </summary>
[Collection("Unit Tests")]
public class TestNearTextSyntax : IAsyncLifetime
{
    private const string CollectionName = "TestCollection";

    private Func<Grpc.Protobuf.V1.SearchRequest?> _getRequest = null!;
    private CollectionClient _collection = null!;

    public ValueTask InitializeAsync()
    {
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        _getRequest = getRequest;
        _collection = client.Collections.Use(CollectionName);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    #region Simple NearTextInput Tests

    [Fact]
    public async Task NearText_SimpleNearTextInput_ProducesValidRequest()
    {
        // Act - Using NearTextInput without target vectors
        await _collection.Query.NearText(
            new NearTextInput(["banana"]),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Single(request.NearText.Query);
        Assert.Equal("banana", request.NearText.Query[0]);
        // Targets may be an empty object or null
    }

    #endregion

    #region NearTextInput With Target Vectors

    [Fact]
    public async Task NearText_NearTextInput_WithTargetVectors_ProducesValidRequest()
    {
        // Act - Using NearTextInput with TargetVectors.Sum
        await _collection.Query.NearText(
            new NearTextInput(["banana"], TargetVectors: TargetVectors.Sum("title", "description")),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.NotNull(request.NearText.Targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearText.Targets.Combination);
        Assert.Equal(2, request.NearText.Targets.TargetVectors.Count);
    }

    [Fact]
    public async Task NearText_NearTextInput_WithCertaintyAndTargets_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearText(
            new NearTextInput(
                ["banana"],
                TargetVectors: TargetVectors.ManualWeights(("title", 1.2), ("description", 0.8)),
                Certainty: 0.7f
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal(0.7, request.NearText.Certainty, precision: 5);
        Assert.NotNull(request.NearText.Targets);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.NearText.Targets.Combination);
        Assert.Equal(2, request.NearText.Targets.WeightsForTargets.Count);
    }

    #endregion

    #region NearTextInput.FactoryFn Lambda Builder

    [Fact]
    public async Task NearText_FactoryFn_Sum_ProducesValidRequest()
    {
        // Act - Lambda builder for NearTextInput with Sum
        await _collection.Query.NearText(
            v => v(["banana"]).Sum("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearText.Targets.Combination);
    }

    [Fact]
    public async Task NearText_FactoryFn_ManualWeights_ProducesValidRequest()
    {
        // Act - Lambda builder with ManualWeights
        await _collection.Query.NearText(
            v => v(["banana"]).ManualWeights(("title", 1.2), ("description", 0.8)),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.NearText.Targets.Combination);
        Assert.Equal(2, request.NearText.Targets.WeightsForTargets.Count);
    }

    [Fact]
    public async Task NearText_FactoryFn_WithCertainty_ProducesValidRequest()
    {
        // Act - Lambda builder with certainty parameter
        await _collection.Query.NearText(
            v => v(["banana"], certainty: 0.7f).Sum("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal(0.7, request.NearText.Certainty, precision: 5);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearText.Targets.Combination);
    }

    [Fact]
    public async Task NearText_FactoryFn_WithMoveParameters_ProducesValidRequest()
    {
        // Act - Lambda builder with Move parameters
        await _collection.Query.NearText(
            v =>
                v(
                        ["banana"],
                        moveTo: new Move(concepts: "fruit", force: 0.5f),
                        moveAway: new Move(concepts: "vegetable", force: 0.3f)
                    )
                    .Average("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.NotNull(request.NearText.MoveTo);
        Assert.Equal(0.5f, request.NearText.MoveTo.Force);
        Assert.NotNull(request.NearText.MoveAway);
        Assert.Equal(0.3f, request.NearText.MoveAway.Force);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.NearText.Targets.Combination);
    }

    [Fact]
    public async Task NearText_FactoryFn_Average_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearText(
            v => v(["banana"]).Average("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.NearText.Targets.Combination);
    }

    [Fact]
    public async Task NearText_FactoryFn_Minimum_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearText(
            v => v(["banana"]).Minimum("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.NearText.Targets.Combination);
    }

    [Fact]
    public async Task NearText_FactoryFn_RelativeScore_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearText(
            v => v(["banana"]).RelativeScore(("title", 0.7), ("description", 0.3)),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal(V1.CombinationMethod.TypeRelativeScore, request.NearText.Targets.Combination);
        Assert.Equal(2, request.NearText.Targets.WeightsForTargets.Count);
    }

    #endregion

    #region GroupBy Tests

    [Fact]
    public async Task NearText_WithGroupBy_ProducesValidRequest()
    {
        // Act - NearTextInput with GroupBy
        await _collection.Query.NearText(
            new NearTextInput(["banana"], TargetVectors: TargetVectors.Sum("title", "description")),
            new GroupByRequest("category") { ObjectsPerGroup = 3 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("category", request.GroupBy.Path[0]);
        Assert.Equal(3, request.GroupBy.ObjectsPerGroup);
    }

    [Fact]
    public async Task NearText_FactoryFn_WithGroupBy_ProducesValidRequest()
    {
        // Act - Lambda builder with GroupBy
        await _collection.Query.NearText(
            v => v(["banana"]).Sum("title", "description"),
            new GroupByRequest("category") { ObjectsPerGroup = 3 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("category", request.GroupBy.Path[0]);
        Assert.Equal(3, request.GroupBy.ObjectsPerGroup);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearText.Targets.Combination);
    }

    [Fact]
    public async Task NearText_WithMoveParameters_ProducesValidRequest()
    {
        // Act - Using NearTextInput with moveTo and moveAway
        await _collection.Query.NearText(
            new NearTextInput(
                Query: "banana",
                MoveTo: new Move(concepts: "fruit apple", force: 0.5f),
                MoveAway: new Move(concepts: "vegetable", force: 0.3f)
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal("banana", request.NearText.Query[0]);

        // Verify MoveTo
        Assert.NotNull(request.NearText.MoveTo);
        Assert.Equal("fruit apple", request.NearText.MoveTo.Concepts[0]);
        Assert.Equal(0.5f, request.NearText.MoveTo.Force);

        // Verify MoveAway
        Assert.NotNull(request.NearText.MoveAway);
        Assert.Equal("vegetable", request.NearText.MoveAway.Concepts[0]);
        Assert.Equal(0.3f, request.NearText.MoveAway.Force);
    }

    #endregion
}

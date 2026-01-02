using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying the NearVector search input syntax combinations compile correctly
/// and produce the expected gRPC request structure.
/// Covers all examples from docs/VECTOR_API_OVERVIEW.md.
/// </summary>
[Collection("Unit Tests")]
public class TestNearVectorSyntax : IAsyncLifetime
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

    #region Simple Float Array (Implicit Conversion)

    [Fact]
    public async Task NearVector_SimpleFloatArray_ProducesValidRequest()
    {
        // Arrange - Example from docs: await collection.Query.NearVector(new[] { 1f, 2f, 3f });
        float[] vector = [1f, 2f, 3f];

        // Act
        await _collection.Query.NearVector(
            vector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(CollectionName, request.Collection);
        Assert.NotNull(request.NearVector);
        Assert.Single(request.NearVector.Vectors);
        Assert.NotEmpty(request.NearVector.Vectors[0].VectorBytes);
        Assert.Empty(request.NearVector.VectorForTargets); // No named targets
    }

    #endregion

    #region Named Vectors

    [Fact]
    public async Task NearVector_NamedVectors_ProducesValidRequest()
    {
        // Arrange - Example from docs:
        // await collection.Query.NearVector(
        //     new Vectors {
        //         { "title", new[] { 1f, 2f } },
        //         { "description", new[] { 3f, 4f } }
        //     }
        // );

        // Act
        await _collection.Query.NearVector(
            new Vectors { { "title", new[] { 1f, 2f } }, { "description", new[] { 3f, 4f } } },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(CollectionName, request.Collection);
        Assert.NotNull(request.NearVector);
        Assert.Equal(2, request.NearVector.VectorForTargets.Count);
        Assert.Empty(request.NearVector.Vectors); // Named vectors use VectorForTargets

        var titleVector = request.NearVector.VectorForTargets.FirstOrDefault(v =>
            v.Name == "title"
        );
        Assert.NotNull(titleVector);
        Assert.Single(titleVector.Vectors);
        Assert.NotEmpty(titleVector.Vectors[0].VectorBytes);

        var descVector = request.NearVector.VectorForTargets.FirstOrDefault(v =>
            v.Name == "description"
        );
        Assert.NotNull(descVector);
        Assert.Single(descVector.Vectors);
        Assert.NotEmpty(descVector.Vectors[0].VectorBytes);
    }

    #endregion

    #region Lambda Builder - Sum Combination

    [Fact]
    public async Task NearVector_LambdaBuilder_Sum_ProducesValidRequest()
    {
        // Arrange - Example from docs:
        // await collection.Query.NearVector(
        //     v => v.Sum(
        //         ("title", new[] { 1f, 2f }),
        //         ("description", new[] { 3f, 4f })
        //     )
        // );

        // Act
        await _collection.Query.NearVector(
            v => v.Sum(("title", new[] { 1f, 2f }), ("description", new[] { 3f, 4f })),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(CollectionName, request.Collection);
        Assert.NotNull(request.NearVector);
        Assert.NotNull(request.NearVector.Targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearVector.Targets.Combination);
        Assert.Equal(2, request.NearVector.Targets.TargetVectors.Count);
        Assert.Contains("title", request.NearVector.Targets.TargetVectors);
        Assert.Contains("description", request.NearVector.Targets.TargetVectors);
        Assert.Equal(2, request.NearVector.VectorForTargets.Count);
    }

    #endregion

    #region Lambda Builder - ManualWeights

    [Fact]
    public async Task NearVector_LambdaBuilder_ManualWeights_ProducesValidRequest()
    {
        // Arrange - Example from docs:
        // await collection.Query.NearVector(
        //     v => v.ManualWeights(
        //         ("title", 1.2, new[] { 1f, 2f }),
        //         ("description", 0.8, new[] { 3f, 4f })
        //     )
        // );

        // Act
        await _collection.Query.NearVector(
            v =>
                v.ManualWeights(
                    ("title", 1.2, new[] { 1f, 2f }),
                    ("description", 0.8, new[] { 3f, 4f })
                ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(CollectionName, request.Collection);
        Assert.NotNull(request.NearVector);
        Assert.NotNull(request.NearVector.Targets);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.NearVector.Targets.Combination);
        Assert.Equal(2, request.NearVector.Targets.WeightsForTargets.Count);

        // Verify weights are set correctly
        var titleWeight = request.NearVector.Targets.WeightsForTargets.FirstOrDefault(w =>
            w.Target == "title"
        );
        Assert.NotNull(titleWeight);
        Assert.Equal(1.2, titleWeight.Weight, precision: 5);

        var descWeight = request.NearVector.Targets.WeightsForTargets.FirstOrDefault(w =>
            w.Target == "description"
        );
        Assert.NotNull(descWeight);
        Assert.Equal(0.8, descWeight.Weight, precision: 5);
    }

    #endregion

    #region Multi-Vector (ColBERT-style)

    [Fact]
    public async Task NearVector_MultiVector_ColBERTStyle_ProducesValidRequest()
    {
        // Arrange - Example from docs:
        // await collection.Query.NearVector(
        //     ("colbert", new[,] {
        //         { 1f, 2f },
        //         { 3f, 4f }
        //     })
        // );

        // Act
        await _collection.Query.NearVector(
            (
                "colbert",
                new[,]
                {
                    { 1f, 2f },
                    { 3f, 4f },
                }
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);

        // Multi-vector (ColBERT) creates a named vector with multiple vector entries
        // The exact structure depends on the conversion path, just verify NearVector is populated
        Assert.True(
            request.NearVector.VectorForTargets.Count > 0 || request.NearVector.Vectors.Count > 0,
            "Expected multi-vector data in either VectorForTargets or Vectors"
        );
    }

    #endregion

    #region With GroupBy

    [Fact]
    public async Task NearVector_WithGroupBy_ProducesValidRequest()
    {
        // Arrange - Example from docs:
        // await collection.Query.NearVector(
        //     new[] { 1f, 2f, 3f },
        //     new GroupByRequest("category") { ObjectsPerGroup = 3 }
        // );

        // Act
        await _collection.Query.NearVector(
            new[] { 1f, 2f, 3f },
            new GroupByRequest("category") { ObjectsPerGroup = 3 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.NotNull(request.GroupBy);
        Assert.Equal("category", request.GroupBy.Path[0]);
        Assert.Equal(3, request.GroupBy.ObjectsPerGroup);
    }

    #endregion

    #region NearVectorInput.FactoryFn Lambda Builder

    [Fact]
    public async Task NearVector_NearVectorInputFactoryFn_WithCertainty_ProducesValidRequest()
    {
        // Arrange - Test NearVectorInput.FactoryFn with certainty parameter
        // await collection.Query.NearVector(
        //     v => v(certainty: 0.8).ManualWeights(
        //         ("title", 1.2, new[] { 1f, 2f }),
        //         ("description", 0.8, new[] { 3f, 4f })
        //     )
        // );

        // Act
        await _collection.Query.NearVector(
            v =>
                v(certainty: 0.8f)
                    .ManualWeights(
                        ("title", 1.2, new[] { 1f, 2f }),
                        ("description", 0.8, new[] { 3f, 4f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.Equal(0.8, request.NearVector.Certainty, precision: 5);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.NearVector.Targets.Combination);
    }

    [Fact]
    public async Task NearVector_NearVectorInputFactoryFn_WithDistance_ProducesValidRequest()
    {
        // Arrange - Test NearVectorInput.FactoryFn with distance parameter

        // Act
        await _collection.Query.NearVector(
            v =>
                v(distance: 0.5f)
                    .Sum(("title", new[] { 1f, 2f }), ("description", new[] { 3f, 4f })),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.Equal(0.5, request.NearVector.Distance, precision: 5);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.NearVector.Targets.Combination);
    }

    [Fact]
    public async Task NearVector_NearVectorInputFactoryFn_Average_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearVector(
            v => v().Average(("title", new[] { 1f, 2f }), ("description", new[] { 3f, 4f })),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.NearVector.Targets.Combination);
    }

    [Fact]
    public async Task NearVector_NearVectorInputFactoryFn_Minimum_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearVector(
            v => v().Minimum(("title", new[] { 1f, 2f }), ("description", new[] { 3f, 4f })),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.NearVector.Targets.Combination);
    }

    [Fact]
    public async Task NearVector_NearVectorInputFactoryFn_RelativeScore_ProducesValidRequest()
    {
        // Act
        await _collection.Query.NearVector(
            v =>
                v()
                    .RelativeScore(
                        ("title", 0.7, new[] { 1f, 2f }),
                        ("description", 0.3, new[] { 3f, 4f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.Equal(
            V1.CombinationMethod.TypeRelativeScore,
            request.NearVector.Targets.Combination
        );
        Assert.Equal(2, request.NearVector.Targets.WeightsForTargets.Count);
    }

    #endregion
}

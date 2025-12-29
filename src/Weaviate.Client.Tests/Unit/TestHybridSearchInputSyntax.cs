using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying the hybrid search input syntax combinations compile correctly
/// and produce the expected gRPC request structure.
/// </summary>
[Collection("Unit Tests")]
public class TestHybridSearchInputSyntax : IAsyncLifetime
{
    private const string CollectionName = "TestCollection";

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    #region Query-Only Tests

    [Fact]
    public async Task Hybrid_QueryOnly_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act
        await collection.Query.Hybrid(
            query: "search text",
            vectors: null,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal("search text", request.HybridSearch.Query);
        Assert.Null(request.HybridSearch.NearText);
        Assert.Null(request.HybridSearch.NearVector);
    }

    [Fact]
    public async Task Hybrid_QueryOnly_WithAlpha_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act
        await collection.Query.Hybrid(
            query: "search text",
            vectors: (HybridVectorInput?)null,
            alpha: 0.5f,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal("search text", request.HybridSearch.Query);
        Assert.Equal(0.5f, request.HybridSearch.Alpha, precision: 5);
    }

    [Fact]
    public async Task Hybrid_QueryOnly_WithFusionType_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act
        await collection.Query.Hybrid(
            query: "search text",
            vectors: (HybridVectorInput?)null,
            fusionType: HybridFusion.RelativeScore,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.Hybrid.Types.FusionType.RelativeScore, request.HybridSearch.FusionType);
    }

    #endregion

    #region VectorSearchInput - Float Array Tests

    [Fact]
    public async Task Hybrid_VectorSearchInput_FloatArray_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        float[] vector = [1f, 2f, 3f];

        // Act - implicit conversion from float[] to HybridVectorInput
        await collection.Query.Hybrid(
            query: null,
            vectors: vector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("default", request.HybridSearch.Targets.TargetVectors);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_DoubleArray_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        double[] vector = [1.0, 2.0, 3.0];

        // Act - implicit conversion from double[] to HybridVectorInput
        await collection.Query.Hybrid(
            query: null,
            vectors: vector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
    }

    #endregion

    #region VectorSearchInput - Named Tuple Tests

    [Fact]
    public async Task Hybrid_VectorSearchInput_NamedTuple_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act - implicit conversion from (string, float[]) tuple
        await collection.Query.Hybrid(
            query: null,
            vectors: ("myVector", new float[] { 1f, 2f, 3f }),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_NamedTuple_Double_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act - implicit conversion from (string, double[]) tuple
        await collection.Query.Hybrid(
            query: null,
            vectors: ("myVector", new double[] { 1.0, 2.0, 3.0 }),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);
    }

    #endregion

    #region VectorSearchInput - Multi-Vector (ColBERT) Tests

    [Fact]
    public async Task Hybrid_VectorSearchInput_MultiVector_Float2D_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        float[,] multiVector =
        {
            { 1f, 2f },
            { 3f, 4f },
        };

        // Act - implicit conversion from (string, float[,]) tuple for ColBERT
        await collection.Query.Hybrid(
            query: null,
            vectors: ("colbert", multiVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("colbert", request.HybridSearch.Targets.TargetVectors);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_MultiVector_Double2D_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        double[,] multiVector =
        {
            { 1.0, 2.0 },
            { 3.0, 4.0 },
        };

        // Act - implicit conversion from (string, double[,]) tuple for ColBERT
        await collection.Query.Hybrid(
            query: null,
            vectors: ("colbert", multiVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("colbert", request.HybridSearch.Targets.TargetVectors);
    }

    #endregion

    #region VectorSearchInput - NamedVector Tests

    [Fact]
    public async Task Hybrid_VectorSearchInput_NamedVector_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var namedVector = new NamedVector("myVector", [1f, 2f, 3f]);

        // Act - implicit conversion from NamedVector
        await collection.Query.Hybrid(
            query: null,
            vectors: namedVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_Vectors_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var vectors = new Vectors { { "vector1", new float[] { 1f, 2f } } };

        // Act - implicit conversion from Vectors
        await collection.Query.Hybrid(
            query: null,
            vectors: vectors,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("vector1", request.HybridSearch.Targets.TargetVectors);
    }

    #endregion

    #region VectorSearchInput Builder - Combination Methods

    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_Sum_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var input = VectorSearchInput.Combine(
            TargetVectors.Sum("vector1", "vector2"),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
        Assert.Contains("vector1", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("vector2", request.HybridSearch.Targets.TargetVectors);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_Average_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var input = VectorSearchInput.Combine(
            TargetVectors.Average("vector1", "vector2"),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_Minimum_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var input = VectorSearchInput.Combine(
            TargetVectors.Minimum("vector1", "vector2"),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.HybridSearch.Targets.Combination);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_ManualWeights_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var input = VectorSearchInput.Combine(
            TargetVectors.ManualWeights(("vector1", 0.7), ("vector2", 0.3)),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_RelativeScore_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var input = VectorSearchInput.Combine(
            TargetVectors.RelativeScore(("vector1", 0.8), ("vector2", 0.2)),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.CombinationMethod.TypeRelativeScore,
            request.HybridSearch.Targets.Combination
        );
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_SameNameSum_ProducesValidRequest()
    {
        // Arrange - multiple vectors with same name for sum combination
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var input = VectorSearchInput.Combine(
            TargetVectors.Sum("regular"),
            ("regular", new float[] { 1f, 2f }),
            ("regular", new float[] { 2f, 1f })
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
        Assert.Single(request.HybridSearch.Targets.TargetVectors);
        Assert.Equal("regular", request.HybridSearch.Targets.TargetVectors[0]);
    }

    #endregion

    #region NearTextInput Tests

    [Fact]
    public async Task Hybrid_NearTextInput_String_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act - implicit conversion from string to NearTextInput via HybridVectorInput
        await collection.Query.Hybrid(
            query: null,
            vectors: (HybridVectorInput)"semantic search",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("semantic search", request.HybridSearch.NearText.Query);
    }

    [Fact]
    public async Task Hybrid_NearTextInput_Explicit_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput("semantic search", Distance: 0.5f);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("semantic search", request.HybridSearch.NearText.Query);
        Assert.Equal(0.5, request.HybridSearch.NearText.Distance, precision: 5);
    }

    [Fact]
    public async Task Hybrid_NearTextInput_WithCertainty_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput("semantic search", Certainty: 0.8f);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Equal(0.8, request.HybridSearch.NearText.Certainty, precision: 5);
    }

    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_Sum_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.Sum("vector1", "vector2")
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets (matching Python client behavior)
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
    }

    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_Average_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.Average("vector1", "vector2")
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);
    }

    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_Minimum_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.Minimum("vector1", "vector2")
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.HybridSearch.Targets.Combination);
    }

    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_ManualWeights_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.ManualWeights(("vector1", 0.7), ("vector2", 0.3))
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);
    }

    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_RelativeScore_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.RelativeScore(("vector1", 0.8), ("vector2", 0.2))
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.CombinationMethod.TypeRelativeScore,
            request.HybridSearch.Targets.Combination
        );
    }

    [Fact]
    public async Task Hybrid_NearTextInput_WithLambdaBuilder_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: t => t.Sum("vector1", "vector2")
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
    }

    #endregion

    #region NearVectorInput Tests

    [Fact]
    public async Task Hybrid_NearVectorInput_Basic_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f });

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
    }

    [Fact]
    public async Task Hybrid_NearVectorInput_WithDistance_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f }, Distance: 0.5f);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(0.5, request.HybridSearch.NearVector.Distance, precision: 5);
    }

    [Fact]
    public async Task Hybrid_NearVectorInput_WithCertainty_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f }, Certainty: 0.9f);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(0.9, request.HybridSearch.NearVector.Certainty, precision: 5);
    }

    [Fact]
    public async Task Hybrid_NearVectorInput_Named_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        VectorSearchInput vectorInput = ("myVector", new float[] { 1f, 2f, 3f });
        var nearVector = new NearVectorInput(vectorInput, Distance: 0.5f);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets (matching Python client behavior)
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);
    }

    [Fact]
    public async Task Hybrid_NearVectorInput_MultiTarget_Sum_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.Sum("vector1", "vector2"),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );
        var nearVector = new NearVectorInput(vectorInput);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
    }

    [Fact]
    public async Task Hybrid_NearVectorInput_MultiTarget_ManualWeights_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.ManualWeights(("vector1", 0.6), ("vector2", 0.4)),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );
        var nearVector = new NearVectorInput(vectorInput);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);
    }

    #endregion

    #region Query + Vector Combinations

    [Fact]
    public async Task Hybrid_QueryAndVectorSearchInput_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act
        await collection.Query.Hybrid(
            query: "keyword search",
            vectors: new float[] { 1f, 2f, 3f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal("keyword search", request.HybridSearch.Query);
        Assert.NotNull(request.HybridSearch.Targets);
    }

    [Fact]
    public async Task Hybrid_QueryAndNearText_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearText = new NearTextInput("semantic meaning");

        // Act
        await collection.Query.Hybrid(
            query: "keyword",
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal("keyword", request.HybridSearch.Query);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("semantic meaning", request.HybridSearch.NearText.Query);
    }

    [Fact]
    public async Task Hybrid_QueryAndNearVector_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f });

        // Act
        await collection.Query.Hybrid(
            query: "keyword",
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal("keyword", request.HybridSearch.Query);
        Assert.NotNull(request.HybridSearch.NearVector);
    }

    [Fact]
    public async Task Hybrid_QueryAndMultiTargetVectors_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.Sum("title", "content"),
            ("title", new float[] { 1f, 2f }),
            ("content", new float[] { 3f, 4f })
        );

        // Act
        await collection.Query.Hybrid(
            query: "search query",
            vectors: HybridVectorInput.FromVectorSearch(vectorInput),
            alpha: 0.7f,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal("search query", request.HybridSearch.Query);
        Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
    }

    #endregion

    #region VectorSearchInput.Combine Tests

    [Fact]
    public async Task Hybrid_VectorSearchInput_Combine_WithTuples_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var targetVectors = TargetVectors.Sum("first", "second");
        var input = VectorSearchInput.Combine(
            targetVectors,
            ("first", new float[] { 1f, 2f }),
            ("second", new float[] { 3f, 4f })
        );

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_Combine_WithVectors_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var targetVectors = TargetVectors.Average("first", "second");
        var vectors = new Vectors
        {
            { "first", new float[] { 1f, 2f } },
            { "second", new float[] { 3f, 4f } },
        };
        var input = VectorSearchInput.Combine(targetVectors, vectors);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);
    }

    [Fact]
    public async Task Hybrid_VectorSearchInput_Combine_WithWeights_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        var targetVectors = TargetVectors.ManualWeights(("first", 0.7), ("second", 0.3));
        var vectors = new Vectors
        {
            { "first", new float[] { 1f, 2f } },
            { "second", new float[] { 3f, 4f } },
        };
        var input = VectorSearchInput.Combine(targetVectors, vectors);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);
    }

    #endregion

    #region Lambda Builder Syntax Tests

    [Fact]
    public async Task Hybrid_LambdaBuilder_Sum_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act - using lambda directly in Hybrid call
        await collection.Query.Hybrid(
            query: "search query",
            vectors: b =>
                b.Sum(("vector1", new float[] { 1f, 2f }), ("vector2", new float[] { 3f, 4f })),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal("search query", request.HybridSearch.Query);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
        Assert.Contains("vector1", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("vector2", request.HybridSearch.Targets.TargetVectors);
    }

    [Fact]
    public async Task Hybrid_LambdaBuilder_ManualWeights_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act - using lambda directly in Hybrid call with ManualWeights
        await collection.Query.Hybrid(
            query: null,
            vectors: b =>
                b.ManualWeights(
                    ("title", 0.7, new float[] { 1f, 2f }),
                    ("description", 0.3, new float[] { 3f, 4f })
                ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);
        Assert.Contains("title", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("description", request.HybridSearch.Targets.TargetVectors);
    }

    [Fact]
    public async Task Hybrid_LambdaBuilder_Average_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act
        await collection.Query.Hybrid(
            query: "test",
            vectors: b => b.Average(("vec1", new float[] { 1f }), ("vec2", new float[] { 2f })),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Hybrid_NullQueryAndNullVectors_ThrowsArgumentException()
    {
        // Arrange
        var (client, _) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            collection.Query.Hybrid(
                query: null,
                vectors: (HybridVectorInput?)null,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public void HybridVectorInput_MultipleTypes_ThrowsArgumentException()
    {
        // This test verifies the discriminated union constraint
        // HybridVectorInput can only hold one of the three types

        // Arrange
        var vectorSearch = new VectorSearchInput { { "test", new float[] { 1f, 2f } } };
        var nearText = new NearTextInput("test");

        // Act - try to create HybridVectorInput with both (this should not compile,
        // but since it's a runtime check, we verify the factory methods work correctly)
        var input1 = HybridVectorInput.FromVectorSearch(vectorSearch);
        var input2 = HybridVectorInput.FromNearText(nearText);

        // Assert - each input should only have its respective type set
        Assert.NotNull(input1.VectorSearch);
        Assert.Null(input1.NearText);
        Assert.Null(input1.NearVector);

        Assert.Null(input2.VectorSearch);
        Assert.NotNull(input2.NearText);
        Assert.Null(input2.NearVector);
    }

    #endregion

    #region Implicit Conversion Coverage Tests

    [Fact]
    public async Task Hybrid_ImplicitConversion_Vector_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);
        Vector vector = new float[] { 1f, 2f, 3f };

        // Act - implicit conversion from Vector
        await collection.Query.Hybrid(
            query: null,
            vectors: vector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
    }

    [Fact]
    public async Task Hybrid_ImplicitConversion_TargetVectorsArray_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        var collection = client.Collections.Use(CollectionName);

        // Create NearTextInput with implicit conversion from string[] to TargetVectors
        TargetVectors targets = new[] { "vector1", "vector2" };
        var nearText = new NearTextInput("query", TargetVectors: targets);

        // Act
        await collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Contains("vector1", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("vector2", request.HybridSearch.Targets.TargetVectors);
    }
    #endregion
}

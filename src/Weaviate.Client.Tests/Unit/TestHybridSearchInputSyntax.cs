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
    private const string ServerVersionEnvVar = "WEAVIATE_VERSION";
    private const string DefaultServerVersion = "1.36.0";

    private Func<V1.SearchRequest?> _getRequest = null!;
    private CollectionClient _collection = null!;

    public ValueTask InitializeAsync()
    {
        var versionString =
            Environment.GetEnvironmentVariable(ServerVersionEnvVar) ?? DefaultServerVersion;
        var version = Version.Parse(versionString);
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture(version);
        _getRequest = getRequest;
        _collection = client.Collections.Use(CollectionName);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    #region Query-Only Tests

    /// <summary>
    /// Tests that hybrid query only produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_QueryOnly_ProducesValidRequest()
    {
        // Act
        await _collection.Query.Hybrid(
            query: "search text",
            vectors: null,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(CollectionName, request.Collection);
        Assert.Equal("search text", request.HybridSearch.Query);
        Assert.Null(request.HybridSearch.NearText);
        Assert.Null(request.HybridSearch.NearVector);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid query only with alpha produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_QueryOnly_WithAlpha_ProducesValidRequest()
    {
        // Act
        await _collection.Query.Hybrid(
            query: "search text",
            vectors: (HybridVectorInput?)null,
            alpha: 0.5f,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("search text", request.HybridSearch.Query);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.5f, request.HybridSearch.Alpha, precision: 5); // Explicit alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(0.5f, request.HybridSearch.AlphaParam, precision: 5);
        }
    }

    /// <summary>
    /// Tests that hybrid query only with fusion type produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_QueryOnly_WithFusionType_ProducesValidRequest()
    {
        // Act
        await _collection.Query.Hybrid(
            query: "search text",
            vectors: (HybridVectorInput?)null,
            fusionType: HybridFusion.RelativeScore,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.Hybrid.Types.FusionType.RelativeScore, request.HybridSearch.FusionType);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    #endregion

    #region VectorSearchInput - Float Array Tests

    /// <summary>
    /// Tests that hybrid vector search input float array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_FloatArray_ProducesValidRequest()
    {
        // Arrange
        float[] vector = [1f, 2f, 3f];

        // Act - implicit conversion from float[] to HybridVectorInput
        await _collection.Query.Hybrid(
            query: null,
            vectors: vector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("default", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input double array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_DoubleArray_ProducesValidRequest()
    {
        // Arrange
        double[] vector = [1.0, 2.0, 3.0];

        // Act - implicit conversion from double[] to HybridVectorInput
        await _collection.Query.Hybrid(
            query: null,
            vectors: vector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region VectorSearchInput - Named Tuple Tests

    /// <summary>
    /// Tests that hybrid vector search input named tuple produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_NamedTuple_ProducesValidRequest()
    {
        // Act - implicit conversion from (string, float[]) tuple
        await _collection.Query.Hybrid(
            query: null,
            vectors: ("myVector", new float[] { 1f, 2f, 3f }),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input named tuple double produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_NamedTuple_Double_ProducesValidRequest()
    {
        // Act - implicit conversion from (string, double[]) tuple
        await _collection.Query.Hybrid(
            query: null,
            vectors: ("myVector", new double[] { 1.0, 2.0, 3.0 }),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region VectorSearchInput - Multi-Vector (ColBERT) Tests

    /// <summary>
    /// Tests that hybrid vector search input multi vector float 2 d produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_MultiVector_Float2D_ProducesValidRequest()
    {
        // Arrange
        float[,] multiVector =
        {
            { 1f, 2f },
            { 3f, 4f },
        };

        // Act - implicit conversion from (string, float[,]) tuple for ColBERT
        await _collection.Query.Hybrid(
            query: null,
            vectors: ("colbert", multiVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("colbert", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input multi vector double 2 d produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_MultiVector_Double2D_ProducesValidRequest()
    {
        // Arrange
        double[,] multiVector =
        {
            { 1.0, 2.0 },
            { 3.0, 4.0 },
        };

        // Act - implicit conversion from (string, double[,]) tuple for ColBERT
        await _collection.Query.Hybrid(
            query: null,
            vectors: ("colbert", multiVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("colbert", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region VectorSearchInput - NamedVector Tests

    /// <summary>
    /// Tests that hybrid vector search input named vector produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_NamedVector_ProducesValidRequest()
    {
        // Arrange
        var namedVector = new NamedVector("myVector", [1f, 2f, 3f]);

        // Act - implicit conversion from NamedVector
        await _collection.Query.Hybrid(
            query: null,
            vectors: namedVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input vectors produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Vectors_ProducesValidRequest()
    {
        // Arrange
        var vectors = new Vectors { { "vector1", new float[] { 1f, 2f } } };

        // Act - implicit conversion from Vectors
        await _collection.Query.Hybrid(
            query: null,
            vectors: vectors,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("vector1", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region VectorSearchInput Builder - Combination Methods

    /// <summary>
    /// Tests that hybrid vector search input builder sum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_Sum_ProducesValidRequest()
    {
        // Arrange
        var input = VectorSearchInput.Combine(
            TargetVectors.Sum("vector1", "vector2"),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
        Assert.Contains("vector1", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("vector2", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input builder average produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_Average_ProducesValidRequest()
    {
        // Arrange
        var input = VectorSearchInput.Combine(
            TargetVectors.Average("vector1", "vector2"),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input builder minimum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_Minimum_ProducesValidRequest()
    {
        // Arrange
        var input = VectorSearchInput.Combine(
            TargetVectors.Minimum("vector1", "vector2"),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input builder manual weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_ManualWeights_ProducesValidRequest()
    {
        // Arrange
        var input = VectorSearchInput.Combine(
            TargetVectors.ManualWeights(("vector1", 0.7), ("vector2", 0.3)),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input builder relative score produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_RelativeScore_ProducesValidRequest()
    {
        // Arrange
        var input = VectorSearchInput.Combine(
            TargetVectors.RelativeScore(("vector1", 0.8), ("vector2", 0.2)),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.CombinationMethod.TypeRelativeScore,
            request.HybridSearch.Targets.Combination
        );

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input builder same name sum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Builder_SameNameSum_ProducesValidRequest()
    {
        // Arrange - multiple vectors with same name for sum combination
        var input = VectorSearchInput.Combine(
            TargetVectors.Sum("regular"),
            ("regular", new float[] { 1f, 2f }),
            ("regular", new float[] { 2f, 1f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
        Assert.Single(request.HybridSearch.Targets.TargetVectors);
        Assert.Equal("regular", request.HybridSearch.Targets.TargetVectors[0]);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region NearTextInput Tests

    /// <summary>
    /// Tests that hybrid near text input string produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_String_ProducesValidRequest()
    {
        // Act - implicit conversion from string to NearTextInput via HybridVectorInput
        await _collection.Query.Hybrid(
            query: null,
            vectors: (HybridVectorInput)"semantic search",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("semantic search", request.HybridSearch.NearText.Query);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input explicit produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_Explicit_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput("semantic search", Distance: 0.5f);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("semantic search", request.HybridSearch.NearText.Query);
        Assert.Equal(0.5, request.HybridSearch.NearText.Distance, precision: 5);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input with certainty produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_WithCertainty_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput("semantic search", Certainty: 0.8f);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Equal(0.8, request.HybridSearch.NearText.Certainty, precision: 5);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input with target vectors sum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_Sum_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.Sum("vector1", "vector2")
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets (matching Python client behavior)
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input with target vectors average produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_Average_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.Average("vector1", "vector2")
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input with target vectors minimum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_Minimum_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.Minimum("vector1", "vector2")
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input with target vectors manual weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_ManualWeights_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.ManualWeights(("vector1", 0.7), ("vector2", 0.3))
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input with target vectors relative score produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_WithTargetVectors_RelativeScore_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: TargetVectors.RelativeScore(("vector1", 0.8), ("vector2", 0.2))
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.CombinationMethod.TypeRelativeScore,
            request.HybridSearch.Targets.Combination
        );

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input with lambda builder produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_WithLambdaBuilder_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput(
            "semantic search",
            TargetVectors: t => t.Sum("vector1", "vector2")
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near text input simple string from docs produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextInput_SimpleString_FromDocs_ProducesValidRequest()
    {
        // Validates corrected docs example at line 449-451

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: new NearTextInput("banana"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.True(string.IsNullOrEmpty(request.HybridSearch.Query)); // Verify query is null or empty
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("banana", request.HybridSearch.NearText.Query);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region NearVectorInput Tests

    /// <summary>
    /// Tests that hybrid near vector input basic produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_Basic_ProducesValidRequest()
    {
        // Arrange
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f });

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input with distance produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_WithDistance_ProducesValidRequest()
    {
        // Arrange
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f }, Distance: 0.5f);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(0.5, request.HybridSearch.NearVector.Distance, precision: 5);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input with certainty produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_WithCertainty_ProducesValidRequest()
    {
        // Arrange
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f }, Certainty: 0.9f);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(0.9, request.HybridSearch.NearVector.Certainty, precision: 5);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input named produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_Named_ProducesValidRequest()
    {
        // Arrange
        VectorSearchInput vectorInput = ("myVector", new float[] { 1f, 2f, 3f });
        var nearVector = new NearVectorInput(vectorInput, Distance: 0.5f);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets (matching Python client behavior)
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input multi target sum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_MultiTarget_Sum_ProducesValidRequest()
    {
        // Arrange
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.Sum("vector1", "vector2"),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );
        var nearVector = new NearVectorInput(vectorInput);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input multi target manual weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_MultiTarget_ManualWeights_ProducesValidRequest()
    {
        // Arrange
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.ManualWeights(("vector1", 0.6), ("vector2", 0.4)),
            ("vector1", new float[] { 1f, 2f }),
            ("vector2", new float[] { 3f, 4f })
        );
        var nearVector = new NearVectorInput(vectorInput);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input with certainty from docs produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_WithCertainty_FromDocs_ProducesValidRequest()
    {
        // Validates new docs example for NearVectorInput in Hybrid

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: new NearVectorInput(new[] { 1f, 2f, 3f }, Certainty: 0.8f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.True(string.IsNullOrEmpty(request.HybridSearch.Query)); // Verify query is null or empty
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(0.8f, request.HybridSearch.NearVector.Certainty, precision: 5);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input multi target from docs produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_MultiTarget_FromDocs_ProducesValidRequest()
    {
        // Validates new docs example for NearVectorInput with multi-target
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.ManualWeights(("title", 0.7), ("description", 0.3)),
            ("title", new[] { 1f, 2f }),
            ("description", new[] { 3f, 4f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: "search query",
            vectors: new NearVectorInput(vectorInput, Distance: 0.5f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("search query", request.HybridSearch.Query);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(0.5f, request.HybridSearch.NearVector.Distance, precision: 5);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    #endregion

    #region Query + Vector Combinations

    /// <summary>
    /// Tests that hybrid query and vector search input produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_QueryAndVectorSearchInput_ProducesValidRequest()
    {
        // Act
        await _collection.Query.Hybrid(
            query: "keyword search",
            vectors: new float[] { 1f, 2f, 3f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("keyword search", request.HybridSearch.Query);
        Assert.NotNull(request.HybridSearch.Targets);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid query and near text produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_QueryAndNearText_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput("semantic meaning");

        // Act
        await _collection.Query.Hybrid(
            query: "keyword",
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("keyword", request.HybridSearch.Query);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("semantic meaning", request.HybridSearch.NearText.Query);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid query and near vector produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_QueryAndNearVector_ProducesValidRequest()
    {
        // Arrange
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f });

        // Act
        await _collection.Query.Hybrid(
            query: "keyword",
            vectors: HybridVectorInput.FromNearVector(nearVector),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("keyword", request.HybridSearch.Query);
        Assert.NotNull(request.HybridSearch.NearVector);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid query and multi target vectors produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_QueryAndMultiTargetVectors_ProducesValidRequest()
    {
        // Arrange
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.Sum("title", "content"),
            ("title", new float[] { 1f, 2f }),
            ("content", new float[] { 3f, 4f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: "search query",
            vectors: HybridVectorInput.FromVectorSearch(vectorInput),
            alpha: 0.7f,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("search query", request.HybridSearch.Query);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Explicit alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(0.7f, request.HybridSearch.AlphaParam, precision: 5);
        }

        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
    }

    #endregion

    #region VectorSearchInput.Combine Tests

    /// <summary>
    /// Tests that hybrid vector search input combine with tuples produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Combine_WithTuples_ProducesValidRequest()
    {
        // Arrange
        var targetVectors = TargetVectors.Sum("first", "second");
        var input = VectorSearchInput.Combine(
            targetVectors,
            ("first", new float[] { 1f, 2f }),
            ("second", new float[] { 3f, 4f })
        );

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input combine with vectors produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Combine_WithVectors_ProducesValidRequest()
    {
        // Arrange
        var targetVectors = TargetVectors.Average("first", "second");
        var vectors = new Vectors
        {
            { "first", new float[] { 1f, 2f } },
            { "second", new float[] { 3f, 4f } },
        };
        var input = VectorSearchInput.Combine(targetVectors, vectors);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid vector search input combine with weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_VectorSearchInput_Combine_WithWeights_ProducesValidRequest()
    {
        // Arrange
        var targetVectors = TargetVectors.ManualWeights(("first", 0.7), ("second", 0.3));
        var vectors = new Vectors
        {
            { "first", new float[] { 1f, 2f } },
            { "second", new float[] { 3f, 4f } },
        };
        var input = VectorSearchInput.Combine(targetVectors, vectors);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromVectorSearch(input),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region Lambda Builder Syntax Tests

    /// <summary>
    /// Tests that hybrid lambda builder sum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_LambdaBuilder_Sum_ProducesValidRequest()
    {
        // Act - using lambda directly in Hybrid call
        await _collection.Query.Hybrid(
            query: "search query",
            vectors: b =>
                b.NearVector()
                    .TargetVectorsSum(
                        ("vector1", new float[] { 1f, 2f }),
                        ("vector2", new float[] { 3f, 4f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("search query", request.HybridSearch.Query);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
        Assert.Contains("vector1", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("vector2", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid lambda builder manual weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_LambdaBuilder_ManualWeights_ProducesValidRequest()
    {
        // Act - using lambda directly in Hybrid call with ManualWeights
        await _collection.Query.Hybrid(
            query: null,
            vectors: b =>
                b.NearVector()
                    .TargetVectorsManualWeights(
                        ("title", 0.7, new float[] { 1f, 2f }),
                        ("description", 0.3, new float[] { 3f, 4f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);
        Assert.Contains("title", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("description", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid lambda builder average produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_LambdaBuilder_Average_ProducesValidRequest()
    {
        // Act
        await _collection.Query.Hybrid(
            query: "test",
            vectors: b =>
                b.NearVector()
                    .TargetVectorsAverage(
                        ("vec1", new float[] { 1f }),
                        ("vec2", new float[] { 2f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    #endregion

    #region Validation Tests

    /// <summary>
    /// Tests that hybrid null query and null vectors throws argument exception
    /// </summary>
    [Fact]
    public async Task Hybrid_NullQueryAndNullVectors_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _collection.Query.Hybrid(
                query: null,
                vectors: (HybridVectorInput?)null,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    /// <summary>
    /// Tests that hybrid vector input multiple types throws argument exception
    /// </summary>
    [Fact]
    public Task HybridVectorInput_MultipleTypes_ThrowsArgumentException()
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
        return Task.CompletedTask;
    }

    #endregion

    #region Implicit Conversion Coverage Tests

    /// <summary>
    /// Tests that hybrid implicit conversion vector produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_ImplicitConversion_Vector_ProducesValidRequest()
    {
        // Arrange
        Vector vector = new float[] { 1f, 2f, 3f };

        // Act - implicit conversion from Vector
        await _collection.Query.Hybrid(
            query: null,
            vectors: vector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid implicit conversion target vectors array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_ImplicitConversion_TargetVectorsArray_ProducesValidRequest()
    {
        // Arrange
        // Create NearTextInput with implicit conversion from string[] to TargetVectors
        TargetVectors targets = new[] { "vector1", "vector2" };
        var nearText = new NearTextInput("query", TargetVectors: targets);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(nearText),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - targets are moved to Hybrid.Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Contains("vector1", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("vector2", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid implicit conversion string to near text produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_ImplicitConversion_String_ToNearText_ProducesValidRequest()
    {
        // Act - implicit conversion from string to HybridVectorInput (via NearText)
        await _collection.Query.Hybrid(
            query: null,
            vectors: "semantic search query",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("semantic search query", request.HybridSearch.NearText.Query);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid implicit conversion named vector array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_ImplicitConversion_NamedVectorArray_ProducesValidRequest()
    {
        // Arrange
        NamedVector[] namedVectors =
        [
            new NamedVector("first", [1f, 2f]),
            new NamedVector("second", [3f, 4f]),
        ];

        // Act - implicit conversion from NamedVector[] to VectorSearchInput to HybridVectorInput
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)namedVectors,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("first", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("second", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid implicit conversion near text input produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_ImplicitConversion_NearTextInput_ProducesValidRequest()
    {
        // Arrange
        var nearText = new NearTextInput("semantic query", Distance: 0.3f);

        // Act - implicit conversion from NearTextInput to HybridVectorInput
        await _collection.Query.Hybrid(
            query: null,
            vectors: nearText,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Equal(0.3, request.HybridSearch.NearText.Distance, precision: 5);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid implicit conversion near vector input produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_ImplicitConversion_NearVectorInput_ProducesValidRequest()
    {
        // Arrange
        var nearVector = new NearVectorInput(new float[] { 1f, 2f, 3f }, Distance: 0.4f);

        // Act - implicit conversion from NearVectorInput to HybridVectorInput
        await _collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(0.4, request.HybridSearch.NearVector.Distance, precision: 5);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid implicit conversion vector search input produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_ImplicitConversion_VectorSearchInput_ProducesValidRequest()
    {
        // Arrange
        var vectorSearch = new VectorSearchInput { { "named", new float[] { 1f, 2f } } };

        // Act - implicit conversion from VectorSearchInput to HybridVectorInput
        await _collection.Query.Hybrid(
            query: null,
            vectors: vectorSearch,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("named", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region VectorSearchInput Dictionary Implicit Conversion Tests

    /// <summary>
    /// Tests that hybrid dictionary string float array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringFloatArray_ProducesValidRequest()
    {
        // Arrange
        Dictionary<string, float[]> dict = new() { ["first"] = [1f, 2f], ["second"] = [3f, 4f] };

        // Act - implicit conversion from Dictionary<string, float[]>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("first", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("second", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid dictionary string double array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringDoubleArray_ProducesValidRequest()
    {
        // Arrange
        Dictionary<string, double[]> dict = new()
        {
            ["first"] = [1.0, 2.0],
            ["second"] = [3.0, 4.0],
        };

        // Act - implicit conversion from Dictionary<string, double[]>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("first", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("second", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid dictionary string vector array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringVectorArray_ProducesValidRequest()
    {
        // Arrange
        Dictionary<string, Vector[]> dict = new()
        {
            ["first"] = [new float[] { 1f, 2f }, new float[] { 5f, 6f }],
            ["second"] = [new float[] { 3f, 4f }],
        };

        // Act - implicit conversion from Dictionary<string, Vector[]>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("first", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("second", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid dictionary string float 2 d produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringFloat2D_ProducesValidRequest()
    {
        // Arrange - 2D arrays for ColBERT multi-vector
        Dictionary<string, float[,]> dict = new()
        {
            ["colbert1"] = new float[,]
            {
                { 1f, 2f },
                { 3f, 4f },
            },
            ["colbert2"] = new float[,]
            {
                { 5f, 6f },
                { 7f, 8f },
            },
        };

        // Act - implicit conversion from Dictionary<string, float[,]>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("colbert1", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("colbert2", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid dictionary string double 2 d produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringDouble2D_ProducesValidRequest()
    {
        // Arrange - 2D arrays for ColBERT multi-vector
        Dictionary<string, double[,]> dict = new()
        {
            ["colbert1"] = new double[,]
            {
                { 1.0, 2.0 },
                { 3.0, 4.0 },
            },
        };

        // Act - implicit conversion from Dictionary<string, double[,]>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("colbert1", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid dictionary string enumerable float array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringEnumerableFloatArray_ProducesValidRequest()
    {
        // Arrange
        Dictionary<string, IEnumerable<float[]>> dict = new()
        {
            ["first"] = new List<float[]> { new[] { 1f, 2f }, new[] { 5f, 6f } },
            ["second"] = new List<float[]> { new[] { 3f, 4f } },
        };

        // Act - implicit conversion from Dictionary<string, IEnumerable<float[]>>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("first", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("second", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid dictionary string enumerable double array produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringEnumerableDoubleArray_ProducesValidRequest()
    {
        // Arrange
        Dictionary<string, IEnumerable<double[]>> dict = new()
        {
            ["first"] = new List<double[]> { new[] { 1.0, 2.0 } },
        };

        // Act - implicit conversion from Dictionary<string, IEnumerable<double[]>>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("first", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid dictionary string enumerable float 2 d produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringEnumerableFloat2D_ProducesValidRequest()
    {
        // Arrange - IEnumerable of 2D arrays for multiple ColBERT vectors
        Dictionary<string, IEnumerable<float[,]>> dict = new()
        {
            ["colbert"] = new List<float[,]>
            {
                new float[,]
                {
                    { 1f, 2f },
                    { 3f, 4f },
                },
                new float[,]
                {
                    { 5f, 6f },
                    { 7f, 8f },
                },
            },
        };

        // Act - implicit conversion from Dictionary<string, IEnumerable<float[,]>>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("colbert", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid dictionary string enumerable double 2 d produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_DictionaryStringEnumerableDouble2D_ProducesValidRequest()
    {
        // Arrange
        Dictionary<string, IEnumerable<double[,]>> dict = new()
        {
            ["colbert"] = new List<double[,]>
            {
                new double[,]
                {
                    { 1.0, 2.0 },
                    { 3.0, 4.0 },
                },
            },
        };

        // Act - implicit conversion from Dictionary<string, IEnumerable<double[,]>>
        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)dict,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("colbert", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region FactoryFn Implicit Conversion Tests

    /// <summary>
    /// Tests that hybrid factory fn sum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_FactoryFn_Sum_ProducesValidRequest()
    {
        // Act - using FactoryFn implicit conversion
        VectorSearchInput.FactoryFn factory = b =>
            b.TargetVectorsSum(
                ("first", new float[] { 1f, 2f }),
                ("second", new float[] { 3f, 4f })
            );

        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)factory,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);
        Assert.Contains("first", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("second", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid factory fn average produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_FactoryFn_Average_ProducesValidRequest()
    {
        // Act - using FactoryFn implicit conversion
        VectorSearchInput.FactoryFn factory = b =>
            b.TargetVectorsAverage(
                ("first", new float[] { 1f, 2f }),
                ("second", new float[] { 3f, 4f })
            );

        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)factory,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid factory fn minimum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_FactoryFn_Minimum_ProducesValidRequest()
    {
        // Act - using FactoryFn implicit conversion
        VectorSearchInput.FactoryFn factory = b =>
            b.TargetVectorsMinimum(
                ("first", new float[] { 1f, 2f }),
                ("second", new float[] { 3f, 4f })
            );

        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)factory,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid factory fn manual weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_FactoryFn_ManualWeights_ProducesValidRequest()
    {
        // Act - using FactoryFn implicit conversion
        VectorSearchInput.FactoryFn factory = b =>
            b.TargetVectorsManualWeights(
                ("first", 0.7, new float[] { 1f, 2f }),
                ("second", 0.3, new float[] { 3f, 4f })
            );

        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)factory,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid factory fn relative score produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_FactoryFn_RelativeScore_ProducesValidRequest()
    {
        // Act - using FactoryFn implicit conversion
        VectorSearchInput.FactoryFn factory = b =>
            b.TargetVectorsRelativeScore(
                ("first", 0.6, new float[] { 1f, 2f }),
                ("second", 0.4, new float[] { 3f, 4f })
            );

        await _collection.Query.Hybrid(
            query: null,
            vectors: (VectorSearchInput)factory,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.CombinationMethod.TypeRelativeScore,
            request.HybridSearch.Targets.Combination
        );

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region TargetVectors Combination Methods Tests (All via implicit conversions)

    /// <summary>
    /// Tests that hybrid target vectors sum via lambda produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_TargetVectors_Sum_ViaLambda_ProducesValidRequest()
    {
        // Act - using lambda builder for TargetVectors
        var nearText = new NearTextInput("query", TargetVectors: t => t.Sum("vec1", "vec2"));

        await _collection.Query.Hybrid(
            query: null,
            vectors: nearText,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid target vectors average via lambda produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_TargetVectors_Average_ViaLambda_ProducesValidRequest()
    {
        // Act
        var nearText = new NearTextInput("query", TargetVectors: t => t.Average("vec1", "vec2"));

        await _collection.Query.Hybrid(
            query: null,
            vectors: nearText,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid target vectors minimum via lambda produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_TargetVectors_Minimum_ViaLambda_ProducesValidRequest()
    {
        // Act
        var nearText = new NearTextInput("query", TargetVectors: t => t.Minimum("vec1", "vec2"));

        await _collection.Query.Hybrid(
            query: null,
            vectors: nearText,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid target vectors manual weights via lambda produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_TargetVectors_ManualWeights_ViaLambda_ProducesValidRequest()
    {
        // Act
        var nearText = new NearTextInput(
            "query",
            TargetVectors: t => t.ManualWeights(("vec1", 0.6), ("vec2", 0.4))
        );

        await _collection.Query.Hybrid(
            query: null,
            vectors: nearText,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid target vectors relative score via lambda produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_TargetVectors_RelativeScore_ViaLambda_ProducesValidRequest()
    {
        // Act
        var nearText = new NearTextInput(
            "query",
            TargetVectors: t => t.RelativeScore(("vec1", 0.8), ("vec2", 0.2))
        );

        await _collection.Query.Hybrid(
            query: null,
            vectors: nearText,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.CombinationMethod.TypeRelativeScore,
            request.HybridSearch.Targets.Combination
        );

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region Collection Initializer Syntax Tests

    /// <summary>
    /// Tests that hybrid collection initializer single vector produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_CollectionInitializer_SingleVector_ProducesValidRequest()
    {
        // Arrange - using collection initializer syntax
        var input = new VectorSearchInput { { "named", new float[] { 1f, 2f, 3f } } };

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: input,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("named", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid collection initializer multiple vectors produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_CollectionInitializer_MultipleVectors_ProducesValidRequest()
    {
        // Arrange - using collection initializer syntax with multiple vectors
        var input = new VectorSearchInput
        {
            { "first", new float[] { 1f, 2f } },
            { "second", new float[] { 3f, 4f } },
            { "third", new float[] { 5f, 6f } },
        };

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: input,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("first", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("second", request.HybridSearch.Targets.TargetVectors);
        Assert.Contains("third", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid collection initializer multiple vectors same name produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_CollectionInitializer_MultipleVectorsSameName_ProducesValidRequest()
    {
        // Arrange - using collection initializer with multiple vectors for same target
        var input = new VectorSearchInput
        {
            { "same", new float[] { 1f, 2f } },
            { "same", new float[] { 3f, 4f } }, // Same name, different vector
        };

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: input,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("same", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid collection initializer with vectors object produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_CollectionInitializer_WithVectorsObject_ProducesValidRequest()
    {
        // Arrange - adding a Vectors object to VectorSearchInput
        var vectors = new Vectors { { "fromVectors", new float[] { 1f, 2f } } };
        var input = new VectorSearchInput { vectors };

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: input,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("fromVectors", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region NearVectorInput with All Combination Methods

    /// <summary>
    /// Tests that hybrid near vector input multi target average produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_MultiTarget_Average_ProducesValidRequest()
    {
        // Arrange
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.Average("vec1", "vec2"),
            ("vec1", new float[] { 1f, 2f }),
            ("vec2", new float[] { 3f, 4f })
        );
        var nearVector = new NearVectorInput(vectorInput);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input multi target minimum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_MultiTarget_Minimum_ProducesValidRequest()
    {
        // Arrange
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.Minimum("vec1", "vec2"),
            ("vec1", new float[] { 1f, 2f }),
            ("vec2", new float[] { 3f, 4f })
        );
        var nearVector = new NearVectorInput(vectorInput);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(V1.CombinationMethod.TypeMin, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    /// <summary>
    /// Tests that hybrid near vector input multi target relative score produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_MultiTarget_RelativeScore_ProducesValidRequest()
    {
        // Arrange
        var vectorInput = VectorSearchInput.Combine(
            TargetVectors.RelativeScore(("vec1", 0.7), ("vec2", 0.3)),
            ("vec1", new float[] { 1f, 2f }),
            ("vec2", new float[] { 3f, 4f })
        );
        var nearVector = new NearVectorInput(vectorInput);

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.CombinationMethod.TypeRelativeScore,
            request.HybridSearch.Targets.Combination
        );
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region NearVectorInput Implicit Conversions

    /// <summary>
    /// Tests that hybrid near vector input from vector search input implicit produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearVectorInput_FromVectorSearchInput_Implicit_ProducesValidRequest()
    {
        // Arrange - implicit conversion from VectorSearchInput to NearVectorInput
        VectorSearchInput vectorSearch = ("myVector", new float[] { 1f, 2f, 3f });
        NearVectorInput nearVector = vectorSearch;

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: nearVector,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion

    #region Extras
    /// <summary>
    /// Tests that hybrid hybrid near text input builder near text no method produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_HybridNearTextInputBuilder_NearText_NoMethod_ProducesValidRequest()
    {
        // Act - using HybridVectorInput.FactoryFn with NearVector().ManualWeights()
        await _collection.Query.Hybrid(
            "test",
            v => v.NearText("query"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Equal(V1.CombinationMethod.Unspecified, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that extras
    /// </summary>
    [Fact]
    public async Task Extras()
    {
        await _collection.Query.NearVector(
            v => v.TargetVectorsSum(("title", new[] { 1f, 2f }), ("description", new[] { 3f, 4f })),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var queryNearVector3 = await _collection.Query.Hybrid(
            query: "fluffy playful",
            vectors: null,
            alpha: 0.7f,
            limit: 5,
            returnProperties: ["name", "breed", "color", "counter"],
            returnMetadata: MetadataOptions.Score | MetadataOptions.Distance,
            cancellationToken: TestContext.Current.CancellationToken
        );

        await _collection.Query.Hybrid(
            "search query",
            v =>
                v.NearVector()
                    .TargetVectorsSum(
                        ("title", new[] { 1f, 2f }),
                        ("description", new[] { 3f, 4f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );
    }
    #endregion

    #region HybridVectorInput.FactoryFn Lambda Builder Tests

    /// <summary>
    /// Tests that hybrid hybrid vector input builder near vector manual weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_HybridVectorInputBuilder_NearVector_ManualWeights_ProducesValidRequest()
    {
        // Act - using HybridVectorInput.FactoryFn with NearVector().ManualWeights()
        await _collection.Query.Hybrid(
            "test",
            v =>
                v.NearVector()
                    .TargetVectorsManualWeights(
                        ("title", 1.2, new[] { 1f, 2f }),
                        ("description", 0.8, new[] { 3f, 4f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(2, request.HybridSearch.NearVector.VectorForTargets.Count);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid hybrid vector input builder near vector sum produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_HybridVectorInputBuilder_NearVector_Sum_ProducesValidRequest()
    {
        // Act - using HybridVectorInput.FactoryFn with NearVector().TargetVectorsSum()
        await _collection.Query.Hybrid(
            "test",
            v =>
                v.NearVector()
                    .TargetVectorsSum(
                        ("title", new[] { 1f, 2f }),
                        ("description", new[] { 3f, 4f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid hybrid vector input builder near vector with manual weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_HybridVectorInputBuilder_NearVector_WithManualWeights_ProducesValidRequest()
    {
        // Act - using HybridVectorInput.FactoryFn with manual weights
        await _collection.Query.Hybrid(
            "test",
            v =>
                v.NearVector()
                    .TargetVectorsManualWeights(
                        ("title", 1.2, new[] { 1f, 2f }),
                        ("description", 0.8, new[] { 3f, 4f })
                    ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearVector);
        // Note: Hybrid NearVector builders do not accept certainty/distance parameters
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid hybrid vector input builder near text manual weights produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_HybridVectorInputBuilder_NearText_ManualWeights_ProducesValidRequest()
    {
        // Act - using HybridVectorInput.FactoryFn with NearText().ManualWeights()
        await _collection.Query.Hybrid(
            "test",
            v =>
                v.NearText(new[] { "concept1", "concept2" })
                    .TargetVectorsManualWeights(("title", 1.2), ("description", 0.8)),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Equal(2, request.HybridSearch.NearText.Query.Count);
        Assert.Equal(V1.CombinationMethod.TypeManual, request.HybridSearch.Targets.Combination);
        Assert.Equal(2, request.HybridSearch.Targets.WeightsForTargets.Count);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid hybrid vector input builder near text average produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_HybridVectorInputBuilder_NearText_Average_ProducesValidRequest()
    {
        // Act - using HybridVectorInput.FactoryFn with NearText().Average()
        await _collection.Query.Hybrid(
            "test",
            v =>
                v.NearText(new[] { "concept1", "concept2" })
                    .TargetVectorsAverage("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Equal(V1.CombinationMethod.TypeAverage, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid hybrid vector input builder near text with certainty and move produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_HybridVectorInputBuilder_NearText_WithCertaintyAndMove_ProducesValidRequest()
    {
        // Act - using HybridVectorInput.FactoryFn with NearText parameters (no certainty/distance for Hybrid)
        await _collection.Query.Hybrid(
            "test",
            v =>
                v.NearText(
                        new[] { "concept1" },
                        moveTo: new Move("positive", 0.5f),
                        moveAway: new Move("negative", 0.3f)
                    )
                    .TargetVectorsSum("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        // Note: Hybrid NearText builders do not accept certainty/distance parameters
        Assert.NotNull(request.HybridSearch.NearText.MoveTo);
        Assert.NotNull(request.HybridSearch.NearText.MoveAway);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid hybrid vector input builder vectors direct syntax produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_HybridVectorInputBuilder_Vectors_DirectSyntax_ProducesValidRequest()
    {
        // Act - using HybridVectorInput.FactoryFn with .Vectors() direct syntax (no combination method)
        await _collection.Query.Hybrid(
            "test",
            v => v.Vectors(("myVector", new[] { 1f, 2f })),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - .Vectors() creates a VectorSearchInput which should populate Targets
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("test", request.HybridSearch.Query);
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Contains("myVector", request.HybridSearch.Targets.TargetVectors);
        // Combination should be Unspecified when using .Vectors() without combination method
        Assert.Equal(V1.CombinationMethod.Unspecified, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(0.7f, request.HybridSearch.Alpha, precision: 5); // Default alpha
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.False(request.HybridSearch.HasAlphaParam);
        }
    }

    /// <summary>
    /// Tests that hybrid near text builder from docs produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_NearTextBuilder_FromDocs_ProducesValidRequest()
    {
        // Validates corrected docs example at line 462-467

        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: v => v.NearText(["banana"]).TargetVectorsSum("title", "description"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.True(string.IsNullOrEmpty(request.HybridSearch.Query)); // Verify query is null or empty
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.Contains("banana", request.HybridSearch.NearText.Query);
        // Note: Hybrid NearText builders do not accept certainty/distance parameters
        Assert.NotNull(request.HybridSearch.Targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, request.HybridSearch.Targets.Combination);

        // Assert alpha depending on server version
        if (_collection.Client.WeaviateVersion < new Version(1, 35, 0))
        {
            Assert.Equal(1.0f, request.HybridSearch.Alpha, precision: 5); // Pure vector search
        }
        else if (_collection.Client.WeaviateVersion >= new Version(1, 36, 0))
        {
            Assert.True(request.HybridSearch.HasAlphaParam);
            Assert.Equal(1.0f, request.HybridSearch.AlphaParam, precision: 5); // Pure vector search
        }
    }

    #endregion
}

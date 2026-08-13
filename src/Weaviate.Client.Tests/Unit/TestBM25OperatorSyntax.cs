using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using Weaviate.Client.Typed;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying the BM25Operator variants map to the expected
/// SearchOperatorOptions in the gRPC request, for both the BM25 and hybrid sites.
/// </summary>
[Collection("Unit Tests")]
public class TestBM25OperatorSyntax : IAsyncLifetime
{
    private const string CollectionName = "TestCollection";

    private Func<V1.SearchRequest?> _getRequest = null!;
    private CollectionClient _collection = null!;

    /// <summary>
    /// The test document class
    /// </summary>
    private class TestDocument
    {
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initializes this instance
    /// </summary>
    /// <returns>The value task</returns>
    public ValueTask InitializeAsync()
    {
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture(new Version(1, 39, 0));
        _getRequest = getRequest;
        _collection = client.Collections.Use(CollectionName);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    /// <returns>The value task</returns>
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    #region QueryClient.BM25 Tests

    /// <summary>
    /// Tests that bm 25 without operator leaves search operator unset
    /// </summary>
    [Fact]
    public async Task BM25_NoOperator_LeavesSearchOperatorUnset()
    {
        // Act
        await _collection.Query.BM25(
            "banana split",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.Bm25Search.SearchOperator);
    }

    /// <summary>
    /// Tests that bm 25 operator and produces valid request
    /// </summary>
    [Fact]
    public async Task BM25_Operator_And_ProducesValidRequest()
    {
        // Act
        await _collection.Query.BM25(
            "banana split",
            searchOperator: new BM25Operator.And(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.And,
            request.Bm25Search.SearchOperator.Operator
        );
    }

    /// <summary>
    /// Tests that bm 25 operator and cross produces valid request
    /// </summary>
    [Fact]
    public async Task BM25_Operator_AndCross_ProducesValidRequest()
    {
        // Act
        await _collection.Query.BM25(
            "banana split",
            searchOperator: new BM25Operator.AndCross(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.AndCross,
            request.Bm25Search.SearchOperator.Operator
        );
    }

    /// <summary>
    /// Tests that bm 25 operator or produces valid request
    /// </summary>
    [Fact]
    public async Task BM25_Operator_Or_ProducesValidRequest()
    {
        // Act
        await _collection.Query.BM25(
            "banana split",
            searchOperator: new BM25Operator.Or(MinimumMatch: 2),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.Or,
            request.Bm25Search.SearchOperator.Operator
        );
        Assert.Equal(2, request.Bm25Search.SearchOperator.MinimumOrTokensMatch);
    }

    #endregion

    #region GenerateClient.BM25 Tests

    /// <summary>
    /// Tests that generate bm 25 without operator leaves search operator unset
    /// </summary>
    [Fact]
    public async Task Generate_BM25_NoOperator_LeavesSearchOperatorUnset()
    {
        // Act
        await _collection.Generate.BM25(
            "banana split",
            singlePrompt: "Summarize this item",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.Bm25Search.SearchOperator);
    }

    /// <summary>
    /// Tests that generate bm 25 operator and produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_BM25_Operator_And_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.BM25(
            "banana split",
            searchOperator: new BM25Operator.And(),
            singlePrompt: "Summarize this item",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.And,
            request.Bm25Search.SearchOperator.Operator
        );
    }

    /// <summary>
    /// Tests that generate bm 25 operator and cross produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_BM25_Operator_AndCross_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.BM25(
            "banana split",
            searchOperator: new BM25Operator.AndCross(),
            singlePrompt: "Summarize this item",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.AndCross,
            request.Bm25Search.SearchOperator.Operator
        );
    }

    /// <summary>
    /// Tests that generate bm 25 operator or produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_BM25_Operator_Or_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.BM25(
            "banana split",
            searchOperator: new BM25Operator.Or(MinimumMatch: 2),
            singlePrompt: "Summarize this item",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.Or,
            request.Bm25Search.SearchOperator.Operator
        );
        Assert.Equal(2, request.Bm25Search.SearchOperator.MinimumOrTokensMatch);
    }

    /// <summary>
    /// Tests that generate bm 25 with group by operator and cross produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_BM25_GroupBy_Operator_AndCross_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.BM25(
            "banana split",
            new GroupByRequest("category") { NumberOfGroups = 5 },
            searchOperator: new BM25Operator.AndCross(),
            groupedTask: "Summarize by category",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.AndCross,
            request.Bm25Search.SearchOperator.Operator
        );
    }

    #endregion

    #region TypedGenerateClient.BM25 Tests

    /// <summary>
    /// Tests that typed generate bm 25 operator and cross produces valid request
    /// </summary>
    [Fact]
    public async Task Typed_Generate_BM25_Operator_AndCross_ProducesValidRequest()
    {
        // Arrange
        var typedGenerate = new TypedGenerateClient<TestDocument>(_collection.Generate);

        // Act
        await typedGenerate.BM25(
            "banana split",
            searchOperator: new BM25Operator.AndCross(),
            singlePrompt: "Summarize this item",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.AndCross,
            request.Bm25Search.SearchOperator.Operator
        );
    }

    /// <summary>
    /// Tests that typed generate bm 25 with group by operator or produces valid request
    /// </summary>
    [Fact]
    public async Task Typed_Generate_BM25_GroupBy_Operator_Or_ProducesValidRequest()
    {
        // Arrange
        var typedGenerate = new TypedGenerateClient<TestDocument>(_collection.Generate);

        // Act
        await typedGenerate.BM25(
            "banana split",
            new GroupByRequest("category") { NumberOfGroups = 5 },
            searchOperator: new BM25Operator.Or(MinimumMatch: 2),
            groupedTask: "Summarize by category",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.Or,
            request.Bm25Search.SearchOperator.Operator
        );
        Assert.Equal(2, request.Bm25Search.SearchOperator.MinimumOrTokensMatch);
    }

    #endregion

    #region QueryClient.Hybrid Tests

    /// <summary>
    /// Tests that hybrid bm 25 operator and cross produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_BM25Operator_AndCross_ProducesValidRequest()
    {
        // Act
        await _collection.Query.Hybrid(
            query: "banana split",
            vectors: (HybridVectorInput?)null,
            bm25Operator: new BM25Operator.AndCross(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.AndCross,
            request.HybridSearch.Bm25SearchOperator.Operator
        );
    }

    /// <summary>
    /// Tests that hybrid bm 25 operator or produces valid request
    /// </summary>
    [Fact]
    public async Task Hybrid_BM25Operator_Or_ProducesValidRequest()
    {
        // Act
        await _collection.Query.Hybrid(
            query: "banana split",
            vectors: (HybridVectorInput?)null,
            bm25Operator: new BM25Operator.Or(MinimumMatch: 1),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.Or,
            request.HybridSearch.Bm25SearchOperator.Operator
        );
        Assert.Equal(1, request.HybridSearch.Bm25SearchOperator.MinimumOrTokensMatch);
    }

    #endregion

    #region Version Guard Tests

    /// <summary>
    /// Tests that bm 25 operator and cross is rejected on servers predating the backports
    /// </summary>
    [Theory]
    [InlineData("1.36.9")]
    [InlineData("1.37.14")]
    [InlineData("1.38.4")]
    [InlineData("1.38.7")]
    public async Task BM25_Operator_AndCross_UnsupportedVersion_Throws(string version)
    {
        // Arrange
        var (client, _) = MockGrpcClient.CreateWithSearchCapture(Version.Parse(version));
        var collection = client.Collections.Use(CollectionName);

        // Act & Assert
        await Assert.ThrowsAsync<WeaviateFeatureNotSupportedException>(async () =>
            await collection.Query.BM25(
                "banana split",
                searchOperator: new BM25Operator.AndCross(),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    /// <summary>
    /// Tests that bm 25 operator and cross is sent on servers that support it
    /// </summary>
    [Theory]
    [InlineData("1.37.15")]
    [InlineData("1.38.8")]
    [InlineData("1.38.9")]
    [InlineData("1.39.0")]
    [InlineData("1.40.0")]
    public async Task BM25_Operator_AndCross_SupportedVersion_Sends(string version)
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture(Version.Parse(version));
        var collection = client.Collections.Use(CollectionName);

        // Act
        await collection.Query.BM25(
            "banana split",
            searchOperator: new BM25Operator.AndCross(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.AndCross,
            request.Bm25Search.SearchOperator.Operator
        );
    }

    /// <summary>
    /// Tests that generate bm 25 operator and cross is rejected on servers predating the backports
    /// </summary>
    [Theory]
    [InlineData("1.36.9")]
    [InlineData("1.37.14")]
    [InlineData("1.38.4")]
    [InlineData("1.38.7")]
    public async Task Generate_BM25_Operator_AndCross_UnsupportedVersion_Throws(string version)
    {
        // Arrange
        var (client, _) = MockGrpcClient.CreateWithSearchCapture(Version.Parse(version));
        var collection = client.Collections.Use(CollectionName);

        // Act & Assert
        await Assert.ThrowsAsync<WeaviateFeatureNotSupportedException>(async () =>
            await collection.Generate.BM25(
                "banana split",
                searchOperator: new BM25Operator.AndCross(),
                singlePrompt: "Summarize this item",
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    /// <summary>
    /// Tests that hybrid bm 25 operator and cross is rejected on an unsupported server
    /// </summary>
    [Fact]
    public async Task Hybrid_BM25Operator_AndCross_UnsupportedVersion_Throws()
    {
        // Arrange
        var (client, _) = MockGrpcClient.CreateWithSearchCapture(new Version(1, 38, 4));
        var collection = client.Collections.Use(CollectionName);

        // Act & Assert
        await Assert.ThrowsAsync<WeaviateFeatureNotSupportedException>(async () =>
            await collection.Query.Hybrid(
                query: "banana split",
                vectors: (HybridVectorInput?)null,
                bm25Operator: new BM25Operator.AndCross(),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    /// <summary>
    /// Tests that and and or operators are unaffected by the guard on older servers
    /// </summary>
    [Fact]
    public async Task BM25_Operator_AndOr_UnsupportedVersion_StillSend()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture(new Version(1, 38, 4));
        var collection = client.Collections.Use(CollectionName);

        // Act
        await collection.Query.BM25(
            "banana split",
            searchOperator: new BM25Operator.And(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.And,
            request.Bm25Search.SearchOperator.Operator
        );

        // Act
        await collection.Query.Hybrid(
            query: "banana split",
            vectors: (HybridVectorInput?)null,
            bm25Operator: new BM25Operator.Or(MinimumMatch: 2),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.Or,
            request.HybridSearch.Bm25SearchOperator.Operator
        );
    }

    #endregion

    #region AggregateClient.Hybrid Tests

    /// <summary>
    /// Tests that aggregate hybrid bm 25 operator and cross produces valid request
    /// </summary>
    [Fact]
    public async Task Aggregate_Hybrid_BM25Operator_AndCross_ProducesValidRequest()
    {
        // Arrange
        var (client, getRequest) = MockGrpcClient.CreateWithAggregateCapture();
        var collection = client.Collections.Use(CollectionName);

        // Act
        await collection.Aggregate.Hybrid(
            "banana split",
            bm25Operator: new BM25Operator.AndCross(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = getRequest();
        Assert.NotNull(request);
        Assert.Equal(
            V1.SearchOperatorOptions.Types.Operator.AndCross,
            request.Hybrid.Bm25SearchOperator.Operator
        );
    }

    #endregion
}

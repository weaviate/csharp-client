using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using Weaviate.Client.Typed;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying the hybrid diversity selection maps to the expected
/// Selection message in the gRPC request across the query, generate and typed paths.
/// </summary>
[Collection("Unit Tests")]
public class TestHybridDiversitySyntax : IAsyncLifetime
{
    private const string CollectionName = "TestCollection";

    private Func<V1.SearchRequest?> _getRequest = null!;
    private CollectionClient _collection = null!;

    /// <summary>
    /// The article test type
    /// </summary>
    private class Article
    {
        /// <summary>
        /// The title
        /// </summary>
        public string? Title { get; set; }
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

    /// <summary>
    /// Tests that hybrid diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Hybrid_DiversitySelection_MMR_SetsSelection()
    {
        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 7, Balance: 0.5f),
            limit: 7,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Selection);
        Assert.Equal(7u, request.HybridSearch.Selection.Mmr.Limit);
        Assert.Equal(0.5f, request.HybridSearch.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that hybrid diversity selection with omitted balance leaves balance unset
    /// </summary>
    [Fact]
    public async Task Hybrid_DiversitySelection_BalanceOmitted_LeavesBalanceUnset()
    {
        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 3),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Selection);
        Assert.True(request.HybridSearch.Selection.Mmr.HasLimit);
        Assert.False(request.HybridSearch.Selection.Mmr.HasBalance);
    }

    /// <summary>
    /// Tests that hybrid without diversity selection leaves selection unset
    /// </summary>
    [Fact]
    public async Task Hybrid_NoDiversitySelection_LeavesSelectionUnset()
    {
        // Act
        await _collection.Query.Hybrid(
            query: null,
            vectors: new float[] { 1f, 0f, 0f },
            limit: 5,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.HybridSearch.Selection);
    }

    /// <summary>
    /// Tests that generate hybrid diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Generate_Hybrid_DiversitySelection_SetsSelection()
    {
        // Act
        await _collection.Generate.Hybrid(
            query: null,
            vectors: new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 4, Balance: 0.25f),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Selection);
        Assert.Equal(4u, request.HybridSearch.Selection.Mmr.Limit);
        Assert.Equal(0.25f, request.HybridSearch.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that typed hybrid diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_Hybrid_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedQueryClient = new TypedQueryClient<Article>(_collection.Query);

        // Act
        await typedQueryClient.Hybrid(
            query: null,
            vectors: new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 2, Balance: 1.0f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Selection);
        Assert.Equal(2u, request.HybridSearch.Selection.Mmr.Limit);
        Assert.Equal(1.0f, request.HybridSearch.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that typed generate hybrid diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_Generate_Hybrid_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedGenerateClient = new TypedGenerateClient<Article>(_collection.Generate);

        // Act
        await typedGenerateClient.Hybrid(
            query: null,
            vectors: new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 6),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.Selection);
        Assert.Equal(6u, request.HybridSearch.Selection.Mmr.Limit);
        Assert.False(request.HybridSearch.Selection.Mmr.HasBalance);
    }
}

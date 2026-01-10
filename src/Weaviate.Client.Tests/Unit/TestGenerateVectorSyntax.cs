using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying GenerateClient vector search syntax with generative prompts.
/// </summary>
[Collection("Unit Tests")]
public class TestGenerateVectorSyntax : IAsyncLifetime
{
    /// <summary>
    /// The collection name
    /// </summary>
    private const string CollectionName = "TestCollection";

    /// <summary>
    /// The get request
    /// </summary>
    private Func<Grpc.Protobuf.V1.SearchRequest?> _getRequest = null!;

    /// <summary>
    /// The collection
    /// </summary>
    private CollectionClient _collection = null!;

    /// <summary>
    /// Initializes this instance
    /// </summary>
    /// <returns>The value task</returns>
    public ValueTask InitializeAsync()
    {
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
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

    #region GenerateClient.NearVector Tests

    /// <summary>
    /// Tests that generate near vector with single prompt produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_NearVector_WithSinglePrompt_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.NearVector(
            new[] { 1f, 2f, 3f },
            singlePrompt: "Summarize this item",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.NotNull(request.Generative);
        Assert.NotNull(request.Generative.Single);
        Assert.Equal("Summarize this item", request.Generative.Single.Prompt);
    }

    /// <summary>
    /// Tests that generate near vector with grouped task produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_NearVector_WithGroupedTask_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.NearVector(
            new[] { 1f, 2f, 3f },
            groupedTask: new GroupedTask("Summarize all items"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector);
        Assert.NotNull(request.Generative);
        Assert.NotNull(request.Generative.Grouped);
        Assert.Equal("Summarize all items", request.Generative.Grouped.Task);
    }

    /// <summary>
    /// Tests that generate near vector lambda builder with prompts produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_NearVector_LambdaBuilder_WithPrompts_ProducesValidRequest()
    {
        // Act - Lambda builder with Sum combination and generative prompts
        await _collection.Generate.NearVector(
            v => v.TargetVectorsSum(("title", new[] { 1f, 2f }), ("desc", new[] { 3f, 4f })),
            singlePrompt: "Describe this",
            groupedTask: new GroupedTask("Summarize all"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal(CollectionName, request.Collection);
        Assert.NotNull(request.NearVector);
        Assert.NotNull(request.NearVector.Targets);
        Assert.NotNull(request.Generative);

        // Verify Single prompt structure
        Assert.NotNull(request.Generative.Single);
        Assert.Equal("Describe this", request.Generative.Single.Prompt);
        Assert.False(request.Generative.Single.Debug); // Default is false

        // Verify Grouped task structure
        Assert.NotNull(request.Generative.Grouped);
        Assert.Equal("Summarize all", request.Generative.Grouped.Task);
        Assert.False(request.Generative.Grouped.Debug); // Default is false
    }

    #endregion

    #region GenerateClient.NearText Tests

    /// <summary>
    /// Tests that generate near text with single prompt produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_NearText_WithSinglePrompt_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.NearText(
            new NearTextInput("banana"),
            singlePrompt: "Describe this fruit",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.Equal("banana", request.NearText.Query[0]);
        Assert.NotNull(request.Generative);
        Assert.NotNull(request.Generative.Single);
        Assert.Equal("Describe this fruit", request.Generative.Single.Prompt);
    }

    /// <summary>
    /// Tests that generate near text with target vectors and prompts produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_NearText_WithTargetVectors_AndPrompts_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.NearText(
            new NearTextInput("banana", TargetVectors: TargetVectors.Sum("title", "description")),
            singlePrompt: "Explain this",
            groupedTask: new GroupedTask("Compare all"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText);
        Assert.NotNull(request.NearText.Targets);
        Assert.NotNull(request.Generative);
        Assert.NotNull(request.Generative.Single);
        Assert.Equal("Explain this", request.Generative.Single.Prompt);
        Assert.NotNull(request.Generative.Grouped);
        Assert.Equal("Compare all", request.Generative.Grouped.Task);
    }

    #endregion

    #region GenerateClient.Hybrid Tests

    /// <summary>
    /// Tests that generate hybrid text only with prompt produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_Hybrid_TextOnly_WithPrompt_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.Hybrid(
            "search query",
            singlePrompt: "Summarize this result",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("search query", request.HybridSearch.Query);
        Assert.NotNull(request.Generative);
        Assert.NotNull(request.Generative.Single);
        Assert.Equal("Summarize this result", request.Generative.Single.Prompt);
    }

    /// <summary>
    /// Tests that generate hybrid with vectors and prompts produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_Hybrid_WithVectors_AndPrompts_ProducesValidRequest()
    {
        // Act
        await _collection.Generate.Hybrid(
            "search query",
            v =>
                v.NearVector()
                    .TargetVectorsSum(("title", new[] { 1f, 2f }), ("desc", new[] { 3f, 4f })),
            singlePrompt: "Explain",
            groupedTask: new GroupedTask("Compare"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Equal("search query", request.HybridSearch.Query);
        Assert.NotNull(request.Generative);
        Assert.NotNull(request.Generative.Single);
        Assert.Equal("Explain", request.Generative.Single.Prompt);
        Assert.NotNull(request.Generative.Grouped);
        Assert.Equal("Compare", request.Generative.Grouped.Task);
    }

    /// <summary>
    /// Tests that generate hybrid near text input with prompt produces valid request
    /// </summary>
    [Fact]
    public async Task Generate_Hybrid_NearTextInput_WithPrompt_ProducesValidRequest()
    {
        // Act - NearTextInput needs to be wrapped in HybridVectorInput
        await _collection.Generate.Hybrid(
            query: null,
            vectors: HybridVectorInput.FromNearText(
                new NearTextInput("banana", TargetVectors: new[] { "title", "description" })
            ),
            singlePrompt: "Describe this item",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.HybridSearch.NearText);
        Assert.NotNull(request.Generative);
        Assert.NotNull(request.Generative.Single);
        Assert.Equal("Describe this item", request.Generative.Single.Prompt);
    }

    #endregion
}

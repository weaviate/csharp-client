using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using Weaviate.Client.Typed;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying the near-vector, near-object, near-text and near-media diversity
/// selection maps to the expected Selection message in the gRPC request across the query,
/// generate and typed paths.
/// </summary>
[Collection("Unit Tests")]
public class TestNearDiversitySyntax : IAsyncLifetime
{
    private const string CollectionName = "TestCollection";

    private static readonly byte[] TestMediaBytes = [1, 2, 3, 4, 5];

    private static readonly Guid TestObjectID = new("11111111-1111-1111-1111-111111111111");

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

    #region NearVector

    /// <summary>
    /// Tests that near vector diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task NearVector_DiversitySelection_MMR_SetsSelection()
    {
        // Act
        await _collection.Query.NearVector(
            new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 7, Balance: 0.5f),
            limit: 7,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector.Selection);
        Assert.Equal(7u, request.NearVector.Selection.Mmr.Limit);
        Assert.Equal(0.5f, request.NearVector.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that near vector diversity selection with omitted balance leaves balance unset
    /// </summary>
    [Fact]
    public async Task NearVector_DiversitySelection_BalanceOmitted_LeavesBalanceUnset()
    {
        // Act
        await _collection.Query.NearVector(
            new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 3),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector.Selection);
        Assert.True(request.NearVector.Selection.Mmr.HasLimit);
        Assert.Equal(3u, request.NearVector.Selection.Mmr.Limit);
        Assert.False(request.NearVector.Selection.Mmr.HasBalance);
    }

    /// <summary>
    /// Tests that near vector diversity selection with omitted limit leaves limit unset
    /// </summary>
    [Fact]
    public async Task NearVector_DiversitySelection_LimitOmitted_LeavesLimitUnset()
    {
        // Act
        await _collection.Query.NearVector(
            new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Balance: 0.25f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector.Selection);
        Assert.False(request.NearVector.Selection.Mmr.HasLimit);
        Assert.True(request.NearVector.Selection.Mmr.HasBalance);
        Assert.Equal(0.25f, request.NearVector.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that near vector without diversity selection leaves selection unset
    /// </summary>
    [Fact]
    public async Task NearVector_NoDiversitySelection_LeavesSelectionUnset()
    {
        // Act
        await _collection.Query.NearVector(
            new float[] { 1f, 0f, 0f },
            limit: 5,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.NearVector.Selection);
    }

    /// <summary>
    /// Tests that near vector with group by carries the diversity selection
    /// </summary>
    [Fact]
    public async Task NearVector_GroupBy_DiversitySelection_SetsSelection()
    {
        // Act
        await _collection.Query.NearVector(
            new float[] { 1f, 0f, 0f },
            new GroupByRequest("category") { ObjectsPerGroup = 3 },
            diversitySelection: new Diversity.MMR(Limit: 5, Balance: 0.75f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector.Selection);
        Assert.Equal(5u, request.NearVector.Selection.Mmr.Limit);
        Assert.Equal(0.75f, request.NearVector.Selection.Mmr.Balance);
    }

    #endregion

    #region NearObject

    /// <summary>
    /// Tests that near object diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task NearObject_DiversitySelection_MMR_SetsSelection()
    {
        // Act
        await _collection.Query.NearObject(
            TestObjectID,
            diversitySelection: new Diversity.MMR(Limit: 4, Balance: 0.4f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearObject.Selection);
        Assert.Equal(4u, request.NearObject.Selection.Mmr.Limit);
        Assert.Equal(0.4f, request.NearObject.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that near object diversity selection with omitted balance leaves balance unset
    /// </summary>
    [Fact]
    public async Task NearObject_DiversitySelection_BalanceOmitted_LeavesBalanceUnset()
    {
        // Act
        await _collection.Query.NearObject(
            TestObjectID,
            diversitySelection: new Diversity.MMR(Limit: 2),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearObject.Selection);
        Assert.True(request.NearObject.Selection.Mmr.HasLimit);
        Assert.False(request.NearObject.Selection.Mmr.HasBalance);
    }

    /// <summary>
    /// Tests that near object without diversity selection leaves selection unset
    /// </summary>
    [Fact]
    public async Task NearObject_NoDiversitySelection_LeavesSelectionUnset()
    {
        // Act
        await _collection.Query.NearObject(
            TestObjectID,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.NearObject.Selection);
    }

    #endregion

    #region NearText

    /// <summary>
    /// Tests that near text diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task NearText_DiversitySelection_MMR_SetsSelection()
    {
        // Act
        await _collection.Query.NearText(
            "banana",
            diversitySelection: new Diversity.MMR(Limit: 6, Balance: 0.2f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText.Selection);
        Assert.Equal(6u, request.NearText.Selection.Mmr.Limit);
        Assert.Equal(0.2f, request.NearText.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that near text diversity selection with omitted limit leaves limit unset
    /// </summary>
    [Fact]
    public async Task NearText_DiversitySelection_LimitOmitted_LeavesLimitUnset()
    {
        // Act
        await _collection.Query.NearText(
            "banana",
            diversitySelection: new Diversity.MMR(Balance: 0.9f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText.Selection);
        Assert.False(request.NearText.Selection.Mmr.HasLimit);
        Assert.Equal(0.9f, request.NearText.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that near text without diversity selection leaves selection unset
    /// </summary>
    [Fact]
    public async Task NearText_NoDiversitySelection_LeavesSelectionUnset()
    {
        // Act
        await _collection.Query.NearText(
            "banana",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.NearText.Selection);
    }

    /// <summary>
    /// Tests that the near text lambda builder overload carries the diversity selection
    /// </summary>
    [Fact]
    public async Task NearText_LambdaBuilder_DiversitySelection_SetsSelection()
    {
        // Act
        await _collection.Query.NearText(
            v => v(["banana"]).TargetVectorsSum("title", "description"),
            diversitySelection: new Diversity.MMR(Limit: 8),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText.Selection);
        Assert.Equal(8u, request.NearText.Selection.Mmr.Limit);
    }

    #endregion

    #region NearMedia

    /// <summary>
    /// Returns the selection carried by the near-media message of the given type
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="mediaType">The media type</param>
    /// <returns>The selection, or null when unset</returns>
    private static V1.Selection? MediaSelection(
        V1.SearchRequest request,
        NearMediaType mediaType
    ) =>
        mediaType switch
        {
            NearMediaType.Image => request.NearImage?.Selection,
            NearMediaType.Video => request.NearVideo?.Selection,
            NearMediaType.Audio => request.NearAudio?.Selection,
            NearMediaType.Depth => request.NearDepth?.Selection,
            NearMediaType.Thermal => request.NearThermal?.Selection,
            NearMediaType.IMU => request.NearImu?.Selection,
            _ => throw new ArgumentOutOfRangeException(nameof(mediaType)),
        };

    /// <summary>
    /// Builds the near-media input for the given media type
    /// </summary>
    /// <param name="mediaType">The media type</param>
    /// <returns>The near media input factory</returns>
    private static NearMediaInput.FactoryFn MediaFactory(NearMediaType mediaType) =>
        mediaType switch
        {
            NearMediaType.Image => m => m.Image(TestMediaBytes).Build(),
            NearMediaType.Video => m => m.Video(TestMediaBytes).Build(),
            NearMediaType.Audio => m => m.Audio(TestMediaBytes).Build(),
            NearMediaType.Depth => m => m.Depth(TestMediaBytes).Build(),
            NearMediaType.Thermal => m => m.Thermal(TestMediaBytes).Build(),
            NearMediaType.IMU => m => m.IMU(TestMediaBytes).Build(),
            _ => throw new ArgumentOutOfRangeException(nameof(mediaType)),
        };

    /// <summary>
    /// Tests that every near media type carries the diversity selection
    /// </summary>
    /// <param name="mediaType">The media type</param>
    /// <returns>A task</returns>
    [Theory]
    [InlineData(NearMediaType.Image)]
    [InlineData(NearMediaType.Video)]
    [InlineData(NearMediaType.Audio)]
    [InlineData(NearMediaType.Depth)]
    [InlineData(NearMediaType.Thermal)]
    [InlineData(NearMediaType.IMU)]
    public async Task NearMedia_DiversitySelection_MMR_SetsSelection(NearMediaType mediaType)
    {
        // Act
        await _collection.Query.NearMedia(
            MediaFactory(mediaType),
            diversitySelection: new Diversity.MMR(Limit: 9, Balance: 0.6f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        var selection = MediaSelection(request, mediaType);
        Assert.NotNull(selection);
        Assert.Equal(9u, selection.Mmr.Limit);
        Assert.Equal(0.6f, selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that near media diversity selection with omitted balance leaves balance unset
    /// </summary>
    [Fact]
    public async Task NearMedia_DiversitySelection_BalanceOmitted_LeavesBalanceUnset()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes).Build(),
            diversitySelection: new Diversity.MMR(Limit: 1),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage.Selection);
        Assert.True(request.NearImage.Selection.Mmr.HasLimit);
        Assert.False(request.NearImage.Selection.Mmr.HasBalance);
    }

    /// <summary>
    /// Tests that near media without diversity selection leaves selection unset
    /// </summary>
    [Fact]
    public async Task NearMedia_NoDiversitySelection_LeavesSelectionUnset()
    {
        // Act
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes).Build(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.NearImage.Selection);
    }

    #endregion

    #region Generate

    /// <summary>
    /// Tests that generate near vector diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Generate_NearVector_DiversitySelection_SetsSelection()
    {
        // Act
        await _collection.Generate.NearVector(
            new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 4, Balance: 0.25f),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector.Selection);
        Assert.Equal(4u, request.NearVector.Selection.Mmr.Limit);
        Assert.Equal(0.25f, request.NearVector.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that generate near object diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Generate_NearObject_DiversitySelection_SetsSelection()
    {
        // Act
        await _collection.Generate.NearObject(
            TestObjectID,
            diversitySelection: new Diversity.MMR(Limit: 3, Balance: 0.1f),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearObject.Selection);
        Assert.Equal(3u, request.NearObject.Selection.Mmr.Limit);
        Assert.Equal(0.1f, request.NearObject.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that generate near text diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Generate_NearText_DiversitySelection_SetsSelection()
    {
        // Act
        await _collection.Generate.NearText(
            "banana",
            diversitySelection: new Diversity.MMR(Limit: 6),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText.Selection);
        Assert.Equal(6u, request.NearText.Selection.Mmr.Limit);
        Assert.False(request.NearText.Selection.Mmr.HasBalance);
    }

    /// <summary>
    /// Tests that generate near media diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Generate_NearMedia_DiversitySelection_SetsSelection()
    {
        // Act
        await _collection.Generate.NearMedia(
            m => m.Image(TestMediaBytes).Build(),
            diversitySelection: new Diversity.MMR(Limit: 2, Balance: 0.8f),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage.Selection);
        Assert.Equal(2u, request.NearImage.Selection.Mmr.Limit);
        Assert.Equal(0.8f, request.NearImage.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that generate near vector without diversity selection leaves selection unset
    /// </summary>
    [Fact]
    public async Task Generate_NearVector_NoDiversitySelection_LeavesSelectionUnset()
    {
        // Act
        await _collection.Generate.NearVector(
            new float[] { 1f, 0f, 0f },
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.NearVector.Selection);
    }

    #endregion

    #region Typed

    /// <summary>
    /// Tests that typed near vector diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_NearVector_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedQueryClient = new TypedQueryClient<Article>(_collection.Query);

        // Act
        await typedQueryClient.NearVector(
            new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 2, Balance: 1.0f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector.Selection);
        Assert.Equal(2u, request.NearVector.Selection.Mmr.Limit);
        Assert.Equal(1.0f, request.NearVector.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that typed near object diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_NearObject_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedQueryClient = new TypedQueryClient<Article>(_collection.Query);

        // Act
        await typedQueryClient.NearObject(
            TestObjectID,
            diversitySelection: new Diversity.MMR(Limit: 5),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearObject.Selection);
        Assert.Equal(5u, request.NearObject.Selection.Mmr.Limit);
        Assert.False(request.NearObject.Selection.Mmr.HasBalance);
    }

    /// <summary>
    /// Tests that typed near text diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_NearText_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedQueryClient = new TypedQueryClient<Article>(_collection.Query);

        // Act
        await typedQueryClient.NearText(
            "banana",
            diversitySelection: new Diversity.MMR(Balance: 0.3f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText.Selection);
        Assert.False(request.NearText.Selection.Mmr.HasLimit);
        Assert.Equal(0.3f, request.NearText.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that typed near media diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_NearMedia_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedQueryClient = new TypedQueryClient<Article>(_collection.Query);

        // Act
        await typedQueryClient.NearMedia(
            m => m.Image(TestMediaBytes).Build(),
            diversitySelection: new Diversity.MMR(Limit: 3, Balance: 0.7f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage.Selection);
        Assert.Equal(3u, request.NearImage.Selection.Mmr.Limit);
        Assert.Equal(0.7f, request.NearImage.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that typed near vector without diversity selection leaves selection unset
    /// </summary>
    [Fact]
    public async Task Typed_NearVector_NoDiversitySelection_LeavesSelectionUnset()
    {
        // Arrange
        var typedQueryClient = new TypedQueryClient<Article>(_collection.Query);

        // Act
        await typedQueryClient.NearVector(
            new float[] { 1f, 0f, 0f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.NearVector.Selection);
    }

    #endregion

    #region Typed generate

    /// <summary>
    /// Tests that typed generate near vector diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_Generate_NearVector_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedGenerateClient = new TypedGenerateClient<Article>(_collection.Generate);

        // Act
        await typedGenerateClient.NearVector(
            new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 6),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearVector.Selection);
        Assert.Equal(6u, request.NearVector.Selection.Mmr.Limit);
        Assert.False(request.NearVector.Selection.Mmr.HasBalance);
    }

    /// <summary>
    /// Tests that typed generate near object diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_Generate_NearObject_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedGenerateClient = new TypedGenerateClient<Article>(_collection.Generate);

        // Act
        await typedGenerateClient.NearObject(
            TestObjectID,
            diversitySelection: new Diversity.MMR(Limit: 1, Balance: 0.05f),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearObject.Selection);
        Assert.Equal(1u, request.NearObject.Selection.Mmr.Limit);
        Assert.Equal(0.05f, request.NearObject.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that typed generate near text diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_Generate_NearText_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedGenerateClient = new TypedGenerateClient<Article>(_collection.Generate);

        // Act
        await typedGenerateClient.NearText(
            "banana",
            diversitySelection: new Diversity.MMR(Limit: 7, Balance: 0.45f),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearText.Selection);
        Assert.Equal(7u, request.NearText.Selection.Mmr.Limit);
        Assert.Equal(0.45f, request.NearText.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that typed generate near media diversity selection sets selection on the request
    /// </summary>
    [Fact]
    public async Task Typed_Generate_NearMedia_DiversitySelection_SetsSelection()
    {
        // Arrange
        var typedGenerateClient = new TypedGenerateClient<Article>(_collection.Generate);

        // Act
        await typedGenerateClient.NearMedia(
            m => m.Image(TestMediaBytes).Build(),
            diversitySelection: new Diversity.MMR(Balance: 0.15f),
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage.Selection);
        Assert.False(request.NearImage.Selection.Mmr.HasLimit);
        Assert.Equal(0.15f, request.NearImage.Selection.Mmr.Balance);
    }

    /// <summary>
    /// Tests that typed generate near vector without diversity selection leaves selection unset
    /// </summary>
    [Fact]
    public async Task Typed_Generate_NearVector_NoDiversitySelection_LeavesSelectionUnset()
    {
        // Arrange
        var typedGenerateClient = new TypedGenerateClient<Article>(_collection.Generate);

        // Act
        await typedGenerateClient.NearVector(
            new float[] { 1f, 0f, 0f },
            singlePrompt: "Describe {title}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.NearVector.Selection);
    }

    #endregion
}

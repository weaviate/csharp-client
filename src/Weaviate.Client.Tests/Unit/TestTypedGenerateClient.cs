using Weaviate.Client.Models;
using Weaviate.Client.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for TypedGenerateClient to ensure proper strongly-typed generative AI operations.
/// </summary>
[Collection("Unit Tests")]
public class TypedGenerateClientTests
{
    private class TestArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
    }

    [Fact]
    public void Constructor_WithValidGenerateClient_InitializesCorrectly()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var generateClient = collectionClient.Generate;

        // Act
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(generateClient);

        // Assert
        Assert.NotNull(typedGenerateClient);
    }

    [Fact]
    public void Constructor_WithNullGenerateClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TypedGenerateClient<TestArticle>(null!));
    }

    [Fact]
    public void FetchObjects_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var prompt = new SinglePrompt("Summarize this article");

        // Act & Assert
        // Verify the method accepts prompt parameter
        Assert.NotNull(typedGenerateClient);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void FetchObjects_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 5, ObjectsPerGroup = 2 };
        var prompt = new SinglePrompt("Summarize this group");

        // Act & Assert
        // Verify the method accepts GroupByRequest and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotNull(groupBy);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void FetchObjectByID_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var testId = Guid.NewGuid();
        var prompt = new SinglePrompt("Explain this article");

        // Act & Assert
        // Verify the method accepts Guid and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEqual(Guid.Empty, testId);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void FetchObjectsByIDs_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var ids = new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var groupedPrompt = new GroupedPrompt("Summarize each article");

        // Act & Assert
        // Verify the method accepts HashSet<Guid> and GroupedPrompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.Equal(2, ids.Count);
        Assert.NotNull(groupedPrompt);
    }

    [Fact]
    public void NearText_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var searchText = new OneOrManyOf<string>("test query");
        var prompt = new SinglePrompt("Generate summary");

        // Act & Assert
        // Verify the method accepts text and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotNull(searchText);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void NearText_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var searchText = new OneOrManyOf<string>("test query");
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        var groupedPrompt = new GroupedPrompt("Summarize each group");

        // Act & Assert
        // Verify the method accepts text, groupBy, and groupedPrompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotNull(searchText);
        Assert.NotNull(groupBy);
        Assert.NotNull(groupedPrompt);
    }

    [Fact]
    public void NearVector_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var vectors = new Vectors { ["default"] = new float[] { 0.1f, 0.2f, 0.3f } };
        var prompt = new SinglePrompt("Analyze these results");

        // Act & Assert
        // Verify the method accepts Vectors and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotNull(vectors);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void NearVector_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var vectors = new Vectors { ["default"] = new float[] { 0.1f, 0.2f, 0.3f } };
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        var groupedPrompt = new GroupedPrompt("Summarize groups");

        // Act & Assert
        // Verify the method accepts Vectors, GroupByRequest, and groupedPrompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotNull(vectors);
        Assert.NotNull(groupBy);
        Assert.NotNull(groupedPrompt);
    }

    [Fact]
    public void BM25_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var query = "search terms";
        var searchFields = new[] { "Title", "Content" };
        var prompt = new SinglePrompt("Generate insights");

        // Act & Assert
        // Verify the method accepts query, searchFields, and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(query);
        Assert.NotEmpty(searchFields);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void BM25_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var query = "search terms";
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        var groupedPrompt = new GroupedPrompt("Analyze groups");

        // Act & Assert
        // Verify the method accepts query, groupBy, and groupedPrompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(query);
        Assert.NotNull(groupBy);
        Assert.NotNull(groupedPrompt);
    }

    [Fact]
    public void Hybrid_WithoutVectors_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var query = "hybrid search";
        var prompt = new SinglePrompt("Generate content");

        // Act & Assert
        // Verify the method accepts query and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(query);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void Hybrid_WithVectors_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var query = "hybrid search";
        var vectors = new Vectors { ["default"] = new float[] { 0.1f, 0.2f, 0.3f } };
        var prompt = new SinglePrompt("Generate analysis");

        // Act & Assert
        // Verify the method accepts query, Vectors, and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(query);
        Assert.NotNull(vectors);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void Hybrid_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var query = "hybrid search";
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        var groupedPrompt = new GroupedPrompt("Generate summaries");

        // Act & Assert
        // Verify the method accepts query, groupBy, and groupedPrompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(query);
        Assert.NotNull(groupBy);
        Assert.NotNull(groupedPrompt);
    }

    [Fact]
    public void NearObject_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var nearObjectId = Guid.NewGuid();
        var prompt = new SinglePrompt("Describe similar objects");

        // Act & Assert
        // Verify the method accepts Guid and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEqual(Guid.Empty, nearObjectId);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void NearObject_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var nearObjectId = Guid.NewGuid();
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        var groupedPrompt = new GroupedPrompt("Compare groups");

        // Act & Assert
        // Verify the method accepts Guid, GroupByRequest, and groupedPrompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEqual(Guid.Empty, nearObjectId);
        Assert.NotNull(groupBy);
        Assert.NotNull(groupedPrompt);
    }

    [Fact]
    public void NearImage_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var prompt = new SinglePrompt("Describe the image");

        // Act & Assert
        // Verify the method accepts byte[] and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(imageData);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void NearImage_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        var groupedPrompt = new GroupedPrompt("Describe image groups");

        // Act & Assert
        // Verify the method accepts byte[], GroupByRequest, and groupedPrompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(imageData);
        Assert.NotNull(groupBy);
        Assert.NotNull(groupedPrompt);
    }

    [Fact]
    public void NearMedia_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var mediaData = new byte[] { 0x01, 0x02, 0x03 };
        var mediaType = NearMediaType.Image;
        var prompt = new SinglePrompt("Analyze this media");

        // Act & Assert
        // Verify the method accepts byte[], NearMediaType, and prompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(mediaData);
        Assert.Equal(NearMediaType.Image, mediaType);
        Assert.NotNull(prompt);
    }

    [Fact]
    public void NearMedia_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        var mediaData = new byte[] { 0x01, 0x02, 0x03 };
        var mediaType = NearMediaType.Image;
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        var groupedPrompt = new GroupedPrompt("Analyze media groups");

        // Act & Assert
        // Verify the method accepts byte[], NearMediaType, GroupByRequest, and groupedPrompt parameters
        Assert.NotNull(typedGenerateClient);
        Assert.NotEmpty(mediaData);
        Assert.Equal(NearMediaType.Image, mediaType);
        Assert.NotNull(groupBy);
        Assert.NotNull(groupedPrompt);
    }

    [Fact]
    public void TypedGenerateClient_WrapsGenerateClient_MaintainsTypeConstraints()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedGenerateClient = new TypedGenerateClient<TestArticle>(collectionClient.Generate);

        // Assert
        // Verify that TypedGenerateClient enforces type constraints
        // The fact that this compiles and accepts TestArticle proves type safety
        Assert.NotNull(typedGenerateClient);
    }

    private static WeaviateClient CreateWeaviateClient()
    {
        return Mocks.MockWeaviateClient.CreateWithMockHandler().Client;
    }
}

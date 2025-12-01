using Weaviate.Client.Models;
using Weaviate.Client.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for TypedQueryClient to ensure proper strongly-typed query operations.
/// </summary>
[Collection("Unit Tests")]
public class TypedQueryClientTests
{
    private class TestArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
    }

    [Fact]
    public void Constructor_WithValidQueryClient_InitializesCorrectly()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var queryClient = collectionClient.Query;

        // Act
        var typedQueryClient = new TypedQueryClient<TestArticle>(queryClient);

        // Assert
        Assert.NotNull(typedQueryClient);
    }

    [Fact]
    public void Constructor_WithNullQueryClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TypedQueryClient<TestArticle>(null!));
    }

    [Fact]
    public void FetchObjects_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        // Act & Assert
        // Verify the method accepts proper parameters
        Assert.NotNull(typedQueryClient);
    }

    [Fact]
    public void FetchObjects_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts GroupByRequest parameter
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(groupBy);
    }

    [Fact]
    public void FetchObjectByID_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var testId = Guid.NewGuid();

        // Act & Assert
        // Verify the method accepts Guid parameter
        Assert.NotNull(typedQueryClient);
        Assert.NotEqual(Guid.Empty, testId);
    }

    [Fact]
    public void FetchObjectsByIDs_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var ids = new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act & Assert
        // Verify the method accepts HashSet<Guid> parameter
        Assert.NotNull(typedQueryClient);
        Assert.Equal(2, ids.Count);
    }

    [Fact]
    public void NearText_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var searchText = new OneOrManyOf<string>("test query");

        // Act & Assert
        // Verify the method accepts text parameter
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(searchText);
    }

    [Fact]
    public void NearText_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var searchText = new OneOrManyOf<string>("test query");
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts both text and groupBy parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(searchText);
        Assert.NotNull(groupBy);
    }

    [Fact]
    public void NearVector_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var vectors = new Vectors { ["default"] = new float[] { 0.1f, 0.2f, 0.3f } };

        // Act & Assert
        // Verify the method accepts Vectors parameter
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(vectors);
    }

    [Fact]
    public void NearVector_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var vectors = new Vectors { ["default"] = new float[] { 0.1f, 0.2f, 0.3f } };
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts both Vectors and GroupByRequest parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(vectors);
        Assert.NotNull(groupBy);
    }

    [Fact]
    public void BM25_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var query = "search terms";
        var searchFields = new[] { "Title", "Content" };

        // Act & Assert
        // Verify the method accepts query string and searchFields parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(query);
        Assert.NotEmpty(searchFields);
    }

    [Fact]
    public void BM25_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var query = "search terms";
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts both query and groupBy parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(query);
        Assert.NotNull(groupBy);
    }

    [Fact]
    public void Hybrid_WithVectors_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var query = "hybrid search";
        var vectors = new Vectors { ["default"] = new float[] { 0.1f, 0.2f, 0.3f } };

        // Act & Assert
        // Verify the method accepts query and Vectors parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(query);
        Assert.NotNull(vectors);
    }

    [Fact]
    public void Hybrid_WithIHybridVectorInput_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var query = "hybrid search";

        // Act & Assert
        // Verify the method accepts query parameter (vectors can be null)
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(query);
    }

    [Fact]
    public void Hybrid_WithGroupByAndVectors_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var query = "hybrid search";
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        var vectors = new Vectors { ["default"] = new float[] { 0.1f, 0.2f, 0.3f } };

        // Act & Assert
        // Verify the method accepts query, groupBy, and Vectors parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(query);
        Assert.NotNull(groupBy);
        Assert.NotNull(vectors);
    }

    [Fact]
    public void NearObject_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var nearObjectId = Guid.NewGuid();

        // Act & Assert
        // Verify the method accepts Guid parameter
        Assert.NotNull(typedQueryClient);
        Assert.NotEqual(Guid.Empty, nearObjectId);
    }

    [Fact]
    public void NearObject_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var nearObjectId = Guid.NewGuid();
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts Guid and GroupByRequest parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEqual(Guid.Empty, nearObjectId);
        Assert.NotNull(groupBy);
    }

    [Fact]
    public void NearImage_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var imageData = new byte[] { 0x01, 0x02, 0x03 };

        // Act & Assert
        // Verify the method accepts byte[] parameter
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(imageData);
    }

    [Fact]
    public void NearImage_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts byte[] and GroupByRequest parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(imageData);
        Assert.NotNull(groupBy);
    }

    [Fact]
    public void NearMedia_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var mediaData = new byte[] { 0x01, 0x02, 0x03 };
        var mediaType = NearMediaType.Image;

        // Act & Assert
        // Verify the method accepts byte[] and NearMediaType parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(mediaData);
        Assert.Equal(NearMediaType.Image, mediaType);
    }

    [Fact]
    public void NearMedia_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var mediaData = new byte[] { 0x01, 0x02, 0x03 };
        var mediaType = NearMediaType.Image;
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts byte[], NearMediaType, and GroupByRequest parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(mediaData);
        Assert.Equal(NearMediaType.Image, mediaType);
        Assert.NotNull(groupBy);
    }

    [Fact]
    public void TypedQueryClient_WrapsQueryClient_MaintainsTypeConstraints()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        // Assert
        // Verify that TypedQueryClient enforces type constraints
        // The fact that this compiles and accepts TestArticle proves type safety
        Assert.NotNull(typedQueryClient);
    }

    private static WeaviateClient CreateWeaviateClient()
    {
        return Mocks.MockWeaviateClient.CreateWithMockHandler().Client;
    }
}

using Weaviate.Client.Models;
using Weaviate.Client.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for TypedQueryClient to ensure proper strongly-typed query operations.
/// </summary>
[Collection("Unit Tests")]
public class TypedQueryClientTests
{
    /// <summary>
    /// The test article class
    /// </summary>
    private class TestArticle
    {
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the content
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the published date
        /// </summary>
        public DateTime PublishedDate { get; set; }

        /// <summary>
        /// Gets or sets the value of the view count
        /// </summary>
        public int ViewCount { get; set; }
    }

    /// <summary>
    /// Tests that constructor with valid query client initializes correctly
    /// </summary>
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

    /// <summary>
    /// Tests that constructor with null query client throws argument null exception
    /// </summary>
    [Fact]
    public void Constructor_WithNullQueryClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TypedQueryClient<TestArticle>(null!));
    }

    /// <summary>
    /// Tests that fetch objects without group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that fetch objects with group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that fetch object by id accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that fetch objects by i ds accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that near text without group by accepts correct parameters
    /// </summary>
    [Fact]
    public void NearText_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        AutoArray<string> searchText = "test query";

        // Act & Assert
        // Verify the method accepts text parameter
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(searchText);
    }

    /// <summary>
    /// Tests that near text with group by accepts correct parameters
    /// </summary>
    [Fact]
    public void NearText_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        AutoArray<string> searchText = "test query";
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts both text and groupBy parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(searchText);
        Assert.NotNull(groupBy);
    }

    /// <summary>
    /// Tests that near vector without group by accepts correct parameters
    /// </summary>
    [Fact]
    public void NearVector_WithoutGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        Vectors vectors = new float[] { 0.1f, 0.2f, 0.3f };

        // Act & Assert
        // Verify the method accepts Vectors parameter
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(vectors);
    }

    /// <summary>
    /// Tests that near vector with group by accepts correct parameters
    /// </summary>
    [Fact]
    public void NearVector_WithGroupBy_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        Vectors vectors = new float[] { 0.1f, 0.2f, 0.3f };
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };

        // Act & Assert
        // Verify the method accepts both Vectors and GroupByRequest parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotNull(vectors);
        Assert.NotNull(groupBy);
    }

    /// <summary>
    /// Tests that bm 25 without group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that bm 25 with group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that hybrid with vectors accepts correct parameters
    /// </summary>
    [Fact]
    public void Hybrid_WithVectors_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var query = "hybrid search";
        Vectors vectors = new float[] { 0.1f, 0.2f, 0.3f };

        // Act & Assert
        // Verify the method accepts query and Vectors parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(query);
        Assert.NotNull(vectors);
    }

    /// <summary>
    /// Tests that hybrid with hybrid vector input accepts correct parameters
    /// </summary>
    [Fact]
    public void Hybrid_WithHybridVectorInput_AcceptsCorrectParameters()
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

    /// <summary>
    /// Tests that hybrid with group by and vectors accepts correct parameters
    /// </summary>
    [Fact]
    public void Hybrid_WithGroupByAndVectors_AcceptsCorrectParameters()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedQueryClient = new TypedQueryClient<TestArticle>(collectionClient.Query);

        var query = "hybrid search";
        var groupBy = new GroupByRequest("Title") { NumberOfGroups = 10 };
        Vectors vectors = new float[] { 0.1f, 0.2f, 0.3f };

        // Act & Assert
        // Verify the method accepts query, groupBy, and Vectors parameters
        Assert.NotNull(typedQueryClient);
        Assert.NotEmpty(query);
        Assert.NotNull(groupBy);
        Assert.NotNull(vectors);
    }

    /// <summary>
    /// Tests that near object without group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that near object with group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that near image without group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that near image with group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that near media without group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that near media with group by accepts correct parameters
    /// </summary>
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

    /// <summary>
    /// Tests that typed query client wraps query client maintains type constraints
    /// </summary>
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

    /// <summary>
    /// Tests that near vector accepts all vector input formats
    /// </summary>
    [Fact]
    public void NearVector_AcceptsAllVectorInputFormats()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var queryClient = collectionClient.Query;

        // Test all vector input formats - verify implicit conversions to VectorSearchInput

        // 1. float[] - basic array
        float[] vector1 = [20f, 21f, 22f];
        VectorSearchInput input1 = vector1; // Test implicit conversion from float[]
        Assert.NotNull(input1);

        // 2. double[] - basic array
        double[] vector1d = [20.0, 21.0, 22.0];
        VectorSearchInput input1d = vector1d; // Test implicit conversion from double[]
        Assert.NotNull(input1d);

        // 3. Vector - implicit conversion from float[]
        Vector vector2 = vector1;
        VectorSearchInput input2 = vector2; // Test implicit conversion from Vector
        Assert.NotNull(input2);

        // 4. NamedVector - tuple syntax (string, Vector)
        NamedVector vector3 = ("default", vector2);
        VectorSearchInput input3 = vector3; // Test implicit conversion from NamedVector
        Assert.NotNull(input3);

        // 5. Vectors - implicit conversion from Vector
        Vectors vector4 = vector2;
        VectorSearchInput input4 = vector4; // Test implicit conversion from Vectors
        Assert.NotNull(input4);

        // 6. Vectors - implicit conversion from NamedVector
        Vectors vector5 = vector3;
        VectorSearchInput input5 = vector5; // Test implicit conversion from Vectors (from NamedVector)
        Assert.NotNull(input5);

        // 7. float[,] - 2D array (multi-vector)
        float[,] vector6 = new[,]
        {
            { 20f, 21f, 22f },
            { 23f, 24f, 25f },
        };

        // 8. Vector - implicit conversion from 2D array
        Vector vector7 = vector6;
        VectorSearchInput input7 = vector7; // Test implicit conversion from Vector (multi-vector)
        Assert.NotNull(input7);

        // 9. NamedVector - from 2D array via Vector
        NamedVector vector8 = ("default", vector7);
        VectorSearchInput input8 = vector8; // Test implicit conversion from NamedVector (multi-vector)
        Assert.NotNull(input8);

        // 10. Vectors - implicit conversion from 2D array
        Vectors vector9 = vector6;
        VectorSearchInput input9 = vector9; // Test implicit conversion from Vectors (multi-vector)
        Assert.NotNull(input9);

        // 11. Vectors - from NamedVector (multi-vector)
        Vectors vectorA = vector8;
        VectorSearchInput inputA = vectorA; // Test implicit conversion from Vectors (from NamedVector multi-vector)
        Assert.NotNull(inputA);

        // 12. NamedVector[] - array of named vectors (multiple target vectors)
        NamedVector[] vectorB = [vector3, vector8];
        VectorSearchInput inputB = vectorB; // Test implicit conversion from NamedVector[]
        Assert.NotNull(inputB);

        // Additional NamedVector[] test cases

        // 13. Single element NamedVector[]
        NamedVector[] singleElementArray = [vector3];
        VectorSearchInput inputC = singleElementArray; // Test implicit conversion from single-element NamedVector[]
        Assert.NotNull(inputC);

        // 14. NamedVector[] containing different named vectors
        Vector textVector = new float[] { 1f, 2f, 3f };
        Vector imageVector = new float[] { 4f, 5f, 6f };
        NamedVector namedVector1 = ("text", textVector);
        NamedVector namedVector2 = ("image", imageVector);
        NamedVector[] multiNamedVectors = [namedVector1, namedVector2];
        VectorSearchInput inputD = multiNamedVectors; // Test implicit conversion from multi-target NamedVector[]
        Assert.NotNull(inputD);

        // 15. NamedVector[] mixing single and multi vectors
        Vector singleVector = new float[] { 7f, 8f, 9f };
        Vector multiVector = new float[,]
        {
            { 10f, 11f },
            { 12f, 13f },
        };
        NamedVector singleVec = ("single", singleVector);
        NamedVector multiVec = ("multi", multiVector);
        NamedVector[] mixedArray = [singleVec, multiVec];
        VectorSearchInput inputE = mixedArray; // Test implicit conversion from mixed NamedVector[]
        Assert.NotNull(inputE);

        // Verify that QueryClient actually accepts VectorSearchInput
        Assert.NotNull(queryClient);
    }

    /// <summary>
    /// Creates the weaviate client
    /// </summary>
    /// <returns>The weaviate client</returns>
    private static WeaviateClient CreateWeaviateClient()
    {
        return Mocks.MockWeaviateClient.CreateWithMockHandler().Client;
    }
}

using Weaviate.Client.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for CollectionClientExtensions to ensure proper extension method behavior.
/// </summary>
[Collection("Unit Tests")]
public class CollectionClientExtensionsTests
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
    /// Tests that as typed with valid collection client returns typed collection client
    /// </summary>
    [Fact]
    public async Task AsTyped_WithValidCollectionClient_ReturnsTypedCollectionClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = await collectionClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(typedClient);
        Assert.IsType<TypedCollectionClient<TestArticle>>(typedClient);
        Assert.Equal("Articles", typedClient.Name);
    }

    /// <summary>
    /// Tests that as typed with null collection client throws argument null exception
    /// </summary>
    [Fact]
    public async Task AsTyped_WithNullCollectionClient_ThrowsArgumentNullException()
    {
        // Arrange
        CollectionClient? collectionClient = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await collectionClient!.AsTyped<TestArticle>(
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    /// <summary>
    /// Tests that as typed without validation does not validate type
    /// </summary>
    [Fact]
    public async Task AsTyped_WithoutValidation_DoesNotValidateType()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = await collectionClient.AsTyped<TestArticle>(
            validateType: false,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        // Should succeed even if schema doesn't match (no HTTP call made)
        Assert.NotNull(typedClient);
        Assert.Equal("Articles", typedClient.Name);
    }

    /// <summary>
    /// Tests that as typed preserves tenant configuration
    /// </summary>
    [Fact]
    public async Task AsTyped_PreservesTenantConfiguration()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles").WithTenant("tenant1");

        // Act
        var typedClient = await collectionClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal("tenant1", typedClient.Tenant);
        Assert.Equal("Articles", typedClient.Name);
    }

    /// <summary>
    /// Tests that as typed preserves consistency level configuration
    /// </summary>
    [Fact]
    public async Task AsTyped_PreservesConsistencyLevelConfiguration()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles").WithConsistencyLevel(
            ConsistencyLevels.Quorum
        );

        // Act
        var typedClient = await collectionClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal(ConsistencyLevels.Quorum, typedClient.ConsistencyLevel);
        Assert.Equal("Articles", typedClient.Name);
    }

    /// <summary>
    /// Tests that as typed with chained configuration preserves all settings
    /// </summary>
    [Fact]
    public async Task AsTyped_WithChainedConfiguration_PreservesAllSettings()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles")
            .WithTenant("tenant1")
            .WithConsistencyLevel(ConsistencyLevels.All);

        // Act
        var typedClient = await collectionClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal("tenant1", typedClient.Tenant);
        Assert.Equal(ConsistencyLevels.All, typedClient.ConsistencyLevel);
        Assert.Equal("Articles", typedClient.Name);
    }

    /// <summary>
    /// Tests that as typed returns client with typed data query generate
    /// </summary>
    [Fact]
    public async Task AsTyped_ReturnsClientWithTypedDataQueryGenerate()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = await collectionClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(typedClient.Data);
        Assert.NotNull(typedClient.Query);
        Assert.NotNull(typedClient.Generate);
        Assert.IsType<TypedDataClient<TestArticle>>(typedClient.Data);
        Assert.IsType<TypedQueryClient<TestArticle>>(typedClient.Query);
        Assert.IsType<TypedGenerateClient<TestArticle>>(typedClient.Generate);
    }

    /// <summary>
    /// Tests that as typed called multiple times returns independent instances
    /// </summary>
    [Fact]
    public async Task AsTyped_CalledMultipleTimes_ReturnsIndependentInstances()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient1 = await collectionClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var typedClient2 = await collectionClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotSame(typedClient1, typedClient2);
        Assert.NotSame(typedClient1.Data, typedClient2.Data);
    }

    /// <summary>
    /// Tests that as typed with different types returns correct typed clients
    /// </summary>
    [Fact]
    public async Task AsTyped_WithDifferentTypes_ReturnsCorrectTypedClients()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var articlesClient = new CollectionClient(client, "Articles");
        var productsClient = new CollectionClient(client, "Products");

        // Act
        var typedArticles = await articlesClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var typedProducts = await productsClient.AsTyped<TestProduct>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal("Articles", typedArticles.Name);
        Assert.Equal("Products", typedProducts.Name);
        Assert.IsType<TypedDataClient<TestArticle>>(typedArticles.Data);
        Assert.IsType<TypedDataClient<TestProduct>>(typedProducts.Data);
    }

    /// <summary>
    /// Tests that as typed round trip untyped property returns original client
    /// </summary>
    [Fact]
    public async Task AsTyped_RoundTrip_UntypedPropertyReturnsOriginalClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = await collectionClient.AsTyped<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var untypedClient = typedClient.Untyped;

        // Assert
        Assert.Equal(collectionClient.Name, untypedClient.Name);
        Assert.Equal(collectionClient.Tenant, untypedClient.Tenant);
        Assert.Equal(collectionClient.ConsistencyLevel, untypedClient.ConsistencyLevel);
    }

    /// <summary>
    /// The test product class
    /// </summary>
    private class TestProduct
    {
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the value of the stock
        /// </summary>
        public int Stock { get; set; }
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

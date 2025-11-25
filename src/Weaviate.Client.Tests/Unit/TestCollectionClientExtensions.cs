using Weaviate.Client.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for CollectionClientExtensions to ensure proper extension method behavior.
/// </summary>
[Collection("Unit Tests")]
public class CollectionClientExtensionsTests
{
    private class TestArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
    }

    [Fact]
    public async Task AsTyped_WithValidCollectionClient_ReturnsTypedCollectionClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = await collectionClient.Use<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(typedClient);
        Assert.IsType<TypedCollectionClient<TestArticle>>(typedClient);
        Assert.Equal("Articles", typedClient.Name);
    }

    [Fact]
    public async Task AsTyped_WithNullCollectionClient_ThrowsArgumentNullException()
    {
        // Arrange
        CollectionClient? collectionClient = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await collectionClient!.Use<TestArticle>(
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task AsTyped_WithoutValidation_DoesNotValidateType()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = await collectionClient.Use<TestArticle>(
            validateType: false,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        // Should succeed even if schema doesn't match (no HTTP call made)
        Assert.NotNull(typedClient);
        Assert.Equal("Articles", typedClient.Name);
    }

    [Fact]
    public async Task AsTyped_PreservesTenantConfiguration()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles").WithTenant("tenant1");

        // Act
        var typedClient = await collectionClient.Use<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal("tenant1", typedClient.Tenant);
        Assert.Equal("Articles", typedClient.Name);
    }

    [Fact]
    public async Task AsTyped_PreservesConsistencyLevelConfiguration()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles").WithConsistencyLevel(
            ConsistencyLevels.Quorum
        );

        // Act
        var typedClient = await collectionClient.Use<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal(ConsistencyLevels.Quorum, typedClient.ConsistencyLevel);
        Assert.Equal("Articles", typedClient.Name);
    }

    [Fact]
    public async Task AsTyped_WithChainedConfiguration_PreservesAllSettings()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles")
            .WithTenant("tenant1")
            .WithConsistencyLevel(ConsistencyLevels.All);

        // Act
        var typedClient = await collectionClient.Use<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal("tenant1", typedClient.Tenant);
        Assert.Equal(ConsistencyLevels.All, typedClient.ConsistencyLevel);
        Assert.Equal("Articles", typedClient.Name);
    }

    [Fact]
    public async Task AsTyped_ReturnsClientWithTypedDataQueryGenerate()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = await collectionClient.Use<TestArticle>(
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

    [Fact]
    public async Task AsTyped_CalledMultipleTimes_ReturnsIndependentInstances()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient1 = await collectionClient.Use<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var typedClient2 = await collectionClient.Use<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotSame(typedClient1, typedClient2);
        Assert.NotSame(typedClient1.Data, typedClient2.Data);
    }

    [Fact]
    public async Task AsTyped_WithDifferentTypes_ReturnsCorrectTypedClients()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var articlesClient = new CollectionClient(client, "Articles");
        var productsClient = new CollectionClient(client, "Products");

        // Act
        var typedArticles = await articlesClient.Use<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var typedProducts = await productsClient.Use<TestProduct>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal("Articles", typedArticles.Name);
        Assert.Equal("Products", typedProducts.Name);
        Assert.IsType<TypedDataClient<TestArticle>>(typedArticles.Data);
        Assert.IsType<TypedDataClient<TestProduct>>(typedProducts.Data);
    }

    [Fact]
    public async Task AsTyped_RoundTrip_UntypedPropertyReturnsOriginalClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = await collectionClient.Use<TestArticle>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var untypedClient = typedClient.Untyped;

        // Assert
        Assert.Equal(collectionClient.Name, untypedClient.Name);
        Assert.Equal(collectionClient.Tenant, untypedClient.Tenant);
        Assert.Equal(collectionClient.ConsistencyLevel, untypedClient.ConsistencyLevel);
    }

    private class TestProduct
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    private static WeaviateClient CreateWeaviateClient()
    {
        return new WeaviateClient(new ClientConfiguration(), null, null);
    }
}

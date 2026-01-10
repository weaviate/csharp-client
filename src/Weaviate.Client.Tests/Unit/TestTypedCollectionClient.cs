using Weaviate.Client.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for TypedCollectionClient to ensure proper strongly-typed operations.
/// </summary>
[Collection("Unit Tests")]
public class TypedCollectionClientTests
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
    /// Tests that constructor with collection client initializes properties correctly
    /// </summary>
    [Fact]
    public void Constructor_WithCollectionClient_InitializesPropertiesCorrectly()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Assert
        Assert.NotNull(typedClient.Data);
        Assert.NotNull(typedClient.Query);
        Assert.NotNull(typedClient.Generate);
        Assert.NotNull(typedClient.Aggregate);
        Assert.NotNull(typedClient.Config);
        Assert.Equal("Articles", typedClient.Name);
    }

    /// <summary>
    /// Tests that constructor with null collection client throws argument null exception
    /// </summary>
    [Fact]
    public void Constructor_WithNullCollectionClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TypedCollectionClient<TestArticle>(null!));
    }

    /// <summary>
    /// Tests that data returns typed data client
    /// </summary>
    [Fact]
    public void Data_ReturnsTypedDataClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var dataClient = typedClient.Data;

        // Assert
        Assert.NotNull(dataClient);
        Assert.IsType<TypedDataClient<TestArticle>>(dataClient);
    }

    /// <summary>
    /// Tests that query returns typed query client
    /// </summary>
    [Fact]
    public void Query_ReturnsTypedQueryClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var queryClient = typedClient.Query;

        // Assert
        Assert.NotNull(queryClient);
        Assert.IsType<TypedQueryClient<TestArticle>>(queryClient);
    }

    /// <summary>
    /// Tests that generate returns typed generate client
    /// </summary>
    [Fact]
    public void Generate_ReturnsTypedGenerateClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var generateClient = typedClient.Generate;

        // Assert
        Assert.NotNull(generateClient);
        Assert.IsType<TypedGenerateClient<TestArticle>>(generateClient);
    }

    /// <summary>
    /// Tests that aggregate returns untyped aggregate client
    /// </summary>
    [Fact]
    public void Aggregate_ReturnsUntypedAggregateClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var aggregateClient = typedClient.Aggregate;

        // Assert
        Assert.NotNull(aggregateClient);
        Assert.IsType<AggregateClient>(aggregateClient);
    }

    /// <summary>
    /// Tests that config returns collection config client
    /// </summary>
    [Fact]
    public void Config_ReturnsCollectionConfigClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var configClient = typedClient.Config;

        // Assert
        Assert.NotNull(configClient);
        Assert.IsType<CollectionConfigClient>(configClient);
    }

    /// <summary>
    /// Tests that name returns collection name
    /// </summary>
    [Fact]
    public void Name_ReturnsCollectionName()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var name = typedClient.Name;

        // Assert
        Assert.Equal("Articles", name);
    }

    /// <summary>
    /// Tests that tenant returns null by default
    /// </summary>
    [Fact]
    public void Tenant_ReturnsNullByDefault()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var tenant = typedClient.Tenant;

        // Assert
        Assert.True(string.IsNullOrEmpty(tenant));
    }

    /// <summary>
    /// Tests that tenant returns tenant when set
    /// </summary>
    [Fact]
    public void Tenant_ReturnsTenantWhenSet()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles").WithTenant("tenant1");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var tenant = typedClient.Tenant;

        // Assert
        Assert.Equal("tenant1", tenant);
    }

    /// <summary>
    /// Tests that consistency level returns null by default
    /// </summary>
    [Fact]
    public void ConsistencyLevel_ReturnsNullByDefault()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var consistencyLevel = typedClient.ConsistencyLevel;

        // Assert
        Assert.Null(consistencyLevel);
    }

    /// <summary>
    /// Tests that consistency level returns level when set
    /// </summary>
    [Fact]
    public void ConsistencyLevel_ReturnsLevelWhenSet()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles").WithConsistencyLevel(
            ConsistencyLevels.Quorum
        );
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var consistencyLevel = typedClient.ConsistencyLevel;

        // Assert
        Assert.Equal(ConsistencyLevels.Quorum, consistencyLevel);
    }

    /// <summary>
    /// Tests that with tenant creates new typed collection client with tenant
    /// </summary>
    [Fact]
    public void WithTenant_CreatesNewTypedCollectionClientWithTenant()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var tenantClient = typedClient.WithTenant("tenant1");

        // Assert
        Assert.NotEqual(typedClient, tenantClient);
        Assert.Equal("tenant1", tenantClient.Tenant);
        Assert.Equal("Articles", tenantClient.Name);
    }

    /// <summary>
    /// Tests that with consistency level creates new typed collection client with consistency level
    /// </summary>
    [Fact]
    public void WithConsistencyLevel_CreatesNewTypedCollectionClientWithConsistencyLevel()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var consistentClient = typedClient.WithConsistencyLevel(ConsistencyLevels.All);

        // Assert
        Assert.NotEqual(typedClient, consistentClient);
        Assert.Equal(ConsistencyLevels.All, consistentClient.ConsistencyLevel);
        Assert.Equal("Articles", consistentClient.Name);
    }

    /// <summary>
    /// Tests that untyped returns underlying collection client
    /// </summary>
    [Fact]
    public void Untyped_ReturnsUnderlyingCollectionClient()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var untypedClient = typedClient.Untyped;

        // Assert
        Assert.NotNull(untypedClient);
        Assert.Equal("Articles", untypedClient.Name);
    }

    /// <summary>
    /// Tests that typed collection client maintains type constraints
    /// </summary>
    [Fact]
    public void TypedCollectionClient_MaintainsTypeConstraints()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");

        // Act
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Assert
        // Verify that TypedCollectionClient enforces type constraints
        // The fact that this compiles and accepts TestArticle proves type safety
        Assert.NotNull(typedClient);
        Assert.IsType<TypedDataClient<TestArticle>>(typedClient.Data);
        Assert.IsType<TypedQueryClient<TestArticle>>(typedClient.Query);
        Assert.IsType<TypedGenerateClient<TestArticle>>(typedClient.Generate);
    }

    /// <summary>
    /// Tests that with tenant chained calls maintains type and configuration
    /// </summary>
    [Fact]
    public void WithTenant_ChainedCalls_MaintainsTypeAndConfiguration()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act
        var chainedClient = typedClient
            .WithTenant("tenant1")
            .WithConsistencyLevel(ConsistencyLevels.Quorum);

        // Assert
        Assert.Equal("tenant1", chainedClient.Tenant);
        Assert.Equal(ConsistencyLevels.Quorum, chainedClient.ConsistencyLevel);
        Assert.Equal("Articles", chainedClient.Name);
        Assert.IsType<TypedDataClient<TestArticle>>(chainedClient.Data);
    }

    /// <summary>
    /// Tests that multiple instances have independent typed clients
    /// </summary>
    [Fact]
    public void MultipleInstances_HaveIndependentTypedClients()
    {
        // Arrange
        var client = CreateWeaviateClient();
        var collectionClient = new CollectionClient(client, "Articles");
        var typedClient1 = new TypedCollectionClient<TestArticle>(collectionClient);
        var typedClient2 = new TypedCollectionClient<TestArticle>(collectionClient);

        // Act & Assert
        // Each typed client should have independent wrapped clients
        Assert.NotSame(typedClient1.Data, typedClient2.Data);
        Assert.NotSame(typedClient1.Query, typedClient2.Query);
        Assert.NotSame(typedClient1.Generate, typedClient2.Generate);
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

using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;
using Weaviate.Client.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for TypedCollectionClient to ensure proper strongly-typed operations.
/// </summary>
[Collection("Unit Tests")]
public class TypedCollectionClientTests
{
    private class TestArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
    }

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

    [Fact]
    public void Constructor_WithNullCollectionClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TypedCollectionClient<TestArticle>(null!));
    }

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
        Assert.Null(tenant);
    }

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

    private static WeaviateClient CreateWeaviateClient()
    {
        // Create a minimal WeaviateClient for testing wrapper logic
        // These tests don't make actual HTTP calls
        return new WeaviateClient(new ClientConfiguration(), null, null);
    }
}

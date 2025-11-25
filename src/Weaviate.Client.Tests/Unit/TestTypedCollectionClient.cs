using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;
using Weaviate.Client.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for TypedCollectionClient to ensure proper strongly-typed operations.
/// </summary>
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

    private static WeaviateClient CreateWeaviateClient()
    {
        // Create a minimal WeaviateClient for testing wrapper logic
        // These tests don't make actual HTTP calls
        return new WeaviateClient(new ClientConfiguration(), null, null);
    }
}

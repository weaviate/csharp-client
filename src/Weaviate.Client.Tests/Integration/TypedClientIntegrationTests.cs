using Weaviate.Client.Models;
using Weaviate.Client.Typed;
using Weaviate.Client.Validation;
using Xunit;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// Integration tests for strongly-typed client operations.
/// Tests both type validation and end-to-end typed workflows.
/// </summary>
public class TypedClientIntegrationTests : IntegrationTests
{
    // Model that matches the schema
    private class Article
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public DateTime PublishedDate { get; set; }
    }

    // Model with incompatible types
    private class IncompatibleArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ViewCount { get; set; } = string.Empty; // Wrong type - should be int
        public DateTime PublishedDate { get; set; }
    }

    // Model with missing required properties
    private class IncompleteArticle
    {
        public string Title { get; set; } = string.Empty;
        // Missing Content, ViewCount, PublishedDate
    }

    private readonly CollectionCreateParams _articleConfig = new()
    {
        Name = "Articles",
        Properties = new[]
        {
            new Property { Name = "title", DataType = DataType.Text },
            new Property { Name = "content", DataType = DataType.Text },
            new Property { Name = "viewCount", DataType = DataType.Int },
            new Property { Name = "publishedDate", DataType = DataType.Date },
        },
    };

    #region Validation Scenarios

    [Fact]
    public async Task Use_WithValidationType_Compatible_Succeeds()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);

        // Act
        var typedClient = await collection.AsTyped<Article>(
            validateType: true,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(typedClient);
        Assert.Equal("Articles", typedClient.Name);
        Assert.IsType<TypedCollectionClient<Article>>(typedClient);
    }

    [Fact]
    public async Task Use_WithValidationType_IncompatibleType_ThrowsException()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await collection.AsTyped<IncompatibleArticle>(
                validateType: true,
                cancellationToken: TestContext.Current.CancellationToken
            );
        });

        Assert.Contains("not compatible", exception.Message);
        Assert.Contains("ViewCount", exception.Message);
    }

    [Fact]
    public async Task Use_WithoutValidation_AlwaysSucceeds()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);

        // Act - even with incompatible type, should not throw when validation is disabled
        var typedClient = await collection.AsTyped<IncompatibleArticle>(
            validateType: false,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(typedClient);
        Assert.IsType<TypedCollectionClient<IncompatibleArticle>>(typedClient);
    }

    [Fact]
    public async Task ValidateType_OnTypedCollectionClient_ReturnsValidationResult()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            validateType: false,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var validationResult = await typedClient.ValidateType(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public async Task ValidateType_WithIncompatibleType_ReturnsErrors()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<IncompatibleArticle>(
            validateType: false, // Skip validation on construction
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var validationResult = await typedClient.ValidateType(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.NotEmpty(validationResult.Errors);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == "ViewCount");
    }

    [Fact]
    public async Task ValidationExtension_ValidateTypeOrThrow_ThrowsOnIncompatible()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var schema = await collection.Config.GetCachedConfig(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            schema.ValidateTypeOrThrow<IncompatibleArticle>();
        });

        Assert.Contains("not compatible", exception.Message);
        Assert.Contains("Articles", exception.Message);
    }

    [Fact]
    public async Task ValidationExtension_ValidateType_ReturnsResult()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var schema = await collection.Config.GetCachedConfig(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var validationResult = schema.ValidateType<Article>();

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
    }

    #endregion

    #region Typed Workflow - CRUD Operations

    [Fact]
    public async Task TypedWorkflow_Insert_SingleObject_Succeeds()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        var article = new Article
        {
            Title = "Introduction to Weaviate",
            Content = "Weaviate is a vector database...",
            ViewCount = 100,
            PublishedDate = DateTime.UtcNow,
        };

        // Act
        var id = await typedClient.Data.Insert(
            article,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task TypedWorkflow_Insert_WithSpecificId_Succeeds()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        var article = new Article
        {
            Title = "Advanced Vector Search",
            Content = "Learn about vector search...",
            ViewCount = 250,
            PublishedDate = DateTime.UtcNow,
        };
        var expectedId = Guid.NewGuid();

        // Act
        var id = await typedClient.Data.Insert(
            article,
            id: expectedId,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal(expectedId, id);
    }

    [Fact]
    public async Task TypedWorkflow_InsertMany_MultipleObjects_Succeeds()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        var articles = new[]
        {
            new Article
            {
                Title = "Vector Embeddings",
                Content = "Understanding embeddings...",
                ViewCount = 150,
                PublishedDate = DateTime.UtcNow.AddDays(-1),
            },
            new Article
            {
                Title = "Semantic Search",
                Content = "Semantic search explained...",
                ViewCount = 200,
                PublishedDate = DateTime.UtcNow,
            },
            new Article
            {
                Title = "Hybrid Search",
                Content = "Combining keyword and vector search...",
                ViewCount = 175,
                PublishedDate = DateTime.UtcNow.AddDays(-2),
            },
        };

        // Act
        var result = await typedClient.Data.InsertMany(
            articles,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Objects.Count());
        Assert.Empty(result.Errors);
        Assert.All(result.Objects, entry => Assert.NotEqual(Guid.Empty, entry.ID!.Value));
    }

    [Fact]
    public async Task TypedWorkflow_Query_FetchObjects_ReturnsTypedResults()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Insert test data
        var article = new Article
        {
            Title = "GraphQL Queries",
            Content = "GraphQL query language...",
            ViewCount = 300,
            PublishedDate = DateTime.UtcNow,
        };
        await typedClient.Data.Insert(
            article,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var results = await typedClient.Query.FetchObjects(
            limit: 10,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results.Objects);
        Assert.All(
            results.Objects,
            obj =>
            {
                Assert.NotNull(obj.Object);
                Assert.IsType<Article>(obj.Object);
            }
        );
    }

    [Fact]
    public async Task TypedWorkflow_Query_BM25Search_ReturnsTypedResults()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Insert test data
        var articles = new[]
        {
            new Article
            {
                Title = "Machine Learning Basics",
                Content = "Introduction to ML concepts...",
                ViewCount = 500,
                PublishedDate = DateTime.UtcNow,
            },
            new Article
            {
                Title = "Deep Learning",
                Content = "Neural networks and deep learning...",
                ViewCount = 600,
                PublishedDate = DateTime.UtcNow,
            },
        };
        await typedClient.Data.InsertMany(
            articles,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var results = await typedClient.Query.BM25(
            query: "machine learning",
            limit: 10,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(results);
        Assert.All(
            results.Objects,
            obj =>
            {
                Assert.NotNull(obj.Object);
                Assert.IsType<Article>(obj.Object);
            }
        );
    }

    [Fact]
    public async Task TypedWorkflow_Update_ReplaceObject_Succeeds()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Insert initial article
        var article = new Article
        {
            Title = "Original Title",
            Content = "Original content...",
            ViewCount = 10,
            PublishedDate = DateTime.UtcNow,
        };
        var id = await typedClient.Data.Insert(
            article,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act - Replace with updated article
        var updatedArticle = new Article
        {
            Title = "Updated Title",
            Content = "Updated content...",
            ViewCount = 100,
            PublishedDate = DateTime.UtcNow,
        };
        await typedClient.Data.Replace(
            id,
            updatedArticle,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - Fetch and verify
        var result = await typedClient.Query.FetchObjectByID(
            id,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(result);
        Assert.NotNull(result.Object);
        Assert.Equal("Updated Title", result.Object.Title);
        Assert.Equal(100, result.Object.ViewCount);
    }

    [Fact]
    public async Task TypedWorkflow_Delete_ById_Succeeds()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Insert test article
        var article = new Article
        {
            Title = "To Be Deleted",
            Content = "This will be deleted...",
            ViewCount = 1,
            PublishedDate = DateTime.UtcNow,
        };
        var id = await typedClient.Data.Insert(
            article,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act - Delete
        await typedClient.Data.DeleteByID(id, TestContext.Current.CancellationToken);

        // Assert - Verify deletion
        var result = await typedClient.Query.FetchObjectByID(
            id,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Null(result);
    }

    [Fact]
    public async Task TypedWorkflow_Count_ReturnsObjectCount()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Insert test data
        var articles = new[]
        {
            new Article
            {
                Title = "Article 1",
                Content = "Content 1",
                ViewCount = 10,
                PublishedDate = DateTime.UtcNow,
            },
            new Article
            {
                Title = "Article 2",
                Content = "Content 2",
                ViewCount = 20,
                PublishedDate = DateTime.UtcNow,
            },
            new Article
            {
                Title = "Article 3",
                Content = "Content 3",
                ViewCount = 30,
                PublishedDate = DateTime.UtcNow,
            },
        };
        await typedClient.Data.InsertMany(
            articles,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var count = await typedClient.Count(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3ul, count);
    }

    [Fact]
    public async Task TypedWorkflow_Iterator_IteratesAllObjects()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Insert test data
        var articles = new[]
        {
            new Article
            {
                Title = "Iterate 1",
                Content = "Content 1",
                ViewCount = 1,
                PublishedDate = DateTime.UtcNow,
            },
            new Article
            {
                Title = "Iterate 2",
                Content = "Content 2",
                ViewCount = 2,
                PublishedDate = DateTime.UtcNow,
            },
            new Article
            {
                Title = "Iterate 3",
                Content = "Content 3",
                ViewCount = 3,
                PublishedDate = DateTime.UtcNow,
            },
        };
        await typedClient.Data.InsertMany(
            articles,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var iteratedObjects = new List<Article>();
        await foreach (
            var obj in typedClient.Iterator(
                cacheSize: 2,
                cancellationToken: TestContext.Current.CancellationToken
            )
        )
        {
            Assert.NotNull(obj.Object);
            iteratedObjects.Add(obj.Object);
        }

        // Assert
        Assert.Equal(3, iteratedObjects.Count);
        Assert.Contains(iteratedObjects, a => a.Title == "Iterate 1");
        Assert.Contains(iteratedObjects, a => a.Title == "Iterate 2");
        Assert.Contains(iteratedObjects, a => a.Title == "Iterate 3");
    }

    [Fact]
    public async Task TypedWorkflow_WithTenant_CreatesNewTypedClient()
    {
        // Arrange
        var multiTenancyConfig = new MultiTenancyConfig { Enabled = true };
        var collection = await CollectionFactory<Article>(
            name: MakeUniqueCollectionName<Article>("tenant_test"),
            properties: _articleConfig.Properties,
            multiTenancyConfig: multiTenancyConfig
        );

        // Add tenant
        await collection.Tenants.Create(
            new[] { new Tenant { Name = "tenant1" } },
            TestContext.Current.CancellationToken
        );

        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var tenantClient = typedClient.WithTenant("tenant1");

        // Assert
        Assert.NotEqual(typedClient, tenantClient);
        Assert.Equal("tenant1", tenantClient.Tenant);
        Assert.IsType<TypedCollectionClient<Article>>(tenantClient);
    }

    [Fact]
    public async Task TypedWorkflow_WithConsistencyLevel_CreatesNewTypedClient()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var consistentClient = typedClient.WithConsistencyLevel(ConsistencyLevels.Quorum);

        // Assert
        Assert.NotEqual(typedClient, consistentClient);
        Assert.Equal(ConsistencyLevels.Quorum, consistentClient.ConsistencyLevel);
        Assert.IsType<TypedCollectionClient<Article>>(consistentClient);
    }

    [Fact]
    public async Task TypedWorkflow_Untyped_ReturnsUnderlyingCollectionClient()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act
        var untypedClient = typedClient.Untyped;

        // Assert
        Assert.NotNull(untypedClient);
        Assert.IsType<CollectionClient>(untypedClient);
        Assert.Equal(typedClient.Name, untypedClient.Name);
    }

    #endregion

    #region Type Safety

    [Fact]
    public async Task TypeSafety_CompileTimeTypeChecking_EnforcesCorrectTypes()
    {
        // Arrange
        var collection = await CollectionFactory<Article>(_articleConfig);
        var typedClient = await collection.AsTyped<Article>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act & Assert - This test verifies compile-time type safety
        // The fact that this compiles proves the type system works correctly
        Article article = new()
        {
            Title = "Type Safe",
            Content = "Compile-time safety",
            ViewCount = 1,
            PublishedDate = DateTime.UtcNow,
        };

        var id = await typedClient.Data.Insert(
            article,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotEqual(Guid.Empty, id);

        // Query returns strongly-typed results
        var results = await typedClient.Query.FetchObjects(
            limit: 10,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Type checking at compile time
        foreach (var obj in results.Objects)
        {
            // Object property is strongly-typed as Article
            string title = obj.Object.Title; // No casting needed
            int viewCount = obj.Object.ViewCount; // No casting needed
            Assert.NotNull(title);
            Assert.True(viewCount >= 0);
        }
    }

    #endregion
}

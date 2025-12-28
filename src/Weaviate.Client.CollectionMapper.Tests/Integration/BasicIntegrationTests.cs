using Weaviate.Client.CollectionMapper.Attributes;
using Weaviate.Client.CollectionMapper.Extensions;
using Weaviate.Client.Models;
using Xunit;

namespace Weaviate.Client.CollectionMapper.Tests.Integration;

/// <summary>
/// Basic integration tests for CollectionMapper end-to-end scenarios.
///
/// To run these tests:
/// 1. Start Weaviate: docker-compose -f docker-compose.integration.yml up -d
/// 2. Run tests: dotnet test --filter "Category=Integration"
/// 3. Stop Weaviate: docker-compose -f docker-compose.integration.yml down
/// </summary>
[Trait("Category", "Integration")]
public class BasicIntegrationTests : IntegrationTestBase
{
    #region Test Models

    [WeaviateCollection]
    public class Article
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = "";

        [Property(DataType.Text)]
        public string Content { get; set; } = "";

        [Property(DataType.Int)]
        public int WordCount { get; set; }

        [Property(DataType.Date)]
        public DateTime PublishedAt { get; set; }

        [Property(DataType.Bool)]
        public bool IsPublished { get; set; }

        [Vector<Vectorizer.Text2VecTransformers>(TextFields = new[] { "title", "content" })]
        public float[]? Embedding { get; set; }
    }

    #endregion

    [Fact]
    public async Task CreateCollection_FromClass_Success()
    {
        // Arrange
        var collectionName = GetTestCollectionName(nameof(Article));

        // Act
        var collection = await Client.Collections.CreateFromClass<Article>();

        // Assert
        Assert.NotNull(collection);
        Assert.Equal(collectionName, collection.Name);

        // Verify collection exists
        var exists = await Client.Collections.Exists(collectionName);
        Assert.True(exists);

        // Verify schema structure
        var config = await Client.Collections.Export(collectionName);
        Assert.NotNull(config);
        Assert.Equal(5, config.Properties.Length); // 5 properties
        Assert.Single(config.VectorConfig); // 1 vector config
    }

    [Fact]
    public async Task Insert_SingleObject_Success()
    {
        // Arrange
        await Client.Collections.CreateFromClass<Article>();
        var collection = Client.Collections.Use(GetTestCollectionName(nameof(Article)));

        var article = new Article
        {
            Title = "Test Article",
            Content = "This is test content for integration testing.",
            WordCount = 7,
            PublishedAt = DateTime.UtcNow,
            IsPublished = true,
        };

        // Act
        var result = await collection.Data.Insert(article);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        // Verify object was inserted
        var fetchResult = await collection.Query.FetchObjects(limit: 10);
        Assert.Single(fetchResult.Objects);

        var retrieved = fetchResult.Objects[0];
        Assert.Equal("Test Article", retrieved.Properties?["title"]?.ToString());
        Assert.Equal(7L, (long?)retrieved.Properties?["wordCount"]);
        Assert.True((bool?)retrieved.Properties?["isPublished"]);
    }

    [Fact]
    public async Task InsertMany_MultipleObjects_Success()
    {
        // Arrange
        await Client.Collections.CreateFromClass<Article>();
        var collection = Client.Collections.Use(GetTestCollectionName(nameof(Article)));

        var articles = new List<Article>
        {
            new()
            {
                Title = "Article 1",
                Content = "Content 1",
                WordCount = 10,
                PublishedAt = DateTime.UtcNow,
                IsPublished = true,
            },
            new()
            {
                Title = "Article 2",
                Content = "Content 2",
                WordCount = 20,
                PublishedAt = DateTime.UtcNow,
                IsPublished = false,
            },
            new()
            {
                Title = "Article 3",
                Content = "Content 3",
                WordCount = 30,
                PublishedAt = DateTime.UtcNow,
                IsPublished = true,
            },
        };

        // Act
        var results = await collection.Data.InsertMany(articles);

        // Assert - Check that all objects were returned with IDs
        Assert.Equal(3, results.Objects.Count());
        Assert.All(
            results.Objects,
            entry =>
            {
                Assert.NotNull(entry.ID);
                Assert.NotEqual(Guid.Empty, entry.ID);
                Assert.Null(entry.Error); // No errors
            }
        );

        // Verify all objects were inserted
        var fetchResult = await collection.Query.FetchObjects(limit: 10);
        Assert.Equal(3, fetchResult.Objects.Count);
    }

    [Fact]
    public async Task Delete_RemoveObject_Success()
    {
        // Arrange
        await Client.Collections.CreateFromClass<Article>();
        var collection = Client.Collections.Use(GetTestCollectionName(nameof(Article)));

        var articles = new List<Article>
        {
            new()
            {
                Title = "Keep",
                Content = "A",
                WordCount = 10,
                PublishedAt = DateTime.UtcNow,
                IsPublished = true,
            },
            new()
            {
                Title = "Delete",
                Content = "B",
                WordCount = 20,
                PublishedAt = DateTime.UtcNow,
                IsPublished = true,
            },
        };

        var results = await collection.Data.InsertMany(articles);
        var deleteId = results.Objects.ElementAt(1).ID!.Value; // Second article ID

        // Act
        await collection.Data.DeleteByID(deleteId);

        // Assert
        var remaining = await collection.Query.FetchObjects(limit: 10);
        Assert.Single(remaining.Objects);
        Assert.Equal("Keep", remaining.Objects[0].Properties?["title"]?.ToString());
    }
}

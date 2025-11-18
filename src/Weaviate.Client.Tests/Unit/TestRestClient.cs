using System.Net;
using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using Dto = Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Example tests demonstrating how to unit test WeaviateClient REST operations
/// using the mock HTTP infrastructure.
/// </summary>
public partial class RestClientTests
{
    [Fact]
    public async Task Create_Collection_SendsCorrectRequest()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        // Mock the response using actual DTO classes from Models.g.cs
        var mockResponse = new Dto.Class
        {
            Class1 = "Article",
            Description = "A news article",
            Properties =
            [
                new Dto.Property
                {
                    Name = "title",
                    DataType = ["text"],
                    Description = "Article title",
                },
            ],
            Vectorizer = "none",
        };

        handler.AddJsonResponse(mockResponse);

        var config = new CollectionConfig
        {
            Name = "Article",
            Description = "A news article",
            Properties = [Property.Text("title") with { Description = "Article title" }],
        };

        // Act
        var result = await client.Collections.Create(config, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(handler.LastRequest);

        // Verify HTTP method and path
        handler.LastRequest!.ShouldHaveMethod(HttpMethod.Post).ShouldHavePath("/v1/schema");

        // Verify request body contains expected JSON fields
        var requestBody = await handler.LastRequest.GetBodyAsString();
        Assert.Contains("Article", requestBody);
        Assert.Contains("A news article", requestBody);
        Assert.Contains("properties", requestBody);

        // Verify the result
        Assert.NotNull(result);
        Assert.Equal("Article", result.Name);
    }

    [Fact]
    public async Task Create_Collection_WithTypedData_InfersProperties()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        // Return proper DTO with inferred properties
        var mockResponse = new Dto.Class
        {
            Class1 = "Product",
            Properties =
            [
                new Dto.Property { Name = "name", DataType = ["text"] },
                new Dto.Property { Name = "price", DataType = ["number"] },
                new Dto.Property { Name = "inStock", DataType = ["boolean"] },
            ],
            Vectorizer = "none",
        };

        handler.AddJsonResponse(mockResponse);

        var config = new CollectionConfig { Name = "Product" };

        // Act
        var result = await client.Collections.Create<ProductData>(
            config,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(handler.LastRequest);
        handler.LastRequest!.ShouldHaveMethod(HttpMethod.Post);

        // Verify the typed collection client is returned
        Assert.NotNull(result);
        Assert.Equal("Product", result.Name);
    }

    [Fact]
    public async Task Create_Collection_WithCustomHandler_AllowsDynamicAssertions()
    {
        // Arrange
        var capturedRequestBody = string.Empty;

        var client = MockWeaviateClient.CreateWithHandler(async request =>
        {
            // Capture request for assertions
            if (request.Content != null)
            {
                capturedRequestBody = await request.Content.ReadAsStringAsync(
                    TestContext.Current.CancellationToken
                );
            }

            // Return mock response using actual DTO
            var mockResponse = JsonSerializer.Serialize(
                new Dto.Class
                {
                    Class1 = "TestClass",
                    Vectorizer = "none",
                    Properties = [],
                },
                Rest.WeaviateRestClient.RestJsonSerializerOptions
            );

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    mockResponse,
                    System.Text.Encoding.UTF8,
                    "application/json"
                ),
            };
        });

        var config = new CollectionConfig
        {
            Name = "TestClass",
            VectorConfig = Configure.Vectors.SelfProvided().New(),
        };

        // Act
        await client.Collections.Create(config, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(capturedRequestBody);
        Assert.Contains("TestClass", capturedRequestBody);
        Assert.Contains("vectorizer", capturedRequestBody);
    }

    [Fact]
    public async Task Create_Collection_WithMultipleProperties_SerializesCorrectly()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        // Return proper DTO matching the expected structure
        var mockResponse = new Dto.Class
        {
            Class1 = "Person",
            Properties =
            [
                new Dto.Property { Name = "name", DataType = ["text"] },
                new Dto.Property { Name = "age", DataType = ["int"] },
                new Dto.Property { Name = "email", DataType = ["text"] },
                new Dto.Property { Name = "isActive", DataType = ["boolean"] },
            ],
        };

        handler.AddJsonResponse(mockResponse);

        var config = new CollectionConfig
        {
            Name = "Person",
            Properties =
            [
                Property.Text("name"),
                Property.Int("age"),
                Property.Text("email"),
                Property.Bool("isActive"),
            ],
        };

        // Act
        await client.Collections.Create(config, TestContext.Current.CancellationToken);

        // Assert
        var requestBody = await handler.LastRequest!.GetBodyAsString();

        // Verify all properties are in the request
        Assert.Contains("name", requestBody);
        Assert.Contains("age", requestBody);
        Assert.Contains("email", requestBody);
        Assert.Contains("isActive", requestBody);
    }

    [Fact]
    public async Task Delete_Collection_SendsCorrectRequest()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        handler.AddResponse(MockResponses.Ok());

        // Act
        await client.Collections.Delete("Article", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/schema/Article");
    }

    [Fact]
    public async Task Exists_Collection_SendsGetRequest()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        // Return schema with the collection using actual Schema DTO
        var mockSchema = new Dto.Schema
        {
            Classes = [new Dto.Class { Class1 = "Article", Properties = [] }],
        };

        handler.AddJsonResponse(mockSchema);

        // Act
        var exists = await client.Collections.Exists(
            "Article",
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.True(exists);
        Assert.NotNull(handler.LastRequest);
        handler.LastRequest!.ShouldHaveMethod(HttpMethod.Get).ShouldHavePath("/v1/schema");
    }

    [Fact]
    public async Task List_Collections_ParsesMultipleCollections()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        // Return schema with multiple collections using actual Schema DTO
        var mockSchema = new Dto.Schema
        {
            Classes =
            [
                new Dto.Class { Class1 = "Article", Properties = [] },
                new Dto.Class { Class1 = "Author", Properties = [] },
                new Dto.Class { Class1 = "Category", Properties = [] },
            ],
        };

        handler.AddJsonResponse(mockSchema);

        // Act
        var collections = await client
            .Collections.List(TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, collections.Count);
        Assert.Contains(collections, c => c.Name == "Article");
        Assert.Contains(collections, c => c.Name == "Author");
        Assert.Contains(collections, c => c.Name == "Category");

        // Verify request
        handler.LastRequest!.ShouldHaveMethod(HttpMethod.Get).ShouldHavePath("/v1/schema");
    }

    [Fact]
    public async Task Create_Collection_WithVectorConfig_SerializesVectorConfig()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        // Return proper DTO with vectorConfig structure matching openapi.json spec
        var mockResponse = new Dto.Class
        {
            Class1 = "Document",
            VectorConfig = new Dictionary<string, Dto.VectorConfig>
            {
                ["Default"] = new Dto.VectorConfig
                {
                    Vectorizer = new Dictionary<string, object> { ["none"] = new { } },
                    VectorIndexType = "hnsw",
                    VectorIndexConfig = new Dictionary<string, object>
                    {
                        ["distance"] = "cosine",
                        ["ef"] = 100,
                        ["efConstruction"] = 128,
                    },
                },
            },
        };

        handler.AddJsonResponse(mockResponse);

        var config = new CollectionConfig
        {
            Name = "Document",
            VectorConfig = Configure.Vectors.SelfProvided().New(),
        };

        // Act
        await client.Collections.Create(config, TestContext.Current.CancellationToken);

        // Assert
        var requestBody = await handler.LastRequest!.GetBodyAsString();
        // Verify vectorConfig structure is present
        Assert.Contains("vectorConfig", requestBody);
    }

    [Fact]
    public async Task Multiple_Sequential_Requests_WorkCorrectly()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        // Queue multiple responses using actual DTOs
        handler.AddJsonResponse(new Dto.Class { Class1 = "First", Properties = [] });
        handler.AddJsonResponse(new Dto.Class { Class1 = "Second", Properties = [] });
        handler.AddJsonResponse(new Dto.Class { Class1 = "Third", Properties = [] });

        // Act
        await client.Collections.Create(
            new CollectionConfig { Name = "First" },
            TestContext.Current.CancellationToken
        );
        await client.Collections.Create(
            new CollectionConfig { Name = "Second" },
            TestContext.Current.CancellationToken
        );
        await client.Collections.Create(
            new CollectionConfig { Name = "Third" },
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal(3, handler.Requests.Count);
        Assert.All(handler.Requests, req => req.ShouldHaveMethod(HttpMethod.Post));
    }

    // Helper class for typed data test
    private class ProductData
    {
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool InStock { get; set; }
    }
}

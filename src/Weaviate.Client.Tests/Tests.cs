using Weaviate.Client.Models;

namespace Weaviate.Client.Tests;

internal class TestData
{
    public string Name { get; set; } = string.Empty;
}

internal class TestDataValue
{
    public string Value { get; set; } = string.Empty;
}

[Collection("BasicTests")]
public class WeaviateClientTest : IDisposable
{
    WeaviateClient _weaviate;

    public WeaviateClientTest()
    {
        _weaviate = new WeaviateClient();
    }

    public void Dispose()
    {
        _weaviate.Dispose();
    }

    async Task<CollectionClient<TData>> CollectionFactory<TData>(string name, string description, IList<Property> properties, IDictionary<string, VectorConfig>? vectorConfig = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = TestContext.Current.TestMethod?.MethodName ?? string.Empty;
        }

        ArgumentException.ThrowIfNullOrEmpty(name);

        if (vectorConfig is null)
        {
            vectorConfig = new Dictionary<string, VectorConfig>
            {
                {
                    "default", new VectorConfig {
                        Vectorizer = new Dictionary<string, object> { { "none", new { } } },
                        VectorIndexType = "hnsw"
                    }
                }
            };
        }

        var c = new Collection
        {
            Name = name,
            Description = description,
            Properties = properties,
            VectorConfig = vectorConfig,
        };

        await _weaviate.Collections.Delete(name);

        var collectionClient = await _weaviate.Collections.Create<TData>(c);

        return collectionClient;
    }

    async Task<CollectionClient<dynamic>> CollectionFactory(string name, string description, IList<Property> properties, IDictionary<string, VectorConfig>? vectorConfig = null)
    {
        return await CollectionFactory<dynamic>(name, description, properties, vectorConfig);
    }

    WeaviateObject<TData> DataFactory<TData>(TData value)
    {
        return new WeaviateObject<TData>()
        {
            Data = value
        };
    }

    [Fact]
    public async Task TestBasicCollectionCreation()
    {
        // Arrange

        // Act
        var collectionClient = await CollectionFactory("", "Test collection description", [
            Property.Text("Name")
        ]);

        // Assert
        var collection = await _weaviate.Collections.Use<dynamic>(collectionClient.Name).Get();
        Assert.NotNull(collection);
        Assert.Equal("TestBasicCollectionCreation", collection.Name);
        Assert.Equal("Test collection description", collection.Description);
    }

    [Fact]
    public async Task TestBasicObjectCreation()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestData>("", "Test collection description", [
            Property.Text("Name")
        ]);

        // Act
        var id = Guid.NewGuid();
        var obj = await collectionClient.Data.Insert(new WeaviateObject<TestData>()
        {
            Data = new TestData { Name = "TestObject" },
            ID = id,
        });

        // Assert

        // Assert object exists
        var retrieved = await collectionClient.Query.FetchObjectByID(id);
        Assert.NotNull(retrieved);
        Assert.Equal(id, retrieved.ID);
        Assert.Equal("TestObject", retrieved.Data?.Name);

        // Delete after usage
        await collectionClient.Data.Delete(id);
        retrieved = await collectionClient.Query.FetchObjectByID(id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task TestBasicNearVectorSearch()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestData>("", "Test collection description", [
            Property.Text("Name")
        ]);

        // Act
        await collectionClient.Data.Insert(new WeaviateObject<TestData>()
        {
            Data = new TestData { Name = "TestObject1" },
            Vectors = new Dictionary<string, IList<float>>
            {
                { "default", new float[] { 0.1f, 0.2f, 0.3f } }
            }
        });

        await collectionClient.Data.Insert(new WeaviateObject<TestData>()
        {
            Data = new TestData { Name = "TestObject2" },
            Vectors = new Dictionary<string, IList<float>>
            {
                { "default", new float[] { 0.3f, 0.4f, 0.5f } }
            }
        });

        await collectionClient.Data.Insert(new WeaviateObject<TestData>()
        {
            Data = new TestData { Name = "TestObject3" },
            Vectors = new Dictionary<string, IList<float>>
            {
                { "default", new float[] { 0.5f, 0.6f, 0.7f } }
            }
        });

        // Assert
        var retrieved = collectionClient.Query.NearVector(new float[] { 0.1f, 0.2f, 0.3f });
        Assert.NotNull(retrieved);

        await foreach (var obj in retrieved)
        {
            Assert.Equal("TestObject1", obj.Data!.Name);
            break;
        }
    }

    [Fact]
    public async Task TestBasicNearTextSearch()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestDataValue>("", "Test collection description", [
            Property.Text("value")
        ], new Dictionary<string, VectorConfig>
        {
            {
                "default", new VectorConfig
                {
                    Vectorizer = new Dictionary<string, object> {
                        {
                            "text2vec-contextionary", new {
                                vectorizeClassName = false
                            }
                        }
                    },
                    VectorIndexType = "hnsw"
                }
            }
        });

        string[] values = ["Apple", "Mountain climbing", "apple cake", "cake"];
        var tasks = values.Select(s => new TestDataValue { Value = s }).Select(DataFactory).Select(collectionClient.Data.Insert);
        Guid[] guids = await Task.WhenAll(tasks);
        var concepts = "hiking";

        // Act
        var retriever = collectionClient.Query.NearText(
            "cake",
            moveTo: new Move(1.0f, objects: guids[0]),
            moveAway: new Move(0.5f, concepts: concepts),
            fields: ["value"]
        );
        var retrieved = await retriever.ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(4, retrieved.Count());

        Assert.Equal(retrieved[0].ID, guids[2]);
        Assert.Contains("default", retrieved[0].Vectors.Keys);
        Assert.Equal("apple cake", retrieved[0].Data?.Value);
    }

    [Fact]
    public async Task TestBasicNearTextGroupBySearch()
    {
        // Arrange
        CollectionClient<dynamic>? collectionClient = await CollectionFactory("", "Test collection description", [
            Property.Text("value")
        ], new Dictionary<string, VectorConfig>
        {
            {
                "default", new VectorConfig
                {
                    Vectorizer = new Dictionary<string, object> {
                        {
                            "text2vec-contextionary", new {
                                vectorizeClassName = false
                            }
                        }
                    },
                    VectorIndexType = "hnsw"
                }
            }
        });

        string[] values = ["Apple", "Mountain climbing", "apple cake", "cake"];
        var tasks = values.Select(s => new { Value = s }).Select(DataFactory<dynamic>).Select(collectionClient.Data.Insert);
        Guid[] guids = await Task.WhenAll(tasks);

        // Act
        var retrieved = await collectionClient.Query.NearText(
            "cake",
            new GroupByConstraint
            {
                PropertyName = "value",
                NumberOfGroups = 2,
                ObjectsPerGroup = 100,
            }
        );

        // Assert
        Assert.NotNull(retrieved.Objects);
        Assert.NotNull(retrieved.Groups);

        var retrievedObjects = retrieved.Objects.ToArray();

        Assert.Equal(2, retrieved.Objects.Count());
        Assert.Equal(2, retrieved.Groups.Count());

        var obj = await collectionClient.Query.FetchObjectByID(guids[3]);
        Assert.NotNull(obj);
        Assert.Equal(guids[3], obj.ID);
        Assert.Contains("default", obj.Vectors.Keys);

        Assert.Equal(guids[3], retrievedObjects[0].ID);
        Assert.Contains("default", retrievedObjects[0].Vectors.Keys);
        Assert.Equal("cake", retrievedObjects[0].BelongsToGroup);
        Assert.Equal(guids[2], retrievedObjects[1].ID);
        Assert.Contains("default", retrievedObjects[1].Vectors.Keys);
        Assert.Equal("apple cake", retrievedObjects[1].BelongsToGroup);
    }
}

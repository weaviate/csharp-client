using System.Numerics;
using Weaviate.Client.Grpc;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests;

internal class TestData
{
    public string Name { get; set; } = string.Empty;
}

[Collection("BasicTests")]
public class WeaviateClientTest : IDisposable
{
    WeaviateClient _weaviate;

    public WeaviateClientTest()
    {
        _weaviate = new WeaviateClient();
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
        var collectionClient = await CollectionFactory("", "Test collection description", [
            Property.Text("Name")
        ]);

        // Act
        await collectionClient.Data.Insert(new WeaviateObject<dynamic>()
        {
            Data = new { Name = "TestObject1" },
            Vectors = new Dictionary<string, IEnumerable<float>>
            {
                { "default", new float[] { 0.1f, 0.2f, 0.3f } }
            }
        });

        await collectionClient.Data.Insert(new WeaviateObject<dynamic>()
        {
            Data = new TestData { Name = "TestObject2" },
            Vectors = new Dictionary<string, IEnumerable<float>>
            {
                { "default", new float[] { 0.3f, 0.4f, 0.5f } }
            }
        });

        await collectionClient.Data.Insert(new WeaviateObject<dynamic>()
        {
            Data = new TestData { Name = "TestObject3" },
            Vectors = new Dictionary<string, IEnumerable<float>>
            {
                { "default", new float[] { 0.5f, 0.6f, 0.7f } }
            }
        });

        // Assert
        var retrieved = collectionClient.Query.NearVector(new float[] { 0.1f, 0.2f, 0.3f });
        Assert.NotNull(retrieved);

        await foreach (var obj in retrieved)
        {
            var lobj = obj.ToWeaviateObject<TestData>();
            Assert.Equal("TestObject1", lobj.Data!.Name);
            break;
        }
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
                    Vectorizer = new Dictionary<string, object> { { "text2vec-contextionary", new {
                        vectorizeClassName = false
                     } } },
                    VectorIndexType = "hnsw"
                }
            }
        });

        Guid[] objects = [
            await collectionClient.Data.Insert(new WeaviateObject<dynamic>()
            {
                Data = new { Value = "Apple" },
            }),

            await collectionClient.Data.Insert(new WeaviateObject<dynamic>()
            {
                Data = new { Value = "Mountain climbing" },
            }),

            await collectionClient.Data.Insert(new WeaviateObject<dynamic>()
            {
                Data = new { Value = "apple cake" },
            }),

            await collectionClient.Data.Insert(new WeaviateObject<dynamic>()
            {
                Data = new { Value = "cake" },
            })
        ];

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
        Assert.NotNull(retrieved.Item1);
        Assert.NotNull(retrieved.Item2);

        var retrievedObjects = retrieved.Item1.ToArray();

        Assert.Equal(2, retrieved.Item1.Count());
        Assert.Equal(2, retrieved.Item2.Count());

        var obj = await collectionClient.Query.FetchObjectByID(objects[3]);
        Assert.NotNull(obj);
        Assert.Equal(objects[3], obj.ID);
        Assert.Contains("default", obj.Vectors.Keys);

        Assert.Equal(objects[3], retrievedObjects[0].ID);
        Assert.Contains("default", retrievedObjects[0].Vectors.Keys);
        Assert.Equal("cake", retrievedObjects[0].BelongsToGroup);
        Assert.Equal(objects[2], retrievedObjects[1].ID);
        Assert.Contains("default", retrievedObjects[1].Vectors.Keys);
        Assert.Equal("apple cake", retrievedObjects[1].BelongsToGroup);
    }

    public void Dispose()
    {
        _weaviate.Dispose();
    }
}
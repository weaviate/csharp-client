using Weaviate.Client.Models;

// using TestData = dynamic;

namespace Weaviate.Client.Tests;

internal class TestData
{
    public string Name { get; set; } = string.Empty;
}

[Collection("BasicTests")]
public class WeaviateClientTest
{
    [Fact]
    public async ValueTask TestBasicCollectionCreation()
    {
        // Arrange
        var vectorizerConfigNone = new VectorConfig
        {
            Vectorizer = new Dictionary<string, object>
            {
                { "none", new object { } }
            },
            VectorIndexType = "hnsw",
        };

        var VectorConfigs = new Dictionary<string, VectorConfig>
        {
            { "default", vectorizerConfigNone }
        };

        // Act
        var weaviate = new WeaviateClient();

        var testName = TestContext.Current.TestMethod?.MethodName ?? "TestNameNotFound";

        await weaviate.Collections.Use<TestData>("TestCollection").Delete();

        await weaviate.Collections.Create<TestData>(c =>
        {
            c.Name = "TestCollection";
            c.Description = "Test collection description";
            c.Properties = [Property.Text("Name")];
            c.VectorConfig = VectorConfigs;
        });

        // Assert
        var collection = await weaviate.Collections.Use<TestData>("TestCollection").Get();
        Assert.NotNull(collection);
        Assert.Equal("TestCollection", collection.Name);
        Assert.Equal("Test collection description", collection.Description);
    }

    [Fact]
    public async Task TestBasicObjectCreation()
    {
        var vectorizerConfigNone = new VectorConfig
        {
            Vectorizer = new Dictionary<string, object>
            {
                { "none", new object { } }
            },
            VectorIndexType = "hnsw",
        };

        var VectorConfigs = new Dictionary<string, VectorConfig>
        {
            { "default", vectorizerConfigNone }
        };

        var weaviate = new WeaviateClient();

        // Delete any existing "TestCollection2" class
        await weaviate.Collections.Use<TestData>("TestCollection2").Delete();

        var collection = await weaviate.Collections.Create<TestData>(c =>
         {
             c.Name = "TestCollection2";
             c.Description = "Test collection description";
             c.Properties = [Property.Text("Name")];
             c.VectorConfig = VectorConfigs;
         });

        // Create an object in the collection
        var id = Guid.NewGuid();
        var obj = await collection.Data.Insert(new WeaviateObject<TestData>()
        {
            Data = new TestData { Name = "TestObject" },
            ID = id,
        });

        // Assert object exists
        var retrieved = await collection.Query.FetchObjectByID(id);
        Assert.NotNull(retrieved);
        Assert.Equal("TestObject", retrieved.Data?.Name);
        Assert.Equal(id, retrieved.ID);

        // delete after usage
        await collection.Data.Delete(id);
        retrieved = await collection.Query.FetchObjectByID(id);
        Assert.Null(retrieved);
   }



    [Fact]
    public async Task TestBasicNearVectorSearch()
    {
        var vectorizerConfigNone = new VectorConfig
        {
            Vectorizer = new Dictionary<string, object>
            {
                { "none", new object { } }
            },
            VectorIndexType = "hnsw",
        };

        var VectorConfigs = new Dictionary<string, VectorConfig>
        {
            { "default", vectorizerConfigNone }
        };

        var weaviate = new WeaviateClient();

        // Delete any existing "TestCollection2" class
        await weaviate.Collections.Use<TestData>("TestCollection3").Delete();

        var collection = await weaviate.Collections.Create<TestData>(c =>
         {
             c.Name = "TestCollection3";
             c.Description = "Test collection description";
             c.Properties = [Property.Text("Name")];
             c.VectorConfig = VectorConfigs;
         });

        // Create an object in the collection
        await collection.Data.Insert(new WeaviateObject<TestData>()
        {
            Data = new TestData { Name = "TestObject1" },
            Vectors = new Dictionary<string, IEnumerable<float>>
            {
                { "default", new float[] { 0.1f, 0.2f, 0.3f } }
            }
        });

        await collection.Data.Insert(new WeaviateObject<TestData>()
        {
            Data = new TestData { Name = "TestObject2" },
            Vectors = new Dictionary<string, IEnumerable<float>>
            {
                { "default", new float[] { 0.3f, 0.4f, 0.5f } }
            }
        });

        await collection.Data.Insert(new WeaviateObject<TestData>()
        {
            Data = new TestData { Name = "TestObject3" },
            Vectors = new Dictionary<string, IEnumerable<float>>
            {
                { "default", new float[] { 0.5f, 0.6f, 0.7f } }
            }
        });

        // Assert object exists
        var retrieved = collection.Query.NearVector(new float[] { 0.1f, 0.2f, 0.3f });
        Assert.NotNull(retrieved);

        await foreach (var obj in retrieved)
        {
            var lobj = obj.ToWeaviateObject<TestData>();
            Assert.Equal("TestObject1", lobj.Data!.Name);
            break;
        }
    }
}
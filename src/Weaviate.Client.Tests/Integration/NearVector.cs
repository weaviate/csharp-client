using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{

    [Fact]
    public async Task NearVectorSearch()
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
}
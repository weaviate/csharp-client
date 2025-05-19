using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
    [Fact]
    public async Task NearVectorSearch()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestData>(
            "",
            "Test collection description",
            [Property.Text("Name")]
        );

        // Act
        await collectionClient.Data.Insert(
            new TestData { Name = "TestObject1" },
            vectors: new NamedVectors { { "default", 0.1f, 0.2f, 0.3f } }
        );

        await collectionClient.Data.Insert(
            new TestData { Name = "TestObject2" },
            vectors: new NamedVectors { { "default", 0.3f, 0.4f, 0.5f } }
        );

        await collectionClient.Data.Insert(
            data: new TestData { Name = "TestObject3" },
            vectors: new NamedVectors { { "default", 0.5f, 0.6f, 0.7f } }
        );

        // Assert
        var retrieved = await collectionClient.Query.NearVector([0.1f, 0.2f, 0.3f]);
        Assert.NotNull(retrieved);
        Assert.NotEmpty(retrieved.Objects);

        Assert.Equal("TestObject1", retrieved.Objects.First().As<TestData>()?.Name);
    }
}

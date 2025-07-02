using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class SearchTests
{
    [Fact]
    public async Task NearVectorSearch()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestData>(
            "NearVectorSearch3vecs",
            "Test collection description"
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

        var objs = await collectionClient.Query.List(metadata: MetadataOptions.Vector);

        // Assert
        var retrieved = await collectionClient.Query.NearVector([0.1f, 0.2f, 0.3f]);
        Assert.NotNull(retrieved);
        Assert.NotEmpty(retrieved.Objects);

        Assert.Equal("TestObject1", retrieved.Objects.First().As<TestData>()?.Name);
    }
}

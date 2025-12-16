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
            vectors: new[] { 0.1f, 0.2f, 0.3f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await collectionClient.Data.Insert(
            new TestData { Name = "TestObject2" },
            vectors: new[] { 0.3f, 0.4f, 0.5f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await collectionClient.Data.Insert(
            properties: new TestData { Name = "TestObject3" },
            vectors: new[] { 0.5f, 0.6f, 0.7f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        var retrieved = await collectionClient.Query.NearVector(
            new[] { 0.1f, 0.2f, 0.3f },
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(retrieved);
        Assert.NotEmpty(retrieved.Objects);

        Assert.Equal("TestObject1", retrieved.Objects.First().As<TestData>()?.Name);
    }
}

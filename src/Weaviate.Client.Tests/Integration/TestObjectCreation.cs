using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests : IntegrationTests
{
    [Fact]
    public async Task ObjectCreation()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestData>("", "Test collection description");

        // Act
        var id = Guid.NewGuid();
        var obj = await collectionClient.Data.Insert(
            new TestData() { Name = "TestObject" },
            id: id
        );

        // Assert

        // Assert object exists
        var retrieved = await collectionClient.Query.FetchObjectByID(id);

        Assert.NotNull(retrieved);
        Assert.Equal(id, retrieved.ID);
        Assert.Equal("TestObject", retrieved.Properties["name"]);
        Assert.Equal("TestObject", retrieved.As<TestData>()?.Name);

        // Delete after usage
        await collectionClient.Data.Delete(id);
        retrieved = await collectionClient.Query.FetchObjectByID(id);
        Assert.Null(retrieved);
    }
}

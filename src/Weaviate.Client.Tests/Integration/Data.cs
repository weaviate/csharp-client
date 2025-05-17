using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
    [Fact]
    public async Task CollectionCreation()
    {
        // Arrange

        // Act
        var collectionClient = await CollectionFactory(
            "",
            "Test collection description",
            [Property.Text("Name")]
        );

        // Assert
        var collection = await _weaviate.Collections.Use<dynamic>(collectionClient.Name).Get();
        Assert.NotNull(collection);
        Assert.Equal("CollectionCreation", collection.Name);
        Assert.Equal("Test collection description", collection.Description);
    }

    [Fact]
    public async Task ObjectCreation()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestData>(
            "",
            "Test collection description",
            [Property.Text("Name")]
        );

        // Act
        var id = Guid.NewGuid();
        var obj = await collectionClient.Data.Insert(
            new TestData() { Name = "TestObject" },
            id: id
        );

        // Assert

        // Assert object exists
        var retrieved = await collectionClient.Query.FetchObjectByID(id);
        var objects = retrieved.Objects.ToList();

        Assert.NotNull(retrieved);
        Assert.Single(objects);
        Assert.Equal(id, objects[0].ID);
        Assert.Equal("TestObject", objects[0].Properties["name"]);
        Assert.Equal("TestObject", objects[0].As<TestData>()?.Name);

        // Delete after usage
        await collectionClient.Data.Delete(id);
        retrieved = await collectionClient.Query.FetchObjectByID(id);
        Assert.NotNull(retrieved.Objects);
        Assert.Empty(retrieved.Objects);
    }
}

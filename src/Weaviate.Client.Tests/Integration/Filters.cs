using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
    [Fact]
    public async Task Filtering()
    {
        // Arrange
        var cA = await CollectionFactory<TestData>("A", "Collection A");

        var uuid_A1 = await cA.Data.Insert(new() { Name = "A1", Size = 3 });
        var uuid_A2 = await cA.Data.Insert(new() { Name = "A2", Size = 5 });

        // Act
        var list = await cA.Query.List(filter: Filter.Property("name").Equal("A1"));

        var objs = list.Objects.ToList();

        // Assert
        Assert.Single(objs);
        Assert.Equal(uuid_A1, objs[0].ID);
    }

    [Fact]
    public async Task FilteringWithExpressions()
    {
        // Arrange
        var cA = await CollectionFactory<TestData>("A", "Collection A");

        var uuid_A1 = await cA.Data.Insert(new() { Name = "A1", Size = 3 });
        var uuid_A2 = await cA.Data.Insert(new() { Name = "A2", Size = 5 });

        // Act
        var list = await cA.Query.List(
            filter: Filter<TestData>.Property(x => x.Size).GreaterThan(3)
        );

        var objs = list.ToList();

        // Assert
        Assert.Single(objs);
        Assert.Equal(uuid_A2, objs[0].ID);
    }

    [Fact]
    public async Task FilteringWithComplexExpressions()
    {
        // Arrange
        // TODO
        // var filter = Filter<TestData>.Build(x => x.Name == "A2" && Size > 3 && Size < 5);
        // Act
        //var list = await cA.Query.List(filter: filter);
        await Task.Yield();
        // Assert
        Assert.True(true);
    }
}

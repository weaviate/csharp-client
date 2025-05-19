using Weaviate.Client.Grpc;
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
        var list1 = await cA.Query.List(filter: Filter.Property("name").Equal("A1"));

        var list2 = await cA.Query.List(
            filter: Filter<TestData>.Property(x => x.Size).GreaterThan(3)
        );

        var objs1 = list1.Objects.ToList();
        var objs2 = list2.ToList();

        // Assert
        Assert.Single(objs1);
        Assert.Equal(uuid_A1, objs1[0].ID);

        Assert.Single(objs2);
        Assert.Equal(uuid_A2, objs2[0].ID);
    }

    [Fact]
    public async Task FilteringWithExpressions()
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

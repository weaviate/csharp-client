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
    public async Task FilteringWithMetadataDates()
    {
        // Arrange
        var cA = await CollectionFactory<TestData>(
            "A",
            "Collection A",
            invertedIndexConfig: new InvertedIndexConfig { IndexTimestamps = true }
        );

        var uuid_A1 = await cA.Data.Insert(new() { Name = "A1", Size = 3 });
        var uuid_A2 = await cA.Data.Insert(new() { Name = "A2", Size = 5 });

        var objsA1 = await cA.Query.FetchObjectByID(
            uuid_A1,
            metadata: MetadataOptions.CreationTime
        );

        // Act
        var objA1 = objsA1.First();
        Assert.NotNull(objA1.Metadata.CreationTime);
        Assert.Equal(DateTimeKind.Utc, objA1.Metadata.CreationTime.Value.Kind);

        var filter = Filter.CreationTime.Equal(objA1.Metadata.CreationTime.Value);
        var list = await cA.Query.List(filter: filter);

        Assert.NotEmpty(list);

        var obj = list.First();

        // Assert
        Assert.Equal(objA1.ID, obj.ID);
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

    public static IEnumerable<TheoryDataRow<Filter, int>> FilteringReferencesTestCases
    {
        get
        {
            yield return (Filter.Reference("ref").Property("size").GreaterThan(3), 1);
            yield return (Filter.Reference("ref").Property("name").Length().LessThan(6), 0);
            yield return (Filter.Reference("ref").ID().Equal(_reusableUuids[1]), 1);
            yield return (
                Filter.Reference("ref2").Reference("ref").Property("name").Length().LessThan(6),
                2
            );
        }
    }

    [Theory]
    [MemberData(nameof(BasicTests.FilteringReferencesTestCases), MemberType = typeof(BasicTests))]
    public async Task FilteringReferences(Filter filter, int expected)
    {
        var cTarget = await CollectionFactory<TestData>(
            "Target",
            "Collection Target",
            invertedIndexConfig: new InvertedIndexConfig() { IndexPropertyLength = true }
        );

        var uuidsTo = new[]
        {
            await cTarget.Data.Insert(new() { Name = "first", Size = 0 }, id: _reusableUuids[0]),
            await cTarget.Data.Insert(new() { Name = "second", Size = 15 }, id: _reusableUuids[1]),
        };

        var cFrom = await CollectionFactory(
            "From",
            properties: [Property.Text("name")],
            references: [Property.Reference("ref", cTarget.Name)]
        );

        await cFrom.AddReference(Property.Reference("ref2", cFrom.Name));

        var uuidsFrom = new List<Guid>
        {
            await cFrom.Data.Insert(new { Name = "first" }, references: [("ref", uuidsTo[0])]),
            await cFrom.Data.Insert(new { Name = "second" }, references: [("ref", uuidsTo[1])]),
        };

        var third = await cFrom.Data.Insert(
            new { Name = "third" },
            references: [("ref2", uuidsFrom[0])]
        );

        var fourth = await cFrom.Data.Insert(
            new { Name = "fourth" },
            references: [("ref2", uuidsFrom[1])]
        );

        uuidsFrom.AddRange([third, fourth]);

        // Act
        var objects = await cFrom.Query.List(filter: filter);

        var objs = objects.ToList();

        // Assert
        Assert.Single(objs);
        Assert.Equal(uuidsFrom[expected], objs.First().ID);
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

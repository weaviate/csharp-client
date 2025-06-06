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

    [Theory]
    [ClassData(typeof(DatasetFilteringReferences))]
    public async Task FilteringReferences(string key)
    {
        var (filter, expected) = DatasetFilteringReferences.Cases[key];

        // Arrange
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

    [Theory]
    [ClassData(typeof(DatasetFilterByID))]
    public async Task FilterByID(string key)
    {
        var filter = DatasetFilterByID.Cases[key];

        // Arrange
        var c = await CollectionFactory(properties: [Property.Text("Name")]);

        var uuids = new[]
        {
            await c.Data.Insert(new { Name = "first" }, _reusableUuids[0]),
            await c.Data.Insert(new { Name = "second" }, _reusableUuids[1]),
        };

        var objects = (await c.Query.List(filter: filter)).ToList();

        Assert.Single(objects);
        Assert.Equal(_reusableUuids[0], objects[0].ID);
    }

    [Theory]
    [ClassData(typeof(DatasetRefCountFilter))]
    public async Task FilteringWithRefCount(string key)
    {
        var (filter, results) = DatasetRefCountFilter.Cases[key];

        // Arrange
        var collection = await CollectionFactory();

        await collection.AddReference(Property.Reference("ref", collection.Name));

        var uuids = new List<Guid>
        {
            await collection.Data.Insert(new { }, id: _reusableUuids[0]),
            await collection.Data.Insert(
                new { },
                id: _reusableUuids[1],
                references: [("ref", _reusableUuids[0])]
            ),
            await collection.Data.Insert(
                new { },
                id: _reusableUuids[2],
                references: [("ref", new[] { _reusableUuids[0], _reusableUuids[1] })]
            ),
        };

        // Act
        var objects = await collection.Query.List(filter: filter);
        var objs = objects.ToList();

        // Assert
        Assert.Equal(results.Length, objs.Count);

        var expectedUuids = results.Select(result => uuids[result]).ToList();
        Assert.True(
            objs.Where(obj => obj.ID.HasValue)
                .All(obj => expectedUuids.Contains(obj.ID ?? Guid.Empty))
        );
    }

    [Fact]
    public async Task FilterByNestedReferenceCount()
    {
        // Arrange
        var one = await CollectionFactory("one");
        var two = await CollectionFactory(
            "two",
            references: [Property.Reference("ref2", one.Name)]
        );

        await one.AddReference(Property.Reference("ref1", one.Name));

        var uuid11 = await one.Data.Insert(new { });
        var uuid12 = await one.Data.Insert(new { }, references: [("ref1", uuid11)]);
        var uuid13 = await one.Data.Insert(
            new { },
            references: [("ref1", new[] { uuid11, uuid12 })]
        );

        await two.Data.Insert(new { });
        var uuid21 = await two.Data.Insert(new { }, references: [("ref2", uuid12)]);
        await two.Data.Insert(new { }, references: [("ref2", uuid13)]);

        // Act
        var objects = await two.Query.List(
            filter: Filter.Reference("ref2").Reference("ref1").Count.Equal(1),
            references:
            [
                new QueryReference("ref2", [], references: [new QueryReference("ref1", [])]),
            ]
        );

        var objs = objects.ToList();

        // Assert
        Assert.Single(objs);
        Assert.Equal(uuid21, objs[0].ID);
    }

    [Fact]
    public async Task TimeFilterContains()
    {
        // Arrange
        var collection = await CollectionFactory(
            invertedIndexConfig: new InvertedIndexConfig() { IndexTimestamps = true }
        );

        await collection.Data.Insert(new { });
        await Task.Delay(10, TestContext.Current.CancellationToken);

        var uuid2 = await collection.Data.Insert(new { });
        await Task.Delay(10, TestContext.Current.CancellationToken);

        var uuid3 = await collection.Data.Insert(new { });

        var obj2 = await collection.Query.FetchObjectByID(
            uuid2,
            metadata: MetadataOptions.CreationTime
        );
        var obj3 = await collection.Query.FetchObjectByID(
            uuid3,
            metadata: MetadataOptions.CreationTime
        );

        // Act
        var objects = await collection.Query.List(
            filter: Filter.CreationTime.ContainsAny(
                [
                    obj2.First().Metadata.CreationTime!.Value,
                    obj3.First().Metadata.CreationTime!.Value,
                ]
            )
        );

        var objs = objects.ToList();

        // Assert
        Assert.Equal(2, objs.Count);
        var expectedUuids = new HashSet<Guid>([uuid2, uuid3]);
        Assert.True(objs.All(obj => obj.ID != null && expectedUuids.Contains(obj.ID.Value)));
    }

    [Theory]
    [ClassData(typeof(DatasetTimeFilter))]
    public async Task TimeFiltering(string key)
    {
        var (filterValue, results, filterFunc) = DatasetTimeFilter.Cases[key];

        // Arrange
        var collection = await CollectionFactory(
            invertedIndexConfig: new InvertedIndexConfig() { IndexTimestamps = true }
        );

        await collection.Data.Insert(new { });
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await collection.Data.Insert(new { });
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await collection.Data.Insert(new { });

        var allObjects = await collection.Query.List(
            sort: [Sort.ByCreationTime()],
            metadata: MetadataOptions.CreationTime
        );
        var allObjectsList = allObjects.ToList();

        var referenceTime = allObjectsList[filterValue].Metadata.CreationTime!.Value;

        var weaviateFilter = filterFunc(referenceTime);

        // Act
        var objects = await collection.Query.List(filter: weaviateFilter);
        var objs = objects.ToList();

        // Assert
        Assert.Equal(results.Length, objs.Count);

        var expectedUuids = new HashSet<Guid>(
            results.Select(result => allObjectsList[result].ID!.Value)
        );
        Assert.True(objs.All(obj => obj.ID != null && expectedUuids.Contains(obj.ID.Value)));
    }

    [Theory]
    [ClassData(typeof(DatasetFilterArrayTypes))]
    public async Task FilterArrayTypes(string key)
    {
        (Filter filter, int[] results) = DatasetFilterArrayTypes.Cases[key];

        // Arrange
        var collection = await CollectionFactory(
            vectorConfig: new Dictionary<string, VectorConfig>
            {
                {
                    "default",
                    new VectorConfig { Vectorizer = Vectorizer.None, VectorIndexType = "hnsw" }
                },
            },
            properties:
            [
                Property.TextArray("texts"),
                Property.IntArray("ints"),
                Property.NumberArray("floats"),
            ]
        );

        var uuids = new[]
        {
            await collection.Data.Insert(
                new
                {
                    texts = new[] { "an", "apple" },
                    ints = new[] { 1, 2 },
                    floats = new[] { 1.0, 2.0 },
                }
            ),
            await collection.Data.Insert(
                new
                {
                    texts = new[] { "a", "banana" },
                    ints = new[] { 2, 3 },
                    floats = new[] { 2.0, 3.0 },
                }
            ),
            await collection.Data.Insert(
                new
                {
                    texts = new[] { "a", "text" },
                    ints = new[] { 4, 5 },
                    floats = new[] { 4.0, 5.0 },
                }
            ),
        };

        // Act
        var objects = await collection.Query.List(filter: filter);

        // Assert
        Assert.Equal(results.Length, objects.Count());

        var expectedUuids = results.Select(result => uuids[result]).ToHashSet();
        Assert.True(objects.All(obj => expectedUuids.Contains(obj.ID!.Value)));
    }

    [Theory]
    [ClassData(typeof(DatasetFilterContains))]
    public async Task FilterContains(string test)
    {
        (Filter filter, int[] results) = DatasetFilterContains.Cases[test];

        // Arrange
        var collection = await CollectionFactory(
            vectorConfig: new Dictionary<string, VectorConfig>
            {
                {
                    "default",
                    new VectorConfig { Vectorizer = Vectorizer.None, VectorIndexType = "hnsw" }
                },
            },
            properties:
            [
                Property.Text("text"),
                Property.TextArray("texts"),
                Property.Int("int"),
                Property.IntArray("ints"),
                Property.Number("float"),
                Property.NumberArray("floats"),
                Property.Bool("bool"),
                Property.BoolArray("bools"),
                Property.DateArray("dates"),
                Property.Date("date"),
                Property.UuidArray("uuids"),
                Property.Uuid("uuid"),
            ]
        );

        var uuids = new[]
        {
            await collection.Data.Insert(
                new
                {
                    text = "this is a test",
                    texts = new[] { "this", "is", "a", "test" },
                    @int = 1,
                    ints = new[] { 1, 2, 4 },
                    @float = 0.5,
                    floats = new[] { 0.4, 0.9, 2.0 },
                    @bool = true,
                    bools = new[] { true, false },
                    dates = new[] { NOW, LATER, MUCH_LATER },
                    date = NOW,
                    uuids = new[] { UUID1, UUID3, UUID2 },
                    uuid = UUID1,
                },
                id: UUID1
            ),
            await collection.Data.Insert(
                new
                {
                    text = "this is not a real test",
                    texts = new[] { "this", "is", "not", "a", "real", "test" },
                    @int = 1,
                    ints = new[] { 5, 6, 9 },
                    @float = 0.3,
                    floats = new[] { 0.1, 0.7, 2.0 },
                    @bool = true,
                    bools = new[] { false, false },
                    dates = new[] { NOW, NOW, MUCH_LATER },
                    date = LATER,
                    uuids = new[] { UUID2, UUID2 },
                    uuid = UUID2,
                },
                id: UUID2
            ),
            await collection.Data.Insert(
                new
                {
                    text = "real deal",
                    texts = new[] { "real", "deal" },
                    @int = 3,
                    ints = new int[0],
                    floats = new double[0],
                    @bool = false,
                    bools = new bool[0],
                    dates = new DateTime[0],
                    uuids = new Guid[0],
                },
                id: UUID3
            ),
            await collection.Data.Insert(
                new
                {
                    text = "not real deal",
                    texts = new[] { "not", "real", "deal" },
                    @int = 4,
                    ints = new[] { 4 },
                    @float = 8.0,
                    floats = new[] { 0.7 },
                    @bool = true,
                    bools = new[] { true },
                    dates = new[] { MUCH_LATER },
                    date = MUCH_LATER,
                    uuids = new[] { UUID1, UUID2 },
                    uuid = UUID2,
                }
            ),
        };

        // Act
        var objects = await collection.Query.List(filter: filter);

        // Assert
        Assert.Equal(results.Length, objects.Count());

        var expectedUuids = results.Select(result => uuids[result]).ToHashSet();
        Assert.True(objects.All(obj => expectedUuids.Contains(obj.ID!.Value)));
    }
}

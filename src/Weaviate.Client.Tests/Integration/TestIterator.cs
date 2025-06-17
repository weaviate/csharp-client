using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
    [Fact]
    public async Task Test_Iterator()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("name")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        await collection.Data.InsertMany(new { Name = "Name 1" }, new { Name = "Name 2" });

        var names = new List<string>();
        await foreach (
            var obj in collection.Iterator(cancellationToken: TestContext.Current.CancellationToken)
        )
        {
            obj.Do(o => names.Add(o.Name));
        }

        Assert.Contains("Name 1", names);
        Assert.Contains("Name 2", names);
    }

    [Theory]
    [InlineData(false, null, null, null)]
    [InlineData(true, null, null, null)]
    [InlineData(false, true, null, null)]
    [InlineData(true, true, null, null)]
    [InlineData(false, null, true, null)]
    [InlineData(true, null, true, null)]
    [InlineData(false, true, true, null)]
    [InlineData(true, true, true, null)]
    [InlineData(false, null, null, 100u)]
    [InlineData(true, null, null, 100u)]
    [InlineData(false, true, null, 100u)]
    [InlineData(true, true, null, 100u)]
    [InlineData(false, null, true, 100u)]
    [InlineData(true, null, true, 100u)]
    [InlineData(false, true, true, 100u)]
    [InlineData(true, true, true, 100u)]
    [InlineData(false, null, null, 10000u)]
    [InlineData(true, null, null, 10000u)]
    [InlineData(false, true, null, 10000u)]
    [InlineData(true, true, null, 10000u)]
    [InlineData(false, null, true, 10000u)]
    [InlineData(true, null, true, 10000u)]
    [InlineData(false, true, true, 10000u)]
    [InlineData(true, true, true, 10000u)]
    public async Task Test_Iterator_Arguments(
        bool includeVector,
        bool? returnFullMetadata,
        bool? returnSpecificProperties,
        uint? cacheSize
    )
    {
        var collection = await CollectionFactory(
            properties: [Property.Int("data"), Property.Text("text")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.Text2VecContextionary())
        );

        // Insert test data
        var insertData = Enumerable
            .Range(0, 10)
            .Select(i => new { data = i, text = "hi" })
            .ToArray();
        await collection.Data.InsertMany(insertData);

        // Build metadata query
        MetadataQuery? metadata = null;
        if (includeVector && returnFullMetadata == true)
        {
            metadata = new MetadataQuery(MetadataOptions.Full | MetadataOptions.Vector);
        }
        else if (includeVector)
        {
            metadata = new MetadataQuery(MetadataOptions.Vector);
        }
        else if (returnFullMetadata == true)
        {
            metadata = new MetadataQuery(MetadataOptions.Full);
        }

        // Build fields array
        string[]? fields = null;
        if (returnSpecificProperties == true)
        {
            fields = ["data"];
        }

        var iter = collection.Iterator(
            metadata: metadata,
            fields: fields,
            cacheSize: cacheSize ?? CollectionClient<object>.ITERATOR_CACHE_SIZE,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objects = new List<WeaviateObject>();
        await foreach (var obj in iter)
        {
            objects.Add(obj);
        }

        // Verify we got all 10 objects
        Assert.Equal(10, objects.Count);

        // Sort by data property for consistent comparison
        var allData = objects.Select(obj => (long)obj.Properties["data"]!).OrderBy(x => x).ToList();
        Assert.Equal(Enumerable.Range(0, 10).Select(Convert.ToInt64).ToList(), allData);

        // Test expectations based on parameters
        if (includeVector && returnSpecificProperties != true && returnFullMetadata == true)
        {
            // Expect everything back
            Assert.All(objects, obj => Assert.True(obj.Properties.ContainsKey("text")));
            Assert.All(objects, obj => Assert.True(obj.Vectors.ContainsKey("default")));
            Assert.All(objects, obj => Assert.NotNull(obj.Metadata.CreationTime));
            Assert.All(objects, obj => Assert.NotNull(obj.Metadata.Score));
        }
        else if (!includeVector && returnSpecificProperties != true && returnFullMetadata == true)
        {
            // Expect everything back except vector
            Assert.All(objects, obj => Assert.True(obj.Properties.ContainsKey("text")));
            Assert.All(objects, obj => Assert.False(obj.Vectors.ContainsKey("default")));
            Assert.All(objects, obj => Assert.NotNull(obj.Metadata.CreationTime));
            Assert.All(objects, obj => Assert.NotNull(obj.Metadata.Score));
        }
        else if (includeVector && returnSpecificProperties == true)
        {
            // Expect specified properties and vector
            Assert.All(objects, obj => Assert.False(obj.Properties.ContainsKey("text")));
            Assert.All(objects, obj => Assert.True(obj.Vectors.ContainsKey("default")));
            if (returnFullMetadata == true)
            {
                Assert.All(objects, obj => Assert.NotNull(obj.Metadata.CreationTime));
                Assert.All(objects, obj => Assert.NotNull(obj.Metadata.Score));
            }
            else
            {
                Assert.All(objects, obj => Assert.Null(obj.Metadata.CreationTime));
            }
        }
        else if (!includeVector && returnSpecificProperties == true)
        {
            // Expect specified properties and no vector
            Assert.All(objects, obj => Assert.False(obj.Properties.ContainsKey("text")));
            Assert.All(objects, obj => Assert.False(obj.Vectors.ContainsKey("default")));
            if (returnFullMetadata == true)
            {
                Assert.All(objects, obj => Assert.NotNull(obj.Metadata.CreationTime));
                Assert.All(objects, obj => Assert.NotNull(obj.Metadata.Score));
            }
            else
            {
                Assert.All(objects, obj => Assert.Null(obj.Metadata.CreationTime));
            }
        }
    }

    [Fact]
    public async Task Test_Iterator_With_Default_Generic()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("this"), Property.Text("that")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        var insertData = Enumerable
            .Range(0, 10)
            .Select(_ => new { @this = "this", that = "that" })
            .ToArray();
        await collection.Data.InsertMany(insertData);

        // Test with all properties
        var allPropsIter = collection.Iterator(
            cancellationToken: TestContext.Current.CancellationToken
        );

        await foreach (var obj in allPropsIter)
        {
            Assert.Equal("this", obj.Properties["this"]);
            Assert.Equal("that", obj.Properties["that"]);
        }

        // Test with specific properties
        var specificPropsIter = collection.Iterator(
            fields: ["this"],
            cancellationToken: TestContext.Current.CancellationToken
        );

        await foreach (var obj in specificPropsIter)
        {
            Assert.Equal("this", obj.Properties["this"]);
            Assert.False(obj.Properties.ContainsKey("that"));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(CollectionClient<int>.ITERATOR_CACHE_SIZE - 1)]
    [InlineData(CollectionClient<int>.ITERATOR_CACHE_SIZE)]
    [InlineData(CollectionClient<int>.ITERATOR_CACHE_SIZE + 1)]
    [InlineData(2 * CollectionClient<int>.ITERATOR_CACHE_SIZE - 1)]
    [InlineData(2 * CollectionClient<int>.ITERATOR_CACHE_SIZE)]
    [InlineData(2 * CollectionClient<int>.ITERATOR_CACHE_SIZE + 1)]
    [InlineData(20 * CollectionClient<int>.ITERATOR_CACHE_SIZE)]
    public async Task Test_Iterator_Basic(uint count)
    {
        var collection = await CollectionFactory(
            properties: [Property.Int("data")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        if (count > 0)
        {
            var insertData = Enumerable
                .Range(0, (int)count)
                .Select(i => new { data = i })
                .ToArray();
            await collection.Data.InsertMany(insertData);
        }

        var expected = Enumerable.Range(0, (int)count).Select(x => Convert.ToInt64(x)).ToList();
        List<long>? firstOrder = null;

        // Make sure a new iterator resets the internal state and that the return order is the same for every run
        for (int run = 0; run < 3; run++)
        {
            var iter = collection.Iterator(
                cancellationToken: TestContext.Current.CancellationToken
            );

            var ret = new List<long>();
            await foreach (var obj in iter)
            {
                ret.Add((long)obj.Properties["data"]!);
            }

            if (firstOrder == null)
            {
                firstOrder = ret;
            }
            else
            {
                Assert.Equal(firstOrder, ret);
            }

            Assert.Equal(expected, ret.OrderBy(x => x).ToList());
        }
    }

    [Fact]
    public async Task Test_Iterator_With_After()
    {
        var collection = await CollectionFactory(
            properties: [Property.Int("data")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        var insertData = Enumerable.Range(0, 10).Select(i => new { data = i }).ToArray();
        await collection.Data.InsertMany(insertData);

        // Get all UUIDs first
        var allUuids = new List<Guid>();
        var initialIter = collection.Iterator(
            cancellationToken: TestContext.Current.CancellationToken
        );

        await foreach (var obj in initialIter)
        {
            allUuids.Add(obj.ID!.Value);
        }

        // Test pagination with after parameter
        var firstAfterObject = await collection
            .Iterator(after: allUuids[5], cancellationToken: TestContext.Current.CancellationToken)
            .FirstAsync(TestContext.Current.CancellationToken);

        // Fetch the object at index 6 to compare
        var expectedObject = await collection.Query.FetchObjectByID(allUuids[6]);

        Assert.Equal(expectedObject!.Properties["data"]!, firstAfterObject.Properties["data"]!);
    }
}

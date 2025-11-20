using Weaviate.Client.Models;
using Weaviate.Client.Tests.Common;

namespace Weaviate.Client.Tests.Integration;

[Collection("TestQueries")]
public class TestQueries : IntegrationTests
{
    [Theory()]
    [InlineData("testText")]
    [InlineData("testInt")]
    [InlineData("testNumber")]
    [InlineData("testDate")]
    public async Task Test_Sorting(string propertyName)
    {
        var props = Property.FromClass<TestProperties>();

        // Arrange
        var collection = await this.CollectionFactory<TestProperties>(
            properties: props,
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
        );

        var testData = new[]
        {
            new TestProperties
            {
                TestText = "Alice",
                TestInt = 1,
                TestNumber = 1.1,
                TestDate = DateTime.Parse("2023-01-01"),
            },
            new TestProperties
            {
                TestText = "Bob",
                TestInt = 2,
                TestNumber = 2.2,
                TestDate = DateTime.Parse("2023-01-02"),
            },
            new TestProperties
            {
                TestText = "Charlie",
                TestInt = 3,
                TestNumber = 3.3,
                TestDate = DateTime.Parse("2023-01-03"),
            },
        };

        await collection.Data.InsertMany(testData, TestContext.Current.CancellationToken);

        // Act
        var dataDesc = await collection.Query.FetchObjects(
            sort: Sort.ByProperty(propertyName).Descending(),
            cancellationToken: TestContext.Current.CancellationToken
        );
        var dataAsc = await collection.Query.FetchObjects(
            sort: Sort.ByProperty(propertyName).Ascending(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var namesDesc = dataDesc.Select(d => d.Properties["testText"]);
        var namesAsc = dataAsc.Select(d => d.Properties["testText"]);

        // Assert
        Assert.Equal(new List<string> { "Charlie", "Bob", "Alice" }, namesDesc);
        Assert.Equal(new List<string> { "Alice", "Bob", "Charlie" }, namesAsc);
    }

    [Fact]
    public async Task Test_BM25_Generate_And_GroupBy_With_Everything()
    {
        // Arrange
        var collection = await this.CollectionFactory<object>(
            properties: [Property.Text("text"), Property.Text("content")],
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided()),
            generativeConfig: new GenerativeConfig.Custom
            {
                Type = "generative-dummy",
                Config = new { ConfigOption = "ConfigValue" },
            }
        );

        var testData = new[]
        {
            new
            {
                text = "apples are big",
                content = "Teddy is the biggest and bigger than everything else",
            },
            new
            {
                text = "bananas are small",
                content = "cats are the smallest and smaller than everything else",
            },
        };

        await collection.Data.InsertMany(
            BatchInsertRequest.Create<object>(testData),
            TestContext.Current.CancellationToken
        );

        // Act
        var groupBy = new GroupByRequest
        {
            PropertyName = "text",
            NumberOfGroups = 2,
            ObjectsPerGroup = 1,
        };

        var res = await collection.Generate.BM25(
            query: "Teddy",
            groupBy: groupBy,
            searchFields: new[] { "content" },
            prompt: new SinglePrompt
            {
                Prompt =
                    "Is there something to eat in {text}? Only answer yes if there is something to eat or no if not without punctuation",
            },
            groupedPrompt: new GroupedPrompt
            {
                Task =
                    "What is the biggest and what is the smallest? Only write the names separated by a space",
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(res);
        Assert.NotNull(res.Generative);
        Assert.NotEmpty(res.Generative);
        Assert.Contains("prompt: What is the biggest", res.Generative[0]);
        Assert.Single(res.Groups);
        var groups = res.Groups.Values.ToList();
        // Get the first object in the first group and check its generative result
        var firstGroupObject = groups[0];
        Assert.NotNull(firstGroupObject);
        Assert.NotNull(firstGroupObject.Generative);
        Assert.Contains(
            "Is there something to eat in apples are big",
            firstGroupObject.Generative[0]
        );
        // Get the first object in the result set and check its group
        var firstObject = res.Objects[0];
        Assert.Equal("apples are big", firstObject.BelongsToGroup);
    }

    [Fact]
    public async Task Test_Collection_Generative_FetchObjects()
    {
        // Arrange: create collection with no vectorizer
        var collection = await CollectionFactory(
            properties: [Property.Text("text")],
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            generativeConfig: new GenerativeConfig.Custom
            {
                Type = "generative-dummy",
                Config = new { ConfigOption = "ConfigValue" },
            }
        );

        // Insert data
        await collection.Data.InsertMany(
            BatchInsertRequest.Create<object>(
                [new { text = "John Doe" }, new { text = "Jane Doe" }]
            ),
            TestContext.Current.CancellationToken
        );

        // Act: generative fetch
        var res = await collection.Generate.FetchObjects(
            prompt: new SinglePrompt { Prompt = "Who is this? {text}" },
            groupedPrompt: new GroupedPrompt
            {
                Task = "Who are these people?",
                Properties = "text",
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(res);
        Assert.NotNull(res.Generative);
        Assert.NotNull(res.Objects);
        Assert.Equal(2, res.Objects.Count());

        foreach (var obj in res.Objects)
        {
            Assert.NotNull(obj.Generative);
            Assert.Contains(
                "I'm sorry, I'm just a dummy and can't generate anything.",
                obj.Generative.Values[0].Result
            );

            // The value of the Result property can be
            // accessed by index of Generative,
            // or via Generative.Values[] indexer.
            // They return the same value.
            // The entries in the Values list contain additional properties.
            Assert.Same(obj.Generative.Values[0].Result, obj.Generative[0]);
        }
    }

    public static IEnumerable<object?[]> GenerateByIdsTestData()
    {
        var uuid1 = _reusableUuids[0];
        var uuid2 = _reusableUuids[1];
        var uuid3 = _reusableUuids[2];

        yield return new object?[] { Array.Empty<Guid>(), 0, new HashSet<Guid>() };
        yield return new object?[] { new Guid[] { }, 0, new HashSet<Guid>() };
        yield return new object?[]
        {
            new Guid[] { uuid3 },
            1,
            new HashSet<Guid> { uuid3 },
        };
        yield return new object?[]
        {
            new Guid[] { uuid1, uuid2 },
            2,
            new HashSet<Guid> { uuid1, uuid2 },
        };
        yield return new object?[]
        {
            new Guid[] { uuid1, uuid3 },
            2,
            new HashSet<Guid> { uuid1, uuid3 },
        };
        yield return new object?[]
        {
            new Guid[] { uuid1, uuid3, uuid3 },
            2,
            new HashSet<Guid> { uuid1, uuid3 },
        };
    }

    [Theory]
    [MemberData(nameof(GenerateByIdsTestData))]
    public async Task Test_Generate_By_Ids(Guid[] ids, int expectedLen, HashSet<Guid> expected)
    {
        var collection = await CollectionFactory(
            vectorConfig: Configure.Vectors.SelfProvided().New(),
            properties: [Property.Text("text")],
            generativeConfig: new GenerativeConfig.Custom
            {
                Type = "generative-dummy",
                Config = new { ConfigOption = "ConfigValue" },
            }
        );

        var result = await collection.Data.InsertMany(
            new (object, Guid)[]
            {
                (new { text = "John Doe" }, id: _reusableUuids[0]),
                (new { text = "Jane Doe" }, id: _reusableUuids[1]),
                (new { text = "J. Doe" }, id: _reusableUuids[2]),
            },
            TestContext.Current.CancellationToken
        );

        var res = await collection.Generate.FetchObjectsByIDs(
            [.. ids],
            prompt: new SinglePrompt { Prompt = "Who is this? {text}" },
            groupedPrompt: new GroupedPrompt
            {
                Task = "Who are these people?",
                Properties = new List<string> { "text" },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(res);
        Assert.NotNull(res.Generative);
        Assert.Equal(expectedLen, res.Objects.Count());
        Assert.Equal(expected, res.Objects.Select(o => o.ID!.Value).ToHashSet());
        foreach (var obj in res.Objects)
        {
            Assert.NotNull(obj.Generative);
        }
    }
}

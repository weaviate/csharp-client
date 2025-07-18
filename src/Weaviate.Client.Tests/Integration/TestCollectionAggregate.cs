using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weaviate.Client.Models;
using Xunit;

namespace Weaviate.Client.Tests.Integration;

[Collection("TestCollections")]
public partial class AggregatesTests : IntegrationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10000)]
    [InlineData(20000)]
    [InlineData(20001)]
    [InlineData(100000)]
    public async Task Test_Collection_Length(int howMany)
    {
        var collectionClient = await CollectionFactory(
            vectorConfig: Configure.Vectors.SelfProvided()
        );

        await collectionClient.Data.InsertMany([.. Enumerable.Repeat(new { }, howMany)]);

        var count = await collectionClient.Count();

        Assert.Equal(howMany, Convert.ToInt64(count));
    }

    [Fact]
    public async Task Test_Empty_Aggregation()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        var result = await collectionClient.Aggregate.OverAll();
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Test_Simple_Aggregation()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        await collectionClient.Data.Insert(new { text = "some text" });

        var result = await collectionClient.Aggregate.OverAll(
            metrics: new[] { Metrics.ForProperty("text").Text(count: true) }
        );

        var text = result.Properties["text"] as Aggregate.Text;
        Assert.NotNull(text);
        Assert.Equal(1, text.Count);
    }

    [Fact]
    public async Task Test_Aggregation_Top_Occurrence_With_Limit()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        await collectionClient.Data.Insert(new { text = "one" });
        await collectionClient.Data.Insert(new { text = "one" });
        await collectionClient.Data.Insert(new { text = "two" });

        var result = await collectionClient.Aggregate.OverAll(
            metrics: new[] { Metrics.ForProperty("text").Text(minOccurrences: 1) }
        );

        var text = result.Properties["text"] as Aggregate.Text;
        Assert.NotNull(text);
        Assert.Single(text.TopOccurrences);
        Assert.Equal(2, text.TopOccurrences[0].Count);
    }

    [Fact]
    public async Task Test_Aggregation_GroupBy_With_Limit()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        await collectionClient.Data.Insert(new { text = "one" });
        await collectionClient.Data.Insert(new { text = "two" });
        await collectionClient.Data.Insert(new { text = "three" });

        var result = await collectionClient.Aggregate.OverAll(
            groupBy: new Aggregate.GroupBy("text", 2),
            metrics: Metrics.ForProperty("text").Text(count: true)
        );

        Assert.Equal(2, result.Groups.Count);
        var group1 = result.Groups[0].Properties["text"] as Aggregate.Text;
        var group2 = result.Groups[1].Properties["text"] as Aggregate.Text;
        Assert.NotNull(group1);
        Assert.NotNull(group2);
        Assert.Equal(1, group1.Count);
        Assert.Equal(1, group2.Count);
    }

    [Fact]
    public async Task Test_Aggregation_GroupBy_No_Results()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        var result = await collectionClient.Aggregate.OverAll(
            groupBy: new Aggregate.GroupBy("text", 2),
            metrics: Metrics.ForProperty("text").Text(count: true)
        );

        Assert.Empty(result.Groups);
    }

    public static IEnumerable<object[]> FilterTestData()
    {
        var uuid1 = _reusableUuids[0];
        var uuid2 = _reusableUuids[1];
        var date1 = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc);

        yield return new object[] { Filter.Property("text").Equal("two") };
        yield return new object[] { Filter.Property("int").Equal(2) };
        yield return new object[] { Filter.Property("float").Equal(2.0) };
        yield return new object[] { Filter.Property("bool").Equal(false) };
        yield return new object[] { Filter.Property("date").Equal(date2) };
        yield return new object[]
        {
            Filter.Property("text").Equal("two") | Filter.Property("int").Equal(2),
        };
        yield return new object[] { Filter.Property("uuid").Equal(uuid2) };
        yield return new object[] { Filter.Property("texts").ContainsAny(new[] { "two" }) };
        yield return new object[] { Filter.Property("ints").ContainsAny(new[] { 2 }) };
        yield return new object[] { Filter.Property("floats").ContainsAny(new[] { 2.0 }) };
        yield return new object[] { Filter.Property("bools").ContainsAny(new[] { false }) };
        yield return new object[] { Filter.Property("dates").ContainsAny(new[] { date2 }) };
        yield return new object[] { Filter.Property("uuids").ContainsAny(new[] { uuid2 }) };
    }

    [Theory]
    [MemberData(nameof(FilterTestData))]
    public async Task Test_OverAll_With_Filters(Filter filter)
    {
        var uuid1 = _reusableUuids[0];
        var uuid2 = _reusableUuids[1];
        var date1 = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc);

        var collectionClient = await CollectionFactory(
            properties: new[]
            {
                Property.Text("text"),
                Property.Int("int"),
                Property.Number("float"),
                Property.Bool("bool"),
                Property.Date("date"),
                Property.Uuid("uuid"),
                Property.TextArray("texts"),
                Property.IntArray("ints"),
                Property.NumberArray("floats"),
                Property.BoolArray("bools"),
                Property.DateArray("dates"),
                Property.UuidArray("uuids"),
            }
        );

        await collectionClient.Data.Insert(
            new
            {
                text = "one",
                @int = 1,
                @float = 1.0,
                @bool = true,
                date = date1,
                uuid = uuid1,
                texts = new[] { "one" },
                ints = new[] { 1 },
                floats = new[] { 1.0 },
                bools = new[] { true },
                dates = new[] { date1 },
                uuids = new[] { uuid1 },
            }
        );

        await collectionClient.Data.Insert(
            new
            {
                text = "two",
                @int = 2,
                @float = 2.0,
                @bool = false,
                date = date2,
                uuid = uuid2,
                texts = new[] { "two" },
                ints = new[] { 2 },
                floats = new[] { 2.0 },
                bools = new[] { false },
                dates = new[] { date2 },
                uuids = new[] { uuid2 },
            }
        );

        var result = await collectionClient.Aggregate.OverAll(
            filter: filter,
            metrics: new[]
            {
                Metrics.ForProperty("text").Text(count: true, topOccurrencesValue: true),
            }
        );

        var text = result.Properties["text"] as Aggregate.Text;
        Assert.NotNull(text);
        Assert.Equal(1, text.Count);
        Assert.Equal("two", text.TopOccurrences[0].Value);
    }

    public static IEnumerable<object[]> NearVectorAggregationTestData()
    {
        yield return new object[]
        {
            new Dictionary<string, object> { { "objectLimit", 1 } },
            1,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "certainty", 0.9 } },
            1,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "distance", 0.1 } },
            1,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "objectLimit", 2 } },
            2,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "certainty", 0.1 } },
            2,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "distance", 0.9 } },
            2,
        };
    }

    [Theory]
    [MemberData(nameof(NearVectorAggregationTestData))]
    public async Task Test_Near_Vector_Aggregation(
        Dictionary<string, object> option,
        int expectedLen
    )
    {
        var collectionClient = await CollectionFactory(
            properties: [Property.Text("text")],
            vectorConfig: Configure
                .Vectors.Text2VecContextionary(vectorizeCollectionName: false)
                .New("default")
        );

        var text1 = "some text";
        var text2 = "nothing like the other one at all, not even a little bit";
        var uuid = await collectionClient.Data.Insert(new { text = text1 });
        var obj = await collectionClient.Query.FetchObjectByID(
            uuid,
            metadata: MetadataOptions.Vector
        );
        Assert.NotNull(obj);
        Assert.True(obj.Vectors.ContainsKey("default"));
        await collectionClient.Data.Insert(new { text = text2 });

        var nearVector = obj.Vectors["default"].ToArray();
        var metrics = new[]
        {
            Metrics
                .ForProperty("text")
                .Text(count: true, topOccurrencesCount: true, topOccurrencesValue: true),
        };

        AggregateResult? result = null;

        if (option.ContainsKey("objectLimit"))
        {
            result = await collectionClient.Aggregate.NearVector(
                nearVector,
                metrics: metrics,
                limit: Convert.ToUInt32(option["objectLimit"])
            );
        }
        else if (option.ContainsKey("certainty"))
        {
            result = await collectionClient.Aggregate.NearVector(
                nearVector,
                metrics: metrics,
                certainty: Convert.ToDouble(option["certainty"])
            );
        }
        else if (option.ContainsKey("distance"))
        {
            result = await collectionClient.Aggregate.NearVector(
                nearVector,
                metrics: metrics,
                distance: Convert.ToDouble(option["distance"])
            );
        }

        Assert.NotNull(result);
        var textAgg = result.Properties["text"] as Aggregate.Text;

        Assert.NotNull(textAgg);
        Assert.Equal(expectedLen, textAgg.Count);
        Assert.Equal(expectedLen, textAgg.TopOccurrences.Count);
        Assert.Contains(text1, textAgg.TopOccurrences.Select(o => o.Value));
        if (expectedLen == 2)
        {
            Assert.Contains(text2, textAgg.TopOccurrences.Select(o => o.Value));
        }
        else
        {
            Assert.DoesNotContain(text2, textAgg.TopOccurrences.Select(o => o.Value));
        }
    }

    public static IEnumerable<object[]> NearTextAggregationOptions()
    {
        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();

        yield return new object[]
        {
            new Dictionary<string, object> { { "object_limit", 1 } },
            1,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "certainty", 0.9 } },
            1,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "distance", 0.1 } },
            1,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "object_limit", 2 } },
            2,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "certainty", 0.1 } },
            2,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object> { { "distance", 0.9 } },
            2,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object>
            {
                { "move_away", new Move(concepts: ["something"], force: 0.000001f) },
                { "distance", 0.9 },
            },
            2,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object>
            {
                { "move_away", new Move(objects: [uuid1], force: 0.000001f) },
                { "distance", 0.9 },
            },
            2,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object>
            {
                {
                    "move_away",
                    new Move(concepts: new[] { "something", "else" }, force: 0.000001f)
                },
                { "distance", 0.9 },
            },
            2,
            uuid1,
            uuid2,
        };
        yield return new object[]
        {
            new Dictionary<string, object>
            {
                { "move_to", new Move(objects: new[] { uuid1, uuid2 }, force: 0.000001f) },
                { "distance", 0.9 },
            },
            2,
            uuid1,
            uuid2,
        };
    }

    [Theory]
    [MemberData(nameof(NearTextAggregationOptions))]
    public async Task Test_Near_Text_Aggregation(
        Dictionary<string, object> option,
        int expectedLen,
        Guid uuid1,
        Guid uuid2
    )
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            properties: new[] { Property.Text("text") },
            vectorConfig: Configure
                .Vectors.Text2VecContextionary(vectorizeCollectionName: false)
                .New("default")
        );
        var text1 = "some text";
        var text2 = "nothing like the other one at all, not even a little bit";
        await collectionClient.Data.Insert(new { text = text1 }, id: uuid1);
        await collectionClient.Data.Insert(new { text = text2 }, id: uuid2);

        // Act
        AggregateResult? res = null;
        var metrics = new[]
        {
            Metrics
                .ForProperty("text")
                .Text(count: true, topOccurrencesCount: true, topOccurrencesValue: true),
        };

        if (option.ContainsKey("object_limit"))
        {
            res = await collectionClient.Aggregate.NearText(
                new[] { text1 },
                metrics: metrics,
                limit: Convert.ToUInt32(option["object_limit"])
            );
        }
        else if (option.ContainsKey("certainty"))
        {
            res = await collectionClient.Aggregate.NearText(
                new[] { text1 },
                metrics: metrics,
                certainty: Convert.ToDouble(option["certainty"])
            );
        }
        else if (option.ContainsKey("distance"))
        {
            res = await collectionClient.Aggregate.NearText(
                new[] { text1 },
                metrics: metrics,
                distance: Convert.ToDouble(option["distance"])
            );
        }
        else
        {
            // Handle move_to/move_away or other options
            var moveTo = option.ContainsKey("move_to") ? option["move_to"] as Move : null;
            var moveAway = option.ContainsKey("move_away") ? option["move_away"] as Move : null;
            res = await collectionClient.Aggregate.NearText(
                new[] { text1 },
                metrics: metrics,
                moveTo: moveTo,
                moveAway: moveAway
            );
        }

        // Assert
        Assert.IsType<Aggregate.Text>(res.Properties["text"]);
        var aggText = (Aggregate.Text)res.Properties["text"];
        Assert.Equal(expectedLen, aggText.Count);
        Assert.Equal(expectedLen, aggText.TopOccurrences.Count);
        Assert.Contains(text1, aggText.TopOccurrences.Select(o => o.Value));
        if (expectedLen == 2)
        {
            Assert.Contains(text2, aggText.TopOccurrences.Select(o => o.Value));
        }
        else
        {
            Assert.DoesNotContain(text2, aggText.TopOccurrences.Select(o => o.Value));
        }
    }

    [Fact]
    public async Task Test_GroupBy_Aggregation_Argument()
    {
        var collectionClient = await CollectionFactory(
            properties: new[] { Property.Text("text"), Property.Int("int") }
        );

        await collectionClient.Data.Insert(new { text = "some text", @int = 1 });
        await collectionClient.Data.Insert(new { text = "some text", @int = 2 });

        // Group by "text"
        var resultByText = await collectionClient.Aggregate.OverAll(
            groupBy: new Aggregate.GroupBy("text"),
            metrics: new[]
            {
                Metrics.ForProperty("text").Text(count: true),
                Metrics.ForProperty("int").Integer(count: true),
            }
        );

        Assert.Single(resultByText.Groups);
        var groupByText = resultByText.Groups[0];
        Assert.Equal("text", groupByText.GroupedBy.Property);
        Assert.Equal("some text", groupByText.GroupedBy.Value);
        Assert.IsType<Aggregate.Text>(groupByText.Properties["text"]);
        Assert.Equal(2, ((Aggregate.Text)groupByText.Properties["text"]).Count);
        Assert.IsType<Aggregate.Integer>(groupByText.Properties["int"]);
        Assert.Equal(2, ((Aggregate.Integer)groupByText.Properties["int"]).Count);

        // Group by "int"
        var resultByInt = await collectionClient.Aggregate.OverAll(
            groupBy: new Aggregate.GroupBy("int"),
            metrics: new[]
            {
                Metrics.ForProperty("text").Text(count: true),
                Metrics.ForProperty("int").Integer(count: true),
            }
        );

        Assert.Equal(2, resultByInt.Groups.Count);

        var group1 = resultByInt.Groups[0];
        var group2 = resultByInt.Groups[1];

        Assert.Equal("int", group1.GroupedBy.Property);
        Assert.Equal("int", group2.GroupedBy.Property);

        var values = new[] { group1.GroupedBy.Value, group2.GroupedBy.Value };
        Assert.Contains(1.0, values);
        Assert.Contains(2.0, values);

        Assert.IsType<Aggregate.Text>(group1.Properties["text"]);
        Assert.Equal(1, ((Aggregate.Text)group1.Properties["text"]).Count);
        Assert.IsType<Aggregate.Integer>(group1.Properties["int"]);
        Assert.Equal(1, ((Aggregate.Integer)group1.Properties["int"]).Count);

        Assert.IsType<Aggregate.Text>(group2.Properties["text"]);
        Assert.Equal(1, ((Aggregate.Text)group2.Properties["text"]).Count);
        Assert.IsType<Aggregate.Integer>(group2.Properties["int"]);
        Assert.Equal(1, ((Aggregate.Integer)group2.Properties["int"]).Count);
    }
}

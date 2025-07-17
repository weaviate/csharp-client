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

        var items = Enumerable.Range(0, howMany).Select(_ => new { });
        await collectionClient.Data.InsertMany([.. items]);

        var result = await collectionClient.Aggregate.OverAll();
        Assert.Equal(howMany, result.TotalCount);
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
}

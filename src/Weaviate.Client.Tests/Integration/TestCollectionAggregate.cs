using Weaviate.Client.Models;

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
            vectorConfig: Configure.Vectors.SelfProvided().New()
        );

        await collectionClient.Data.InsertMany(
            Enumerable.Repeat(BatchInsertRequest.Create(new { }), howMany),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var count = await collectionClient.Count(
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(howMany, Convert.ToInt64(count));
    }

    [Fact]
    public async Task Test_Empty_Aggregation()
    {
        var collectionClient = await CollectionFactory(properties: Property.Text("text"));

        var result = await collectionClient.Aggregate.OverAll(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Test_Simple_Aggregation()
    {
        var collectionClient = await CollectionFactory(properties: Property.Text("text"));

        await collectionClient.Data.Insert(
            new { text = "some text" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collectionClient.Aggregate.OverAll(
            returnMetrics: [Metrics.ForProperty("text").Text(count: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        var text = result.Properties["text"] as Aggregate.Text;
        Assert.NotNull(text);
        Assert.Equal(1, text.Count);
    }

    [Fact]
    public async Task Test_Aggregation_Top_Occurrence_With_Limit()
    {
        var collectionClient = await CollectionFactory(properties: Property.Text("text"));

        await collectionClient.Data.Insert(
            new { text = "one" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { text = "one" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { text = "two" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collectionClient.Aggregate.OverAll(
            returnMetrics: [Metrics.ForProperty("text").Text(minOccurrences: 1)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        var text = result.Properties["text"] as Aggregate.Text;
        Assert.NotNull(text);
        Assert.Single(text.TopOccurrences);
        Assert.Equal(2, text.TopOccurrences[0].Count);
    }

    [Fact]
    public async Task Test_Aggregation_GroupBy_With_Limit()
    {
        var collectionClient = await CollectionFactory(properties: Property.Text("text"));

        await collectionClient.Data.Insert(
            new { text = "one" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { text = "two" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { text = "three" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collectionClient.Aggregate.OverAll(
            groupBy: new Aggregate.GroupBy("text", 2),
            returnMetrics: [Metrics.ForProperty("text").Text(count: true)],
            cancellationToken: TestContext.Current.CancellationToken
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
        var collectionClient = await CollectionFactory(properties: Property.Text("text"));

        var result = await collectionClient.Aggregate.OverAll(
            groupBy: new Aggregate.GroupBy("text", 2),
            returnMetrics: [Metrics.ForProperty("text").Text(count: true)],
            cancellationToken: TestContext.Current.CancellationToken
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
            },
            cancellationToken: TestContext.Current.CancellationToken
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
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collectionClient.Aggregate.OverAll(
            filters: filter,
            returnMetrics:
            [
                Metrics.ForProperty("text").Text(count: true, topOccurrencesValue: true),
            ],
            cancellationToken: TestContext.Current.CancellationToken
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
            new Dictionary<string, object> { { "distance", 1.1 } },
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
            vectorConfig: Configure.Vectors.SelfProvided().New("default")
        );

        var text1 = "some text";
        var text2 = "nothing like the other one at all, not even a little bit";
        var uuid = await collectionClient.Data.Insert(
            new { text = text1 },
            vectors: new[] { 1.0f, 0f, 0f },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var obj = await collectionClient.Query.FetchObjectByID(
            uuid,
            includeVectors: true,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj);
        Assert.True(obj.Vectors.ContainsKey("default"));
        await collectionClient.Data.Insert(
            new { text = text2 },
            vectors: new[] { 0f, 0.0f, 0f },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var nearVector = obj.Vectors["default"];
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
                returnMetrics: metrics,
                limit: Convert.ToUInt32(option["objectLimit"]),
                cancellationToken: TestContext.Current.CancellationToken
            );
        }
        else if (option.ContainsKey("certainty"))
        {
            result = await collectionClient.Aggregate.NearVector(
                nearVector,
                returnMetrics: metrics,
                certainty: Convert.ToDouble(option["certainty"]),
                cancellationToken: TestContext.Current.CancellationToken
            );
        }
        else if (option.ContainsKey("distance"))
        {
            result = await collectionClient.Aggregate.NearVector(
                nearVector,
                returnMetrics: metrics,
                distance: Convert.ToDouble(option["distance"]),
                cancellationToken: TestContext.Current.CancellationToken
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

    private static Dictionary<
        string,
        (Dictionary<string, object> option, int expectedLen)
    > _nearTextAggregationOptions = new()
    {
        ["distance 0.9=2"] = (new Dictionary<string, object> { { "distance", 0.9 } }, 2),
        ["limit 1"] = (new Dictionary<string, object> { { "object_limit", 1 } }, 1),
        ["limit 2"] = (new Dictionary<string, object> { { "object_limit", 2 } }, 2),
        ["certainty 0.1"] = (new Dictionary<string, object> { { "certainty", 0.1 } }, 2),
        ["move_away concepts"] = (
            new Dictionary<string, object>
            {
                { "move_away", new Move(concepts: new[] { "something" }, force: 0.000001f) },
                { "distance", 0.9 },
            },
            2
        ),
        ["move_away uuid1"] = (
            new Dictionary<string, object>
            {
                { "move_away", new Move(objects: new[] { _reusableUuids[0] }, force: 0.000001f) },
                { "distance", 0.9 },
            },
            2
        ),
        ["move_away concepts array"] = (
            new Dictionary<string, object>
            {
                {
                    "move_away",
                    new Move(concepts: new[] { "something", "else" }, force: 0.000001f)
                },
                { "distance", 0.9 },
            },
            2
        ),
        ["move_to uuid1 uuid2"] = (
            new Dictionary<string, object>
            {
                {
                    "move_to",
                    new Move(
                        objects: new[] { _reusableUuids[0], _reusableUuids[1] },
                        force: 0.000001f
                    )
                },
                { "distance", 0.9 },
            },
            2
        ),
    };

    public static TheoryData<string> NearTextAggregationOptions()
    {
        return [.. _nearTextAggregationOptions.Keys];
    }

    [Theory]
    [MemberData(nameof(NearTextAggregationOptions))]
    public async Task Test_Near_Text_Aggregation(string usecase)
    {
        // Arrange
        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();

        var (option, expectedLen) = _nearTextAggregationOptions[usecase];
        var collectionClient = await CollectionFactory(
            properties: new[] { Property.Text("text") },
            vectorConfig: Configure
                .Vectors.Text2VecTransformers(vectorizeCollectionName: true)
                .New("default")
        );
        var text1 = "some text";
        var text2 = "nothing like the other one at all, not even a little bit";
        await collectionClient.Data.Insert(
            new { text = text1 },
            id: uuid1,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { text = text2 },
            id: uuid2,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(
            2UL,
            await collectionClient.Count(cancellationToken: TestContext.Current.CancellationToken)
        );

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
                text1,
                returnMetrics: metrics,
                limit: Convert.ToUInt32(option["object_limit"]),
                cancellationToken: TestContext.Current.CancellationToken
            );
        }
        else if (option.ContainsKey("certainty"))
        {
            res = await collectionClient.Aggregate.NearText(
                [text1],
                returnMetrics: metrics,
                certainty: Convert.ToDouble(option["certainty"]),
                cancellationToken: TestContext.Current.CancellationToken
            );
        }
        else if (option.ContainsKey("distance"))
        {
            res = await collectionClient.Aggregate.NearText(
                new[] { text1 }!,
                returnMetrics: metrics,
                distance: Convert.ToDouble(option["distance"]),
                cancellationToken: TestContext.Current.CancellationToken
            );
        }
        else
        {
            // Handle move_to/move_away or other options
            var moveTo = option.ContainsKey("move_to") ? option["move_to"] as Move : null;
            var moveAway = option.ContainsKey("move_away") ? option["move_away"] as Move : null;
            res = await collectionClient.Aggregate.NearText(
                new[] { text1 }!,
                returnMetrics: metrics,
                moveTo: moveTo,
                moveAway: moveAway,
                cancellationToken: TestContext.Current.CancellationToken
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

        await collectionClient.Data.Insert(
            new { text = "some text", @int = 1 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { text = "some text", @int = 2 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Group by "text"
        var resultByText = await collectionClient.Aggregate.OverAll(
            groupBy: "text", // shorthand for new Aggregate.GroupBy("text")
            returnMetrics:
            [
                Metrics.ForProperty("text").Text(count: true),
                Metrics.ForProperty("int").Integer(count: true),
            ],
            cancellationToken: TestContext.Current.CancellationToken
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
            groupBy: "int", // shorthand for new Aggregate.GroupBy("int")
            returnMetrics:
            [
                Metrics.ForProperty("text").Text(count: true),
                Metrics.ForProperty("int").Integer(count: true),
            ],
            cancellationToken: TestContext.Current.CancellationToken
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

    [Theory]
    [InlineData("text", new object[] { "some text", "another text", "some text" })]
    [InlineData("int", new object[] { 42, 42, 100 })]
    [InlineData("float", new object[] { 3.14, 5.67, 3.14 })]
    [InlineData("bool", new object[] { true, true, false, false, false })]
    [InlineData(
        "date",
        new[] { "2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z", "2023-01-03T00:00:00Z" }
    )]
    public async Task Test_Simple_Aggregation_AllTypes(string propertyType, object[] values)
    {
        Property property = propertyType switch
        {
            "text" => Property.Text("text"),
            "int" => Property.Int("int"),
            "float" => Property.Number("float"),
            "bool" => Property.Bool("bool"),
            "date" => Property.Date("date"),
            _ => throw new ArgumentException("Unknown property type"),
        };

        var collectionClient = await CollectionFactory(properties: new[] { property });

        object[] insertObj = propertyType switch
        {
            "text" => values.Select(value => new { text = Convert.ToString(value) }).ToArray(),
            "int" => values.Select(value => new { @int = Convert.ToInt32(value) }).ToArray(),
            "float" => values.Select(value => new { @float = Convert.ToSingle(value) }).ToArray(),
            "bool" => values.Select(value => new { @bool = Convert.ToBoolean(value) }).ToArray(),
            "date" => values
                .Select(value => new { date = DateTime.Parse((string)value).ToUniversalTime() })
                .ToArray(),
            _ => throw new ArgumentException("Unknown property type"),
        };

        await collectionClient.Data.InsertMany(
            insertObj,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Aggregate.Metric metric = propertyType switch
        {
            "text" => Metrics
                .ForProperty("text")
                .Text(count: true, topOccurrencesCount: true, topOccurrencesValue: true),
            "int" => Metrics
                .ForProperty("int")
                .Integer(
                    count: true,
                    minimum: true,
                    maximum: true,
                    mean: true,
                    sum: true,
                    median: true
                ),
            "float" => Metrics
                .ForProperty("float")
                .Number(
                    count: true,
                    minimum: true,
                    maximum: true,
                    mean: true,
                    sum: true,
                    median: true
                ),
            "bool" => Metrics
                .ForProperty("bool")
                .Boolean(count: true, totalTrue: true, totalFalse: true),
            "date" => Metrics.ForProperty("date").Date(count: true, minimum: true, maximum: true),
            _ => throw new ArgumentException("Unknown property type"),
        };

        var result = await collectionClient.Aggregate.OverAll(
            returnMetrics: [metric],
            cancellationToken: TestContext.Current.CancellationToken
        );

        switch (propertyType)
        {
            case "text":
                var textAgg = result.Properties["text"] as Aggregate.Text;
                Assert.NotNull(textAgg);
                Assert.Equal(3, textAgg.Count);
                Assert.Equal(2, textAgg.TopOccurrences.Count);
                Assert.Equal(values[0], textAgg.TopOccurrences[0].Value);
                break;
            case "int":
                var intAgg = result.Properties["int"] as Aggregate.Integer;
                Assert.NotNull(intAgg);
                Assert.Equal(3, intAgg.Count);
                Assert.Equal(Convert.ToInt64(values[0]), intAgg.Minimum);
                Assert.Equal(Convert.ToInt64(values[2]), intAgg.Maximum);
                Assert.Equal(Convert.ToInt64(values[1]), intAgg.Median);
                Assert.Equal(values.Select(Convert.ToDouble).Average(), intAgg.Mean);
                Assert.Equal(values.Select(Convert.ToInt64).Sum(), intAgg.Sum);
                break;
            case "float":
                var floatAgg = result.Properties["float"] as Aggregate.Number;
                Assert.NotNull(floatAgg);
                Assert.Equal(3, floatAgg.Count);
                Assert.Equal(Convert.ToDouble(values[0]), floatAgg.Minimum!.Value, 4);
                Assert.Equal(Convert.ToDouble(values[1]), floatAgg.Maximum!.Value, 4);
                Assert.Equal(Convert.ToDouble(values[2]), floatAgg.Median!.Value, 4);
                Assert.Equal(values.Select(Convert.ToDouble).Average(), floatAgg.Mean!.Value, 4);
                Assert.Equal(values.Select(Convert.ToDouble).Sum(), floatAgg.Sum!.Value, 4);
                break;
            case "bool":
                var boolAgg = result.Properties["bool"] as Aggregate.Boolean;
                Assert.NotNull(boolAgg);
                Assert.Equal(5, boolAgg.Count);
                Assert.Equal(2, boolAgg.TotalTrue);
                Assert.Equal(3, boolAgg.TotalFalse);
                Assert.Equal(0.40, boolAgg.PercentageTrue);
                Assert.Equal(0.60, boolAgg.PercentageFalse);
                break;
            case "date":
                var dateAgg = result.Properties["date"] as Aggregate.Date;
                Assert.NotNull(dateAgg);
                Assert.Equal(3, dateAgg.Count);
                var expectedDate1 = DateTime.Parse((string)values[0]).ToUniversalTime();
                var expectedDate2 = DateTime.Parse((string)values[1]).ToUniversalTime();
                var expectedDate3 = DateTime.Parse((string)values[2]).ToUniversalTime();
                Assert.Equal(expectedDate1, dateAgg.Minimum);
                Assert.Equal(expectedDate3, dateAgg.Maximum);
                break;
        }
    }

    [Theory]
    [InlineData("texts", new[] { "a", "b", "c" })]
    [InlineData("ints", new[] { 1, 2, 3 })]
    [InlineData("floats", new[] { 1.1, 2.2, 3.3 })]
    [InlineData("bools", new[] { true, false, true })]
    [InlineData("dates", new[] { "2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z" })]
    public async Task Test_Simple_Aggregation_ArrayTypes(string propertyType, object value)
    {
        Property property = propertyType switch
        {
            "texts" => Property.TextArray("texts"),
            "ints" => Property.IntArray("ints"),
            "floats" => Property.NumberArray("floats"),
            "bools" => Property.BoolArray("bools"),
            "dates" => Property.DateArray("dates"),
            _ => throw new ArgumentException("Unknown property type"),
        };

        var collectionClient = await CollectionFactory(properties: new[] { property });

        object insertObj = propertyType switch
        {
            "texts" => new { texts = (string[])value },
            "ints" => new { ints = (int[])value },
            "floats" => new { floats = ((double[])value) },
            "bools" => new { bools = (bool[])value },
            "dates" => new
            {
                dates = ((string[])value)
                    .Select(DateTime.Parse)
                    .Select(d => d.ToUniversalTime())
                    .ToArray(),
            },
            _ => throw new ArgumentException("Unknown property type"),
        };

        await collectionClient.Data.Insert(
            insertObj,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Aggregate.Metric metric = propertyType switch
        {
            "texts" => Metrics
                .ForProperty("texts")
                .Text(count: true, topOccurrencesCount: true, topOccurrencesValue: true),
            "ints" => Metrics
                .ForProperty("ints")
                .Integer(count: true, minimum: true, maximum: true, mean: true, sum: true),
            "floats" => Metrics
                .ForProperty("floats")
                .Number(count: true, minimum: true, maximum: true, mean: true, sum: true),
            "bools" => Metrics
                .ForProperty("bools")
                .Boolean(count: true, totalTrue: true, totalFalse: true),
            "dates" => Metrics.ForProperty("dates").Date(count: true, minimum: true, maximum: true),
            _ => throw new ArgumentException("Unknown property type"),
        };

        var result = await collectionClient.Aggregate.OverAll(
            returnMetrics: [metric],
            cancellationToken: TestContext.Current.CancellationToken
        );

        switch (propertyType)
        {
            case "texts":
                var textAgg = result.Properties["texts"] as Aggregate.Text;
                Assert.NotNull(textAgg);
                Assert.Equal(((string[])value).Length, textAgg.Count);
                Assert.Equal(((string[])value).Length, textAgg.TopOccurrences.Count);
                foreach (var v in (string[])value)
                    Assert.Contains(v, textAgg.TopOccurrences.Select(o => o.Value));
                break;
            case "ints":
                var intAgg = result.Properties["ints"] as Aggregate.Integer;
                Assert.NotNull(intAgg);
                Assert.Equal(((int[])value).Length, intAgg.Count);
                Assert.Equal(((int[])value).Min(), intAgg.Minimum);
                Assert.Equal(((int[])value).Max(), intAgg.Maximum);
                Assert.Equal(((int[])value).Average(), intAgg.Mean);
                Assert.Equal(((int[])value).Sum(), intAgg.Sum);
                break;
            case "floats":
                var floatAgg = result.Properties["floats"] as Aggregate.Number;
                Assert.NotNull(floatAgg);
                var floats = (double[])value;
                Assert.Equal(floats.Length, floatAgg.Count);
                Assert.Equal(floats.Min(), floatAgg.Minimum);
                Assert.Equal(floats.Max(), floatAgg.Maximum);
                Assert.Equal(floats.Average(), floatAgg.Mean);
                Assert.Equal(floats.Sum(), floatAgg.Sum);
                break;
            case "bools":
                var boolAgg = result.Properties["bools"] as Aggregate.Boolean;
                Assert.NotNull(boolAgg);
                var bools = (bool[])value;
                Assert.Equal(bools.Length, boolAgg.Count);
                Assert.Equal(bools.Count(b => b), boolAgg.TotalTrue);
                Assert.Equal(bools.Count(b => !b), boolAgg.TotalFalse);
                break;
            case "dates":
                var dateAgg = result.Properties["dates"] as Aggregate.Date;
                Assert.NotNull(dateAgg);
                var dates = ((string[])value)
                    .Select(DateTime.Parse)
                    .Select(d => d.ToUniversalTime())
                    .ToArray();
                Assert.Equal(dates.Length, dateAgg.Count);
                Assert.Equal(dates.Min(), dateAgg.Minimum);
                Assert.Equal(dates.Max(), dateAgg.Maximum);
                break;
        }
    }
}

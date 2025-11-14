using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class SearchTests : IntegrationTests
{
    [Theory]
    [InlineData(HybridFusion.Ranked)]
    [InlineData(HybridFusion.RelativeScore)]
    public async Task Test_SearchHybrid(HybridFusion fusionType)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.Text2VecTransformers().New("default")
        );

        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();

        await collection.Data.Insert(new { Name = "some name" }, id: uuid1);
        await collection.Data.Insert(new { Name = "other word" }, id: uuid2);

        var objs = (
            await collection.Query.Hybrid(
                alpha: 0,
                query: "name",
                fusionType: fusionType,
                includeVectors: true
            )
        ).Objects;

        Assert.Single(objs);

        objs = (
            await collection.Query.Hybrid(
                alpha: 1,
                query: "name",
                fusionType: fusionType,
                vectors: objs.First().Vectors["default"]
            )
        ).Objects;

        Assert.Equal(2, objs.Count());
    }

    [Fact]
    public async Task Test_SearchHybridGroupBy()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.Text2VecTransformers().New("default")
        );

        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();

        await collection.Data.Insert(new { Name = "some name" }, id: uuid1);
        await collection.Data.Insert(new { Name = "other word" }, id: uuid2);

        var objs = (
            await collection.Query.Hybrid(
                alpha: 0,
                query: "name",
                groupBy: new GroupByRequest()
                {
                    PropertyName = "name",
                    ObjectsPerGroup = 1,
                    NumberOfGroups = 2,
                },
                includeVectors: true
            )
        ).Objects;

        Assert.Single(objs);
        Assert.Equal("some name", objs.First().BelongsToGroup);
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    public async Task Test_SearchHybridOnlyVector(string? query)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.Text2VecTransformers().New()
        );

        var uuid = Guid.NewGuid();
        await collection.Data.Insert(new { Name = "some name" }, id: uuid);

        var obj = await collection.Query.FetchObjectByID(uuid, includeVectors: true);
        Assert.NotNull(obj);
        Assert.NotEmpty(obj.Vectors);

        await collection.Data.Insert(new { Name = "other word" }, id: Guid.NewGuid());

        var objs = await collection.Query.Hybrid(
            alpha: 1,
            query: query,
            vectors: obj.Vectors["default"]
        );

        Assert.Equal(2, objs.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Test_Hybrid_Limit(uint limit)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.SelfProvided().New()
        );

        var res = await collection.Data.InsertMany(
            BatchInsertRequest.Create<object>(
                new { Name = "test" },
                new { Name = "another" },
                new { Name = "test" }
            )
        );

        Assert.Equal(0, res.Count(r => r.Error is not null));

        var objs = (await collection.Query.Hybrid(query: "test", alpha: 0, limit: limit)).Objects;

        Assert.Equal(limit, (uint)objs.Count());
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    public async Task Test_Hybrid_Offset(uint offset, int expected)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.SelfProvided().New()
        );

        var res = await collection.Data.InsertMany(
            BatchInsertRequest.Create<object>(
                new { Name = "test" },
                new { Name = "another" },
                new { Name = "test" }
            )
        );

        Assert.Equal(0, res.Count(r => r.Error is not null));

        var objs = (await collection.Query.Hybrid(query: "test", alpha: 0, offset: offset)).Objects;

        Assert.Equal(expected, objs.Count());
    }

    [Fact]
    public async Task Test_Hybrid_Alpha()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vectors.Text2VecTransformers().New("default")
        );

        var res = await collection.Data.InsertMany(
            BatchInsertRequest.Create<object>(
                new { name = "banana" },
                new { name = "fruit" },
                new { name = "car" }
            )
        );
        Assert.Equal(0, res.Count(r => r.Error is not null));

        var hybridRes = (await collection.Query.Hybrid(query: "fruit", alpha: 0)).Objects;
        var bm25Res = (await collection.Query.BM25(query: "fruit")).Objects;
        Assert.Equal(hybridRes.Count(), bm25Res.Count());
        Assert.True(hybridRes.Zip(bm25Res).All(pair => pair.First.ID == pair.Second.ID));

        hybridRes = (await collection.Query.Hybrid(query: "fruit", alpha: 1)).Objects;
        var textRes = (await collection.Query.NearText(text: "fruit")).Objects;
        Assert.Equal(hybridRes.Count(), textRes.Count());
        Assert.True(hybridRes.Zip(textRes).All(pair => pair.First.ID == pair.Second.ID));
    }

    [Fact]
    public async Task Test_Hybrid_Near_Vector_Search()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("text") },
            vectorConfig: Configure.Vectors.Text2VecTransformers().New("default")
        );

        var uuidBanana = Guid.NewGuid();
        await collection.Data.Insert(new { text = "banana" }, id: uuidBanana);
        var obj = await collection.Query.FetchObjectByID(uuidBanana, includeVectors: true);
        Assert.NotNull(obj);

        await collection.Data.Insert(new { text = "dog" });
        await collection.Data.Insert(new { text = "different concept" });

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearVector(obj.Vectors["default"])
            )
        ).Objects;

        Assert.Equal(uuidBanana, hybridObjs.First().ID);
        Assert.Equal(3, hybridObjs.Count());

        var nearVec = (
            await collection.Query.NearVector(
                vector: obj.Vectors["default"],
                returnMetadata: MetadataOptions.Distance
            )
        ).Objects;

        Assert.NotNull(nearVec.First().Metadata.Distance);

        var hybridObjs2 = await collection.Query.Hybrid(
            query: null,
            vectors: new HybridNearVector(
                obj.Vectors["default"],
                Certainty: null,
                Distance: Convert.ToSingle(nearVec.First().Metadata.Distance!.Value + 0.001)
            ),
            returnMetadata: MetadataOptions.All
        );

        Assert.Equal(uuidBanana, hybridObjs2.First().ID);
        Assert.Single(hybridObjs2);
    }

    [Fact]
    public async Task Test_Hybrid_Near_Vector_Search_Named_Vectors()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("text"), Property.Int("int") },
            vectorConfig: new[]
            {
                Configure.Vectors.Text2VecTransformers().New("text"),
                Configure.Vectors.Text2VecTransformers().New("int"),
            }
        );

        var uuidBanana = Guid.NewGuid();
        await collection.Data.Insert(new { text = "banana" }, id: uuidBanana);
        await collection.Data.Insert(new { text = "dog" });
        await collection.Data.Insert(new { text = "different concept" });

        var obj = await collection.Query.FetchObjectByID(uuidBanana, includeVectors: true);

        Assert.NotNull(obj);

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearVector(obj.Vectors["text"]),
                targetVector: ["text"]
            )
        ).Objects;

        Assert.Equal(uuidBanana, hybridObjs.First().ID);
        Assert.Equal(3, hybridObjs.Count());

        var nearVec = (
            await collection.Query.NearVector(
                vector: obj.Vectors["text"],
                targetVector: ["text"],
                returnMetadata: MetadataOptions.Distance
            )
        ).Objects;

        Assert.NotNull(nearVec.First().Metadata.Distance);

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearVector(
                    obj.Vectors["text"],
                    Certainty: null,
                    Distance: Convert.ToSingle(nearVec.First().Metadata.Distance!.Value + 0.001)
                ),
                targetVector: ["text"],
                returnMetadata: MetadataOptions.All
            )
        ).Objects;

        Assert.Equal(uuidBanana, hybridObjs2.First().ID);
        Assert.Single(hybridObjs2);
    }

    [Fact]
    public async Task Test_Hybrid_Near_Text_Search()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("text") },
            vectorConfig: Configure.Vectors.Text2VecTransformers().New()
        );

        var uuidBananaPudding = Guid.NewGuid();
        await collection.Data.Insert(new { text = "banana pudding" }, id: uuidBananaPudding);
        await collection.Data.Insert(new { text = "apple smoothie" });
        await collection.Data.Insert(new { text = "different concept" });

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearText("banana pudding")
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs.First().ID);
        Assert.Equal(3, hybridObjs.Count());

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearText(
                    "banana",
                    Certainty: null,
                    Distance: null,
                    MoveTo: new Move(force: 0.1f, concepts: ["pudding"]),
                    MoveAway: new Move(force: 0.1f, concepts: ["smoothie"])
                ),
                returnMetadata: MetadataOptions.All
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs2.First().ID);
    }

    [Fact]
    public async Task Test_Hybrid_Near_Text_Search_Named_Vectors()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("text"), Property.Int("int") },
            vectorConfig: new[]
            {
                Configure.Vectors.Text2VecTransformers().New("text"),
                Configure.Vectors.Text2VecTransformers().New("int"),
            }
        );

        var uuidBananaPudding = Guid.NewGuid();
        await collection.Data.Insert(new { text = "banana pudding" }, id: uuidBananaPudding);
        await collection.Data.Insert(new { text = "apple smoothie" });
        await collection.Data.Insert(new { text = "different concept" });

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearText("banana pudding"),
                targetVector: ["text"]
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs.First().ID);
        Assert.Equal(3, hybridObjs.Count());

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearText(
                    "banana",
                    Certainty: null,
                    Distance: null,
                    MoveTo: new Move(force: 0.1f, concepts: ["pudding"]),
                    MoveAway: new Move(force: 0.1f, concepts: ["smoothie"])
                ),
                targetVector: ["text"],
                returnMetadata: MetadataOptions.All
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs2.First().ID);
    }

    [Fact]
    public async Task Test_Vector_Per_Target()
    {
        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided().New("first"),
                Configure.Vectors.SelfProvided().New("second"),
            }
        );

        var vector = new Vectors
        {
            { "first", new float[] { 1, 0 } },
            { "second", new float[] { 1, 0, 0 } },
        };

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new float[] { 1, 0 } },
                { "second", new float[] { 1, 0, 0 } },
            }
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new float[] { 0, 1 } },
                { "second", new float[] { 0, 0, 1 } },
            }
        );

        var objs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearVector(vector),
                targetVector: ["first", "second"]
            )
        ).ToList();

        Assert.Equal(2, objs.Count);
        Assert.Equal(uuid1, objs[0].ID);
        Assert.Equal(uuid2, objs[1].ID);

        objs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new HybridNearVector(vector, Certainty: null, Distance: 0.1f),
                targetVector: new[] { "first", "second" }
            )
        ).Objects.ToList();

        Assert.Single(objs);
        Assert.Equal(uuid1, objs[0].ID);
    }

    // TODO Is Second a list of vectors or a multivector?
    // TODO Hybrid doesn't like that multiple vectors are passed in. How to handle this?
    // public static IEnumerable<object[]> SameTargetVectorMultipleInputCombinationsData =>
    //     new List<object[]>
    //     {
    //         new object[]
    //         {
    //             new VectorContainer
    //             {
    //                 { "first", new float[] { 0, 1 } },
    //                 { "second", new[] { new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 } } },
    //             },
    //             new[] { "first", "second" },
    //         },
    //         new object[]
    //         {
    //             new VectorContainer
    //             {
    //                 { "first", new[] { new float[] { 0, 1 }, new float[] { 0, 1 } } },
    //                 { "second", new float[] { 1, 0, 0 } },
    //             },
    //             new[] { "first", "second" },
    //         },
    //         new object[]
    //         {
    //             new VectorContainer
    //             {
    //                 { "first", new[] { new float[] { 0, 1 }, new float[] { 0, 1 } } },
    //                 { "second", new[] { new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 } } },
    //             },
    //             new[] { "first", "second" },
    //         },
    //         new object[]
    //         {
    //             new HybridNearVector(
    //                 new VectorContainer
    //                 {
    //                     { "first", new float[] { 0, 1 } },
    //                     { "second", new[] { new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 } } },
    //                 }
    //             ),
    //             new[] { "first", "second" },
    //         },
    //         new object[]
    //         {
    //             new HybridNearVector(
    //                 new VectorContainer
    //                 {
    //                     { "first", new[] { new float[] { 0, 1 }, new float[] { 0, 1 } } },
    //                     { "second", new float[] { 1, 0, 0 } },
    //                 }
    //             ),
    //             new[] { "first", "second" },
    //         },
    //         new object[]
    //         {
    //             new HybridNearVector(
    //                 new VectorContainer
    //                 {
    //                     { "first", new[] { new float[] { 0, 1 }, new float[] { 0, 1 } } },
    //                     { "second", new[] { new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 } } },
    //                 }
    //             ),
    //             new[] { "first", "second" },
    //         },
    //     };

    // [Theory]
    // [MemberData(nameof(SameTargetVectorMultipleInputCombinationsData))]
    // public async Task Test_Same_Target_Vector_Multiple_Input_Combinations(
    //     IHybridVectorInput nearVector,
    //     string[] targetVector
    // )
    // {
    //     var collection = await CollectionFactory(
    //         properties: Array.Empty<Property>(),
    //         vectorConfig: new[]
    //         {
    //             Configure.Vectors.SelfProvided("first"),
    //             Configure.Vectors.SelfProvided("second"),
    //         }
    //     );

    //     var uuid1 = await collection.Data.Insert(
    //         new { },
    //         vectors: new()
    //         {
    //             { "first", new float[] { 1, 0 } },
    //             { "second", new float[] { 0, 1, 0 } },
    //         }
    //     );
    //     var uuid2 = await collection.Data.Insert(
    //         new { },
    //         vectors: new()
    //         {
    //             { "first", new float[] { 0, 1 } },
    //             { "second", new float[] { 1, 0, 0 } },
    //         }
    //     );

    //     var objs = (
    //         await collection.Query.Hybrid(
    //             query: null,
    //             vector: nearVector,
    //             targetVector: targetVector,
    //             metadata: MetadataOptions.Full
    //         )
    //     ).ToList();

    //     var uuids = objs.Select(o => o.ID).OrderBy(x => x).ToHashSet();
    //     var expected = new HashSet<Guid?> { uuid1, uuid2 };
    //     Assert.Equal(expected, uuids);
    // }

    [Fact]
    public async Task Test_Vector_Distance()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vectors.Text2VecTransformers().New("default")
        );

        var uuid1 = await collection.Data.Insert(new { }, vectors: new float[] { 1, 0, 0 });
        await collection.Data.Insert(new { }, vectors: new float[] { 0, 1, 0 });
        await collection.Data.Insert(new { }, vectors: new float[] { 0, 0, 1 });

        var objs = (
            await collection.Query.Hybrid("name", vectors: Vector.Create(1f, 0f, 0f), alpha: 0.7f)
        ).ToList();
        Assert.Equal(3, objs.Count);
        Assert.Equal(uuid1, objs[0].ID);

        objs = (
            await collection.Query.Hybrid(
                "name",
                vectors: Vectors.Create(1f, 0f, 0f),
                maxVectorDistance: 0.1f,
                alpha: 0.7f
            )
        ).ToList();
        Assert.Single(objs);
        Assert.Equal(uuid1, objs[0].ID);
    }

    [Fact]
    public async Task Test_Hybrid_BM25_Operators()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vectors.SelfProvided().New()
        );

        var uuid1 = await collection.Data.Insert(
            new { name = "banana one" },
            vectors: new float[] { 1, 0, 0, 0 }
        );
        var uuid2 = await collection.Data.Insert(
            new { name = "banana two" },
            vectors: new float[] { 0, 1, 0, 0 }
        );
        var uuid3 = await collection.Data.Insert(
            new { name = "banana three" },
            vectors: new float[] { 0, 1, 0, 0 }
        );
        var uuid4 = await collection.Data.Insert(
            new { name = "banana four" },
            vectors: new float[] { 1, 0, 0, 0 }
        );

        var objs = (
            await collection.Query.Hybrid(
                "banana two",
                alpha: 0.0f,
                bm25Operator: new BM25Operator.Or(MinimumMatch: 1)
            )
        ).ToList();

        Assert.Equal(4, objs.Count);
        Assert.Equal(uuid2, objs[0].ID);
        var rest = objs.Skip(1).Select(o => o.ID).OrderBy(x => x).ToList();
        var expected = new List<Guid?> { uuid1, uuid3, uuid4 };
        expected.Sort();
        Assert.Equal(expected, rest);
    }

    [Fact]
    public async Task Test_Aggregate_Max_Vector_Distance()
    {
        Assert.Skip("Aggregate Hybrid with Named Vectors not fully supported yet");

        RequireVersion("1.26.4");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vectors.SelfProvided().New()
        );

        await collection.Data.Insert(
            new { name = "banana one" },
            vectors: new float[] { 1, 0, 0, 0 }
        );
        await collection.Data.Insert(
            new { name = "banana two" },
            vectors: new float[] { 0, 1, 0, 0 }
        );
        await collection.Data.Insert(
            new { name = "banana three" },
            vectors: new float[] { 0, 1, 0, 0 }
        );
        await collection.Data.Insert(
            new { name = "banana four" },
            vectors: new float[] { 1, 0, 0, 0 }
        );

        var res = await collection.Aggregate.Hybrid(
            query: "banana",
            vectors: new[] { 1f, 0f, 0f, 0f },
            maxVectorDistance: 0.5f,
            targetVector: "default",
            metrics: Metrics.ForProperty("name").Text(count: true)
        );

        Assert.Equal(2, res.TotalCount);
    }
}

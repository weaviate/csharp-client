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
            vectorConfig: Configure.Vectors.Text2VecContextionary().New("default")
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
                metadata: MetadataOptions.Vector
            )
        ).Objects;

        Assert.Single(objs);

        objs = (
            await collection.Query.Hybrid(
                alpha: 1,
                query: "name",
                fusionType: fusionType,
                vector: VectorData.Create(objs.First().Vectors["default"])
            )
        ).Objects;

        Assert.Equal(2, objs.Count());
    }

    [Fact]
    public async Task Test_SearchHybridGroupBy()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.Text2VecContextionary().New("default")
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
                metadata: MetadataOptions.Vector
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
            vectorConfig: Configure.Vectors.Text2VecContextionary().New()
        );

        var uuid = Guid.NewGuid();
        await collection.Data.Insert(new { Name = "some name" }, id: uuid);

        var obj = await collection.Query.FetchObjectByID(uuid, metadata: MetadataOptions.Vector);
        Assert.NotNull(obj);
        Assert.NotEmpty(obj.Vectors);

        await collection.Data.Insert(new { Name = "other word" }, id: Guid.NewGuid());

        var objs = await collection.Query.Hybrid(
            alpha: 1,
            query: query,
            vector: VectorData.Create(obj.Vectors["default"])
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
            vectorConfig: Configure.Vectors.SelfProvided()
        );

        var res = await collection.Data.InsertMany(
            new[] { new { Name = "test" }, new { Name = "another" }, new { Name = "test" } }
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
            vectorConfig: Configure.Vectors.SelfProvided()
        );

        var res = await collection.Data.InsertMany(
            new[] { new { Name = "test" }, new { Name = "another" }, new { Name = "test" } }
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
            vectorConfig: Configure.Vectors.Text2VecContextionary().New("default")
        );

        var res = await collection.Data.InsertMany(
            new[] { new { name = "banana" }, new { name = "fruit" }, new { name = "car" } }
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
            vectorConfig: Configure.Vectors.Text2VecContextionary().New("default")
        );

        var uuidBanana = Guid.NewGuid();
        await collection.Data.Insert(new { text = "banana" }, id: uuidBanana);
        var obj = await collection.Query.FetchObjectByID(
            uuidBanana,
            metadata: MetadataOptions.Vector
        );
        Assert.NotNull(obj);

        await collection.Data.Insert(new { text = "dog" });
        await collection.Data.Insert(new { text = "different concept" });

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vector: new HybridNearVector((VectorData<float>)obj.Vectors["default"])
            )
        ).Objects;

        Assert.Equal(uuidBanana, hybridObjs.First().ID);
        Assert.Equal(3, hybridObjs.Count());

        var nearVec = (
            await collection.Query.NearVector(
                vector: VectorData.Create(obj.Vectors["default"]),
                metadata: MetadataOptions.Distance
            )
        ).Objects;

        Assert.NotNull(nearVec.First().Metadata.Distance);

        var hybridObjs2 = await collection.Query.Hybrid(
            query: null,
            vector: new HybridNearVector(
                (VectorData<float>)obj.Vectors["default"],
                Distance: Convert.ToSingle(nearVec.First().Metadata.Distance!.Value + 0.001)
            ),
            metadata: MetadataOptions.Full
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
                Configure.Vectors.Text2VecContextionary().New("text"),
                Configure.Vectors.Text2VecContextionary().New("int"),
            }
        );

        var uuidBanana = Guid.NewGuid();
        await collection.Data.Insert(new { text = "banana" }, id: uuidBanana);
        await collection.Data.Insert(new { text = "dog" });
        await collection.Data.Insert(new { text = "different concept" });

        var obj = await collection.Query.FetchObjectByID(
            uuidBanana,
            metadata: MetadataOptions.Vector
        );

        Assert.NotNull(obj);

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vector: VectorData.Create(obj.Vectors["text"]),
                targetVector: "text"
            )
        ).Objects;

        Assert.Equal(uuidBanana, hybridObjs.First().ID);
        Assert.Equal(3, hybridObjs.Count());

        var nearVec = (
            await collection.Query.NearVector(
                vector: VectorData.Create(obj.Vectors["text"]),
                targetVector: "text",
                metadata: MetadataOptions.Distance
            )
        ).Objects;

        Assert.NotNull(nearVec.First().Metadata.Distance);

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vector: new HybridNearVector(
                    (VectorData<float>)obj.Vectors["text"],
                    Distance: Convert.ToSingle(nearVec.First().Metadata.Distance!.Value + 0.001)
                ),
                targetVector: "text",
                metadata: MetadataOptions.Full
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
            vectorConfig: Configure.Vectors.Text2VecContextionary().New()
        );

        var uuidBananaPudding = Guid.NewGuid();
        await collection.Data.Insert(new { text = "banana pudding" }, id: uuidBananaPudding);
        await collection.Data.Insert(new { text = "banana smoothie" });
        await collection.Data.Insert(new { text = "different concept" });

        var hybridObjs = (
            await collection.Query.Hybrid(query: null, vector: new HybridNearText("banana pudding"))
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs.First().ID);
        Assert.Equal(3, hybridObjs.Count());

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vector: new HybridNearText(
                    "banana",
                    MoveTo: new Move(force: 0.1f, concepts: ["pudding"]),
                    MoveAway: new Move(force: 0.1f, concepts: ["smoothie"])
                ),
                metadata: MetadataOptions.Full
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
                Configure.Vectors.Text2VecContextionary().New("text"),
                Configure.Vectors.Text2VecContextionary().New("int"),
            }
        );

        var uuidBananaPudding = Guid.NewGuid();
        await collection.Data.Insert(new { text = "banana pudding" }, id: uuidBananaPudding);
        await collection.Data.Insert(new { text = "banana smoothie" });
        await collection.Data.Insert(new { text = "different concept" });

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vector: new HybridNearText("banana pudding"),
                targetVector: "text"
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs.First().ID);
        Assert.Equal(3, hybridObjs.Count());

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vector: new HybridNearText(
                    "banana",
                    MoveTo: new Move(force: 0.1f, concepts: ["pudding"]),
                    MoveAway: new Move(force: 0.1f, concepts: ["smoothie"])
                ),
                targetVector: "text",
                metadata: MetadataOptions.Full
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs2.First().ID);
    }
}

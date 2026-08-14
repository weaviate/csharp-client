using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// The search tests class
/// </summary>
/// <seealso cref="IntegrationTests"/>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1861:Avoid constant arrays as arguments",
    Justification = "Irrelevant"
)]
public partial class SearchTests : IntegrationTests
{
    /// <summary>
    /// Tests that test search hybrid
    /// </summary>
    /// <param name="fusionType">The fusion type</param>
    [Theory]
    [InlineData(HybridFusion.Ranked)]
    [InlineData(HybridFusion.RelativeScore)]
    public async Task Test_SearchHybrid(HybridFusion fusionType)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();

        await collection.Data.Insert(
            new { Name = "some name" },
            uuid: uuid1,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { Name = "other word" },
            uuid: uuid2,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = (
            await collection.Query.Hybrid(
                query: "name",
                vectors: null,
                alpha: 0,
                fusionType: fusionType,
                includeVectors: true,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Single(objs);

        objs = (
            await collection.Query.Hybrid(
                query: "name",
                vectors: new Vectors(objs.First().Vectors["default"]),
                alpha: 1,
                fusionType: fusionType,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(2, objs.Count);
    }

    /// <summary>
    /// Tests that test search hybrid group by
    /// </summary>
    [Fact]
    public async Task Test_SearchHybridGroupBy()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();

        await collection.Data.Insert(
            new { Name = "some name" },
            uuid: uuid1,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { Name = "other word" },
            uuid: uuid2,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = (
            await collection.Query.Hybrid(
                query: "name",
                vectors: null,
                groupBy: new GroupByRequest("name") { ObjectsPerGroup = 1, NumberOfGroups = 2 },
                alpha: 0,
                includeVectors: true,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Single(objs);
        Assert.Equal("some name", objs.First().BelongsToGroup);
    }

    /// <summary>
    /// Tests that test search hybrid only vector
    /// </summary>
    /// <param name="query">The query</param>
    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    public async Task Test_SearchHybridOnlyVector(string? query)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        var uuid = Guid.NewGuid();
        await collection.Data.Insert(
            new { Name = "some name" },
            uuid: uuid,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var obj = await collection.Query.FetchObjectByID(
            uuid,
            includeVectors: true,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj);
        Assert.NotEmpty(obj.Vectors);

        await collection.Data.Insert(
            new { Name = "other word" },
            uuid: Guid.NewGuid(),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = await collection.Query.Hybrid(
            alpha: 1,
            query: query,
            vectors: obj.Vectors["default"],
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(2, objs.Count());
    }

    /// <summary>
    /// Tests that test hybrid limit
    /// </summary>
    /// <param name="limit">The limit</param>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Test_Hybrid_Limit(uint limit)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vector(t => t.SelfProvided())
        );

        var res = await collection.Data.InsertMany(
            BatchInsertRequest.Create([
                new { Name = "test" },
                new { Name = "another" },
                new { Name = "test" },
            ]),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(0, res.Count(r => r.Error is not null));

        var objs = (
            await collection.Query.Hybrid(
                query: "test",
                vectors: null,
                alpha: 0,
                limit: limit,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(limit, (uint)objs.Count);
    }

    /// <summary>
    /// Tests that test hybrid offset
    /// </summary>
    /// <param name="offset">The offset</param>
    /// <param name="expected">The expected</param>
    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    public async Task Test_Hybrid_Offset(uint offset, int expected)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vector(t => t.SelfProvided())
        );

        var res = await collection.Data.InsertMany(
            BatchInsertRequest.Create([
                new { Name = "test" },
                new { Name = "another" },
                new { Name = "test" },
            ]),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(0, res.Count(r => r.Error is not null));

        var objs = (
            await collection.Query.Hybrid(
                query: "test",
                vectors: null,
                alpha: 0,
                offset: offset,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(expected, objs.Count);
    }

    /// <summary>
    /// Tests that test hybrid alpha
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_Alpha()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        var res = await collection.Data.InsertMany(
            BatchInsertRequest.Create([
                new { name = "banana" },
                new { name = "fruit" },
                new { name = "car" },
            ]),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(0, res.Count(r => r.Error is not null));

        var hybridRes = (
            await collection.Query.Hybrid(
                query: "fruit",
                vectors: null,
                alpha: 0,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;
        var bm25Res = (
            await collection.Query.BM25(
                query: "fruit",
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;
        Assert.Equal(hybridRes.Count, bm25Res.Count);
        Assert.True(hybridRes.Zip(bm25Res).All(pair => pair.First.UUID == pair.Second.UUID));

        hybridRes = (
            await collection.Query.Hybrid(
                query: "fruit",
                vectors: null,
                alpha: 1,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;
        var textRes = (
            await collection.Query.NearText(
                query: "fruit",
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;
        Assert.Equal(hybridRes.Count, textRes.Count);
        Assert.True(hybridRes.Zip(textRes).All(pair => pair.First.UUID == pair.Second.UUID));
    }

    /// <summary>
    /// Tests that test hybrid near vector search
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_Near_Vector_Search()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("text") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        var uuidBanana = Guid.NewGuid();
        await collection.Data.Insert(
            new { text = "banana" },
            uuid: uuidBanana,
            cancellationToken: TestContext.Current.CancellationToken
        );
        var obj = await collection.Query.FetchObjectByID(
            uuidBanana,
            includeVectors: true,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj);

        await collection.Data.Insert(
            new { text = "dog" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { text = "different concept" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new NearVectorInput(Vector: obj.Vectors["default"]),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(uuidBanana, hybridObjs.First().UUID);
        Assert.Equal(3, hybridObjs.Count);

        var nearVec = (
            await collection.Query.NearVector(
                obj.Vectors["default"],
                returnMetadata: MetadataOptions.Distance,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.NotNull(nearVec.First().Metadata.Distance);

        var hybridObjs2 = await collection.Query.Hybrid(
            query: null,
            vectors: new NearVectorInput(
                obj.Vectors["default"],
                Certainty: null,
                Distance: Convert.ToSingle(nearVec.First().Metadata.Distance!.Value + 0.001)
            ),
            returnMetadata: MetadataOptions.All,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(uuidBanana, hybridObjs2.First().UUID);
        Assert.Single(hybridObjs2);
    }

    /// <summary>
    /// Tests that test hybrid near vector search named vectors
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_Near_Vector_Search_Named_Vectors()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("text"), Property.Int("int") },
            vectorConfig: new[]
            {
                Configure.Vector("text", v => v.Text2VecTransformers()),
                Configure.Vector("int", v => v.Text2VecTransformers()),
            }
        );

        var uuidBanana = Guid.NewGuid();
        await collection.Data.Insert(
            new { text = "banana" },
            uuid: uuidBanana,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { text = "dog" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { text = "different concept" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var obj = await collection.Query.FetchObjectByID(
            uuidBanana,
            includeVectors: true,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(obj);

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new NearVectorInput(Vector: obj.Vectors["text"]),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(uuidBanana, hybridObjs.First().UUID);
        Assert.Equal(3, hybridObjs.Count);

        var nearVec = (
            await collection.Query.NearVector(
                obj.Vectors["text"],
                returnMetadata: MetadataOptions.Distance,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.NotNull(nearVec.First().Metadata.Distance);

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new NearVectorInput(
                    obj.Vectors["text"],
                    Certainty: null,
                    Distance: Convert.ToSingle(nearVec.First().Metadata.Distance!.Value + 0.001)
                ),
                returnMetadata: MetadataOptions.All,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(uuidBanana, hybridObjs2.First().UUID);
        Assert.Single(hybridObjs2);
    }

    /// <summary>
    /// Tests that test hybrid near text search
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_Near_Text_Search()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("text") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        var uuidBananaPudding = Guid.NewGuid();
        await collection.Data.Insert(
            new { text = "banana pudding" },
            uuid: uuidBananaPudding,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { text = "apple smoothie" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { text = "different concept" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new NearTextInput(Query: "banana pudding"),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs.First().UUID);
        Assert.Equal(3, hybridObjs.Count);

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new NearTextInput(
                    "banana",
                    Certainty: null,
                    Distance: null,
                    MoveTo: new Move(force: 0.1f, concepts: ["pudding"]),
                    MoveAway: new Move(force: 0.1f, concepts: ["smoothie"])
                ),
                returnMetadata: MetadataOptions.All,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs2.First().UUID);
    }

    /// <summary>
    /// Tests that test hybrid near text search named vectors
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_Near_Text_Search_Named_Vectors()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("text"), Property.Int("int") },
            vectorConfig: new[]
            {
                Configure.Vector("text", v => v.Text2VecTransformers()),
                Configure.Vector("int", v => v.Text2VecTransformers()),
            }
        );

        var uuidBananaPudding = Guid.NewGuid();
        await collection.Data.Insert(
            new { text = "banana pudding" },
            uuid: uuidBananaPudding,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { text = "apple smoothie" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { text = "different concept" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var hybridObjs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new NearTextInput(
                    Query: "banana pudding",
                    TargetVectors: new[] { "text" }
                ),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs.First().UUID);
        Assert.Equal(3, hybridObjs.Count);

        var hybridObjs2 = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new NearTextInput(
                    "banana",
                    Certainty: null,
                    Distance: null,
                    MoveTo: new Move(force: 0.1f, concepts: ["pudding"]),
                    MoveAway: new Move(force: 0.1f, concepts: ["smoothie"]),
                    TargetVectors: new[] { "text" }
                ),
                returnMetadata: MetadataOptions.All,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects;

        Assert.Equal(uuidBananaPudding, hybridObjs2.First().UUID);
    }

    /// <summary>
    /// Tests that test vector per target
    /// </summary>
    [Fact]
    public async Task Test_Vector_Per_Target()
    {
        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vector("first", v => v.SelfProvided()),
                Configure.Vector("second", v => v.SelfProvided()),
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
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new Vectors
            {
                { "first", new float[] { 0, 1 } },
                { "second", new float[] { 0, 0, 1 } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new NearVectorInput(Vector: vector),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();

        Assert.Equal(2, objs.Count);
        Assert.Equal(uuid1, objs[0].UUID);
        Assert.Equal(uuid2, objs[1].UUID);

        objs =
        [
            .. (
                await collection.Query.Hybrid(
                    query: null,
                    vectors: new NearVectorInput(Vector: vector, Certainty: null, Distance: 0.1f),
                    cancellationToken: TestContext.Current.CancellationToken
                )
            ).Objects,
        ];

        Assert.Single(objs);
        Assert.Equal(uuid1, objs[0].UUID);
    }

    /// <summary>
    /// Gets the value of the same target vector multiple input combinations data
    /// </summary>
    public static IEnumerable<
        TheoryDataRow<HybridVectorInput>
    > SameTargetVectorMultipleInputCombinationsData =>
        [
            .. new List<HybridVectorInput>
            {
                new NearVectorInput(
                    Vector: new VectorSearchInput
                    {
                        { "first", new float[] { 0, 1 } },
                        { "second", new float[] { 1, 0, 0 } },
                        { "second", new float[] { 0, 0, 1 } },
                    }
                ),
                new NearVectorInput(
                    Vector: new VectorSearchInput
                    {
                        { "first", new float[] { 0, 1 } },
                        { "first", new float[] { 0, 1 } },
                        { "second", new float[] { 1, 0, 0 } },
                    }
                ),
                new NearVectorInput(
                    Vector: new VectorSearchInput
                    {
                        { "first", new float[] { 0, 1 } },
                        { "first", new float[] { 0, 1 } },
                        { "second", new float[] { 1, 0, 0 } },
                        { "second", new float[] { 0, 0, 1 } },
                    }
                ),
                new NearVectorInput(
                    Vector: new VectorSearchInput
                    {
                        { "first", new float[] { 0, 1 } },
                        { "second", new float[] { 1, 0, 0 } },
                        { "second", new float[] { 0, 0, 1 } },
                    }
                ),
                new NearVectorInput(
                    Vector: new VectorSearchInput
                    {
                        { "first", new float[] { 0, 1 }, new float[] { 0, 1 } },
                        { "second", new float[] { 1, 0, 0 } },
                    }
                ),
                new NearVectorInput(
                    Vector: new VectorSearchInput
                    {
                        { "first", new float[] { 0, 1 }, new float[] { 0, 1 } },
                        { "second", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 } },
                    }
                ),
            },
        ];

    /// <summary>
    /// Tests that test same target vector multiple input combinations
    /// </summary>
    /// <param name="nearVector">The near vector</param>
    [Theory]
    [MemberData(nameof(SameTargetVectorMultipleInputCombinationsData))]
    public async Task Test_Same_Target_Vector_Multiple_Input_Combinations(
        HybridVectorInput nearVector
    )
    {
        var collection = await CollectionFactory(
            properties: Array.Empty<Property>(),
            vectorConfig: new[]
            {
                Configure.Vector("first", t => t.SelfProvided()),
                Configure.Vector("second", t => t.SelfProvided()),
            }
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new()
            {
                { "first", new float[] { 1, 0 } },
                { "second", new float[] { 0, 1, 0 } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            vectors: new()
            {
                { "first", new float[] { 0, 1 } },
                { "second", new float[] { 1, 0, 0 } },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = (
            await collection.Query.Hybrid(
                query: null,
                vectors: nearVector,
                returnMetadata: MetadataOptions.All,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();

        var uuids = objs.Select(o => o.UUID).OrderBy(x => x).ToHashSet();
        var expected = new HashSet<Guid?> { uuid1, uuid2 };
        Assert.Equal(expected, uuids);
    }

    /// <summary>
    /// Tests that test vector distance
    /// </summary>
    [Fact]
    public async Task Test_Vector_Distance()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            vectors: new float[] { 1, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { },
            vectors: new float[] { 0, 1, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { },
            vectors: new float[] { 0, 0, 1 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = (
            await collection.Query.Hybrid(
                "name",
                vectors: new float[] { 1f, 0f, 0f },
                alpha: 0.7f,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Equal(3, objs.Count);
        Assert.Equal(uuid1, objs[0].UUID);

        objs =
        [
            .. (
                await collection.Query.Hybrid(
                    "name",
                    vectors: new float[] { 1f, 0f, 0f },
                    maxVectorDistance: 0.1f,
                    alpha: 0.7f,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            ),
        ];
        Assert.Single(objs);
        Assert.Equal(uuid1, objs[0].UUID);
    }

    /// <summary>
    /// Tests that test hybrid bm 25 operators
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_BM25_Operators()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(t => t.SelfProvided())
        );

        var uuid1 = await collection.Data.Insert(
            new { name = "banana one" },
            vectors: new float[] { 1, 0, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { name = "banana two" },
            vectors: new float[] { 0, 1, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid3 = await collection.Data.Insert(
            new { name = "banana three" },
            vectors: new float[] { 0, 1, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid4 = await collection.Data.Insert(
            new { name = "banana four" },
            vectors: new float[] { 1, 0, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objs = (
            await collection.Query.Hybrid(
                "banana two",
                vectors: null,
                alpha: 0.0f,
                bm25Operator: new BM25Operator.Or(MinimumMatch: 1),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();

        Assert.Equal(4, objs.Count);
        Assert.Equal(uuid2, objs[0].UUID);
        var rest = objs.Skip(1).Select(o => o.UUID).OrderBy(x => x).ToList();
        var expected = new List<Guid?> { uuid1, uuid3, uuid4 };
        expected.Sort();
        Assert.Equal(expected, rest);
    }

    /// <summary>
    /// Tests that test hybrid bm 25 operator and cross
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_BM25_Operator_AndCross()
    {
        RequireVersion("1.38.8");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("title"), Property.Text("body") },
            vectorConfig: Configure.Vector(t => t.SelfProvided())
        );

        // Neither of splitAcross's properties holds both tokens, so only cross-property AND matches it.
        var splitAcross = await collection.Data.Insert(
            new { title = "banana", body = "split" },
            vectors: new float[] { 1, 0, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { title = "banana", body = "bread" },
            vectors: new float[] { 0, 1, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var andObjs = (
            await collection.Query.Hybrid(
                "banana split",
                vectors: null,
                alpha: 0.0f,
                bm25Operator: new BM25Operator.And(),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Empty(andObjs);

        var andCrossObjs = (
            await collection.Query.Hybrid(
                "banana split",
                vectors: null,
                alpha: 0.0f,
                bm25Operator: new BM25Operator.AndCross(),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).ToList();
        Assert.Single(andCrossObjs);
        Assert.Equal(splitAcross, andCrossObjs[0].UUID);
    }

    /// <summary>
    /// Creates a collection with 3 tight clusters (a, b, c) of vectors in 3D.
    /// </summary>
    private async Task<CollectionClient> CreateClusteredCollection()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(t => t.SelfProvided())
        );

        var data = new (string Name, float[] Vector)[]
        {
            ("a1", [1.0f, 0.0f, 0.0f]),
            ("a2", [0.95f, 0.05f, 0.0f]),
            ("a3", [0.9f, 0.1f, 0.0f]),
            ("b1", [0.0f, 1.0f, 0.0f]),
            ("b2", [0.05f, 0.95f, 0.0f]),
            ("c1", [0.0f, 0.0f, 1.0f]),
        };
        foreach (var (name, vector) in data)
        {
            await collection.Data.Insert(
                new { name },
                vectors: vector,
                cancellationToken: TestContext.Current.CancellationToken
            );
        }

        return collection;
    }

    /// <summary>
    /// Tests that test hybrid diversity balance zero differs from balance one
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_Diversity_Balance_Reorders()
    {
        RequireVersion("1.38.6");

        var collection = await CreateClusteredCollection();

        var baseline = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new float[] { 1f, 0f, 0f },
                limit: 3,
                cancellationToken: TestContext.Current.CancellationToken
            )
        )
            .Select(o => o.UUID)
            .ToList();

        var balanceZero = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new float[] { 1f, 0f, 0f },
                diversitySelection: new Diversity.MMR(Limit: 3, Balance: 0.0f),
                limit: 3,
                cancellationToken: TestContext.Current.CancellationToken
            )
        )
            .Select(o => o.UUID)
            .ToList();

        var balanceOne = (
            await collection.Query.Hybrid(
                query: null,
                vectors: new float[] { 1f, 0f, 0f },
                diversitySelection: new Diversity.MMR(Limit: 3, Balance: 1.0f),
                limit: 3,
                cancellationToken: TestContext.Current.CancellationToken
            )
        )
            .Select(o => o.UUID)
            .ToList();

        // Pure diversity picks across clusters, so it must differ from pure relevance,
        // while pure relevance matches the plain hybrid baseline.
        Assert.NotEqual(balanceZero, balanceOne);
        Assert.Equal(baseline, balanceOne);
    }

    /// <summary>
    /// Tests that test hybrid diversity mmr limit caps results
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_Diversity_MMR_Limit_Caps_Results()
    {
        RequireVersion("1.38.6");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(t => t.SelfProvided())
        );

        // Enough items (>25) that a small mmr limit is distinguishable from the server's default limit.
        for (var i = 0; i < 50; i++)
        {
            await collection.Data.Insert(
                new { name = $"t{i}" },
                vectors: new float[] { 1.0f - 0.001f * i, 0f, 0f },
                cancellationToken: TestContext.Current.CancellationToken
            );
        }

        var objs = await collection.Query.Hybrid(
            query: null,
            vectors: new float[] { 1f, 0f, 0f },
            diversitySelection: new Diversity.MMR(Limit: 5, Balance: 0.5f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(5, objs.Count());
    }

    /// <summary>
    /// Tests that test hybrid diversity missing limit errors
    /// </summary>
    [Fact]
    public async Task Test_Hybrid_Diversity_MissingLimit_Errors()
    {
        RequireVersion("1.38.6");

        var collection = await CreateClusteredCollection();

        // The server requires the MMR limit; the client forwards the request unvalidated.
        var exception = await Assert.ThrowsAnyAsync<WeaviateException>(async () =>
            await collection.Query.Hybrid(
                query: null,
                vectors: new float[] { 1f, 0f, 0f },
                diversitySelection: new Diversity.MMR(Balance: 0.5f),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
        Assert.NotNull(exception.InnerException);
        Assert.Contains("MMR limit", exception.InnerException.Message);
    }

    /// <summary>
    /// Tests that test aggregate max vector distance
    /// </summary>
    [Fact]
    public async Task Test_Aggregate_Max_Vector_Distance()
    {
        Assert.Skip("Aggregate Hybrid with Named Vectors not fully supported yet");

        RequireVersion("1.26.4");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(t => t.SelfProvided())
        );

        await collection.Data.Insert(
            new { name = "banana one" },
            vectors: new float[] { 1, 0, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { name = "banana two" },
            vectors: new float[] { 0, 1, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { name = "banana three" },
            vectors: new float[] { 0, 1, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            new { name = "banana four" },
            vectors: new float[] { 1, 0, 0, 0 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var res = await collection.Aggregate.Hybrid(
            query: "banana",
            vectors: ("default", new[] { 1f, 0f, 0f, 0f }),
            maxVectorDistance: 0.5f,
            returnMetrics: [Metrics.ForProperty("name").Text(count: true)],
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(2, res.TotalCount);
    }
}

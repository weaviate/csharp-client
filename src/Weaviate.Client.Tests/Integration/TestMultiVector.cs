namespace Weaviate.Client.Tests.Integration;

using Weaviate.Client.Models;

public class TestMultiVector : IntegrationTests
{
    [Fact]
    public async Task Test_Should_Create_Collection_With_MultiVectors_And_Have_MuveraEncodingConfig()
    {
        RequireVersion("1.31.0");

        var collection = await CollectionFactory(
            name: "TestMultiVectorCollection",
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided().New(name: "regular"),
                Configure
                    .MultiVectors.SelfProvided()
                    .New(
                        name: "colbert",
                        encoding: new VectorIndexConfig.MuveraEncoding(),
                        indexConfig: new VectorIndex.HNSW
                        {
                            MultiVector = new VectorIndexConfig.MultiVectorConfig
                            {
                                Aggregation = VectorIndexConfig.MultiVectorAggregation.MaxSim,
                            },
                        }
                    ),
            }
        );

        var vic =
            (await collection.Config.Get(cancellationToken: TestContext.Current.CancellationToken))
                ?.VectorConfig["colbert"]
                .VectorIndexConfig as VectorIndex.HNSW;

        Assert.NotNull(vic);
        Assert.NotNull(vic.MultiVector);
        Assert.NotNull(vic.MultiVector.Encoding);
    }

    [Fact]
    public async Task Test_Should_Create_Collection_With_MultiVectors()
    {
        var client = await CollectionFactory(
            name: "TestMultiVectorCollection",
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided().New(name: "regular"),
                Configure.MultiVectors.SelfProvided().New(name: "colbert"),
            }
        );

        Assert.NotNull(client);

        var collection = await client.Config.Get(
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(collection);
        Assert.Contains(collection.VectorConfig.Keys, k => k == "regular");
        Assert.Contains(collection.VectorConfig.Keys, k => k == "colbert");
    }

    [Fact]
    public async Task Test_Should_Get_Config_Of_Created_Collection()
    {
        var collection = await CollectionFactory(
            name: "TestMultiVectorCollectionConfig",
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided().New(name: "regular"),
                Configure.MultiVectors.SelfProvided().New(name: "colbert"),
            }
        );

        var config = await collection.Config.Get(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(config);
        Assert.NotNull(config.VectorConfig);
        Assert.True(config.VectorConfig.ContainsKey("regular"));
        Assert.True(config.VectorConfig.ContainsKey("colbert"));

        var regularIndexConfig =
            config.VectorConfig["regular"].VectorIndexConfig as VectorIndex.HNSW;
        var colbertIndexConfig =
            config.VectorConfig["colbert"].VectorIndexConfig as VectorIndex.HNSW;

        Assert.NotNull(regularIndexConfig);
        Assert.NotNull(colbertIndexConfig);

        Assert.Null(regularIndexConfig.MultiVector);
        Assert.NotNull(colbertIndexConfig.MultiVector);
    }

    [Fact]
    public async Task Test_MultiVector_SelfProvided()
    {
        RequireVersion("1.29.0");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("title") },
            vectorConfig: new[]
            {
                Configure
                    .MultiVectors.SelfProvided()
                    .New(
                        name: "colbert",
                        indexConfig: new VectorIndex.HNSW()
                        {
                            MultiVector = new VectorIndexConfig.MultiVectorConfig
                            {
                                Aggregation = VectorIndexConfig.MultiVectorAggregation.MaxSim,
                            },
                        }
                    ),
                Configure.Vectors.SelfProvided().New(name: "regular"),
            }
        );

        var config = await collection.Config.Get(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(config);
        Assert.NotNull(config.VectorConfig);
        Assert.IsType<VectorIndex.HNSW>(config.VectorConfig["colbert"].VectorIndexConfig);

        VectorIndex.HNSW? colbert =
            config.VectorConfig["colbert"].VectorIndexConfig as VectorIndex.HNSW;
        Assert.NotNull(colbert?.MultiVector);

        Assert.Equal(
            VectorIndexConfig.MultiVectorAggregation.MaxSim,
            colbert.MultiVector.Aggregation
        );

        var result = await collection.Data.InsertMany(
            [
                BatchInsertRequest.Create(
                    new { },
                    null,
                    new Vectors()
                    {
                        { "regular", new[] { 1f, 2f } },
                        {
                            "colbert",
                            new[,]
                            {
                                { 1f, 2f },
                                { 4f, 5f },
                            }
                        },
                    }
                ),
            ],
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(0, result.Count(r => r.Error != null));

        Assert.Equal(1UL, await collection.Count(TestContext.Current.CancellationToken));

        var objs = await collection.Query.NearVector(
            new[] { 1f, 2f },
            targetVector: ["regular"],
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(objs);

        objs = await collection.Query.NearVector(
            new[,]
            {
                { 1f, 2f },
                { 3f, 4f },
            },
            targetVector: ["colbert"],
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(objs);

        // Vector Lists not supported
        // objs = await collection.Query.NearVector(
        //     VectorData.Create("regular", new[] { new[] { 1f, 2f }, new[] { 2f, 1f } }),
        //     targetVector: "regular"
        // );
        // Assert.Single(objs);

        objs = await collection.Query.NearVector(
            Vector.Create(
                "colbert",
                new[,]
                {
                    { 1f, 2f },
                    { 3f, 4f },
                }
            ),
            targetVector: ["colbert"],
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(objs);

        objs = await collection.Query.Hybrid(
            query: null,
            vectors: Vector.Create(1f, 2f),
            targetVector: ["regular"],
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(objs);

        objs = await collection.Query.Hybrid(
            query: null,
            vectors: Vectors.Create(
                "default",
                new[,]
                {
                    { 1f, 2f },
                    { 3f, 4f },
                }
            ),
            targetVector: ["colbert"],
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(objs);

        objs = await collection.Query.Hybrid(
            query: null,
            vectors: Vectors.Create(
                "colbert",
                new[,]
                {
                    { 1f, 2f },
                    { 3f, 4f },
                }
            ),
            targetVector: ["colbert"],
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(objs);

        // Vector Lists not supported (yet, but coming soon)
        // objs = await collection.Query.Hybrid(
        //     query: null,
        //     vector: Vectors.Create("colbert", new[] { new[] { 1f, 2f }, new[] { 3f, 4f } }),
        //     targetVector: "colbert"
        // );
        // Assert.Single(objs);
    }

    [Fact]
    public async Task Test_Collection_Fetch_With_Multivector()
    {
        RequireVersion("1.29.0");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: new[]
            {
                Configure
                    .MultiVectors.SelfProvided()
                    .New(
                        name: "default",
                        indexConfig: new VectorIndex.HNSW()
                        {
                            MultiVector = new VectorIndexConfig.MultiVectorConfig
                            {
                                Aggregation = VectorIndexConfig.MultiVectorAggregation.MaxSim,
                            },
                        }
                    ),
            }
        );

        // Generate random vectors for testing
        var random = new Random(42); // Seed for reproducibility
        var vector1 = new float[4, 3];
        var vector2 = new float[4, 3];
        var vector3 = new float[4, 3];
        var vector4 = new float[4, 3];
        var vector5 = new float[4, 3];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                vector1[i, j] = (float)random.NextDouble();
                vector2[i, j] = (float)random.NextDouble();
                vector3[i, j] = (float)random.NextDouble();
                vector4[i, j] = (float)random.NextDouble();
                vector5[i, j] = (float)random.NextDouble();
            }
        }

        // Insert a few items with normal Insert, manually passing "default" vector
        var id1 = await collection.Data.Insert(
            new { name = "Item1" },
            vectors: vector1,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var id2 = await collection.Data.Insert(
            new { name = "Item2" },
            vectors: vector2,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var id3 = await collection.Data.Insert(
            new { name = "Item3" },
            vectors: vector3,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Insert a few more with InsertMany, manually passing "default" vector
        var insertManyResults = await collection.Data.InsertMany(
            [
                BatchInsertRequest.Create(new { name = "Item4" }, vectors: vector4),
                BatchInsertRequest.Create(new { name = "Item5" }, vectors: vector5),
            ],
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(insertManyResults);
        Assert.False(insertManyResults.HasErrors);
        Assert.Equal(2, insertManyResults.Count);

        // Fetch objects with includeVector: true
        var fetchedObjects = await collection.Query.FetchObjects(
            includeVectors: true,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(fetchedObjects);
        Assert.Equal(5, fetchedObjects.Objects.Count);

        // Assert that all objects have the "default" vector and correct data
        var obj1 = fetchedObjects.Objects.First(o => o.ID == id1);
        Assert.NotNull(obj1);
        Assert.Equal("Item1", obj1.Properties["name"]);
        Assert.True(obj1.Vectors.ContainsKey("default"));
        Assert.Equal(4, obj1.Vectors["default"].Dimensions);
        Assert.Equal(3, obj1.Vectors["default"].Count);
        Assert.True(obj1.Vectors["default"].IsMultiVector);
        float[,] obj1Vector = obj1.Vectors["default"];
        Assert.Equal(vector1, obj1Vector);

        var obj2 = fetchedObjects.Objects.First(o => o.ID == id2);
        Assert.NotNull(obj2);
        Assert.Equal("Item2", obj2.Properties["name"]);
        Assert.True(obj2.Vectors.ContainsKey("default"));
        float[,] obj2Vector = obj2.Vectors["default"];
        Assert.Equal(vector2, obj2Vector);

        var obj3 = fetchedObjects.Objects.First(o => o.ID == id3);
        Assert.NotNull(obj3);
        Assert.Equal("Item3", obj3.Properties["name"]);
        Assert.True(obj3.Vectors.ContainsKey("default"));
        float[,] obj3Vector = obj3.Vectors["default"];
        Assert.Equal(vector3, obj3Vector);

        var obj4 = fetchedObjects.Objects.First(o => o.Properties["name"]?.ToString() == "Item4");
        Assert.NotNull(obj4);
        Assert.True(obj4.Vectors.ContainsKey("default"));
        float[,] obj4Vector = obj4.Vectors["default"];
        Assert.Equal(vector4, obj4Vector);

        var obj5 = fetchedObjects.Objects.First(o => o.Properties["name"]?.ToString() == "Item5");
        Assert.NotNull(obj5);
        Assert.True(obj5.Vectors.ContainsKey("default"));
        float[,] obj5Vector = obj5.Vectors["default"];
        Assert.Equal(vector5, obj5Vector);
    }
}

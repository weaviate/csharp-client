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
                        indexConfig: new VectorIndex.HNSW
                        {
                            MultiVector = new VectorIndexConfig.MultiVectorConfig
                            {
                                Aggregation = VectorIndexConfig.MultiVectorAggregation.MaxSim,
                                Encoding = new VectorIndexConfig.MuveraEncoding() { },
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
}

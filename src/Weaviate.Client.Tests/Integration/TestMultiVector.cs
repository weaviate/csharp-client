namespace Weaviate.Client.Tests.Integration;

using Weaviate.Client.Models;

public class TestMultiVector : IntegrationTests
{
    [Fact]
    public async Task Test_Should_Create_Collection_With_MultiVectors()
    {
        var collection = await CollectionFactory(
            name: "TestMultiVectorCollection",
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(name: "regular"),
                Configure.MultiVectors.SelfProvided(name: "colbert"),
            }
        );

        Assert.NotNull(collection);
        Assert.NotNull(collection.Collection);
        Assert.Contains(collection.Collection.VectorConfig.Keys, k => k == "regular");
        Assert.Contains(collection.Collection.VectorConfig.Keys, k => k == "colbert");
    }

    [Fact]
    public async Task Test_Should_Get_Config_Of_Created_Collection()
    {
        var collection = await CollectionFactory(
            name: "TestMultiVectorCollectionConfig",
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(name: "regular"),
                Configure.MultiVectors.SelfProvided(name: "colbert"),
            }
        );

        var config = await collection.Get();
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
        var dummy = await CollectionFactory();
        if (dummy.WeaviateVersion < Version.Parse("1.29.0"))
        {
            Assert.Skip("Skipping test for Weaviate versions < 1.29.0");
        }

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("title") },
            vectorConfig: new[]
            {
                Configure.MultiVectors.SelfProvided(
                    name: "colbert",
                    indexConfig: new VectorIndex.HNSW()
                    {
                        MultiVector = new VectorIndexConfig.MultiVectorConfig
                        {
                            Aggregation = VectorIndexConfig.MultiVectorAggregation.MaxSim,
                        },
                    }
                ),
                Configure.Vectors.SelfProvided(name: "regular"),
            }
        );

        var config = await collection.Get();
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

        // var result = await collection.Data.InsertMany(ins =>
        // {
        //     ins(
        //         new { },
        //         null,
        //         new NamedVectors()
        //         {
        //             { "regular", new[] { 1f, 2f } },
        //             { "colbert", new[] { new[] { 1f, 2f }, new[] { 4f, 5f } } },
        //         }
        //     );
        // });

        // Assert.Equal(1UL, await collection.Count());

        // var objs = collection.Query.NearVector(new[] { 1f, 2f }, targetVector: "regular").Objects;
        // Assert.Single(objs);

        // objs = collection
        //     .Query.NearVector(new[] { new[] { 1f, 2f }, new[] { 3f, 4f } }, targetVector: "colbert")
        //     .Objects;
        // Assert.Single(objs);

        // objs = collection
        //     .Query.NearVector(
        //         new Dictionary<string, object>
        //         {
        //             { "regular", new[] { new[] { 1f, 2f }, new[] { 2f, 1f } } },
        //         },
        //         targetVector: "regular"
        //     )
        //     .Objects;
        // Assert.Single(objs);

        // objs = collection
        //     .Query.NearVector(
        //         new Dictionary<string, object>
        //         {
        //             { "colbert", new[] { new[] { 1f, 2f }, new[] { 3f, 4f } } },
        //         },
        //         targetVector: "colbert"
        //     )
        //     .Objects;
        // Assert.Single(objs);

        // objs = collection
        //     .Query.NearVector(
        //         new Dictionary<string, object>
        //         {
        //             {
        //                 "colbert",
        //                 NearVector.ListOfVectors(new[] { new[] { 1f, 2f }, new[] { 3f, 4f } })
        //             },
        //         },
        //         targetVector: "colbert"
        //     )
        //     .Objects;
        // Assert.Single(objs);

        // objs = collection
        //     .Query.Hybrid(query: null, vector: new[] { 1f, 2f }, targetVector: "regular")
        //     .Objects;
        // Assert.Single(objs);

        // objs = collection
        //     .Query.Hybrid(
        //         query: null,
        //         vector: new[] { new[] { 1f, 2f }, new[] { 3f, 4f } },
        //         targetVector: "colbert"
        //     )
        //     .Objects;
        // Assert.Single(objs);

        // objs = collection
        //     .Query.Hybrid(
        //         query: null,
        //         vector: new Dictionary<string, object>
        //         {
        //             { "colbert", new[] { new[] { 1f, 2f }, new[] { 3f, 4f } } },
        //         },
        //         targetVector: "colbert"
        //     )
        //     .Objects;
        // Assert.Single(objs);

        // objs = collection
        //     .Query.Hybrid(
        //         query: null,
        //         vector: new Dictionary<string, object>
        //         {
        //             {
        //                 "colbert",
        //                 NearVector.ListOfVectors(new[] { new[] { 1f, 2f }, new[] { 3f, 4f } })
        //             },
        //         },
        //         targetVector: "colbert"
        //     )
        //     .Objects;
        // Assert.Single(objs);
    }
}

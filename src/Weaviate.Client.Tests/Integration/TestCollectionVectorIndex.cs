using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// Integration tests for deleting vector indices via
/// DELETE /schema/{className}/vectors/{vectorIndexName}/index
/// </summary>
[Collection("TestCollectionVectorIndex")]
public class TestCollectionVectorIndex : IntegrationTests
{
    /// <summary>
    /// DeleteVectorIndex drops the vector index for a named vector on a 1.37+ server.
    /// </summary>
    [Fact]
    public async Task DeleteVectorIndex_DropsVectorIndex()
    {
        RequireVersion<CollectionConfigClient>(nameof(CollectionConfigClient.DeleteVectorIndex));

        var collection = await CollectionFactory(
            vectorConfig: [Configure.Vector("myVector", v => v.SelfProvided())]
        );

        // Should not throw on a 1.37+ server
        await collection.Config.DeleteVectorIndex(
            "myVector",
            TestContext.Current.CancellationToken
        );
    }

    /// <summary>
    /// Config.Get marks or omits a dropped vector index, and an unrelated update still succeeds.
    /// </summary>
    [Fact]
    public async Task Update_WithDroppedVectorIndex_Succeeds()
    {
        RequireVersion<CollectionConfigClient>(nameof(CollectionConfigClient.DeleteVectorIndex));

        var collection = await CollectionFactory(
            vectorConfig:
            [
                Configure.Vector("kept", v => v.SelfProvided()),
                Configure.Vector("dropped", v => v.SelfProvided()),
            ]
        );

        // Enough data that the drop's cleanup is still running during the test.
        var vector = new float[] { 0.1f, 0.2f, 0.3f };
        await collection.Data.InsertMany(
            Enumerable
                .Range(0, 3000)
                .Select(_ =>
                    BatchInsertRequest.Create(
                        new { },
                        vectors: new Vectors { { "kept", vector }, { "dropped", vector } }
                    )
                ),
            TestContext.Current.CancellationToken
        );

        await collection.Config.DeleteVectorIndex("dropped", TestContext.Current.CancellationToken);

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.IsNotType<VectorIndex.None>(config.VectorConfig["kept"].VectorIndexConfig);
        // The server removes the entry once the drop's cleanup finishes.
        Assert.True(
            !config.VectorConfig.TryGetValue("dropped", out var dropped)
                || (
                    dropped.VectorIndexConfig is VectorIndex.None
                    && dropped.VectorIndexType == "none"
                )
        );

        await collection.Config.Update(
            c => c.Description = "updated",
            TestContext.Current.CancellationToken
        );

        config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.Equal("updated", config.Description);
        Assert.True(
            !config.VectorConfig.TryGetValue("dropped", out dropped)
                || dropped.VectorIndexConfig is VectorIndex.None
        );
    }
}

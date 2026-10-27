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
    /// Creates a collection with two named vectors and drops the index of "dropped".
    /// </summary>
    private async Task<CollectionClient> CollectionWithDroppedVector()
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

        return collection;
    }

    /// <summary>
    /// Config.Get reads a collection with a dropped vector index and marks that vector.
    /// </summary>
    [Fact]
    public async Task Get_WithDroppedVectorIndex_MarksVector()
    {
        var collection = await CollectionWithDroppedVector();

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);

        Assert.IsNotType<VectorIndex.None>(config.VectorConfig["kept"].VectorIndexConfig);
        Assert.IsType<VectorIndex.None>(config.VectorConfig["dropped"].VectorIndexConfig);
        Assert.Equal("none", config.VectorConfig["dropped"].VectorIndexType);
    }

    /// <summary>
    /// An unrelated update succeeds while a vector index is dropped.
    /// </summary>
    [Fact]
    public async Task Update_WithDroppedVectorIndex_Succeeds()
    {
        var collection = await CollectionWithDroppedVector();

        await collection.Config.Update(
            c => c.Description = "updated",
            TestContext.Current.CancellationToken
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.Equal("updated", config.Description);
        Assert.IsType<VectorIndex.None>(config.VectorConfig["dropped"].VectorIndexConfig);
    }

    /// <summary>
    /// Updating the index of a dropped vector is refused by the client.
    /// </summary>
    [Fact]
    public async Task UpdateIndex_OfDroppedVector_Throws()
    {
        var collection = await CollectionWithDroppedVector();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            collection.Config.Update(
                c => c.VectorConfig["dropped"].VectorIndexConfig.UpdateHNSW(h => h.Ef = 42),
                TestContext.Current.CancellationToken
            )
        );
        Assert.Contains("dropped", ex.Message);
    }
}

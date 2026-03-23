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
        RequireVersion<CollectionConfigClient>(
            nameof(CollectionConfigClient.DeleteVectorIndex)
        );

        var collection = await CollectionFactory(
            vectorConfig:
            [
                Configure.Vector("myVector", v => v.SelfProvided()),
            ]
        );

        // Should not throw on a 1.37+ server
        await collection.Config.DeleteVectorIndex(
            "myVector",
            TestContext.Current.CancellationToken
        );
    }
}

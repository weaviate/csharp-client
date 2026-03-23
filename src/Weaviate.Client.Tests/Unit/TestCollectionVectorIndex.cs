using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Tests for deleting vector indices via
/// DELETE /schema/{className}/vectors/{vectorIndexName}/index
/// </summary>
public class CollectionVectorIndexTests
{
    /// <summary>
    /// DeleteVectorIndex throws WeaviateVersionMismatchException when server version is below 1.37
    /// </summary>
    [Fact]
    public async Task DeleteVectorIndex_OnServerBelow1_37_ThrowsVersionMismatchException()
    {
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(serverVersion: "1.36.0");

        await Assert.ThrowsAsync<WeaviateVersionMismatchException>(() =>
            client
                .Collections.Use("Article")
                .Config.DeleteVectorIndex(
                    "myVector",
                    TestContext.Current.CancellationToken
                )
        );
    }

    /// <summary>
    /// DeleteVectorIndex sends a DELETE to the correct endpoint path for a named vector
    /// </summary>
    [Fact]
    public async Task DeleteVectorIndex_SendsDeleteToCorrectPath()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        handler.AddResponse(MockResponses.Ok());

        await client
            .Collections.Use("Article")
            .Config.DeleteVectorIndex(
                "myVector",
                TestContext.Current.CancellationToken
            );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/schema/Article/vectors/myVector/index");
    }

    /// <summary>
    /// DeleteVectorIndex sends a DELETE to the correct endpoint path for the default vector
    /// </summary>
    [Fact]
    public async Task DeleteVectorIndex_DefaultVector_SendsDeleteToCorrectPath()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        handler.AddResponse(MockResponses.Ok());

        await client
            .Collections.Use("Article")
            .Config.DeleteVectorIndex(
                "default",
                TestContext.Current.CancellationToken
            );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/schema/Article/vectors/default/index");
    }
}

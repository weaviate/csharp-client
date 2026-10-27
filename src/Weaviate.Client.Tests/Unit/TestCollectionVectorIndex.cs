using System.Text.Json;
using Weaviate.Client.Models;
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
                .Config.DeleteVectorIndex("myVector", TestContext.Current.CancellationToken)
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
            .Config.DeleteVectorIndex("myVector", TestContext.Current.CancellationToken);

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
            .Config.DeleteVectorIndex("default", TestContext.Current.CancellationToken);

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/schema/Article/vectors/default/index");
    }

    /// <summary>
    /// A dropped vector reads as VectorIndex.None, is sent back unchanged on update and left out on create
    /// </summary>
    [Fact]
    public void DroppedVector_ReadsAsNone_SentBackOnUpdate_OmittedOnCreate()
    {
        var json = """
            {
                "class": "Article",
                "vectorConfig": {
                    "kept": { "vectorIndexType": "hnsw", "vectorIndexConfig": {}, "vectorizer": { "none": {} } },
                    "dropped": { "vectorIndexType": "none", "vectorizer": { "none": {} } }
                }
            }
            """;
        var dto = JsonSerializer.Deserialize<Rest.Dto.Class>(
            json,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        var config = dto!.ToModel();

        Assert.IsType<VectorIndex.None>(config.VectorConfig["dropped"].VectorIndexConfig);
        Assert.Equal("none", config.VectorConfig["dropped"].VectorIndexType);

        var sentBack = config.ToDto().VectorConfig!["dropped"];
        Assert.Equal("none", sentBack.VectorIndexType);
        Assert.Null(sentBack.VectorIndexConfig);

        var create = CollectionConfig.FromCollectionCreate(
            new CollectionCreateParams { Name = "Article", VectorConfig = config.VectorConfig }
        );
        Assert.Equal(["kept"], create.VectorConfig.Keys);
    }

    /// <summary>
    /// Updating the index of a dropped vector throws a clear error
    /// </summary>
    [Fact]
    public void DroppedVector_UpdateIndex_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new VectorIndexConfigUpdate(new VectorIndex.None()).UpdateHNSW(_ => { })
        );
        Assert.Contains("dropped", ex.Message);
    }
}

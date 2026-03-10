using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Tests for dropping inverted indices from collection properties via
/// DELETE /schema/{className}/properties/{propertyName}/index/{indexName}
/// </summary>
public class CollectionPropertyIndexTests
{
    /// <summary>
    /// WeaviateVersionMismatchException inherits from WeaviateClientException
    /// </summary>
    [Fact]
    public void WeaviateVersionMismatchException_IsSubclassOf_WeaviateClientException()
    {
        var ex = new WeaviateVersionMismatchException(
            "DeletePropertyIndex",
            new Version(1, 36, 0),
            new Version(1, 35, 0)
        );
        Assert.IsAssignableFrom<WeaviateClientException>(ex);
    }

    /// <summary>
    /// DeletePropertyIndex throws WeaviateVersionMismatchException when server version is below 1.36
    /// </summary>
    [Fact]
    public async Task DeletePropertyIndex_OnServerBelow1_36_ThrowsVersionMismatchException()
    {
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(serverVersion: "1.35.0");

        await Assert.ThrowsAsync<WeaviateVersionMismatchException>(() =>
            client
                .Collections.Use("Article")
                .Config.DeletePropertyIndex(
                    "title",
                    PropertyIndexType.Filterable,
                    TestContext.Current.CancellationToken
                )
        );
    }

    /// <summary>
    /// DeletePropertyIndex sends a DELETE to the correct endpoint path for Filterable index
    /// </summary>
    [Fact]
    public async Task DeletePropertyIndex_Filterable_SendsDeleteToCorrectPath()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        handler.AddResponse(MockResponses.Ok());

        await client
            .Collections.Use("Article")
            .Config.DeletePropertyIndex(
                "title",
                PropertyIndexType.Filterable,
                TestContext.Current.CancellationToken
            );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/schema/Article/properties/title/index/filterable");
    }

    /// <summary>
    /// DeletePropertyIndex sends a DELETE to the correct endpoint path for Searchable index
    /// </summary>
    [Fact]
    public async Task DeletePropertyIndex_Searchable_SendsDeleteToCorrectPath()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        handler.AddResponse(MockResponses.Ok());

        await client
            .Collections.Use("Article")
            .Config.DeletePropertyIndex(
                "title",
                PropertyIndexType.Searchable,
                TestContext.Current.CancellationToken
            );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/schema/Article/properties/title/index/searchable");
    }

    /// <summary>
    /// DeletePropertyIndex sends a DELETE to the correct endpoint path for RangeFilters index
    /// </summary>
    [Fact]
    public async Task DeletePropertyIndex_RangeFilters_SendsDeleteToCorrectPath()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        handler.AddResponse(MockResponses.Ok());

        await client
            .Collections.Use("Article")
            .Config.DeletePropertyIndex(
                "publishedAt",
                PropertyIndexType.RangeFilters,
                TestContext.Current.CancellationToken
            );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/schema/Article/properties/publishedAt/index/rangeFilters");
    }

    /// <summary>
    /// PropertyIndexType enum values map to the correct API strings
    /// </summary>
    [Theory]
    [InlineData(PropertyIndexType.Filterable, "filterable")]
    [InlineData(PropertyIndexType.Searchable, "searchable")]
    [InlineData(PropertyIndexType.RangeFilters, "rangeFilters")]
    public void PropertyIndexType_ToApiString_ReturnsCorrectValue(
        PropertyIndexType indexType,
        string expectedApiString
    )
    {
        Assert.Equal(expectedApiString, indexType.ToApiString());
    }
}

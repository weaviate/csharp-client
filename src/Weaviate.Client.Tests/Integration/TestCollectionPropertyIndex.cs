using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// Integration tests for dropping inverted indices from collection properties via
/// DELETE /schema/{className}/properties/{propertyName}/index/{indexName}
/// </summary>
[Collection("TestCollectionPropertyIndex")]
public class TestCollectionPropertyIndex : IntegrationTests
{
    /// <summary>
    /// DeletePropertyIndex drops the filterable index from a property on a 1.36+ server.
    /// </summary>
    [Fact]
    public async Task DeletePropertyIndex_DropsFilterableIndex()
    {
        RequireVersion<CollectionConfigClient>(nameof(CollectionConfigClient.DeletePropertyIndex));

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title",
                    DataType = DataType.Text,
                    IndexFilterable = true,
                },
            ]
        );

        // Should not throw on a 1.36+ server
        await collection.Config.DeletePropertyIndex(
            "title",
            PropertyIndexType.Filterable,
            TestContext.Current.CancellationToken
        );
    }
}

using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class PaginationTests : IntegrationTests
{
    private class Item
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task FetchObjects_AfterCursor_Paginates()
    {
        // Arrange: simple collection with one text property
        var collection = await CollectionFactory<Item>(
            properties: [Property.Text("Name")],
            description: "Pagination test for FetchObjects after cursor"
        );

        // Insert a handful of items
        var insert = await collection.Data.InsertMany(
            Enumerable.Range(1, 5).Select(i => new Item { Name = $"Item-{i}" }),
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Sanity: all inserts succeeded and we have stable IDs
        Assert.Equal(5, insert.Count);
        var allIds = insert.Select(r => r.ID!.Value).ToArray();

        // Act: fetch first page
        var page1 = await collection.Query.FetchObjects(
            limit: 2,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert page1
        Assert.Equal(2, page1.Count());
        var page1Ids = page1.Select(o => o.UUID!.Value).ToArray();
        var afterCursor = page1Ids.Last();

        // Act: fetch second page using `after` cursor
        var page2 = await collection.Query.FetchObjects(
            after: afterCursor,
            limit: 2,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert page2 does not include page1 items and has expected count
        var page2Ids = page2.Select(o => o.UUID!.Value).ToArray();
        Assert.Equal(2, page2Ids.Length);
        Assert.DoesNotContain(page2Ids[0], page1Ids);
        Assert.DoesNotContain(page2Ids[1], page1Ids);

        // Act: fetch final page using `after` from page2
        var finalAfter = page2Ids.Last();
        var page3 = await collection.Query.FetchObjects(
            after: finalAfter,
            limit: 5,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert last page has the remaining single item
        var page3Ids = page3.Select(o => o.UUID!.Value).ToArray();
        Assert.Single(page3Ids);
        Assert.DoesNotContain(page3Ids[0], page1Ids);
        Assert.DoesNotContain(page3Ids[0], page2Ids);
    }
}

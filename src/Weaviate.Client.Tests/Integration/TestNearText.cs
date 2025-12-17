using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

[Collection("SearchTests")]
public partial class SearchTests : IntegrationTests
{
    [Fact]
    public async Task NearTextSearch()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestDataValue>(
            null,
            "Test collection description",
            [Property.Text("value")],
            vectorConfig: Configure.Vector(
                v => v.Text2VecTransformers(),
                sourceProperties: ["value"]
            )
        );

        string[] values = ["Apple", "Mountain climbing", "apple cake", "cake"];
        var tasks = values
            .Select(s => new TestDataValue { Value = s })
            .Select(d =>
                collectionClient.Data.Insert(
                    d,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            );
        Guid[] guids = await Task.WhenAll(tasks);
        var concepts = new[] { "hiking" };

        // Act
        var retriever = await collectionClient.Query.NearText(
            "cake",
            moveTo: new Move(1.0f, objects: [guids[0]]),
            moveAway: new Move(0.5f, concepts: concepts),
            returnProperties: ["value"],
            includeVectors: "default",
            cancellationToken: TestContext.Current.CancellationToken
        );
        var retrieved = retriever.Objects.ToList();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(4, retrieved.Count);

        Assert.Equal(retrieved[0].UUID, guids[2]);
        Assert.Contains("default", retrieved[0].Vectors.Keys);
        Assert.Equal("apple cake", retrieved[0].As<TestDataValue>()?.Value);
    }

    [Fact]
    public async Task Test_Search_NearText_GroupBy()
    {
        // Arrange
        CollectionClient? collectionClient = await CollectionFactory(
            "",
            "Test collection description",
            [Property.Text("value")],
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        string[] values = ["Apple", "Mountain climbing", "apple cake", "cake"];
        var tasks = values
            .Select(s => new { Value = s })
            .Select(d =>
                collectionClient.Data.Insert(
                    d,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            );
        Guid[] guids = await Task.WhenAll(tasks);

        // Act
        var retrieved = await collectionClient.Query.NearText(
            "cake",
            groupBy: new GroupByRequest("value") { NumberOfGroups = 2, ObjectsPerGroup = 100 },
            includeVectors: new[] { "default" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(retrieved.Objects);
        Assert.NotNull(retrieved.Groups);

        var retrievedObjects = retrieved.Objects.OrderBy(o => o.BelongsToGroup).ToArray();

        Assert.Equal(2, retrievedObjects.Length);
        Assert.Equal(2, retrieved.Groups.Count);

        // Verify the expected GUIDs are present (order-independent)
        var retrievedIds = retrievedObjects.Select(o => o.UUID).ToHashSet();
        Assert.Contains(guids[2], retrievedIds); // "apple cake"
        Assert.Contains(guids[3], retrievedIds); // "cake"

        // Verify each object has the default vector
        Assert.All(retrievedObjects, obj => Assert.Contains("default", obj.Vectors.Keys));

        // Verify BelongsToGroup matches the value property for each object
        var cakeObject = retrievedObjects.First(o => o.UUID == guids[3]);
        Assert.Equal("cake", cakeObject.BelongsToGroup);

        var appleCakeObject = retrievedObjects.First(o => o.UUID == guids[2]);
        Assert.Equal("apple cake", appleCakeObject.BelongsToGroup);

        // Optional: Verify the separate fetch still works
        var obj = await collectionClient.Query.FetchObjectByID(
            guids[3],
            includeVectors: new[] { "default" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj);
        Assert.Equal(guids[3], obj.UUID);
        Assert.Contains("default", obj.Vectors.Keys);
    }
}

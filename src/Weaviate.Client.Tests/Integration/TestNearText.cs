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
            vectorConfig: new VectorConfig(
                "default",
                new Vectorizer.Text2VecTransformers() { SourceProperties = ["value"] }
            )
        );

        string[] values = ["Apple", "Mountain climbing", "apple cake", "cake"];
        var tasks = values
            .Select(s => new TestDataValue { Value = s })
            .Select(d => collectionClient.Data.Insert(d));
        Guid[] guids = await Task.WhenAll(tasks);
        var concepts = new[] { "hiking" };

        // Act
        var retriever = await collectionClient.Query.NearText(
            "cake",
            moveTo: new Move(1.0f, objects: [guids[0]]),
            moveAway: new Move(0.5f, concepts: concepts),
            returnProperties: ["value"],
            includeVectors: "default"
        );
        var retrieved = retriever.Objects.ToList();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(4, retrieved.Count());

        Assert.Equal(retrieved[0].ID, guids[2]);
        Assert.Contains("default", retrieved[0].Vectors.Keys);
        Assert.Equal("apple cake", retrieved[0].As<TestDataValue>()?.Value);
    }

    [Fact]
    public async Task Test_Search_NearText_GroupBy()
    {
        // Arrange
        CollectionClient<dynamic>? collectionClient = await CollectionFactory(
            "",
            "Test collection description",
            [Property.Text("value")],
            vectorConfig: new VectorConfig("default", new Vectorizer.Text2VecTransformers())
        );

        string[] values = ["Apple", "Mountain climbing", "apple cake", "cake"];
        var tasks = values
            .Select(s => new { Value = s })
            .Select(d => collectionClient.Data.Insert(d));
        Guid[] guids = await Task.WhenAll(tasks);

        // Act
        var retrieved = await collectionClient.Query.NearText(
            "cake",
            groupBy: new GroupByRequest
            {
                PropertyName = "value",
                NumberOfGroups = 2,
                ObjectsPerGroup = 100,
            },
            includeVectors: new[] { "default" }
        );

        // Assert
        Assert.NotNull(retrieved.Objects);
        Assert.NotNull(retrieved.Groups);

        var retrievedObjects = retrieved.Objects.ToArray();

        Assert.Equal(2, retrieved.Objects.Count());
        Assert.Equal(2, retrieved.Groups.Count());

        var obj = await collectionClient.Query.FetchObjectByID(
            guids[3],
            includeVectors: new[] { "default" }
        );

        Assert.NotNull(obj);
        Assert.Equal(guids[3], obj.ID);
        Assert.Contains("default", obj.Vectors.Keys);

        Assert.Equal(guids[3], retrievedObjects[0].ID);
        Assert.Contains("default", retrievedObjects[0].Vectors.Keys);
        Assert.Equal("cake", retrievedObjects[0].BelongsToGroup);
        Assert.Equal(guids[2], retrievedObjects[1].ID);
        Assert.Contains("default", retrievedObjects[1].Vectors.Keys);
        Assert.Equal("apple cake", retrievedObjects[1].BelongsToGroup);
    }
}

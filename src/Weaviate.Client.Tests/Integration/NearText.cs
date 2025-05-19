using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
    [Fact]
    public async Task NearTextSearch()
    {
        // Arrange
        var collectionClient = await CollectionFactory<TestDataValue>(
            "",
            "Test collection description",
            [Property.Text("value")],
            vectorConfig: new Dictionary<string, VectorConfig>
            {
                {
                    "default",
                    new VectorConfig
                    {
                        Vectorizer = new Dictionary<string, object>
                        {
                            { "text2vec-contextionary", new { vectorizeClassName = false } },
                        },
                        VectorIndexType = "hnsw",
                    }
                },
            }
        );

        string[] values = ["Apple", "Mountain climbing", "apple cake", "cake"];
        var tasks = values
            .Select(s => new TestDataValue { Value = s })
            .Select(d => collectionClient.Data.Insert(d));
        Guid[] guids = await Task.WhenAll(tasks);
        var concepts = "hiking";

        // Act
        var retriever = await collectionClient.Query.NearText(
            "cake",
            moveTo: new Move(1.0f, objects: guids[0]),
            moveAway: new Move(0.5f, concepts: concepts),
            fields: ["value"],
            metadata: new MetadataQuery(Vectors: new HashSet<string>(["default"]))
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
    public async Task NearTextGroupBySearch()
    {
        // Arrange
        CollectionClient<dynamic>? collectionClient = await CollectionFactory(
            "",
            "Test collection description",
            [Property.Text("value")],
            vectorConfig: new Dictionary<string, VectorConfig>
            {
                {
                    "default",
                    new VectorConfig
                    {
                        Vectorizer = new Dictionary<string, object>
                        {
                            { "text2vec-contextionary", new { vectorizeClassName = false } },
                        },
                        VectorIndexType = "hnsw",
                    }
                },
            }
        );

        string[] values = ["Apple", "Mountain climbing", "apple cake", "cake"];
        var tasks = values
            .Select(s => new { Value = s })
            .Select(d => collectionClient.Data.Insert(d));
        Guid[] guids = await Task.WhenAll(tasks);

        // Act
        var retrieved = await collectionClient.Query.NearText(
            "cake",
            new GroupByRequest
            {
                PropertyName = "value",
                NumberOfGroups = 2,
                ObjectsPerGroup = 100,
            },
            metadata: new MetadataQuery(Vectors: ["default"])
        );

        // Assert
        Assert.NotNull(retrieved.Objects);
        Assert.NotNull(retrieved.Groups);

        var retrievedObjects = retrieved.Objects.ToArray();

        Assert.Equal(2, retrieved.Objects.Count());
        Assert.Equal(2, retrieved.Groups.Count());

        var result = await collectionClient.Query.FetchObjectByID(
            guids[3],
            metadata: new MetadataQuery(Vectors: ["default"])
        );
        var obj = result.Objects.First();

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

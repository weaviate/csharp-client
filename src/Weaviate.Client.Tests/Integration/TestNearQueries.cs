using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public class TestNearQueries : IntegrationTests
{
    [Fact]
    public async Task NearObject_Returns_All_Objects_With_Metadata()
    {
        var collectionClient = await CollectionFactory(
            "",
            "Test collection description",
            [Property.Text("Name")],
            vectorConfig: new VectorConfig("default", new Vectorizer.Text2VecTransformers { })
        );

        var uuidBanana = await collectionClient.Data.Insert(new { Name = "Banana" });
        await collectionClient.Data.Insert(new { Name = "Fruit" });
        await collectionClient.Data.Insert(new { Name = "car" });
        await collectionClient.Data.Insert(new { Name = "Mountain" });

        var fullObjects = (
            await collectionClient.Query.NearObject(
                uuidBanana,
                returnMetadata: new MetadataQuery(
                    MetadataOptions.Distance | MetadataOptions.Certainty
                )
            )
        ).Objects.ToList();

        Assert.Equal(4, fullObjects.Count);

        var objectsDistance = (
            await collectionClient.Query.NearObject(
                uuidBanana,
                distance: fullObjects[2].Metadata.Distance
            )
        ).Objects.ToList();

        Assert.Equal(3, objectsDistance.Count);

        var objectsCertainty = (
            await collectionClient.Query.NearObject(
                uuidBanana,
                certainty: fullObjects[2].Metadata.Certainty
            )
        ).Objects.ToList();

        Assert.Equal(3, objectsCertainty.Count);
    }

    [Fact]
    public async Task NearObject_Limit_Returns_Correct_Objects()
    {
        var collectionClient = await CollectionFactory(
            "",
            "Test collection description",
            [Property.Text("Name")],
            vectorConfig: new VectorConfig("default", new Vectorizer.Text2VecTransformers { })
        );

        var uuidBanana = await collectionClient.Data.Insert(new { Name = "Banana" });
        var uuidFruit = await collectionClient.Data.Insert(new { Name = "Fruit" });
        await collectionClient.Data.Insert(new { Name = "car" });
        await collectionClient.Data.Insert(new { Name = "Mountain" });

        var banana = await collectionClient.Query.FetchObjectByID(uuidBanana);
        Assert.NotNull(banana);

        var objs = (
            await collectionClient.Query.NearObject(banana.ID!.Value, limit: 2)
        ).Objects.ToList();

        Assert.Equal(2, objs.Count);
        Assert.Equal(uuidBanana, objs[0].ID);
        Assert.Equal(uuidFruit, objs[1].ID);
    }

    [Fact]
    public async Task NearObject_Offset_Returns_Correct_Objects()
    {
        var collectionClient = await CollectionFactory(
            "",
            "Test collection description",
            [Property.Text("Name")],
            vectorConfig: new VectorConfig("default", new Vectorizer.Text2VecTransformers { })
        );

        var uuidBanana = await collectionClient.Data.Insert(new { Name = "Banana" });
        var uuidFruit = await collectionClient.Data.Insert(new { Name = "Fruit" });
        await collectionClient.Data.Insert(new { Name = "car" });
        await collectionClient.Data.Insert(new { Name = "Mountain" });

        var banana = await collectionClient.Query.FetchObjectByID(uuidBanana);
        Assert.NotNull(banana);

        var objs = (
            await collectionClient.Query.NearObject(banana.ID!.Value, offset: 1)
        ).Objects.ToList();

        Assert.Equal(3, objs.Count);
        Assert.Equal(uuidFruit, objs[0].ID);
    }

    [Fact]
    public async Task NearObject_GroupBy_Returns_Correct_Groups()
    {
        var collectionClient = await CollectionFactory(
            "",
            "Test collection description",
            [Property.Text("Name"), Property.Int("Count")],
            vectorConfig: new VectorConfig("default", new Vectorizer.Text2VecTransformers { })
        );

        var uuidBanana1 = await collectionClient.Data.Insert(new { Name = "Banana", Count = 51 });
        await collectionClient.Data.Insert(new { Name = "Banana", Count = 72 });
        await collectionClient.Data.Insert(new { Name = "car", Count = 12 });
        await collectionClient.Data.Insert(new { Name = "Mountain", Count = 1 });

        var ret = await collectionClient.Query.NearObject(
            uuidBanana1,
            groupBy: new GroupByRequest
            {
                PropertyName = "Name",
                NumberOfGroups = 4,
                ObjectsPerGroup = 10,
            },
            returnMetadata: new MetadataQuery(MetadataOptions.Distance | MetadataOptions.Certainty)
        );

        var objects = ret.Objects.ToList();

        Assert.Equal(4, objects.Count);
        Assert.Equal("Banana", objects[0].BelongsToGroup);
        Assert.Equal("Banana", objects[1].BelongsToGroup);
        Assert.Equal("car", objects[2].BelongsToGroup);
        Assert.Equal("Mountain", objects[3].BelongsToGroup);
    }
}

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

        var uuidBanana = await collectionClient.Data.Insert(
            new { Name = "Banana" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "Fruit" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "car" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "Mountain" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var fullObjects = (
            await collectionClient.Query.NearObject(
                uuidBanana,
                returnMetadata: new MetadataQuery(
                    MetadataOptions.Distance | MetadataOptions.Certainty
                ),
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects.ToList();

        Assert.Equal(4, fullObjects.Count);

        var objectsDistance = (
            await collectionClient.Query.NearObject(
                uuidBanana,
                distance: fullObjects[2].Metadata.Distance,
                cancellationToken: TestContext.Current.CancellationToken
            )
        ).Objects.ToList();

        Assert.Equal(3, objectsDistance.Count);

        var objectsCertainty = (
            await collectionClient.Query.NearObject(
                uuidBanana,
                certainty: fullObjects[2].Metadata.Certainty,
                cancellationToken: TestContext.Current.CancellationToken
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

        var uuidBanana = await collectionClient.Data.Insert(
            new { Name = "Banana" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuidFruit = await collectionClient.Data.Insert(
            new { Name = "Fruit" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "car" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "Mountain" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var banana = await collectionClient.Query.FetchObjectByID(
            uuidBanana,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(banana);

        var objs = (
            await collectionClient.Query.NearObject(
                banana.ID!.Value,
                limit: 2,
                cancellationToken: TestContext.Current.CancellationToken
            )
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

        var uuidBanana = await collectionClient.Data.Insert(
            new { Name = "Banana" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuidFruit = await collectionClient.Data.Insert(
            new { Name = "Fruit" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "car" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "Mountain" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var banana = await collectionClient.Query.FetchObjectByID(
            uuidBanana,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(banana);

        var objs = (
            await collectionClient.Query.NearObject(
                banana.ID!.Value,
                offset: 1,
                cancellationToken: TestContext.Current.CancellationToken
            )
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

        var uuidBanana1 = await collectionClient.Data.Insert(
            new { Name = "Banana", Count = 51 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "Banana", Count = 72 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "car", Count = 12 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await collectionClient.Data.Insert(
            new { Name = "Mountain", Count = 1 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var ret = await collectionClient.Query.NearObject(
            uuidBanana1,
            groupBy: new GroupByRequest
            {
                PropertyName = "Name",
                NumberOfGroups = 4,
                ObjectsPerGroup = 10,
            },
            returnMetadata: new MetadataQuery(MetadataOptions.Distance | MetadataOptions.Certainty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objects = ret.Objects.ToList();

        Assert.Equal(4, objects.Count);
        Assert.Equal(objects[0].Properties["name"], objects[0].BelongsToGroup);
        Assert.Equal(objects[1].Properties["name"], objects[1].BelongsToGroup);
        Assert.Equal(objects[2].Properties["name"], objects[2].BelongsToGroup);
        Assert.Equal(objects[3].Properties["name"], objects[3].BelongsToGroup);
    }
}

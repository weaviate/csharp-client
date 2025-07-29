using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class SearchTests : IntegrationTests
{
    [Theory]
    [InlineData(HybridFusion.Ranked)]
    [InlineData(HybridFusion.RelativeScore)]
    public async Task Test_SearchHybrid(HybridFusion fusionType)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.Text2VecContextionary().New("default")
        );

        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();

        await collection.Data.Insert(new { Name = "some name" }, id: uuid1);
        await collection.Data.Insert(new { Name = "other word" }, id: uuid2);

        var objs = (
            await collection.Query.Hybrid(
                alpha: 0,
                query: "name",
                fusionType: fusionType,
                metadata: MetadataOptions.Vector
            )
        ).Objects;

        Assert.Single(objs);

        objs = (
            await collection.Query.Hybrid(
                alpha: 1,
                query: "name",
                fusionType: fusionType,
                vector: VectorData.Create(objs.First().Vectors["default"])
            )
        ).Objects;

        Assert.Equal(2, objs.Count());
    }

    [Fact]
    public async Task Test_SearchHybridGroupBy()
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.Text2VecContextionary().New("default")
        );

        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();

        await collection.Data.Insert(new { Name = "some name" }, id: uuid1);
        await collection.Data.Insert(new { Name = "other word" }, id: uuid2);

        var objs = (
            await collection.Query.Hybrid(
                alpha: 0,
                query: "name",
                groupBy: new GroupByRequest()
                {
                    PropertyName = "name",
                    ObjectsPerGroup = 1,
                    NumberOfGroups = 2,
                },
                metadata: MetadataOptions.Vector
            )
        ).Objects;

        Assert.Single(objs);
        Assert.Equal("some name", objs.First().BelongsToGroup);
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    public async Task Test_SearchHybridOnlyVector(string? query)
    {
        var collection = await CollectionFactory(
            properties: new[] { Property.Text("Name") },
            vectorConfig: Configure.Vectors.Text2VecContextionary().New()
        );

        var uuid = Guid.NewGuid();
        await collection.Data.Insert(new { Name = "some name" }, id: uuid);

        var obj = await collection.Query.FetchObjectByID(uuid, metadata: MetadataOptions.Vector);
        Assert.NotNull(obj);
        Assert.NotEmpty(obj.Vectors);

        await collection.Data.Insert(new { Name = "other word" }, id: Guid.NewGuid());

        var objs = await collection.Query.Hybrid(
            alpha: 1,
            query: query,
            vector: VectorData.Create(obj.Vectors["default"])
        );

        Assert.Equal(2, objs.Count());
    }
}

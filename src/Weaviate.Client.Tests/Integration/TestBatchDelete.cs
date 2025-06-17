using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Tests.Integration;

[Collection("BatchDeleteTests")]
public partial class BatchDeleteTests : IntegrationTests
{
    [Fact]
    public async Task Test_Delete_Many_Return()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("Name")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        await collection.Data.InsertMany(batcher =>
        {
            batcher(new { name = "delet me" }, Guid.NewGuid());
        });

        var result = await collection.Data.DeleteMany(
            where: Filter.Property("name").Equal("delet me")
        );

        Assert.Equal(0, result.Failed);
        Assert.Equal(1, result.Matches);
        Assert.Empty(result.Objects);
        Assert.Equal(1, result.Successful);
    }

    [Fact]
    public async Task Test_Delete_Many_Or()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("Name"), Property.Int("Age")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        await collection.Data.InsertMany(batcher =>
        {
            batcher(new { age = 10, name = "Timmy" }, Guid.NewGuid());
            batcher(new { age = 20, name = "Tim" }, Guid.NewGuid());
            batcher(new { age = 30, name = "Timothy" }, Guid.NewGuid());
        });

        var objects = await collection.Query.List();
        Assert.Equal(3, objects.Objects.Count());

        await collection.Data.DeleteMany(
            where: Filter.Property("age").Equal(10) | Filter.Property("age").Equal(30)
        );

        objects = await collection.Query.List();
        Assert.Single(objects.Objects);
        Assert.Equal(20, (long)objects.Objects.First().Properties["age"]!);
        Assert.Equal("Tim", objects.Objects.First().Properties["name"]!);
    }

    [Fact]
    public async Task Test_Delete_Many_And()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("Name"), Property.Int("Age")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        await collection.Data.InsertMany(batcher =>
        {
            batcher(new { age = 10, name = "Timmy" }, Guid.NewGuid());
            batcher(new { age = 10, name = "Tommy" }, Guid.NewGuid());
        });

        var objects = await collection.Query.List();
        Assert.Equal(2, objects.Objects.Count());

        await collection.Data.DeleteMany(
            where: Filter.Property("age").Equal(10) & Filter.Property("name").Equal("Timmy")
        );

        objects = await collection.Query.List();
        Assert.Single(objects.Objects);
        Assert.Equal(10, (long)objects.Objects.First().Properties["age"]!);
        Assert.Equal("Tommy", (string)objects.Objects.First().Properties["name"]!);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_Dry_Run(bool dryRun)
    {
        var collection = await CollectionFactory(
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        var uuid1 = await collection.Data.Insert(new { });
        var uuid2 = await collection.Data.Insert(new { });

        var result = await collection.Data.DeleteMany(
            where: Filter.ID.Equal(uuid1),
            dryRun: dryRun
        );

        Assert.Equal(0, result.Failed);
        Assert.Equal(1, result.Matches);
        Assert.Equal(1, result.Successful);
        Assert.Empty(result.Objects);

        if (dryRun)
        {
            // TODO Requires aggregate support
            // Assert.Equal(2, await collection.Count());
            Assert.NotNull(await collection.Query.FetchObjectByID(uuid1));
        }
        else
        {
            // TODO Requires aggregate support
            // Assert.Equal(1, await collection.Count());
            Assert.Null(await collection.Query.FetchObjectByID(uuid1));
        }

        Assert.NotNull(await collection.Query.FetchObjectByID(uuid2));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_Verbosity(bool verbose)
    {
        var collection = await CollectionFactory(
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
        );

        var uuid1 = await collection.Data.Insert(new { });
        var uuid2 = await collection.Data.Insert(new { });

        var result = await collection.Data.DeleteMany(
            where: Filter.ID.Equal(uuid1),
            verbose: verbose,
            dryRun: false
        );

        Assert.Equal(0, result.Failed);
        Assert.Equal(1, result.Matches);
        Assert.Equal(1, result.Successful);

        if (verbose)
        {
            Assert.NotEmpty(result.Objects);
            Assert.Single(result.Objects);
            Assert.Equal(uuid1, result.Objects.First().Uuid);
            Assert.True(result.Objects.First().Successful);
            Assert.Null(result.Objects.First().Error);
        }
        else
        {
            Assert.Empty(result.Objects);
        }

        // TODO Requires aggregate support
        // Assert.Equal(1, await collection.Count());
        Assert.NotNull(await collection.Query.FetchObjectByID(uuid2));
    }
}

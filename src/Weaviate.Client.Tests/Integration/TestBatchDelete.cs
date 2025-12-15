using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

[Collection("BatchDeleteTests")]
public partial class BatchDeleteTests : IntegrationTests
{
    [Fact]
    public async Task Test_Delete_Many_Return()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("Name")],
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
        );

        await collection.Data.InsertMany(
            [BatchInsertRequest.Create(new { name = "delet me" }, Guid.NewGuid())],
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collection.Data.DeleteMany(
            where: Filter.Property("name").IsEqual("delet me"),
            cancellationToken: TestContext.Current.CancellationToken
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
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
        );

        await collection.Data.InsertMany(
            new[]
            {
                BatchInsertRequest.Create(new { age = 10, name = "Timmy" }, Guid.NewGuid()),
                BatchInsertRequest.Create(new { age = 20, name = "Tim" }, Guid.NewGuid()),
                BatchInsertRequest.Create(new { age = 30, name = "Timothy" }, Guid.NewGuid()),
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objects = await collection.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(3, objects.Objects.Count());

        await collection.Data.DeleteMany(
            where: Filter.Property("age").IsEqual(10) | Filter.Property("age").IsEqual(30),
            cancellationToken: TestContext.Current.CancellationToken
        );

        objects = await collection.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(objects.Objects);
        Assert.Equal(20, (long)objects.Objects.First().Properties["age"]!);
        Assert.Equal("Tim", objects.Objects.First().Properties["name"]!);
    }

    [Fact]
    public async Task Test_Delete_Many_And()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("Name"), Property.Int("Age")],
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
        );

        await collection.Data.InsertMany(
            new (object, Guid)[]
            {
                (new { age = 10, name = "Timmy" }, Guid.NewGuid()),
                (new { age = 10, name = "Tommy" }, Guid.NewGuid()),
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var objects = await collection.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(2, objects.Objects.Count());

        await collection.Data.DeleteMany(
            where: Filter.Property("age").IsEqual(10) & Filter.Property("name").IsEqual("Timmy"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        objects = await collection.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
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
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collection.Data.DeleteMany(
            where: Filter.ID.IsEqual(uuid1),
            dryRun: dryRun,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(0, result.Failed);
        Assert.Equal(1, result.Matches);
        Assert.Equal(1, result.Successful);
        Assert.Empty(result.Objects);

        if (dryRun)
        {
            Assert.Equal(2UL, await collection.Count(TestContext.Current.CancellationToken));
            Assert.NotNull(
                await collection.Query.FetchObjectByID(
                    uuid1,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            );
        }
        else
        {
            Assert.Equal(1UL, await collection.Count(TestContext.Current.CancellationToken));
            Assert.Null(
                await collection.Query.FetchObjectByID(
                    uuid1,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            );
        }

        Assert.NotNull(
            await collection.Query.FetchObjectByID(
                uuid2,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_Verbosity(bool verbose)
    {
        var collection = await CollectionFactory(
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
        );

        var uuid1 = await collection.Data.Insert(
            new { },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var uuid2 = await collection.Data.Insert(
            new { },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collection.Data.DeleteMany(
            where: Filter.ID.IsEqual(uuid1),
            verbose: verbose,
            dryRun: false,
            cancellationToken: TestContext.Current.CancellationToken
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

        Assert.Equal(1UL, await collection.Count(TestContext.Current.CancellationToken));
        Assert.NotNull(
            await collection.Query.FetchObjectByID(
                uuid2,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }
}

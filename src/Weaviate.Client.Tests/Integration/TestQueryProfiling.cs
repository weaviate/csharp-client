using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// Integration tests for query profiling (Weaviate ≥ 1.36).
/// Each test that requests profiling verifies that the result carries a non-null
/// <see cref="Weaviate.Client.Models.QueryProfile"/> with at least one shard entry.
/// The "not requested" test verifies the opt-in semantics: omitting the flag returns null.
/// </summary>
public partial class SearchTests : IntegrationTests
{
    [Fact]
    public async Task QueryProfiling_BM25_Returns_Profile()
    {
        RequireVersion("1.36.9");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(v => v.SelfProvided())
        );

        await collection.Data.Insert(
            new { name = "test object" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collection.Query.BM25(
            query: "test",
            returnMetadata: MetadataOptions.QueryProfile,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(result.QueryProfile);
        Assert.NotEmpty(result.QueryProfile.Shards);
    }

    [Fact]
    public async Task QueryProfiling_NearText_Returns_Profile()
    {
        RequireVersion("1.36.9");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        await collection.Data.Insert(
            new { name = "test object" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collection.Query.NearText(
            query: "test",
            returnMetadata: MetadataOptions.QueryProfile,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(result.QueryProfile);
        Assert.NotEmpty(result.QueryProfile.Shards);
    }

    [Fact]
    public async Task QueryProfiling_Hybrid_Returns_Profile()
    {
        RequireVersion("1.36.9");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        await collection.Data.Insert(
            new { name = "test object" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collection.Query.Hybrid(
            query: "test",
            vectors: null,
            returnMetadata: MetadataOptions.QueryProfile,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(result.QueryProfile);
        Assert.NotEmpty(result.QueryProfile.Shards);
    }

    [Fact]
    public async Task QueryProfiling_NotRequested_Returns_Null()
    {
        RequireVersion("1.36.9");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(v => v.SelfProvided())
        );

        await collection.Data.Insert(
            new { name = "test object" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collection.Query.BM25(
            query: "test",
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Null(result.QueryProfile);
    }

    [Fact]
    public async Task QueryProfiling_NearText_GroupBy_Returns_Profile()
    {
        RequireVersion("1.36.9");

        var collection = await CollectionFactory(
            properties: new[] { Property.Text("name") },
            vectorConfig: Configure.Vector(v => v.Text2VecTransformers())
        );

        await collection.Data.Insert(
            new { name = "first object" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await collection.Data.Insert(
            new { name = "second object" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await collection.Query.NearText(
            query: "test",
            groupBy: new GroupByRequest("name") { NumberOfGroups = 2, ObjectsPerGroup = 1 },
            returnMetadata: MetadataOptions.QueryProfile,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(result.QueryProfile);
        Assert.NotEmpty(result.QueryProfile.Shards);
    }
}

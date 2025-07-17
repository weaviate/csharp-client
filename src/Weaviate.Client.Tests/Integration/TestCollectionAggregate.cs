using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weaviate.Client.Models;
using Xunit;

namespace Weaviate.Client.Tests.Integration;

[Collection("TestCollections")]
public partial class AggregatesTests : IntegrationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10000)]
    [InlineData(20000)]
    [InlineData(20001)]
    [InlineData(100000)]
    public async Task Test_Collection_Length(int howMany)
    {
        var collectionClient = await CollectionFactory(
            vectorConfig: Configure.Vectors.SelfProvided()
        );

        var items = Enumerable.Range(0, howMany).Select(_ => new { });
        await collectionClient.Data.InsertMany([.. items]);

        var result = await collectionClient.Aggregate.OverAll();
        Assert.Equal(howMany, result.TotalCount);
    }

    // [Theory]
    // [InlineData(1)]
    // [InlineData(10000)]
    // [InlineData(20000)]
    // [InlineData(20001)]
    // [InlineData(100000)]
    // public async Task Test_Collection_Length_Tenant(int howMany)
    // {
    //     var collectionClient = await CollectionFactory(
    //     vectorConfig: Configure.Vectors.SelfProvided(),
    //     multiTenancyConfig: new MultiTenancyConfig {
    //         Enabled = true,
    //     }
    //     );

    //     await collectionClient.Tenants.Create(new[]
    //     {
    //         new Tenant("tenant1"),
    //         new Tenant("tenant2"),
    //         new Tenant("tenant3")
    //     });

    //     var tenant2Client = collectionClient.WithTenant("tenant2");
    //     await tenant2Client.Data.InsertMany(Enumerable.Range(0, howMany * 2).Select(_ => new { }).ToList());

    //     var resultTenant2 = await tenant2Client.Aggregate.OverAll();
    //     Assert.Equal(howMany * 2, resultTenant2.TotalCount);

    //     var resultTenant3 = await collectionClient.WithTenant("tenant3").Aggregate.OverAll();
    //     Assert.Equal(0, resultTenant3.TotalCount);
    // }

    [Fact]
    public async Task Test_Empty_Aggregation()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        var result = await collectionClient.Aggregate.OverAll();
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Test_Simple_Aggregation()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        await collectionClient.Data.Insert(new { text = "some text" });

        var result = await collectionClient.Aggregate.OverAll(
            metrics: new[] { Metrics.ForProperty("text").Text(count: true) }
        );

        var text = result.Properties["text"] as Aggregate.Text;
        Assert.NotNull(text);
        Assert.Equal(1, text.Count);
    }

    [Fact]
    public async Task Test_Aggregation_Top_Occurrence_With_Limit()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        await collectionClient.Data.Insert(new { text = "one" });
        await collectionClient.Data.Insert(new { text = "one" });
        await collectionClient.Data.Insert(new { text = "two" });

        var result = await collectionClient.Aggregate.OverAll(
            metrics: new[] { Metrics.ForProperty("text").Text(minOccurrences: 1) }
        );

        var text = result.Properties["text"] as Aggregate.Text;
        Assert.NotNull(text);
        Assert.Single(text.TopOccurrences);
        Assert.Equal(2, text.TopOccurrences[0].Count);
    }

    [Fact]
    public async Task Test_Aggregation_GroupBy_With_Limit()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        await collectionClient.Data.Insert(new { text = "one" });
        await collectionClient.Data.Insert(new { text = "two" });
        await collectionClient.Data.Insert(new { text = "three" });

        var result = await collectionClient.Aggregate.OverAll(
            groupBy: new Aggregate.GroupBy("text", 2),
            metrics: Metrics.ForProperty("text").Text(count: true)
        );

        Assert.Equal(2, result.Groups.Count);
        var group1 = result.Groups[0].Properties["text"] as Aggregate.Text;
        var group2 = result.Groups[1].Properties["text"] as Aggregate.Text;
        Assert.NotNull(group1);
        Assert.NotNull(group2);
        Assert.Equal(1, group1.Count);
        Assert.Equal(1, group2.Count);
    }

    [Fact]
    public async Task Test_Aggregation_GroupBy_No_Results()
    {
        var collectionClient = await CollectionFactory(properties: new[] { Property.Text("text") });

        var result = await collectionClient.Aggregate.OverAll(
            groupBy: new Aggregate.GroupBy("text", 2),
            metrics: Metrics.ForProperty("text").Text(count: true)
        );

        Assert.Empty(result.Groups);
    }
}

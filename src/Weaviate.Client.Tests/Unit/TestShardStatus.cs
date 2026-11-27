using System.Net;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for shard status parsing and the shards endpoint mapping.
/// </summary>
public class TestShardStatus
{
    /// <summary>
    /// Every status core 1.40 reports parses, and an unrecognized one maps to Unknown instead of throwing.
    /// </summary>
    [Theory]
    [InlineData("READY", ShardStatus.Ready)]
    [InlineData("READONLY", ShardStatus.ReadOnly)]
    [InlineData("INDEXING", ShardStatus.Indexing)]
    [InlineData("LOADING", ShardStatus.Loading)]
    [InlineData("LAZY_LOADING", ShardStatus.LazyLoading)]
    [InlineData("SHUTDOWN", ShardStatus.Shutdown)]
    [InlineData("UNAVAILABLE", ShardStatus.Unavailable)]
    [InlineData("RECOVERING", ShardStatus.Recovering)]
    [InlineData("SOMETHING_NEW", ShardStatus.Unknown)]
    [InlineData(null, ShardStatus.Ready)]
    public void ParseStatus_MapsWireValue(string? value, ShardStatus expected)
    {
        Assert.Equal(expected, ShardStatusExtensions.ParseStatus(value));
    }

    /// <summary>
    /// GetShards surfaces per_node_status, and leaves it null when an older server omits it.
    /// </summary>
    [Fact]
    public async Task GetShards_MapsPerNodeStatus()
    {
        var json = """
            [
                {"name":"s1","status":"READY","vectorQueueSize":0,"per_node_status":{"node1":"READY","node2":"LOADING"}},
                {"name":"s2","status":"READY","vectorQueueSize":0}
            ]
            """;
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            }
        );

        var shards = await client
            .Collections.Use("Article")
            .Config.GetShards(TestContext.Current.CancellationToken);

        Assert.Equal(
            new Dictionary<string, ShardStatus>
            {
                ["node1"] = ShardStatus.Ready,
                ["node2"] = ShardStatus.Loading,
            },
            shards[0].PerNodeStatus
        );
        Assert.Null(shards[1].PerNodeStatus);
    }
}

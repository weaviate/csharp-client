using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Rest.Dto;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public partial class RestClientTests
{
    [Fact]
    public async Task Nodes_Deserializes_NodeStatusResponse_Correctly()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        var json =
            @"{
  ""nodes"": [
    {
      ""batchStats"": {
        ""queueLength"": 0,
        ""ratePerSecond"": 6
      },
      ""gitHash"": ""9c22d0c"",
      ""name"": ""node1"",
      ""shards"": [
        {
          ""asyncReplicationStatus"": [],
          ""class"": ""TestNodesVerboseWithCollection_cdaa5ae9195970a533282417820a87d5445f04c60b552593b1efd7e13bd5189d_Object_TestNodesVerboseWithCollection"",
          ""compressed"": false,
          ""loaded"": true,
          ""name"": ""gvCkCYs2fnA3"",
          ""numberOfReplicas"": 1,
          ""objectCount"": 0,
          ""replicationFactor"": 1,
          ""vectorIndexingStatus"": ""READY"",
          ""vectorQueueLength"": 0
        }
      ],
      ""stats"": {
        ""objectCount"": 0,
        ""shardCount"": 1
      },
      ""status"": ""HEALTHY"",
      ""version"": ""1.34.0""
    }
  ]
}";

        var dto = JsonSerializer.Deserialize<NodesStatusResponse>(
            json,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        handler.AddJsonResponse(dto!);

        // Act
        var result = await client.Cluster.Nodes.ListVerbose(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var node = result[0];
        Assert.Equal("node1", node.Name);
        Assert.Equal("9c22d0c", node.GitHash);
        Assert.Equal("HEALTHY", node.StatusRaw);
        Assert.Equal(Weaviate.Client.Models.NodeStatus.Healthy, node.Status);
        Assert.Equal("1.34.0", node.Version);
        Assert.Equal(0, node.Stats.ObjectCount);
        Assert.Equal(1, node.Stats.ShardCount);
        Assert.Single(node.Shards);
        var shard = node.Shards[0];
        Assert.Equal("gvCkCYs2fnA3", shard.Name);
        Assert.Equal(0, shard.ObjectCount);
        Assert.Equal(VectorIndexingStatus.Ready, shard.VectorIndexingStatus);
        Assert.Equal(0, shard.VectorQueueLength);
        Assert.True(shard.Loaded ?? false);
        Assert.False(shard.Compressed);
    }
}

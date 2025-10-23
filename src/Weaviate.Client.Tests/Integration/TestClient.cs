namespace Weaviate.Client.Tests.Integration;

[Collection("ClientTests")]
public partial class ClientTests : IntegrationTests
{
    [Fact]
    public async Task ConnectToLocal()
    {
        var client = Connect.Local();

        var ex = await Record.ExceptionAsync(async () =>
            await client
                .Collections.List()
                .ToListAsync(TestContext.Current.CancellationToken)
                .AsTask()
        );
        Assert.Null(ex);
    }

    [Fact]
    public async Task ConnectToCloud()
    {
        var WCS_HOST = "piblpmmdsiknacjnm1ltla.c1.europe-west3.gcp.weaviate.cloud";
        var WCS_CREDS = "cy4ua772mBlMdfw3YnclqAWzFhQt0RLIN0sl";

        var client = Connect.Cloud(WCS_HOST, WCS_CREDS);

        var ex = await Record.ExceptionAsync(async () =>
            await client.Collections.List().ToListAsync(TestContext.Current.CancellationToken)
        );
        Assert.Null(ex);
    }

    [Fact]
    public async Task TestMeta()
    {
        var client = Connect.Local();
        var meta = await client.GetMeta();

        // ip is different depending on the environment
        Assert.Contains("8080", meta.Hostname);
        Assert.Contains("http://", meta.Hostname);
    }

    [Fact]
    public async Task TestNodesMinimal()
    {
        var client = Connect.Local();
        var nodesMinimal = await client.Cluster.Nodes.NodesMinimal();

        // ip is different depending on the environment
        Assert.Single(nodesMinimal);
        var node = nodesMinimal[0];
        Assert.NotNull(node);
        Assert.NotNull(node.GitHash);
        Assert.NotNull(node.Name);
        Assert.NotNull(node.Version);
    }

    [Fact]
    public async Task TestNodesVerbose()
    {
        var client = Connect.Local();
        var nodesVerbose = await client.Cluster.Nodes.NodesVerbose();
        Assert.Single(nodesVerbose);
        var nodeV = nodesVerbose[0];
        Assert.NotNull(nodeV);
        Assert.NotNull(nodeV.GitHash);
        Assert.NotNull(nodeV.Name);
        Assert.NotNull(nodeV.Version);
        // additional verbose options
        Assert.NotNull(nodeV.Shards);
    }

    [Fact]
    public async Task TestNodesVerboseWithCollection()
    {
        var client = Connect.Local();

        var collectionName = "TestNodesVerboseWithCollection";
        var collection = await CollectionFactory(collectionName);

        var nodesVerbose = await client.Cluster.Nodes.NodesVerbose(collection.Name);
        Assert.Single(nodesVerbose);
        var nodeV = nodesVerbose[0];
        Assert.NotNull(nodeV);
        Assert.NotNull(nodeV.GitHash);
        Assert.NotNull(nodeV.Name);
        Assert.NotNull(nodeV.Version);
        // additional verbose options
        Assert.NotNull(nodeV.Shards);
    }
}

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// The client tests class
/// </summary>
/// <seealso cref="IntegrationTests"/>
[Collection("ClientTests")]
public partial class ClientTests : IntegrationTests
{
    /// <summary>
    /// Tests that connect to local
    /// </summary>
    [Fact]
    public async Task ConnectToLocal()
    {
        var client = await Connect.Local();

        var ex = await Record.ExceptionAsync(async () =>
            await client
                .Collections.List(TestContext.Current.CancellationToken)
                .ToListAsync(TestContext.Current.CancellationToken)
                .AsTask()
        );
        Assert.Null(ex);
    }

    /// <summary>
    /// Tests that connect to cloud
    /// </summary>
    [Fact]
    public async Task ConnectToCloud()
    {
        var WCS_HOST = "piblpmmdsiknacjnm1ltla.c1.europe-west3.gcp.weaviate.cloud";
        var WCS_CREDS = "cy4ua772mBlMdfw3YnclqAWzFhQt0RLIN0sl";

        var client = await Connect.Cloud(WCS_HOST, WCS_CREDS);

        var ex = await Record.ExceptionAsync(async () =>
            await client
                .Collections.List(TestContext.Current.CancellationToken)
                .ToListAsync(TestContext.Current.CancellationToken)
        );
        Assert.Null(ex);
    }

    /// <summary>
    /// Tests that test meta
    /// </summary>
    [Fact]
    public async Task TestMeta()
    {
        var client = await Connect.Local();
        var meta = await client.GetMeta(TestContext.Current.CancellationToken);

        // ip is different depending on the environment
        Assert.Contains("8080", meta.Hostname);
        Assert.Contains("http://", meta.Hostname);
    }

    /// <summary>
    /// Tests that test nodes minimal
    /// </summary>
    [Fact]
    public async Task TestNodesMinimal()
    {
        var nodesMinimal = await _weaviate.Cluster.Nodes.List(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // ip is different depending on the environment
        Assert.Single(nodesMinimal);
        var node = nodesMinimal[0];
        Assert.NotNull(node);
        Assert.NotNull(node.GitHash);
        Assert.NotNull(node.Name);
        Assert.NotNull(node.Version);
    }

    /// <summary>
    /// Tests that test nodes list verbose
    /// </summary>
    [Fact]
    public async Task TestNodesListVerbose()
    {
        var nodesVerbose = await _weaviate.Cluster.Nodes.ListVerbose(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(nodesVerbose);
        var nodeV = nodesVerbose[0];
        Assert.NotNull(nodeV);
        Assert.NotNull(nodeV.GitHash);
        Assert.NotNull(nodeV.Name);
        Assert.NotNull(nodeV.Version);
        // additional verbose options
        Assert.NotNull(nodeV.Shards);
    }

    /// <summary>
    /// Tests that test nodes list verbose with collection
    /// </summary>
    [Fact]
    public async Task TestNodesListVerboseWithCollection()
    {
        var collectionName = "TestNodesListVerboseWithCollection";
        var collection = await CollectionFactory(collectionName);

        var nodesVerbose = await _weaviate.Cluster.Nodes.ListVerbose(
            collection.Name,
            cancellationToken: TestContext.Current.CancellationToken
        );
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

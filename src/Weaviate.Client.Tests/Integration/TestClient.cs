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

        var ex = await Record.ExceptionAsync(() =>
            client.Collections.List().ToListAsync(TestContext.Current.CancellationToken).AsTask()
        );
        Assert.Null(ex);
    }

    [Fact]
    public async Task TestMeta()
    {
        var client = Connect.Local();
        var meta = await client.GetMeta();

        Assert.Equal("http://127.0.0.1:8080", meta.Hostname);
    }
}

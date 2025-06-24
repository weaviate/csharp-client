namespace Weaviate.Client.Tests.Integration;

[Collection("ConnectionTests")]
public partial class ConnectionTests : IntegrationTests
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
}

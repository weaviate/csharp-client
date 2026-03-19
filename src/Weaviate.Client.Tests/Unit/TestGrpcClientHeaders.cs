#if ENABLE_INTERNAL_TESTS
using Weaviate.Client.DependencyInjection;
using Weaviate.Client.Grpc;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public class TestGrpcClientHeaders
{
    [Fact]
    public void GrpcClient_WithNoCustomHeaders_AlwaysAddsWeaviateClientHeader()
    {
        var channel = NoOpGrpcChannel.Create();
        var client = new WeaviateGrpcClient(channel, headers: null);

        Assert.NotNull(client._defaultHeaders);
        // gRPC Metadata stores keys in lowercase
        Assert.Contains(
            client._defaultHeaders,
            h =>
                h.Key.Equals("x-weaviate-client", StringComparison.OrdinalIgnoreCase)
                && h.Value.StartsWith("weaviate-client-csharp/")
        );
    }

    [Fact]
    public void GrpcClient_WithCustomHeaders_AddsWeaviateClientHeader()
    {
        var channel = NoOpGrpcChannel.Create();
        var client = new WeaviateGrpcClient(
            channel,
            headers: new Dictionary<string, string> { ["X-Custom"] = "value" }
        );

        Assert.NotNull(client._defaultHeaders);
        // gRPC Metadata stores keys in lowercase
        Assert.Contains(
            client._defaultHeaders,
            h =>
                h.Key.Equals("x-weaviate-client", StringComparison.OrdinalIgnoreCase)
                && h.Value.StartsWith("weaviate-client-csharp/")
        );
    }

    [Fact]
    public void GrpcClient_WithWcdHost_AddsClusterUrlAndWeaviateClientHeader()
    {
        var channel = NoOpGrpcChannel.Create();
        var client = new WeaviateGrpcClient(channel, wcdHost: "my-cluster.weaviate.cloud");

        Assert.NotNull(client._defaultHeaders);
        Assert.Contains(
            client._defaultHeaders,
            h => h.Key.Equals("x-weaviate-cluster-url", StringComparison.OrdinalIgnoreCase)
        );
        Assert.Contains(
            client._defaultHeaders,
            h =>
                h.Key.Equals("x-weaviate-client", StringComparison.OrdinalIgnoreCase)
                && h.Value.StartsWith("weaviate-client-csharp/")
        );
    }
}

public class TestWeaviateDefaults
{
    [Fact]
    public void IntegrationAgent_HasExpectedFormat()
    {
        var agent = WeaviateDefaults.IntegrationAgent("weaviate-client-csharp-managed");
        Assert.StartsWith("weaviate-client-csharp-managed/", agent);
        // version part should follow
        var version = agent.Substring("weaviate-client-csharp-managed/".Length);
        Assert.Matches(@"^\d+\.\d+", version);
    }

    [Fact]
    public void IntegrationAgent_UsesProvidedName()
    {
        var agent = WeaviateDefaults.IntegrationAgent("my-integration");
        Assert.StartsWith("my-integration/", agent);
    }
}

public class TestWeaviateOptions
{
    [Fact]
    public void AddHeader_SetsHeaderValue()
    {
        var opts = new WeaviateOptions();
        var result = opts.AddHeader("X-Custom", "value");

        Assert.Same(opts, result); // fluent - same instance
        Assert.NotNull(opts.Headers);
        Assert.Equal("value", opts.Headers["X-Custom"]);
    }

    [Fact]
    public void AddHeader_OverwritesExistingValue()
    {
        var opts = new WeaviateOptions();
        opts.AddHeader("X-Custom", "first");
        opts.AddHeader("X-Custom", "second");

        Assert.Equal("second", opts.Headers!["X-Custom"]);
    }

    [Fact]
    public void AddIntegration_SetsIntegrationHeader()
    {
        var opts = new WeaviateOptions();
        var result = opts.AddIntegration("my-lib/1.0.0");

        Assert.Same(opts, result);
        Assert.Equal("my-lib/1.0.0", opts.Headers![WeaviateDefaults.IntegrationHeader]);
    }

    [Fact]
    public void AddIntegration_AppendsSpaceSeparated()
    {
        var opts = new WeaviateOptions();
        opts.AddIntegration("first/1.0").AddIntegration("second/2.0");

        Assert.Equal("first/1.0 second/2.0", opts.Headers![WeaviateDefaults.IntegrationHeader]);
    }

    [Fact]
    public void AddIntegration_ChainableWithAddHeader()
    {
        var opts = new WeaviateOptions();
        opts.AddIntegration("my-lib/1.0").AddHeader("X-Custom", "val");

        Assert.Equal("my-lib/1.0", opts.Headers![WeaviateDefaults.IntegrationHeader]);
        Assert.Equal("val", opts.Headers["X-Custom"]);
    }

    [Theory]
    [InlineData("my lib/1.0")]
    [InlineData("my-lib/1.0 extra")]
    [InlineData("my-lib/1.0\t")]
    [InlineData("\nmy-lib/1.0")]
    public void AddIntegration_ThrowsIfValueContainsWhitespace(string value)
    {
        var opts = new WeaviateOptions();
        Assert.Throws<ArgumentException>(() => opts.AddIntegration(value));
    }
}

public class TestClientConfigurationExtensions
{
    [Theory]
    [InlineData("my lib/1.0")]
    [InlineData("my-lib/1.0 extra")]
    [InlineData("my-lib/1.0\t")]
    public void WithIntegration_ThrowsIfValueContainsWhitespace(string value)
    {
        var config = new ClientConfiguration();
        Assert.Throws<ArgumentException>(() => config.WithIntegration(value));
    }
}
#endif

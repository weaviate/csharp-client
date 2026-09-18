using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weaviate.Client.DependencyInjection;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying that the Weaviate Cloud connection helpers accept a full cluster URL or a
/// bare hostname, derive the REST and gRPC endpoints from its host, and reject malformed endpoints.
/// </summary>
[Collection("Unit Tests")]
public class TestCloudEndpoint
{
    /// <summary>
    /// Accepted cluster endpoints and the host each one must resolve to
    /// </summary>
    public static TheoryData<string, string> AcceptedEndpoints =>
        new()
        {
            { "https://xyz.weaviate.cloud", "xyz.weaviate.cloud" },
            { "xyz.weaviate.cloud", "xyz.weaviate.cloud" },
            { "https://xyz.weaviate.cloud/", "xyz.weaviate.cloud" },
            { "https://xyz.weaviate.cloud/v1", "xyz.weaviate.cloud" },
            { "http://xyz.weaviate.cloud", "xyz.weaviate.cloud" },
            { "https://xyz.weaviate.cloud:443", "xyz.weaviate.cloud" },
            { "  https://xyz.weaviate.cloud/v1?x=1#top  ", "xyz.weaviate.cloud" },
            { "HTTPS://XYZ.Weaviate.Cloud", "xyz.weaviate.cloud" },
            { "xyz.weaviate.cloud:443", "xyz.weaviate.cloud" },
            { "xyz.weaviate.cloud/a://b", "xyz.weaviate.cloud" },
            { "https://https.weaviate.cloud", "https.weaviate.cloud" },
        };

    /// <summary>
    /// Cluster endpoints that must be rejected, each with a fragment of it that the exception
    /// message must not echo (null where the input has nothing distinctive)
    /// </summary>
    public static TheoryData<string?, string?> RejectedEndpoints =>
        new()
        {
            { null, null },
            { "", null },
            { "   ", null },
            { "https://", null },
            { "ftp://xyz.weaviate.cloud", "ftp" },
            { "sk-SECRET123://xyz.weaviate.cloud", "SECRET123" },
            { "https://user:pw@xyz.weaviate.cloud", "pw" },
            { "https://xyz.weaviate.cloud:8080", "8080" },
            { "xyz.weaviate.cloud:8080", "8080" },
            { "http://xyz.weaviate.cloud:80", ":80" },
            { "https://10.0.0.1", "10.0.0.1" },
            { "https://https://xyz.weaviate.cloud", "xyz" },
            { "https:/xyz.weaviate.cloud", "xyz" },
            { "https//xyz.weaviate.cloud", "xyz" },
            { "not a host!", "not a host" },
        };

    /// <summary>
    /// Tests that AddWeaviateCloud keeps only the host and always uses TLS on port 443
    /// </summary>
    [Theory]
    [MemberData(nameof(AcceptedEndpoints))]
    public void AddWeaviateCloud_WithUrlOrHostname_UsesClusterHost(
        string endpoint,
        string expectedHost
    )
    {
        using var provider = new ServiceCollection()
            .AddWeaviateCloud(endpoint, apiKey: "key", eagerInitialization: false)
            .BuildServiceProvider();

        AssertCloudOptions(
            provider.GetRequiredService<IOptions<WeaviateOptions>>().Value,
            expectedHost
        );
        AssertCloudUris(provider.GetRequiredService<WeaviateClient>(), expectedHost);
    }

    /// <summary>
    /// Tests that AddWeaviateCloud rejects a malformed endpoint when it is registered, not when
    /// the client is first resolved
    /// </summary>
    [Theory]
    [MemberData(nameof(RejectedEndpoints))]
    public void AddWeaviateCloud_WithInvalidEndpoint_ThrowsAtRegistration(
        string? endpoint,
        string? mustNotEcho
    )
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<ArgumentException>(() =>
            services.AddWeaviateCloud(endpoint!, eagerInitialization: false)
        );

        AssertRejected(ex, "clusterEndpoint", mustNotEcho);
    }

    /// <summary>
    /// Tests that the scoped token service overload of AddWeaviateCloud keeps only the host
    /// </summary>
    [Fact]
    public void AddWeaviateCloudWithTokenService_WithUrl_UsesClusterHost()
    {
        using var provider = new ServiceCollection()
            .AddWeaviateCloud<StubTokenService>("https://xyz.weaviate.cloud/")
            .BuildServiceProvider();

        AssertCloudOptions(
            provider.GetRequiredService<IOptions<WeaviateOptions>>().Value,
            "xyz.weaviate.cloud"
        );
        AssertCloudUris(provider.GetRequiredService<WeaviateClient>(), "xyz.weaviate.cloud");
    }

    /// <summary>
    /// Tests that the scoped token service overload of AddWeaviateCloud rejects a malformed endpoint
    /// at registration
    /// </summary>
    [Fact]
    public void AddWeaviateCloudWithTokenService_WithInvalidEndpoint_ThrowsAtRegistration()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<ArgumentException>(() =>
            services.AddWeaviateCloud<StubTokenService>("https://xyz.weaviate.cloud:8080")
        );

        AssertRejected(ex, "clusterEndpoint", "8080");
    }

    /// <summary>
    /// Tests that the named client overload of AddWeaviateCloud keeps only the host
    /// </summary>
    [Fact]
    public void AddWeaviateCloudNamed_WithUrl_UsesClusterHost()
    {
        using var provider = new ServiceCollection()
            .AddWeaviateCloud(
                name: "prod",
                clusterEndpoint: "https://xyz.weaviate.cloud/",
                apiKey: "key"
            )
            .BuildServiceProvider();

        AssertCloudOptions(
            provider.GetRequiredService<IOptionsMonitor<WeaviateOptions>>().Get("prod"),
            "xyz.weaviate.cloud"
        );
    }

    /// <summary>
    /// Tests that the named client overload of AddWeaviateCloud rejects a malformed endpoint at
    /// registration
    /// </summary>
    [Fact]
    public void AddWeaviateCloudNamed_WithInvalidEndpoint_ThrowsAtRegistration()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<ArgumentException>(() =>
            services.AddWeaviateCloud(name: "prod", clusterEndpoint: "ftp://xyz.weaviate.cloud")
        );

        AssertRejected(ex, "clusterEndpoint", "ftp");
    }

    /// <summary>
    /// Tests that a client built with WeaviateClientBuilder.Cloud sends its first REST request to
    /// the cluster host over TLS on port 443
    /// </summary>
    [Theory]
    [MemberData(nameof(AcceptedEndpoints))]
    public async Task Cloud_WithUrlOrHostname_SendsRequestsToClusterHost(
        string endpoint,
        string expectedHost
    )
    {
        var handler = new CaptureFirstRequestHandler();

        await Assert.ThrowsAsync<RequestCapturedException>(() =>
            WeaviateClientBuilder.Cloud(endpoint, httpMessageHandler: handler).BuildAsync()
        );

        AssertCloudRequest(handler.RequestUri, expectedHost);
    }

    /// <summary>
    /// Tests that WeaviateClientBuilder.Cloud rejects a malformed endpoint before any request is made
    /// </summary>
    [Theory]
    [MemberData(nameof(RejectedEndpoints))]
    public void Cloud_WithInvalidEndpoint_Throws(string? endpoint, string? mustNotEcho)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            WeaviateClientBuilder.Cloud(endpoint!);
        });

        AssertRejected(ex, "restEndpoint", mustNotEcho);
    }

    /// <summary>
    /// Tests that Connect.Cloud sends its first REST request to the cluster host
    /// </summary>
    [Fact]
    public async Task ConnectCloud_WithUrl_SendsRequestsToClusterHost()
    {
        var handler = new CaptureFirstRequestHandler();

        await Assert.ThrowsAsync<RequestCapturedException>(() =>
            Connect.Cloud("https://xyz.weaviate.cloud/", httpMessageHandler: handler)
        );

        AssertCloudRequest(handler.RequestUri, "xyz.weaviate.cloud");
    }

    /// <summary>
    /// Tests that Connect.Cloud rejects a malformed endpoint
    /// </summary>
    [Fact]
    public async Task ConnectCloud_WithInvalidEndpoint_Throws()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            Connect.Cloud("https://user:pw@xyz.weaviate.cloud")
        );

        AssertRejected(ex, "restEndpoint", "pw");
    }

    /// <summary>
    /// Asserts that a rejection names the parameter and the expected forms, and does not echo the input
    /// </summary>
    private static void AssertRejected(ArgumentException ex, string paramName, string? mustNotEcho)
    {
        Assert.Equal(paramName, ex.ParamName);
        Assert.Contains("'https://my-cluster.weaviate.cloud'", ex.Message);
        if (mustNotEcho is not null)
        {
            Assert.DoesNotContain(mustNotEcho, ex.Message);
        }
    }

    /// <summary>
    /// Asserts the options registered for a Weaviate Cloud client
    /// </summary>
    private static void AssertCloudOptions(WeaviateOptions options, string expectedHost)
    {
        Assert.Equal(expectedHost, options.RestEndpoint);
        Assert.Equal($"grpc-{expectedHost}", options.GrpcEndpoint);
        Assert.Equal((ushort)443, options.RestPort);
        Assert.Equal((ushort)443, options.GrpcPort);
        Assert.True(options.UseSsl);
    }

    /// <summary>
    /// Asserts the REST and gRPC URIs of a client resolved for Weaviate Cloud
    /// </summary>
    private static void AssertCloudUris(WeaviateClient client, string expectedHost)
    {
        Assert.Equal($"https://{expectedHost}/v1/", client.Configuration.RestUri.AbsoluteUri);
        Assert.Equal($"https://grpc-{expectedHost}/", client.Configuration.GrpcUri.AbsoluteUri);
    }

    /// <summary>
    /// Asserts the URI of the first REST request a Weaviate Cloud client sends
    /// </summary>
    private static void AssertCloudRequest(Uri? requestUri, string expectedHost)
    {
        Assert.NotNull(requestUri);
        Assert.Equal("https", requestUri.Scheme);
        Assert.Equal(expectedHost, requestUri.Host);
        Assert.Equal(443, requestUri.Port);
        Assert.Equal("/v1/meta", requestUri.AbsolutePath);
    }

    /// <summary>
    /// Records the first request and stops client initialization before any network call
    /// </summary>
    private sealed class CaptureFirstRequestHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestUri ??= request.RequestUri;
            throw new RequestCapturedException();
        }
    }

    /// <summary>
    /// Thrown by <see cref="CaptureFirstRequestHandler"/> once the request is recorded
    /// </summary>
    private sealed class RequestCapturedException : Exception;

    /// <summary>
    /// A token service that is never called by these tests
    /// </summary>
    private sealed class StubTokenService : ITokenService
    {
        public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>("token");

        public Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public bool IsAuthenticated() => true;
    }
}

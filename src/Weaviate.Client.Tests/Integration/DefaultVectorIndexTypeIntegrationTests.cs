using System.Runtime.InteropServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// End-to-end coverage for the C# client logic introduced in commit
/// <c>Send empty vector index type on Weaviate 1.37.5+</c>:
/// <list type="bullet">
///   <item>
///     On Weaviate 1.37.4 (legacy server) the client must inject
///     <c>"hnsw"</c> for any omitted <c>vectorIndexType</c>, and the
///     server must store it as <c>"hnsw"</c>.
///   </item>
///   <item>
///     On Weaviate 1.37.5+ with <c>DEFAULT_VECTOR_INDEX=flat</c> the client
///     must omit <c>vectorIndexType</c>, and the server must apply its own
///     default — in this case <c>"flat"</c>.
///   </item>
///   <item>
///     A user-supplied <c>VectorIndexType</c> (e.g. <c>flat</c>) must be
///     preserved on both server versions — the inject helper is not allowed
///     to mutate explicit values.
///   </item>
/// </list>
/// Unlike <see cref="IntegrationTests"/>, these tests start their own
/// Weaviate container per scenario so the two server versions can be
/// exercised in the same test run, with different env vars.
///
/// Requires Docker. If Docker is not reachable, the tests will fail rather
/// than skip, so missing infrastructure is loud in CI.
/// </summary>
public sealed class DefaultVectorIndexTypeIntegrationTests : IAsyncLifetime
{
    /// <summary>
    /// Last Weaviate version that does NOT apply a server-side default for
    /// <c>vectorIndexType</c>. The client must inject <c>"hnsw"</c> here.
    /// </summary>
    private const string LegacyImage = "cr.weaviate.io/semitechnologies/weaviate:1.37.4";

    /// <summary>
    /// First Weaviate build that consumes <c>DEFAULT_VECTOR_INDEX</c>.
    /// Two arch-specific tags are published; pick the right one at runtime.
    /// </summary>
    private static string NewImage =>
        RuntimeInformation.OSArchitecture == Architecture.Arm64
            ? "cr.weaviate.io/semitechnologies/weaviate:1.37.5-e0fe0d5.arm64"
            : "cr.weaviate.io/semitechnologies/weaviate:1.37.5-e0fe0d5.amd64";

    private readonly List<IContainer> _containers = new();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        foreach (var container in _containers)
        {
            try
            {
                await container.DisposeAsync();
            }
            catch
            {
                // Best-effort cleanup; container disposal failures should not mask
                // the actual test outcome.
            }
        }
    }

    /// <summary>
    /// Builds and starts a single-node Weaviate container with anonymous auth
    /// enabled, then constructs a <see cref="WeaviateClient"/> pointed at it.
    /// The client's <c>_metaCache</c> is populated by <c>BuildAsync</c>.
    /// </summary>
    private async Task<WeaviateClient> StartWeaviateAsync(
        string image,
        IDictionary<string, string>? extraEnv = null
    )
    {
        var builder = new ContainerBuilder(image)
            .WithPortBinding(8080, assignRandomHostPort: true)
            .WithPortBinding(50051, assignRandomHostPort: true)
            .WithEnvironment("QUERY_DEFAULTS_LIMIT", "25")
            .WithEnvironment("AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED", "true")
            .WithEnvironment("PERSISTENCE_DATA_PATH", "/var/lib/weaviate")
            .WithEnvironment("CLUSTER_HOSTNAME", "node1")
            .WithEnvironment("DISABLE_TELEMETRY", "true")
            .WithCommand("--host", "0.0.0.0", "--port", "8080", "--scheme", "http")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(req =>
                        req.ForPort(8080).ForPath("/v1/.well-known/ready")
                    )
            );

        if (extraEnv is not null)
        {
            foreach (var kvp in extraEnv)
            {
                builder = builder.WithEnvironment(kvp.Key, kvp.Value);
            }
        }

        var container = builder.Build();
        _containers.Add(container);

        await container.StartAsync(TestContext.Current.CancellationToken);

        var restPort = container.GetMappedPublicPort(8080);
        var grpcPort = container.GetMappedPublicPort(50051);

        var client = await WeaviateClientBuilder
            .Local(restPort: restPort, grpcPort: grpcPort)
            .BuildAsync();

        // Sanity check: client should know the server version after BuildAsync.
        Assert.NotNull(client.WeaviateVersion);

        return client;
    }

    /// <summary>
    /// Creates a collection with a single named vector and no explicit
    /// <c>VectorIndexType</c>, then returns the stored type on the server.
    /// </summary>
    private static async Task<string?> CreateAndReadIndexTypeAsync(
        WeaviateClient client,
        string collectionName,
        VectorIndexConfig? index
    )
    {
        // Best-effort cleanup of any leftover collection with this name.
        await client.Collections.Delete(collectionName);

        var create = new CollectionCreateParams
        {
            Name = collectionName,
            Properties = [Property.Text("title")],
            VectorConfig = Configure.Vector("default", v => v.SelfProvided(), index: index),
        };

        var collection = await client.Collections.Create(
            create,
            TestContext.Current.CancellationToken
        );
        try
        {
            var config = await collection.Config.Get(
                cancellationToken: TestContext.Current.CancellationToken
            );

            Assert.NotNull(config.VectorConfig);
            Assert.True(config.VectorConfig.ContainsKey("default"));
            return config.VectorConfig["default"].VectorIndexType;
        }
        finally
        {
            await client.Collections.Delete(collectionName);
        }
    }

    /// <summary>
    /// 1.37.4 has no server-side default for <c>vectorIndexType</c>. The C#
    /// client must inject <c>"hnsw"</c> in that case, and the server must
    /// then store the collection with that type.
    /// </summary>
    [Fact]
    public async Task LegacyServer_NoIndexType_GetsHnsw()
    {
        var client = await StartWeaviateAsync(LegacyImage);

        // Sanity check the version gate — must be strictly older than the
        // first version that applies a server-side default.
        Assert.True(client.WeaviateVersion! < new Version(1, 37, 5));

        var stored = await CreateAndReadIndexTypeAsync(
            client,
            nameof(LegacyServer_NoIndexType_GetsHnsw),
            index: null
        );

        Assert.Equal(VectorIndex.HNSW.TypeValue, stored);
    }

    /// <summary>
    /// 1.37.5+ with <c>DEFAULT_VECTOR_INDEX=flat</c>. The client must omit
    /// <c>vectorIndexType</c> entirely so the server applies the configured
    /// default — verifying both the client-side change AND the new server
    /// behaviour.
    /// </summary>
    [Fact]
    public async Task NewServer_DefaultVectorIndexFlat_NoIndexType_GetsFlat()
    {
        var client = await StartWeaviateAsync(
            NewImage,
            extraEnv: new Dictionary<string, string> { ["DEFAULT_VECTOR_INDEX"] = "flat" }
        );

        // Sanity check: this build must be at or beyond the version gate.
        Assert.True(client.WeaviateVersion! >= new Version(1, 37, 5));

        var stored = await CreateAndReadIndexTypeAsync(
            client,
            nameof(NewServer_DefaultVectorIndexFlat_NoIndexType_GetsFlat),
            index: null
        );

        Assert.Equal(VectorIndex.Flat.TypeValue, stored);
    }

    /// <summary>
    /// An explicit user choice (<c>flat</c>) must round-trip on the legacy
    /// server unchanged — the inject helper must never overwrite a value the
    /// user explicitly set.
    /// </summary>
    [Fact]
    public async Task LegacyServer_ExplicitFlat_IsPreserved()
    {
        var client = await StartWeaviateAsync(LegacyImage);

        var stored = await CreateAndReadIndexTypeAsync(
            client,
            nameof(LegacyServer_ExplicitFlat_IsPreserved),
            index: new VectorIndex.Flat()
        );

        Assert.Equal(VectorIndex.Flat.TypeValue, stored);
    }

    /// <summary>
    /// Same explicit-choice guarantee on a 1.37.5+ server with a different
    /// server-side default — proves the client doesn't accidentally rely on
    /// the server falling back to <c>DEFAULT_VECTOR_INDEX</c>.
    /// </summary>
    [Fact]
    public async Task NewServer_ExplicitFlat_IsPreserved()
    {
        // Use hnsw as the server default so a passing test really is showing
        // that the user-supplied "flat" survived end-to-end.
        var client = await StartWeaviateAsync(
            NewImage,
            extraEnv: new Dictionary<string, string> { ["DEFAULT_VECTOR_INDEX"] = "hnsw" }
        );

        var stored = await CreateAndReadIndexTypeAsync(
            client,
            nameof(NewServer_ExplicitFlat_IsPreserved),
            index: new VectorIndex.Flat()
        );

        Assert.Equal(VectorIndex.Flat.TypeValue, stored);
    }
}

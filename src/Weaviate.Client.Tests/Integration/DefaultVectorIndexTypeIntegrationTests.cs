using System.Runtime.InteropServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// End-to-end coverage for the C# client logic introduced in commits
/// <c>Send empty vector index type on Weaviate 1.37.5+</c> and
/// <c>Fix vector index type defaulting bugs surfaced by integration tests</c>.
///
/// <para>
/// One container per run. The target server version is read from the
/// <c>WEAVIATE_VERSION</c> environment variable (the existing convention used
/// by the unit tests, see <c>TestHybridSearchInputSyntax.ServerVersionEnvVar</c>),
/// defaulting to a 1.37.5+ build when unset. CI's matrix runs the suite once
/// per version, so coverage across the 1.37.5 cutoff happens automatically;
/// locally, set <c>WEAVIATE_VERSION=1.37.4</c> (or any older tag) to exercise
/// the legacy branch.
/// </para>
///
/// <para>
/// Assertions adapt at runtime based on whether the parsed version is
/// <c>&gt;= 1.37.5</c>:
/// <list type="bullet">
///   <item>
///     On 1.37.5+ the server applies its own default — the container is
///     started with <c>DEFAULT_VECTOR_INDEX=flat</c>, so any user-omitted
///     <c>vectorIndexType</c> must land as <c>"flat"</c>.
///   </item>
///   <item>
///     On older servers the client injects <c>"hnsw"</c> client-side, and the
///     server stores it verbatim.
///   </item>
///   <item>
///     A user-supplied explicit <c>VectorIndexType</c> (e.g. <c>flat</c>) must
///     round-trip unchanged on every version.
///   </item>
/// </list>
/// </para>
///
/// <para>
/// <c>DEFAULT_VECTOR_INDEX=flat</c> is set on every run: older servers ignore
/// unknown env vars, so it is harmless on 1.37.4-and-below and asserts the
/// new behaviour on 1.37.5+.
/// </para>
///
/// Requires Docker. If Docker is not reachable, the tests fail rather than
/// skip, so missing infrastructure is loud in CI.
/// </summary>
public sealed class DefaultVectorIndexTypeIntegrationTests : IAsyncLifetime
{
    /// <summary>
    /// Name of the env var that picks the Weaviate server version. Matches the
    /// existing convention in the unit-test suite.
    /// </summary>
    private const string ServerVersionEnvVar = "WEAVIATE_VERSION";

    /// <summary>
    /// Default tag when <see cref="ServerVersionEnvVar"/> is unset. The amd64
    /// variant is the canonical default; if the host is arm64 we swap to the
    /// matching arm64 build. When the user explicitly sets the env var, they
    /// own the suffix and we use the value verbatim.
    /// </summary>
    private const string DefaultServerVersion = "1.37.5-e0fe0d5.amd64";

    private const string ImageRepo = "cr.weaviate.io/semitechnologies/weaviate";

    /// <summary>
    /// First Weaviate version that applies a server-side default for
    /// <c>vectorIndexType</c> when the client omits it.
    /// </summary>
    private static readonly Version ServerDefaultCutoff = new(1, 37, 5);

    private readonly string _imageTag;
    private readonly bool _serverAppliesDefault;
    private readonly List<IContainer> _containers = new();

    public DefaultVectorIndexTypeIntegrationTests()
    {
        var envValue = Environment.GetEnvironmentVariable(ServerVersionEnvVar);
        if (string.IsNullOrWhiteSpace(envValue))
        {
            // Default kicked in — swap amd64 -> arm64 if needed so the test
            // works locally on Apple Silicon without a manual override.
            _imageTag =
                RuntimeInformation.OSArchitecture == Architecture.Arm64
                    ? DefaultServerVersion.Replace(".amd64", ".arm64")
                    : DefaultServerVersion;
        }
        else
        {
            // User explicitly set the version; respect the tag suffix exactly.
            _imageTag = envValue;
        }

        var parsedVersion = MetaInfo.ParseWeaviateVersion(_imageTag);
        _serverAppliesDefault = parsedVersion is not null && parsedVersion >= ServerDefaultCutoff;
    }

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
                // Best-effort cleanup; container disposal failures should not
                // mask the actual test outcome.
            }
        }
    }

    /// <summary>
    /// Builds and starts a single-node Weaviate container at the configured
    /// version with anonymous auth enabled and <c>DEFAULT_VECTOR_INDEX=flat</c>
    /// applied. The flat default is always set: older servers ignore it.
    /// Returns a <see cref="WeaviateClient"/> wired to the container; the
    /// client's meta cache is populated by <c>BuildAsync</c>.
    /// </summary>
    private async Task<WeaviateClient> StartWeaviateAsync()
    {
        var image = $"{ImageRepo}:{_imageTag}";

        var container = new ContainerBuilder(image)
            .WithPortBinding(8080, assignRandomHostPort: true)
            .WithPortBinding(50051, assignRandomHostPort: true)
            .WithEnvironment("QUERY_DEFAULTS_LIMIT", "25")
            .WithEnvironment("AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED", "true")
            .WithEnvironment("PERSISTENCE_DATA_PATH", "/var/lib/weaviate")
            .WithEnvironment("CLUSTER_HOSTNAME", "node1")
            .WithEnvironment("DISABLE_TELEMETRY", "true")
            .WithEnvironment("DEFAULT_VECTOR_INDEX", "flat")
            .WithCommand("--host", "0.0.0.0", "--port", "8080", "--scheme", "http")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(req =>
                        req.ForPort(8080).ForPath("/v1/.well-known/ready")
                    )
            )
            .Build();

        _containers.Add(container);

        await container.StartAsync(TestContext.Current.CancellationToken);

        var restPort = container.GetMappedPublicPort(8080);
        var grpcPort = container.GetMappedPublicPort(50051);

        var client = await WeaviateClientBuilder
            .Local(restPort: restPort, grpcPort: grpcPort)
            .BuildAsync();

        // Sanity check: client should know the server version after BuildAsync,
        // and it should agree with our env-derived gate so the test is
        // self-consistent if a tag is mislabelled.
        Assert.NotNull(client.WeaviateVersion);
        Assert.Equal(_serverAppliesDefault, client.WeaviateVersion! >= ServerDefaultCutoff);

        return client;
    }

    /// <summary>
    /// Creates a collection on the connected server, reads the stored config
    /// back, and returns the result of <paramref name="readStoredType"/>
    /// applied to it. The collection is best-effort deleted before and after.
    /// </summary>
    private static async Task<string?> CreateAndReadAsync(
        WeaviateClient client,
        CollectionCreateParams create,
        Func<CollectionConfig, string?> readStoredType
    )
    {
        await client.Collections.Delete(create.Name);

        var collection = await client.Collections.Create(
            create,
            TestContext.Current.CancellationToken
        );
        try
        {
            var config = await collection.Config.Get(
                cancellationToken: TestContext.Current.CancellationToken
            );
            return readStoredType(config);
        }
        finally
        {
            await client.Collections.Delete(create.Name);
        }
    }

    /// <summary>
    /// Scenario A on the named-vector path: user omits <c>VectorIndexType</c>
    /// when configuring a named vector.
    /// <list type="bullet">
    ///   <item>1.37.5+: server applies <c>DEFAULT_VECTOR_INDEX=flat</c>.</item>
    ///   <item>Older: client injects <c>"hnsw"</c> and server stores it.</item>
    /// </list>
    /// </summary>
    [Fact]
    public async Task NamedVector_NoIndexType_GetsServerOrInjectedDefault()
    {
        var client = await StartWeaviateAsync();

        var create = new CollectionCreateParams
        {
            Name = nameof(NamedVector_NoIndexType_GetsServerOrInjectedDefault),
            Properties = [Property.Text("title")],
            VectorConfig = Configure.Vector("default", v => v.SelfProvided(), index: null),
        };

        var stored = await CreateAndReadAsync(
            client,
            create,
            config =>
            {
                Assert.NotNull(config.VectorConfig);
                Assert.True(config.VectorConfig.ContainsKey("default"));
                return config.VectorConfig["default"].VectorIndexType;
            }
        );

        var expected = _serverAppliesDefault
            ? VectorIndex.Flat.TypeValue
            : VectorIndex.HNSW.TypeValue;
        Assert.Equal(expected, stored);
    }

    /// <summary>
    /// Scenario B on the named-vector path: user explicitly configures
    /// <c>VectorIndex.Flat</c>. Must round-trip as <c>"flat"</c> on every
    /// server version — the inject helper must never overwrite an explicit
    /// value.
    /// </summary>
    [Fact]
    public async Task NamedVector_ExplicitFlat_IsPreserved()
    {
        var client = await StartWeaviateAsync();

        var create = new CollectionCreateParams
        {
            Name = nameof(NamedVector_ExplicitFlat_IsPreserved),
            Properties = [Property.Text("title")],
            VectorConfig = Configure.Vector(
                "default",
                v => v.SelfProvided(),
                index: new VectorIndex.Flat()
            ),
        };

        var stored = await CreateAndReadAsync(
            client,
            create,
            config =>
            {
                Assert.NotNull(config.VectorConfig);
                Assert.True(config.VectorConfig.ContainsKey("default"));
                return config.VectorConfig["default"].VectorIndexType;
            }
        );

        Assert.Equal(VectorIndex.Flat.TypeValue, stored);
    }

    // The legacy single-vector path (CollectionCreateParams with no
    // VectorConfig — the server stores the choice in the top-level
    // `vectorIndexType` field) is intentionally NOT exercised here:
    //
    //   * `CollectionCreateParams` does not expose a top-level
    //     `VectorIndexType` setter on the write side, so Scenario B
    //     (explicit flat) is not reachable from the public API.
    //   * On the read side, `Extensions.ToModel(Rest.Dto.Class)` does not
    //     copy the top-level `vectorIndexType` from the GET response into
    //     `CollectionConfig.VectorIndexType`; the field stays at its default
    //     regardless of what the server returned. End-to-end assertions on
    //     the top-level field therefore cannot pass until that parse path
    //     is fixed — a separate bug.
    //
    // The legacy top-level inject behaviour is locked in by the wire-level
    // unit tests in
    // `CollectionsClient.DefaultVectorIndexTypeTests.InjectLegacyDefaultVectorIndexType_*`,
    // which assert directly on the outgoing DTO. The integration coverage
    // here focuses on the named-vector path, which is the supported public
    // API surface for picking an index type in modern code.
}

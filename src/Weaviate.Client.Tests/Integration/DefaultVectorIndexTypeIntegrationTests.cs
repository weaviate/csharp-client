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
/// and is required — CI sets it; locally export it (e.g.
/// <c>WEAVIATE_VERSION=1.37.4</c> for the legacy branch) before running.
/// </para>
///
/// <para>
/// Assertions adapt at runtime based on whether the parsed version is
/// <c>&gt;= 1.37.5</c>:
/// <list type="bullet">
///   <item>
///     On 1.37.5+ the server applies its own default — the container is
///     started with <c>DEFAULT_VECTOR_INDEX</c> set from the host env var
///     (default <c>"hfresh"</c>), so any user-omitted <c>vectorIndexType</c>
///     must land as that value.
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
/// <c>DEFAULT_VECTOR_INDEX</c> is read from the host env var (default
/// <c>"hfresh"</c>) and forwarded to the container on every run: older
/// servers ignore unknown env vars, so it is harmless on 1.37.4-and-below
/// and asserts the new behaviour on 1.37.5+.
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
    /// Name of the env var that picks both the container's
    /// <c>DEFAULT_VECTOR_INDEX</c> setting and the expected stored
    /// <c>vectorIndexType</c> on servers <c>&gt;= 1.37.5</c>. Defaults to
    /// <see cref="DefaultVectorIndexFallback"/> when unset so local runs match
    /// the production recommended default.
    /// </summary>
    private const string DefaultVectorIndexEnvVar = "DEFAULT_VECTOR_INDEX";

    /// <summary>
    /// Fallback value when <see cref="DefaultVectorIndexEnvVar"/> is unset.
    /// </summary>
    private const string DefaultVectorIndexFallback = "hfresh";

    private const string ImageRepo = "cr.weaviate.io/semitechnologies/weaviate";

    /// <summary>
    /// First Weaviate version that applies a server-side default for
    /// <c>vectorIndexType</c> when the client omits it.
    /// </summary>
    private static readonly Version ServerDefaultCutoff = new(1, 37, 5);

    private readonly string _imageTag;
    private readonly bool _serverAppliesDefault;
    private readonly string _defaultVectorIndex;
    private readonly List<IContainer> _containers = new();

    public DefaultVectorIndexTypeIntegrationTests()
    {
        var defaultVectorIndexEnv = Environment.GetEnvironmentVariable(DefaultVectorIndexEnvVar);
        _defaultVectorIndex = string.IsNullOrWhiteSpace(defaultVectorIndexEnv)
            ? DefaultVectorIndexFallback
            : defaultVectorIndexEnv;

        var envValue = Environment.GetEnvironmentVariable(ServerVersionEnvVar);
        if (string.IsNullOrWhiteSpace(envValue))
        {
            throw new InvalidOperationException(
                $"{ServerVersionEnvVar} env var is required for this integration test."
            );
        }
        _imageTag = envValue;

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
    /// version with anonymous auth enabled and <c>DEFAULT_VECTOR_INDEX</c>
    /// applied (value read from the host env var, default
    /// <see cref="DefaultVectorIndexFallback"/>). Older servers ignore the
    /// env var. Returns a <see cref="WeaviateClient"/> wired to the
    /// container; the client's meta cache is populated by <c>BuildAsync</c>.
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
            .WithEnvironment("DEFAULT_VECTOR_INDEX", _defaultVectorIndex)
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
    /// Returns the expected stored <c>VectorIndexType</c> for a vector that was
    /// configured without an explicit index type.
    /// <list type="bullet">
    ///   <item>1.37.5+: server applies <c>DEFAULT_VECTOR_INDEX</c> from the
    ///   container env var.</item>
    ///   <item>Older: client injects <c>"hnsw"</c>.</item>
    /// </list>
    /// </summary>
    private string ExpectedDefaultIndexType() =>
        _serverAppliesDefault ? _defaultVectorIndex : VectorIndex.HNSW.TypeValue;

    /// <summary>
    /// Scenario A on the named-vector path: user omits <c>VectorIndexType</c>
    /// when configuring a named vector.
    /// <list type="bullet">
    ///   <item>1.37.5+: server applies <c>DEFAULT_VECTOR_INDEX</c> from the
    ///   host env var (default <see cref="DefaultVectorIndexFallback"/>).</item>
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

        Assert.Equal(ExpectedDefaultIndexType(), stored);
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

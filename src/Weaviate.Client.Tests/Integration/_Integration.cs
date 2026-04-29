using System.Reflection;
using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Weaviate.Client.Internal;
using Weaviate.Client.Models;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// The integration tests class
/// </summary>
/// <seealso cref="IAsyncDisposable"/>
/// <seealso cref="IAsyncLifetime"/>
public abstract partial class IntegrationTests : IAsyncDisposable, IAsyncLifetime
{
    /// <summary>
    /// The env file
    /// </summary>
    private const string ENV_FILE = "development.env";

    /// <summary>
    /// The delete collections after test
    /// </summary>
    const bool _deleteCollectionsAfterTest = true;

    /// <summary>
    /// The collections
    /// </summary>
    List<string> _collections = new();

    /// <summary>
    /// The weaviate
    /// </summary>
    protected WeaviateClient _weaviate = null!;

    /// <summary>
    /// The http message handler
    /// </summary>
    protected HttpMessageHandler? _httpMessageHandler;

    /// <summary>
    /// Configuration for tests, built from environment variables and optional appsettings.Test.json
    /// </summary>
    protected readonly IConfiguration _configuration;

    /// <summary>
    /// The new guid
    /// </summary>
    protected static readonly Guid[] _reusableUuids =
    [
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTests"/> class
    /// </summary>
    public IntegrationTests()
    {
        if (File.Exists(ENV_FILE))
        {
            DotEnv.Load(options: new DotEnvOptions(envFilePaths: [ENV_FILE]));
        }

        // Build configuration from environment variables and optional test config file
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Gets the value of the credentials
    /// </summary>
    public virtual ICredentials? Credentials => null;

    /// <summary>
    /// Gets the value of the rest host
    /// </summary>
    public virtual string RestHost =>
        _configuration.GetValue<string>("WV_TEST_HOST") ?? "localhost";

    /// <summary>
    /// Gets the value of the rest port
    /// </summary>
    public virtual ushort RestPort => _configuration.GetValue<ushort>("WV_TEST_REST_PORT", 8080);

    /// <summary>
    /// Gets the value of the grpc port
    /// </summary>
    public virtual ushort GrpcPort => _configuration.GetValue<ushort>("WV_TEST_GRPC_PORT", 50051);

    /// <summary>
    /// Gets the value of the OIDC host. Override via WV_TEST_OIDC_HOST (default: localhost).
    /// </summary>
    public virtual string OidcHost =>
        _configuration.GetValue<string>("WV_TEST_OIDC_HOST") ?? "localhost";

    /// <summary>
    /// Gets the value of the OIDC Okta client-credentials REST port.
    /// Override via WV_TEST_OIDC_OKTA_CC_PORT (default: 8082).
    /// </summary>
    public virtual ushort OidcOktaCcPort =>
        _configuration.GetValue<ushort>("WV_TEST_OIDC_OKTA_CC_PORT", 8082);

    /// <summary>
    /// Gets the value of the OIDC Okta users REST port.
    /// Override via WV_TEST_OIDC_OKTA_USERS_PORT (default: 8083).
    /// </summary>
    public virtual ushort OidcOktaUsersPort =>
        _configuration.GetValue<ushort>("WV_TEST_OIDC_OKTA_USERS_PORT", 8083);

    /// <summary>
    /// Disposes this instance
    /// </summary>
    /// <returns>The value task</returns>
    public virtual async ValueTask DisposeAsync()
    {
        if (_deleteCollectionsAfterTest && _collections.Count > 0)
        {
            await Task.WhenAll(_collections.Select(c => _weaviate.Collections.Delete(c)));
        }
        _weaviate.Dispose();
    }

    /// <summary>
    /// Initializes this instance
    /// </summary>
    /// <exception cref="InvalidOperationException">Weaviate not ready on REST:{RestPort} gRPC:{GrpcPort}. Expected a running instance for integration tests.</exception>
    /// <exception cref="InvalidOperationException">Weaviate readiness check failed during test initialization </exception>
    /// <returns>The value task</returns>
    public virtual async ValueTask InitializeAsync()
    {
        // Build the client asynchronously
        var builder = WeaviateClientBuilder.Local(httpMessageHandler: _httpMessageHandler);

        if (
            Environment.GetEnvironmentVariable("WEAVIATE_OPENAI_API_KEY") is { } openaiKey
            && !string.IsNullOrEmpty(openaiKey)
        )
        {
            builder.WithOpenAI(openaiKey);
        }

        builder.WithRestPort(RestPort);
        builder.WithGrpcPort(GrpcPort);
        if (Credentials != null)
        {
            builder.WithCredentials(Credentials);
        }

        // Enable request logging if configured via Weaviate__LogRequests=true
        // Console output is captured by xUnit via [assembly: CaptureConsole]
        if (_configuration.GetValue<bool>("Weaviate:LogRequests"))
        {
            var minLogLevel = _configuration.GetValue<LogLevel>(
                "Weaviate:RequestLoggingLevel",
                LogLevel.Debug
            );

            var loggerFactory = LoggerFactory.Create(b =>
            {
                b.AddConsole().SetMinimumLevel(minLogLevel);
            });

            builder.WithLoggerFactory(loggerFactory);
            builder.UseRequestLogging(
                _configuration.GetValue<LogLevel>(
                    "Weaviate:RequestLoggingLevel",
                    LogLevel.Information
                )
            );
        }

        _weaviate = await builder.BuildAsync();

        // Global readiness gate: ensure the constructed client can reach a ready Weaviate instance.
        // Fail fast so tests surface environment issues instead of producing cascading failures.
        var ready = false;
        try
        {
            ready = await _weaviate.IsReady();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Weaviate readiness check failed during test initialization",
                ex
            );
        }

        if (!ready)
        {
            throw new InvalidOperationException(
                $"Weaviate not ready on REST:{RestPort} gRPC:{GrpcPort}. Expected a running instance for integration tests."
            );
        }

        // Enforce minimum supported version globally for integration tests.
        RequireVersion(Weaviate.Client.Tests.Common.ServerVersions.MinSupported);
    }

    /// <summary>
    /// Makes the unique collection name using the specified suffix
    /// </summary>
    /// <param name="suffix">The suffix</param>
    /// <param name="collectionNamePartSeparator">The collection name part separator</param>
    /// <returns>The string</returns>
    public string MakeUniqueCollectionName(string? suffix, string collectionNamePartSeparator = "_")
    {
        var strings = new string?[]
        {
            TestContext.Current.TestMethod?.MethodName,
            TestContext.Current.Test?.UniqueID,
            suffix,
        }
            .Where(s => !string.IsNullOrEmpty(s))
            .Cast<string>();

        return string.Join(collectionNamePartSeparator, strings);
    }

    /// <summary>
    /// Makes the unique collection name using the specified suffix
    /// </summary>
    /// <typeparam name="TData">The data</typeparam>
    /// <param name="suffix">The suffix</param>
    /// <param name="collectionNamePartSeparator">The collection name part separator</param>
    /// <returns>The string</returns>
    public string MakeUniqueCollectionName<TData>(
        string? suffix,
        string collectionNamePartSeparator = "_"
    )
    {
        var strings = new string?[]
        {
            TestContext.Current.TestMethod?.MethodName,
            TestContext.Current.Test?.UniqueID,
            typeof(TData).Name,
            suffix,
        }
            .Where(s => !string.IsNullOrEmpty(s))
            .Cast<string>();

        return string.Join(collectionNamePartSeparator, strings);
    }

    /// <summary>
    /// Collections the factory using the specified c
    /// </summary>
    /// <typeparam name="TData">The data</typeparam>
    /// <param name="c">The </param>
    /// <returns>The collection client</returns>
    public async Task<CollectionClient> CollectionFactory<TData>(CollectionCreateParams c)
    {
        await _weaviate.Collections.Delete(c.Name);

        var collectionClient = await _weaviate.Collections.Create(c);

        _collections.Add(collectionClient.Name);

        return collectionClient;
    }

    /// <summary>
    /// Collections the factory using the specified name
    /// </summary>
    /// <typeparam name="TData">The data</typeparam>
    /// <param name="name">The name</param>
    /// <param name="description">The description</param>
    /// <param name="properties">The properties</param>
    /// <param name="references">The references</param>
    /// <param name="vectorConfig">The vector config</param>
    /// <param name="multiTenancyConfig">The multi tenancy config</param>
    /// <param name="invertedIndexConfig">The inverted index config</param>
    /// <param name="replicationConfig">The replication config</param>
    /// <param name="shardingConfig">The sharding config</param>
    /// <param name="rerankerConfig">The reranker config</param>
    /// <param name="generativeConfig">The generative config</param>
    /// <param name="collectionNamePartSeparator">The collection name part separator</param>
    /// <param name="objectTTLConfig">The object TTL configuration</param>
    /// <returns>A task containing the collection client</returns>
    public async Task<CollectionClient> CollectionFactory<TData>(
        string? name = null,
        string? description = null,
        AutoArray<Property>? properties = null,
        AutoArray<Reference>? references = null,
        VectorConfigList? vectorConfig = null,
        MultiTenancyConfig? multiTenancyConfig = null,
        InvertedIndexConfig? invertedIndexConfig = null,
        ReplicationConfig? replicationConfig = null,
        ShardingConfig? shardingConfig = null,
        IRerankerConfig? rerankerConfig = null,
        IGenerativeConfig? generativeConfig = null,
        string collectionNamePartSeparator = "_",
        ObjectTTLConfig? objectTTLConfig = null
    )
    {
        name = MakeUniqueCollectionName<TData>(name, collectionNamePartSeparator);

        description ??= TestContext.Current.TestMethod?.MethodName ?? string.Empty;

        properties ??= Property.FromClass<TData>();

        ArgumentException.ThrowIfNullOrEmpty(name);

        // Default is Vectorizer.SelfProvided
        vectorConfig ??= Configure.Vector("default", v => v.SelfProvided());

        references ??= [];

        var c = new CollectionCreateParams
        {
            Name = name,
            Description = description,
            Properties = properties?.ToArray() ?? [],
            References = references?.ToArray() ?? [],
            VectorConfig = vectorConfig,
            MultiTenancyConfig = multiTenancyConfig,
            InvertedIndexConfig = invertedIndexConfig,
            ReplicationConfig = replicationConfig,
            ShardingConfig = shardingConfig,
            RerankerConfig = rerankerConfig,
            GenerativeConfig = generativeConfig,
            ObjectTTLConfig = objectTTLConfig,
        };

        return await CollectionFactory<TData>(c);
    }

    /// <summary>
    /// Collections the factory using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="description">The description</param>
    /// <param name="properties">The properties</param>
    /// <param name="references">The references</param>
    /// <param name="vectorConfig">The vector config</param>
    /// <param name="multiTenancyConfig">The multi tenancy config</param>
    /// <param name="invertedIndexConfig">The inverted index config</param>
    /// <param name="replicationConfig">The replication config</param>
    /// <param name="shardingConfig">The sharding config</param>
    /// <param name="rerankerConfig">The reranker config</param>
    /// <param name="generativeConfig">The generative config</param>
    /// <param name="collectionNamePartSeparator">The collection name part separator</param>
    /// <param name="objectTTLConfig">The object TTL configuration</param>
    /// <returns>A task containing the collection client</returns>
    protected async Task<CollectionClient> CollectionFactory(
        string? name = null,
        string? description = null,
        AutoArray<Property>? properties = null,
        AutoArray<Reference>? references = null,
        VectorConfigList? vectorConfig = null,
        MultiTenancyConfig? multiTenancyConfig = null,
        InvertedIndexConfig? invertedIndexConfig = null,
        ReplicationConfig? replicationConfig = null,
        ShardingConfig? shardingConfig = null,
        IRerankerConfig? rerankerConfig = null,
        IGenerativeConfig? generativeConfig = null,
        string collectionNamePartSeparator = "_",
        ObjectTTLConfig? objectTTLConfig = null
    )
    {
        return await CollectionFactory<object>(
            name,
            description,
            properties,
            references,
            vectorConfig,
            multiTenancyConfig,
            invertedIndexConfig,
            replicationConfig,
            shardingConfig,
            rerankerConfig,
            generativeConfig,
            collectionNamePartSeparator,
            objectTTLConfig
        );
    }

    /// <summary>
    /// Versions the is in range using the specified version
    /// </summary>
    /// <param name="version">The version</param>
    /// <param name="minimumVersion">The minimum version</param>
    /// <param name="maximumVersion">The maximum version</param>
    /// <returns>The bool</returns>
    protected static bool VersionIsInRange(
        Version version,
        string minimumVersion,
        string? maximumVersion = null
    )
    {
        return (
            version >= System.Version.Parse(minimumVersion)
            && (maximumVersion == null || version <= System.Version.Parse(maximumVersion))
        );
    }

    /// <summary>
    /// Servers the version is in range using the specified minimum version
    /// </summary>
    /// <param name="minimumVersion">The minimum version</param>
    /// <param name="maximumVersion">The maximum version</param>
    /// <returns>The bool</returns>
    protected bool ServerVersionIsInRange(string minimumVersion, string? maximumVersion = null)
    {
        if (_weaviate.WeaviateVersion == null)
        {
            return false;
        }
        return VersionIsInRange(_weaviate.WeaviateVersion, minimumVersion, maximumVersion);
    }

    /// <summary>
    /// Requires the version using the specified minimum version
    /// </summary>
    /// <param name="minimumVersion">The minimum version</param>
    /// <param name="maximumVersion">The maximum version</param>
    /// <param name="message">The message</param>
    protected void RequireVersion(
        string minimumVersion,
        string? maximumVersion = null,
        string? message = ""
    )
    {
        if (!ServerVersionIsInRange(minimumVersion, maximumVersion))
        {
            if (maximumVersion is null)
            {
                Assert.Skip(
                    string.Join(
                        " ",
                        message,
                        $"Weaviate minimum version should be at least {minimumVersion}. Current version: {_weaviate.WeaviateVersion}"
                    )
                );
            }
            else
            {
                Assert.Skip(
                    string.Join(
                        " ",
                        message,
                        $"Weaviate minimum version should be between {minimumVersion} and {maximumVersion}. Current version: {_weaviate.WeaviateVersion}"
                    )
                );
            }
        }
    }

    /// <summary>
    /// Skips the test if the connected server version does not meet the minimum version
    /// declared by <see cref="RequiresWeaviateVersionAttribute"/> on the specified method.
    /// </summary>
    /// <typeparam name="TClient">The client class that declares the method.</typeparam>
    /// <param name="methodName">The method name (use <c>nameof()</c>).</param>
    protected void RequireVersion<TClient>(string methodName)
    {
        var attr = typeof(TClient)
            .GetMethod(methodName)
            ?.GetCustomAttribute<RequiresWeaviateVersionAttribute>();

        if (attr is not null)
        {
            RequireVersion(attr.MinimumVersion.ToString());
        }
    }
}

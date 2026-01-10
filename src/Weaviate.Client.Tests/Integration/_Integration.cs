using System.Diagnostics;
using dotenv.net;
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

        _httpMessageHandler = new LoggingHandler(str =>
        {
            Debug.WriteLine(str);
        })
        {
            InnerHandler = new HttpClientHandler(),
        };
    }

    /// <summary>
    /// Gets the value of the credentials
    /// </summary>
    public virtual ICredentials? Credentials => null;

    /// <summary>
    /// Gets the value of the rest port
    /// </summary>
    public virtual ushort RestPort => 8080;

    /// <summary>
    /// Gets the value of the grpc port
    /// </summary>
    public virtual ushort GrpcPort => 50051; // default local gRPC port

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
        string collectionNamePartSeparator = "_"
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
        string collectionNamePartSeparator = "_"
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
            collectionNamePartSeparator
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
        System.Version version,
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
}

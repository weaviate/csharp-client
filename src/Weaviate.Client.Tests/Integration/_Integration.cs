using System.Diagnostics;
using dotenv.net;
using Weaviate.Client.Models;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace Weaviate.Client.Tests.Integration;

public abstract partial class IntegrationTests : IAsyncDisposable, IAsyncLifetime
{
    private const string ENV_FILE = "development.env";
    const bool _deleteCollectionsAfterTest = true;
    List<string> _collections = new();

    protected WeaviateClient _weaviate = null!;
    protected HttpMessageHandler? _httpMessageHandler;

    protected static readonly Guid[] _reusableUuids =
    [
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
    ];

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

    public virtual ICredentials? Credentials => null;

    public virtual ushort RestPort => 8080;
    public virtual ushort GrpcPort => 50051; // default local gRPC port

    public virtual async ValueTask DisposeAsync()
    {
        if (_deleteCollectionsAfterTest && _collections.Count > 0)
        {
            await Task.WhenAll(_collections.Select(c => _weaviate.Collections.Delete(c)));
        }
        _weaviate.Dispose();
    }

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

    public async Task<CollectionClient> CollectionFactory<TData>(CollectionCreateParams c)
    {
        await _weaviate.Collections.Delete(c.Name);

        var collectionClient = await _weaviate.Collections.Create(c);

        _collections.Add(collectionClient.Name);

        return collectionClient;
    }

    public async Task<CollectionClient> CollectionFactory<TData>(
        string? name = null,
        string? description = null,
        OneOrManyOf<Property>? properties = null,
        OneOrManyOf<Reference>? references = null,
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
        vectorConfig ??= new VectorConfig("default");

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

    protected async Task<CollectionClient> CollectionFactory(
        string? name = null,
        string? description = null,
        OneOrManyOf<Property>? properties = null,
        OneOrManyOf<Reference>? references = null,
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

    protected bool ServerVersionIsInRange(string minimumVersion, string? maximumVersion = null)
    {
        if (_weaviate.WeaviateVersion == null)
        {
            return false;
        }
        return VersionIsInRange(_weaviate.WeaviateVersion, minimumVersion, maximumVersion);
    }

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

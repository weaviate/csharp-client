using System.Diagnostics;
using dotenv.net;
using Weaviate.Client.Models;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace Weaviate.Client.Tests.Integration;

public abstract partial class IntegrationTests : IAsyncDisposable
{
    private const string ENV_FILE = "development.env";
    const bool _deleteCollectionsAfterTest = true;
    List<string> _collections = new();

    protected WeaviateClient _weaviate;
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

        var builder = WeaviateClientBuilder.Local(httpMessageHandler: _httpMessageHandler);

        if (
            Environment.GetEnvironmentVariable("WEAVIATE_OPENAI_API_KEY") is { } openaiKey
            && !string.IsNullOrEmpty(openaiKey)
        )
        {
            builder.WithOpenAI(openaiKey);
        }

        _weaviate = builder.Build();
    }

    public async ValueTask DisposeAsync()
    {
        if (_deleteCollectionsAfterTest && _collections.Count > 0)
        {
            await Task.WhenAll(_collections.Select(c => _weaviate.Collections.Delete(c)));
        }

        _weaviate.Dispose();
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

    public async Task<CollectionClient<TData>> CollectionFactory<TData>(Collection c)
    {
        await _weaviate.Collections.Delete(c.Name);

        var collectionClient = await _weaviate.Collections.Create<TData>(c);

        _collections.Add(collectionClient.Name);

        return collectionClient;
    }

    public async Task<CollectionClient<TData>> CollectionFactory<TData>(
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

        var c = new Collection
        {
            Name = name,
            Description = description,
            Properties = properties?.Concat(references!.Select(p => (Property)p)).ToList() ?? [],
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

    protected async Task<CollectionClient<object>> CollectionFactory(
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
}

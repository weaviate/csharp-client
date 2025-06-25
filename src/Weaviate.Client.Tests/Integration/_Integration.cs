using System.Diagnostics;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace Weaviate.Client.Tests.Integration;

public abstract partial class IntegrationTests : IAsyncDisposable
{
    const bool _deleteCollectionsAfterTest = true;
    List<string> _collections = new();

    protected WeaviateClient _weaviate;
    HttpClient _httpClient;

    protected static readonly Guid[] _reusableUuids =
    [
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
    ];

    public IntegrationTests()
    {
        _httpClient = new HttpClient(
            new LoggingHandler(str =>
            {
                Debug.WriteLine(str);
            })
            {
                InnerHandler = new HttpClientHandler(),
            }
        );

        _weaviate = Connect.Local(httpClient: _httpClient);
    }

    public async ValueTask DisposeAsync()
    {
        if (_deleteCollectionsAfterTest && _collections.Count > 0)
        {
            await Task.WhenAll(_collections.Select(c => _weaviate.Collections.Delete(c)));
        }

        _weaviate.Dispose();
    }

    protected async Task<CollectionClient<TData>> CollectionFactory<TData>(
        string? name = null,
        string? description = null,
        IList<Property>? properties = null,
        IList<ReferenceProperty>? references = null,
        VectorConfigList? vectorConfig = null,
        MultiTenancyConfig? multiTenancyConfig = null,
        InvertedIndexConfig? invertedIndexConfig = null,
        ReplicationConfig? replicationConfig = null,
        ShardingConfig? shardingConfig = null,
        string collectionNamePartSeparator = "_"
    )
    {
        description ??= TestContext.Current.TestMethod?.MethodName ?? string.Empty;

        var strings = new string?[]
        {
            TestContext.Current.TestMethod?.MethodName,
            typeof(TData).Name,
            TestContext.Current.Test?.UniqueID,
            name,
        }
            .Where(s => !string.IsNullOrEmpty(s))
            .Cast<string>();

        name = string.Join(collectionNamePartSeparator, strings);

        properties ??= Property.FromCollection<TData>();

        ArgumentException.ThrowIfNullOrEmpty(name);

        // Default is VectorizerConfig.None
        vectorConfig ??= new VectorConfig("default");

        references ??= [];

        var c = new Collection
        {
            Name = name,
            Description = description,
            Properties = properties.Concat(references!.Select(p => (Property)p)).ToList(),
            VectorConfig = vectorConfig,
            MultiTenancyConfig = multiTenancyConfig,
            InvertedIndexConfig = invertedIndexConfig,
            ReplicationConfig = replicationConfig,
            ShardingConfig = shardingConfig,
        };

        await _weaviate.Collections.Delete(name);

        var collectionClient = await _weaviate.Collections.Create<TData>(c);

        _collections.Add(collectionClient.Name);

        return collectionClient;
    }

    protected async Task<CollectionClient<dynamic>> CollectionFactory(
        string? name = null,
        string? description = null,
        IList<Property>? properties = null,
        IList<ReferenceProperty>? references = null,
        VectorConfigList? vectorConfig = null,
        MultiTenancyConfig? multiTenancyConfig = null,
        InvertedIndexConfig? invertedIndexConfig = null,
        ReplicationConfig? replicationConfig = null,
        ShardingConfig? shardingConfig = null,
        string collectionNamePartSeparator = "_"
    )
    {
        return await CollectionFactory<dynamic>(
            name,
            description,
            properties,
            references,
            vectorConfig,
            multiTenancyConfig,
            invertedIndexConfig,
            replicationConfig,
            shardingConfig,
            collectionNamePartSeparator
        );
    }
}

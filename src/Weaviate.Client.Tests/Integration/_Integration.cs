using System.Diagnostics;
using Weaviate.Client.Models;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace Weaviate.Client.Tests.Integration;

internal class TestData
{
    public string Name { get; set; } = string.Empty;
    public int Size { get; set; } = 0;
}

internal class TestDataValue
{
    public string Value { get; set; } = string.Empty;
}

[Collection("BasicTests")]
public partial class BasicTests : IAsyncDisposable
{
    const bool _deleteCollectionsAfterTest = true;
    List<string> _collections = new();

    WeaviateClient _weaviate;
    HttpClient _httpClient;

    static readonly Guid[] _reusableUuids =
    [
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
    ];

    public BasicTests()
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

        _weaviate = new WeaviateClient(httpClient: _httpClient);
    }

    public async ValueTask DisposeAsync()
    {
        if (_deleteCollectionsAfterTest && _collections.Count > 0)
        {
            await Task.WhenAll(_collections.Select(c => _weaviate.Collections.Delete(c)));
        }

        _weaviate.Dispose();
    }

    async Task<CollectionClient<TData>> CollectionFactory<TData>(
        string? name = null,
        string? description = null,
        IList<Property>? properties = null,
        IList<ReferenceProperty>? references = null,
        IDictionary<string, VectorConfig>? vectorConfig = null,
        InvertedIndexConfig? invertedIndexConfig = null
    )
    {
        description ??= TestContext.Current.TestMethod?.MethodName ?? string.Empty;

        name ??= typeof(TData).Name + TestContext.Current.Test?.UniqueID;

        name = $"{TestContext.Current.TestMethod?.MethodName ?? string.Empty}_{name}";

        properties ??= Property.FromCollection<TData>();

        ArgumentException.ThrowIfNullOrEmpty(name);

        vectorConfig ??= new Dictionary<string, VectorConfig>
        {
            {
                "default",
                new VectorConfig
                {
                    Vectorizer = new Dictionary<string, object> { { "none", new { } } },
                    VectorIndexType = "hnsw",
                }
            },
        };

        references ??= [];

        var c = new Collection
        {
            Name = name,
            Description = description,
            Properties = properties.Concat(references!.Select(p => (Property)p)).ToList(),
            VectorConfig = vectorConfig,
            InvertedIndexConfig = invertedIndexConfig,
        };

        await _weaviate.Collections.Delete(name);

        var collectionClient = await _weaviate.Collections.Create<TData>(c);

        _collections.Add(name);

        return collectionClient;
    }

    async Task<CollectionClient<dynamic>> CollectionFactory(
        string? name = null,
        string? description = null,
        IList<Property>? properties = null,
        IList<ReferenceProperty>? references = null,
        IDictionary<string, VectorConfig>? vectorConfig = null,
        InvertedIndexConfig? invertedIndexConfig = null
    )
    {
        return await CollectionFactory<dynamic>(
            name,
            description,
            properties,
            references,
            vectorConfig,
            invertedIndexConfig
        );
    }
}

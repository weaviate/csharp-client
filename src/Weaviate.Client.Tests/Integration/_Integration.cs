using Weaviate.Client.Models;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace Weaviate.Client.Tests.Integration;

internal class TestData
{
    public string Name { get; set; } = string.Empty;
}

internal class TestDataValue
{
    public string Value { get; set; } = string.Empty;
}

[Collection("BasicTests")]
public partial class BasicTests : IAsyncDisposable
{
    const bool _deleteCollectionsAfterTest = true;

    WeaviateClient _weaviate;

    public BasicTests(ITestOutputHelper output)
    {
        _weaviate = new WeaviateClient();
    }

    public async ValueTask DisposeAsync()
    {
        if (_deleteCollectionsAfterTest && TestContext.Current.TestMethod?.MethodName is not null)
        {
            await _weaviate.Collections.Delete(TestContext.Current.TestMethod!.MethodName);
        }

        _weaviate.Dispose();
    }

    async Task<CollectionClient<TData>> CollectionFactory<TData>(string name,
                                                                 string description,
                                                                 IList<Property> properties,
                                                                 IList<ReferenceProperty>? references = null,
                                                                 IDictionary<string, VectorConfig>? vectorConfig = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = TestContext.Current.TestMethod?.MethodName ?? string.Empty;
        }

        ArgumentException.ThrowIfNullOrEmpty(name);

        if (vectorConfig is null)
        {
            vectorConfig = new Dictionary<string, VectorConfig>
            {
                {
                    "default", new VectorConfig {
                        Vectorizer = new Dictionary<string, object> { { "none", new { } } },
                        VectorIndexType = "hnsw"
                    }
                }
            };
        }

        references = references ?? [];

        var c = new Collection
        {
            Name = name,
            Description = description,
            Properties = properties.Concat(references!.Select(p => (Property)p)).ToList(),
            VectorConfig = vectorConfig,
        };

        await _weaviate.Collections.Delete(name);

        var collectionClient = await _weaviate.Collections.Create<TData>(c);

        return collectionClient;
    }

    async Task<CollectionClient<dynamic>> CollectionFactory(string name,
                                                            string description,
                                                            IList<Property> properties,
                                                            IList<ReferenceProperty>? references = null,
                                                            IDictionary<string, VectorConfig>? vectorConfig = null)
    {
        return await CollectionFactory<dynamic>(name, description, properties, references, vectorConfig);
    }

    WeaviateObject<TData> DataFactory<TData>(TData value)
    {
        return new WeaviateObject<TData>()
        {
            Data = value
        };
    }

    WeaviateObject<TData> DataFactory<TData>(TData value, string collectionName)
    {
        return new WeaviateObject<TData>(collectionName)
        {
            Data = value
        };
    }
}

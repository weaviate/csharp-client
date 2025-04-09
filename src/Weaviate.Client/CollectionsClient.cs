namespace Weaviate.Client;

using Weaviate.Client.Models;
using Weaviate.Client.Rest.Models;

internal static class WeaviateClientExtensions
{
    internal static Collection ToCollection(this CollectionGeneric collection)
    {
        return new Collection()
        {
            Name = collection.Class,
            Description = collection.Description,
            Properties = collection.Properties.Select(p => new Models.Property()
            {
                Name = p.Name,
                DataType = p.DataType.ToList()
            }).ToList(),
            InvertedIndexConfig = null,
            ShardingConfig = collection.ShardingConfig,
            ReplicationConfig = null,
            ModuleConfig = collection.ModuleConfig,
            MultiTenancyConfig = null,
            VectorConfig =
                collection.VectorConfig.ToList()
                .ToDictionary(
                    e => e.Key,
                    e => new Models.VectorConfig
                    {
                        VectorIndexConfig = e.Value.VectorIndexConfig,
                        VectorIndexType = e.Value.VectorIndexType,
                        Vectorizer = e.Value.Vectorizer,
                    }
                    ),
            Vectorizer = collection.Vectorizer,
            VectorIndexType = collection.VectorIndexType,
            VectorIndexConfig = collection.VectorIndexConfig,
        };
    }
}

public struct CollectionsClient
{
    private readonly WeaviateClient _client;

    internal CollectionsClient(WeaviateClient client)
    {
        _client = client;
    }

    public async Task<CollectionClient> Create(Action<Collection> collectionConfigurator)
    {
        var collection = new Collection();

        collectionConfigurator(collection);

        var data = new CollectionGeneric()
        {
            Class = collection.Name,
            Description = collection.Description,
            Properties = new List<Rest.Models.Property>(),
            VectorIndexType = collection.VectorIndexType,
            VectorIndexConfig = collection.VectorIndexConfig,
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = collection.ModuleConfig,
        };

        foreach (var property in collection.Properties)
        {
            data.Properties.Add(new Rest.Models.Property()
            {
                Name = property.Name,
                DataType = [.. property.DataType]
            });
        }

        if (collection.InvertedIndexConfig != null)
        {
            data.InvertedIndexConfig = new Rest.Models.InvertedIndexConfig()
            {
                Bm25 = collection.InvertedIndexConfig.Bm25 == null ? null : new Rest.Models.BM25Config
                {
                    B = collection.InvertedIndexConfig.Bm25.B,
                    K1 = collection.InvertedIndexConfig.Bm25.K1,
                },
                Stopwords = collection.InvertedIndexConfig.Stopwords == null ? null : new Rest.Models.StopwordConfig
                {
                    Additions = collection.InvertedIndexConfig.Stopwords.Additions,
                    Preset = collection.InvertedIndexConfig.Stopwords.Preset,
                    Removals = collection.InvertedIndexConfig.Stopwords.Removals,
                },
                CleanupIntervalSeconds = collection.InvertedIndexConfig.CleanupIntervalSeconds,
                IndexNullState = collection.InvertedIndexConfig.IndexNullState,
                IndexPropertyLength = collection.InvertedIndexConfig.IndexPropertyLength,
                IndexTimestamps = collection.InvertedIndexConfig.IndexTimestamps,
            };
        }

        var response = await _client.RestClient.CollectionCreate(data);

        return new CollectionClient(_client, response.ToCollection());
    }

    // TODO Return bool
    public async Task Delete(string name)
    {
        await _client.RestClient.CollectionDelete(name);
    }

    public CollectionClient this[string name] => Get(name).Result;

    public async Task<CollectionClient> Get(string name)
    {
        var response = await _client.RestClient.CollectionGet(name);

        // TODO throw if collection is not found.

        var collection = new CollectionClient(_client, response.ToCollection());

        return collection;
    }

    public async IAsyncEnumerable<Collection> List()
    {
        var response = await _client.RestClient.CollectionList();

        foreach (var c in response.Collections)
        {
            yield return c.ToCollection();
        }
    }
}

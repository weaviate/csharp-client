namespace Weaviate.Client;

internal static class WeaviateClientExtensions
{
    internal static Models.Collection ToCollection(this Rest.Models.CollectionGeneric collection)
    {
        return new Models.Collection()
        {
            Name = collection.Class,
            Description = collection.Description,
            Properties = collection.Properties.Select(p => new Models.Property()
            {
                Name = p.Name,
                DataType = p.DataType.ToList()
            }).ToList(),
            InvertedIndexConfig = (collection.InvertedIndexConfig is Rest.Models.InvertedIndexConfig iic)
                ? new Models.InvertedIndexConfig()
                {
                    Bm25 = iic.Bm25 == null ? null : new Models.BM25Config
                    {
                        B = iic.Bm25.B,
                        K1 = iic.Bm25.K1,
                    },
                    Stopwords = (iic.Stopwords is Rest.Models.StopwordConfig swc)
                    ? new Models.StopwordConfig
                    {
                        Additions = swc.Additions,
                        Preset = swc.Preset,
                        Removals = swc.Removals,
                    } : null,
                    CleanupIntervalSeconds = iic.CleanupIntervalSeconds,
                    IndexNullState = iic.IndexNullState,
                    IndexPropertyLength = iic.IndexPropertyLength,
                    IndexTimestamps = iic.IndexTimestamps,
                } : null,
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = collection.ModuleConfig,
            ReplicationConfig = (collection.ReplicationConfig is Rest.Models.ReplicationConfig rc)
                ? new Models.ReplicationConfig
                {
                    AsyncEnabled = rc.AsyncEnabled,
                    Factor = rc.Factor,
                    DeletionStrategy = (Models.DeletionStrategy?)rc.DeletionStrategy,
                } : null,
            MultiTenancyConfig = (collection.MultiTenancyConfig is Rest.Models.MultiTenancyConfig mtc)
                ? new Models.MultiTenancyConfig
                {
                    Enabled = mtc.Enabled,
                    AutoTenantActivation = mtc.AutoTenantActivation,
                    AutoTenantCreation = mtc.AutoTenantCreation,
                } : null,
            VectorConfig =
                collection.VectorConfig?.ToList()
                .ToDictionary(
                    e => e.Key,
                    e => new Models.VectorConfig
                    {
                        VectorIndexConfig = e.Value.VectorIndexConfig,
                        VectorIndexType = e.Value.VectorIndexType,
                        Vectorizer = e.Value.Vectorizer,
                    }
                    ) ?? new Dictionary<string, Models.VectorConfig>(),
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

    public async Task<CollectionClient> Create(Action<Models.Collection> collectionConfigurator)
    {
        var collection = new Models.Collection();

        collectionConfigurator(collection);

        var data = new Rest.Models.CollectionGeneric()
        {
            Class = collection.Name,
            Description = collection.Description,
            Properties = new List<Rest.Models.Property>(),
            VectorConfig = collection.VectorConfig?.ToList()
                .ToDictionary(
                    e => e.Key,
                    e => new Rest.Models.VectorConfig
                    {
                        VectorIndexConfig = e.Value.VectorIndexConfig,
                        VectorIndexType = e.Value.VectorIndexType,
                        Vectorizer = e.Value.Vectorizer,
                    }
                    ) ?? new Dictionary<string, Rest.Models.VectorConfig>(),
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = collection.ModuleConfig,
            VectorIndexType = collection.VectorIndexType,
            VectorIndexConfig = collection.VectorIndexConfig,
            Vectorizer = collection.Vectorizer,
        };

        foreach (var property in collection.Properties)
        {
            data.Properties.Add(new Rest.Models.Property()
            {
                Name = property.Name,
                DataType = [.. property.DataType]
            });
        }

        if (collection.ReplicationConfig is Models.ReplicationConfig rc)
        {
            data.ReplicationConfig = new Rest.Models.ReplicationConfig()
            {
                AsyncEnabled = rc.AsyncEnabled,
                DeletionStrategy = (Rest.Models.DeletionStrategy?)rc.DeletionStrategy,
                Factor = rc.Factor
            };
        }

        if (collection.MultiTenancyConfig is Models.MultiTenancyConfig mtc)
        {
            data.MultiTenancyConfig = new Rest.Models.MultiTenancyConfig()
            {
                AutoTenantActivation = mtc.AutoTenantActivation,
                AutoTenantCreation = mtc.AutoTenantCreation,
                Enabled = mtc.Enabled,
            };
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

    public async Task Delete(string name)
    {
        await _client.RestClient.CollectionDelete(name);
    }

    public CollectionClient this[string name] => Get(name).Result;

    public async Task<CollectionClient> Get(string name)
    {
        var response = await _client.RestClient.CollectionGet(name);

        if (response is null)
        {
            return null!;
        }

        var collection = new CollectionClient(_client, response.ToCollection());

        return collection;
    }

    public async IAsyncEnumerable<Models.Collection> List()
    {
        var response = await _client.RestClient.CollectionList();

        foreach (var c in response.Collections)
        {
            yield return c.ToCollection();
        }
    }
}

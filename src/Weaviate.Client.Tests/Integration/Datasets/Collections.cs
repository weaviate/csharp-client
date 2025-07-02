using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public class DatasetCollectionCreateAndExport : TheoryData<string>
{
    private static Property _nameProperty = Property.Text(
        "Name",
        indexFilterable: true,
        indexRangeFilters: false,
        indexSearchable: true,
        tokenization: PropertyTokenization.Word
    );

    private static VectorIndexConfig _vectorIndexConfig = new VectorIndex.HNSW()
    {
        CleanupIntervalSeconds = 300,
        Distance = VectorIndexConfig.VectorDistance.Cosine,
        DynamicEfFactor = 8,
        DynamicEfMax = 500,
        DynamicEfMin = 100,
        Ef = -1,
        EfConstruction = 128,
        FilterStrategy = VectorIndexConfig.VectorIndexFilterStrategy.Sweeping,
        FlatSearchCutoff = 40000,
        MaxConnections = 32,
        Skip = false,
        VectorCacheMaxObjects = 1000000000000L,
    };

    public static Dictionary<string, Collection> Cases =>
        new()
        {
            ["with default values"] = new Collection
            {
                Name = "CreationAndExport",
                Description = "My own description too",
                Properties = [_nameProperty],
                VectorConfig = new VectorConfig(
                    "default",
                    new Vectorizer.SelfProvided(),
                    vectorIndexConfig: _vectorIndexConfig
                ),
                InvertedIndexConfig = InvertedIndexConfig.Default,
                ReplicationConfig = ReplicationConfig.Default,
                ShardingConfig = ShardingConfig.Default,
                MultiTenancyConfig = MultiTenancyConfig.Default,
            },
            ["with non default values, with sharding"] = new Collection
            {
                Name = "CreationAndExportNonDefaultSharding",
                Description = "My own description too",
                Properties =
                [
                    _nameProperty,
                    Property.Int(
                        "SomeNumber",
                        indexFilterable: true,
                        indexRangeFilters: false,
                        indexSearchable: false
                    ),
                ],
                VectorConfig = Configure
                    .Vectors.Text2VecContextionary(false)
                    .New("nondefault", _vectorIndexConfig),
                InvertedIndexConfig = new()
                {
                    Bm25 = new() { B = 0.70f, K1 = 1.3f },
                    CleanupIntervalSeconds = 30,
                    IndexNullState = true,
                    IndexPropertyLength = true,
                    IndexTimestamps = true,
                    Stopwords = new()
                    {
                        Preset = StopwordConfig.Presets.None,
                        Additions = ["plus"],
                        Removals = ["minus"],
                    },
                },
                ReplicationConfig = new()
                {
                    DeletionStrategy = DeletionStrategy.TimeBasedResolution,
                    AsyncEnabled = true,
                    Factor = 1,
                },
                ShardingConfig = new()
                {
                    ActualCount = 1,
                    ActualVirtualCount = 136,
                    DesiredCount = 1,
                    DesiredVirtualCount = 136,
                    VirtualPerPhysical = 136,
                },
                MultiTenancyConfig = MultiTenancyConfig.Default,
            },
            ["with non default values, with multi-tenancy"] = new Collection
            {
                Name = "CreationAndExportNonDefaultSharding",
                Description = "My own description too",
                Properties =
                [
                    _nameProperty,
                    Property.Int(
                        "SomeNumber",
                        indexFilterable: true,
                        indexRangeFilters: false,
                        indexSearchable: false
                    ),
                ],
                VectorConfig = Configure
                    .Vectors.Text2VecContextionary(false)
                    .New("nondefault", _vectorIndexConfig),
                InvertedIndexConfig = new()
                {
                    Bm25 = new() { B = 0.70f, K1 = 1.3f },
                    CleanupIntervalSeconds = 30,
                    IndexNullState = true,
                    IndexPropertyLength = true,
                    IndexTimestamps = true,
                    Stopwords = new()
                    {
                        Preset = StopwordConfig.Presets.None,
                        Additions = ["plus"],
                        Removals = ["minus"],
                    },
                },
                ReplicationConfig = new()
                {
                    DeletionStrategy = DeletionStrategy.TimeBasedResolution,
                    AsyncEnabled = true,
                    Factor = 1,
                },
                ShardingConfig = null,
                MultiTenancyConfig = new()
                {
                    AutoTenantActivation = true,
                    AutoTenantCreation = true,
                    Enabled = true,
                },
            },
        };

    public DatasetCollectionCreateAndExport()
        : base(Cases.Keys) { }
}

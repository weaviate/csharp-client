using Weaviate.Client.Models;
using static Weaviate.Client.Models.VectorIndex;

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

    private static VectorIndex.HNSW _vectorIndexConfigHNSW_base = new VectorIndex.HNSW()
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

    private static VectorIndexConfig _vectorIndexConfigHNSW_BQ = _vectorIndexConfigHNSW_base with
    {
        Quantizer = new Quantizers.BQ() { Cache = true, RescoreLimit = 64 },
    };

    private static VectorIndexConfig _vectorIndexConfigHNSW_PQ = _vectorIndexConfigHNSW_base with
    {
        Quantizer = new Quantizers.PQ()
        {
            Encoder = new()
            {
                Distribution = Quantizers.DistributionType.LogNormal,
                Type = Quantizers.EncoderType.Kmeans,
            },
            BitCompression = false,
            Segments = 0,
            Centroids = 256,
            TrainingLimit = 100000,
        },
    };

    private static VectorIndexConfig _vectorIndexConfigHNSW_SQ = _vectorIndexConfigHNSW_base with
    {
        Quantizer = new Quantizers.SQ() { TrainingLimit = 100000, RescoreLimit = 20 },
    };

    private static VectorIndex.Flat _vectorIndexConfigFlat_base = new VectorIndex.Flat()
    {
        Distance = VectorIndexConfig.VectorDistance.Cosine,
        VectorCacheMaxObjects = 1000000000000L,
    };

    private static VectorIndex.Flat _vectorIndexConfigFlat_BQ = _vectorIndexConfigFlat_base with
    {
        Quantizer = new Quantizers.BQ() { Cache = true, RescoreLimit = 64 },
    };

    private static VectorIndex.Dynamic _vectorIndexConfigDynamic_base = new VectorIndex.Dynamic()
    {
        Flat = _vectorIndexConfigFlat_base,
        Hnsw = _vectorIndexConfigHNSW_base,
        Distance = VectorIndexConfig.VectorDistance.Cosine,
        Threshold = 1000,
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
                    vectorIndexConfig: _vectorIndexConfigHNSW_base
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
                    .New("nondefault", _vectorIndexConfigHNSW_base),
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
                    .New("nondefault", _vectorIndexConfigHNSW_base),
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
            ["with all vector index configurations"] = new Collection
            {
                Name = "AllVectorIndexConfigurations",
                Description = "Vector Index Configurations",
                Properties = [_nameProperty],
                VectorConfig = new[]
                {
                    Configure.Vectors.SelfProvided("hnswbase", _vectorIndexConfigHNSW_base),
                    // TODO BQ is only returning Enabled property for HNSW
                    // Configure.Vectors.SelfProvided("hnswbq", _vectorIndexConfigHNSW_BQ),
                    Configure.Vectors.SelfProvided("hnswpq", _vectorIndexConfigHNSW_PQ),
                    Configure.Vectors.SelfProvided("hnswsq", _vectorIndexConfigHNSW_SQ),
                    Configure.Vectors.SelfProvided("flatbase", _vectorIndexConfigFlat_base),
                    Configure.Vectors.SelfProvided("flatbq", _vectorIndexConfigFlat_BQ),
                    // Requires ASYNC_INDEXING: 'true'
                    // Configure.Vectors.SelfProvided("dynamicbase", _vectorIndexConfigDynamic_base),
                },
                InvertedIndexConfig = InvertedIndexConfig.Default,
                ReplicationConfig = ReplicationConfig.Default,
                ShardingConfig = ShardingConfig.Default,
                MultiTenancyConfig = MultiTenancyConfig.Default,
            },
        };

    public DatasetCollectionCreateAndExport()
        : base(Cases.Keys) { }
}

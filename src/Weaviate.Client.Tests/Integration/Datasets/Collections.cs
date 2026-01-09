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

    private static VectorIndex.HNSW _vectorIndexConfigHNSW_BQ = _vectorIndexConfigHNSW_base with
    {
        Quantizer = new Quantizers.BQ() { Cache = true, RescoreLimit = 64 },
    };

    private static VectorIndex.HNSW _vectorIndexConfigHNSW_PQ = _vectorIndexConfigHNSW_base with
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

    private static VectorIndex.HNSW _vectorIndexConfigHNSW_SQ = _vectorIndexConfigHNSW_base with
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

    public static Dictionary<string, CollectionCreateParams> Cases =>
        new()
        {
            ["with default values"] = new CollectionCreateParams
            {
                Name = "CreationAndExport",
                Description = "My own description too",
                Properties = [_nameProperty],
                VectorConfig = Configure.Vector(
                    "default",
                    v => v.SelfProvided(),
                    index: _vectorIndexConfigHNSW_base
                ),
                InvertedIndexConfig = InvertedIndexConfig.Default,
                ReplicationConfig = ReplicationConfig.Default,
                ShardingConfig = ShardingConfig.Default,
                MultiTenancyConfig = MultiTenancyConfig.Default,
            },
            ["with non default values, with sharding"] = new CollectionCreateParams
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
                VectorConfig = Configure.Vector(
                    "nondefault",
                    t =>
                        t.Text2VecTransformers(
                            vectorizeCollectionName: false,
                            poolingStrategy: "masked_mean"
                        ),
                    _vectorIndexConfigHNSW_base
                ),
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
            ["with non default values, with multi-tenancy"] = new CollectionCreateParams
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
                VectorConfig =
                [
                    Configure.Vector(
                        "nondefault",
                        t =>
                            t.Text2VecTransformers(
                                vectorizeCollectionName: false,
                                poolingStrategy: "masked_mean"
                            ),
                        _vectorIndexConfigHNSW_base
                    ),
                ],
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
            ["with all vector index configurations"] = new CollectionCreateParams
            {
                Name = "AllVectorIndexConfigurations",
                Description = "Vector Index Configurations",
                Properties = [_nameProperty],
                VectorConfig =
                [
                    Configure.Vector(
                        "hnswbase",
                        t => t.SelfProvided(),
                        _vectorIndexConfigHNSW_base
                    ),
                    // TODO BQ is only returning Enabled property for HNSW
                    // Configure.Vectorizer.SelfProvided("hnswbq", _vectorIndexConfigHNSW_BQ),
                    Configure.Vector("hnswpq", t => t.SelfProvided(), _vectorIndexConfigHNSW_PQ),
                    Configure.Vector("hnswsq", t => t.SelfProvided(), _vectorIndexConfigHNSW_SQ),
                    Configure.Vector(
                        "flatbase",
                        t => t.SelfProvided(),
                        _vectorIndexConfigFlat_base
                    ),
                    Configure.Vector("flatbq", t => t.SelfProvided(), _vectorIndexConfigFlat_BQ),
                    // Requires ASYNC_INDEXING: 'true'
                    // ("dynamicbase", Configure.Vectorizer.SelfProvided(), _vectorIndexConfigDynamic_base),
                ],
                InvertedIndexConfig = InvertedIndexConfig.Default,
                ReplicationConfig = ReplicationConfig.Default,
                ShardingConfig = ShardingConfig.Default,
                MultiTenancyConfig = MultiTenancyConfig.Default,
            },
        };

    public DatasetCollectionCreateAndExport()
        : base(Cases.Keys) { }
}

using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Tests.Integration;

[Collection("TestCollections")]
public partial class CollectionsTests : IntegrationTests
{
    [Fact]
    public async Task Test_Collections_List()
    {
        // Arrange
        var collection = new[]
        {
            await CollectionFactory(
                name: "Collection1",
                properties: [Property.Text("Name")],
                vectorConfig: new VectorConfig("default", new Vectorizer.None())
            ),
            await CollectionFactory(
                name: "Collection2",
                properties: [Property.Text("Lastname")],
                vectorConfig: new VectorConfig("default", new Vectorizer.None())
            ),
            await CollectionFactory(
                name: "Collection3",
                properties: [Property.Text("Address")],
                vectorConfig: new VectorConfig("default", new Vectorizer.None())
            ),
        };

        var collectionNames = collection.Select(c => c.Name).ToHashSet();

        // Act
        var list = await _weaviate
            .Collections.List()
            .Select(x => x.Name)
            .ToHashSetAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Superset(collectionNames, list);
    }

    [Fact]
    public async Task Test_Collections_Exists()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("Name")],
            vectorConfig: new VectorConfig("default", new Vectorizer.None())
        );

        bool exists = await _weaviate.Collections.Exists(collection.Name);

        Assert.True(exists);
    }

    [Fact]
    public async Task Test_Collections_Not_Exists()
    {
        bool exists = await _weaviate.Collections.Exists("Some_Random_Name");

        Assert.False(exists);
    }

    [Fact]
    public async Task Test_Collections_Export()
    {
        var collection = await CollectionFactory(
            name: "MyOwnSuffix",
            description: "My own description too",
            properties: [Property.Text("Name")],
            vectorConfig: new VectorConfig("default", new Vectorizer.None())
        );

        var export = await _weaviate.Collections.Export(collection.Name);

        // Basic collection properties
        Assert.NotNull(export);
        Assert.EndsWith("MyOwnSuffix", export.Name);
        Assert.Equal("My own description too", export.Description);

        // Properties validation
        Assert.NotNull(export.Properties);
        Assert.Single(export.Properties);
        var property = export.Properties.First();
        Assert.Equal("name", property.Name);
        Assert.Contains("text", property.DataType);

        // InvertedIndexConfig validation
        Assert.NotNull(export.InvertedIndexConfig);
        Assert.NotNull(export.InvertedIndexConfig.Bm25);
        Assert.Equal(0.75f, export.InvertedIndexConfig.Bm25.B);
        Assert.Equal(1.2f, export.InvertedIndexConfig.Bm25.K1);
        Assert.Equal(60, export.InvertedIndexConfig.CleanupIntervalSeconds);
        Assert.False(export.InvertedIndexConfig.IndexNullState);
        Assert.False(export.InvertedIndexConfig.IndexPropertyLength);
        Assert.False(export.InvertedIndexConfig.IndexTimestamps);

        // Stopwords validation
        Assert.NotNull(export.InvertedIndexConfig.Stopwords);
        Assert.Equal("en", export.InvertedIndexConfig.Stopwords.Preset);
        Assert.Empty(export.InvertedIndexConfig.Stopwords.Additions);
        Assert.Empty(export.InvertedIndexConfig.Stopwords.Removals);

        // Module config should be null for base Collection
        Assert.Null(export.ModuleConfig);

        // MultiTenancyConfig validation
        Assert.NotNull(export.MultiTenancyConfig);
        Assert.False(export.MultiTenancyConfig.AutoTenantActivation);
        Assert.False(export.MultiTenancyConfig.AutoTenantCreation);
        Assert.False(export.MultiTenancyConfig.Enabled);

        // ReplicationConfig validation
        Assert.NotNull(export.ReplicationConfig);
        Assert.False(export.ReplicationConfig.AsyncEnabled);
        Assert.Equal(
            DeletionStrategy.NoAutomatedResolution,
            export.ReplicationConfig.DeletionStrategy
        );
        Assert.Equal(1, export.ReplicationConfig.Factor);

        // ShardingConfig validation
        Assert.NotNull(export.ShardingConfig);
        Assert.Equal(1, export.ShardingConfig?.ActualCount);
        Assert.Equal(128, export.ShardingConfig?.ActualVirtualCount);
        Assert.Equal(1, export.ShardingConfig?.DesiredCount);
        Assert.Equal(128, export.ShardingConfig?.DesiredVirtualCount);
        Assert.Equal("murmur3", export.ShardingConfig?.Function);
        Assert.Equal("_id", export.ShardingConfig?.Key);
        Assert.Equal("hash", export.ShardingConfig?.Strategy);
        Assert.Equal(128, export.ShardingConfig?.VirtualPerPhysical);

        // VectorConfig validation
        Assert.NotNull(export.VectorConfig);
        Assert.True(export.VectorConfig.ContainsKey("default"));

        var defaultVectorConfig = export.VectorConfig["default"];
        Assert.Equal("default", defaultVectorConfig.Name);
        Assert.Equal("hnsw", defaultVectorConfig.VectorIndexType);
        Assert.NotNull(defaultVectorConfig.Vectorizer);

        // VectorIndexConfig validation
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.Equal("hnsw", defaultVectorConfig.VectorIndexConfig.Type);
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);

        var config = defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW;

        // HNSW specific configuration assertions
        Assert.Equal(VectorIndexConfig.VectorDistance.Cosine, config?.Distance);
        Assert.Equal(8, config?.DynamicEfFactor);
        Assert.Equal(500, config?.DynamicEfMax);
        Assert.Equal(100, config?.DynamicEfMin);
        Assert.Equal(-1, config?.Ef);
        Assert.Equal(128, config?.EfConstruction);
        Assert.Equal(VectorIndexConfig.VectorIndexFilterStrategy.Sweeping, config?.FilterStrategy);
        Assert.Equal(40000, config?.FlatSearchCutoff);
        Assert.Equal(32, config?.MaxConnections);
        Assert.Equal(300, config?.CleanupIntervalSeconds);
        Assert.False(config?.Skip);
        Assert.Equal(1000000000000L, config?.VectorCacheMaxObjects);

        // TODO: Binary Quantization (bq) validation
        // Assert.NotNull(config?.bq);
        // Assert.False(config?.bq.enabled);

        // TODO: Product Quantization (pq) validation
        // Assert.NotNull(config?.pq);
        // Assert.False(config?.pq.enabled);
        // Assert.False(config?.pq.bitCompression);
        // Assert.Equal(256, config?.pq.centroids);
        // Assert.Equal(0, config?.pq.segments);
        // Assert.Equal(100000, config?.pq.trainingLimit);
        // Assert.NotNull(config?.pq.encoder);
        // Assert.Equal("log-normal", config?.pq.encoder.distribution);
        // Assert.Equal("kmeans", config?.pq.encoder.type);

        // TODO: Scalar Quantization (sq) validation
        // Assert.NotNull(config?.sq);
        // Assert.False(config?.sq.enabled);
        // Assert.Equal(20, config?.sq.rescoreLimit);
        // Assert.Equal(100000, config?.sq.trainingLimit);

        // TODO: Multivector validation
        // Assert.NotNull(config?.multivector);
        // Assert.False(config?.multivector.enabled);
        // Assert.Equal("maxSim", config?.multivector.aggregation);

        // Available from v1.31
        // Assert.NotNull(config?.multivector.muvera);
        // Assert.False(config?.multivector.muvera.enabled);
        // Assert.Equal(16, config?.multivector.muvera.dprojections);
        // Assert.Equal(4, config?.multivector.muvera.ksim);
        // Assert.Equal(10, config?.multivector.muvera.repetitions);

        // Obsolete properties should be null/empty for new VectorConfig usage
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.Null(export.VectorIndexConfig);
        Assert.Null(export.VectorIndexType);
        Assert.Equal("", export.Vectorizer);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public async Task Test_Collections_Export_NonDefaultValues_Sharding()
    {
        var collection = await CollectionFactory(
            name: "MyOwnSuffixNonDefault",
            description: "My own description too",
            properties: [Property.Text("Name"), Property.Int("SomeNumber")],
            references: null,
            collectionNamePartSeparator: "",
            vectorConfig: new VectorConfig(
                "nondefault",
                new Vectorizer.Text2VecContextionary() { VectorizeClassName = false }
            ),
            invertedIndexConfig: new()
            {
                Bm25 = new() { B = 0.70f, K1 = 1.3f },
                CleanupIntervalSeconds = 30,
                IndexNullState = true,
                IndexPropertyLength = true,
                IndexTimestamps = true,
                Stopwords = new()
                {
                    Preset = "none",
                    Additions = ["plus"],
                    Removals = ["minus"],
                },
            },
            replicationConfig: new()
            {
                DeletionStrategy = DeletionStrategy.TimeBasedResolution,
                AsyncEnabled = true,
                Factor = 1,
            },
            shardingConfig: new()
            {
                ActualCount = 1,
                ActualVirtualCount = 136,
                DesiredCount = 1,
                DesiredVirtualCount = 136,
                VirtualPerPhysical = 136,
            }
        );

        var export = await _weaviate.Collections.Export(collection.Name);

        // Basic collection properties
        Assert.NotNull(export);
        Assert.EndsWith("MyOwnSuffixNonDefault", export.Name);
        Assert.Equal("My own description too", export.Description);

        // Properties validation
        Assert.NotNull(export.Properties);
        Assert.Equal(2, export.Properties.Count);
        var property = export.Properties.First();
        Assert.Equal("name", property.Name);
        Assert.Contains("text", property.DataType);

        // InvertedIndexConfig validation
        Assert.NotNull(export.InvertedIndexConfig);
        Assert.NotNull(export.InvertedIndexConfig.Bm25);
        Assert.Equal(0.70f, export.InvertedIndexConfig.Bm25.B);
        Assert.Equal(1.3f, export.InvertedIndexConfig.Bm25.K1);
        Assert.Equal(30, export.InvertedIndexConfig.CleanupIntervalSeconds);
        Assert.True(export.InvertedIndexConfig.IndexNullState);
        Assert.True(export.InvertedIndexConfig.IndexPropertyLength);
        Assert.True(export.InvertedIndexConfig.IndexTimestamps);

        // Stopwords validation
        Assert.NotNull(export.InvertedIndexConfig.Stopwords);
        Assert.Equal("none", export.InvertedIndexConfig.Stopwords.Preset);
        Assert.NotEmpty(export.InvertedIndexConfig.Stopwords.Additions);
        Assert.NotEmpty(export.InvertedIndexConfig.Stopwords.Removals);

        // Module config should be null for base Collection
        Assert.Null(export.ModuleConfig);

        // MultiTenancyConfig validation
        Assert.NotNull(export.MultiTenancyConfig);
        Assert.False(export.MultiTenancyConfig.AutoTenantActivation);
        Assert.False(export.MultiTenancyConfig.AutoTenantCreation);
        Assert.False(export.MultiTenancyConfig.Enabled);

        // ReplicationConfig validation
        Assert.NotNull(export.ReplicationConfig);
        Assert.True(export.ReplicationConfig.AsyncEnabled);
        Assert.Equal(
            DeletionStrategy.TimeBasedResolution,
            export.ReplicationConfig.DeletionStrategy
        );
        Assert.Equal(1, export.ReplicationConfig.Factor);

        // ShardingConfig validation
        Assert.NotNull(export.ShardingConfig);
        Assert.Equal(1, export.ShardingConfig?.ActualCount);
        Assert.Equal(136, export.ShardingConfig?.ActualVirtualCount);
        Assert.Equal(1, export.ShardingConfig?.DesiredCount);
        Assert.Equal(136, export.ShardingConfig?.DesiredVirtualCount);
        Assert.Equal("murmur3", export.ShardingConfig?.Function);
        Assert.Equal("_id", export.ShardingConfig?.Key);
        Assert.Equal("hash", export.ShardingConfig?.Strategy);
        Assert.Equal(136, export.ShardingConfig?.VirtualPerPhysical);

        // VectorConfig validation
        Assert.NotNull(export.VectorConfig);
        Assert.True(export.VectorConfig.ContainsKey("nondefault"));

        var defaultVectorConfig = export.VectorConfig["nondefault"];
        Assert.Equal("nondefault", defaultVectorConfig.Name);
        Assert.NotNull(defaultVectorConfig.Vectorizer);
        Assert.Equal("text2vec-contextionary", defaultVectorConfig.Vectorizer.Identifier);

        // VectorIndexConfig validation
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.Equal("hnsw", defaultVectorConfig.VectorIndexConfig.Type);
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);

        var config = defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW;

        // HNSW specific configuration assertions
        Assert.Equal(VectorIndexConfig.VectorDistance.Cosine, config?.Distance);
        Assert.Equal(8, config?.DynamicEfFactor);
        Assert.Equal(500, config?.DynamicEfMax);
        Assert.Equal(100, config?.DynamicEfMin);
        Assert.Equal(-1, config?.Ef);
        Assert.Equal(128, config?.EfConstruction);
        Assert.Equal(VectorIndexConfig.VectorIndexFilterStrategy.Sweeping, config?.FilterStrategy);
        Assert.Equal(40000, config?.FlatSearchCutoff);
        Assert.Equal(32, config?.MaxConnections);
        Assert.Equal(300, config?.CleanupIntervalSeconds);
        Assert.False(config?.Skip);
        Assert.Equal(1000000000000L, config?.VectorCacheMaxObjects);

        // TODO: Binary Quantization (bq) validation
        // Assert.NotNull(config?.bq);
        // Assert.False(config?.bq.enabled);

        // TODO: Product Quantization (pq) validation
        // Assert.NotNull(config?.pq);
        // Assert.False(config?.pq.enabled);
        // Assert.False(config?.pq.bitCompression);
        // Assert.Equal(256, config?.pq.centroids);
        // Assert.Equal(0, config?.pq.segments);
        // Assert.Equal(100000, config?.pq.trainingLimit);
        // Assert.NotNull(config?.pq.encoder);
        // Assert.Equal("log-normal", config?.pq.encoder.distribution);
        // Assert.Equal("kmeans", config?.pq.encoder.type);

        // TODO: Scalar Quantization (sq) validation
        // Assert.NotNull(config?.sq);
        // Assert.False(config?.sq.enabled);
        // Assert.Equal(20, config?.sq.rescoreLimit);
        // Assert.Equal(100000, config?.sq.trainingLimit);

        // TODO: Multivector validation
        // Assert.NotNull(config?.multivector);
        // Assert.False(config?.multivector.enabled);
        // Assert.Equal("maxSim", config?.multivector.aggregation);

        // Available from v1.31
        // Assert.NotNull(config?.multivector.muvera);
        // Assert.False(config?.multivector.muvera.enabled);
        // Assert.Equal(16, config?.multivector.muvera.dprojections);
        // Assert.Equal(4, config?.multivector.muvera.ksim);
        // Assert.Equal(10, config?.multivector.muvera.repetitions);

        // Obsolete properties should be null/empty for new VectorConfig usage
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.Null(export.VectorIndexConfig);
        Assert.Null(export.VectorIndexType);
        Assert.Equal("", export.Vectorizer);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public async Task Test_Collections_Export_NonDefaultValues_MultiTenacy()
    {
        var collection = await CollectionFactory(
            name: "MyOwnSuffixNonDefault",
            description: "My own description too",
            properties: [Property.Text("Name"), Property.Int("SomeNumber")],
            references: null,
            collectionNamePartSeparator: "",
            vectorConfig: new VectorConfig(
                "nondefault",
                new Vectorizer.Text2VecContextionary() { VectorizeClassName = false }
            ),
            multiTenancyConfig: new()
            {
                AutoTenantActivation = true,
                AutoTenantCreation = true,
                Enabled = true,
            },
            invertedIndexConfig: new()
            {
                Bm25 = new() { B = 0.70f, K1 = 1.3f },
                CleanupIntervalSeconds = 30,
                IndexNullState = true,
                IndexPropertyLength = true,
                IndexTimestamps = true,
                Stopwords = new()
                {
                    Preset = "none",
                    Additions = ["plus"],
                    Removals = ["minus"],
                },
            },
            replicationConfig: new()
            {
                DeletionStrategy = DeletionStrategy.TimeBasedResolution,
                AsyncEnabled = true,
                Factor = 1,
            }
        );

        var export = await _weaviate.Collections.Export(collection.Name);

        // Basic collection properties
        Assert.NotNull(export);
        Assert.EndsWith("MyOwnSuffixNonDefault", export.Name);
        Assert.Equal("My own description too", export.Description);

        // Properties validation
        Assert.NotNull(export.Properties);
        Assert.Equal(2, export.Properties.Count);
        var property = export.Properties.First();
        Assert.Equal("name", property.Name);
        Assert.Contains("text", property.DataType);

        // InvertedIndexConfig validation
        Assert.NotNull(export.InvertedIndexConfig);
        Assert.NotNull(export.InvertedIndexConfig.Bm25);
        Assert.Equal(0.70f, export.InvertedIndexConfig.Bm25.B);
        Assert.Equal(1.3f, export.InvertedIndexConfig.Bm25.K1);
        Assert.Equal(30, export.InvertedIndexConfig.CleanupIntervalSeconds);
        Assert.True(export.InvertedIndexConfig.IndexNullState);
        Assert.True(export.InvertedIndexConfig.IndexPropertyLength);
        Assert.True(export.InvertedIndexConfig.IndexTimestamps);

        // Stopwords validation
        Assert.NotNull(export.InvertedIndexConfig.Stopwords);
        Assert.Equal("none", export.InvertedIndexConfig.Stopwords.Preset);
        Assert.NotEmpty(export.InvertedIndexConfig.Stopwords.Additions);
        Assert.NotEmpty(export.InvertedIndexConfig.Stopwords.Removals);

        // Module config should be null for base Collection
        Assert.Null(export.ModuleConfig);

        // MultiTenancyConfig validation
        Assert.NotNull(export.MultiTenancyConfig);
        Assert.True(export.MultiTenancyConfig.AutoTenantActivation);
        Assert.True(export.MultiTenancyConfig.AutoTenantCreation);
        Assert.True(export.MultiTenancyConfig.Enabled);

        // ReplicationConfig validation
        Assert.NotNull(export.ReplicationConfig);
        Assert.True(export.ReplicationConfig.AsyncEnabled);
        Assert.Equal(
            DeletionStrategy.TimeBasedResolution,
            export.ReplicationConfig.DeletionStrategy
        );
        Assert.Equal(1, export.ReplicationConfig.Factor);

        // ShardingConfig validation
        Assert.NotNull(export.ShardingConfig);
        Assert.Equal(0, export.ShardingConfig?.ActualCount);
        Assert.Equal(0, export.ShardingConfig?.ActualVirtualCount);
        Assert.Equal(0, export.ShardingConfig?.DesiredCount);
        Assert.Equal(0, export.ShardingConfig?.DesiredVirtualCount);
        Assert.Equal("", export.ShardingConfig?.Function);
        Assert.Equal("", export.ShardingConfig?.Key);
        Assert.Equal("", export.ShardingConfig?.Strategy);
        Assert.Equal(0, export.ShardingConfig?.VirtualPerPhysical);

        // VectorConfig validation
        Assert.NotNull(export.VectorConfig);
        Assert.True(export.VectorConfig.ContainsKey("nondefault"));

        var defaultVectorConfig = export.VectorConfig["nondefault"];
        Assert.Equal("nondefault", defaultVectorConfig.Name);
        Assert.NotNull(defaultVectorConfig.Vectorizer);
        Assert.Equal("text2vec-contextionary", defaultVectorConfig.Vectorizer.Identifier);

        // VectorIndexConfig validation
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.Equal("hnsw", defaultVectorConfig.VectorIndexConfig.Type);
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW);

        var config = defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW;

        // HNSW specific configuration assertions
        Assert.Equal(VectorIndexConfig.VectorDistance.Cosine, config?.Distance);
        Assert.Equal(8, config?.DynamicEfFactor);
        Assert.Equal(500, config?.DynamicEfMax);
        Assert.Equal(100, config?.DynamicEfMin);
        Assert.Equal(-1, config?.Ef);
        Assert.Equal(128, config?.EfConstruction);
        Assert.Equal(VectorIndexConfig.VectorIndexFilterStrategy.Sweeping, config?.FilterStrategy);
        Assert.Equal(40000, config?.FlatSearchCutoff);
        Assert.Equal(32, config?.MaxConnections);
        Assert.Equal(300, config?.CleanupIntervalSeconds);
        Assert.False(config?.Skip);
        Assert.Equal(1000000000000L, config?.VectorCacheMaxObjects);

        // TODO: Binary Quantization (bq) validation
        // Assert.NotNull(config?.bq);
        // Assert.False(config?.bq.enabled);

        // TODO: Product Quantization (pq) validation
        // Assert.NotNull(config?.pq);
        // Assert.False(config?.pq.enabled);
        // Assert.False(config?.pq.bitCompression);
        // Assert.Equal(256, config?.pq.centroids);
        // Assert.Equal(0, config?.pq.segments);
        // Assert.Equal(100000, config?.pq.trainingLimit);
        // Assert.NotNull(config?.pq.encoder);
        // Assert.Equal("log-normal", config?.pq.encoder.distribution);
        // Assert.Equal("kmeans", config?.pq.encoder.type);

        // TODO: Scalar Quantization (sq) validation
        // Assert.NotNull(config?.sq);
        // Assert.False(config?.sq.enabled);
        // Assert.Equal(20, config?.sq.rescoreLimit);
        // Assert.Equal(100000, config?.sq.trainingLimit);

        // TODO: Multivector validation
        // Assert.NotNull(config?.multivector);
        // Assert.False(config?.multivector.enabled);
        // Assert.Equal("maxSim", config?.multivector.aggregation);

        // Available from v1.31
        // Assert.NotNull(config?.multivector.muvera);
        // Assert.False(config?.multivector.muvera.enabled);
        // Assert.Equal(16, config?.multivector.muvera.dprojections);
        // Assert.Equal(4, config?.multivector.muvera.ksim);
        // Assert.Equal(10, config?.multivector.muvera.repetitions);

        // Obsolete properties should be null/empty for new VectorConfig usage
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.Null(export.VectorIndexConfig);
        Assert.Null(export.VectorIndexType);
        Assert.Equal("", export.Vectorizer);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}

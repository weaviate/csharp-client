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
                vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
            ),
            await CollectionFactory(
                name: "Collection2",
                properties: [Property.Text("Lastname")],
                vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
            ),
            await CollectionFactory(
                name: "Collection3",
                properties: [Property.Text("Address")],
                vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
            ),
        };

        var collectionNames = collection.Select(c => c.Name).ToHashSet();

        // Act
        var list = await _weaviate
            .Collections.List()
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.ProperSubset(list.Select(l => l.Name).ToHashSet(), collectionNames);
    }

    [Fact]
    public async Task Test_Collections_Exists()
    {
        var collection = await CollectionFactory(
            properties: [Property.Text("Name")],
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
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
            vectorConfig: Vector.Name("default").With(new VectorizerConfig.None())
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
        Assert.Equal(1, ((dynamic?)export.ShardingConfig)?.actualCount);
        Assert.Equal(128, export.ShardingConfig?.actualVirtualCount);
        Assert.Equal(1, export.ShardingConfig?.desiredCount);
        Assert.Equal(128, export.ShardingConfig?.desiredVirtualCount);
        Assert.Equal("murmur3", export.ShardingConfig?.function);
        Assert.Equal("_id", export.ShardingConfig?.key);
        Assert.Equal("hash", export.ShardingConfig?.strategy);
        Assert.Equal(128, export.ShardingConfig?.virtualPerPhysical);

        // VectorConfig validation
        Assert.NotNull(export.VectorConfig);
        Assert.True(export.VectorConfig.ContainsKey("default"));

        var defaultVectorConfig = export.VectorConfig["default"];
        Assert.Equal("default", defaultVectorConfig.Name);
        Assert.Equal("hnsw", defaultVectorConfig.VectorIndexType);
        Assert.NotNull(defaultVectorConfig.Vectorizer);

        // VectorIndexConfig validation
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.Equal("hnsw", defaultVectorConfig.VectorIndexConfig.Identifier);
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig.Configuration);

        var config = defaultVectorConfig.VectorIndexConfig.Configuration;

        // HNSW specific configuration assertions
        Assert.Equal("cosine", config?.distance);
        Assert.Equal(8, config?.dynamicEfFactor);
        Assert.Equal(500, config?.dynamicEfMax);
        Assert.Equal(100, config?.dynamicEfMin);
        Assert.Equal(-1, config?.ef);
        Assert.Equal(128, config?.efConstruction);
        Assert.Equal("sweeping", config?.filterStrategy);
        Assert.Equal(40000, config?.flatSearchCutoff);
        Assert.Equal(32, config?.maxConnections);
        Assert.Equal(300, config?.cleanupIntervalSeconds);
        Assert.False(config?.skip);
        Assert.Equal(1000000000000L, config?.vectorCacheMaxObjects);

        // Binary Quantization (bq) validation
        Assert.NotNull(config?.bq);
        Assert.False(config?.bq.enabled);

        // Product Quantization (pq) validation
        Assert.NotNull(config?.pq);
        Assert.False(config?.pq.enabled);
        Assert.False(config?.pq.bitCompression);
        Assert.Equal(256, config?.pq.centroids);
        Assert.Equal(0, config?.pq.segments);
        Assert.Equal(100000, config?.pq.trainingLimit);
        Assert.NotNull(config?.pq.encoder);
        Assert.Equal("log-normal", config?.pq.encoder.distribution);
        Assert.Equal("kmeans", config?.pq.encoder.type);

        // Scalar Quantization (sq) validation
        Assert.NotNull(config?.sq);
        Assert.False(config?.sq.enabled);
        Assert.Equal(20, config?.sq.rescoreLimit);
        Assert.Equal(100000, config?.sq.trainingLimit);

        // Multivector validation
        Assert.NotNull(config?.multivector);
        Assert.False(config?.multivector.enabled);
        Assert.Equal("maxSim", config?.multivector.aggregation);

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

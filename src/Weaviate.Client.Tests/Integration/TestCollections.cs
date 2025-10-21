using Weaviate.Client.Models;
using Xunit.Runner.Common;

namespace Weaviate.Client.Tests.Integration;

[Collection("TestCollections")]
public partial class CollectionsTests : IntegrationTests
{
    [Fact]
    public async Task CollectionClient_Creates_And_Retrieves_Collection()
    {
        // Arrange
        var collectionName = "RandomCollectionName";

        // Act
        var collectionClient = await CollectionFactory(
            collectionName,
            "Test collection description",
            [Property.Text("Name")]
        );

        // Assert
        var collection = _weaviate.Collections.Use(collectionClient.Name);
        var config = await collection.Config.Get();
        Assert.NotNull(config);
        Assert.Equal(
            $"CollectionClient_Creates_And_Retrieves_Collection_{TestContext.Current.Test?.UniqueID}_Object_RandomCollectionName",
            config.Name
        );
        Assert.Equal("Test collection description", config.Description);
    }

    [Fact]
    public async Task Test_Collections_List()
    {
        // Arrange
        var collection = new[]
        {
            await CollectionFactory(
                name: "Collection1",
                properties: [Property.Text("Name")],
                vectorConfig: Configure.Vectors.SelfProvided()
            ),
            await CollectionFactory(
                name: "Collection2",
                properties: [Property.Text("Lastname")],
                vectorConfig: Configure.Vectors.SelfProvided()
            ),
            await CollectionFactory(
                name: "Collection3",
                properties: [Property.Text("Address")],
                vectorConfig: Configure.Vectors.SelfProvided()
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
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
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

    [Theory]
    [ClassData(typeof(DatasetCollectionCreateAndExport))]
    public async Task Test_Collections_Export_Cases(string key)
    {
        var c = DatasetCollectionCreateAndExport.Cases[key];

        c.Name = MakeUniqueCollectionName<dynamic>(c.Name);

        var client = await CollectionFactory<dynamic>(c);

        var export = await _weaviate.Collections.Export(client.Name);

        Assert.Equal(c, export);
    }

    [Fact]
    public async Task Collection_Creates_And_Retrieves_Reranker_Config()
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            properties: [Property.Text("Name")],
            rerankerConfig: new Reranker.Custom
            {
                Type = "reranker-dummy",
                Config = new { ConfigOption = "ConfigValue" },
            }
        );

        // Act
        var collection = await _weaviate
            .Collections.Use<dynamic>(collectionClient.Name)
            .Config.Get();

        // Assert
        Assert.NotNull(collection);
        Assert.NotNull(collection.RerankerConfig);
        Assert.IsType<Reranker.Custom>(collection.RerankerConfig);
    }

    [Fact]
    public async Task Collection_Creates_And_Retrieves_Generative_Config()
    {
        // Arrange
        var collectionClient = await CollectionFactory(
            properties: [Property.Text("Name")],
            generativeConfig: new GenerativeConfig.Custom
            {
                Type = "generative-dummy",
                Config = new { ConfigOption = "ConfigValue" },
            }
        );

        // Act
        var collection = await _weaviate
            .Collections.Use<dynamic>(collectionClient.Name)
            .Config.Get();

        // Assert
        Assert.NotNull(collection);
        Assert.NotNull(collection.GenerativeConfig);
        Assert.IsType<GenerativeConfig.Custom>(collection.GenerativeConfig);
    }

    [Fact]
    public async Task Test_Collections_Export()
    {
        var collection = await CollectionFactory(
            name: "MyOwnSuffix",
            description: "My own description too",
            properties: [Property.Text("Name")],
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
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
        Assert.Equal(StopwordConfig.Presets.EN, export.InvertedIndexConfig.Stopwords.Preset);
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
        Assert.Equal(ShardingConfig.Functions.Murmur3, export.ShardingConfig?.Function);
        Assert.Equal("_id", export.ShardingConfig?.Key);
        Assert.Equal(ShardingConfig.Strategies.Hash, export.ShardingConfig?.Strategy);
        Assert.Equal(128, export.ShardingConfig?.VirtualPerPhysical);

        // VectorConfig validation
        Assert.NotNull(export.VectorConfig);
        Assert.True(export.VectorConfig.ContainsKey("default"));

        var defaultVectorConfig = export.VectorConfig["default"];
        Assert.Equal("default", defaultVectorConfig.Name);
        Assert.Equal(VectorIndex.HNSW.TypeValue, defaultVectorConfig.VectorIndexType);
        Assert.NotNull(defaultVectorConfig.Vectorizer);

        // VectorIndexConfig validation
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.Equal(VectorIndex.HNSW.TypeValue, defaultVectorConfig.VectorIndexConfig.Type);
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
                    Preset = StopwordConfig.Presets.None,
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
        Assert.Equal(2, export.Properties.Length);
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
        Assert.Equal(StopwordConfig.Presets.None, export.InvertedIndexConfig.Stopwords.Preset);
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
        Assert.Equal(ShardingConfig.Functions.Murmur3, export.ShardingConfig?.Function);
        Assert.Equal("_id", export.ShardingConfig?.Key);
        Assert.Equal(ShardingConfig.Strategies.Hash, export.ShardingConfig?.Strategy);
        Assert.Equal(136, export.ShardingConfig?.VirtualPerPhysical);

        // VectorConfig validation
        Assert.NotNull(export.VectorConfig);
        Assert.True(export.VectorConfig.ContainsKey("nondefault"));

        var defaultVectorConfig = export.VectorConfig["nondefault"];
        Assert.Equal("nondefault", defaultVectorConfig.Name);
        Assert.NotNull(defaultVectorConfig.Vectorizer);
        Assert.Equal(
            Vectorizer.Text2VecContextionary.IdentifierValue,
            defaultVectorConfig.Vectorizer.Identifier
        );

        // VectorIndexConfig validation
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.Equal(VectorIndex.HNSW.TypeValue, defaultVectorConfig.VectorIndexConfig.Type);
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);

        var config = defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW;

        // HNSW specific configuration assertions
        Assert.Equal(VectorIndexConfig.VectorDistance.Cosine, config?.Distance);
        Assert.Equal(8, config?.DynamicEfFactor);
        Assert.Equal(500, config?.DynamicEfMax);
        Assert.Equal(100, config?.DynamicEfMin);
        Assert.Equal(-1, config?.Ef);
        Assert.Equal(128, config?.EfConstruction);
        if (ServerVersionIsInRange("0.0.0", "1.33.0"))
        {
            Assert.Equal(
                VectorIndexConfig.VectorIndexFilterStrategy.Sweeping,
                config?.FilterStrategy
            );
        }
        else
        {
            Assert.Equal(VectorIndexConfig.VectorIndexFilterStrategy.Acorn, config?.FilterStrategy);
        }
        Assert.Equal(40000, config?.FlatSearchCutoff);
        Assert.Equal(32, config?.MaxConnections);
        Assert.Equal(300, config?.CleanupIntervalSeconds);
        Assert.False(config?.Skip);
        Assert.Equal(1000000000000L, config?.VectorCacheMaxObjects);

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
                    Preset = StopwordConfig.Presets.None,
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
        Assert.Equal(2, export.Properties.Length);
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
        Assert.Equal(StopwordConfig.Presets.None, export.InvertedIndexConfig.Stopwords.Preset);
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
        Assert.Equal(ShardingConfig.Functions.None, export.ShardingConfig?.Function);
        Assert.Equal("", export.ShardingConfig?.Key);
        Assert.Equal(ShardingConfig.Strategies.None, export.ShardingConfig?.Strategy);
        Assert.Equal(0, export.ShardingConfig?.VirtualPerPhysical);

        // VectorConfig validation
        Assert.NotNull(export.VectorConfig);
        Assert.True(export.VectorConfig.ContainsKey("nondefault"));

        var defaultVectorConfig = export.VectorConfig["nondefault"];
        Assert.Equal("nondefault", defaultVectorConfig.Name);
        Assert.NotNull(defaultVectorConfig.Vectorizer);
        Assert.Equal(
            Vectorizer.Text2VecContextionary.IdentifierValue,
            defaultVectorConfig.Vectorizer.Identifier
        );

        // VectorIndexConfig validation
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.Equal(VectorIndex.HNSW.TypeValue, defaultVectorConfig.VectorIndexConfig.Type);
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW);

        var config = defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW;

        // HNSW specific configuration assertions
        Assert.Equal(VectorIndexConfig.VectorDistance.Cosine, config?.Distance);
        Assert.Equal(8, config?.DynamicEfFactor);
        Assert.Equal(500, config?.DynamicEfMax);
        Assert.Equal(100, config?.DynamicEfMin);
        Assert.Equal(-1, config?.Ef);
        Assert.Equal(128, config?.EfConstruction);
        if (ServerVersionIsInRange("0.0.0", "1.33.0"))
        {
            Assert.Equal(
                VectorIndexConfig.VectorIndexFilterStrategy.Sweeping,
                config?.FilterStrategy
            );
        }
        else
        {
            Assert.Equal(VectorIndexConfig.VectorIndexFilterStrategy.Acorn, config?.FilterStrategy);
        }
        Assert.Equal(40000, config?.FlatSearchCutoff);
        Assert.Equal(32, config?.MaxConnections);
        Assert.Equal(300, config?.CleanupIntervalSeconds);
        Assert.False(config?.Skip);
        Assert.Equal(1000000000000L, config?.VectorCacheMaxObjects);

        // Obsolete properties should be null/empty for new VectorConfig usage
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.Null(export.VectorIndexConfig);
        Assert.Null(export.VectorIndexType);
        Assert.Equal("", export.Vectorizer);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public async Task Test_Collection_Config_Add_Vector()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "Test",
            vectorConfig: Configure.Vectors.SelfProvided("default"),
            properties: [Property.Text("name")]
        );

        RequireVersion("1.31.0");

        await collection.Config.AddVector(
            Configure.Vectors.Text2VecContextionary().New("nondefault")
        );

        var c = await collection.Config.Get();

        Assert.NotNull(c);
        Assert.Equal(2, c.VectorConfig.Count);
    }

    public static IEnumerable<object?> GenerativeConfigData()
    {
        yield return null;
        yield return new GenerativeConfig.Anyscale();
    }

    public static IEnumerable<object?> VectorizerConfigData()
    {
        yield return null;
        yield return Configure.Vectors.SelfProvided();
        yield return Configure
            .Vectors.Text2VecContextionary(vectorizeCollectionName: false)
            .New("vec");
        yield return new[]
        {
            Configure
                .Vectors.Text2VecContextionary(vectorizeCollectionName: false)
                .New(name: "vec"),
        };
    }

    public static IEnumerable<object?[]> AddPropertyTestData()
    {
        foreach (var generativeConfig in GenerativeConfigData())
        {
            foreach (var vectorizerConfig in VectorizerConfigData())
            {
                yield return new object?[] { generativeConfig, vectorizerConfig };
            }
        }
    }

    [Theory]
    [MemberData(nameof(AddPropertyTestData))]
    public async Task Test_Config_Add_Property(
        IGenerativeConfig? generativeConfig,
        VectorConfigList? vectorizerConfig
    )
    {
        // Arrange
        var collection = await CollectionFactory(
            properties: [Property.Text("title")],
            generativeConfig: generativeConfig,
            vectorConfig: vectorizerConfig
        );

        await collection.Config.AddProperty(Property.Text("description"));

        var config = await collection.Config.Get();

        Assert.NotNull(config);
        Assert.Contains(config.Properties, p => p.Name == "description");
    }

    [Fact]
    public async Task Test_Collection_Config_Update()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "TestCollectionUpdate",
            vectorConfig: Configure.Vectors.SelfProvided(),
            properties: [Property.Text("name"), Property.Int("age")],
            multiTenancyConfig: new()
            {
                Enabled = true,
                AutoTenantCreation = false,
                AutoTenantActivation = false,
            }
        );

        // Act & Assert - Initial state
        CollectionConfig config = (await collection.Config.Get())!;

        Assert.Equal(1, config.ReplicationConfig!.Factor);
        Assert.False(config.ReplicationConfig.AsyncEnabled);
        Assert.True(config.MultiTenancyConfig!.Enabled);
        Assert.False(config.MultiTenancyConfig!.AutoTenantActivation);
        Assert.False(config.MultiTenancyConfig!.AutoTenantCreation);

        // Vector config assertions
        Assert.NotNull(config.VectorConfig);
        Assert.True(config.VectorConfig.ContainsKey("default"));
        var defaultVectorConfig = config.VectorConfig["default"];
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.IsType<VectorIndex.HNSW>(defaultVectorConfig.VectorIndexConfig);
        var hnswConfig = defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW;

        if (ServerVersionIsInRange("0.0.0", "1.33.0"))
        {
            Assert.Equal(
                VectorIndexConfig.VectorIndexFilterStrategy.Sweeping,
                hnswConfig?.FilterStrategy
            );
        }
        else
        {
            Assert.Equal(
                VectorIndexConfig.VectorIndexFilterStrategy.Acorn,
                hnswConfig?.FilterStrategy
            );
        }

        // Act - Update configuration
        await collection.Config.Update(c =>
        {
            c.Description = "Test";
            c.InvertedIndexConfig.Bm25.B = 0.8f;
            c.InvertedIndexConfig.Bm25.K1 = 1.25f;
            c.InvertedIndexConfig.CleanupIntervalSeconds = 10;
            c.InvertedIndexConfig.Stopwords.Preset = StopwordConfig.Presets.EN;
            c.InvertedIndexConfig.Stopwords.Additions = ["a"];
            c.InvertedIndexConfig.Stopwords.Removals = ["the"];

            c.ReplicationConfig.Factor = 2;
            c.ReplicationConfig.AsyncEnabled = true;
            c.ReplicationConfig.DeletionStrategy = DeletionStrategy.DeleteOnConflict;

            var vc = c.VectorConfig["default"];
            vc.VectorIndexConfig.UpdateHNSW(vic =>
            {
                vic.DynamicEfFactor = 8;
                vic.DynamicEfMax = 500;
                vic.DynamicEfMin = 100;
                vic.Ef = -1;
                vic.FilterStrategy = VectorIndexConfig.VectorIndexFilterStrategy.Acorn;
                vic.FlatSearchCutoff = 40000;
                vic.Quantizer = new VectorIndex.Quantizers.PQ
                {
                    Centroids = 128,
                    Encoder = new VectorIndex.Quantizers.PQ.EncoderConfig
                    {
                        Type = VectorIndex.Quantizers.EncoderType.Tile,
                        Distribution = VectorIndex.Quantizers.DistributionType.Normal,
                    },
                    Segments = 4,
                    TrainingLimit = 100001,
                };
                vic.VectorCacheMaxObjects = 2000000;
            });

            c.MultiTenancyConfig.AutoTenantCreation = true;
            c.MultiTenancyConfig.AutoTenantActivation = true;
        });

        // Assert - After first update
        config = (await collection.Config.Get())!;

        // Description assertion with version check
        if (ServerVersionIsInRange("1.25.2") || !ServerVersionIsInRange("1.25.0"))
        {
            Assert.Equal("Test", config.Description);
        }
        else
        {
            Assert.Null(config.Description);
        }

        // Inverted index config assertions
        Assert.NotNull(config.InvertedIndexConfig);
        Assert.NotNull(config.InvertedIndexConfig.Bm25);
        Assert.Equal(0.8f, config.InvertedIndexConfig.Bm25.B);
        Assert.Equal(1.25f, config.InvertedIndexConfig.Bm25.K1);
        Assert.Equal(10, config.InvertedIndexConfig.CleanupIntervalSeconds);

        // Stopwords assertions
        Assert.NotNull(config.InvertedIndexConfig.Stopwords);
        // Assert.Equal(["a"], config.InvertedIndexConfig.Stopwords.Additions); // potential weaviate bug, this returns as None
        Assert.Equal(["the"], config.InvertedIndexConfig.Stopwords.Removals);

        // Replication config assertions
        Assert.NotNull(config.ReplicationConfig);
        Assert.Equal(2, config.ReplicationConfig.Factor);

        if (ServerVersionIsInRange("1.26.0"))
        {
            Assert.True(config.ReplicationConfig.AsyncEnabled);
        }
        else
        {
            Assert.False(config.ReplicationConfig.AsyncEnabled);
        }

        if (ServerVersionIsInRange("1.24.25"))
        {
            Assert.Equal(
                DeletionStrategy.DeleteOnConflict,
                config.ReplicationConfig.DeletionStrategy
            );
        }
        else
        {
            // default value if not present in schema
            Assert.Equal(
                DeletionStrategy.NoAutomatedResolution,
                config.ReplicationConfig.DeletionStrategy
            );
        }

        // Vector config assertions
        Assert.NotNull(config.VectorConfig);
        Assert.True(config.VectorConfig.ContainsKey("default"));
        defaultVectorConfig = config.VectorConfig["default"];
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.IsType<VectorIndex.HNSW>(defaultVectorConfig.VectorIndexConfig);

        hnswConfig = defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW;
        Assert.NotNull(hnswConfig);
        Assert.IsType<VectorIndex.Quantizers.PQ>(hnswConfig.Quantizer);
        var pqQuantizer = hnswConfig.Quantizer as VectorIndex.Quantizers.PQ;

        Assert.Equal(300, hnswConfig.CleanupIntervalSeconds);
        Assert.Equal(VectorIndexConfig.VectorDistance.Cosine, hnswConfig.Distance);
        Assert.Equal(8, hnswConfig.DynamicEfFactor);
        Assert.Equal(500, hnswConfig.DynamicEfMax);
        Assert.Equal(100, hnswConfig.DynamicEfMin);
        Assert.Equal(-1, hnswConfig.Ef);
        Assert.Equal(128, hnswConfig.EfConstruction);
        Assert.Equal(40000, hnswConfig.FlatSearchCutoff);

        if (ServerVersionIsInRange("0.0.0", "1.26.0"))
        {
            Assert.Equal(64, hnswConfig.MaxConnections);
        }
        else
        {
            Assert.Equal(32, hnswConfig.MaxConnections);
        }

        // PQ Quantizer assertions
        Assert.NotNull(pqQuantizer);
        Assert.False(pqQuantizer.BitCompression);
        Assert.Equal(128, pqQuantizer.Centroids);
        Assert.NotNull(pqQuantizer.Encoder);
        Assert.Equal(VectorIndex.Quantizers.EncoderType.Tile, pqQuantizer.Encoder.Type);
        Assert.Equal(
            VectorIndex.Quantizers.DistributionType.Normal,
            pqQuantizer.Encoder.Distribution
        );
        Assert.Equal(4, pqQuantizer.Segments);
        Assert.Equal(100001, pqQuantizer.TrainingLimit);
        Assert.False(hnswConfig.Skip);
        Assert.Equal(2000000, hnswConfig.VectorCacheMaxObjects);

        if (ServerVersionIsInRange("1.27.0"))
        {
            Assert.Equal(
                VectorIndexConfig.VectorIndexFilterStrategy.Acorn,
                hnswConfig.FilterStrategy
            );
        }
        else
        {
            // default value if not present in schema
            Assert.Equal(
                VectorIndexConfig.VectorIndexFilterStrategy.Sweeping,
                hnswConfig.FilterStrategy
            );
        }

        // Multi-tenancy config assertions
        Assert.NotNull(config.MultiTenancyConfig);
        Assert.True(config.MultiTenancyConfig.Enabled);

        if (ServerVersionIsInRange("1.25.2"))
        {
            Assert.True(config.MultiTenancyConfig.AutoTenantActivation);
        }
        else
        {
            Assert.False(config.MultiTenancyConfig.AutoTenantActivation);
        }

        if (ServerVersionIsInRange("1.25.1"))
        {
            Assert.True(config.MultiTenancyConfig.AutoTenantCreation);
        }
        else
        {
            Assert.False(config.MultiTenancyConfig.AutoTenantCreation);
        }

        // Act - Second update (disable quantizer and reset filter strategy)
        await collection.Config.Update(c =>
        {
            var vc = c.VectorConfig["default"];
            vc.VectorIndexConfig.UpdateHNSW(vic =>
            {
                vic.FilterStrategy = VectorIndexConfig.VectorIndexFilterStrategy.Sweeping;
                vic.Quantizer = null; // Disable quantizer
            });

            c.ReplicationConfig.DeletionStrategy = DeletionStrategy.NoAutomatedResolution;
        });

        // Assert - After second update
        config = (await collection.Config.Get())!;

        // Description should persist
        if (ServerVersionIsInRange("1.25.2") || !ServerVersionIsInRange("1.25.0"))
        {
            Assert.Equal("Test", config.Description);
        }
        else
        {
            Assert.Null(config.Description);
        }

        // Previous inverted index config should persist
        Assert.NotNull(config.InvertedIndexConfig);
        Assert.NotNull(config.InvertedIndexConfig.Bm25);
        Assert.Equal(0.8f, config.InvertedIndexConfig.Bm25.B);
        Assert.Equal(1.25f, config.InvertedIndexConfig.Bm25.K1);
        Assert.Equal(10, config.InvertedIndexConfig.CleanupIntervalSeconds);
        Assert.NotNull(config.InvertedIndexConfig.Stopwords);
        Assert.Equal(["the"], config.InvertedIndexConfig.Stopwords.Removals);

        // Replication config assertions
        Assert.NotNull(config.ReplicationConfig);
        Assert.Equal(2, config.ReplicationConfig.Factor);
        Assert.Equal(
            DeletionStrategy.NoAutomatedResolution,
            config.ReplicationConfig.DeletionStrategy
        );

        if (ServerVersionIsInRange("1.26.0"))
        {
            Assert.True(config.ReplicationConfig.AsyncEnabled);
        }
        else
        {
            Assert.False(config.ReplicationConfig.AsyncEnabled);
        }

        // Vector index config after quantizer disabled
        Assert.NotNull(config.VectorConfig);
        Assert.True(config.VectorConfig.ContainsKey("default"));
        defaultVectorConfig = config.VectorConfig["default"];
        Assert.NotNull(defaultVectorConfig.VectorIndexConfig);
        Assert.IsType<VectorIndex.HNSW>(defaultVectorConfig.VectorIndexConfig);

        hnswConfig = defaultVectorConfig.VectorIndexConfig as VectorIndex.HNSW;
        Assert.NotNull(hnswConfig);

        Assert.Equal(300, hnswConfig.CleanupIntervalSeconds);
        Assert.Equal(VectorIndexConfig.VectorDistance.Cosine, hnswConfig.Distance);
        Assert.Equal(8, hnswConfig.DynamicEfFactor);
        Assert.Equal(500, hnswConfig.DynamicEfMax);
        Assert.Equal(100, hnswConfig.DynamicEfMin);
        Assert.Equal(-1, hnswConfig.Ef);
        Assert.Equal(128, hnswConfig.EfConstruction);
        Assert.Equal(40000, hnswConfig.FlatSearchCutoff);

        if (!ServerVersionIsInRange("1.26.0"))
        {
            Assert.Equal(64, hnswConfig.MaxConnections);
        }
        else
        {
            Assert.Equal(32, hnswConfig.MaxConnections);
        }

        Assert.Null(hnswConfig.Quantizer); // Quantizer disabled
        Assert.False(hnswConfig.Skip);
        Assert.Equal(2000000, hnswConfig.VectorCacheMaxObjects);
        Assert.Equal(
            VectorIndexConfig.VectorIndexFilterStrategy.Sweeping,
            hnswConfig.FilterStrategy
        );
    }

    [Fact]
    public async Task Test_sq_and_rq()
    {
        if (!ServerVersionIsInRange("1.32.0"))
        {
            Assert.Skip("RQ only supported in server version 1.32.0+");
        }
        var collection = await CollectionFactory(
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(
                    name: "hnswSq",
                    indexConfig: new VectorIndex.HNSW
                    {
                        Quantizer = new VectorIndex.Quantizers.SQ
                        {
                            TrainingLimit = 100001,
                            RescoreLimit = 123,
                        },
                    }
                ),
                Configure.Vectors.SelfProvided(
                    name: "hnswRq",
                    indexConfig: new VectorIndex.HNSW
                    {
                        Quantizer = new VectorIndex.Quantizers.RQ { Bits = 8, RescoreLimit = 123 },
                    }
                ),
            }
        );
        var config = await collection.Config.Get();
        Assert.NotNull(config);
        var vcSQ = config.VectorConfig["hnswSq"];
        Assert.NotNull(vcSQ);
        var hnswConfig = vcSQ.VectorIndexConfig as VectorIndex.HNSW;
        Assert.NotNull(hnswConfig);
        var sqQuantizer = hnswConfig.Quantizer as VectorIndex.Quantizers.SQ;
        Assert.NotNull(sqQuantizer);
        Assert.Equal(100001, sqQuantizer.TrainingLimit);
        Assert.Equal(123, sqQuantizer.RescoreLimit);

        var vchnswRQ = config.VectorConfig["hnswRq"];
        Assert.NotNull(vchnswRQ);
        hnswConfig = vchnswRQ.VectorIndexConfig as VectorIndex.HNSW;
        Assert.NotNull(hnswConfig);
        var rqQuantizer = hnswConfig.Quantizer as VectorIndex.Quantizers.RQ;
        Assert.NotNull(rqQuantizer);
        Assert.Equal(8, rqQuantizer.Bits);
        Assert.Equal(123, rqQuantizer.RescoreLimit);

        await collection.Config.Update(c =>
        {
            var vc = c.VectorConfig["hnswSq"];
            vc.VectorIndexConfig.UpdateHNSW(vic =>
            {
                vic.FilterStrategy = VectorIndexConfig.VectorIndexFilterStrategy.Sweeping;
                vic.Quantizer = new VectorIndex.Quantizers.SQ
                {
                    TrainingLimit = 456,
                    RescoreLimit = 789,
                };
            });
        });

        await collection.Config.Update(c =>
        {
            var vc = c.VectorConfig["hnswRq"];
            vc.VectorIndexConfig.UpdateHNSW(vic =>
            {
                vic.Quantizer = new VectorIndex.Quantizers.RQ { RescoreLimit = 456 };
            });
        });

        config = await collection.Config.Get();
        Assert.NotNull(config);
        vcSQ = config.VectorConfig["hnswSq"];
        Assert.NotNull(vcSQ);
        hnswConfig = vcSQ.VectorIndexConfig as VectorIndex.HNSW;
        Assert.NotNull(hnswConfig);
        sqQuantizer = hnswConfig.Quantizer as VectorIndex.Quantizers.SQ;
        Assert.NotNull(sqQuantizer);
        Assert.Equal(456, sqQuantizer.TrainingLimit);
        Assert.Equal(789, sqQuantizer.RescoreLimit);

        var vcRQ = config.VectorConfig["hnswRq"];
        Assert.NotNull(vcRQ);
        hnswConfig = vcRQ.VectorIndexConfig as VectorIndex.HNSW;
        Assert.NotNull(hnswConfig);
        rqQuantizer = hnswConfig.Quantizer as VectorIndex.Quantizers.RQ;
        Assert.NotNull(rqQuantizer);
        Assert.Equal(8, rqQuantizer.Bits);
        Assert.Equal(456, rqQuantizer.RescoreLimit);
    }

    [Fact]
    public async Task Test_flat_rq()
    {
        if (!ServerVersionIsInRange("1.34.0"))
        {
            Assert.Skip("RQ with flat only supported in server version 1.34.0+");
        }
        var collection = await CollectionFactory(
            vectorConfig: new[]
            {
                Configure.Vectors.SelfProvided(
                    name: "flatRq",
                    indexConfig: new VectorIndex.Flat
                    {
                        Quantizer = new VectorIndex.Quantizers.RQ
                        {
                            Bits = 8,
                            RescoreLimit = 123,
                            Cache = true,
                        },
                    }
                ),
            }
        );
        var config = await collection.Config.Get();
        Assert.NotNull(config);
        var vcRQ = config.VectorConfig["flatRq"];
        Assert.NotNull(vcRQ);
        var flatConfig = vcRQ.VectorIndexConfig as VectorIndex.Flat;
        Assert.NotNull(flatConfig);
        var rqQuantizer = flatConfig.Quantizer as VectorIndex.Quantizers.RQ;
        Assert.NotNull(rqQuantizer);
        Assert.Equal(8, rqQuantizer.Bits);
        Assert.Equal(123, rqQuantizer.RescoreLimit);
        if (ServerVersionIsInRange("1.34.0"))
        {
            Assert.True(rqQuantizer.Cache);
        }

        await collection.Config.Update(c =>
        {
            var vc = c.VectorConfig["flatRq"];
            vc.VectorIndexConfig.UpdateFlat(vic =>
            {
                vic.Quantizer = new VectorIndex.Quantizers.RQ { RescoreLimit = 456 };
            });
        });

        config = await collection.Config.Get();
        Assert.NotNull(config);
        vcRQ = config.VectorConfig["flatRq"];
        Assert.NotNull(vcRQ);
        flatConfig = vcRQ.VectorIndexConfig as VectorIndex.Flat;
        Assert.NotNull(flatConfig);
        rqQuantizer = flatConfig.Quantizer as VectorIndex.Quantizers.RQ;
        Assert.NotNull(rqQuantizer);
        Assert.Equal(8, rqQuantizer.Bits);
        Assert.Equal(456, rqQuantizer.RescoreLimit);
    }

    [Fact]
    public async Task Test_Return_Blob_Property()
    {
        // Arrange
        var blobData = Weaviate.Client.Tests.Common.Constants.WeaviateLogoOldEncoded; // Should be a byte[] or base64 string
        var collection = await CollectionFactory(properties: [Property.Blob("blob")]);

        // Insert single object
        var uuid = await collection.Data.Insert(new { blob = blobData });

        // Insert many
        await collection.Data.InsertMany(
            BatchInsertRequest.Create<object>(new { blob = blobData })
        );

        // Fetch by id
        var obj = await collection.Query.FetchObjectByID(uuid, returnProperties: new[] { "blob" });

        // Fetch all
        var objs = (
            await collection.Query.FetchObjects(returnProperties: new[] { "blob" })
        ).Objects.ToList();

        Assert.Equal(2, objs.Count());
        Assert.NotNull(obj);
        Assert.Equal(blobData, obj.Properties["blob"]);
        Assert.Equal(blobData, objs[0].Properties["blob"]);
        Assert.Equal(blobData, objs[1].Properties["blob"]);
    }

    [Fact]
    public async Task Test_Collection_Query_Rerank()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "QueryTestCollection",
            properties: [Property.Text("firstName"), Property.Int("age"), Property.Text("bio")],
            rerankerConfig: new Reranker.Custom { Type = "reranker-dummy", Config = new { } },
            vectorConfig: Configure.Vectors.SelfProvided()
        );

        // Sample data. The reranker-dummy module will use the length of the "bio" property to
        // rerank the results. Longer bios should get higher scores.
        // In order to make the assertions later work, the age property reflects the length of the bio in characters.
        // This way we can assert that the reranked results are ordered by the length of "bio" and "age" descending.
        var data = new[]
        {
            new
            {
                firstName = "Bob",
                age = 38,
                bio = "This person enjoys painting and music.",
            },
            new
            {
                firstName = "Alice",
                age = 41,
                bio = "This person loves programming and hiking.",
            },
            new
            {
                firstName = "Charlie",
                age = 43,
                bio = "This person is an avid reader and traveler.",
            },
        };

        // Insert data
        var id1 = await collection.Data.Insert(data[0], id: _reusableUuids[0]);
        var id2 = await collection.Data.Insert(data[1], id: _reusableUuids[1]);
        var id3 = await collection.Data.Insert(data[2], id: _reusableUuids[2]);

        // Act
        var results = (await collection.Query.FetchObjects(returnMetadata: MetadataOptions.Score))
            .Objects.Select(r => new
            {
                r.ID,
                Age = r.Properties["age"],
                r.Metadata.RerankScore,
            })
            .ToList();

        var resultsReranked = (
            await collection.Query.FetchObjects(
                rerank: new Rerank { Property = "bio" },
                returnMetadata: MetadataOptions.Score
            )
        )
            .Objects.Select(r => new
            {
                r.ID,
                Age = r.Properties["age"],
                r.Metadata.RerankScore,
            })
            .ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(3, resultsReranked.Count);

        Assert.True(results.All(r => r.RerankScore == null));
        Assert.True(resultsReranked.All(r => r.RerankScore != null));

        var resultsIDs = results.OrderByDescending(x => x.Age).Select(x => x.ID).ToList();
        var resultsRerankedIDs = resultsReranked.Select(x => x.ID).ToList();

        Assert.Equal(resultsIDs, resultsRerankedIDs);
    }
}

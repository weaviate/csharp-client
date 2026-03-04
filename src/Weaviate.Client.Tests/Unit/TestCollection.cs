using System.Text.Json;
using System.Text.Json.Nodes;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The collection tests class
/// </summary>
public class CollectionTests
{
    /// <summary>
    /// Tests that collections are equal
    /// </summary>
    [Fact]
    public void Collections_Are_Equal()
    {
        var c1 = new CollectionConfig
        {
            Name = "ClassName",
            Description = "Description",
            Properties = [Property.Text("Name")],
            VectorConfig = new VectorConfig(
                "default",
                new Vectorizer.SelfProvided(),
                new VectorIndex.HNSW()
            ),
            InvertedIndexConfig = InvertedIndexConfig.Default,
            ReplicationConfig = ReplicationConfig.Default,
            ShardingConfig = ShardingConfig.Default,
            MultiTenancyConfig = MultiTenancyConfig.Default,
        };

        var c2 = new CollectionConfig
        {
            Name = "ClassName",
            Description = "Description",
            Properties = [Property.Text("Name")],
            VectorConfig = new VectorConfig(
                "default",
                new Vectorizer.SelfProvided(),
                new VectorIndex.HNSW()
            ),
            InvertedIndexConfig = InvertedIndexConfig.Default,
            ReplicationConfig = ReplicationConfig.Default,
            ShardingConfig = ShardingConfig.Default,
            MultiTenancyConfig = MultiTenancyConfig.Default,
        };

        Assert.Equal(c1, c2);
    }

    /// <summary>
    /// Tests that collection equals null returns false
    /// </summary>
    [Fact]
    public void Collection_Equals_Null_Returns_False()
    {
        // Arrange
        var collection = new CollectionCreateParams();

        // Act
        var result = collection.Equals(null);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that collection equals self returns true
    /// </summary>
    [Fact]
    public void Collection_Equals_Self_Returns_True()
    {
        // Arrange
        var collection = new CollectionCreateParams();

        // Act
        var result = collection.Equals(collection);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that collection equals different objects returns false
    /// </summary>
    [Fact]
    public void Collection_Equals_DifferentObjects_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionCreateParams();
        var collection2 = new CollectionCreateParams();

        // Act

        // Assert
        Assert.Equal(collection1, collection2);
        Assert.NotSame(collection1, collection2);
    }

    /// <summary>
    /// Tests that collection equals mismatching returns false
    /// </summary>
    [Fact]
    public void Collection_Equals_Mismatching_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionCreateParams { Name = "Test", Description = "Test" };
        var collection2 = new CollectionCreateParams { Name = "Different", Description = "Test" };

        // Act

        // Assert
        Assert.NotEqual(collection1, collection2);
    }

    /// <summary>
    /// Tests that collection equals inverted index config matches returns true
    /// </summary>
    [Fact]
    public void Collection_Equals_InvertedIndexConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionCreateParams
        {
            InvertedIndexConfig = new InvertedIndexConfig(),
        };
        var collection2 = new CollectionCreateParams
        {
            InvertedIndexConfig = new InvertedIndexConfig(),
        };

        // Act

        // Assert
        Assert.Equal(collection1, collection2);
    }

    /// <summary>
    /// Tests that collection equals inverted index config does not match returns false
    /// </summary>
    [Fact]
    public void Collection_Equals_InvertedIndexConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionCreateParams
        {
            InvertedIndexConfig = new InvertedIndexConfig(),
        };
        var collection2 = new CollectionCreateParams { InvertedIndexConfig = null };

        // Act

        // Assert
        Assert.NotEqual(collection1, collection2);
    }

    /// <summary>
    /// Tests that collection equals module config matches returns true
    /// </summary>
    [Fact]
    public void Collection_Equals_ModuleConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionConfig { ModuleConfig = new() };
        var collection2 = new CollectionConfig { ModuleConfig = new() };

        // Act

        // Assert
        Assert.Equivalent(collection1, collection2);
        Assert.NotSame(collection1, collection2);
    }

    /// <summary>
    /// Tests that collection equals module config does not match returns false
    /// </summary>
    [Fact]
    public void Collection_Equals_ModuleConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionConfig { ModuleConfig = new() };
        var collection2 = new CollectionConfig { ModuleConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that collection equals multi tenancy config matches returns true
    /// </summary>
    [Fact]
    public void Collection_Equals_MultiTenancyConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionCreateParams
        {
            MultiTenancyConfig = new MultiTenancyConfig(),
        };
        var collection2 = new CollectionCreateParams
        {
            MultiTenancyConfig = new MultiTenancyConfig(),
        };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that collection equals multi tenancy config does not match returns false
    /// </summary>
    [Fact]
    public void Collection_Equals_MultiTenancyConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionCreateParams
        {
            MultiTenancyConfig = new MultiTenancyConfig(),
        };
        var collection2 = new CollectionCreateParams { MultiTenancyConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that collection equals replication config matches returns true
    /// </summary>
    [Fact]
    public void Collection_Equals_ReplicationConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionCreateParams
        {
            ReplicationConfig = new ReplicationConfig(),
        };
        var collection2 = new CollectionCreateParams
        {
            ReplicationConfig = new ReplicationConfig(),
        };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that collection equals replication config does not match returns false
    /// </summary>
    [Fact]
    public void Collection_Equals_ReplicationConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionCreateParams
        {
            ReplicationConfig = new ReplicationConfig(),
        };
        var collection2 = new CollectionCreateParams { ReplicationConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that collection equals sharding config matches returns true
    /// </summary>
    [Fact]
    public void Collection_Equals_ShardingConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionCreateParams { ShardingConfig = new ShardingConfig() };
        var collection2 = new CollectionCreateParams { ShardingConfig = new ShardingConfig() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that collection equals sharding config does not match returns false
    /// </summary>
    [Fact]
    public void Collection_Equals_ShardingConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionCreateParams { ShardingConfig = new ShardingConfig() };
        var collection2 = new CollectionCreateParams { ShardingConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that collection equals vector config matches returns true
    /// </summary>
    [Fact]
    public void Collection_Equals_VectorConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionCreateParams { VectorConfig = new VectorConfigList() };
        var collection2 = new CollectionCreateParams { VectorConfig = new VectorConfigList() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that collection generative deserializes into i generative config
    /// </summary>
    [Fact]
    public void Collection_Generative_Deserializes_Into_IGenerativeConfig()
    {
        var key = "generative-dummy";
        var value = JsonSerializer.Deserialize<object>(
            "{\"config\":{\"configOption\":\"ConfigValue\"}}",
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        dynamic? config = GenerativeConfigSerialization.Factory(key, value);

        Assert.NotNull(config);
        Assert.NotNull(config!.Config);
        Assert.Equal("ConfigValue", config!.Config.configOption);
        Assert.IsType<GenerativeConfig.Custom>(config);
        Assert.IsAssignableFrom<IGenerativeConfig>(config);
    }

    /// <summary>
    /// Tests that collection rerank deserializes into i reranker config
    /// </summary>
    [Fact]
    public void Collection_Rerank_Deserializes_Into_IRerankerConfig()
    {
        var key = "reranker-dummy";
        var value = JsonSerializer.Deserialize<object>(
            "{\"config\":{\"configOption\":\"ConfigValue\"}}",
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        dynamic? config = RerankerConfigSerialization.Factory(key, value);

        Assert.NotNull(config);
        Assert.NotNull(config!.Config);
        Assert.Equal("ConfigValue", config!.Config.configOption);
        Assert.IsType<Reranker.Custom>(config);
        Assert.IsAssignableFrom<IRerankerConfig>(config);
    }

    /// <summary>
    /// Tests that collection import export are equal
    /// </summary>
    [Fact]
    public void Collection_Import_Export_Are_Equal()
    {
        string json =
            $@"{{
            ""class"": ""ClassName"",
            ""description"": ""Test_Collection_Config_Add_Vector"",
            ""invertedIndexConfig"": {{
                ""bm25"": {{
                    ""b"": 0.75,
                    ""k1"": 1.2
                }},
                ""cleanupIntervalSeconds"": 60,
                ""stopwords"": {{
                    ""preset"": ""en""
                }},
                ""usingBlockMaxWAND"": true
            }},
            ""objectTtlConfig"": {{
                ""defaultTtl"": 0,
                ""deleteOn"": ""_creationTimeUnix"",
                ""enabled"": false,
                ""filterExpiredObjects"": false
            }},
            ""multiTenancyConfig"": {{
                ""autoTenantActivation"": false,
                ""autoTenantCreation"": false,
                ""enabled"": false
            }},
            ""properties"": [
                {{
                    ""dataType"": [
                        ""text""
                    ],
                    ""indexFilterable"": true,
                    ""indexRangeFilters"": false,
                    ""indexSearchable"": true,
                    ""moduleConfig"": {{
                        ""text2vec-cohere"": {{
                            ""skip"": false,
                            ""vectorizePropertyName"": false
                        }}
                    }},
                    ""name"": ""name"",
                    ""tokenization"": ""word""
                }}
            ],
            ""replicationConfig"": {{
                ""asyncEnabled"": false,
                ""deletionStrategy"": ""NoAutomatedResolution"",
                ""factor"": 1
            }},
            ""shardingConfig"": {{
                ""actualCount"": 1,
                ""actualVirtualCount"": 128,
                ""desiredCount"": 1,
                ""desiredVirtualCount"": 128,
                ""function"": ""murmur3"",
                ""key"": ""_id"",
                ""strategy"": ""hash"",
                ""virtualPerPhysical"": 128
            }},
            ""vectorConfig"": {{
                ""default"": {{
                    ""vectorIndexConfig"": {{
                        ""bq"": {{
                            ""enabled"": false
                        }},
                        ""cleanupIntervalSeconds"": 300,
                        ""distance"": ""cosine"",
                        ""dynamicEfFactor"": 8,
                        ""dynamicEfMax"": 500,
                        ""dynamicEfMin"": 100,
                        ""ef"": -1,
                        ""efConstruction"": 128,
                        ""filterStrategy"": ""sweeping"",
                        ""flatSearchCutoff"": 40000,
                        ""maxConnections"": 32,
                        ""multivector"": {{
                            ""aggregation"": ""maxSim"",
                            ""enabled"": false,
                            ""muvera"": {{
                                ""dprojections"": 16,
                                ""enabled"": false,
                                ""ksim"": 4,
                                ""repetitions"": 10
                            }}
                        }},
                        ""pq"": {{
                            ""bitCompression"": false,
                            ""centroids"": 256,
                            ""enabled"": false,
                            ""encoder"": {{
                                ""distribution"": ""log-normal"",
                                ""type"": ""kmeans""
                            }},
                            ""segments"": 0,
                            ""trainingLimit"": 100000
                        }},
                        ""rq"": {{
                            ""bits"": 8,
                            ""enabled"": false,
                            ""rescoreLimit"": 20
                        }},
                        ""skip"": false,
                        ""sq"": {{
                            ""enabled"": false,
                            ""rescoreLimit"": 20,
                            ""trainingLimit"": 100000
                        }},
                        ""vectorCacheMaxObjects"": 1000000000000
                    }},
                    ""vectorIndexType"": ""hnsw"",
                    ""vectorizer"": {{
                        ""text2vec-cohere"": {{
                            ""baseUrl"": ""https://api.cohere.ai"",
                            ""model"": ""embed-multilingual-v3.0"",
                            ""truncate"": ""END"",
                            ""vectorizeClassName"": true
                        }}
                    }}
                }}
            }}
        }}";

        var collectionFromJson = JsonSerializer.Deserialize<Rest.Dto.Class>(
            json,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        var collectionModel = collectionFromJson!.ToModel();

        var collectionDto = collectionModel.ToDto();

        // Serialize both to JSON
        var expectedJson = JsonSerializer.Serialize(
            collectionFromJson,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        var actualJson = JsonSerializer.Serialize(
            collectionDto,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        // Parse as JsonElement for semantic comparison (ignoring property order)
        var expectedDoc = JsonNode.Parse(expectedJson);
        var actualDoc = JsonNode.Parse(actualJson);

        var eq = JsonNode.DeepEquals(expectedDoc, actualDoc);

        Assert.True(
            eq,
            $"JSON structures differ:\nExpected:\n{JsonComparer.SortJsonNode(expectedDoc)}\n\nActual:\n{JsonComparer.SortJsonNode(actualDoc)}"
        );
    }

    /// <summary>
    /// Tests that to collection config create params throws when legacy settings present
    /// </summary>
    [Fact]
    public void ToCollectionConfigCreateParams_Throws_WhenLegacySettingsPresent()
    {
        var export = new CollectionConfigExport
        {
            Name = "Test",
            VectorIndexType = "HNSW",
            VectorIndexConfig = new object(),
            Vectorizer = "text2vec",
        };

        var ex = Assert.Throws<WeaviateClientException>(() =>
            export.ToCollectionConfigCreateParams()
        );
        Assert.Contains("legacy settings", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests that to collection config create params succeeds when no legacy settings
    /// </summary>
    [Fact]
    public void ToCollectionConfigCreateParams_Succeeds_WhenNoLegacySettings()
    {
        var export = new CollectionConfigExport
        {
            Name = "Test",
            Description = "desc",
            Properties = [],
            References = [],
            InvertedIndexConfig = null,
            MultiTenancyConfig = null,
            ReplicationConfig = null,
            ShardingConfig = null,
            VectorConfig = [],
            GenerativeConfig = null,
            RerankerConfig = null,
            VectorIndexType = "",
            VectorIndexConfig = null,
            Vectorizer = "",
        };

        var result = export.ToCollectionConfigCreateParams();

        Assert.NotNull(result);
        Assert.Equal(export.Name, result.Name);
        Assert.Equal(export.Description, result.Description);
        Assert.Equal(export.Properties, result.Properties);
        Assert.Equal(export.References, result.References);
        Assert.Equal(export.VectorConfig, result.VectorConfig);
    }

    /// <summary>
    /// Tests that ReplicationConfig with AsyncConfig maps all fields to the DTO.
    /// </summary>
    [Fact]
    public void ReplicationConfig_WithAsyncConfig_MapsToDto()
    {
        var asyncConfig = new ReplicationAsyncConfig
        {
            MaxWorkers = 4,
            HashtreeHeight = 16,
            Frequency = 1000,
            FrequencyWhilePropagating = 500,
            AliveNodesCheckingFrequency = 30000,
            LoggingFrequency = 60,
            DiffBatchSize = 100,
            DiffPerNodeTimeout = 30,
            PrePropagationTimeout = 120,
            PropagationTimeout = 60,
            PropagationLimit = 10000,
            PropagationDelay = 5000,
            PropagationConcurrency = 2,
            PropagationBatchSize = 50,
        };

        var collection = new CollectionConfig
        {
            Name = "TestCollection",
            ReplicationConfig = new ReplicationConfig { AsyncConfig = asyncConfig },
        };

        var dto = collection.ToDto();

        Assert.NotNull(dto.ReplicationConfig?.AsyncConfig);
        var ac = dto.ReplicationConfig!.AsyncConfig!;
        Assert.Equal(4, ac.MaxWorkers);
        Assert.Equal(16, ac.HashtreeHeight);
        Assert.Equal(1000, ac.Frequency);
        Assert.Equal(500, ac.FrequencyWhilePropagating);
        Assert.Equal(30000, ac.AliveNodesCheckingFrequency);
        Assert.Equal(60, ac.LoggingFrequency);
        Assert.Equal(100, ac.DiffBatchSize);
        Assert.Equal(30, ac.DiffPerNodeTimeout);
        Assert.Equal(120, ac.PrePropagationTimeout);
        Assert.Equal(60, ac.PropagationTimeout);
        Assert.Equal(10000, ac.PropagationLimit);
        Assert.Equal(5000, ac.PropagationDelay);
        Assert.Equal(2, ac.PropagationConcurrency);
        Assert.Equal(50, ac.PropagationBatchSize);
    }

    /// <summary>
    /// Tests that DTO with AsyncConfig round-trips back to the model correctly.
    /// </summary>
    [Fact]
    public void ReplicationConfig_WithAsyncConfig_RoundTripsFromDto()
    {
        var dtoAsyncConfig = new Rest.Dto.ReplicationAsyncConfig
        {
            MaxWorkers = 8,
            HashtreeHeight = 12,
            PropagationLimit = 5000,
        };

        var dto = new Rest.Dto.Class
        {
            Class1 = "TestCollection",
            ReplicationConfig = new Rest.Dto.ReplicationConfig { AsyncConfig = dtoAsyncConfig },
        };

        var model = dto.ToModel();

        Assert.NotNull(model.ReplicationConfig?.AsyncConfig);
        var ac = model.ReplicationConfig!.AsyncConfig!;
        Assert.Equal(8, ac.MaxWorkers);
        Assert.Equal(12, ac.HashtreeHeight);
        Assert.Equal(5000, ac.PropagationLimit);
        // Unset fields are null
        Assert.Null(ac.Frequency);
        Assert.Null(ac.PropagationBatchSize);
    }

    /// <summary>
    /// Tests that ReplicationConfig without AsyncConfig does not produce an asyncConfig in the DTO.
    /// </summary>
    [Fact]
    public void ReplicationConfig_WithoutAsyncConfig_ProducesNullAsyncConfigInDto()
    {
        var collection = new CollectionConfig
        {
            Name = "TestCollection",
            ReplicationConfig = new ReplicationConfig { Factor = 2 },
        };

        var dto = collection.ToDto();

        Assert.Null(dto.ReplicationConfig?.AsyncConfig);
    }

    /// <summary>
    /// Tests that DTO with all AsyncConfig fields set round-trips back to the model correctly.
    /// </summary>
    [Fact]
    public void ReplicationConfig_WithAsyncConfig_RoundTripsAllFieldsFromDto()
    {
        var dtoAsyncConfig = new Rest.Dto.ReplicationAsyncConfig
        {
            MaxWorkers = 1,
            HashtreeHeight = 2,
            Frequency = 3,
            FrequencyWhilePropagating = 4,
            AliveNodesCheckingFrequency = 5,
            LoggingFrequency = 6,
            DiffBatchSize = 7,
            DiffPerNodeTimeout = 8,
            PrePropagationTimeout = 9,
            PropagationTimeout = 10,
            PropagationLimit = 11,
            PropagationDelay = 12,
            PropagationConcurrency = 13,
            PropagationBatchSize = 14,
        };

        var dto = new Rest.Dto.Class
        {
            Class1 = "TestCollection",
            ReplicationConfig = new Rest.Dto.ReplicationConfig { AsyncConfig = dtoAsyncConfig },
        };

        var model = dto.ToModel();

        Assert.NotNull(model.ReplicationConfig?.AsyncConfig);
        var ac = model.ReplicationConfig!.AsyncConfig!;
        Assert.Equal(1, ac.MaxWorkers);
        Assert.Equal(2, ac.HashtreeHeight);
        Assert.Equal(3, ac.Frequency);
        Assert.Equal(4, ac.FrequencyWhilePropagating);
        Assert.Equal(5, ac.AliveNodesCheckingFrequency);
        Assert.Equal(6, ac.LoggingFrequency);
        Assert.Equal(7, ac.DiffBatchSize);
        Assert.Equal(8, ac.DiffPerNodeTimeout);
        Assert.Equal(9, ac.PrePropagationTimeout);
        Assert.Equal(10, ac.PropagationTimeout);
        Assert.Equal(11, ac.PropagationLimit);
        Assert.Equal(12, ac.PropagationDelay);
        Assert.Equal(13, ac.PropagationConcurrency);
        Assert.Equal(14, ac.PropagationBatchSize);
    }

    /// <summary>
    /// Tests that ReplicationConfigUpdate forwards AsyncConfig get/set to the wrapped ReplicationConfig.
    /// </summary>
    [Fact]
    public void ReplicationConfigUpdate_AsyncConfig_ForwardsToWrappedConfig()
    {
        var replicationConfig = new ReplicationConfig { AsyncConfig = null };
        var update = new ReplicationConfigUpdate(replicationConfig);

        Assert.Null(update.AsyncConfig);

        update.AsyncConfig = new ReplicationAsyncConfig { MaxWorkers = 42 };

        Assert.NotNull(update.AsyncConfig);
        Assert.Equal(42, update.AsyncConfig!.MaxWorkers);
        Assert.NotNull(replicationConfig.AsyncConfig);
        Assert.Equal(42, replicationConfig.AsyncConfig!.MaxWorkers);
    }
}

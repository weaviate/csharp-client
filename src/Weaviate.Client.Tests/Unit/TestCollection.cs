using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class CollectionTests
{
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

    [Fact]
    public void Collection_Equals_Null_Returns_False()
    {
        // Arrange
        var collection = new CollectionConfig();

        // Act
        var result = collection.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_Self_Returns_True()
    {
        // Arrange
        var collection = new CollectionConfig();

        // Act
        var result = collection.Equals(collection);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Collection_Equals_DifferentObjects_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionConfig();
        var collection2 = new CollectionConfig();

        // Act

        // Assert
        Assert.Equal(collection1, collection2);
        Assert.NotSame(collection1, collection2);
    }

    [Fact]
    public void Collection_Equals_Mismatching_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionConfig { Name = "Test", Description = "Test" };
        var collection2 = new CollectionConfig { Name = "Different", Description = "Test" };

        // Act

        // Assert
        Assert.NotEqual(collection1, collection2);
    }

    [Fact]
    public void Collection_Equals_InvertedIndexConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionConfig { InvertedIndexConfig = new InvertedIndexConfig() };
        var collection2 = new CollectionConfig { InvertedIndexConfig = new InvertedIndexConfig() };

        // Act

        // Assert
        Assert.Equal(collection1, collection2);
    }

    [Fact]
    public void Collection_Equals_InvertedIndexConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionConfig { InvertedIndexConfig = new InvertedIndexConfig() };
        var collection2 = new CollectionConfig { InvertedIndexConfig = null };

        // Act

        // Assert
        Assert.NotEqual(collection1, collection2);
    }

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

    [Fact]
    public void Collection_Equals_MultiTenancyConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionConfig { MultiTenancyConfig = new MultiTenancyConfig() };
        var collection2 = new CollectionConfig { MultiTenancyConfig = new MultiTenancyConfig() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Collection_Equals_MultiTenancyConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionConfig { MultiTenancyConfig = new MultiTenancyConfig() };
        var collection2 = new CollectionConfig { MultiTenancyConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_ReplicationConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionConfig { ReplicationConfig = new ReplicationConfig() };
        var collection2 = new CollectionConfig { ReplicationConfig = new ReplicationConfig() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Collection_Equals_ReplicationConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionConfig { ReplicationConfig = new ReplicationConfig() };
        var collection2 = new CollectionConfig { ReplicationConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_ShardingConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionConfig { ShardingConfig = new ShardingConfig() };
        var collection2 = new CollectionConfig { ShardingConfig = new ShardingConfig() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Collection_Equals_ShardingConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new CollectionConfig { ShardingConfig = new ShardingConfig() };
        var collection2 = new CollectionConfig { ShardingConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_VectorConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new CollectionConfig { VectorConfig = new VectorConfigList() };
        var collection2 = new CollectionConfig { VectorConfig = new VectorConfigList() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

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
        using var expectedDoc = JsonDocument.Parse(expectedJson);
        using var actualDoc = JsonDocument.Parse(actualJson);

        // Use JsonElement.DeepEquals for semantic comparison
        Assert.True(
            JsonElement.DeepEquals(expectedDoc.RootElement, actualDoc.RootElement),
            $"JSON structures differ:\nExpected:\n{expectedJson}\n\nActual:\n{actualJson}"
        );
    }
}

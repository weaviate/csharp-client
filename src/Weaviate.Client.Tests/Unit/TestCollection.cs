using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class CollectionTests
{
    [Fact]
    public void Collections_Are_Equal()
    {
        var c1 = new Collection
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

        var c2 = new Collection
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
        var collection = new Collection();

        // Act
        var result = collection.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_Self_Returns_True()
    {
        // Arrange
        var collection = new Collection();

        // Act
        var result = collection.Equals(collection);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Collection_Equals_DifferentObjects_Returns_False()
    {
        // Arrange
        var collection1 = new Collection();
        var collection2 = new Collection();

        // Act

        // Assert
        Assert.Equal(collection1, collection2);
        Assert.NotSame(collection1, collection2);
    }

    [Fact]
    public void Collection_Equals_Mismatching_Returns_False()
    {
        // Arrange
        var collection1 = new Collection { Name = "Test", Description = "Test" };
        var collection2 = new Collection { Name = "Different", Description = "Test" };

        // Act

        // Assert
        Assert.NotEqual(collection1, collection2);
    }

    [Fact]
    public void Collection_Equals_InvertedIndexConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new Collection { InvertedIndexConfig = new InvertedIndexConfig() };
        var collection2 = new Collection { InvertedIndexConfig = new InvertedIndexConfig() };

        // Act

        // Assert
        Assert.Equal(collection1, collection2);
    }

    [Fact]
    public void Collection_Equals_InvertedIndexConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new Collection { InvertedIndexConfig = new InvertedIndexConfig() };
        var collection2 = new Collection { InvertedIndexConfig = null };

        // Act

        // Assert
        Assert.NotEqual(collection1, collection2);
    }

    [Fact]
    public void Collection_Equals_ModuleConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new Collection { ModuleConfig = new() };
        var collection2 = new Collection { ModuleConfig = new() };

        // Act

        // Assert
        Assert.Equivalent(collection1, collection2);
        Assert.NotSame(collection1, collection2);
    }

    [Fact]
    public void Collection_Equals_ModuleConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new Collection { ModuleConfig = new() };
        var collection2 = new Collection { ModuleConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_MultiTenancyConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new Collection { MultiTenancyConfig = new MultiTenancyConfig() };
        var collection2 = new Collection { MultiTenancyConfig = new MultiTenancyConfig() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Collection_Equals_MultiTenancyConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new Collection { MultiTenancyConfig = new MultiTenancyConfig() };
        var collection2 = new Collection { MultiTenancyConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_ReplicationConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new Collection { ReplicationConfig = new ReplicationConfig() };
        var collection2 = new Collection { ReplicationConfig = new ReplicationConfig() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Collection_Equals_ReplicationConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new Collection { ReplicationConfig = new ReplicationConfig() };
        var collection2 = new Collection { ReplicationConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_ShardingConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new Collection { ShardingConfig = new ShardingConfig() };
        var collection2 = new Collection { ShardingConfig = new ShardingConfig() };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Collection_Equals_ShardingConfig_DoesNotMatch_Returns_False()
    {
        // Arrange
        var collection1 = new Collection { ShardingConfig = new ShardingConfig() };
        var collection2 = new Collection { ShardingConfig = null };

        // Act
        var result = collection1.Equals(collection2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Collection_Equals_VectorConfig_Matches_Returns_True()
    {
        // Arrange
        var collection1 = new Collection { VectorConfig = new VectorConfigList() };
        var collection2 = new Collection { VectorConfig = new VectorConfigList() };

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
}

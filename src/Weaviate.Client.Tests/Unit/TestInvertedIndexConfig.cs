using Weaviate.Client.Models;
using Weaviate.Client.Rest;

namespace Weaviate.Client.Tests;

public class InvertedIndexConfigTests
{
    [Fact]
    public void Equals_Null_Returns_False()
    {
        // Arrange
        var config = new InvertedIndexConfig();

        // Act
        var result = config.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Self_Returns_True()
    {
        // Arrange
        var config = new InvertedIndexConfig();

        // Act
        var result = config.Equals(config);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Different_Configs_Returns_False()
    {
        // Arrange
        var config1 = new InvertedIndexConfig();
        var config2 = new InvertedIndexConfig();

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.True(result);
        Assert.NotSame(config1, config2);
    }

    [Fact]
    public void Equals_Configs_With_Different_Bm25_Returns_False()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { Bm25 = BM25Config.Default };
        var config2 = new InvertedIndexConfig { Bm25 = new BM25Config() { B = 0.76f } };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Configs_With_Same_Bm25_Returns_True()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { Bm25 = BM25Config.Default };
        var config2 = new InvertedIndexConfig { Bm25 = BM25Config.Default };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Configs_With_Different_CleanupIntervalSeconds_Returns_False()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { CleanupIntervalSeconds = 60 };
        var config2 = new InvertedIndexConfig { CleanupIntervalSeconds = 30 };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Configs_With_Same_CleanupIntervalSeconds_Returns_True()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { CleanupIntervalSeconds = 60 };
        var config2 = new InvertedIndexConfig { CleanupIntervalSeconds = 60 };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Configs_With_Different_IndexNullState_Returns_False()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { IndexNullState = true };
        var config2 = new InvertedIndexConfig { IndexNullState = false };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Configs_With_Same_IndexNullState_Returns_True()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { IndexNullState = true };
        var config2 = new InvertedIndexConfig { IndexNullState = true };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Configs_With_Different_IndexPropertyLength_Returns_False()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { IndexPropertyLength = true };
        var config2 = new InvertedIndexConfig { IndexPropertyLength = false };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Configs_With_Same_IndexPropertyLength_Returns_True()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { IndexPropertyLength = true };
        var config2 = new InvertedIndexConfig { IndexPropertyLength = true };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Configs_With_Different_IndexTimestamps_Returns_False()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { IndexTimestamps = true };
        var config2 = new InvertedIndexConfig { IndexTimestamps = false };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Configs_With_Same_IndexTimestamps_Returns_True()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { IndexTimestamps = true };
        var config2 = new InvertedIndexConfig { IndexTimestamps = true };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Configs_With_Different_Stopwords_Returns_False()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { Stopwords = new StopwordConfig() };
        var config2 = new InvertedIndexConfig { Stopwords = new StopwordConfig() };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.True(result);
        Assert.NotSame(config1, config2);
    }

    [Fact]
    public void Equals_Configs_With_Same_Stopwords_Returns_True()
    {
        // Arrange
        var config1 = new InvertedIndexConfig { Stopwords = new StopwordConfig() };
        var config2 = new InvertedIndexConfig { Stopwords = new StopwordConfig() };

        // Act
        var result = config1.Equals(config2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Deserialize_InvertedIndexConfig()
    {
        // Arrange
        var json =
            @"{
            ""bm25"": {
              ""b"": 0.7,
              ""k1"": 1.3
            },
            ""cleanupIntervalSeconds"": 30,
            ""indexNullState"": true,
            ""indexPropertyLength"": true,
            ""indexTimestamps"": true,
            ""stopwords"": {
              ""additions"": [
                ""plus""
              ],
              ""preset"": ""none"",
              ""removals"": [
                ""minus""
              ]
            }
        }";

        // Act
        var config = System.Text.Json.JsonSerializer.Deserialize<InvertedIndexConfig>(
            json,
            WeaviateRestClient.RestJsonSerializerOptions
        );

        // Assert
        Assert.NotNull(config);
        Assert.NotNull(config.Bm25);
        Assert.Equal(0.7f, config.Bm25.B);
        Assert.Equal(1.3f, config.Bm25.K1);
        Assert.Equal(30, config.CleanupIntervalSeconds);
        Assert.True(config.IndexNullState);
        Assert.True(config.IndexPropertyLength);
        Assert.True(config.IndexTimestamps);
        Assert.NotNull(config.Stopwords);
        Assert.Equal(StopwordConfig.Presets.None, config.Stopwords.Preset);
        Assert.Equal(new List<string> { "plus" }, config.Stopwords.Additions);
        Assert.Equal(new List<string> { "minus" }, config.Stopwords.Removals);
    }

    [Fact]
    public void Serialize_InvertedIndexConfig()
    {
        // Arrange
        var config = new InvertedIndexConfig
        {
            Bm25 = new BM25Config { B = 0.7f, K1 = 1.3f },
            CleanupIntervalSeconds = 30,
            IndexNullState = true,
            IndexPropertyLength = true,
            IndexTimestamps = true,
            Stopwords = new StopwordConfig
            {
                Preset = StopwordConfig.Presets.None,
                Additions = new List<string> { "plus" },
                Removals = new List<string> { "minus" },
            },
        };

        var expectedJson =
            @"{""bm25"":{""b"":0.7,""k1"":1.3},""cleanupIntervalSeconds"":30,""indexNullState"":true,""indexPropertyLength"":true,""indexTimestamps"":true,""stopwords"":{""additions"":[""plus""],""preset"":""none"",""removals"":[""minus""]}}";

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(
            config,
            WeaviateRestClient.RestJsonSerializerOptions
        );

        // Assert
        Assert.True(JsonComparer.AreJsonEqual(expectedJson, json));
    }
}

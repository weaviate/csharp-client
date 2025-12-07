using Weaviate.Client.Models;
using Weaviate.Client.Orm.Attributes;
using Weaviate.Client.Orm.Schema;
using Xunit;

namespace Weaviate.Client.Orm.Tests.Schema;

public class CollectionConfigBuilderTests
{
    #region Generative Configuration Tests

    [Fact]
    public void BuildCollectionConfig_WithGenerativeOpenAI_ConfiguresCorrectly()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithGenerativeOpenAI>();

        // Assert
        Assert.NotNull(config.GenerativeConfig);
        Assert.IsType<GenerativeConfig.OpenAI>(config.GenerativeConfig);

        var generative = config.GenerativeConfig as GenerativeConfig.OpenAI;
        Assert.NotNull(generative);
        Assert.Equal("gpt-4", generative.Model);
        Assert.Equal(500, generative.MaxTokens);
        Assert.Equal(0.7, generative.Temperature);
    }

    [Fact]
    public void BuildCollectionConfig_WithGenerativeAnthropic_ConfiguresCorrectly()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithGenerativeAnthropic>();

        // Assert
        Assert.NotNull(config.GenerativeConfig);
        Assert.IsType<GenerativeConfig.Anthropic>(config.GenerativeConfig);

        var generative = config.GenerativeConfig as GenerativeConfig.Anthropic;
        Assert.NotNull(generative);
        Assert.Equal("claude-3-5-sonnet-20241022", generative.Model);
        Assert.Equal(4096, generative.MaxTokens);
    }

    [Fact]
    public void BuildCollectionConfig_WithGenerativeConfigMethod_InvokesMethod()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithGenerativeConfigMethod>();

        // Assert
        Assert.NotNull(config.GenerativeConfig);
        var generative = config.GenerativeConfig as GenerativeConfig.OpenAI;
        Assert.NotNull(generative);
        Assert.Equal("gpt-4", generative.Model); // From attribute
        Assert.Equal(0.9, generative.Temperature); // From ConfigMethod
        Assert.Equal(1000, generative.MaxTokens); // From ConfigMethod
    }

    [Fact]
    public void BuildCollectionConfig_WithGenerativeConfigMethodClass_UsesTypeSafe()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithGenerativeConfigMethodClass>();

        // Assert
        Assert.NotNull(config.GenerativeConfig);
        var generative = config.GenerativeConfig as GenerativeConfig.Cohere;
        Assert.NotNull(generative);
        Assert.Equal("command-r-plus", generative.Model); // Changed by ConfigMethod
        Assert.Equal(2.0, generative.Temperature); // Set by ConfigMethod
    }

    #endregion

    #region Reranker Configuration Tests

    [Fact]
    public void BuildCollectionConfig_WithRerankerCohere_ConfiguresCorrectly()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithRerankerCohere>();

        // Assert
        Assert.NotNull(config.RerankerConfig);
        Assert.IsType<Reranker.Cohere>(config.RerankerConfig);

        var reranker = config.RerankerConfig as Reranker.Cohere;
        Assert.NotNull(reranker);
        Assert.Equal("rerank-english-v2.0", reranker.Model);
    }

    [Fact]
    public void BuildCollectionConfig_WithRerankerVoyageAI_ConfiguresCorrectly()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithRerankerVoyageAI>();

        // Assert
        Assert.NotNull(config.RerankerConfig);
        Assert.IsType<Reranker.VoyageAI>(config.RerankerConfig);

        var reranker = config.RerankerConfig as Reranker.VoyageAI;
        Assert.NotNull(reranker);
        Assert.Equal("rerank-2.5", reranker.Model);
    }

    [Fact]
    public void BuildCollectionConfig_WithRerankerTransformers_ConfiguresCorrectly()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithRerankerTransformers>();

        // Assert
        Assert.NotNull(config.RerankerConfig);
        Assert.IsType<Reranker.Transformers>(config.RerankerConfig);
    }

    #endregion

    #region Sharding Configuration Tests

    [Fact]
    public void BuildCollectionConfig_WithShardingConfig_ConfiguresCorrectly()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithShardingConfig>();

        // Assert
        Assert.NotNull(config.ShardingConfig);
        Assert.Equal(3, config.ShardingConfig.DesiredCount);
        Assert.Equal(256, config.ShardingConfig.VirtualPerPhysical);
        Assert.Equal("customKey", config.ShardingConfig.Key);
    }

    [Fact]
    public void BuildCollectionConfig_WithoutShardingConfig_ReturnsNull()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithGenerativeOpenAI>();

        // Assert - ShardingConfig should be null when not specified
        Assert.Null(config.ShardingConfig);
    }

    #endregion

    #region Replication Configuration Tests

    [Fact]
    public void BuildCollectionConfig_WithReplicationConfig_ConfiguresCorrectly()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithReplicationConfig>();

        // Assert
        Assert.NotNull(config.ReplicationConfig);
        Assert.Equal(3, config.ReplicationConfig.Factor);
        Assert.True(config.ReplicationConfig.AsyncEnabled);
    }

    [Fact]
    public void BuildCollectionConfig_WithoutReplicationConfig_UsesDefaults()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithGenerativeOpenAI>();

        // Assert - ReplicationConfig should have default values
        Assert.NotNull(config.ReplicationConfig);
        Assert.Equal(1, config.ReplicationConfig.Factor); // Default
        Assert.False(config.ReplicationConfig.AsyncEnabled); // Default
    }

    #endregion

    #region CollectionConfigMethod Tests

    [Fact]
    public void BuildCollectionConfig_WithCollectionConfigMethod_InvokesMethod()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithCollectionConfigMethod>();

        // Assert
        Assert.NotNull(config.InvertedIndexConfig);
        Assert.NotNull(config.InvertedIndexConfig.Bm25);
        Assert.Equal(1.5f, config.InvertedIndexConfig.Bm25.K1); // Set by ConfigMethod
        Assert.Equal(0.75f, config.InvertedIndexConfig.Bm25.B); // Set by ConfigMethod
    }

    [Fact]
    public void BuildCollectionConfig_WithCollectionConfigMethodClass_UsesTypeSafe()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithCollectionConfigMethodClass>();

        // Assert
        Assert.NotNull(config.InvertedIndexConfig);
        Assert.NotNull(config.InvertedIndexConfig.Bm25);
        Assert.Equal(2.0f, config.InvertedIndexConfig.Bm25.K1); // Set by external ConfigMethod
    }

    #endregion

    #region Combined Configuration Tests

    [Fact]
    public void BuildCollectionConfig_WithAllFeatures_ConfiguresCorrectly()
    {
        // Act
        var config = CollectionSchemaBuilder.FromClass<ArticleWithAllFeatures>();

        // Assert
        Assert.Equal("FullFeaturedArticles", config.Name);
        Assert.Equal("Articles with all features enabled", config.Description);

        // Generative
        Assert.NotNull(config.GenerativeConfig);
        Assert.IsType<GenerativeConfig.OpenAI>(config.GenerativeConfig);

        // Reranker
        Assert.NotNull(config.RerankerConfig);
        Assert.IsType<Reranker.Cohere>(config.RerankerConfig);

        // Sharding
        Assert.NotNull(config.ShardingConfig);
        Assert.Equal(2, config.ShardingConfig.DesiredCount);

        // Replication
        Assert.NotNull(config.ReplicationConfig);
        Assert.Equal(2, config.ReplicationConfig.Factor);
    }

    #endregion

    #region Test Classes

    [WeaviateCollection("Articles")]
    [Generative<GenerativeConfig.OpenAI>(Model = "gpt-4", MaxTokens = 500, Temperature = 0.7)]
    private class ArticleWithGenerativeOpenAI
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection("Articles")]
    [Generative<GenerativeConfig.Anthropic>(Model = "claude-3-5-sonnet-20241022", MaxTokens = 4096)]
    private class ArticleWithGenerativeAnthropic
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection("Articles")]
    [Generative<GenerativeConfig.OpenAI>(
        Model = "gpt-4",
        ConfigMethod = nameof(ConfigureGenerative)
    )]
    private class ArticleWithGenerativeConfigMethod
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;

        public static GenerativeConfig.OpenAI ConfigureGenerative(GenerativeConfig.OpenAI prebuilt)
        {
            prebuilt.Temperature = 0.9;
            prebuilt.MaxTokens = 1000;
            return prebuilt;
        }
    }

    [WeaviateCollection("Articles")]
    [Generative<GenerativeConfig.Cohere>(
        Model = "command",
        ConfigMethod = nameof(ModuleConfigurations.ConfigureCohere),
        ConfigMethodClass = typeof(ModuleConfigurations)
    )]
    private class ArticleWithGenerativeConfigMethodClass
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection("Articles")]
    [Reranker<Reranker.Cohere>(Model = "rerank-english-v2.0")]
    private class ArticleWithRerankerCohere
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection("Articles")]
    [Reranker<Reranker.VoyageAI>(Model = "rerank-2.5")]
    private class ArticleWithRerankerVoyageAI
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection("Articles")]
    [Reranker<Reranker.Transformers>]
    private class ArticleWithRerankerTransformers
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection(
        "Articles",
        ShardingDesiredCount = 3,
        ShardingVirtualPerPhysical = 256,
        ShardingKey = "customKey"
    )]
    private class ArticleWithShardingConfig
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection("Articles", ReplicationFactor = 3, ReplicationAsyncEnabled = true)]
    private class ArticleWithReplicationConfig
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection("Articles", CollectionConfigMethod = nameof(CustomizeConfig))]
    private class ArticleWithCollectionConfigMethod
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;

        public static CollectionConfig CustomizeConfig(CollectionConfig prebuilt)
        {
            prebuilt.InvertedIndexConfig = new InvertedIndexConfig
            {
                Bm25 = new BM25Config { K1 = 1.5f, B = 0.75f },
            };
            return prebuilt;
        }
    }

    [WeaviateCollection(
        "Articles",
        CollectionConfigMethod = nameof(CollectionConfigurations.CustomizeArticles),
        ConfigMethodClass = typeof(CollectionConfigurations)
    )]
    private class ArticleWithCollectionConfigMethodClass
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;
    }

    [WeaviateCollection(
        "FullFeaturedArticles",
        Description = "Articles with all features enabled",
        ShardingDesiredCount = 2,
        ReplicationFactor = 2
    )]
    [Generative<GenerativeConfig.OpenAI>(Model = "gpt-4")]
    [Reranker<Reranker.Cohere>(Model = "rerank-english-v2.0")]
    private class ArticleWithAllFeatures
    {
        [Property(DataType.Text)]
        public string Title { get; set; } = string.Empty;

        [Property(DataType.Text)]
        public string Content { get; set; } = string.Empty;
    }

    #endregion
}

/// <summary>
/// External configuration class for testing module configs
/// </summary>
public static class ModuleConfigurations
{
    public static GenerativeConfig.Cohere ConfigureCohere(GenerativeConfig.Cohere prebuilt)
    {
        prebuilt.Model = "command-r-plus";
        prebuilt.Temperature = 2.0;
        return prebuilt;
    }
}

/// <summary>
/// External configuration class for testing collection configs
/// </summary>
public static class CollectionConfigurations
{
    public static CollectionConfig CustomizeArticles(CollectionConfig prebuilt)
    {
        prebuilt.InvertedIndexConfig = new InvertedIndexConfig
        {
            Bm25 = new BM25Config { K1 = 2.0f, B = 0.5f },
        };
        return prebuilt;
    }
}

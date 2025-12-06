using Weaviate.Client.Models;
using Weaviate.Client.Orm.Attributes;
using Weaviate.Client.Orm.Schema;
using Xunit;

namespace Weaviate.Client.Orm.Tests.Schema;

public class VectorConfigBuilderTests
{
    #region Name Property Tests

    [Fact]
    public void BuildVectorConfigs_WithNameProperty_UsesCustomName()
    {
        // Act
        var vectorConfigs = VectorConfigBuilder.BuildVectorConfigs(typeof(ArticleWithNamedVector));

        // Assert
        Assert.NotNull(vectorConfigs);
        var configs = vectorConfigs.Values.ToList();
        Assert.Single(configs);
        Assert.Equal("custom_vector_name", configs[0].Name);
    }

    [Fact]
    public void BuildVectorConfigs_WithoutNameProperty_UsesPropertyName()
    {
        // Act
        var vectorConfigs = VectorConfigBuilder.BuildVectorConfigs(typeof(ArticleWithDefaultName));

        // Assert
        Assert.NotNull(vectorConfigs);
        var configs = vectorConfigs.Values.ToList();
        Assert.Single(configs);
        Assert.Equal("contentEmbedding", configs[0].Name); // camelCase conversion
    }

    #endregion

    #region ConfigMethod Tests

    [Fact]
    public void BuildVectorConfigs_WithConfigMethod_InvokesSameClassMethod()
    {
        // Act
        var vectorConfigs = VectorConfigBuilder.BuildVectorConfigs(typeof(ArticleWithConfigMethod));

        // Assert
        Assert.NotNull(vectorConfigs);
        var configs = vectorConfigs.Values.ToList();
        Assert.Single(configs);

        // The config method should have set Type = "text"
        var vectorizer = configs[0].Vectorizer as Vectorizer.Text2VecOpenAI;
        Assert.NotNull(vectorizer);
        Assert.Equal("text-embedding-3-small", vectorizer.Model);
        Assert.Equal("text", vectorizer.Type); // Set by config method
        Assert.False(vectorizer.VectorizeCollectionName); // Set by config method
    }

    [Fact]
    public void BuildVectorConfigs_WithConfigMethod_InvokesDifferentClassMethod()
    {
        // Act
        var vectorConfigs = VectorConfigBuilder.BuildVectorConfigs(
            typeof(ArticleWithExternalConfigMethod)
        );

        // Assert
        Assert.NotNull(vectorConfigs);
        var configs = vectorConfigs.Values.ToList();
        Assert.Single(configs);

        var vectorizer = configs[0].Vectorizer as Vectorizer.Text2VecOpenAI;
        Assert.NotNull(vectorizer);
        Assert.Equal("gpt-4-embedding", vectorizer.Model); // Changed by external config
    }

    [Fact]
    public void BuildVectorConfigs_WithConfigMethodClass_UsesTypeSafeApproach()
    {
        // Act
        var vectorConfigs = VectorConfigBuilder.BuildVectorConfigs(
            typeof(ArticleWithConfigMethodClass)
        );

        // Assert
        Assert.NotNull(vectorConfigs);
        var configs = vectorConfigs.Values.ToList();
        Assert.Single(configs);

        var vectorizer = configs[0].Vectorizer as Vectorizer.Text2VecOpenAI;
        Assert.NotNull(vectorizer);
        Assert.Equal("type-safe-model", vectorizer.Model); // Changed by type-safe config
        Assert.Equal("code", vectorizer.Type); // Set by config method
    }

    [Fact]
    public void BuildVectorConfigs_WithConfigMethod_ReceivesVectorName()
    {
        // Act
        var vectorConfigs = VectorConfigBuilder.BuildVectorConfigs(
            typeof(ArticleWithVectorNameInConfig)
        );

        // Assert
        Assert.NotNull(vectorConfigs);
        var configs = vectorConfigs.Values.ToList();
        Assert.Single(configs);

        // VectorNameCapturingConfig.LastVectorName is set by the config method
        Assert.Equal("myCustomVector", VectorNameCapturingConfig.LastVectorName);
    }

    [Fact]
    public void BuildVectorConfigs_WithInvalidConfigMethod_ThrowsException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VectorConfigBuilder.BuildVectorConfigs(typeof(ArticleWithInvalidConfigMethod))
        );

        Assert.Contains("Could not find static method", exception.Message);
    }

    [Fact]
    public void BuildVectorConfigs_WithWrongSignatureConfigMethod_ThrowsException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VectorConfigBuilder.BuildVectorConfigs(typeof(ArticleWithWrongSignatureConfigMethod))
        );

        Assert.Contains("invalid signature", exception.Message);
    }

    #endregion

    #region SelfProvided Validation Tests

    [Fact]
    public void BuildVectorConfigs_SelfProvidedWithNoConfig_Succeeds()
    {
        // Act
        var vectorConfigs = VectorConfigBuilder.BuildVectorConfigs(
            typeof(ArticleWithSelfProvidedVector)
        );

        // Assert
        Assert.NotNull(vectorConfigs);
        var configs = vectorConfigs.Values.ToList();
        Assert.Single(configs);
        Assert.IsType<Vectorizer.SelfProvided>(configs[0].Vectorizer);
    }

    [Fact]
    public void BuildVectorConfigs_SelfProvidedWithModel_ThrowsException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VectorConfigBuilder.BuildVectorConfigs(typeof(ArticleWithInvalidSelfProvidedConfig))
        );

        Assert.Contains("SelfProvided vectorizer should not have", exception.Message);
        Assert.Contains("Model", exception.Message);
    }

    [Fact]
    public void BuildVectorConfigs_SelfProvidedWithSourceProperties_ThrowsException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            VectorConfigBuilder.BuildVectorConfigs(
                typeof(ArticleWithSelfProvidedAndSourceProperties)
            )
        );

        Assert.Contains("SelfProvided vectorizer should not have", exception.Message);
        Assert.Contains("SourceProperties", exception.Message);
    }

    #endregion

    #region Test Classes

    private class ArticleWithNamedVector
    {
        [Vector<Vectorizer.Text2VecOpenAI>(
            Name = "custom_vector_name",
            Model = "text-embedding-ada-002"
        )]
        public float[]? ContentEmbedding { get; set; }
    }

    private class ArticleWithDefaultName
    {
        [Vector<Vectorizer.Text2VecOpenAI>(Model = "text-embedding-ada-002")]
        public float[]? ContentEmbedding { get; set; }
    }

    private class ArticleWithConfigMethod
    {
        [Vector<Vectorizer.Text2VecOpenAI>(
            Model = "text-embedding-3-small",
            ConfigMethod = nameof(ConfigureContentVector)
        )]
        public float[]? ContentEmbedding { get; set; }

        public static Vectorizer.Text2VecOpenAI ConfigureContentVector(
            string vectorName,
            Vectorizer.Text2VecOpenAI prebuilt
        )
        {
            // Model is already set from attribute
            prebuilt.Type = "text";
            prebuilt.VectorizeCollectionName = false;
            return prebuilt;
        }
    }

    private class ArticleWithExternalConfigMethod
    {
        [Vector<Vectorizer.Text2VecOpenAI>(
            Model = "text-embedding-3-small",
            ConfigMethod = "VectorConfigurations.ConfigureOpenAI"
        )]
        public float[]? ContentEmbedding { get; set; }
    }

    private class ArticleWithConfigMethodClass
    {
        [Vector<Vectorizer.Text2VecOpenAI>(
            Model = "text-embedding-3-small",
            ConfigMethod = nameof(VectorConfigurations.ConfigureTypeSafe),
            ConfigMethodClass = typeof(VectorConfigurations)
        )]
        public float[]? ContentEmbedding { get; set; }
    }

    private class ArticleWithVectorNameInConfig
    {
        [Vector<Vectorizer.Text2VecOpenAI>(
            Name = "myCustomVector",
            Model = "text-embedding-3-small",
            ConfigMethod = "VectorNameCapturingConfig.CaptureVectorName"
        )]
        public float[]? ContentEmbedding { get; set; }
    }

    private class ArticleWithInvalidConfigMethod
    {
        [Vector<Vectorizer.Text2VecOpenAI>(
            Model = "text-embedding-3-small",
            ConfigMethod = "NonExistentMethod"
        )]
        public float[]? ContentEmbedding { get; set; }
    }

    private class ArticleWithWrongSignatureConfigMethod
    {
        [Vector<Vectorizer.Text2VecOpenAI>(
            Model = "text-embedding-3-small",
            ConfigMethod = nameof(WrongSignature)
        )]
        public float[]? ContentEmbedding { get; set; }

        public static Vectorizer.Text2VecOpenAI WrongSignature(string vectorName)
        {
            // Missing second parameter
            return new Vectorizer.Text2VecOpenAI();
        }
    }

    private class ArticleWithSelfProvidedVector
    {
        [Vector<Vectorizer.SelfProvided>]
        public float[]? CustomEmbedding { get; set; }
    }

    private class ArticleWithInvalidSelfProvidedConfig
    {
        [Vector<Vectorizer.SelfProvided>(Model = "should-not-be-set")]
        public float[]? CustomEmbedding { get; set; }
    }

    private class ArticleWithSelfProvidedAndSourceProperties
    {
        [Vector<Vectorizer.SelfProvided>(SourceProperties = new[] { "Title" })]
        public float[]? CustomEmbedding { get; set; }
    }

    #endregion
}

/// <summary>
/// External configuration class for testing cross-class ConfigMethod
/// </summary>
public static class VectorConfigurations
{
    public static Vectorizer.Text2VecOpenAI ConfigureOpenAI(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt
    )
    {
        // Override model from attribute
        prebuilt.Model = "gpt-4-embedding";
        prebuilt.Type = "text";
        return prebuilt;
    }

    public static Vectorizer.Text2VecOpenAI ConfigureTypeSafe(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt
    )
    {
        // Type-safe configuration using ConfigMethodClass
        prebuilt.Model = "type-safe-model";
        prebuilt.Type = "code";
        return prebuilt;
    }
}

/// <summary>
/// Helper class to capture vector name in tests
/// </summary>
public static class VectorNameCapturingConfig
{
    public static string? LastVectorName { get; private set; }

    public static Vectorizer.Text2VecOpenAI CaptureVectorName(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt
    )
    {
        LastVectorName = vectorName;
        return prebuilt;
    }
}

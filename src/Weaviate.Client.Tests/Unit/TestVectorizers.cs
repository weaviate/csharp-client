using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Tests;

public partial class UnitTests
{
    [Fact]
    public void Test_VectorizerList_ImplicitConversion()
    {
        // Arrange
        var namedVectorConfig1 = NamedVectorConfig.New(
            "default",
            new Text2VecContextionaryConfig().ForProperties("name")
        );
        var namedVectorConfig2 = NamedVectorConfig.New(
            "fromSizes",
            new Text2VecWeaviateConfig().ForProperties("size")
        );

        // Act
        NamedVectorConfigList ncList = new[] { namedVectorConfig1, namedVectorConfig2 };
        Dictionary<string, VectorConfig> asDict = ncList;

        // Assert
        Assert.Equal(["default", "fromSizes"], asDict.Keys);
    }

    [Fact]
    public void Test_NamedVectorConfig_None_Has_Null_Properties()
    {
        // Arrange
        var vc = NamedVectorConfig.None();

        // Act
        var dto = vc.Vectorizer.ToDto();

        var json = JsonSerializer.Serialize(dto);

        // Assert
        Assert.DoesNotContain("properties", json);
    }

    [Fact]
    public void Test_NamedVectorConfig_None_Deserialization()
    {
        // Arrange

        // Act
        var config = VectorizerConfigFactory.Create("none", null);

        // Assert
        Assert.Null(config.Properties);
    }

    [Fact]
    public void Test_NamedVectorConfig_Has_Properties()
    {
        // Arrange
        var vc = NamedVectorConfig.New(
            "default",
            new Text2VecContextionaryConfig().ForProperties("name")
        );

        // Act
        var dto = vc.Vectorizer.ToDto();

        var json = JsonSerializer.Serialize(dto);

        // Assert
        Assert.Contains("properties", json);
    }
}

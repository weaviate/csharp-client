using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Tests;

public partial class UnitTests
{
    [Fact]
    public void Test_VectorConfigList()
    {
        // Arrange
        VectorConfigList ncList = new[]
        {
            new VectorConfig(
                "default",
                new Vectorizer.Text2VecContextionary { Properties = ["breed", "color"] },
                new VectorIndex.HNSW()
            ),
            new VectorConfig(
                "fromSizes",
                new Vectorizer.Text2VecContextionary { Properties = ["size"] }
            ),
            new VectorConfig(
                "location",
                new Vectorizer.Text2VecContextionary { Properties = ["location"] }
            ),
            new VectorConfig("nein", new Vectorizer.None()),
        };

        // Act

        // Assert
        Assert.Equal(["default", "fromSizes", "location", "nein"], ncList.Keys);
    }

    [Fact]
    public void Test_NamedVectorConfig_None_Has_No_Properties()
    {
        // Arrange
        var vc = new VectorConfig("default", new Vectorizer.None());

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;

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
        var defaultVec = new VectorConfig(
            "default",
            new Vectorizer.Text2VecContextionary() { Properties = ["name"] }
        );

        // Build explicitely, when typing as VectorConfig is needed,
        // like when accessing the Vectorizer property.
        // var vc = defaultVec.Build();

        // Cast implicitly, for passing as argument, will call Build transparently.
        VectorConfig vc = defaultVec;

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;

        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // Case-insensitive property matching
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Convert JSON names to PascalCase (C# convention)
                WriteIndented = false, // For testability
            }
        );

        // Assert
        Assert.Contains("\"properties\":[\"name\"]", json);
    }
}

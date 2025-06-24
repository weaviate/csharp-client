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
        VectorConfigList ncList = new[]
        {
            Vector.Name("default").With(new VectorizerConfig.Text2VecContextionary()).From("name"),
            Vector.Name("fromSizes").With(new VectorizerConfig.Text2VecWeaviate()).From("size"),
            Vector
                .Name("location")
                .With(new VectorizerConfig.Text2VecContextionary())
                .From("location"),
            Vector
                .Name("nein")
                .With(new VectorizerConfig.None())
                .With(new VectorIndexConfig.HNSW()),
            Vector.Name("built").With(new VectorizerConfig.None()).Build(),
        };

        // Act

        // Assert
        Assert.Equal(["default", "fromSizes", "location", "nein", "built"], ncList.Keys);
    }

    [Fact]
    public void Test_NamedVectorConfig_None_Has_No_Properties()
    {
        // Arrange
        var vc = Vector.Name(Vector.DefaultVectorName).With(new VectorizerConfig.None()).Build();

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
        var defaultVec = Vector
            .Name("default")
            .With(new VectorizerConfig.Text2VecContextionary())
            .From("name");

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

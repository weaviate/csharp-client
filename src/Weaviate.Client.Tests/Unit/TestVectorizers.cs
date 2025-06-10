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
        var namedVectorConfig1 = Vector.Text2VecContextionary(
            "default",
            new VectorizerConfig.Text2VecContextionary(),
            sourceProperties: ["name"]
        );
        var namedVectorConfig2 = Vector.Text2VecWeaviate(
            "fromSizes",
            new VectorizerConfig.Text2VecWeaviate(),
            null,
            "size"
        );
        var namedVectorConfig3 = Vector
            .Name("location")
            .With(new VectorizerConfig.Text2VecContextionary())
            .From("location");

        // Act
        VectorConfigList ncList = new[]
        {
            namedVectorConfig1,
            namedVectorConfig2,
            namedVectorConfig3,
            Vector
                .Name("nein")
                .With(new VectorizerConfig.None())
                .With(new VectorIndexConfig.HNSW()),
            Vector.Name("built").With(new VectorizerConfig.None()).Build(),
        };
        Dictionary<string, VectorConfig> asDict = ncList;

        // Assert
        Assert.Equal(["default", "fromSizes", "location", "nein", "built"], asDict.Keys);
    }

    [Fact]
    public void Test_NamedVectorConfig_None_Has_Null_Properties()
    {
        // Arrange
        // var vc = Configuration.None(Configuration.DefaultVectorName);
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
        // var vc = Vector.Text2VecContextionary(
        //     "default",
        //     new VectorizerConfig.Text2VecContextionary(),
        //     null,
        //     "name"
        // );

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
                WriteIndented = true, // For readability
            }
        );

        // Assert
        Assert.Contains("properties", json);
    }
}

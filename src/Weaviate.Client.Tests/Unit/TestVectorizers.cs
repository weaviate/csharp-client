using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;
using Quantizers = Weaviate.Client.Models.VectorIndex.Quantizers;

namespace Weaviate.Client.Tests.Unit;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1861:Avoid constant arrays as arguments",
    Justification = "<Pending>"
)]
public partial class VectorConfigListTests
{
    [Fact]
    public void Throws_When_FlatIndex_Already_Has_Quantizer()
    {
        var flat = new VectorIndex.Flat
        {
            Quantizer = new Quantizers.BQ { Cache = true, RescoreLimit = 10 },
        };
        var bq = new Quantizers.BQ { Cache = false, RescoreLimit = 5 };
        Assert.Throws<WeaviateClientException>(() =>
            Configure.Vector("flat-bq", v => v.SelfProvided(), index: flat, quantizer: bq)
        );
    }

    [Fact]
    public void Throws_When_HNSW_Already_Has_Quantizer()
    {
        var hnsw = new VectorIndex.HNSW
        {
            Quantizer = new Quantizers.BQ { Cache = true, RescoreLimit = 10 },
        };
        var bq = new Quantizers.BQ { Cache = false, RescoreLimit = 5 };
        Assert.Throws<WeaviateClientException>(() =>
            Configure.Vector("hnsw-bq", v => v.SelfProvided(), index: hnsw, quantizer: bq)
        );
    }

    [Fact]
    public void NamedVectorInitialization()
    {
        var v1 = new Vectors { { "default", new[] { 0.1f, 0.2f, 0.3f } } };

        // Act & Assert
        Assert.Equal([0.1f, 0.2f, 0.3f], v1["default"].Cast<float>());
    }

    [Fact]
    public void Test_VectorConfigList()
    {
        static VectorizerConfig transformerVectorizer(VectorizerFactory v) =>
            v.Text2VecTransformers();

        // Arrange
        VectorConfigList ncList = new[]
        {
            new VectorConfig(
                "default",
                new Vectorizer.Text2VecTransformers { SourceProperties = ["breed", "color"] },
                new VectorIndex.HNSW()
                {
                    Distance = VectorIndexConfig.VectorDistance.Cosine,
                    Quantizer = new Quantizers.PQ
                    {
                        Encoder = new Quantizers.PQ.EncoderConfig
                        {
                            Distribution = Quantizers.DistributionType.Normal,
                            Type = Quantizers.EncoderType.Kmeans,
                        },
                    },
                }
            ),
            new VectorConfig(
                "fromSizes",
                new Vectorizer.Text2VecTransformers { SourceProperties = ["size"] }
            ),
            new VectorConfig(
                "location",
                new Vectorizer.Text2VecTransformers { SourceProperties = ["location"] }
            ),
            new VectorConfig("nein", new Vectorizer.SelfProvided()),
            Configure.Vector("transf1", transformerVectorizer, sourceProperties: ["breed"]),
            Configure.Vector("transf2", transformerVectorizer, sourceProperties: ["color"]),
            Configure.Vector(
                "weaviate",
                v => v.Text2VecWeaviate(vectorizeCollectionName: true),
                sourceProperties: ["color"]
            ),
            Configure.Vector("neural", v => v.Img2VecNeural([]), sourceProperties: ["color"]),
        };

        // Act

        // Assert
        Assert.Equal(
            [
                "default",
                "fromSizes",
                "location",
                "nein",
                "neural",
                "transf1",
                "transf2",
                "weaviate",
            ],
            ncList.Keys
        );
    }

    [Fact]
    public void Test_NamedVectorConfig_SelfProvided_Has_No_Properties()
    {
        // Arrange
        var vc = new VectorConfig("default", new Vectorizer.SelfProvided());

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
        Assert.Null(config.SourceProperties);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_NamedVectorConfig_Has_Properties()
    {
        // Arrange
        var defaultVec = new VectorConfig(
            "default",
            new Vectorizer.Text2VecTransformers() { SourceProperties = ["name"] }
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

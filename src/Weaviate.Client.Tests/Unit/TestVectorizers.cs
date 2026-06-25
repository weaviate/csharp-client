using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;
using Quantizers = Weaviate.Client.Models.VectorIndex.Quantizers;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The vector config list tests class
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1861:Avoid constant arrays as arguments",
    Justification = "<Pending>"
)]
public partial class VectorConfigListTests
{
    /// <summary>
    /// Tests that throws when flat index already has quantizer
    /// </summary>
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

    /// <summary>
    /// Tests that throws when hnsw already has quantizer
    /// </summary>
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

    /// <summary>
    /// Tests that named vector initialization
    /// </summary>
    [Fact]
    public void NamedVectorInitialization()
    {
        var v1 = new Vectors { { "default", new[] { 0.1f, 0.2f, 0.3f } } };

        // Act & Assert
        Assert.Equal([0.1f, 0.2f, 0.3f], v1["default"].Cast<float>());
    }

    /// <summary>
    /// Tests that test vector config list
    /// </summary>
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

    /// <summary>
    /// Tests that test named vector config self provided has no properties
    /// </summary>
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

    /// <summary>
    /// Tests that test named vector config none deserialization
    /// </summary>
    [Fact]
    public void Test_NamedVectorConfig_None_Deserialization()
    {
        // Arrange

        // Act
        var config = VectorizerConfigFactory.Create("none", null);

        // Assert
        Assert.Null(config.SourceProperties);
    }

    /// <summary>
    /// Tests that test named vector config has properties
    /// </summary>
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

    /// <summary>
    /// Tests that Multi2MultiVecWeaviate serializes imageFields correctly
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2MultiVecWeaviate_Serializes_ImageFields()
    {
        // Arrange
        var vc = Configure.MultiVector(
            "default",
            v => v.Multi2MultiVecWeaviate(imageFields: new[] { "image" }, model: "my-model")
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"imageFields\"", json);
        Assert.Contains("\"image\"", json);
        Assert.Contains("\"my-model\"", json);
    }

    /// <summary>
    /// Tests that Multi2VecGoogle serializes audioFields correctly with string arrays
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecGoogle_Serializes_AudioFields_StringArray()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecGoogle(
                    projectId: "my-project",
                    location: "us-central1",
                    imageFields: new[] { "image" },
                    textFields: new[] { "text" },
                    videoFields: new[] { "video" },
                    audioFields: new[] { "audio" }
                )
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"audioFields\"", json);
        Assert.Contains("\"audio\"", json);
        Assert.Contains("\"imageFields\"", json);
        Assert.Contains("\"textFields\"", json);
        Assert.Contains("\"videoFields\"", json);
    }

    /// <summary>
    /// Tests that Multi2VecGoogle serializes audioFields correctly with WeightedFields
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecGoogle_Serializes_AudioFields_WeightedFields()
    {
        // Arrange
        var imageFields = new WeightedFields { ("image", 0.7) };
        var textFields = new WeightedFields { ("text", 0.8) };
        var videoFields = new WeightedFields { ("video", 0.6) };
        var audioFields = new WeightedFields { ("audio", 0.9) };

        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecGoogle(
                    projectId: "my-project",
                    location: "us-central1",
                    imageFields: imageFields,
                    textFields: textFields,
                    videoFields: videoFields,
                    audioFields: audioFields
                )
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"audioFields\"", json);
        Assert.Contains("\"audio\"", json);
        Assert.Contains("\"imageFields\"", json);
        Assert.Contains("\"textFields\"", json);
        Assert.Contains("\"videoFields\"", json);
    }

    /// <summary>
    /// Tests that Multi2VecGoogleGemini serializes audioFields correctly with string arrays
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecGoogleGemini_Serializes_AudioFields_StringArray()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecGoogleGemini(
                    imageFields: new[] { "image" },
                    textFields: new[] { "text" },
                    videoFields: new[] { "video" },
                    audioFields: new[] { "audio" }
                )
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"audioFields\"", json);
        Assert.Contains("\"audio\"", json);
        Assert.Contains("\"imageFields\"", json);
        Assert.Contains("\"textFields\"", json);
        Assert.Contains("\"videoFields\"", json);
    }

    /// <summary>
    /// Tests that Multi2VecGoogleGemini serializes audioFields correctly with WeightedFields
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecGoogleGemini_Serializes_AudioFields_WeightedFields()
    {
        // Arrange
        var imageFields = new WeightedFields { ("image", 0.7) };
        var textFields = new WeightedFields { ("text", 0.8) };
        var videoFields = new WeightedFields { ("video", 0.6) };
        var audioFields = new WeightedFields { ("audio", 0.9) };

        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecGoogleGemini(
                    imageFields: imageFields,
                    textFields: textFields,
                    videoFields: videoFields,
                    audioFields: audioFields
                )
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"audioFields\"", json);
        Assert.Contains("\"audio\"", json);
        Assert.Contains("\"imageFields\"", json);
        Assert.Contains("\"textFields\"", json);
        Assert.Contains("\"videoFields\"", json);
    }

    /// <summary>
    /// Tests that Text2VecDigitalOcean serializes baseURL and model correctly under the
    /// <c>text2vec-digitalocean</c> module key.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecDigitalOcean_Serializes_BaseURL_And_Model()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v =>
                v.Text2VecDigitalOcean(
                    model: "qwen3-embedding-0.6b",
                    baseURL: "https://inference.do-ai.run",
                    vectorizeCollectionName: false
                )
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"text2vec-digitalocean\"", json);
        Assert.Contains("\"baseURL\":\"https://inference.do-ai.run\"", json);
        Assert.Contains("\"model\":\"qwen3-embedding-0.6b\"", json);
        Assert.Contains("\"vectorizeClassName\":false", json);
    }

    /// <summary>
    /// Tests that Text2VecDigitalOcean omits unset optional fields so the server can apply
    /// its defaults (no <c>baseURL</c>). <c>model</c> is required by the factory so it is
    /// always present.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecDigitalOcean_Omits_Unset_BaseURL()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v => v.Text2VecDigitalOcean(model: "qwen3-embedding-0.6b")
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System
                    .Text
                    .Json
                    .Serialization
                    .JsonIgnoreCondition
                    .WhenWritingNull,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"text2vec-digitalocean\"", json);
        Assert.Contains("\"model\":\"qwen3-embedding-0.6b\"", json);
        Assert.DoesNotContain("\"baseURL\"", json);
    }

    /// <summary>
    /// Tests that Text2VecGoogle omits <c>location</c> when unset so the server can apply its
    /// default.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecGoogle_Omits_Unset_Location()
    {
        // Arrange
        var vc = Configure.Vector("default", v => v.Text2VecGoogleVertex(projectId: "my-project"));

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System
                    .Text
                    .Json
                    .Serialization
                    .JsonIgnoreCondition
                    .WhenWritingNull,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"text2vec-google\"", json);
        Assert.DoesNotContain("\"location\"", json);
    }

    /// <summary>
    /// Tests that Text2VecAWS serializes <c>dimensions</c> as a JSON number (not a string) when it
    /// is set via the Bedrock factory.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecAWS_Serializes_Dimensions_When_Set()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v =>
                v.Text2VecAWSBedrock(
                    region: "us-east-1",
                    model: "amazon.titan-embed-text-v2:0",
                    dimensions: 1024
                )
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System
                    .Text
                    .Json
                    .Serialization
                    .JsonIgnoreCondition
                    .WhenWritingNull,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"text2vec-aws\"", json);
        Assert.Contains("\"dimensions\":1024", json);
        Assert.DoesNotContain("\"dimensions\":\"1024\"", json);
    }

    /// <summary>
    /// Tests that Text2VecAWS omits <c>dimensions</c> when it is unset so the server can apply its
    /// default.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecAWS_Omits_Unset_Dimensions()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v => v.Text2VecAWSSagemaker(region: "us-east-1", endpoint: "my-endpoint")
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System
                    .Text
                    .Json
                    .Serialization
                    .JsonIgnoreCondition
                    .WhenWritingNull,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"text2vec-aws\"", json);
        Assert.DoesNotContain("\"dimensions\"", json);
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
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
    /// Tests that Multi2VecGoogle maps each string-array modality to its own key, and that the
    /// unweighted overload emits no <c>weights</c> object.
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
        Assert.Contains("\"multi2vec-palm\"", json);
        Assert.Contains("\"imageFields\":[\"image\"]", json);
        Assert.Contains("\"textFields\":[\"text\"]", json);
        Assert.Contains("\"videoFields\":[\"video\"]", json);
        Assert.Contains("\"audioFields\":[\"audio\"]", json);
        Assert.DoesNotContain("\"weights\"", json);
    }

    /// <summary>
    /// Tests that Multi2VecGoogle routes every modality's weights to that modality's key.
    /// The whole <c>weights</c> object is asserted, with a distinct value per modality and a
    /// distinct field count for video versus audio, because the factory calls
    /// <c>FromWeightedFields</c> — whose parameters run image, text, audio, depth, imu,
    /// thermal, video — and a positional call there silently files video weights under
    /// <c>audioFields</c> and audio weights under <c>depthFields</c>, a modality this module
    /// does not have. A substring-presence assertion cannot see that swap; an equality
    /// assertion on the whole object can.
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
        var imageFields = new WeightedFields { ("image", 0.11), ("thumbnail", 0.12) };
        var textFields = new WeightedFields { ("text", 0.21) };
        var videoFields = new WeightedFields { ("video", 0.31), ("clip", 0.32) };
        var audioFields = new WeightedFields { ("audio", 0.41) };

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
        Assert.Contains("\"multi2vec-palm\"", json);
        Assert.Contains("\"imageFields\":[\"image\",\"thumbnail\"]", json);
        Assert.Contains("\"textFields\":[\"text\"]", json);
        Assert.Contains("\"videoFields\":[\"video\",\"clip\"]", json);
        Assert.Contains("\"audioFields\":[\"audio\"]", json);
        // Every weight lands under its own modality, and no modality the module does not
        // support (depth, imu, thermal) appears.
        Assert.Contains(
            "\"weights\":{\"audioFields\":[0.41],\"imageFields\":[0.11,0.12],"
                + "\"textFields\":[0.21],\"videoFields\":[0.31,0.32]}",
            json
        );
        Assert.DoesNotContain("depthFields", json);
        Assert.DoesNotContain("imuFields", json);
        Assert.DoesNotContain("thermalFields", json);
    }

    /// <summary>
    /// Tests that Multi2VecGoogleGemini maps each string-array modality to its own key, and
    /// that the unweighted overload emits no <c>weights</c> object.
    /// Also pins the module name: there is no <c>multi2vec-google-gemini</c> module on any
    /// server, so the Gemini factory must emit the <c>multi2vec-google</c> module (under its
    /// <c>multi2vec-palm</c> wire name) and select Gemini with <c>apiEndpoint</c> instead —
    /// and must send neither <c>projectId</c> nor <c>location</c>, which are Vertex-only.
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
                    audioFields: new[] { "audio" },
                    dimensions: 512
                )
        );

        // Act
        var dto = vc.Vectorizer?.ToDto() ?? default;
        var json = JsonSerializer.Serialize(
            dto,
            // Mirrors WeaviateRestClient.RestJsonSerializerOptions, so what is absent here is
            // absent on the wire.
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"multi2vec-palm\"", json);
        Assert.DoesNotContain("multi2vec-google-gemini", json);
        Assert.Contains("\"apiEndpoint\":\"generativelanguage.googleapis.com\"", json);
        // Vertex-only settings the Gemini API has no equivalent of; neither may be sent.
        Assert.DoesNotContain("projectId", json);
        Assert.DoesNotContain("location", json);
        Assert.Contains("\"imageFields\":[\"image\"]", json);
        Assert.Contains("\"textFields\":[\"text\"]", json);
        Assert.Contains("\"videoFields\":[\"video\"]", json);
        Assert.Contains("\"audioFields\":[\"audio\"]", json);
        Assert.Contains("\"dimensions\":512", json);
        // Inherited shipped API on Multi2VecGoogle, but this factory never sets it.
        Assert.DoesNotContain("vectorizeClassName", json);
        Assert.DoesNotContain("\"weights\"", json);
    }

    /// <summary>
    /// Tests that Multi2VecGoogleGemini routes every modality's weights to that modality's
    /// key. Asserted as a whole <c>weights</c> object with a distinct value per modality, for
    /// the same reason as the Multi2VecGoogle case above: this factory shares the
    /// <c>FromWeightedFields</c> parameter order in which audio precedes video.
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
        var imageFields = new WeightedFields { ("image", 0.13), ("thumbnail", 0.14) };
        var textFields = new WeightedFields { ("text", 0.23) };
        var videoFields = new WeightedFields { ("video", 0.33), ("clip", 0.34) };
        var audioFields = new WeightedFields { ("audio", 0.43) };

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
            // Mirrors WeaviateRestClient.RestJsonSerializerOptions, so what is absent here is
            // absent on the wire.
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
            }
        );

        // Assert
        Assert.Contains("\"multi2vec-palm\"", json);
        Assert.DoesNotContain("multi2vec-google-gemini", json);
        Assert.Contains("\"apiEndpoint\":\"generativelanguage.googleapis.com\"", json);
        Assert.Contains("\"imageFields\":[\"image\",\"thumbnail\"]", json);
        Assert.Contains("\"textFields\":[\"text\"]", json);
        Assert.Contains("\"videoFields\":[\"video\",\"clip\"]", json);
        Assert.Contains("\"audioFields\":[\"audio\"]", json);
        Assert.Contains(
            "\"weights\":{\"audioFields\":[0.43],\"imageFields\":[0.13,0.14],"
                + "\"textFields\":[0.23],\"videoFields\":[0.33,0.34]}",
            json
        );
        // dimensions is left unset by this case, so the omit-when-null path stays covered;
        // vectorizeClassName is never settable through this factory at all.
        Assert.DoesNotContain("dimensions", json);
        Assert.DoesNotContain("vectorizeClassName", json);
        Assert.DoesNotContain("depthFields", json);
        Assert.DoesNotContain("imuFields", json);
        Assert.DoesNotContain("thermalFields", json);
    }

    /// <summary>
    /// Tests that Multi2VecBind routes all seven modalities' weights to their own keys. This
    /// is the widest <c>FromWeightedFields</c> call in the client, so every weight gets its own
    /// value: any transposition between neighbouring modalities shows up as a wrong number
    /// rather than as a still-present key.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecBind_WeightedFields_Overload_Serializes_All_Modality_Weights()
    {
        // Arrange
        var imageFields = new WeightedFields { ("image", 0.11) };
        var textFields = new WeightedFields { ("text", 0.21) };
        var audioFields = new WeightedFields { ("audio", 0.31) };
        var depthFields = new WeightedFields { ("depth", 0.41) };
        var imuFields = new WeightedFields { ("imu", 0.51) };
        var thermalFields = new WeightedFields { ("thermal", 0.61) };
        var videoFields = new WeightedFields { ("video", 0.71) };

        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecBind(
                    imageFields: imageFields,
                    textFields: textFields,
                    audioFields: audioFields,
                    depthFields: depthFields,
                    imuFields: imuFields,
                    thermalFields: thermalFields,
                    videoFields: videoFields
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
        Assert.Contains("\"multi2vec-bind\"", json);
        Assert.Contains("\"imageFields\":[\"image\"]", json);
        Assert.Contains("\"textFields\":[\"text\"]", json);
        Assert.Contains("\"audioFields\":[\"audio\"]", json);
        Assert.Contains("\"depthFields\":[\"depth\"]", json);
        Assert.Contains("\"imuFields\":[\"imu\"]", json);
        Assert.Contains("\"thermalFields\":[\"thermal\"]", json);
        Assert.Contains("\"videoFields\":[\"video\"]", json);
        Assert.Contains(
            "\"weights\":{\"audioFields\":[0.31],\"depthFields\":[0.41],"
                + "\"imageFields\":[0.11],\"imuFields\":[0.51],\"textFields\":[0.21],"
                + "\"thermalFields\":[0.61],\"videoFields\":[0.71]}",
            json
        );
    }

    /// <summary>
    /// Tests the configuration Multi2VecBind's weighted overload forces on every caller: all
    /// seven modalities are required parameters, so an unused one has to be passed as an empty
    /// <c>WeightedFields</c>. Each empty modality must vanish from the payload — both its field
    /// name list and its weight array — because the server rejects a modality key that is
    /// present but empty. Without that, the overload cannot create anything but a collection
    /// that uses all seven modalities.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecBind_WeightedFields_Overload_Omits_Empty_Modalities()
    {
        // Arrange — only image and audio are in use; the other five are unavoidably empty.
        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecBind(
                    imageFields: new WeightedFields { ("image", 0.11) },
                    textFields: new WeightedFields(),
                    audioFields: new WeightedFields { ("audio", 0.31) },
                    depthFields: new WeightedFields(),
                    imuFields: new WeightedFields(),
                    thermalFields: new WeightedFields(),
                    videoFields: new WeightedFields()
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
        var wireJson = JsonSerializer.Serialize(
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
        Assert.Contains("\"multi2vec-bind\"", json);
        Assert.Contains("\"weights\":{\"audioFields\":[0.31],\"imageFields\":[0.11]}", json);
        Assert.DoesNotContain("\"textFields\":[]", json);
        Assert.DoesNotContain("\"depthFields\":[]", json);
        Assert.DoesNotContain("\"imuFields\":[]", json);
        Assert.DoesNotContain("\"thermalFields\":[]", json);
        Assert.DoesNotContain("\"videoFields\":[]", json);
        Assert.Contains("\"imageFields\":[\"image\"]", wireJson);
        Assert.Contains("\"audioFields\":[\"audio\"]", wireJson);
        Assert.DoesNotContain("\"textFields\"", wireJson);
        Assert.DoesNotContain("\"depthFields\"", wireJson);
        Assert.DoesNotContain("\"imuFields\"", wireJson);
        Assert.DoesNotContain("\"thermalFields\"", wireJson);
        Assert.DoesNotContain("\"videoFields\"", wireJson);
    }

    /// <summary>
    /// Tests that Multi2VecVoyageAI routes its three modalities' weights to their own keys.
    /// Video is the modality at risk here: it is the last <c>FromWeightedFields</c> parameter,
    /// so a positional third argument would land it in <c>audioFields</c>, which this module
    /// does not support.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecVoyageAI_WeightedFields_Overload_Serializes_All_Modality_Weights()
    {
        // Arrange
        var imageFields = new WeightedFields { ("image", 0.15) };
        var textFields = new WeightedFields { ("text", 0.25), ("caption", 0.26) };
        var videoFields = new WeightedFields { ("video", 0.35) };

        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecVoyageAI(
                    imageFields: imageFields,
                    textFields: textFields,
                    videoFields: videoFields
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
        Assert.Contains("\"multi2vec-voyageai\"", json);
        Assert.Contains("\"imageFields\":[\"image\"]", json);
        Assert.Contains("\"textFields\":[\"text\",\"caption\"]", json);
        Assert.Contains("\"videoFields\":[\"video\"]", json);
        Assert.Contains(
            "\"weights\":{\"imageFields\":[0.15],\"textFields\":[0.25,0.26],"
                + "\"videoFields\":[0.35]}",
            json
        );
        Assert.DoesNotContain("audioFields", json);
        Assert.DoesNotContain("depthFields", json);
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

    /// <summary>
    /// Tests that Multi2VecTwelveLabs serializes all fields correctly under the
    /// <c>multi2vec-twelvelabs</c> module key, and that the unweighted overload emits no
    /// <c>weights</c> key at all (asserted without <c>DefaultIgnoreCondition</c>, so a missing
    /// per-property ignore condition would show up as <c>"weights":null</c>).
    /// Every field the record exposes is set here, so the absence of <c>vectorizeClassName</c>
    /// pins that the client never sends that key: the setting is a no-op for multi2vec modules,
    /// and sending it makes a caller believe a value they chose took effect. Serializing without
    /// <c>DefaultIgnoreCondition</c> means re-adding the property would surface here even if it
    /// were left null.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecTwelveLabs_Serializes_All_Fields()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecTwelveLabs(
                    imageFields: new[] { "image" },
                    textFields: new[] { "text" },
                    baseURL: "https://api.twelvelabs.io/v1.3",
                    model: "marengo3.0"
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
        Assert.Contains("\"multi2vec-twelvelabs\"", json);
        Assert.Contains("\"baseURL\":\"https://api.twelvelabs.io/v1.3\"", json);
        Assert.Contains("\"imageFields\":[\"image\"]", json);
        Assert.Contains("\"model\":\"marengo3.0\"", json);
        Assert.Contains("\"textFields\":[\"text\"]", json);
        // vectorizeClassName does nothing in a multi2vec module, so the client does not offer it
        // and never puts it on the wire, not even as null.
        Assert.DoesNotContain("vectorizeClassName", json);
        Assert.DoesNotContain("\"weights\"", json);
    }

    /// <summary>
    /// Tests that Multi2VecTwelveLabs omits unset optional fields so the server can apply
    /// its defaults (no <c>baseURL</c> and no <c>model</c>), and that <c>vectorizeClassName</c>
    /// is absent on the wire shape too — it is not part of this vectorizer's surface at all.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecTwelveLabs_Omits_Unset_Optional_Fields()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v => v.Multi2VecTwelveLabs(imageFields: new[] { "image" }, textFields: new[] { "text" })
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
        Assert.Contains("\"multi2vec-twelvelabs\"", json);
        Assert.Contains("\"imageFields\":[\"image\"]", json);
        Assert.Contains("\"textFields\":[\"text\"]", json);
        Assert.DoesNotContain("\"baseURL\"", json);
        Assert.DoesNotContain("\"model\"", json);
        Assert.DoesNotContain("\"vectorizeClassName\"", json);
    }

    /// <summary>
    /// Tests that the Multi2VecTwelveLabs WeightedFields overload maps the field names into
    /// the <c>imageFields</c> and <c>textFields</c> arrays and emits the matching
    /// <c>weights</c> object, with each weight array in the same order as its field list.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecTwelveLabs_WeightedFields_Overload_Maps_Field_Names_And_Weights()
    {
        // Arrange
        var imageFields = new WeightedFields { ("image", 0.7), ("thumbnail", 0.2) };
        var textFields = new WeightedFields { ("text", 0.3) };

        var vc = Configure.Vector(
            "default",
            v => v.Multi2VecTwelveLabs(imageFields: imageFields, textFields: textFields)
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
        Assert.Contains("\"multi2vec-twelvelabs\"", json);
        Assert.Contains("\"imageFields\":[\"image\",\"thumbnail\"]", json);
        Assert.Contains("\"textFields\":[\"text\"]", json);
        // The weights the caller supplied reach the wire, per modality, in field order.
        Assert.Contains("\"weights\":{\"imageFields\":[0.7,0.2],\"textFields\":[0.3]}", json);
    }

    /// <summary>
    /// Tests that a modality whose weighted field collection is empty drops out of the payload
    /// completely: no weight array, and no field name list either. The server rejects a
    /// modality key that is present but empty (<c>must contain at least one text field name in
    /// textFields</c>), so emitting <c>"textFields":[]</c> would make this image-only
    /// configuration impossible to create.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecTwelveLabs_WeightedFields_Overload_Omits_Empty_Modality_Weights()
    {
        // Arrange
        var imageFields = new WeightedFields { ("image", 0.7) };
        var textFields = new WeightedFields();

        var vc = Configure.Vector(
            "default",
            v => v.Multi2VecTwelveLabs(imageFields: imageFields, textFields: textFields)
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
        // The REST client serializes with WhenWritingNull, so the wire shape is the one that
        // decides whether the server sees a textFields key at all.
        var wireJson = JsonSerializer.Serialize(
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
        Assert.Contains("\"multi2vec-twelvelabs\"", json);
        Assert.Contains("\"weights\":{\"imageFields\":[0.7]}", json);
        // The empty modality contributes no field name list, not an empty one.
        Assert.DoesNotContain("\"textFields\":[]", json);
        Assert.Contains("\"imageFields\":[\"image\"]", wireJson);
        Assert.DoesNotContain("\"textFields\"", wireJson);
    }

    /// <summary>
    /// Tests that when every modality is empty the <c>weights</c> key is dropped entirely
    /// rather than serialized as an empty object, and that no modality key is emitted either:
    /// the payload carries the module key alone and the server decides what to make of it.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecTwelveLabs_WeightedFields_Overload_Omits_Weights_When_All_Empty()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v =>
                v.Multi2VecTwelveLabs(
                    imageFields: new WeightedFields(),
                    textFields: new WeightedFields()
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
        var wireJson = JsonSerializer.Serialize(
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
        Assert.Contains("\"multi2vec-twelvelabs\"", json);
        Assert.DoesNotContain("\"weights\"", json);
        Assert.DoesNotContain("\"imageFields\"", wireJson);
        Assert.DoesNotContain("\"textFields\"", wireJson);
    }

    /// <summary>
    /// Tests that the string-array overload treats an empty array exactly like an empty
    /// <c>WeightedFields</c>: the modality is omitted rather than emitted as <c>[]</c>, which
    /// the server rejects.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecTwelveLabs_StringArray_Overload_Omits_Empty_Modality()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v => v.Multi2VecTwelveLabs(imageFields: ["image"], textFields: [])
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
        var wireJson = JsonSerializer.Serialize(
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
        Assert.Contains("\"multi2vec-twelvelabs\"", json);
        Assert.DoesNotContain("\"textFields\":[]", json);
        Assert.DoesNotContain("\"weights\"", json);
        Assert.Contains("\"imageFields\":[\"image\"]", wireJson);
        Assert.DoesNotContain("\"textFields\"", wireJson);
    }

    /// <summary>
    /// Tests that the weights payload is not specific to Multi2VecTwelveLabs: the same
    /// internal <c>Weights</c> property on every multi2vec record now reaches the wire, pinned
    /// here on the Multi2VecClip sibling.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecClip_WeightedFields_Overload_Serializes_Weights()
    {
        // Arrange
        var imageFields = new WeightedFields { ("image", 0.9) };
        var textFields = new WeightedFields { ("title", 0.6), ("body", 0.4) };

        var vc = Configure.Vector(
            "default",
            v => v.Multi2VecClip(imageFields: imageFields, textFields: textFields)
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
        Assert.Contains("\"multi2vec-clip\"", json);
        Assert.Contains("\"imageFields\":[\"image\"]", json);
        Assert.Contains("\"textFields\":[\"title\",\"body\"]", json);
        Assert.Contains("\"weights\":{\"imageFields\":[0.9],\"textFields\":[0.6,0.4]}", json);
    }

    /// <summary>
    /// Tests that the Multi2VecClip string-array overload, which supplies no weights, emits no
    /// <c>weights</c> key.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Multi2VecClip_StringArray_Overload_Omits_Weights()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v => v.Multi2VecClip(imageFields: new[] { "image" }, textFields: new[] { "text" })
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
        Assert.Contains("\"multi2vec-clip\"", json);
        Assert.DoesNotContain("\"weights\"", json);
    }

    /// <summary>
    /// Tests that Multi2VecTwelveLabs deserializes from the <c>multi2vec-twelvelabs</c>
    /// module configuration returned by the server. The server stamps its own
    /// <c>vectorizeClassName</c> default into the stored config even though the module never
    /// reads it, so the payload here includes that key: the client must ignore it rather than
    /// fail, since it no longer models the setting.
    /// </summary>
    [Fact]
    public void Test_Multi2VecTwelveLabs_Deserialization()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["baseURL"] = "https://api.twelvelabs.io/v1.3",
            ["imageFields"] = new[] { "image" },
            ["model"] = "marengo3.0",
            ["textFields"] = new[] { "text" },
            // Server-supplied and unmodelled; present to prove it is tolerated, not mapped.
            ["vectorizeClassName"] = false,
        };

        // Act
        var config = VectorizerConfigFactory.Create("multi2vec-twelvelabs", parameters);

        // Assert
        var twelveLabs = Assert.IsType<Vectorizer.Multi2VecTwelveLabs>(config);
        Assert.Equal("multi2vec-twelvelabs", twelveLabs.Identifier);
        Assert.Equal("https://api.twelvelabs.io/v1.3", twelveLabs.BaseURL);
        Assert.NotNull(twelveLabs.ImageFields);
        Assert.Equal(["image"], twelveLabs.ImageFields);
        Assert.Equal("marengo3.0", twelveLabs.Model);
        Assert.NotNull(twelveLabs.TextFields);
        Assert.Equal(["text"], twelveLabs.TextFields);
    }

    /// <summary>
    /// Tests that Text2VecOpenAI serializes <c>endpoint</c> when it is set.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecOpenAI_Serializes_Endpoint_When_Set()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v => v.Text2VecOpenAI(model: "text-embedding-3-small", endpoint: "/v2/embeddings")
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
        Assert.Contains("\"text2vec-openai\"", json);
        Assert.Contains("\"endpoint\":\"/v2/embeddings\"", json);
    }

    /// <summary>
    /// Tests that Text2VecOpenAI omits <c>endpoint</c> when unset so the server can apply its
    /// default.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecOpenAI_Omits_Unset_Endpoint()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v => v.Text2VecOpenAI(model: "text-embedding-3-small")
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
        Assert.Contains("\"text2vec-openai\"", json);
        Assert.DoesNotContain("\"endpoint\"", json);
    }

    /// <summary>
    /// Tests that Text2VecMorph serializes <c>endpoint</c> when it is set.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecMorph_Serializes_Endpoint_When_Set()
    {
        // Arrange
        var vc = Configure.Vector(
            "default",
            v => v.Text2VecMorph(model: "morph-embedding-v2", endpoint: "/v2/embeddings")
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
        Assert.Contains("\"text2vec-morph\"", json);
        Assert.Contains("\"endpoint\":\"/v2/embeddings\"", json);
    }

    /// <summary>
    /// Tests that Text2VecMorph omits <c>endpoint</c> when unset so the server can apply its
    /// default.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>"
    )]
    public void Test_Text2VecMorph_Omits_Unset_Endpoint()
    {
        // Arrange
        var vc = Configure.Vector("default", v => v.Text2VecMorph(model: "morph-embedding-v2"));

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
        Assert.Contains("\"text2vec-morph\"", json);
        Assert.DoesNotContain("\"endpoint\"", json);
    }
}

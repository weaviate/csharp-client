using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The vector index config tests class
/// </summary>
public partial class VectorIndexConfigTests
{
    /// <summary>
    /// Tests that vector index config hnsw from json
    /// </summary>
    /// <param name="bqEnabled">The bq enabled</param>
    /// <param name="pqEnabled">The pq enabled</param>
    /// <param name="sqEnabled">The sq enabled</param>
    /// <param name="expectedQuantizer">The expected quantizer</param>
    [Theory]
    [InlineData(true, false, false, "bq")]
    [InlineData(false, true, false, "pq")]
    [InlineData(false, false, true, "sq")]
    public void VectorIndexConfig_HNSW_From_Json(
        bool bqEnabled,
        bool pqEnabled,
        bool sqEnabled,
        string expectedQuantizer
    )
    {
        var json =
            $@"{{ ""skip"": false, ""cleanupIntervalSeconds"": 300, ""maxConnections"": 32, ""efConstruction"": 128, ""ef"": -1, ""dynamicEfMin"": 100, ""dynamicEfMax"": 500, ""dynamicEfFactor"": 8, ""vectorCacheMaxObjects"": 1000000000000, ""flatSearchCutoff"": 40000, ""distance"": ""cosine"", ""pq"": {{ ""enabled"": {pqEnabled.ToString().ToLower()}, ""bitCompression"": false, ""segments"": 0, ""centroids"": 256, ""trainingLimit"": 100000, ""encoder"": {{ ""type"": ""kmeans"", ""distribution"": ""log-normal"" }} }}, ""bq"": {{ ""enabled"": {bqEnabled.ToString().ToLower()} }}, ""sq"": {{ ""enabled"": {sqEnabled.ToString().ToLower()}, ""trainingLimit"": 100000, ""rescoreLimit"": 20 }}, ""filterStrategy"": ""sweeping"", ""multivector"": {{ ""enabled"": false, ""muvera"": {{ ""enabled"": false, ""ksim"": 4, ""dprojections"": 16, ""repetitions"": 10 }}, ""aggregation"": ""maxSim"" }} }}";

        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        var hnsw = (VectorIndex.HNSW?)VectorIndexSerialization.Factory("hnsw", config);

        Assert.NotNull(hnsw);
        Assert.NotNull(hnsw?.Quantizer);
        Assert.Equal(expectedQuantizer, hnsw?.Quantizer.Type);
        Assert.Null(hnsw!.MultiVector);
    }

    /// <summary>
    /// Tests that vector index config sets quantizer when provided
    /// </summary>
    [Fact]
    public void VectorIndexConfig_Sets_Quantizer_When_Provided()
    {
        // Arrange
        var quantizer = new VectorIndex.Quantizers.BQ();

        var config = new VectorIndex.HNSW { Ef = 100, MaxConnections = 16 };

        // Act & Assert
        var vectorConfig = Configure.Vector(
            "regular",
            v => v.SelfProvided(),
            index: config,
            quantizer: quantizer
        );

        Assert.Null(config.Quantizer);

        Assert.NotNull(vectorConfig);
        Assert.Equal("regular", vectorConfig.Name);
        Assert.IsType<VectorIndex.HNSW>(vectorConfig.VectorIndexConfig);
        Assert.NotNull((vectorConfig.VectorIndexConfig as VectorIndex.HNSW)?.Quantizer);
        Assert.IsType<VectorIndex.Quantizers.BQ>(
            (vectorConfig.VectorIndexConfig as VectorIndex.HNSW)?.Quantizer
        );
    }

    /// <summary>
    /// Tests that vector index config none quantizer sets skip default quantization
    /// </summary>
    [Fact]
    public void VectorIndexConfig_None_Quantizer_Sets_SkipDefaultQuantization()
    {
        // Arrange
        var hnsw = new VectorIndex.HNSW { Quantizer = new VectorIndex.Quantizers.None() };

        // Act - serialize to DTO
        var dto = VectorIndexSerialization.ToDto(hnsw);
        var json = JsonSerializer.Serialize(
            dto,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        // Assert
        Assert.NotNull(deserialized);
        Assert.True(deserialized.ContainsKey("skipDefaultQuantization"));
        Assert.True(deserialized["skipDefaultQuantization"].ToString() == "True");
    }

    /// <summary>
    /// Tests that vector index config skip default quantization deserializes to none quantizer
    /// </summary>
    [Fact]
    public void VectorIndexConfig_SkipDefaultQuantization_Deserializes_To_None_Quantizer()
    {
        // Arrange
        var json = @"{ ""skipDefaultQuantization"": true, ""distance"": ""cosine"" }";
        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        // Act
        var hnsw = (VectorIndex.HNSW?)VectorIndexSerialization.Factory("hnsw", config);

        // Assert
        Assert.NotNull(hnsw);
        Assert.NotNull(hnsw?.Quantizer);
        Assert.Equal("none", hnsw?.Quantizer.Type);
        Assert.IsType<VectorIndex.Quantizers.None>(hnsw?.Quantizer);
    }

    /// <summary>
    /// Tests that vector index config hfresh deserializes from json
    /// </summary>
    [Fact]
    public void VectorIndexConfig_HFresh_From_Json()
    {
        var json =
            @"{ ""distance"": ""cosine"", ""maxPostingSizeKB"": 256, ""replicas"": 4, ""searchProbe"": 64 }";

        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        var hfresh = (VectorIndex.HFresh?)VectorIndexSerialization.Factory("hfresh", config);

        Assert.NotNull(hfresh);
        Assert.Equal(VectorIndexConfig.VectorDistance.Cosine, hfresh?.Distance);
        Assert.Equal(256, hfresh?.MaxPostingSizeKb);
        Assert.Equal(4, hfresh?.Replicas);
        Assert.Equal(64, hfresh?.SearchProbe);
        Assert.Null(hfresh?.Quantizer);
        Assert.Null(hfresh?.MultiVector);
    }

    /// <summary>
    /// Tests that vector index config hfresh roundtrips through serialization
    /// </summary>
    [Fact]
    public void VectorIndexConfig_HFresh_Roundtrip()
    {
        var original = new VectorIndex.HFresh
        {
            Distance = VectorIndexConfig.VectorDistance.Dot,
            MaxPostingSizeKb = 512,
            Replicas = 8,
            SearchProbe = 128,
        };

        // Serialize to DTO → JSON → deserialize back
        var json = VectorIndexSerialization.SerializeHFresh(original);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        var roundtripped = (VectorIndex.HFresh?)VectorIndexSerialization.Factory("hfresh", dict);

        Assert.NotNull(roundtripped);
        Assert.Equal(original.Distance, roundtripped?.Distance);
        Assert.Equal(original.MaxPostingSizeKb, roundtripped?.MaxPostingSizeKb);
        Assert.Equal(original.Replicas, roundtripped?.Replicas);
        Assert.Equal(original.SearchProbe, roundtripped?.SearchProbe);
    }

    /// <summary>
    /// Tests that vector index config hfresh preserves RQ quantizer through serialization
    /// </summary>
    [Fact]
    public void VectorIndexConfig_HFresh_With_RQ_Quantizer()
    {
        var original = new VectorIndex.HFresh
        {
            Quantizer = new VectorIndex.Quantizers.RQ { Bits = 8, RescoreLimit = 20 },
        };

        var json = VectorIndexSerialization.SerializeHFresh(original);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        var roundtripped = (VectorIndex.HFresh?)VectorIndexSerialization.Factory("hfresh", dict);

        Assert.NotNull(roundtripped?.Quantizer);
        var rq = Assert.IsType<VectorIndex.Quantizers.RQ>(roundtripped?.Quantizer);
        Assert.Equal("rq", rq.Type);
        Assert.Equal(8, rq.Bits);
        Assert.Equal(20, rq.RescoreLimit);
    }

    /// <summary>
    /// Tests that vector index config hfresh preserves multi vector config through serialization
    /// </summary>
    [Fact]
    public void VectorIndexConfig_HFresh_With_MultiVector_Roundtrip()
    {
        var original = new VectorIndex.HFresh
        {
            Distance = VectorIndexConfig.VectorDistance.Cosine,
            MultiVector = new VectorIndexConfig.MultiVectorConfig { Aggregation = "maxSim" },
        };

        var json = VectorIndexSerialization.SerializeHFresh(original);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        var roundtripped = (VectorIndex.HFresh?)VectorIndexSerialization.Factory("hfresh", dict);

        Assert.NotNull(roundtripped);
        Assert.NotNull(roundtripped?.MultiVector);
        Assert.Equal("maxSim", roundtripped?.MultiVector?.Aggregation);
    }

    /// <summary>
    /// Tests that vector index config hfresh throws when serializing with BQ quantizer
    /// </summary>
    [Fact]
    public void VectorIndexConfig_HFresh_With_BQ_Quantizer_Throws()
    {
        var hfresh = new VectorIndex.HFresh { Quantizer = new VectorIndex.Quantizers.BQ() };

        var ex = Assert.Throws<WeaviateClientException>(() =>
            VectorIndexSerialization.SerializeHFresh(hfresh)
        );
        Assert.Contains("HFresh only supports RQ", ex.Message);
    }

    /// <summary>
    /// Tests that vector index config hfresh deserializes from json with named vector fields
    /// </summary>
    [Fact]
    public void VectorIndexConfig_HFresh_From_Json_With_NamedVector()
    {
        var json =
            @"{ ""distance"": ""dot"", ""maxPostingSizeKB"": 128, ""replicas"": 2, ""searchProbe"": 32, ""rq"": { ""enabled"": true, ""bits"": 8, ""rescoreLimit"": 50, ""trainingLimit"": 100000 } }";

        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        var hfresh = (VectorIndex.HFresh?)VectorIndexSerialization.Factory("hfresh", config);

        Assert.NotNull(hfresh);
        Assert.Equal(VectorIndexConfig.VectorDistance.Dot, hfresh?.Distance);
        Assert.Equal(128, hfresh?.MaxPostingSizeKb);
        Assert.Equal(2, hfresh?.Replicas);
        Assert.Equal(32, hfresh?.SearchProbe);
        Assert.NotNull(hfresh?.Quantizer);
        var rq = Assert.IsType<VectorIndex.Quantizers.RQ>(hfresh?.Quantizer);
        Assert.Equal(8, rq.Bits);
        Assert.Equal(50, rq.RescoreLimit);
    }

    /// <summary>
    /// Tests that a centered 4-bit RQ config serializes <c>centering</c> and
    /// <c>trainingLimit</c> under the exact keys the server reads
    /// (entities/vectorindex/hnsw/rq_config.go), omits them when unset so the server
    /// defaults apply, and round-trips.
    /// </summary>
    [Fact]
    public void VectorIndexConfig_HNSW_With_Centered_RQ4_Roundtrip()
    {
        var original = new VectorIndex.HNSW
        {
            Quantizer = new VectorIndex.Quantizers.RQ
            {
                Bits = 4,
                Centering = true,
                RescoreLimit = 123,
                TrainingLimit = 5012,
            },
        };
        var plain = new VectorIndex.HNSW
        {
            Quantizer = new VectorIndex.Quantizers.RQ { Bits = 8, RescoreLimit = 123 },
        };

        var json = VectorIndexSerialization.SerializeHnsw(original);
        var jsonPlain = VectorIndexSerialization.SerializeHnsw(plain);

        using var doc = JsonDocument.Parse(json);
        var rqJson = doc.RootElement.GetProperty("rq");
        Assert.Equal(4, rqJson.GetProperty("bits").GetInt32());
        Assert.True(rqJson.GetProperty("centering").GetBoolean());
        Assert.Equal(123, rqJson.GetProperty("rescoreLimit").GetInt32());
        Assert.Equal(5012, rqJson.GetProperty("trainingLimit").GetInt32());

        // The default PQ/SQ blocks carry their own trainingLimit, so scope the omit check to rq.
        using var docPlain = JsonDocument.Parse(jsonPlain);
        var rqJsonPlain = docPlain.RootElement.GetProperty("rq");
        Assert.False(rqJsonPlain.TryGetProperty("centering", out _));
        Assert.False(rqJsonPlain.TryGetProperty("trainingLimit", out _));

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(
            json,
            Weaviate.Client.Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        var roundtripped = (VectorIndex.HNSW?)VectorIndexSerialization.Factory("hnsw", dict);

        Assert.NotNull(roundtripped?.Quantizer);
        var rq4 = Assert.IsType<VectorIndex.Quantizers.RQ>(roundtripped?.Quantizer);
        Assert.Equal(4, rq4.Bits);
        Assert.True(rq4.Centering);
        Assert.Equal(123, rq4.RescoreLimit);
        Assert.Equal(5012, rq4.TrainingLimit);
    }

    /// <summary>
    /// Tests that serializing an RQ config with centering but without 4 bits throws, matching
    /// the server rule, while a training limit without centering passes through — the server
    /// accepts that combination and simply ignores the value.
    /// </summary>
    [Fact]
    public void VectorIndexConfig_RQ_Centering_Without_4_Bits_Throws()
    {
        var withBits8 = new VectorIndex.HNSW
        {
            Quantizer = new VectorIndex.Quantizers.RQ { Bits = 8, Centering = true },
        };
        var withBitsUnset = new VectorIndex.HNSW
        {
            Quantizer = new VectorIndex.Quantizers.RQ { Centering = true },
        };
        var trainingLimitOnly = new VectorIndex.HNSW
        {
            Quantizer = new VectorIndex.Quantizers.RQ { Bits = 8, TrainingLimit = 5000 },
        };

        var ex = Assert.Throws<WeaviateClientException>(() =>
            VectorIndexSerialization.SerializeHnsw(withBits8)
        );
        Assert.Contains("RQ centering requires bits: 4", ex.Message);

        Assert.Throws<WeaviateClientException>(() =>
            VectorIndexSerialization.SerializeHnsw(withBitsUnset)
        );

        _ = VectorIndexSerialization.SerializeHnsw(trainingLimitOnly);
    }
}

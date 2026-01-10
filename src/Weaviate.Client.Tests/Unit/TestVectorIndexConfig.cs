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
}

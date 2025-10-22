using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public partial class VectorIndexConfigTests
{
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

        using var document = JsonDocument.Parse(json);
        JsonElement config = document.RootElement;

        var hnsw = (VectorIndex.HNSW?)VectorIndexSerialization.Factory("hnsw", config);

        Assert.NotNull(hnsw);
        Assert.NotNull(hnsw?.Quantizer);
        Assert.Equal(expectedQuantizer, hnsw?.Quantizer.Type);
        Assert.Null(hnsw!.MultiVector);
    }

    [Fact]
    public void VectorIndexConfig_Sets_Quantizer_When_Provided()
    {
        // Arrange
        var quantizer = new VectorIndex.Quantizers.BQ();

        var config = new VectorIndex.HNSW { Ef = 100, MaxConnections = 16 };

        // Act & Assert
        var vectorConfig = Configure
            .Vectors.SelfProvided()
            .New(name: "regular", indexConfig: config, quantizerConfig: quantizer);

        Assert.Null(config.Quantizer);

        Assert.NotNull(vectorConfig);
        Assert.Equal("regular", vectorConfig.Name);
        Assert.IsType<VectorIndex.HNSW>(vectorConfig.VectorIndexConfig);
        Assert.NotNull((vectorConfig.VectorIndexConfig as VectorIndex.HNSW)?.Quantizer);
        Assert.IsType<VectorIndex.Quantizers.BQ>(
            (vectorConfig.VectorIndexConfig as VectorIndex.HNSW)?.Quantizer
        );
    }
}

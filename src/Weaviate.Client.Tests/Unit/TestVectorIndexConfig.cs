using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests;

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
}

using System.Text.Json;
using System.Text.Json.Serialization;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.Models;

public abstract record VectorIndexConfig()
{
    [JsonIgnore]
    public abstract string Type { get; }

    internal static VectorIndexConfig Factory(string type, object? vectorIndexConfig)
    {
        VectorIndexConfig? result = null;

        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true, // For readability
            Converters = { new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.CamelCase) },
        };

        if (vectorIndexConfig is JsonElement vic)
        {
            result = type switch
            {
                "hnsw" => JsonSerializer.Deserialize<VectorIndex.HNSW>(vic.GetRawText(), options),
                "flat" => JsonSerializer.Deserialize<VectorIndex.Flat>(vic.GetRawText(), options),
                "dynamic" => JsonSerializer.Deserialize<VectorIndex.Dynamic>(
                    vic.GetRawText(),
                    options
                ),
                _ => null,
            };
        }

        return result ?? throw new WeaviateException("Unable to create VectorIndexConfig");
    }

    public enum VectorDistance
    {
        Cosine,
        Dot,
        L2Squared,
        Hamming,
    }

    public enum VectorIndexFilterStrategy
    {
        Sweeping,
        Acorn,
    }

    public abstract class QuantizerConfig
    {
        public abstract string Type { get; }
    }
}

public static class VectorIndex
{
    public static class Quantizers
    {
        public enum DistributionType
        {
            LogNormal,
            Normal,
        }

        public enum EncoderType
        {
            Kmeans,
            Tile,
        }

        public class BQConfig : QuantizerConfig
        {
            public bool Cache { get; set; }
            public int RescoreLimit { get; set; }
            public override string Type => "bq";
        }

        public class SQConfig : QuantizerConfig
        {
            public int RescoreLimit { get; set; }
            public int TrainingLimit { get; set; }
            public override string Type => "sq";
        }

        public class PQConfig : QuantizerConfig
        {
            public class EncoderConfig
            {
                public EncoderType Type { get; set; }
                public DistributionType Distribution { get; set; }
            }

            public bool BitCompression { get; set; }
            public int Centroids { get; set; }
            public required EncoderConfig Encoder { get; set; }
            public int Segments { get; set; }
            public int TrainingLimit { get; set; }

            public override string Type => "pq";
        }
    }

    public sealed record HNSW : VectorIndexConfig
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CleanupIntervalSeconds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VectorDistance? Distance { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? DynamicEfMin { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? DynamicEfMax { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? DynamicEfFactor { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? EfConstruction { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Ef { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VectorIndexFilterStrategy? FilterStrategy { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? FlatSearchCutoff { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxConnections { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public QuantizerConfig? Quantizer { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Skip { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? VectorCacheMaxObjects { get; set; }

        [JsonIgnore]
        public override string Type => "hnsw";
    }

    public sealed record Flat : VectorIndexConfig
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VectorDistance? Distance { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? VectorCacheMaxObjects { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Quantizers.BQConfig? Quantizer { get; set; }

        [JsonIgnore]
        public override string Type => "flat";
    }

    public sealed record Dynamic : VectorIndexConfig
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VectorDistance? Distance { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Threshold { get; set; }
        public required HNSW? Hnsw { get; set; }
        public required Flat? Flat { get; set; }

        [JsonIgnore]
        public override string Type => "dynamic";
    }
}

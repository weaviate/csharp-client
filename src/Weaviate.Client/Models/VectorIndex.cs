using System.Text.Json;
using System.Text.Json.Serialization;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.Models;

public abstract record VectorIndexConfig()
{
    [JsonIgnore]
    public abstract string Type { get; }

    public enum VectorDistance
    {
        [System.Runtime.Serialization.EnumMember(Value = "cosine")]
        Cosine,

        [System.Runtime.Serialization.EnumMember(Value = "dot")]
        Dot,

        [System.Runtime.Serialization.EnumMember(Value = "l2squared")]
        L2Squared,

        [System.Runtime.Serialization.EnumMember(Value = "hamming")]
        Hamming,
    }

    public enum VectorIndexFilterStrategy
    {
        [System.Runtime.Serialization.EnumMember(Value = "sweeping")]
        Sweeping,

        [System.Runtime.Serialization.EnumMember(Value = "acorn")]
        Acorn,
    }

    public abstract record QuantizerConfig
    {
        [JsonIgnore]
        public abstract string Type { get; }

        public bool Enabled { get; init; } = true;
    }
}

public static class VectorIndex
{
    public static class Quantizers
    {
        public enum DistributionType
        {
            [System.Runtime.Serialization.EnumMember(Value = "log-normal")]
            LogNormal,

            [System.Runtime.Serialization.EnumMember(Value = "normal")]
            Normal,
        }

        public enum EncoderType
        {
            [System.Runtime.Serialization.EnumMember(Value = "kmeans")]
            Kmeans,

            [System.Runtime.Serialization.EnumMember(Value = "tile")]
            Tile,
        }

        public record BQ : QuantizerConfig
        {
            public bool Cache { get; set; }
            public int RescoreLimit { get; set; }

            [JsonIgnore]
            public override string Type => "bq";
        }

        public record SQ : QuantizerConfig
        {
            public int RescoreLimit { get; set; }
            public int TrainingLimit { get; set; }

            [JsonIgnore]
            public override string Type => "sq";
        }

        public record PQ : QuantizerConfig
        {
            public record EncoderConfig
            {
                public EncoderType Type { get; set; }
                public DistributionType Distribution { get; set; }
            }

            public bool BitCompression { get; set; }
            public int Centroids { get; set; }
            public required EncoderConfig Encoder { get; set; }
            public int Segments { get; set; }
            public int TrainingLimit { get; set; }

            [JsonIgnore]
            public override string Type => "pq";
        }
    }

    public sealed record HNSW : VectorIndexConfig
    {
        public int? CleanupIntervalSeconds { get; set; }
        public VectorDistance? Distance { get; set; }
        public int? DynamicEfMin { get; set; }
        public int? DynamicEfMax { get; set; }
        public int? DynamicEfFactor { get; set; }
        public int? EfConstruction { get; set; }
        public int? Ef { get; set; }
        public VectorIndexFilterStrategy? FilterStrategy { get; set; }
        public int? FlatSearchCutoff { get; set; }
        public int? MaxConnections { get; set; }
        public bool? Skip { get; set; }
        public long? VectorCacheMaxObjects { get; set; }

        public QuantizerConfig? Quantizer { get; set; }

        public override string Type => "hnsw";
    }

    public sealed record Flat : VectorIndexConfig
    {
        public VectorDistance? Distance { get; set; }
        public long? VectorCacheMaxObjects { get; set; }
        public Quantizers.BQ? Quantizer { get; set; }

        public override string Type => "flat";
    }

    public sealed record Dynamic : VectorIndexConfig
    {
        public VectorDistance? Distance { get; set; }
        public int? Threshold { get; set; }

        public required HNSW? Hnsw { get; set; }
        public required Flat? Flat { get; set; }

        [JsonIgnore]
        public override string Type => "dynamic";
    }
}

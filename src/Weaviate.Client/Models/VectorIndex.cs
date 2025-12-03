using System.Text.Json.Serialization;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.Models;

public abstract record VectorIndexConfig()
{
    public static class MultiVectorAggregation
    {
        public const string MaxSim = "maxSim";
    }

    public abstract record EncodingConfig { }

    public record MuveraEncoding : EncodingConfig
    {
        public double? KSim { get; init; } = 4;
        public double? DProjections { get; init; } = 16;
        public double? Repetitions { get; init; } = 10;

        internal MuveraDto ToDto() =>
            new MuveraDto()
            {
                Enabled = true,
                KSim = KSim,
                DProjections = DProjections,
                Repetitions = Repetitions,
            };
    }

    public record MultiVectorConfig
    {
        public string? Aggregation { get; init; } = "maxSim";
        public EncodingConfig? Encoding { get; init; } = new MuveraEncoding();
    }

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

    public abstract record QuantizerConfigBase
    {
        [JsonIgnore]
        public abstract string Type { get; }

        public bool Enabled { get; init; } = true;
    }

    public abstract record QuantizerConfigFlat : QuantizerConfigBase { }
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

        public record BQ : QuantizerConfigFlat
        {
            public const string TypeValue = "bq";

            public bool Cache { get; set; }
            public int RescoreLimit { get; set; }

            [JsonIgnore]
            public override string Type => TypeValue;
        }

        public record RQ : QuantizerConfigFlat
        {
            public const string TypeValue = "rq";

            public int RescoreLimit { get; set; }
            public int? Bits { get; set; }
            public bool Cache { get; set; }

            [JsonIgnore]
            public override string Type => TypeValue;
        }

        public record SQ : QuantizerConfigBase
        {
            public const string TypeValue = "sq";

            public int RescoreLimit { get; set; }
            public int TrainingLimit { get; set; }

            [JsonIgnore]
            public override string Type => TypeValue;
        }

        public record PQ : QuantizerConfigBase
        {
            public const string TypeValue = "pq";

            public record EncoderConfig
            {
                public EncoderType Type { get; set; }
                public DistributionType Distribution { get; set; }
            }

            public bool BitCompression { get; set; }
            public int Centroids { get; set; }
            public EncoderConfig? Encoder { get; set; }
            public int Segments { get; set; }
            public int TrainingLimit { get; set; }

            [JsonIgnore]
            public override string Type => TypeValue;
        }

        public record None : QuantizerConfigFlat
        {
            public const string TypeValue = "none";

            [JsonIgnore]
            public override string Type => TypeValue;
        }
    }

    public sealed record HNSW : VectorIndexConfig
    {
        public const string TypeValue = "hnsw";

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
        public QuantizerConfigBase? Quantizer { get; set; }
        public MultiVectorConfig? MultiVector { get; set; }
        public bool? SkipDefaultQuantization { get; set; }

        public override string Type => TypeValue;
    }

    public sealed record Flat : VectorIndexConfig
    {
        public const string TypeValue = "flat";

        public VectorDistance? Distance { get; set; }
        public long? VectorCacheMaxObjects { get; set; }
        public QuantizerConfigFlat? Quantizer { get; set; }

        public override string Type => TypeValue;
    }

    public sealed record Dynamic : VectorIndexConfig
    {
        public const string TypeValue = "dynamic";

        public VectorDistance? Distance { get; set; }
        public int? Threshold { get; set; }

        public required HNSW? Hnsw { get; set; }
        public required Flat? Flat { get; set; }

        [JsonIgnore]
        public override string Type => TypeValue;
    }
}

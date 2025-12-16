using System.Text.Json.Serialization;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.Models;

/// <summary>
/// Base class for vector index configurations. Defines how vectors are indexed for similarity search.
/// </summary>
/// <remarks>
/// Vector index configuration determines the algorithm and parameters used for vector search.
/// Common implementations include HNSW (Hierarchical Navigable Small World), Flat, and Dynamic indexes.
/// Each type offers different trade-offs between speed, accuracy, and memory usage.
/// </remarks>
public abstract record VectorIndexConfig()
{
    /// <summary>
    /// Constants for multi-vector aggregation strategies.
    /// </summary>
    public static class MultiVectorAggregation
    {
        /// <summary>
        /// Maximum similarity aggregation for multi-vector search.
        /// </summary>
        public const string MaxSim = "maxSim";
    }

    /// <summary>
    /// Base class for encoding configurations used with multi-vector setups.
    /// </summary>
    public abstract record EncodingConfig { }

    /// <summary>
    /// MUVERA (Multi-Vector Representation Aggregation) encoding configuration.
    /// </summary>
    /// <remarks>
    /// MUVERA is an encoding technique for efficient multi-vector search,
    /// particularly useful with late interaction models like ColBERT.
    /// </remarks>
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

    /// <summary>
    /// Configuration for multi-vector search behavior.
    /// </summary>
    public record MultiVectorConfig
    {
        /// <summary>
        /// Gets or sets the aggregation strategy for combining multiple vector scores. Defaults to "maxSim".
        /// </summary>
        public string? Aggregation { get; set; } = "maxSim";

        /// <summary>
        /// Gets or sets the encoding configuration for multi-vector compression.
        /// </summary>
        public EncodingConfig? Encoding { get; set; } = null;
    }

    /// <summary>
    /// Gets the type identifier for this vector index configuration.
    /// </summary>
    [JsonIgnore]
    public abstract string Type { get; }

    /// <summary>
    /// Distance metric used for vector similarity calculations.
    /// </summary>
    public enum VectorDistance
    {
        /// <summary>
        /// Cosine distance (1 - cosine similarity). Range: [0, 2], where 0 is identical.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "cosine")]
        Cosine,

        /// <summary>
        /// Dot product distance. Higher values indicate greater similarity.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "dot")]
        Dot,

        /// <summary>
        /// Squared L2 (Euclidean) distance. Lower values indicate greater similarity.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "l2squared")]
        L2Squared,

        /// <summary>
        /// Hamming distance for binary vectors. Counts differing bits.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "hamming")]
        Hamming,
    }

    /// <summary>
    /// Strategy for applying filters during vector search.
    /// </summary>
    public enum VectorIndexFilterStrategy
    {
        /// <summary>
        /// Sweeping strategy: filters after vector search.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "sweeping")]
        Sweeping,

        /// <summary>
        /// Acorn strategy: filters during vector search for better performance with selective filters.
        /// </summary>
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

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool Cache { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int RescoreLimit { get; set; }

            [JsonIgnore]
            public override string Type => TypeValue;
        }

        public record RQ : QuantizerConfigFlat
        {
            public const string TypeValue = "rq";

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int RescoreLimit { get; set; }

            public int? Bits { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
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

        public record None : QuantizerConfigBase
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

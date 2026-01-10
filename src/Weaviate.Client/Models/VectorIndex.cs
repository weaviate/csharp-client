using System.Text.Json.Serialization;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.Models;

/// <summary>
/// Base class for vector index configurations.
/// </summary>
public abstract record VectorIndexConfig()
{
    /// <summary>
    /// Contains multi-vector aggregation strategy constants.
    /// </summary>
    public static class MultiVectorAggregation
    {
        /// <summary>
        /// Maximum similarity aggregation strategy.
        /// </summary>
        public const string MaxSim = "maxSim";
    }

    /// <summary>
    /// Base class for encoding configurations.
    /// </summary>
    public abstract record EncodingConfig { }

    /// <summary>
    /// Configuration for Muvera encoding.
    /// </summary>
    public record MuveraEncoding : EncodingConfig
    {
        /// <summary>
        /// Gets or initializes the similarity parameter for Muvera encoding.
        /// </summary>
        public double? KSim { get; init; } = 4;

        /// <summary>
        /// Gets or initializes the number of projections for dimensionality reduction.
        /// </summary>
        public double? DProjections { get; init; } = 16;

        /// <summary>
        /// Gets or initializes the number of repetitions for the encoding process.
        /// </summary>
        public double? Repetitions { get; init; } = 10;

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>The muvera dto</returns>
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
    /// Configuration for multi-vector handling.
    /// </summary>
    public record MultiVectorConfig
    {
        /// <summary>
        /// Gets or sets the aggregation strategy for multi-vectors.
        /// </summary>
        public string? Aggregation { get; set; } = "maxSim";

        /// <summary>
        /// Gets or sets the encoding configuration for multi-vectors.
        /// </summary>
        public EncodingConfig? Encoding { get; set; } = null;
    }

    /// <summary>
    /// Gets the type identifier for the vector index configuration.
    /// </summary>
    [JsonIgnore]
    public abstract string Type { get; }

    /// <summary>
    /// Specifies the distance metric for vector similarity calculations.
    /// </summary>
    public enum VectorDistance
    {
        /// <summary>
        /// Cosine distance metric.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "cosine")]
        Cosine,

        /// <summary>
        /// Dot product distance metric.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "dot")]
        Dot,

        /// <summary>
        /// L2 squared (Euclidean squared) distance metric.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "l2squared")]
        L2Squared,

        /// <summary>
        /// Hamming distance metric for binary vectors.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "hamming")]
        Hamming,
    }

    /// <summary>
    /// Specifies the filter strategy for vector index operations.
    /// </summary>
    public enum VectorIndexFilterStrategy
    {
        /// <summary>
        /// Sweeping filter strategy.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "sweeping")]
        Sweeping,

        /// <summary>
        /// Acorn filter strategy.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "acorn")]
        Acorn,
    }

    /// <summary>
    /// Base class for quantizer configurations.
    /// </summary>
    public abstract record QuantizerConfigBase
    {
        /// <summary>
        /// Gets the type identifier for the quantizer.
        /// </summary>
        [JsonIgnore]
        public abstract string Type { get; }

        /// <summary>
        /// Gets or initializes whether the quantizer is enabled.
        /// </summary>
        public bool Enabled { get; init; } = true;
    }

    /// <summary>
    /// Base class for flat quantizer configurations.
    /// </summary>
    public abstract record QuantizerConfigFlat : QuantizerConfigBase { }
}

/// <summary>
/// Contains vector index type configurations.
/// </summary>
public static class VectorIndex
{
    /// <summary>
    /// Contains quantizer configurations for vector compression.
    /// </summary>
    public static class Quantizers
    {
        /// <summary>
        /// Specifies the distribution type for quantization.
        /// </summary>
        public enum DistributionType
        {
            /// <summary>
            /// Log-normal distribution.
            /// </summary>
            [System.Runtime.Serialization.EnumMember(Value = "log-normal")]
            LogNormal,

            /// <summary>
            /// Normal (Gaussian) distribution.
            /// </summary>
            [System.Runtime.Serialization.EnumMember(Value = "normal")]
            Normal,
        }

        /// <summary>
        /// Specifies the encoder type for quantization.
        /// </summary>
        public enum EncoderType
        {
            /// <summary>
            /// K-means clustering encoder.
            /// </summary>
            [System.Runtime.Serialization.EnumMember(Value = "kmeans")]
            Kmeans,

            /// <summary>
            /// Tile-based encoder.
            /// </summary>
            [System.Runtime.Serialization.EnumMember(Value = "tile")]
            Tile,
        }

        /// <summary>
        /// Binary quantization (BQ) configuration.
        /// </summary>
        public record BQ : QuantizerConfigFlat
        {
            /// <summary>
            /// The type value for binary quantization.
            /// </summary>
            public const string TypeValue = "bq";

            /// <summary>
            /// Gets or sets whether to cache quantized vectors.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool Cache { get; set; }

            /// <summary>
            /// Gets or sets the limit for rescoring operations.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int RescoreLimit { get; set; }

            /// <summary>
            /// Gets the type identifier for the quantizer.
            /// </summary>
            [JsonIgnore]
            public override string Type => TypeValue;
        }

        /// <summary>
        /// Residual quantization (RQ) configuration.
        /// </summary>
        public record RQ : QuantizerConfigFlat
        {
            /// <summary>
            /// The type value for residual quantization.
            /// </summary>
            public const string TypeValue = "rq";

            /// <summary>
            /// Gets or sets the limit for rescoring operations.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int RescoreLimit { get; set; }

            /// <summary>
            /// Gets or sets the number of bits for quantization.
            /// </summary>
            public int? Bits { get; set; }

            /// <summary>
            /// Gets or sets whether to cache quantized vectors.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool Cache { get; set; }

            /// <summary>
            /// Gets the type identifier for the quantizer.
            /// </summary>
            [JsonIgnore]
            public override string Type => TypeValue;
        }

        /// <summary>
        /// Scalar quantization (SQ) configuration.
        /// </summary>
        public record SQ : QuantizerConfigBase
        {
            /// <summary>
            /// The type value for scalar quantization.
            /// </summary>
            public const string TypeValue = "sq";

            /// <summary>
            /// Gets or sets the limit for rescoring operations.
            /// </summary>
            public int RescoreLimit { get; set; }

            /// <summary>
            /// Gets or sets the training limit for the quantizer.
            /// </summary>
            public int TrainingLimit { get; set; }

            /// <summary>
            /// Gets the type identifier for the quantizer.
            /// </summary>
            [JsonIgnore]
            public override string Type => TypeValue;
        }

        /// <summary>
        /// Product quantization (PQ) configuration.
        /// </summary>
        public record PQ : QuantizerConfigBase
        {
            /// <summary>
            /// The type value for product quantization.
            /// </summary>
            public const string TypeValue = "pq";

            /// <summary>
            /// Configuration for the PQ encoder.
            /// </summary>
            public record EncoderConfig
            {
                /// <summary>
                /// Gets or sets the encoder type.
                /// </summary>
                public EncoderType Type { get; set; }

                /// <summary>
                /// Gets or sets the distribution type for training.
                /// </summary>
                public DistributionType Distribution { get; set; }
            }

            /// <summary>
            /// Gets or sets whether to use bit compression.
            /// </summary>
            public bool BitCompression { get; set; }

            /// <summary>
            /// Gets or sets the number of centroids for quantization.
            /// </summary>
            public int Centroids { get; set; }

            /// <summary>
            /// Gets or sets the encoder configuration.
            /// </summary>
            public EncoderConfig? Encoder { get; set; }

            /// <summary>
            /// Gets or sets the number of segments for quantization.
            /// </summary>
            public int Segments { get; set; }

            /// <summary>
            /// Gets or sets the training limit for the quantizer.
            /// </summary>
            public int TrainingLimit { get; set; }

            /// <summary>
            /// Gets the type identifier for the quantizer.
            /// </summary>
            [JsonIgnore]
            public override string Type => TypeValue;
        }

        /// <summary>
        /// No quantization configuration.
        /// </summary>
        public record None : QuantizerConfigBase
        {
            /// <summary>
            /// The type value for no quantization.
            /// </summary>
            public const string TypeValue = "none";

            /// <summary>
            /// Gets the type identifier for the quantizer.
            /// </summary>
            [JsonIgnore]
            public override string Type => TypeValue;
        }
    }

    /// <summary>
    /// Configuration for HNSW (Hierarchical Navigable Small World) vector index.
    /// </summary>
    public sealed record HNSW : VectorIndexConfig
    {
        /// <summary>
        /// The type value for HNSW index.
        /// </summary>
        public const string TypeValue = "hnsw";

        /// <summary>
        /// Gets or sets the cleanup interval in seconds for index maintenance.
        /// </summary>
        public int? CleanupIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the distance metric for vector similarity.
        /// </summary>
        public VectorDistance? Distance { get; set; }

        /// <summary>
        /// Gets or sets the minimum dynamic ef parameter for adaptive search.
        /// </summary>
        public int? DynamicEfMin { get; set; }

        /// <summary>
        /// Gets or sets the maximum dynamic ef parameter for adaptive search.
        /// </summary>
        public int? DynamicEfMax { get; set; }

        /// <summary>
        /// Gets or sets the dynamic ef factor for adaptive search tuning.
        /// </summary>
        public int? DynamicEfFactor { get; set; }

        /// <summary>
        /// Gets or sets the ef construction parameter for index building.
        /// </summary>
        public int? EfConstruction { get; set; }

        /// <summary>
        /// Gets or sets the ef parameter for search operations.
        /// </summary>
        public int? Ef { get; set; }

        /// <summary>
        /// Gets or sets the filter strategy for pre-filtering.
        /// </summary>
        public VectorIndexFilterStrategy? FilterStrategy { get; set; }

        /// <summary>
        /// Gets or sets the threshold for switching to flat search.
        /// </summary>
        public int? FlatSearchCutoff { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of connections per layer.
        /// </summary>
        public int? MaxConnections { get; set; }

        /// <summary>
        /// Gets or sets whether to skip indexing.
        /// </summary>
        public bool? Skip { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of objects to cache in memory.
        /// </summary>
        public long? VectorCacheMaxObjects { get; set; }

        /// <summary>
        /// Gets or sets the quantizer configuration for vector compression.
        /// </summary>
        public QuantizerConfigBase? Quantizer { get; set; }

        /// <summary>
        /// Gets or sets the multi-vector configuration.
        /// </summary>
        public MultiVectorConfig? MultiVector { get; set; }

        /// <summary>
        /// Gets or sets whether to skip default quantization.
        /// </summary>
        public bool? SkipDefaultQuantization { get; set; }

        /// <summary>
        /// Gets the type identifier for the index.
        /// </summary>
        public override string Type => TypeValue;
    }

    /// <summary>
    /// Configuration for flat (brute-force) vector index.
    /// </summary>
    public sealed record Flat : VectorIndexConfig
    {
        /// <summary>
        /// The type value for flat index.
        /// </summary>
        public const string TypeValue = "flat";

        /// <summary>
        /// Gets or sets the distance metric for vector similarity.
        /// </summary>
        public VectorDistance? Distance { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of objects to cache in memory.
        /// </summary>
        public long? VectorCacheMaxObjects { get; set; }

        /// <summary>
        /// Gets or sets the quantizer configuration for vector compression.
        /// </summary>
        public QuantizerConfigFlat? Quantizer { get; set; }

        /// <summary>
        /// Gets the type identifier for the index.
        /// </summary>
        public override string Type => TypeValue;
    }

    /// <summary>
    /// Configuration for dynamic vector index that switches between HNSW and flat based on data size.
    /// </summary>
    public sealed record Dynamic : VectorIndexConfig
    {
        /// <summary>
        /// The type value for dynamic index.
        /// </summary>
        public const string TypeValue = "dynamic";

        /// <summary>
        /// Gets or sets the distance metric for vector similarity.
        /// </summary>
        public VectorDistance? Distance { get; set; }

        /// <summary>
        /// Gets or sets the threshold for switching between index types.
        /// </summary>
        public int? Threshold { get; set; }

        /// <summary>
        /// Gets or sets the HNSW configuration for larger datasets.
        /// </summary>
        public required HNSW? Hnsw { get; set; }

        /// <summary>
        /// Gets or sets the flat configuration for smaller datasets.
        /// </summary>
        public required Flat? Flat { get; set; }

        /// <summary>
        /// Gets the type identifier for the index.
        /// </summary>
        [JsonIgnore]
        public override string Type => TypeValue;
    }
}

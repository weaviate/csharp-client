using System.Text.Json;
using System.Text.Json.Serialization;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.Models;

// DTOs for serialization
/// <summary>
/// The multi vector encoding dto class
/// </summary>
internal abstract class MultiVectorEncodingDto
{
    /// <summary>
    /// Gets or sets the value of the enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; } = false;
}

/// <summary>
/// The muvera dto class
/// </summary>
/// <seealso cref="MultiVectorEncodingDto"/>
internal class MuveraDto : MultiVectorEncodingDto
{
    /// <summary>
    /// Gets or sets the value of the k sim
    /// </summary>
    [JsonPropertyName("ksim")]
    public double? KSim { get; set; } = 4;

    /// <summary>
    /// Gets or sets the value of the d projections
    /// </summary>
    [JsonPropertyName("dprojections")]
    public double? DProjections { get; set; } = 16;

    /// <summary>
    /// Gets or sets the value of the repetitions
    /// </summary>
    [JsonPropertyName("repetitions")]
    public double? Repetitions { get; set; } = 10;

    /// <summary>
    /// Returns the model
    /// </summary>
    /// <returns>The muvera encoding</returns>
    public MuveraEncoding? ToModel() =>
        (Enabled ?? false)
            ? new()
            {
                KSim = KSim,
                DProjections = DProjections,
                Repetitions = Repetitions,
            }
            : null;
}

/// <summary>
/// The multi vector dto class
/// </summary>
internal class MultiVectorDto
{
    /// <summary>
    /// Gets or sets the value of the enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the aggregation
    /// </summary>
    [JsonPropertyName("aggregation")]
    public string? Aggregation { get; set; } = "maxSim";

    /// <summary>
    /// Gets or sets the value of the muvera
    /// </summary>
    [JsonPropertyName("muvera")]
    public MuveraDto? Muvera { get; set; } = new MuveraDto();
}

/// <summary>
/// The hnsw dto class
/// </summary>
internal class HnswDto
{
    /// <summary>
    /// Gets or sets the value of the cleanup interval seconds
    /// </summary>
    [JsonPropertyName("cleanupIntervalSeconds")]
    public int? CleanupIntervalSeconds { get; set; }

    /// <summary>
    /// Gets or sets the value of the distance
    /// </summary>
    [JsonPropertyName("distance")]
    public VectorIndexConfig.VectorDistance? Distance { get; set; }

    /// <summary>
    /// Gets or sets the value of the dynamic ef min
    /// </summary>
    [JsonPropertyName("dynamicEfMin")]
    public int? DynamicEfMin { get; set; }

    /// <summary>
    /// Gets or sets the value of the dynamic ef max
    /// </summary>
    [JsonPropertyName("dynamicEfMax")]
    public int? DynamicEfMax { get; set; }

    /// <summary>
    /// Gets or sets the value of the dynamic ef factor
    /// </summary>
    [JsonPropertyName("dynamicEfFactor")]
    public int? DynamicEfFactor { get; set; }

    /// <summary>
    /// Gets or sets the value of the ef construction
    /// </summary>
    [JsonPropertyName("efConstruction")]
    public int? EfConstruction { get; set; }

    /// <summary>
    /// Gets or sets the value of the ef
    /// </summary>
    [JsonPropertyName("ef")]
    public int? Ef { get; set; }

    /// <summary>
    /// Gets or sets the value of the filter strategy
    /// </summary>
    [JsonPropertyName("filterStrategy")]
    public VectorIndexConfig.VectorIndexFilterStrategy? FilterStrategy { get; set; }

    /// <summary>
    /// Gets or sets the value of the flat search cutoff
    /// </summary>
    [JsonPropertyName("flatSearchCutoff")]
    public int? FlatSearchCutoff { get; set; }

    /// <summary>
    /// Gets or sets the value of the max connections
    /// </summary>
    [JsonPropertyName("maxConnections")]
    public int? MaxConnections { get; set; }

    /// <summary>
    /// Gets or sets the value of the skip
    /// </summary>
    [JsonPropertyName("skip")]
    public bool? Skip { get; set; }

    /// <summary>
    /// Gets or sets the value of the vector cache max objects
    /// </summary>
    [JsonPropertyName("vectorCacheMaxObjects")]
    public long? VectorCacheMaxObjects { get; set; }

    // Quantizers at root level
    /// <summary>
    /// Gets or sets the value of the bq
    /// </summary>
    [JsonPropertyName("bq")]
    public VectorIndex.Quantizers.BQ? BQ { get; set; }

    /// <summary>
    /// Gets or sets the value of the pq
    /// </summary>
    [JsonPropertyName("pq")]
    public VectorIndex.Quantizers.PQ? PQ { get; set; }

    /// <summary>
    /// Gets or sets the value of the sq
    /// </summary>
    [JsonPropertyName("sq")]
    public VectorIndex.Quantizers.SQ? SQ { get; set; }

    /// <summary>
    /// Gets or sets the value of the rq
    /// </summary>
    [JsonPropertyName("rq")]
    public VectorIndex.Quantizers.RQ? RQ { get; set; }

    /// <summary>
    /// Gets or sets the value of the multi vector
    /// </summary>
    [JsonPropertyName("multivector")]
    public MultiVectorDto? MultiVector { get; set; }

    /// <summary>
    /// Gets or sets the value of the skip default quantization
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("skipDefaultQuantization")]
    public bool? SkipDefaultQuantization { get; set; }
}

/// <summary>
/// The flat dto class
/// </summary>
internal class FlatDto
{
    /// <summary>
    /// Gets or sets the value of the distance
    /// </summary>
    [JsonPropertyName("distance")]
    public VectorIndexConfig.VectorDistance? Distance { get; set; }

    /// <summary>
    /// Gets or sets the value of the vector cache max objects
    /// </summary>
    [JsonPropertyName("vectorCacheMaxObjects")]
    public long? VectorCacheMaxObjects { get; set; }

    // All quantizer properties are BQ type as specified
    /// <summary>
    /// Gets or sets the value of the bq
    /// </summary>
    [JsonPropertyName("bq")]
    public VectorIndex.Quantizers.BQ? BQ { get; set; }

    /// <summary>
    /// Gets or sets the value of the pq
    /// </summary>
    [JsonPropertyName("pq")]
    public VectorIndex.Quantizers.PQ? PQ { get; set; }

    /// <summary>
    /// Gets or sets the value of the sq
    /// </summary>
    [JsonPropertyName("sq")]
    public VectorIndex.Quantizers.SQ? SQ { get; set; }

    /// <summary>
    /// Gets or sets the value of the rq
    /// </summary>
    [JsonPropertyName("rq")]
    public VectorIndex.Quantizers.RQ? RQ { get; set; }
}

/// <summary>
/// The dynamic dto class
/// </summary>
internal class DynamicDto
{
    /// <summary>
    /// Gets or sets the value of the distance
    /// </summary>
    [JsonPropertyName("distance")]
    public VectorIndexConfig.VectorDistance? Distance { get; set; }

    /// <summary>
    /// Gets or sets the value of the threshold
    /// </summary>
    [JsonPropertyName("threshold")]
    public int? Threshold { get; set; }

    /// <summary>
    /// Gets or sets the value of the hnsw
    /// </summary>
    [JsonPropertyName("hnsw")]
    public HnswDto? Hnsw { get; set; }

    /// <summary>
    /// Gets or sets the value of the flat
    /// </summary>
    [JsonPropertyName("flat")]
    public FlatDto? Flat { get; set; }
}

// Extension methods for mapping
/// <summary>
/// The vector index mapping extensions class
/// </summary>
internal static class VectorIndexMappingExtensions
{
    // Helper to get the single enabled quantizer
    /// <summary>
    /// Gets the enabled quantizer using the specified quantizers
    /// </summary>
    /// <param name="quantizers">The quantizers</param>
    /// <returns>The quantizer config base</returns>
    private static QuantizerConfigBase? GetEnabledQuantizer(
        params QuantizerConfigBase?[] quantizers
    )
    {
        return quantizers.FirstOrDefault(q => q?.Enabled == true);
    }

    // HNSW mapping
    /// <summary>
    /// Returns the hnsw using the specified dto
    /// </summary>
    /// <param name="dto">The dto</param>
    /// <returns>The vector index hnsw</returns>
    public static VectorIndex.HNSW ToHnsw(this HnswDto dto)
    {
        var quantizer = GetEnabledQuantizer(
            dto.BQ,
            dto.PQ,
            dto.SQ,
            dto.RQ,
            new VectorIndex.Quantizers.None() { Enabled = dto.SkipDefaultQuantization == true }
        );

        var muvera = dto.MultiVector?.Muvera?.ToModel();

        var multivector =
            dto.MultiVector != null && dto.MultiVector.Enabled == true
                ? new MultiVectorConfig
                {
                    Aggregation = dto.MultiVector.Aggregation,
                    Encoding = muvera,
                }
                : null;

        return new VectorIndex.HNSW
        {
            CleanupIntervalSeconds = dto.CleanupIntervalSeconds,
            Distance = dto.Distance,
            DynamicEfMin = dto.DynamicEfMin,
            DynamicEfMax = dto.DynamicEfMax,
            DynamicEfFactor = dto.DynamicEfFactor,
            EfConstruction = dto.EfConstruction,
            Ef = dto.Ef,
            FilterStrategy = dto.FilterStrategy,
            FlatSearchCutoff = dto.FlatSearchCutoff,
            MaxConnections = dto.MaxConnections,
            Skip = dto.Skip,
            VectorCacheMaxObjects = dto.VectorCacheMaxObjects,
            Quantizer = quantizer,
            MultiVector = multivector,
            // Only preserve SkipDefaultQuantization if it's true (immutability requirement)
            // If false/null, we don't set it so it can be omitted from serialization
            SkipDefaultQuantization = dto.SkipDefaultQuantization == true ? true : null,
        };
    }

    /// <summary>
    /// Returns the dto using the specified hnsw
    /// </summary>
    /// <param name="hnsw">The hnsw</param>
    /// <returns>The dto</returns>
    public static HnswDto ToDto(this VectorIndex.HNSW hnsw)
    {
        var dto = new HnswDto
        {
            CleanupIntervalSeconds = hnsw.CleanupIntervalSeconds,
            Distance = hnsw.Distance,
            DynamicEfMin = hnsw.DynamicEfMin,
            DynamicEfMax = hnsw.DynamicEfMax,
            DynamicEfFactor = hnsw.DynamicEfFactor,
            EfConstruction = hnsw.EfConstruction,
            Ef = hnsw.Ef,
            FilterStrategy = hnsw.FilterStrategy,
            FlatSearchCutoff = hnsw.FlatSearchCutoff,
            MaxConnections = hnsw.MaxConnections,
            Skip = hnsw.Skip,
            VectorCacheMaxObjects = hnsw.VectorCacheMaxObjects,
            MultiVector =
                hnsw.MultiVector != null
                    ? new MultiVectorDto
                    {
                        Enabled = true,
                        Muvera = (hnsw.MultiVector?.Encoding as MuveraEncoding)?.ToDto(),
                        Aggregation = hnsw.MultiVector?.Aggregation,
                    }
                    : new MultiVectorDto
                    {
                        Enabled = false,
                        Aggregation = "maxSim",
                        Muvera = new MuveraDto
                        {
                            Enabled = false,
                            KSim = 4,
                            DProjections = 16,
                            Repetitions = 10,
                        },
                    },
            SkipDefaultQuantization = hnsw.SkipDefaultQuantization,
            // Always include all quantizers with defaults
            BQ = new VectorIndex.Quantizers.BQ { Enabled = false },
            PQ = new VectorIndex.Quantizers.PQ
            {
                Enabled = false,
                BitCompression = false,
                Centroids = 256,
                Segments = 0,
                TrainingLimit = 100000,
                Encoder = new VectorIndex.Quantizers.PQ.EncoderConfig
                {
                    Type = VectorIndex.Quantizers.EncoderType.Kmeans,
                    Distribution = VectorIndex.Quantizers.DistributionType.LogNormal,
                },
            },
            SQ = new VectorIndex.Quantizers.SQ
            {
                Enabled = false,
                RescoreLimit = 20,
                TrainingLimit = 100000,
            },
            RQ = new VectorIndex.Quantizers.RQ
            {
                Enabled = false,
                Bits = 8,
                RescoreLimit = 20,
            },
        };

        // Override with the enabled quantizer if present
        if (hnsw.Quantizer != null)
        {
            switch (hnsw.Quantizer.Type.ToLowerInvariant())
            {
                case "bq":
                    dto.BQ = hnsw.Quantizer as VectorIndex.Quantizers.BQ;
                    break;
                case "pq":
                    dto.PQ = hnsw.Quantizer as VectorIndex.Quantizers.PQ;
                    break;
                case "sq":
                    dto.SQ = hnsw.Quantizer as VectorIndex.Quantizers.SQ;
                    break;
                case "rq":
                    dto.RQ = hnsw.Quantizer as VectorIndex.Quantizers.RQ;
                    break;
                case "none":
                    dto.SkipDefaultQuantization = true;
                    break;
            }
        }

        return dto;
    }

    // Flat mapping
    /// <summary>
    /// Returns the flat using the specified dto
    /// </summary>
    /// <param name="dto">The dto</param>
    /// <returns>The vector index flat</returns>
    public static VectorIndex.Flat ToFlat(this FlatDto dto)
    {
        // For Flat, the Quantizer property is specifically BQ type
        var quantizer = GetEnabledQuantizer(dto.BQ, dto.PQ, dto.SQ, dto.RQ);

        return new VectorIndex.Flat
        {
            Distance = dto.Distance,
            VectorCacheMaxObjects = dto.VectorCacheMaxObjects,
            Quantizer = quantizer as QuantizerConfigFlat,
        };
    }

    /// <summary>
    /// Returns the dto using the specified flat
    /// </summary>
    /// <param name="flat">The flat</param>
    /// <returns>The dto</returns>
    public static FlatDto ToDto(this VectorIndex.Flat flat)
    {
        var dto = new FlatDto
        {
            Distance = flat.Distance,
            VectorCacheMaxObjects = flat.VectorCacheMaxObjects,
        };

        // For Flat, the Quantizer is always BQ type
        if (flat.Quantizer != null)
        {
            // Based on the original quantizer type, set the appropriate DTO property
            switch (flat.Quantizer.Type.ToLowerInvariant())
            {
                case "bq":
                    dto.BQ = flat.Quantizer as VectorIndex.Quantizers.BQ;
                    break;
                // case "pq":
                //     dto.PQ = flat.Quantizer as VectorIndex.Quantizers.PQ;
                //     break;
                // case "sq":
                //     dto.SQ = flat.Quantizer as VectorIndex.Quantizers.SQ;
                //     break;
                case "rq":
                    dto.RQ = flat.Quantizer as VectorIndex.Quantizers.RQ;
                    break;
            }
        }

        return dto;
    }

    // Dynamic mapping
    /// <summary>
    /// Returns the dynamic using the specified dto
    /// </summary>
    /// <param name="dto">The dto</param>
    /// <returns>The vector index dynamic</returns>
    public static VectorIndex.Dynamic ToDynamic(this DynamicDto dto)
    {
        return new VectorIndex.Dynamic
        {
            Distance = dto.Distance,
            Threshold = dto.Threshold,
            Hnsw = dto.Hnsw?.ToHnsw(),
            Flat = dto.Flat?.ToFlat(),
        };
    }

    /// <summary>
    /// Returns the dto using the specified dynamic
    /// </summary>
    /// <param name="dynamic">The dynamic</param>
    /// <returns>The dynamic dto</returns>
    public static DynamicDto ToDto(this VectorIndex.Dynamic dynamic)
    {
        return new DynamicDto
        {
            Distance = dynamic.Distance,
            Threshold = dynamic.Threshold,
            Hnsw = dynamic.Hnsw?.ToDto(),
            Flat = dynamic.Flat?.ToDto(),
        };
    }
}

/// <summary>
/// The vector index serialization class
/// </summary>
internal static class VectorIndexSerialization
{
    /// <summary>
    /// Factories the type
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="vectorIndexConfig">The vector index config</param>
    /// <exception cref="WeaviateClientException">Unable to create VectorIndexConfig</exception>
    /// <returns>The vector index config</returns>
    internal static VectorIndexConfig? Factory(string? type, object? vectorIndexConfig)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);

        if (vectorIndexConfig is IDictionary<string, object?> vic)
        {
            var result = type switch
            {
                VectorIndex.HNSW.TypeValue => (VectorIndexConfig?)
                    VectorIndexSerialization.DeserializeHnsw(vic),
                VectorIndex.Flat.TypeValue => VectorIndexSerialization.DeserializeFlat(vic),
                VectorIndex.Dynamic.TypeValue => VectorIndexSerialization.DeserializeDynamic(vic),
                _ => null,
            };

            return result;
        }

        throw new WeaviateClientException("Unable to create VectorIndexConfig");
    }

    /// <summary>
    /// Returns the dto using the specified config
    /// </summary>
    /// <param name="config">The config</param>
    /// <returns>The object</returns>
    public static object? ToDto(VectorIndexConfig? config) =>
        config switch
        {
            VectorIndex.HNSW hnsw => (object?)hnsw.ToDto(),
            VectorIndex.Flat flat => (object?)flat.ToDto(),
            VectorIndex.Dynamic dynamic => (object?)dynamic.ToDto(),
            _ => null,
        };

    /// <summary>
    /// Serializes the hnsw using the specified hnsw
    /// </summary>
    /// <param name="hnsw">The hnsw</param>
    /// <returns>The string</returns>
    public static string SerializeHnsw(VectorIndex.HNSW hnsw)
    {
        var dto = hnsw.ToDto();
        return JsonSerializer.Serialize(dto, Rest.WeaviateRestClient.RestJsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes the hnsw using the specified json
    /// </summary>
    /// <param name="json">The json</param>
    /// <returns>The vector index hnsw</returns>
    public static VectorIndex.HNSW DeserializeHnsw(IDictionary<string, object?> json)
    {
        var dto = JsonSerializer.Deserialize<HnswDto>(
            JsonSerializer.Serialize(json, Rest.WeaviateRestClient.RestJsonSerializerOptions),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToHnsw() ?? new VectorIndex.HNSW();
    }

    /// <summary>
    /// Serializes the flat using the specified flat
    /// </summary>
    /// <param name="flat">The flat</param>
    /// <returns>The string</returns>
    public static string SerializeFlat(VectorIndex.Flat flat)
    {
        var dto = flat.ToDto();
        return JsonSerializer.Serialize(dto, Rest.WeaviateRestClient.RestJsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes the flat using the specified json
    /// </summary>
    /// <param name="json">The json</param>
    /// <returns>The vector index flat</returns>
    public static VectorIndex.Flat DeserializeFlat(IDictionary<string, object?> json)
    {
        var dto = JsonSerializer.Deserialize<FlatDto>(
            JsonSerializer.Serialize(json, Rest.WeaviateRestClient.RestJsonSerializerOptions),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToFlat() ?? new VectorIndex.Flat();
    }

    /// <summary>
    /// Serializes the dynamic using the specified dynamic
    /// </summary>
    /// <param name="dynamic">The dynamic</param>
    /// <returns>The string</returns>
    public static string SerializeDynamic(VectorIndex.Dynamic dynamic)
    {
        var dto = dynamic.ToDto();
        return JsonSerializer.Serialize(dto, Rest.WeaviateRestClient.RestJsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes the dynamic using the specified json
    /// </summary>
    /// <param name="json">The json</param>
    /// <returns>The vector index dynamic</returns>
    public static VectorIndex.Dynamic DeserializeDynamic(IDictionary<string, object?> json)
    {
        var dto = JsonSerializer.Deserialize<DynamicDto>(
            JsonSerializer.Serialize(json, Rest.WeaviateRestClient.RestJsonSerializerOptions),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToDynamic() ?? new VectorIndex.Dynamic() { Flat = null, Hnsw = null };
    }
}

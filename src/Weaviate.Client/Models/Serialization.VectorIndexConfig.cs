using System.Text.Json;
using System.Text.Json.Serialization;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.Models;

// DTOs for serialization
internal abstract class MultiVectorEncodingDto
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; } = false;
}

internal class MuveraDto : MultiVectorEncodingDto
{
    [JsonPropertyName("ksim")]
    public double? KSim { get; set; } = 4;

    [JsonPropertyName("dprojections")]
    public double? DProjections { get; set; } = 16;

    [JsonPropertyName("repetitions")]
    public double? Repetitions { get; set; } = 10;

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

internal class MultiVectorDto
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; } = false;

    [JsonPropertyName("aggregation")]
    public string? Aggregation { get; set; } = "maxSim";

    [JsonPropertyName("muvera")]
    public MuveraDto? Muvera { get; set; } = new MuveraDto();
}

internal class HnswDto
{
    [JsonPropertyName("cleanupIntervalSeconds")]
    public int? CleanupIntervalSeconds { get; set; }

    [JsonPropertyName("distance")]
    public VectorIndexConfig.VectorDistance? Distance { get; set; }

    [JsonPropertyName("dynamicEfMin")]
    public int? DynamicEfMin { get; set; }

    [JsonPropertyName("dynamicEfMax")]
    public int? DynamicEfMax { get; set; }

    [JsonPropertyName("dynamicEfFactor")]
    public int? DynamicEfFactor { get; set; }

    [JsonPropertyName("efConstruction")]
    public int? EfConstruction { get; set; }

    [JsonPropertyName("ef")]
    public int? Ef { get; set; }

    [JsonPropertyName("filterStrategy")]
    public VectorIndexConfig.VectorIndexFilterStrategy? FilterStrategy { get; set; }

    [JsonPropertyName("flatSearchCutoff")]
    public int? FlatSearchCutoff { get; set; }

    [JsonPropertyName("maxConnections")]
    public int? MaxConnections { get; set; }

    [JsonPropertyName("skip")]
    public bool? Skip { get; set; }

    [JsonPropertyName("vectorCacheMaxObjects")]
    public long? VectorCacheMaxObjects { get; set; }

    // Quantizers at root level
    [JsonPropertyName("bq")]
    public VectorIndex.Quantizers.BQ? BQ { get; set; }

    [JsonPropertyName("pq")]
    public VectorIndex.Quantizers.PQ? PQ { get; set; }

    [JsonPropertyName("sq")]
    public VectorIndex.Quantizers.SQ? SQ { get; set; }

    [JsonPropertyName("rq")]
    public VectorIndex.Quantizers.RQ? RQ { get; set; }

    [JsonPropertyName("multivector")]
    public MultiVectorDto? MultiVector { get; set; }

    [JsonPropertyName("skipDefaultQuantization")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool? SkipDefaultQuantization { get; set; }
}

internal class FlatDto
{
    [JsonPropertyName("distance")]
    public VectorIndexConfig.VectorDistance? Distance { get; set; }

    [JsonPropertyName("vectorCacheMaxObjects")]
    public long? VectorCacheMaxObjects { get; set; }

    // All quantizer properties are BQ type as specified
    [JsonPropertyName("bq")]
    public VectorIndex.Quantizers.BQ? BQ { get; set; }

    [JsonPropertyName("pq")]
    public VectorIndex.Quantizers.PQ? PQ { get; set; }

    [JsonPropertyName("sq")]
    public VectorIndex.Quantizers.SQ? SQ { get; set; }

    [JsonPropertyName("rq")]
    public VectorIndex.Quantizers.RQ? RQ { get; set; }

    [JsonPropertyName("skipDefaultQuantization")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool? SkipDefaultQuantization { get; set; }
}

internal class DynamicDto
{
    [JsonPropertyName("distance")]
    public VectorIndexConfig.VectorDistance? Distance { get; set; }

    [JsonPropertyName("threshold")]
    public int? Threshold { get; set; }

    [JsonPropertyName("hnsw")]
    public HnswDto? Hnsw { get; set; }

    [JsonPropertyName("flat")]
    public FlatDto? Flat { get; set; }
}

// Extension methods for mapping
internal static class VectorIndexMappingExtensions
{
    // Helper to get the single enabled quantizer
    private static QuantizerConfigBase? GetEnabledQuantizer(
        params QuantizerConfigBase?[] quantizers
    )
    {
        return quantizers.FirstOrDefault(q => q?.Enabled == true);
    }

    // HNSW mapping
    public static VectorIndex.HNSW ToHnsw(this HnswDto dto)
    {
        var quantizer = GetEnabledQuantizer(
            dto.BQ,
            dto.PQ,
            dto.SQ,
            dto.RQ,
            new VectorIndex.Quantizers.None() { Enabled = dto.SkipDefaultQuantization ?? true }
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
            // Only set SkipDefaultQuantization if it's true to avoid serializing false values
            SkipDefaultQuantization = dto.SkipDefaultQuantization == true ? true : null,
        };
    }

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
                    : null,
        };

        // Set the appropriate quantizer property based on type
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
    public static VectorIndex.Flat ToFlat(this FlatDto dto)
    {
        // For Flat, the Quantizer property is specifically BQ type
        var quantizer = GetEnabledQuantizer(
            dto.BQ,
            dto.PQ,
            dto.SQ,
            dto.RQ,
            new VectorIndex.Quantizers.None() { Enabled = dto.SkipDefaultQuantization ?? true }
        );

        return new VectorIndex.Flat
        {
            Distance = dto.Distance,
            VectorCacheMaxObjects = dto.VectorCacheMaxObjects,
            Quantizer = quantizer as QuantizerConfigFlat,
        };
    }

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

internal static class VectorIndexSerialization
{
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

    public static object? ToDto(VectorIndexConfig? config) =>
        config switch
        {
            VectorIndex.HNSW hnsw => (object?)hnsw.ToDto(),
            VectorIndex.Flat flat => (object?)flat.ToDto(),
            VectorIndex.Dynamic dynamic => (object?)dynamic.ToDto(),
            _ => null,
        };

    public static string SerializeHnsw(VectorIndex.HNSW hnsw)
    {
        var dto = hnsw.ToDto();
        return JsonSerializer.Serialize(dto, Rest.WeaviateRestClient.RestJsonSerializerOptions);
    }

    public static VectorIndex.HNSW DeserializeHnsw(IDictionary<string, object?> json)
    {
        var dto = JsonSerializer.Deserialize<HnswDto>(
            JsonSerializer.Serialize(json, Rest.WeaviateRestClient.RestJsonSerializerOptions),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToHnsw() ?? new VectorIndex.HNSW();
    }

    public static string SerializeFlat(VectorIndex.Flat flat)
    {
        var dto = flat.ToDto();
        return JsonSerializer.Serialize(dto, Rest.WeaviateRestClient.RestJsonSerializerOptions);
    }

    public static VectorIndex.Flat DeserializeFlat(IDictionary<string, object?> json)
    {
        var dto = JsonSerializer.Deserialize<FlatDto>(
            JsonSerializer.Serialize(json, Rest.WeaviateRestClient.RestJsonSerializerOptions),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToFlat() ?? new VectorIndex.Flat();
    }

    public static string SerializeDynamic(VectorIndex.Dynamic dynamic)
    {
        var dto = dynamic.ToDto();
        return JsonSerializer.Serialize(dto, Rest.WeaviateRestClient.RestJsonSerializerOptions);
    }

    public static VectorIndex.Dynamic DeserializeDynamic(IDictionary<string, object?> json)
    {
        var dto = JsonSerializer.Deserialize<DynamicDto>(
            JsonSerializer.Serialize(json, Rest.WeaviateRestClient.RestJsonSerializerOptions),
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToDynamic() ?? new VectorIndex.Dynamic() { Flat = null, Hnsw = null };
    }
}

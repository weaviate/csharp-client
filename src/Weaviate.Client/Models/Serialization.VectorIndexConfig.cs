using System.Text.Json;
using System.Text.Json.Serialization;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.Models;

// DTOs for serialization
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
    public VectorIndex.Quantizers.BQ? PQ { get; set; }

    [JsonPropertyName("sq")]
    public VectorIndex.Quantizers.BQ? SQ { get; set; }
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
    private static QuantizerConfig? GetEnabledQuantizer(params QuantizerConfig?[] quantizers)
    {
        return quantizers.FirstOrDefault(q => q?.Enabled == true);
    }

    // HNSW mapping
    public static VectorIndex.HNSW ToHnsw(this HnswDto dto)
    {
        var quantizer = GetEnabledQuantizer(dto.BQ, dto.PQ, dto.SQ);

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
            }
        }

        return dto;
    }

    // Flat mapping
    public static VectorIndex.Flat ToFlat(this FlatDto dto)
    {
        // For Flat, the Quantizer property is specifically BQ type
        var quantizer = GetEnabledQuantizer(dto.BQ, dto.PQ, dto.SQ);

        return new VectorIndex.Flat
        {
            Distance = dto.Distance,
            VectorCacheMaxObjects = dto.VectorCacheMaxObjects,
            Quantizer = quantizer as VectorIndex.Quantizers.BQ,
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
            // But since Flat.Quantizer is always BQ, we can determine the type from the actual quantizer
            if (flat.Quantizer is VectorIndex.Quantizers.BQ bqQuantizer)
            {
                switch (bqQuantizer.Type.ToLowerInvariant())
                {
                    case "bq":
                        dto.BQ = bqQuantizer;
                        break;
                    case "pq":
                        dto.PQ = bqQuantizer;
                        break;
                    case "sq":
                        dto.SQ = bqQuantizer;
                        break;
                }
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

        if (vectorIndexConfig is JsonElement vic)
        {
            var result = type switch
            {
                VectorIndex.HNSW.TypeValue => (VectorIndexConfig?)
                    VectorIndexSerialization.DeserializeHnsw(vic.GetRawText()),
                VectorIndex.Flat.TypeValue => VectorIndexSerialization.DeserializeFlat(
                    vic.GetRawText()
                ),
                VectorIndex.Dynamic.TypeValue => VectorIndexSerialization.DeserializeDynamic(
                    vic.GetRawText()
                ),
                _ => null,
            };

            return result;
        }

        throw new WeaviateException("Unable to create VectorIndexConfig");
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

    public static VectorIndex.HNSW DeserializeHnsw(string json)
    {
        var dto = JsonSerializer.Deserialize<HnswDto>(
            json,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToHnsw() ?? new VectorIndex.HNSW();
    }

    public static string SerializeFlat(VectorIndex.Flat flat)
    {
        var dto = flat.ToDto();
        return JsonSerializer.Serialize(dto, Rest.WeaviateRestClient.RestJsonSerializerOptions);
    }

    public static VectorIndex.Flat DeserializeFlat(string json)
    {
        var dto = JsonSerializer.Deserialize<FlatDto>(
            json,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToFlat() ?? new VectorIndex.Flat();
    }

    public static string SerializeDynamic(VectorIndex.Dynamic dynamic)
    {
        var dto = dynamic.ToDto();
        return JsonSerializer.Serialize(dto, Rest.WeaviateRestClient.RestJsonSerializerOptions);
    }

    public static VectorIndex.Dynamic DeserializeDynamic(string json)
    {
        var dto = JsonSerializer.Deserialize<DynamicDto>(
            json,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );
        return dto?.ToDynamic() ?? new VectorIndex.Dynamic() { Flat = null, Hnsw = null };
    }
}

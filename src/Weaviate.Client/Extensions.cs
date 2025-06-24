using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client;

public static class WeaviateExtensions
{
    private static readonly JsonSerializerOptions _defaultJsonSerializationOptions =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // Case-insensitive property matching
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Convert JSON names to PascalCase (C# convention)
            WriteIndented = true, // For readability
        };

    internal static Rest.Dto.Class ToDto(this Collection collection)
    {
        var data = new Rest.Dto.Class()
        {
            Class1 = collection.Name,
            Description = collection.Description,
            Properties = new List<Rest.Dto.Property>(),
            VectorConfig =
                collection
                    .VectorConfig?.ToList()
                    .ToDictionary(
                        e => e.Key,
                        e => new Rest.Dto.VectorConfig
                        {
                            VectorIndexConfig = e.Value.VectorIndexConfig.Configuration,
                            VectorIndexType = e.Value.VectorIndexType,
                            Vectorizer = e.Value.Vectorizer?.ToDto(),
                        }
                    ) ?? new Dictionary<string, Rest.Dto.VectorConfig>(),
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = collection.ModuleConfig,
        };

        foreach (var property in collection.Properties)
        {
            data.Properties.Add(
                new Rest.Dto.Property() { Name = property.Name, DataType = [.. property.DataType] }
            );
        }

        if (collection.ReplicationConfig is ReplicationConfig rc)
        {
            data.ReplicationConfig = new Rest.Dto.ReplicationConfig()
            {
                AsyncEnabled = rc.AsyncEnabled,
                DeletionStrategy = (Rest.Dto.ReplicationConfigDeletionStrategy?)rc.DeletionStrategy,
                Factor = rc.Factor,
            };
        }

        if (collection.MultiTenancyConfig is MultiTenancyConfig mtc)
        {
            data.MultiTenancyConfig = new Rest.Dto.MultiTenancyConfig()
            {
                AutoTenantActivation = mtc.AutoTenantActivation,
                AutoTenantCreation = mtc.AutoTenantCreation,
                Enabled = mtc.Enabled,
            };
        }

        if (collection.InvertedIndexConfig != null)
        {
            data.InvertedIndexConfig = new Rest.Dto.InvertedIndexConfig()
            {
                Bm25 =
                    collection.InvertedIndexConfig.Bm25 == null
                        ? null
                        : new Rest.Dto.BM25Config
                        {
                            B = collection.InvertedIndexConfig.Bm25.B,
                            K1 = collection.InvertedIndexConfig.Bm25.K1,
                        },
                Stopwords =
                    collection.InvertedIndexConfig.Stopwords == null
                        ? null
                        : new Rest.Dto.StopwordConfig
                        {
                            Additions = collection.InvertedIndexConfig.Stopwords.Additions,
                            Preset = collection.InvertedIndexConfig.Stopwords.Preset,
                            Removals = collection.InvertedIndexConfig.Stopwords.Removals,
                        },
                CleanupIntervalSeconds = collection.InvertedIndexConfig.CleanupIntervalSeconds,
                IndexNullState = collection.InvertedIndexConfig.IndexNullState,
                IndexPropertyLength = collection.InvertedIndexConfig.IndexPropertyLength,
                IndexTimestamps = collection.InvertedIndexConfig.IndexTimestamps,
            };
        }

        return data;
    }

    internal static Collection ToModel(this Rest.Dto.Class collection)
    {
        var makeVectorConfig = (string name, Rest.Dto.VectorConfig v) =>
        {
            var vectorizer = v.Vectorizer;
            var vic = VectorIndexConfig.Factory(v.VectorIndexType ?? "hnsw", v.VectorIndexConfig);
            VectorizerConfig? vc = null;

            if (vectorizer is Dictionary<string, object> vecAsDict)
            {
                if (vecAsDict.Count > 0)
                {
                    var key = vecAsDict.Keys.First();

                    vc = VectorizerConfigFactory.Create(key, vecAsDict.Values.First());
                }
            }
            else if (vectorizer is JsonElement vecAsJson)
            {
                var vec = JsonSerializer.Deserialize<Dictionary<string, object>>(vecAsJson) ?? [];

                if (vec.Count > 0)
                {
                    var item = vec.First();

                    vc = VectorizerConfigFactory.Create(item.Key, item.Value);
                }
            }

            return new VectorConfig(name) { Vectorizer = vc, VectorIndexConfig = vic };
        };

        var vectorConfig =
            collection
                .VectorConfig?.Select(e => new KeyValuePair<string, VectorConfig>(
                    e.Key,
                    makeVectorConfig(e.Key, e.Value)
                ))
                .ToDictionary() ?? new Dictionary<string, VectorConfig>();

        ShardingConfig? shardingConfig = (
            collection.ShardingConfig as JsonElement?
        )?.Deserialize<ShardingConfig>(
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }
        );

        var moduleConfig = ObjectHelper.JsonElementToExpandoObject(
            collection.ModuleConfig as JsonElement?
        );

        return new Collection()
        {
            Name = collection.Class1 ?? string.Empty,
            Description = collection.Description ?? string.Empty,
            Properties =
                collection
                    ?.Properties?.Select(p => new Property()
                    {
                        Name = p.Name ?? string.Empty,
                        DataType = p.DataType?.ToList() ?? [],
                    })
                    .ToList() ?? [],
            InvertedIndexConfig =
                (collection?.InvertedIndexConfig is Rest.Dto.InvertedIndexConfig iic)
                    ? new InvertedIndexConfig()
                    {
                        Bm25 =
                            iic.Bm25 == null
                                ? null
                                : new BM25Config
                                {
                                    B = iic.Bm25.B ?? BM25Config.Default.B,
                                    K1 = iic.Bm25.K1 ?? BM25Config.Default.K1,
                                },
                        Stopwords =
                            (iic.Stopwords is Rest.Dto.StopwordConfig swc)
                                ? new StopwordConfig
                                {
                                    Additions = swc.Additions?.ToList() ?? new List<string>(),
                                    Preset = swc.Preset ?? string.Empty,
                                    Removals = swc.Removals?.ToList() ?? new List<string>(),
                                }
                                : null,
                        CleanupIntervalSeconds = iic.CleanupIntervalSeconds.HasValue
                            ? Convert.ToInt32(iic.CleanupIntervalSeconds)
                            : InvertedIndexConfig.Default.CleanupIntervalSeconds,
                        IndexNullState =
                            iic.IndexNullState ?? InvertedIndexConfig.Default.IndexNullState,
                        IndexPropertyLength =
                            iic.IndexPropertyLength
                            ?? InvertedIndexConfig.Default.IndexPropertyLength,
                        IndexTimestamps =
                            iic.IndexTimestamps ?? InvertedIndexConfig.Default.IndexTimestamps,
                    }
                    : null,
            ModuleConfig = moduleConfig,
            MultiTenancyConfig =
                (collection?.MultiTenancyConfig is Rest.Dto.MultiTenancyConfig mtc)
                    ? new MultiTenancyConfig
                    {
                        Enabled = mtc.Enabled ?? MultiTenancyConfig.Default.Enabled,
                        AutoTenantActivation =
                            mtc.AutoTenantActivation
                            ?? MultiTenancyConfig.Default.AutoTenantActivation,
                        AutoTenantCreation =
                            mtc.AutoTenantCreation ?? MultiTenancyConfig.Default.AutoTenantCreation,
                    }
                    : null,
            ReplicationConfig =
                (collection?.ReplicationConfig is Rest.Dto.ReplicationConfig rc)
                    ? new ReplicationConfig
                    {
                        AsyncEnabled = rc.AsyncEnabled ?? ReplicationConfig.Default.AsyncEnabled,
                        Factor = rc.Factor ?? ReplicationConfig.Default.Factor,
                        DeletionStrategy = (DeletionStrategy?)rc.DeletionStrategy,
                    }
                    : null,
            ShardingConfig = shardingConfig,
            VectorConfig = vectorConfig,
        };
    }

    internal static IEnumerable<T> FromByteString<T>(this Google.Protobuf.ByteString byteString)
        where T : struct
    {
        using var stream = new MemoryStream();

        byteString.WriteTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        using var reader = new BinaryReader(stream);

        while (stream.Position < stream.Length)
        {
            object value = typeof(T) switch
            {
                Type t when t == typeof(int) => reader.ReadInt32(),
                Type t when t == typeof(long) => reader.ReadInt64(),
                Type t when t == typeof(short) => reader.ReadInt16(),
                Type t when t == typeof(float) => reader.ReadSingle(),
                Type t when t == typeof(double) => reader.ReadDouble(),
                Type t when t == typeof(byte) => reader.ReadByte(),
                Type t when t == typeof(bool) => reader.ReadBoolean(),
                Type t when t == typeof(string) => reader.ReadString(),
                _ => throw new NotSupportedException(
                    $"The type '{typeof(T).FullName}' is not supported by FromByteString<T>."
                ),
            };

            yield return (T)value;
        }
    }

    internal static Stream ToStream<T>(this IEnumerable<T> items)
        where T : struct
    {
        var stream = new MemoryStream();

        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            foreach (T item in items)
            {
                switch (item)
                {
                    case long v:
                        writer.Write(v);
                        break;
                    case int v:
                        writer.Write(v);
                        break;
                    case double v:
                        writer.Write(v);
                        break;
                    case float v:
                        writer.Write(v);
                        break;
                    case Guid v:
                        writer.Write(v.ToByteArray());
                        break;
                }
            }
            writer.Flush();
        }

        stream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning

        return stream;
    }

    internal static Google.Protobuf.ByteString ToByteString<T>(this IEnumerable<T> items)
        where T : struct
    {
        using var stream = items.ToStream();
        return Google.Protobuf.ByteString.FromStream(stream);
    }

    public static string Capitalize(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return char.ToUpper(str[0]) + str[1..];
    }

    public static string Decapitalize(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return char.ToLower(str[0]) + str[1..];
    }

    public static bool IsNativeType(this Type type)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        // Check basic value types (excluding structs that aren't primitives)
        if (underlyingType.IsPrimitive)
        {
            return true;
        }

        // Check common .NET types using TypeCode
        switch (Type.GetTypeCode(underlyingType))
        {
            case TypeCode.Boolean:
            case TypeCode.Char:
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
            case TypeCode.String:
            case TypeCode.DateTime:
                return true;
        }

        // Check for other common native types
        if (
            underlyingType == typeof(Guid)
            || underlyingType == typeof(GeoCoordinate)
            || underlyingType == typeof(TimeSpan)
            || underlyingType == typeof(DateTimeOffset)
            || underlyingType == typeof(DateTime)
            || underlyingType == typeof(Uri)
        )
        {
            return true;
        }

        // Check for arrays of native types
        if (type.IsArray)
        {
            return type.GetElementType()?.IsNativeType() == true;
        }

        // Check for generic IEnumerable<T> where T is native
        if (type.IsGenericType)
        {
            var genericDefinition = type.GetGenericTypeDefinition();

            // Handle common generic collection types
            if (
                genericDefinition == typeof(IEnumerable<>)
                || genericDefinition == typeof(ICollection<>)
                || genericDefinition == typeof(IList<>)
                || genericDefinition == typeof(List<>)
                || genericDefinition == typeof(HashSet<>)
                || genericDefinition == typeof(ISet<>)
                || genericDefinition == typeof(Queue<>)
                || genericDefinition == typeof(Stack<>)
            )
            {
                var elementType = type.GetGenericArguments()[0];
                return elementType.IsNativeType();
            }
        }

        // Check for non-generic IEnumerable (less precise, but handles ArrayList, etc.)
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            // For non-generic collections, we can't determine the element type at compile time
            // Consider them as native for serialization purposes.
            return true;
        }

        return false;
    }
}

using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;
using Weaviate.Client.Rest;

namespace Weaviate.Client;

public static class WeaviateExtensions
{
    internal static Rest.Dto.Class ToDto(this CollectionConfig collection)
    {
        var moduleConfig = new ModuleConfigList();

        if (collection.GenerativeConfig is not null)
        {
            moduleConfig[collection.GenerativeConfig.Type] = collection.GenerativeConfig;
        }

        if (collection.RerankerConfig is not null)
        {
            moduleConfig[collection.RerankerConfig.Type] = collection.RerankerConfig;
        }

        var data = new Rest.Dto.Class()
        {
            Class1 = collection.Name,
            Description = collection.Description,
            Properties = collection.Properties.Any()
                ? collection
                    .Properties.Concat(
                        collection.References.Select(r => (Weaviate.Client.Models.Property)r)
                    )
                    .Select(p => p.ToDto())
                    .ToList()
                : null,
            VectorConfig = collection.VectorConfig?.Values.ToDictionary(
                e => e.Name,
                e => new Rest.Dto.VectorConfig
                {
                    VectorIndexConfig = VectorIndexSerialization.ToDto(e.VectorIndexConfig),
                    VectorIndexType = e.VectorIndexType ?? "hnsw",
                    Vectorizer = e.Vectorizer?.ToDto(),
                }
            ),
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = moduleConfig.Any() ? moduleConfig : null,
        };

        if (collection.ReplicationConfig is Weaviate.Client.Models.ReplicationConfig rc)
        {
            data.ReplicationConfig = new Rest.Dto.ReplicationConfig()
            {
                AsyncEnabled = rc.AsyncEnabled,
                DeletionStrategy = (Rest.Dto.ReplicationConfigDeletionStrategy?)rc.DeletionStrategy,
                Factor = rc.Factor,
            };
        }

        if (collection.MultiTenancyConfig is Weaviate.Client.Models.MultiTenancyConfig mtc)
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
            var stopWordConfig =
                collection.InvertedIndexConfig.Stopwords == null
                    ? null
                    : new Rest.Dto.StopwordConfig
                    {
                        Additions = collection.InvertedIndexConfig.Stopwords.Additions,
                        Preset =
                            collection.InvertedIndexConfig.Stopwords.Preset.ToEnumMemberString(),
                        Removals = collection.InvertedIndexConfig.Stopwords.Removals,
                    };

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
                Stopwords = stopWordConfig,
                CleanupIntervalSeconds = collection.InvertedIndexConfig.CleanupIntervalSeconds,
                IndexNullState = collection.InvertedIndexConfig.IndexNullState,
                IndexPropertyLength = collection.InvertedIndexConfig.IndexPropertyLength,
                IndexTimestamps = collection.InvertedIndexConfig.IndexTimestamps,
            };
        }

        return data;
    }

    internal static CollectionConfig ToModel(this Rest.Dto.Class collection)
    {
        var makeVectorConfig = (string name, Rest.Dto.VectorConfig v) =>
        {
            var vectorizer = v.Vectorizer;

            var vic = VectorIndexSerialization.Factory(v.VectorIndexType, v.VectorIndexConfig);

            Weaviate.Client.Models.VectorizerConfig? vc = null;

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

            return new Weaviate.Client.Models.VectorConfig(name, vc, vic);
        };

        var vectorConfig = new Weaviate.Client.Models.VectorConfigList(
            [.. collection.VectorConfig?.Select(e => makeVectorConfig(e.Key, e.Value)) ?? []]
        );

        ShardingConfig? shardingConfig = (
            collection.ShardingConfig as JsonElement?
        )?.Deserialize<ShardingConfig>(WeaviateRestClient.RestJsonSerializerOptions);

        IGenerativeConfig? generative = null;
        IRerankerConfig? reranker = null;

        var moduleConfigJE = collection.ModuleConfig as JsonElement?;
        if (moduleConfigJE is not null)
        {
            var objectEnumerator = moduleConfigJE
                .Value.EnumerateObject()
                .Cast<JsonProperty?>()
                .ToList();

            var generativeJE = objectEnumerator.SingleOrDefault(p =>
                p!.Value.Name.StartsWith("generative-")
            );

            if (generativeJE is not null)
            {
                generative = GenerativeConfigSerialization.Factory(
                    generativeJE!.Value.Name,
                    generativeJE!.Value.Value
                );
            }

            var rerankerJE = objectEnumerator.SingleOrDefault(p =>
                p!.Value.Name.StartsWith("reranker-")
            );

            if (rerankerJE is not null)
            {
                reranker = RerankerConfigSerialization.Factory(
                    rerankerJE!.Value.Name,
                    rerankerJE!.Value.Value
                );
            }
        }

        var moduleConfig = (
            collection.ModuleConfig as JsonElement?
        )?.Deserialize<ModuleConfigList?>(WeaviateRestClient.RestJsonSerializerOptions);

        var invertedIndexConfig =
            (collection?.InvertedIndexConfig is Rest.Dto.InvertedIndexConfig iic)
                ? new Weaviate.Client.Models.InvertedIndexConfig()
                {
                    Bm25 =
                        iic.Bm25 == null
                            ? null
                            : new Weaviate.Client.Models.BM25Config
                            {
                                B = iic.Bm25.B ?? Weaviate.Client.Models.BM25Config.Default.B,
                                K1 = iic.Bm25.K1 ?? Weaviate.Client.Models.BM25Config.Default.K1,
                            },
                    Stopwords =
                        (iic.Stopwords is Rest.Dto.StopwordConfig swc)
                            ? new Weaviate.Client.Models.StopwordConfig
                            {
                                Additions = swc.Additions?.ToList() ?? new List<string>(),
                                Preset = (
                                    swc.Preset ?? ""
                                ).FromEnumMemberString<Weaviate.Client.Models.StopwordConfig.Presets>(),
                                Removals = swc.Removals?.ToList() ?? new List<string>(),
                            }
                            : null,
                    CleanupIntervalSeconds = iic.CleanupIntervalSeconds.HasValue
                        ? Convert.ToInt32(iic.CleanupIntervalSeconds)
                        : Weaviate.Client.Models.InvertedIndexConfig.Default.CleanupIntervalSeconds,
                    IndexNullState =
                        iic.IndexNullState
                        ?? Weaviate.Client.Models.InvertedIndexConfig.Default.IndexNullState,

                    IndexPropertyLength =
                        iic.IndexPropertyLength
                        ?? Weaviate.Client.Models.InvertedIndexConfig.Default.IndexPropertyLength,

                    IndexTimestamps =
                        iic.IndexTimestamps
                        ?? Weaviate.Client.Models.InvertedIndexConfig.Default.IndexTimestamps,
                }
                : null;

        return new CollectionConfig()
        {
            Name = collection?.Class1 ?? string.Empty,
            Description = collection?.Description ?? string.Empty,
            References =
                collection
                    ?.Properties?.Where(p => p.DataType?.Any(t => char.IsUpper(t.First())) ?? false)
                    .Select(p =>
                        (Reference)
                            new Weaviate.Client.Models.Property()
                            {
                                Name = p.Name ?? string.Empty,
                                DataType = p.DataType?.ToList() ?? [],
                                Description = p.Description,
                            }
                    )
                    .ToArray() ?? [],
            Properties =
                collection
                    ?.Properties?.Where(p => p.DataType?.All(t => char.IsLower(t.First())) ?? false)
                    .Select(p => p.ToModel())
                    .ToArray() ?? [],
            InvertedIndexConfig = invertedIndexConfig,
            ModuleConfig = moduleConfig,
            RerankerConfig = reranker,
            GenerativeConfig = generative,
            MultiTenancyConfig =
                (collection?.MultiTenancyConfig is Rest.Dto.MultiTenancyConfig mtc)
                    ? new Weaviate.Client.Models.MultiTenancyConfig
                    {
                        Enabled =
                            mtc.Enabled
                            ?? Weaviate.Client.Models.MultiTenancyConfig.Default.Enabled,

                        AutoTenantActivation =
                            mtc.AutoTenantActivation
                            ?? Weaviate
                                .Client
                                .Models
                                .MultiTenancyConfig
                                .Default
                                .AutoTenantActivation,

                        AutoTenantCreation =
                            mtc.AutoTenantCreation
                            ?? Weaviate.Client.Models.MultiTenancyConfig.Default.AutoTenantCreation,
                    }
                    : null,
            ReplicationConfig =
                (collection?.ReplicationConfig is Rest.Dto.ReplicationConfig rc)
                    ? new Weaviate.Client.Models.ReplicationConfig
                    {
                        AsyncEnabled =
                            rc.AsyncEnabled
                            ?? Weaviate.Client.Models.ReplicationConfig.Default.AsyncEnabled,

                        Factor =
                            rc.Factor ?? Weaviate.Client.Models.ReplicationConfig.Default.Factor,

                        DeletionStrategy = (Weaviate.Client.Models.DeletionStrategy?)
                            rc.DeletionStrategy,
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

    internal static Stream ToStream<T>(this IEnumerable<T[]> items)
        where T : struct
    {
        var stream = new MemoryStream();

        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            short dimensions = (short)(items.FirstOrDefault()?.Length ?? 0);
            writer.Write(dimensions);
            if (dimensions == 0)
            {
                throw new ArgumentException("dimension cannot be zero.");
            }

            foreach (var value in items.SelectMany(array => array))
            {
                switch (value)
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

    internal static Google.Protobuf.ByteString ToByteString(this Vector vector)
    {
        if (vector is null || vector.Count == 0)
        {
            return Google.Protobuf.ByteString.Empty;
        }

        return vector.ValueType switch
        {
            Type t when t == typeof(float) => ToByteString(vector as IEnumerable<float>),
            Type t when t == typeof(double) => ToByteString(vector as IEnumerable<double>),
            Type t when t == typeof(int) => ToByteString(vector as IEnumerable<int>),
            Type t when t == typeof(long) => ToByteString(vector as IEnumerable<long>),
            Type t when t == typeof(short) => ToByteString(vector as IEnumerable<short>),
            Type t when t == typeof(byte) => ToByteString(vector as IEnumerable<byte>),
            Type t when t == typeof(bool) => ToByteString(vector as IEnumerable<bool>),
            Type t when t == typeof(decimal) => ToByteString(vector as IEnumerable<decimal>),
            Type t when t == typeof(float[]) => ToByteString(vector as IEnumerable<float[]>),
            Type t when t == typeof(double[]) => ToByteString(vector as IEnumerable<double[]>),
            Type t when t == typeof(int[]) => ToByteString(vector as IEnumerable<int[]>),
            Type t when t == typeof(long[]) => ToByteString(vector as IEnumerable<long[]>),
            Type t when t == typeof(short[]) => ToByteString(vector as IEnumerable<short[]>),
            Type t when t == typeof(byte[]) => ToByteString(vector as IEnumerable<byte[]>),
            Type t when t == typeof(bool[]) => ToByteString(vector as IEnumerable<bool[]>),
            Type t when t == typeof(decimal[]) => ToByteString(vector as IEnumerable<decimal[]>),
            _ => throw new NotSupportedException(
                $"The type '{vector.ValueType.FullName}' is not supported by ToByteString."
            ),
        };
    }

    internal static Google.Protobuf.ByteString ToByteString<T>(this IEnumerable<T[]>? items)
        where T : struct
    {
        using var stream = items?.ToStream();

        if (stream is null || stream.Length == 0)
        {
            return Google.Protobuf.ByteString.Empty;
        }

        return Google.Protobuf.ByteString.FromStream(stream);
    }

    internal static Google.Protobuf.ByteString ToByteString<T>(this IEnumerable<T>? items)
        where T : struct
    {
        using var stream = items?.ToStream();

        if (stream is null || stream.Length == 0)
        {
            return Google.Protobuf.ByteString.Empty;
        }

        return Google.Protobuf.ByteString.FromStream(stream);
    }

    /// <summary>
    /// Converts an enum value to its wire-format string using EnumMemberAttribute.
    /// </summary>
    internal static T ToEquivalentEnum<T>(this Enum value)
        where T : struct, Enum
    {
        var str = value.ToEnumMemberString();
        if (!str.IsValidEnumMemberString<T>())
            throw new InvalidEnumWireFormatException($"Can't translate Enum value: {str}");

        return value.ToEnumMemberString().FromEnumMemberString<T>();
    }

    /// <summary>
    /// Converts an enum value to its wire-format string using EnumMemberAttribute.
    /// </summary>
    internal static string ToEnumMemberString<T>(this T value)
        where T : Enum
    {
        var type = typeof(T);
        var member = type.GetMember(value.ToString()).FirstOrDefault();
        var attr = member?.GetCustomAttribute<EnumMemberAttribute>();
        return attr?.Value ?? value.ToString();
    }

    /// <summary>
    /// Converts an enum value to its wire-format string using EnumMemberAttribute.
    /// </summary>
    internal static string ToEnumMemberString<T>(this Nullable<T> value)
        where T : struct, Enum
    {
        if (!value.HasValue)
            throw new ArgumentNullException(nameof(value));

        return ToEnumMemberString(value!.Value);
    }

    /// <summary>
    /// Parses a wire-format string to an enum value using EnumMemberAttribute.
    /// Throws ArgumentException if no match is found.
    /// </summary>
    internal static T FromEnumMemberString<T>(this string value)
        where T : struct, Enum
    {
        var type = typeof(T);
        foreach (var field in type.GetFields())
        {
            var attr = field.GetCustomAttribute<EnumMemberAttribute>();
            if ((attr?.Value ?? field.Name).Equals(value, StringComparison.OrdinalIgnoreCase))
                return (T)field.GetValue(null)!;
        }
        throw new ArgumentException($"Value '{value}' is not valid for enum {type.Name}");
    }

    /// <summary>
    /// Validates if a string is a valid wire-format value for the enum.
    /// </summary>
    internal static bool IsValidEnumMemberString<T>(this string value)
        where T : Enum
    {
        var type = typeof(T);
        return type.GetFields()
            .Any(field =>
            {
                var attr = field.GetCustomAttribute<EnumMemberAttribute>();
                return (attr?.Value ?? field.Name).Equals(
                    value,
                    StringComparison.OrdinalIgnoreCase
                );
            });
    }

    internal static string Capitalize(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return char.ToUpper(str[0]) + str[1..];
    }

    internal static string Decapitalize(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return char.ToLower(str[0]) + str[1..];
    }

    internal static bool IsNativeType(this Type type)
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

using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;
using Weaviate.Client.Rest;

namespace Weaviate.Client;

public static class WeaviateExtensions
{
    internal static Rest.Dto.Class ToDto(this Collection collection)
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
                ?
                [
                    .. collection
                        .Properties.Concat(collection.References.Select(r => (Property)r))
                        .Select(p => new Rest.Dto.Property()
                        {
                            Name = p.Name,
                            DataType = [.. p.DataType],
                            Description = p.Description,
                            IndexFilterable = p.IndexFilterable,
#pragma warning disable CS0612 // Type or member is obsolete
                            IndexInverted = p.IndexInverted,
#pragma warning restore CS0612 // Type or member is obsolete
                            IndexRangeFilters = p.IndexRangeFilters,
                            IndexSearchable = p.IndexSearchable,
                            Tokenization = (Rest.Dto.PropertyTokenization?)p.PropertyTokenization,
                        }),
                ]
                : null,
            VectorConfig = collection.VectorConfig?.Values.ToDictionary(
                e => e.Name,
                e => new Rest.Dto.VectorConfig
                {
                    VectorIndexConfig = VectorIndexSerialization.ToDto(e.VectorIndexConfig),
                    VectorIndexType = e.VectorIndexType,
                    Vectorizer = e.Vectorizer?.ToDto(),
                }
            ),
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = moduleConfig.Any() ? moduleConfig : null,
        };

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

    internal static Collection ToModel(this Rest.Dto.Class collection)
    {
        var makeVectorConfig = (string name, Rest.Dto.VectorConfig v) =>
        {
            var vectorizer = v.Vectorizer;

            var vic = VectorIndexSerialization.Factory(v.VectorIndexType, v.VectorIndexConfig);

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

            return new VectorConfig(name, vc, vic);
        };

        var vectorConfig = new VectorConfigList(
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
                                Preset = (
                                    swc.Preset ?? ""
                                ).FromEnumMemberString<StopwordConfig.Presets>(),
                                Removals = swc.Removals?.ToList() ?? new List<string>(),
                            }
                            : null,
                    CleanupIntervalSeconds = iic.CleanupIntervalSeconds.HasValue
                        ? Convert.ToInt32(iic.CleanupIntervalSeconds)
                        : InvertedIndexConfig.Default.CleanupIntervalSeconds,
                    IndexNullState =
                        iic.IndexNullState ?? InvertedIndexConfig.Default.IndexNullState,
                    IndexPropertyLength =
                        iic.IndexPropertyLength ?? InvertedIndexConfig.Default.IndexPropertyLength,
                    IndexTimestamps =
                        iic.IndexTimestamps ?? InvertedIndexConfig.Default.IndexTimestamps,
                }
                : null;

        return new Collection()
        {
            Name = collection?.Class1 ?? string.Empty,
            Description = collection?.Description ?? string.Empty,
            References =
                collection
                    ?.Properties?.Where(p => p.DataType?.Any(t => char.IsUpper(t.First())) ?? false)
                    .Select(p =>
                        (ReferenceProperty)
                            new Property()
                            {
                                Name = p.Name ?? string.Empty,
                                DataType = p.DataType?.ToList() ?? [],
                                Description = p.Description,
                            }
                    )
                    .ToList() ?? [],
            Properties =
                collection
                    ?.Properties?.Where(p => p.DataType?.All(t => char.IsLower(t.First())) ?? false)
                    .Select(p => new Property()
                    {
                        Name = p.Name ?? string.Empty,
                        DataType = p.DataType?.ToList() ?? [],
                        Description = p.Description,
                        IndexFilterable = p.IndexFilterable,
#pragma warning disable CS0612 // Type or member is obsolete
                        IndexInverted = p.IndexInverted,
#pragma warning restore CS0612 // Type or member is obsolete
                        IndexRangeFilters = p.IndexRangeFilters,
                        IndexSearchable = p.IndexSearchable,
                        PropertyTokenization = (PropertyTokenization?)p.Tokenization,
                    })
                    .ToList() ?? [],
            InvertedIndexConfig = invertedIndexConfig,
            ModuleConfig = moduleConfig,
            RerankerConfig = reranker,
            GenerativeConfig = generative,
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

    public static VectorData<T> ToVectorData<T>(this IEnumerable<T> values)
        where T : struct
    {
        return new(values);
    }

    public static MultiVectorData<T> ToVectorData<T>(this IEnumerable<T[]> values)
        where T : struct
    {
        return new(values);
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

    internal static Google.Protobuf.ByteString ToByteString(this AbstractVectorData vector)
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
            Type t when t == typeof(float[]) => ToByteString(vector as IEnumerable<float[]>),
            Type t when t == typeof(double[]) => ToByteString(vector as IEnumerable<double[]>),
            Type t when t == typeof(int[]) => ToByteString(vector as IEnumerable<int[]>),
            Type t when t == typeof(long[]) => ToByteString(vector as IEnumerable<long[]>),
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

    internal static string? ToEnumMemberString(this Enum? enumValue)
    {
        if (enumValue == null)
        {
            return null;
        }

        return enumValue
                .GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<EnumMemberAttribute>()
                ?.Value ?? enumValue.ToString();
    }

    internal static T? FromEnumMemberString<T>(this string? str)
    {
        if (str is null)
        {
            return default(T);
        }

        var enumType = typeof(T);
        foreach (var name in Enum.GetNames(enumType))
        {
            var enumMemberAttribute = (EnumMemberAttribute)(
                enumType
                    .GetField(name)!
                    .GetCustomAttributes(typeof(EnumMemberAttribute), true)
                    .Single()
            );

            if (enumMemberAttribute.Value == str)
                return (T)Enum.Parse(enumType, name);
        }

        return default(T);
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

using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client;

public static class WeaviateExtensions
{
    private static readonly JsonSerializerOptions _defaultJsonSerializationOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, // Case-insensitive property matching
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Convert JSON names to PascalCase (C# convention)
        WriteIndented = true, // For readability
    };

    public static WeaviateObject<T> ToWeaviateObject<T>(this WeaviateObject<dynamic> data)
    {
        var obj = (T)BuildConcreteTypeObjectFromProperties<T>(data.Data);

        return new WeaviateObject<T>(data.CollectionName ?? string.Empty)
        {
            Data = obj,
            ID = data.ID,
            Additional = data.Additional,
            Metadata = new WeaviateObject<T>.ObjectMetadata
            {
                CreationTime = data.Metadata.CreationTime,
                LastUpdateTime = data.Metadata.LastUpdateTime,
            },
            Tenant = data.Tenant,
            Vector = data.Vector,
            Vectors = data.Vectors,
        };
    }

    public static WeaviateObject<T> ToWeaviateObject<T>(this Rest.Dto.Object data)
    {
        return new WeaviateObject<T>(data.Class ?? string.Empty)
        {
            Data = BuildConcreteTypeObjectFromProperties<T>(data.Properties),
            ID = data.Id,
            Additional = data.Additional ?? [],
            Metadata = new WeaviateObject<T>.ObjectMetadata
            {
                CreationTime = data.CreationTimeUnix.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.CreationTimeUnix.Value).DateTime : null,
                LastUpdateTime = data.LastUpdateTimeUnix.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.LastUpdateTimeUnix.Value).DateTime : null,
            },
            Tenant = data.Tenant,
            Vector = data.Vector,
            Vectors = data.Vectors?.ToDictionary(kv => kv.Key, kv => (IList<float>)kv.Value) ?? [],
        };
    }

    public static WeaviateObject<T> ToWeaviateObject<T>(this Models.WeaviateObject data)
    {
        var obj = (T)BuildConcreteTypeObjectFromProperties<T>(data.Data);

        return new WeaviateObject<T>(data.CollectionName ?? string.Empty)
        {
            Data = obj,
            ID = data.ID,
            Additional = data.Additional,
            Metadata = new WeaviateObject<T>.ObjectMetadata
            {
                CreationTime = data.Metadata.CreationTime,
                LastUpdateTime = data.Metadata.LastUpdateTime,
            },
            References = data.References,
            Tenant = data.Tenant,
            Vector = data.Vector,
            Vectors = data.Vectors,
            Properties = data.Properties,
        };
    }

    internal static T? BuildConcreteTypeObjectFromProperties<T>(object? data)
    {
        switch (data)
        {
            case JsonElement properties:
                return properties.Deserialize<T>(_defaultJsonSerializationOptions);
            case IDictionary<string, object> dict:
                return UnmarshallProperties<T>(dict);
            case null:
                return default;
            default:
                throw new NotSupportedException($"Unsupported type for properties: {data?.GetType()}");
        }
    }

    private static T? UnmarshallProperties<T>(IDictionary<string, object> dict)
    {
        if (dict == null)
            throw new ArgumentNullException(nameof(dict));

        // Create an instance of T using the default constructor
        var props = Activator.CreateInstance<T>();

        if (props is IDictionary<string, object> target)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value is IDictionary<string, object> subDict)
                {
                    dynamic? v = UnmarshallProperties<dynamic>(subDict);

                    target[Capitalize(kvp.Key)] = v ?? subDict;
                }
                else
                {
                    target[Capitalize(kvp.Key)] = kvp.Value;
                }
            }
            return props;
        }

        var type = typeof(T);
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var matchingKey = dict.Keys.FirstOrDefault(k =>
                string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase));

            if (matchingKey != null)
            {
                var value = dict[matchingKey];
                if (value != null)
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(props, convertedValue);
                    }
                    catch
                    {
                        // Skip if conversion fails
                        continue;
                    }
                }
            }
        }

        return props;
    }

    internal static IEnumerable<WeaviateObject<T>> ToObjects<T>(this IEnumerable<WeaviateObject<dynamic>> list)
    {
        return list.Select(ToWeaviateObject<T>);
    }

    internal static IEnumerable<WeaviateObject<T>> ToObjects<T>(this IEnumerable<Rest.Dto.Object> list)
    {
        return list.Select(ToWeaviateObject<T>);
    }

    internal static Rest.Dto.Class ToDto(this Collection collection)
    {
        var data = new Rest.Dto.Class()
        {
            Class1 = collection.Name,
            Description = collection.Description,
            Properties = new List<Rest.Dto.Property>(),
            VectorConfig = collection.VectorConfig?.ToList()
                .ToDictionary(
                    e => e.Key,
                    e => new Rest.Dto.VectorConfig
                    {
                        VectorIndexConfig = e.Value.VectorIndexConfig,
                        VectorIndexType = e.Value.VectorIndexType,
                        Vectorizer = e.Value.Vectorizer,
                    }
                ) ?? new Dictionary<string, Rest.Dto.VectorConfig>(),
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = collection.ModuleConfig,
            VectorIndexType = collection.VectorIndexType,
            VectorIndexConfig = collection.VectorIndexConfig,
            Vectorizer = collection.Vectorizer,
        };

        foreach (var property in collection.Properties)
        {
            data.Properties.Add(new Rest.Dto.Property()
            {
                Name = property.Name,
                DataType = [.. property.DataType]
            });
        }

        if (collection.ReplicationConfig is ReplicationConfig rc)
        {
            data.ReplicationConfig = new Rest.Dto.ReplicationConfig()
            {
                AsyncEnabled = rc.AsyncEnabled,
                DeletionStrategy = (Rest.Dto.ReplicationConfigDeletionStrategy?)rc.DeletionStrategy,
                Factor = rc.Factor
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
                Bm25 = collection.InvertedIndexConfig.Bm25 == null ? null : new Rest.Dto.BM25Config
                {
                    B = collection.InvertedIndexConfig.Bm25.B,
                    K1 = collection.InvertedIndexConfig.Bm25.K1,
                },
                Stopwords = collection.InvertedIndexConfig.Stopwords == null ? null : new Rest.Dto.StopwordConfig
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
        var tf = (Rest.Dto.VectorConfig v) =>
        {
            var vic = v.VectorIndexConfig;
            var vit = v.VectorIndexType;
            var vectorizer = v.Vectorizer;

            var vc = new VectorConfig()
            {
                VectorIndexConfig = vic,
                VectorIndexType = vit
            };

            if (vectorizer is Dictionary<string, object> vecAsDict)
            {
                vc.Vectorizer = vecAsDict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(kvp.Value))
                );
            }
            else if (vectorizer is System.Text.Json.JsonElement vecAsJson)
            {
                vc.Vectorizer = JsonSerializer.Deserialize<Dictionary<string, object>>(vecAsJson);
            }

            return vc;
        };

        var vectorConfig = collection.VectorConfig?
            .Select(e => new KeyValuePair<string, VectorConfig>(e.Key, tf(e.Value)))
            .ToDictionary()
            ?? new Dictionary<string, VectorConfig>();

        return new Collection()
        {
            Name = collection.Class1,
            Description = collection.Description,
            Properties = collection.Properties.Select(p => new Property()
            {
                Name = p.Name,
                DataType = p.DataType.ToList()
            }).ToList(),
            InvertedIndexConfig = (collection.InvertedIndexConfig is Rest.Dto.InvertedIndexConfig iic)
                ? new InvertedIndexConfig()
                {
                    Bm25 = iic.Bm25 == null ? null : new BM25Config
                    {
                        B = iic.Bm25.B ?? BM25Config.Default.B,
                        K1 = iic.Bm25.K1 ?? BM25Config.Default.K1,
                    },
                    Stopwords = (iic.Stopwords is Rest.Dto.StopwordConfig swc)
                    ? new StopwordConfig
                    {
                        Additions = swc.Additions?.ToList() ?? new List<string>(),
                        Preset = swc.Preset ?? string.Empty,
                        Removals = swc.Removals?.ToList() ?? new List<string>(),
                    } : null,
                    CleanupIntervalSeconds = iic.CleanupIntervalSeconds.HasValue ? Convert.ToInt32(iic.CleanupIntervalSeconds) : InvertedIndexConfig.Default.CleanupIntervalSeconds,
                    IndexNullState = iic.IndexNullState ?? InvertedIndexConfig.Default.IndexNullState,
                    IndexPropertyLength = iic.IndexPropertyLength ?? InvertedIndexConfig.Default.IndexPropertyLength,
                    IndexTimestamps = iic.IndexTimestamps ?? InvertedIndexConfig.Default.IndexTimestamps,
                } : null,
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = collection.ModuleConfig,
            ReplicationConfig = (collection.ReplicationConfig is Rest.Dto.ReplicationConfig rc)
                ? new ReplicationConfig
                {
                    AsyncEnabled = rc.AsyncEnabled ?? ReplicationConfig.Default.AsyncEnabled,
                    Factor = rc.Factor ?? ReplicationConfig.Default.Factor,
                    DeletionStrategy = (DeletionStrategy?)rc.DeletionStrategy,
                } : null,
            MultiTenancyConfig = (collection.MultiTenancyConfig is Rest.Dto.MultiTenancyConfig mtc)
                ? new MultiTenancyConfig
                {
                    Enabled = mtc.Enabled ?? MultiTenancyConfig.Default.Enabled,
                    AutoTenantActivation = mtc.AutoTenantActivation ?? MultiTenancyConfig.Default.AutoTenantActivation,
                    AutoTenantCreation = mtc.AutoTenantCreation ?? MultiTenancyConfig.Default.AutoTenantCreation,
                } : null,
            VectorConfig = vectorConfig,
            Vectorizer = collection.Vectorizer,
            VectorIndexType = collection.VectorIndexType,
            VectorIndexConfig = collection.VectorIndexConfig,
        };
    }

    internal static IEnumerable<T> FromByteString<T>(this Google.Protobuf.ByteString byteString) where T : struct
    {
        using var stream = new MemoryStream();

        byteString.WriteTo(stream);
        stream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning

        using var reader = new BinaryReader(stream);

        while (stream.Position < stream.Length)
        {
            // Temporary variable to hold the read object before casting
            object value = Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Int32 => reader.ReadInt32(),
                TypeCode.Single => reader.ReadSingle(),
                TypeCode.Double => reader.ReadDouble(),
                TypeCode.String => reader.ReadString(),
                TypeCode.Byte => reader.ReadByte(),
                TypeCode.Boolean => reader.ReadBoolean(),
                _ => throw new NotSupportedException($"The type '{typeof(T).FullName}' is not supported by FromStream<T>."),// Handle unsupported types gracefully
            };

            // Cast the object to T and yield
            yield return (T)value;
        }
    }

    internal static MemoryStream ToStream<T>(this IEnumerable<T> items) where T : struct
    {
        var stream = new MemoryStream();

        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))

        {
            foreach (T item in items)
            {
                switch (item)
                {
                    case int v: writer.Write(v); break;
                    case float v: writer.Write(v); break;
                }
            }
            writer.Flush();
        }

        stream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning

        return stream;
    }

    internal static Google.Protobuf.ByteString ToByteString<T>(this IEnumerable<T> items) where T : struct
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
}
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

    internal static WeaviateObject<T> ToWeaviateObject<T>(this Rest.Dto.WeaviateObject data)
    {
        return new WeaviateObject<T>(data.Class ?? string.Empty)
        {
            Data = BuildConcreteTypeObjectFromProperties<T>(data.Properties),
            ID = data.Id,
            Additional = data.Additional,
            CreationTime = data.CreationTimeUnix.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.CreationTimeUnix.Value).DateTime : null,
            LastUpdateTime = data.LastUpdateTimeUnix.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.LastUpdateTimeUnix.Value).DateTime : null,
            Tenant = data.Tenant,
            Vector = data.Vector,
            Vectors = data.Vectors,
        };
    }

    internal static T? BuildConcreteTypeObjectFromProperties<T>(object? data)
    {
        T? props = default;

        switch (data)
        {
            case JsonElement properties:
                props = properties.Deserialize<T>(_defaultJsonSerializationOptions);
                break;
            case IDictionary<string, object?> dict:
                props = UnmarshallProperties<T>(dict);
                break;
            case null:
                return props;
            default:
                throw new NotSupportedException($"Unsupported type for properties: {data?.GetType()}");
        }

        return props;
    }

    private static T? UnmarshallProperties<T>(IDictionary<string, object?> dict)
    {
        if (dict == null)
            throw new ArgumentNullException(nameof(dict));

        // Create an instance of T using the default constructor
        var props = Activator.CreateInstance<T>();

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

    internal static IEnumerable<WeaviateObject<T>> ToObjects<T>(this IEnumerable<Rest.Dto.WeaviateObject> list)
    {
        return list.Select(ToWeaviateObject<T>);
    }

    internal static Rest.Dto.CollectionGeneric ToDto(this Models.Collection collection)
    {
        var data = new Rest.Dto.CollectionGeneric()
        {
            Class = collection.Name,
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

        if (collection.ReplicationConfig is Models.ReplicationConfig rc)
        {
            data.ReplicationConfig = new Rest.Dto.ReplicationConfig()
            {
                AsyncEnabled = rc.AsyncEnabled,
                DeletionStrategy = (Rest.Dto.DeletionStrategy?)rc.DeletionStrategy,
                Factor = rc.Factor
            };
        }

        if (collection.MultiTenancyConfig is Models.MultiTenancyConfig mtc)
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

    internal static Models.Collection ToModel(this Rest.Dto.CollectionGeneric collection)
    {
        return new Models.Collection()
        {
            Name = collection.Class,
            Description = collection.Description,
            Properties = collection.Properties.Select(p => new Models.Property()
            {
                Name = p.Name,
                DataType = p.DataType.ToList()
            }).ToList(),
            InvertedIndexConfig = (collection.InvertedIndexConfig is Rest.Dto.InvertedIndexConfig iic)
                ? new Models.InvertedIndexConfig()
                {
                    Bm25 = iic.Bm25 == null ? null : new Models.BM25Config
                    {
                        B = iic.Bm25.B,
                        K1 = iic.Bm25.K1,
                    },
                    Stopwords = (iic.Stopwords is Rest.Dto.StopwordConfig swc)
                    ? new Models.StopwordConfig
                    {
                        Additions = swc.Additions,
                        Preset = swc.Preset,
                        Removals = swc.Removals,
                    } : null,
                    CleanupIntervalSeconds = iic.CleanupIntervalSeconds,
                    IndexNullState = iic.IndexNullState,
                    IndexPropertyLength = iic.IndexPropertyLength,
                    IndexTimestamps = iic.IndexTimestamps,
                } : null,
            ShardingConfig = collection.ShardingConfig,
            ModuleConfig = collection.ModuleConfig,
            ReplicationConfig = (collection.ReplicationConfig is Rest.Dto.ReplicationConfig rc)
                ? new Models.ReplicationConfig
                {
                    AsyncEnabled = rc.AsyncEnabled,
                    Factor = rc.Factor,
                    DeletionStrategy = (Models.DeletionStrategy?)rc.DeletionStrategy,
                } : null,
            MultiTenancyConfig = (collection.MultiTenancyConfig is Rest.Dto.MultiTenancyConfig mtc)
                ? new Models.MultiTenancyConfig
                {
                    Enabled = mtc.Enabled,
                    AutoTenantActivation = mtc.AutoTenantActivation,
                    AutoTenantCreation = mtc.AutoTenantCreation,
                } : null,
            VectorConfig =
                collection.VectorConfig?.ToList()
                .ToDictionary(
                    e => e.Key,
                    e => new Models.VectorConfig
                    {
                        VectorIndexConfig = e.Value.VectorIndexConfig,
                        VectorIndexType = e.Value.VectorIndexType,
                        Vectorizer = e.Value.Vectorizer,
                    }
                    ) ?? new Dictionary<string, Models.VectorConfig>(),
            Vectorizer = collection.Vectorizer,
            VectorIndexType = collection.VectorIndexType,
            VectorIndexConfig = collection.VectorIndexConfig,
        };
    }

    internal static IEnumerable<T> FromStream<T>(this Stream stream) where T : struct
    {
        // Ensure the stream is readable and seekable for the Length check
        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable.", nameof(stream));
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable to check Length.", nameof(stream));

        // Keep the stream open after the reader is disposed
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

    internal static byte[] ToByteArray<T>(this IEnumerable<T> items) where T : struct
    {
        using (var stream = items.ToStream())
        {
            return stream.ToArray();
        }
    }

    public static string Capitalize(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return char.ToUpper(str[0]) + str[1..];
    }
}
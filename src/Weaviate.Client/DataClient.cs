using System.Collections.Frozen;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Weaviate.Client.Models;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client;

internal class ObjectHelper
{
    internal static T? UnmarshallProperties<T>(IDictionary<string, object?> dict)
        where T : new()
    {
        ArgumentNullException.ThrowIfNull(dict);

        // Create an instance of T using the default constructor
        var instance = new T();

        if (instance is IDictionary<string, object?> target)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value is IDictionary<string, object?> subDict)
                {
                    object? nestedValue = UnmarshallProperties<ExpandoObject>(subDict);

                    target[kvp.Key.Capitalize()] = nestedValue ?? subDict;
                }
                else
                {
                    if (kvp.Value?.GetType() == typeof(Rest.Dto.GeoCoordinates))
                    {
                        var value = (Rest.Dto.GeoCoordinates)kvp.Value;
                        target[kvp.Key.Capitalize()] = new GeoCoordinate(
                            value.Latitude ?? 0f,
                            value.Longitude ?? 0f
                        );
                    }
                    else
                    {
                        target[kvp.Key.Capitalize()] = kvp.Value;
                    }
                }
            }
            return instance;
        }

        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToArray();

        foreach (var property in properties)
        {
            var matchingKey = dict.Keys.FirstOrDefault(k =>
                string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase)
            );

            if (matchingKey is null)
            {
                continue;
            }

            var value = dict[matchingKey];

            try
            {
                var convertedValue = ConvertValue(value, property.PropertyType);
                property.SetValue(instance, convertedValue);
            }
            catch (Exception ex)
            {
                // Skip if conversion fails
                Debug.WriteLine($"Failed to convert property {property.Name}: {ex.Message}");
                continue;
            }
        }

        return instance;
    }

    private static object? ConvertValue(object? value, System.Type targetType)
    {
        // Handle null values
        if (value == null)
        {
            if (IsNullableType(targetType) || !targetType.IsValueType)
            {
                return null;
            }
            // For non-nullable value types, return default value
            return Activator.CreateInstance(targetType);
        }

        // If types already match, return as-is
        if (targetType.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        // Handle nullable types
        if (IsNullableType(targetType))
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType)!;
            return ConvertValue(value, underlyingType);
        }

        // Handle nested objects (dictionaries -> custom types)
        if (
            value is IDictionary<string, object?> nestedDict
            && !typeof(IDictionary<string, object?>).IsAssignableFrom(targetType)
        )
        {
            var method = typeof(ObjectHelper)
                .GetMethod("UnmarshallProperties", BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(targetType);
            return method.Invoke(null, new object[] { nestedDict });
        }

        // Handle collections
        if (
            IsCollectionType(targetType)
            && value is System.Collections.IEnumerable enumerable
            && !(value is string)
        )
        {
            return ConvertCollection(enumerable, targetType);
        }

        // Handle enums
        if (targetType.IsEnum)
        {
            if (value is string stringValue)
            {
                return System.Enum.Parse(targetType, stringValue, true);
            }
            return System.Enum.ToObject(targetType, value);
        }

        // Try TypeConverter first (handles more cases than Convert.ChangeType)
        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(value.GetType()))
        {
            return converter.ConvertFrom(value);
        }

        // Fallback to Convert.ChangeType for basic types
        return Convert.ChangeType(value, targetType);
    }

    private static bool IsNullableType(System.Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static bool IsCollectionType(System.Type type)
    {
        return type.IsArray
            || (
                type.IsGenericType
                && (
                    type.GetGenericTypeDefinition() == typeof(List<>)
                    || type.GetGenericTypeDefinition() == typeof(IList<>)
                    || type.GetGenericTypeDefinition() == typeof(ICollection<>)
                    || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                )
            );
    }

    private static object? ConvertCollection(
        System.Collections.IEnumerable source,
        System.Type targetType
    )
    {
        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType()!;
            var items = new List<object?>();

            foreach (var item in source)
            {
                items.Add(ConvertValue(item, elementType));
            }

            var array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                array.SetValue(items[i], i);
            }
            return array;
        }

        if (targetType.IsGenericType)
        {
            var elementType = targetType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (System.Collections.IList)Activator.CreateInstance(listType)!;

            foreach (var item in source)
            {
                list.Add(ConvertValue(item, elementType));
            }

            return list;
        }

        // Fallback - convert to object array
        var fallbackItems = new List<object?>();
        foreach (var item in source)
        {
            fallbackItems.Add(item);
        }
        return fallbackItems.ToArray();
    }

    internal static IDictionary<string, object?> BuildDataTransferObject(object? data)
    {
        var obj = new ExpandoObject();
        var propDict = obj as IDictionary<string, object?>;

        if (data is null)
        {
            return propDict;
        }

        foreach (var propertyInfo in data.GetType().GetProperties())
        {
            if (!propertyInfo.CanRead)
                continue; // skip non-readable properties

            var value = propertyInfo.GetValue(data);

            if (value is null)
            {
                continue;
            }
            else if (propertyInfo.PropertyType.IsNativeType())
            {
                propDict[propertyInfo.Name] = value;
            }
            else if (propertyInfo.PropertyType == typeof(GeoCoordinate))
            {
                var newValue = (GeoCoordinate)value;
                propDict[propertyInfo.Name] = new GeoCoordinates
                {
                    Latitude = newValue.Latitude,
                    Longitude = newValue.Longitude,
                };
            }
            else
            {
                propDict[propertyInfo.Name] = BuildDataTransferObject(value); // recursive call
            }
        }

        return obj;
    }

    internal static V1.BatchObject.Types.Properties BuildBatchProperties<TProps>(TProps data)
    {
        var props = new V1.BatchObject.Types.Properties();

        if (data is null)
        {
            return props;
        }

        Google.Protobuf.WellKnownTypes.Struct? nonRefProps = null;

        foreach (var propertyInfo in data.GetType().GetProperties())
        {
            if (propertyInfo is null)
            {
                continue;
            }

            if (!propertyInfo.CanRead)
                continue; // skip non-readable properties

            var value = propertyInfo.GetValue(data);

            if (value is null)
            {
                continue;
            }

            if (propertyInfo.PropertyType.IsArray)
            {
                switch (value)
                {
                    case bool[] v:
                        props.BooleanArrayProperties.Add(
                            new V1.BooleanArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { v },
                            }
                        );
                        break;
                    case int[] v:
                        props.IntArrayProperties.Add(
                            new V1.IntArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { v.Select(Convert.ToInt64) },
                            }
                        );
                        break;
                    case long[] v:
                        props.IntArrayProperties.Add(
                            new V1.IntArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { v },
                            }
                        );
                        break;
                    case double[] v:
                        props.NumberArrayProperties.Add(
                            new V1.NumberArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                ValuesBytes = v.ToByteString(),
                            }
                        );
                        break;
                    case float[] v:
                        props.NumberArrayProperties.Add(
                            new V1.NumberArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                ValuesBytes = v.Select(Convert.ToDouble).ToByteString(),
                            }
                        );
                        break;
                    case string[] v:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { v },
                            }
                        );
                        break;
                    case Guid[] v:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { v.Select(v => v.ToString()) },
                            }
                        );
                        break;
                    case DateTime[] v:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { v.Select(v => v.ToUniversalTime().ToString("o")) },
                            }
                        );
                        break;
                    case DateTimeOffset[] v:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { v.Select(dto => dto.ToUniversalTime().ToString("o")) },
                            }
                        );
                        break;

                    // Handle general IEnumerable<T> (e.g., List<T>, HashSet<T>)
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<bool> bools:
                        props.BooleanArrayProperties.Add(
                            new V1.BooleanArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { bools },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<int> ints:
                        props.IntArrayProperties.Add(
                            new V1.IntArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { ints.Select(Convert.ToInt64) },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<long> longs:
                        props.IntArrayProperties.Add(
                            new V1.IntArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { longs },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<double> doubles:
                        props.NumberArrayProperties.Add(
                            new V1.NumberArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                ValuesBytes = doubles.ToByteString(),
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<float> floats:
                        props.NumberArrayProperties.Add(
                            new V1.NumberArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                ValuesBytes = floats.Select(f => (double)f).ToByteString(),
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<string> strings:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { strings },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<Guid> guids:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values = { guids.Select(g => g.ToString()) },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<DateTime> dateTimes:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values =
                                {
                                    dateTimes.Select(dt => dt.ToUniversalTime().ToString("o")),
                                },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<DateTimeOffset> dateTimeOffsets:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propertyInfo.Name,
                                Values =
                                {
                                    dateTimeOffsets.Select(dto =>
                                        dto.ToUniversalTime().ToString("o")
                                    ),
                                },
                            }
                        );
                        continue;
                    default:
                        throw new WeaviateException(
                            $"Unsupported array type '{value.GetType().GetElementType()?.Name ?? value.GetType().Name}' for property '{propertyInfo.Name}'. Check the documentation for supported array value types."
                        );
                }
                continue; // Move to the next property after handling array
            }

            if (propertyInfo.PropertyType.IsNativeType())
            {
                nonRefProps ??= new();

                nonRefProps.Fields.Add(propertyInfo.Name, ConvertToProtoValue(value));
            }
        }

        props.NonRefProperties = nonRefProps;

        return props;
    }

    // Helper method to convert C# objects to protobuf Values
    internal static Value ConvertToProtoValue(object obj)
    {
        return obj switch
        {
            null => Value.ForNull(),
            bool b => Value.ForBool(b),
            int i => Value.ForNumber(i),
            long l => Value.ForNumber(l),
            float f => Value.ForNumber(f),
            double d => Value.ForNumber(d),
            decimal dec => Value.ForNumber((double)dec),
            string s => Value.ForString(s),
            DateTime dt => Value.ForString(dt.ToUniversalTime().ToString("o")),
            Guid uuid => Value.ForString(uuid.ToString()),
            GeoCoordinate v => Value.ForStruct(
                new Struct
                {
                    Fields =
                    {
                        ["latitude"] = Value.ForNumber(v.Latitude),
                        ["longitude"] = Value.ForNumber(v.Longitude),
                    },
                }
            ),
            // Dictionary<string, object> dict => Value.ForStruct(CreateStructFromDictionary(dict)),
            // IEnumerable<object> enumerable => CreateListValue(enumerable),
            _ => throw new ArgumentException($"Unsupported type: {obj.GetType()}"),
        };
    }
}

public class DataClient<TData>
{
    private readonly CollectionClient<TData> _collectionClient;
    private WeaviateClient _client => _collectionClient.Client;
    private string _collectionName => _collectionClient.Name;

    internal DataClient(CollectionClient<TData> collectionClient)
    {
        _collectionClient = collectionClient;
    }

    public static IDictionary<string, string>[] MakeBeacons(params Guid[] guids)
    {
        return
        [
            .. guids.Select(uuid => new Dictionary<string, string>
            {
                { "beacon", $"weaviate://localhost/{uuid}" },
            }),
        ];
    }

    public async Task<Guid> Insert(
        TData data,
        Guid? id = null,
        NamedVectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    )
    {
        var propDict = ObjectHelper.BuildDataTransferObject(data);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = MakeBeacons(kvp.TargetID);
        }

        var dtoVectors =
            vectors?.Count == 0 ? null : Vectors.FromJson(JsonSerializer.Serialize(vectors));

        var dto = new Rest.Dto.Object()
        {
            Id = id ?? Guid.NewGuid(),
            Class = _collectionName,
            Properties = propDict,
            Vectors = dtoVectors,
            Tenant = tenant,
        };

        var response = await _client.RestClient.ObjectInsert(_collectionName, dto);

        return response.Id!.Value;
    }

    public delegate void InsertDelegate(
        TData data,
        Guid? id = null,
        NamedVectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    );

    public async Task<IEnumerable<BatchInsertResponse>> InsertMany(
        params BatchInsertRequest<TData>[] requests
    )
    {
        var objects = requests
            .Select(
                (r, idx) =>
                {
                    var o = new V1.BatchObject
                    {
                        Collection = _collectionName,
                        Uuid = (r.ID ?? Guid.NewGuid()).ToString(),
                        Properties = ObjectHelper.BuildBatchProperties(r.Data),
                    };

                    if (r.References?.Any() ?? false)
                    {
                        foreach (var reference in r.References!)
                        {
                            var strp = new Weaviate.V1.BatchObject.Types.SingleTargetRefProps()
                            {
                                PropName = reference.Name,
                                Uuids = { reference.TargetID.Select(id => id.ToString()) },
                            };

                            o.Properties.SingleTargetRefProps.Add(strp);
                        }
                    }

                    if (r.Vectors != null)
                    {
                        o.Vectors.AddRange(
                            r.Vectors.Select(v => new V1.Vectors
                            {
                                Name = v.Key,
                                VectorBytes = v.Value.ToByteString(),
                            })
                        );
                    }

                    return new { Index = idx, BatchObject = o };
                }
            )
            .ToList();

        var inserts = await _client.GrpcClient.InsertMany(objects.Select(o => o.BatchObject));

        var dictErr = inserts.Errors.ToFrozenDictionary(kv => kv.Index, kv => kv.Error);
        var dictUuid = objects
            .Select(o => new { o.Index, o.BatchObject.Uuid })
            .Where(o => !dictErr.ContainsKey(o.Index))
            .ToDictionary(kv => kv.Index, kv => Guid.Parse(kv.Uuid));

        var results = new List<BatchInsertResponse>();

        foreach (int r in Enumerable.Range(0, objects.Count))
        {
            results.Add(
                new BatchInsertResponse(
                    Index: r,
                    dictUuid.TryGetValue(r, out Guid uuid) ? uuid : (Guid?)null,
                    dictErr.TryGetValue(r, out string? error) ? new WeaviateException(error) : null
                )
            );
        }

        return results;
    }

    public async Task<IEnumerable<BatchInsertResponse>> InsertMany(
        params Action<InsertDelegate>[] inserterList
    )
    {
        var responses = new List<BatchInsertResponse>();

        foreach (var inserter in inserterList)
        {
            IList<BatchInsertRequest<TData>> requests = [];

            InsertDelegate _inserter = (
                TData data,
                Guid? id = null,
                NamedVectors? vectors = null,
                IEnumerable<ObjectReference>? references = null,
                string? tenant = null
            ) =>
            {
                requests.Add(new BatchInsertRequest<TData>(data, id, vectors, references, tenant));
            };

            inserter(_inserter);

            var response = await InsertMany([.. requests]);

            responses.AddRange(
                [
                    .. response.Select(r => new BatchInsertResponse(
                        r.Index + responses.Count,
                        r.ID,
                        r.Error
                    )),
                ]
            );
        }

        return responses;
    }

    public async Task Delete(Guid id)
    {
        await _client.RestClient.DeleteObject(_collectionName, id);
    }

    public async Task ReferenceAdd(Guid from, string fromProperty, Guid to)
    {
        await _client.RestClient.ReferenceAdd(_collectionName, from, fromProperty, to);
    }

    public async Task ReferenceReplace(Guid from, string fromProperty, Guid[] to)
    {
        await _client.RestClient.ReferenceReplace(_collectionName, from, fromProperty, to);
    }

    public async Task ReferenceDelete(Guid from, string fromProperty, Guid to)
    {
        await _client.RestClient.ReferenceDelete(_collectionName, from, fromProperty, to);
    }
}

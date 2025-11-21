using System.Collections.Concurrent;
using System.Reflection;
using Weaviate.Client.Models;
using Weaviate.Client.Serialization.Converters;

namespace Weaviate.Client.Serialization;

/// <summary>
/// Registry for property converters. Maps C# types to Weaviate data types and provides
/// serialization/deserialization through the appropriate converter.
/// </summary>
public class PropertyConverterRegistry
{
    private readonly Dictionary<string, IPropertyConverter> _byDataType = new();
    private readonly Dictionary<Type, IPropertyConverter> _byType = new();
    private readonly ConcurrentDictionary<Type, PropertyInfo[]> _readablePropertyCache = new();
    private readonly ConcurrentDictionary<Type, PropertyInfo[]> _writablePropertyCache = new();

    private static readonly Lazy<PropertyConverterRegistry> _default = new(() => CreateDefault());

    public static PropertyConverterRegistry Default => _default.Value;

    public PropertyConverterRegistry() { }

    public static PropertyConverterRegistry CreateDefault()
    {
        var registry = new PropertyConverterRegistry();

        // Basic converters
        registry.Register(new TextPropertyConverter());
        registry.Register(new IntPropertyConverter());
        registry.Register(new NumberPropertyConverter());
        registry.Register(new BoolPropertyConverter());
        registry.Register(new DatePropertyConverter());
        registry.Register(new UuidPropertyConverter());

        // Special converters (no array support)
        registry.Register(new BlobPropertyConverter());
        registry.Register(new GeoPropertyConverter());
        registry.Register(new PhonePropertyConverter());

        // Object converter (recursive - needs registry reference)
        registry.Register(new ObjectPropertyConverter(() => registry));

        return registry;
    }

    public void Register(IPropertyConverter converter)
    {
        _byDataType[converter.DataType] = converter;

        if (converter.SupportsArray)
        {
            _byDataType[converter.DataType + "[]"] = converter;
        }

        foreach (var type in converter.SupportedTypes)
        {
            _byType[type] = converter;

            // Also register array types
            if (converter.SupportsArray)
            {
                _byType[type.MakeArrayType()] = converter;

                // Register List<T> as well
                var listType = typeof(List<>).MakeGenericType(type);
                _byType[listType] = converter;

                var iListType = typeof(IList<>).MakeGenericType(type);
                _byType[iListType] = converter;

                var iEnumerableType = typeof(IEnumerable<>).MakeGenericType(type);
                _byType[iEnumerableType] = converter;
            }
        }
    }

    public IPropertyConverter? GetConverterByDataType(string dataType)
    {
        return _byDataType.TryGetValue(dataType, out var converter) ? converter : null;
    }

    public IPropertyConverter? GetConverterForType(Type type)
    {
        // Direct lookup
        if (_byType.TryGetValue(type, out var converter))
            return converter;

        // Handle nullable types
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null && _byType.TryGetValue(underlying, out converter))
            return converter;

        // Handle generic collections
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            var elementType = type.GetGenericArguments().FirstOrDefault();

            if (elementType != null && IsCollectionType(genericDef))
            {
                if (_byType.TryGetValue(elementType, out converter))
                    return converter;
            }
        }

        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType != null && _byType.TryGetValue(elementType, out converter))
                return converter;
        }

        // Fall back to object converter for complex types
        return _byDataType.TryGetValue("object", out converter) ? converter : null;
    }

    /// <summary>
    /// Converts a value to a gRPC Value using the appropriate converter.
    /// </summary>
    public Google.Protobuf.WellKnownTypes.Value ToGrpcValue(object? value)
    {
        if (value is null)
            return Google.Protobuf.WellKnownTypes.Value.ForNull();

        var converter = GetConverterForType(value.GetType());
        return converter?.ToGrpc(value) ?? Google.Protobuf.WellKnownTypes.Value.ForNull();
    }

    /// <summary>
    /// Serializes an object's properties to a dictionary suitable for REST API.
    /// </summary>
    public IDictionary<string, object?> SerializeToRest(object obj)
    {
        var result = new Dictionary<string, object?>();
        var properties = GetReadableProperties(obj.GetType());

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            var propName = DecapitalizeName(prop.Name);

            if (value is null)
            {
                continue;
            }

            var converter = GetConverterForType(prop.PropertyType);
            if (converter is null)
            {
                result[propName] = value;
                continue;
            }

            var isArray = IsArrayOrCollection(prop.PropertyType);
            if (
                isArray
                && value is System.Collections.IEnumerable enumerable
                && value is not string
            )
            {
                // Preserve typed arrays for primitive types
                result[propName] = ConvertArrayToRest(enumerable, converter);
            }
            else
            {
                result[propName] = converter.ToRest(value);
            }
        }

        return result;
    }

    /// <summary>
    /// Deserializes a dictionary from REST API to a typed object.
    /// </summary>
    public T? DeserializeFromRest<T>(IDictionary<string, object?> dict)
        where T : class, new()
    {
        return (T?)DeserializeFromRest(dict, typeof(T));
    }

    /// <summary>
    /// Deserializes a dictionary from REST API to a typed object.
    /// </summary>
    public object? DeserializeFromRest(IDictionary<string, object?> dict, System.Type targetType)
    {
        if (dict is null)
            return null;

        // If target is dictionary, return as-is with capitalized keys
        if (typeof(IDictionary<string, object?>).IsAssignableFrom(targetType))
        {
            return CapitalizeKeys(dict);
        }

        var instance = Activator.CreateInstance(targetType);
        if (instance is null)
            return null;

        var properties = GetWritableProperties(targetType);
        var dictLower = dict.ToDictionary(kvp => kvp.Key.ToLowerInvariant(), kvp => kvp.Value);

        foreach (var prop in properties)
        {
            var propNameLower = prop.Name.ToLowerInvariant();
            if (!dictLower.TryGetValue(propNameLower, out var value) || value is null)
                continue;

            var converter = GetConverterForType(prop.PropertyType);
            if (converter is null)
            {
                TrySetProperty(prop, instance, value);
                continue;
            }

            var isArray = IsArrayOrCollection(prop.PropertyType);
            object? converted;

            if (
                isArray
                && value is System.Collections.IEnumerable enumerable
                && value is not string
            )
            {
                var elementType = GetElementType(prop.PropertyType);

                // For object/class arrays, convert each dictionary item directly
                // Check if element type is a class (excluding string) that needs object deserialization
                var isObjectArray =
                    elementType.IsClass
                    && elementType != typeof(string)
                    && elementType != typeof(object)
                    && !typeof(System.Collections.IEnumerable).IsAssignableFrom(elementType);
                if (isObjectArray)
                {
                    var items = new List<object?>();
                    foreach (var item in enumerable)
                    {
                        if (item is IDictionary<string, object?> itemDict)
                        {
                            items.Add(DeserializeFromRest(itemDict, elementType));
                        }
                        else if (item is IDictionary<string, object> dictNonNullable)
                        {
                            var dictNullable = dictNonNullable.ToDictionary(
                                kvp => kvp.Key,
                                kvp => (object?)kvp.Value
                            );
                            items.Add(DeserializeFromRest(dictNullable, elementType));
                        }
                        else
                        {
                            items.Add(item);
                        }
                    }
                    converted = CreateTypedArray(items, elementType);
                }
                else
                {
                    var items = enumerable.Cast<object?>().ToList();
                    converted = converter.FromRestArray(items!, elementType);
                }
            }
            else
            {
                converted = converter.FromRest(value, prop.PropertyType);
            }

            TrySetProperty(prop, instance, converted);
        }

        return instance;
    }

    private PropertyInfo[] GetReadableProperties(Type type)
    {
        return _readablePropertyCache.GetOrAdd(
            type,
            t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToArray()
        );
    }

    private PropertyInfo[] GetWritableProperties(Type type)
    {
        return _writablePropertyCache.GetOrAdd(
            type,
            t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite)
                    .ToArray()
        );
    }

    private static void TrySetProperty(PropertyInfo prop, object instance, object? value)
    {
        try
        {
            if (value is null)
            {
                if (
                    !prop.PropertyType.IsValueType
                    || Nullable.GetUnderlyingType(prop.PropertyType) != null
                )
                {
                    prop.SetValue(instance, null);
                }
                return;
            }

            // Try direct assignment
            if (prop.PropertyType.IsAssignableFrom(value.GetType()))
            {
                prop.SetValue(instance, value);
                return;
            }

            // Try conversion
            var converted = Convert.ChangeType(value, prop.PropertyType);
            prop.SetValue(instance, converted);
        }
        catch
        {
            // Silently skip properties that can't be set
        }
    }

    private static bool IsCollectionType(Type genericDef)
    {
        return genericDef == typeof(List<>)
            || genericDef == typeof(IList<>)
            || genericDef == typeof(ICollection<>)
            || genericDef == typeof(IEnumerable<>);
    }

    private static bool IsArrayOrCollection(Type type)
    {
        if (type == typeof(string) || type == typeof(byte[]))
            return false;

        if (type.IsArray)
            return true;

        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            return IsCollectionType(genericDef);
        }

        return false;
    }

    private static Type GetElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType()!;

        if (collectionType.IsGenericType)
            return collectionType.GetGenericArguments()[0];

        return typeof(object);
    }

    private static object ConvertArrayToRest(
        System.Collections.IEnumerable enumerable,
        IPropertyConverter converter
    )
    {
        // Preserve typed arrays for common primitive types
        return enumerable switch
        {
            string[] arr => arr.Select(s => converter.ToRest(s)?.ToString()).ToArray(),
            int[] arr => arr.Select(i => Convert.ToInt64(i)).ToArray(),
            long[] arr => arr,
            double[] arr => arr,
            float[] arr => arr.Select(f => (double)f).ToArray(),
            bool[] arr => arr,
            DateTime[] arr => arr.Select(d => converter.ToRest(d)).ToArray(),
            Guid[] arr => arr.Select(g => g.ToString()).ToArray(),
            IEnumerable<string> strings => strings
                .Select(s => converter.ToRest(s)?.ToString())
                .ToArray(),
            IEnumerable<int> ints => ints.Select(i => Convert.ToInt64(i)).ToArray(),
            IEnumerable<long> longs => longs.ToArray(),
            IEnumerable<double> doubles => doubles.ToArray(),
            IEnumerable<bool> bools => bools.ToArray(),
            IEnumerable<DateTime> dates => dates.Select(d => converter.ToRest(d)).ToArray(),
            IEnumerable<Guid> guids => guids.Select(g => g.ToString()).ToArray(),
            _ => enumerable.Cast<object?>().Select(v => converter.ToRest(v)).ToArray(),
        };
    }

    private static Array CreateTypedArray(IList<object?> items, Type elementType)
    {
        var array = Array.CreateInstance(elementType, items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            array.SetValue(items[i], i);
        }
        return array;
    }

    private static string DecapitalizeName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static IDictionary<string, object?> CapitalizeKeys(IDictionary<string, object?> dict)
    {
        return dict.ToDictionary(
            kvp => char.ToUpperInvariant(kvp.Key[0]) + kvp.Key[1..],
            kvp => kvp.Value
        );
    }
}

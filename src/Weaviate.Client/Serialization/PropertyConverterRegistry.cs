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
    private readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

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
    /// Serializes an object's properties to a dictionary suitable for REST API.
    /// </summary>
    public IDictionary<string, object?> SerializeToRest(object obj)
    {
        var result = new Dictionary<string, object?>();
        var properties = GetCachedProperties(obj.GetType());

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            var propName = DecapitalizeName(prop.Name);

            if (value is null)
            {
                result[propName] = null;
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
                var items = enumerable.Cast<object?>().ToList();
                result[propName] = converter.ToRestArray(items!);
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

        var properties = GetCachedProperties(targetType);
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
                var items = enumerable.Cast<object?>().ToList();
                converted = converter.FromRestArray(items!, elementType);
            }
            else
            {
                converted = converter.FromRest(value, prop.PropertyType);
            }

            TrySetProperty(prop, instance, converted);
        }

        return instance;
    }

    private PropertyInfo[] GetCachedProperties(Type type)
    {
        return _propertyCache.GetOrAdd(
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

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Weaviate.Client.Models;

namespace Weaviate.Client.Serialization;

/// <summary>
/// A strongly-typed property bag that replaces ExpandoObject for storing Weaviate object properties.
/// Implements IDictionary for backward compatibility while providing typed accessors.
/// </summary>
public class PropertyBag : IDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _properties = new(
        StringComparer.OrdinalIgnoreCase
    );

    public PropertyBag() { }

    public PropertyBag(IDictionary<string, object?> properties)
    {
        foreach (var kvp in properties)
        {
            _properties[kvp.Key] = kvp.Value;
        }
    }

    #region Typed Accessors

    /// <summary>
    /// Gets a string value by property name.
    /// </summary>
    public string? GetString(string name)
    {
        return TryGetValue(name, out var value) ? value?.ToString() : null;
    }

    /// <summary>
    /// Gets an integer value by property name.
    /// </summary>
    public int? GetInt(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        return value switch
        {
            int i => i,
            long l => (int)l,
            double d => (int)d,
            _ => Convert.ToInt32(value),
        };
    }

    /// <summary>
    /// Gets a long value by property name.
    /// </summary>
    public long? GetLong(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        return value switch
        {
            long l => l,
            int i => i,
            double d => (long)d,
            _ => Convert.ToInt64(value),
        };
    }

    /// <summary>
    /// Gets a double value by property name.
    /// </summary>
    public double? GetDouble(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        return Convert.ToDouble(value);
    }

    /// <summary>
    /// Gets a boolean value by property name.
    /// </summary>
    public bool? GetBool(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        return Convert.ToBoolean(value);
    }

    /// <summary>
    /// Gets a DateTime value by property name.
    /// </summary>
    public DateTime? GetDateTime(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        var converter = PropertyConverterRegistry.Default.GetConverterForType(typeof(DateTime));
        return (DateTime?)converter?.FromRest(value, typeof(DateTime));
    }

    /// <summary>
    /// Gets a Guid value by property name.
    /// </summary>
    public Guid? GetGuid(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        return value switch
        {
            Guid g => g,
            string s => Guid.Parse(s),
            _ => null,
        };
    }

    /// <summary>
    /// Gets a GeoCoordinate value by property name.
    /// </summary>
    public GeoCoordinate? GetGeo(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        var converter = PropertyConverterRegistry.Default.GetConverterForType(
            typeof(GeoCoordinate)
        );
        return (GeoCoordinate?)converter?.FromRest(value, typeof(GeoCoordinate));
    }

    /// <summary>
    /// Gets a PhoneNumber value by property name.
    /// </summary>
    public PhoneNumber? GetPhone(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        var converter = PropertyConverterRegistry.Default.GetConverterForType(typeof(PhoneNumber));
        return (PhoneNumber?)converter?.FromRest(value, typeof(PhoneNumber));
    }

    /// <summary>
    /// Gets a blob (byte array) value by property name.
    /// </summary>
    public byte[]? GetBlob(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        var converter = PropertyConverterRegistry.Default.GetConverterForType(typeof(byte[]));
        return (byte[]?)converter?.FromRest(value, typeof(byte[]));
    }

    /// <summary>
    /// Gets an array of strings by property name.
    /// </summary>
    public string[]? GetStringArray(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        if (value is string[] arr)
            return arr;

        if (value is IEnumerable<object> enumerable)
            return enumerable.Select(v => v?.ToString() ?? "").ToArray();

        return null;
    }

    /// <summary>
    /// Gets an array of integers by property name.
    /// </summary>
    public int[]? GetIntArray(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        if (value is int[] arr)
            return arr;

        if (value is IEnumerable<object> enumerable)
            return enumerable.Select(v => Convert.ToInt32(v)).ToArray();

        return null;
    }

    /// <summary>
    /// Gets a nested PropertyBag by property name (for object properties).
    /// </summary>
    public PropertyBag? GetNested(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        if (value is PropertyBag bag)
            return bag;

        if (value is IDictionary<string, object?> dict)
            return new PropertyBag(dict);

        return null;
    }

    /// <summary>
    /// Gets a typed value using the PropertyConverterRegistry.
    /// </summary>
    public T? GetAs<T>(string name)
        where T : class, new()
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        if (value is T typed)
            return typed;

        if (value is IDictionary<string, object?> dict)
        {
            return PropertyConverterRegistry.Default.BuildConcreteTypeFromProperties<T>(dict);
        }

        return null;
    }

    #endregion

    #region IDictionary Implementation

    public object? this[string key]
    {
        get => _properties.TryGetValue(key, out var value) ? value : null;
        set => _properties[key] = value;
    }

    public ICollection<string> Keys => _properties.Keys;
    public ICollection<object?> Values => _properties.Values;
    public int Count => _properties.Count;
    public bool IsReadOnly => false;

    public void Add(string key, object? value) => _properties.Add(key, value);

    public void Add(KeyValuePair<string, object?> item) => _properties.Add(item.Key, item.Value);

    public void Clear() => _properties.Clear();

    public bool Contains(KeyValuePair<string, object?> item) =>
        _properties.TryGetValue(item.Key, out var value) && Equals(value, item.Value);

    public bool ContainsKey(string key) => _properties.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, object?>>)_properties).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() =>
        _properties.GetEnumerator();

    public bool Remove(string key) => _properties.Remove(key);

    public bool Remove(KeyValuePair<string, object?> item) => _properties.Remove(item.Key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value) =>
        _properties.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

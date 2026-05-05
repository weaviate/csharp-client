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
    /// <summary>
    /// The ordinal ignore case
    /// </summary>
    private readonly Dictionary<string, object?> _properties = new(
        StringComparer.OrdinalIgnoreCase
    );

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBag"/> class.
    /// </summary>
    public PropertyBag() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBag"/> class with the specified properties.
    /// </summary>
    /// <param name="properties">The initial properties to add to the bag.</param>
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
            long l => Convert.ToInt32(l),
            double d => Convert.ToInt32(d),
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
            double d => Convert.ToInt64(d),
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
    /// Gets a blob hash (string) value by property name.
    /// </summary>
    public string? GetBlobHash(string name)
    {
        if (!TryGetValue(name, out var value) || value is null)
            return null;

        var converter = PropertyConverterRegistry.Default.GetConverterByDataType("blobHash");
        return (string?)converter?.FromRest(value, typeof(string));
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


    /// <summary>
    /// Gets or sets the property value with the specified key.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The property value, or null if not found.</returns>
    public object? this[string key]
    {
        get => _properties.TryGetValue(key, out var value) ? value : null;
        set => _properties[key] = value;
    }

    /// <summary>
    /// Gets the collection of property keys.
    /// </summary>
    public ICollection<string> Keys => _properties.Keys;

    /// <summary>
    /// Gets the collection of property values.
    /// </summary>
    public ICollection<object?> Values => _properties.Values;

    /// <summary>
    /// Gets the number of properties in the bag.
    /// </summary>
    public int Count => _properties.Count;

    /// <summary>
    /// Gets a value indicating whether the property bag is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds the key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    public void Add(string key, object? value) => _properties.Add(key, value);

    /// <summary>
    /// Adds the item
    /// </summary>
    /// <param name="item">The item</param>
    public void Add(KeyValuePair<string, object?> item) => _properties.Add(item.Key, item.Value);

    /// <summary>
    /// Clears this instance
    /// </summary>
    public void Clear() => _properties.Clear();

    /// <summary>
    /// Containses the item
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The bool</returns>
    public bool Contains(KeyValuePair<string, object?> item) =>
        _properties.TryGetValue(item.Key, out var value) && Equals(value, item.Value);

    /// <summary>
    /// Containses the key using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The bool</returns>
    public bool ContainsKey(string key) => _properties.ContainsKey(key);

    /// <summary>
    /// Copies the to using the specified array
    /// </summary>
    /// <param name="array">The array</param>
    /// <param name="arrayIndex">The array index</param>
    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, object?>>)_properties).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of key value pair string and object</returns>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() =>
        _properties.GetEnumerator();

    /// <summary>
    /// Removes the key
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The bool</returns>
    public bool Remove(string key) => _properties.Remove(key);

    /// <summary>
    /// Removes the item
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The bool</returns>
    public bool Remove(KeyValuePair<string, object?> item) => _properties.Remove(item.Key);

    /// <summary>
    /// Tries the get value using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>The bool</returns>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value) =>
        _properties.TryGetValue(key, out value);

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

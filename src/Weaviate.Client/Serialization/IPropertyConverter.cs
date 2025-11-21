using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization;

/// <summary>
/// Converts property values between C# types and Weaviate REST/gRPC representations.
/// </summary>
public interface IPropertyConverter
{
    /// <summary>
    /// The Weaviate data type string (e.g., "text", "int", "boolean").
    /// </summary>
    string DataType { get; }

    /// <summary>
    /// Whether this converter supports array variants (e.g., "text[]").
    /// </summary>
    bool SupportsArray { get; }

    /// <summary>
    /// C# types this converter can handle.
    /// </summary>
    IReadOnlyList<System.Type> SupportedTypes { get; }

    /// <summary>
    /// Converts a C# value to REST DTO representation.
    /// </summary>
    object? ToRest(object? value);

    /// <summary>
    /// Converts a C# array to REST DTO representation.
    /// </summary>
    object? ToRestArray(IEnumerable<object?> values);

    /// <summary>
    /// Converts a C# value to gRPC Protobuf Value.
    /// </summary>
    Value ToGrpc(object? value);

    /// <summary>
    /// Converts a C# array to gRPC Protobuf ListValue.
    /// </summary>
    ListValue ToGrpcArray(IEnumerable<object?> values);

    /// <summary>
    /// Converts a REST DTO value to C# type.
    /// </summary>
    object? FromRest(object? value, System.Type targetType);

    /// <summary>
    /// Converts a REST DTO array to C# array.
    /// </summary>
    object? FromRestArray(IEnumerable<object?> values, System.Type elementType);

    /// <summary>
    /// Converts a gRPC Value to C# type.
    /// </summary>
    object? FromGrpc(Value value, System.Type targetType);

    /// <summary>
    /// Converts a gRPC ListValue to C# array.
    /// </summary>
    object? FromGrpcArray(ListValue values, System.Type elementType);
}

/// <summary>
/// Base class for property converters with common functionality.
/// </summary>
public abstract class PropertyConverterBase : IPropertyConverter
{
    public abstract string DataType { get; }
    public virtual bool SupportsArray => true;
    public abstract IReadOnlyList<System.Type> SupportedTypes { get; }

    public abstract object? ToRest(object? value);
    public abstract Value ToGrpc(object? value);
    public abstract object? FromRest(object? value, System.Type targetType);
    public abstract object? FromGrpc(Value value, System.Type targetType);

    public virtual object? ToRestArray(IEnumerable<object?> values)
    {
        if (!SupportsArray)
            throw new NotSupportedException($"Array not supported for {DataType}");

        return values.Select(ToRest).ToArray();
    }

    public virtual ListValue ToGrpcArray(IEnumerable<object?> values)
    {
        if (!SupportsArray)
            throw new NotSupportedException($"Array not supported for {DataType}");

        var list = new ListValue();
        foreach (var v in values)
        {
            list.Values.Add(ToGrpc(v));
        }
        return list;
    }

    public virtual object? FromRestArray(IEnumerable<object?> values, System.Type elementType)
    {
        if (!SupportsArray)
            throw new NotSupportedException($"Array not supported for {DataType}");

        var converted = values.Select(v => FromRest(v, elementType)).ToList();
        return CreateTypedArray(converted, elementType);
    }

    public virtual object? FromGrpcArray(ListValue values, System.Type elementType)
    {
        if (!SupportsArray)
            throw new NotSupportedException($"Array not supported for {DataType}");

        var converted = values.Values.Select(v => FromGrpc(v, elementType)).ToList();
        return CreateTypedArray(converted, elementType);
    }

    protected static Array CreateTypedArray(IList<object?> items, System.Type elementType)
    {
        var array = Array.CreateInstance(elementType, items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            array.SetValue(items[i], i);
        }
        return array;
    }
}

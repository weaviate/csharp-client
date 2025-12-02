using System.Reflection;
using System.Runtime.Serialization;
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

    /// <summary>
    /// Checks if the target type is an enum type (including nullable enums).
    /// </summary>
    protected static bool IsEnumType(System.Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsEnum;
    }

    /// <summary>
    /// Converts a string value to an enum, matching by name or EnumMember attribute.
    /// </summary>
    protected static object? ConvertStringToEnum(string value, System.Type enumType)
    {
        var actualEnumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
        if (!actualEnumType.IsEnum)
            throw new ArgumentException($"Type {enumType.Name} is not an enum type");

        // Try to find a matching enum value by checking:
        // 1. Enum name (case-insensitive)
        // 2. EnumMember attribute value (case-insensitive)
        foreach (var field in actualEnumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            // Check if the field name matches
            if (field.Name.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return field.GetValue(null);
            }

            // Check if the EnumMember attribute value matches
            var enumMemberAttr = field.GetCustomAttribute<EnumMemberAttribute>();
            if (
                enumMemberAttr?.Value != null
                && enumMemberAttr.Value.Equals(value, StringComparison.OrdinalIgnoreCase)
            )
            {
                return field.GetValue(null);
            }
        }

        // If no match found, throw an exception
        throw new ArgumentException(
            $"Value '{value}' cannot be converted to enum type '{actualEnumType.Name}'. "
                + $"Valid values are: {string.Join(", ", System.Enum.GetNames(actualEnumType))}"
        );
    }

    /// <summary>
    /// Converts a numeric value to an enum.
    /// </summary>
    protected static object? ConvertNumberToEnum(object value, System.Type enumType)
    {
        var actualEnumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
        if (!actualEnumType.IsEnum)
            throw new ArgumentException($"Type {enumType.Name} is not an enum type");

        return System.Enum.ToObject(actualEnumType, value);
    }

    /// <summary>
    /// Converts an enum to its string representation (EnumMember value if present, otherwise name).
    /// </summary>
    protected static string? EnumToString(object? value)
    {
        if (value is null)
            return null;

        if (value is not System.Enum enumValue)
            throw new ArgumentException($"Value is not an enum: {value.GetType().Name}");

        // Return the EnumMember value if present, otherwise the enum name
        var type = value.GetType();
        var member = type.GetMember(enumValue.ToString()).FirstOrDefault();
        var attr = member?.GetCustomAttribute<EnumMemberAttribute>();
        return attr?.Value ?? enumValue.ToString();
    }
}

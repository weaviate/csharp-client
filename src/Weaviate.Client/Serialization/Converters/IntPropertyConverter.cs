using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "int" and "int[]" data types.
/// Maps to C# int, long, short, byte (and nullable variants).
/// </summary>
internal class IntPropertyConverter : PropertyConverterBase
{
    /// <summary>
    /// Gets the value of the data type
    /// </summary>
    public override string DataType => "int";

    /// <summary>
    /// Gets the value of the supported types
    /// </summary>
    public override IReadOnlyList<System.Type> SupportedTypes =>
        [
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(byte),
            typeof(int?),
            typeof(long?),
            typeof(short?),
            typeof(byte?),
        ];

    /// <summary>
    /// Returns the rest using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The object</returns>
    public override object? ToRest(object? value)
    {
        if (value is null)
            return null;

        // Handle enum types - convert to their underlying numeric value
        if (value is System.Enum enumValue)
        {
            return Convert.ToInt64(enumValue);
        }

        // Always convert to long for consistency with Weaviate int type
        return Convert.ToInt64(value);
    }

    /// <summary>
    /// Returns the rest array using the specified values
    /// </summary>
    /// <param name="values">The values</param>
    /// <returns>An enumerable of object</returns>
    public override IEnumerable<object?> ToRestArray(IEnumerable<object?> values)
    {
        return values.Select(v => (object?)(v is null ? 0L : Convert.ToInt64(v))).ToArray();
    }

    /// <summary>
    /// Returns the grpc using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The value</returns>
    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        var longValue = Convert.ToInt64(value);
        return new Value { NumberValue = longValue };
    }

    /// <summary>
    /// Creates the rest using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="targetType">The target type</param>
    /// <returns>The object</returns>
    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Handle enum types
        if (IsEnumType(targetType))
        {
            return ConvertNumberToEnum(value, targetType);
        }

        var longValue = Convert.ToInt64(value);

        if (underlying == typeof(int))
            return (int)longValue;
        if (underlying == typeof(long))
            return longValue;
        if (underlying == typeof(short))
            return (short)longValue;
        if (underlying == typeof(byte))
            return (byte)longValue;

        return (int)longValue;
    }

    /// <summary>
    /// Creates the rest array using the specified values
    /// </summary>
    /// <param name="values">The values</param>
    /// <param name="elementType">The element type</param>
    /// <returns>The object</returns>
    public override object? FromRestArray(IEnumerable<object?> values, System.Type elementType)
    {
        var underlying = Nullable.GetUnderlyingType(elementType) ?? elementType;
        var items = values.Select(v => FromRest(v, underlying)).ToList();
        return CreateTypedArray(items, underlying);
    }

    /// <summary>
    /// Creates the grpc using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="targetType">The target type</param>
    /// <returns>The object</returns>
    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        var longValue = (long)value.NumberValue;
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Handle enum types
        if (IsEnumType(targetType))
        {
            return ConvertNumberToEnum((int)longValue, targetType);
        }

        if (underlying == typeof(int))
            return (int)longValue;
        if (underlying == typeof(long))
            return longValue;
        if (underlying == typeof(short))
            return (short)longValue;
        if (underlying == typeof(byte))
            return (byte)longValue;

        return (int)longValue;
    }
}

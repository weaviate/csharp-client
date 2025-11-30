using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "int" and "int[]" data types.
/// Maps to C# int, long, short, byte (and nullable variants).
/// </summary>
public class IntPropertyConverter : PropertyConverterBase
{
    public override string DataType => "int";

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

    public override object? ToRest(object? value)
    {
        if (value is null)
            return null;

        // Always convert to long for consistency with Weaviate int type
        return Convert.ToInt64(value);
    }

    public override IEnumerable<object?> ToRestArray(IEnumerable<object?> values)
    {
        return values.Select(v => (object?)(v is null ? 0L : Convert.ToInt64(v))).ToArray();
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        var longValue = Convert.ToInt64(value);
        return new Value { NumberValue = longValue };
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
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

    public override object? FromRestArray(IEnumerable<object?> values, System.Type elementType)
    {
        var underlying = Nullable.GetUnderlyingType(elementType) ?? elementType;
        var items = values.Select(v => FromRest(v, underlying)).ToList();
        return CreateTypedArray(items, underlying);
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        var longValue = (long)value.NumberValue;
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

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

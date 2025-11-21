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
        return value switch
        {
            null => null,
            int i => i,
            long l => l,
            short s => (int)s,
            byte b => (int)b,
            _ => Convert.ToInt64(value),
        };
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

        return underlying switch
        {
            System.Type t when t == typeof(int) => (int)longValue,
            System.Type t when t == typeof(long) => longValue,
            System.Type t when t == typeof(short) => (short)longValue,
            System.Type t when t == typeof(byte) => (byte)longValue,
            _ => (int)longValue,
        };
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        var longValue = (long)value.NumberValue;
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return underlying switch
        {
            System.Type t when t == typeof(int) => (int)longValue,
            System.Type t when t == typeof(long) => longValue,
            System.Type t when t == typeof(short) => (short)longValue,
            System.Type t when t == typeof(byte) => (byte)longValue,
            _ => (int)longValue,
        };
    }
}

using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "number" and "number[]" data types.
/// Maps to C# double, float, decimal (and nullable variants).
/// </summary>
public class NumberPropertyConverter : PropertyConverterBase
{
    public override string DataType => "number";

    public override IReadOnlyList<System.Type> SupportedTypes =>
        [
            typeof(double),
            typeof(float),
            typeof(decimal),
            typeof(double?),
            typeof(float?),
            typeof(decimal?),
        ];

    public override object? ToRest(object? value)
    {
        return value switch
        {
            null => null,
            double d => d,
            float f => (double)f,
            decimal m => (double)m,
            _ => Convert.ToDouble(value),
        };
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        var doubleValue = Convert.ToDouble(value);
        return Value.ForNumber(doubleValue);
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var doubleValue = Convert.ToDouble(value);

        return underlying switch
        {
            System.Type t when t == typeof(double) => doubleValue,
            System.Type t when t == typeof(float) => (float)doubleValue,
            System.Type t when t == typeof(decimal) => (decimal)doubleValue,
            _ => doubleValue,
        };
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        var doubleValue = value.NumberValue;
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return underlying switch
        {
            System.Type t when t == typeof(double) => doubleValue,
            System.Type t when t == typeof(float) => (float)doubleValue,
            System.Type t when t == typeof(decimal) => (decimal)doubleValue,
            _ => doubleValue,
        };
    }
}

using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "boolean" and "boolean[]" data types.
/// </summary>
public class BoolPropertyConverter : PropertyConverterBase
{
    public override string DataType => "boolean";

    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(bool), typeof(bool?)];

    public override object? ToRest(object? value)
    {
        return value switch
        {
            null => null,
            bool b => b,
            _ => Convert.ToBoolean(value),
        };
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        return Value.ForBool(Convert.ToBoolean(value));
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        return Convert.ToBoolean(value);
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        return value.BoolValue;
    }
}

using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "text" and "text[]" data types.
/// </summary>
public class TextPropertyConverter : PropertyConverterBase
{
    public override string DataType => "text";
    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(string)];

    public override object? ToRest(object? value)
    {
        return value?.ToString();
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        return Value.ForString(value.ToString()!);
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        return value?.ToString();
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        return value.StringValue;
    }
}

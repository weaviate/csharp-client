using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "uuid" and "uuid[]" data types.
/// </summary>
public class UuidPropertyConverter : PropertyConverterBase
{
    public override string DataType => "uuid";

    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(Guid), typeof(Guid?)];

    public override object? ToRest(object? value)
    {
        return value switch
        {
            null => null,
            Guid g => g.ToString(),
            _ => value.ToString(),
        };
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        var guidString = value switch
        {
            Guid g => g.ToString(),
            _ => value.ToString()!,
        };

        return Value.ForString(guidString);
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        var guidString = value.ToString();
        if (string.IsNullOrEmpty(guidString))
            return null;

        return Guid.Parse(guidString);
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        var guidString = value.StringValue;
        if (string.IsNullOrEmpty(guidString))
            return null;

        return Guid.Parse(guidString);
    }
}

using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "blob" data type (no array variant).
/// Stores binary data as Base64 strings.
/// </summary>
public class BlobPropertyConverter : PropertyConverterBase
{
    public override string DataType => "blob";
    public override bool SupportsArray => false;

    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(byte[])];

    public override object? ToRest(object? value)
    {
        return value switch
        {
            null => null,
            byte[] bytes => Convert.ToBase64String(bytes),
            _ => null,
        };
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        if (value is byte[] bytes)
            return Value.ForString(Convert.ToBase64String(bytes));

        return Value.ForNull();
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        var base64 = value.ToString();
        if (string.IsNullOrEmpty(base64))
            return null;

        return Convert.FromBase64String(base64);
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        var base64 = value.StringValue;
        if (string.IsNullOrEmpty(base64))
            return null;

        return Convert.FromBase64String(base64);
    }
}

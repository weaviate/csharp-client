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
        if (value is null)
            return null;

        // Handle enum types
        if (value is System.Enum)
        {
            return EnumToString(value);
        }

        return value.ToString();
    }

    public override IEnumerable<object?> ToRestArray(IEnumerable<object?> values)
    {
        return values.Select(v => v?.ToString()).ToArray();
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        return Value.ForString(value.ToString()!);
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        // Handle enum types
        if (IsEnumType(targetType))
        {
            // Check if the value is numeric (enum can be stored as number or string)
            if (value is int || value is long || value is short || value is byte)
            {
                try
                {
                    return ConvertNumberToEnum(value, targetType);
                }
                catch
                {
                    // Fallback to string conversion if numeric conversion fails
                }
            }

            // Treat as string (either non-numeric value or numeric conversion failed)
            var stringValue = value.ToString()!;
            return ConvertStringToEnum(stringValue, targetType);
        }

        return value.ToString()!;
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        var stringValue = value.StringValue;

        // Handle enum types
        if (IsEnumType(targetType))
        {
            return ConvertStringToEnum(stringValue, targetType);
        }

        return stringValue;
    }
}

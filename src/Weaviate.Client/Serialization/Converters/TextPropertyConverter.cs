using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "text" and "text[]" data types.
/// </summary>
internal class TextPropertyConverter : PropertyConverterBase
{
    /// <summary>
    /// Gets the value of the data type
    /// </summary>
    public override string DataType => "text";

    /// <summary>
    /// Gets the value of the supported types
    /// </summary>
    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(string)];

    /// <summary>
    /// Returns the rest using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The object</returns>
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

    /// <summary>
    /// Returns the rest array using the specified values
    /// </summary>
    /// <param name="values">The values</param>
    /// <returns>An enumerable of object</returns>
    public override IEnumerable<object?> ToRestArray(IEnumerable<object?> values)
    {
        return values.Select(v => v?.ToString()).ToArray();
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

        return Value.ForString(value.ToString()!);
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

    /// <summary>
    /// Creates the grpc using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="targetType">The target type</param>
    /// <returns>The string value</returns>
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

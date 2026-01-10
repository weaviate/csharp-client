using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "uuid" and "uuid[]" data types.
/// </summary>
internal class UuidPropertyConverter : PropertyConverterBase
{
    /// <summary>
    /// Gets the value of the data type
    /// </summary>
    public override string DataType => "uuid";

    /// <summary>
    /// Gets the value of the supported types
    /// </summary>
    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(Guid), typeof(Guid?)];

    /// <summary>
    /// Returns the rest using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The object</returns>
    public override object? ToRest(object? value)
    {
        return value switch
        {
            null => null,
            Guid g => g.ToString(),
            _ => value.ToString(),
        };
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

        var guidString = value switch
        {
            Guid g => g.ToString(),
            _ => value.ToString()!,
        };

        return Value.ForString(guidString);
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

        var guidString = value.ToString();
        if (string.IsNullOrEmpty(guidString))
            return null;

        return Guid.Parse(guidString);
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

        var guidString = value.StringValue;
        if (string.IsNullOrEmpty(guidString))
            return null;

        return Guid.Parse(guidString);
    }
}

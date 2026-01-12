using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "blob" data type (no array variant).
/// Stores binary data as Base64 strings.
/// </summary>
internal class BlobPropertyConverter : PropertyConverterBase
{
    /// <summary>
    /// Gets the value of the data type
    /// </summary>
    public override string DataType => "blob";

    /// <summary>
    /// Gets the value of the supports array
    /// </summary>
    public override bool SupportsArray => false;

    /// <summary>
    /// Gets the value of the supported types
    /// </summary>
    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(byte[])];

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
            byte[] bytes => Convert.ToBase64String(bytes),
            _ => null,
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

        if (value is byte[] bytes)
            return Value.ForString(Convert.ToBase64String(bytes));

        return Value.ForNull();
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

        var base64 = value.ToString();
        if (string.IsNullOrEmpty(base64))
            return null;

        return Convert.FromBase64String(base64);
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

        var base64 = value.StringValue;
        if (string.IsNullOrEmpty(base64))
            return null;

        return Convert.FromBase64String(base64);
    }
}

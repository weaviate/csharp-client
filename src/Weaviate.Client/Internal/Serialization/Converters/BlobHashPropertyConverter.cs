using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "blobHash" data type (no array variant).
/// Stores the hash of a blob as a string instead of the blob data itself.
/// </summary>
internal class BlobHashPropertyConverter : PropertyConverterBase
{
    /// <summary>
    /// Gets the value of the data type
    /// </summary>
    public override string DataType => "blobHash";

    /// <summary>
    /// Gets the value of the supports array
    /// </summary>
    public override bool SupportsArray => false;

    /// <summary>
    /// Gets the value of the supported types.
    /// Empty to avoid conflicting with TextPropertyConverter for string lookups.
    /// </summary>
    public override IReadOnlyList<System.Type> SupportedTypes => [];

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
            string s => s,
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

        if (value is string s)
            return Value.ForString(s);

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

        return value.ToString();
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

        return value.StringValue;
    }
}

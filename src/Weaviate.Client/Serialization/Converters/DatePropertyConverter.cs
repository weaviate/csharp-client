using System.Globalization;
using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "date" and "date[]" data types.
/// Uses ISO 8601 format for REST serialization.
/// </summary>
public class DatePropertyConverter : PropertyConverterBase
{
    public override string DataType => "date";

    public override IReadOnlyList<System.Type> SupportedTypes =>
        [typeof(DateTime), typeof(DateTimeOffset), typeof(DateTime?), typeof(DateTimeOffset?)];

    public override object? ToRest(object? value)
    {
        return value switch
        {
            null => null,
            DateTime dt => dt.ToUniversalTime().ToString("o"),
            DateTimeOffset dto => dto.UtcDateTime.ToString("o"),
            _ => null,
        };
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        var dateString = value switch
        {
            DateTime dt => dt.ToUniversalTime().ToString("o"),
            DateTimeOffset dto => dto.UtcDateTime.ToString("o"),
            _ => throw new ArgumentException($"Cannot convert {value.GetType()} to date"),
        };

        return Value.ForString(dateString);
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        // If already a DateTime, just ensure it's UTC
        if (value is DateTime dt)
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

        var dateString = value.ToString();
        if (string.IsNullOrEmpty(dateString))
            return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Try parsing with multiple formats for compatibility
        DateTime parsed;
        if (
            !DateTime.TryParse(
                dateString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces,
                out parsed
            )
        )
        {
            // Fall back to current culture parsing (for formats like "20/11/2025 20:19:52")
            parsed = DateTime.Parse(dateString);
        }

        var utc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        if (underlying == typeof(DateTimeOffset))
            return new DateTimeOffset(utc);

        return utc;
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        var dateString = value.StringValue;
        if (string.IsNullOrEmpty(dateString))
            return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Try parsing with multiple formats for compatibility
        DateTime parsed;
        if (
            !DateTime.TryParse(
                dateString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces,
                out parsed
            )
        )
        {
            // Fall back to current culture parsing
            parsed = DateTime.Parse(dateString);
        }

        var utc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        if (underlying == typeof(DateTimeOffset))
            return new DateTimeOffset(utc);

        return utc;
    }
}

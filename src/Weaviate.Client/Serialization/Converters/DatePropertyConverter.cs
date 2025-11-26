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

        // If already a DateTime, ensure it's marked as UTC
        if (value is DateTime dt)
        {
            // If already UTC, return as-is
            if (dt.Kind == DateTimeKind.Utc)
                return dt;

            // Otherwise, mark as UTC (server always sends UTC)
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        var dateString = value.ToString();
        if (string.IsNullOrEmpty(dateString))
            return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Server always returns dates in UTC, so parse as UTC
        DateTime parsed;
        if (
            !DateTime.TryParse(
                dateString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out parsed
            )
        )
        {
            // Fall back to current culture parsing and mark as UTC
            parsed = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
        }

        // Ensure the result is UTC
        var utc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        if (underlying == typeof(DateTimeOffset))
            return new DateTimeOffset(utc);

        // Return as UTC - server always sends UTC, application can convert if needed
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

        // Server always returns dates in UTC, so parse as UTC
        DateTime parsed;
        if (
            !DateTime.TryParse(
                dateString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out parsed
            )
        )
        {
            // Fall back to current culture parsing and mark as UTC
            parsed = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
            parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }

        // Ensure the result is UTC
        var utc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        if (underlying == typeof(DateTimeOffset))
            return new DateTimeOffset(utc);

        // Return as UTC - server always sends UTC, application can convert if needed
        return utc;
    }
}

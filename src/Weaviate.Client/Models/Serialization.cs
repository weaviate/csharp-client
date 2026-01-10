using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// The json converter empty collection as null class
/// </summary>
/// <seealso cref="JsonConverter{IEnumerable}"/>
public class JsonConverterEmptyCollectionAsNull : JsonConverter<IEnumerable>
{
    /// <summary>
    /// Reads the reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The options</param>
    /// <returns>The enumerable</returns>
    public override IEnumerable? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // Deserialize to the actual type
        return (IEnumerable?)JsonSerializer.Deserialize(ref reader, typeToConvert, options);
    }

    /// <summary>
    /// Writes the writer
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    /// <param name="options">The options</param>
    public override void Write(
        Utf8JsonWriter writer,
        IEnumerable value,
        JsonSerializerOptions options
    )
    {
        // If the collection is null or empty, write null
        if (value == null || !value.Cast<object>().Any())
        {
            writer.WriteNullValue();
        }
        else
        {
            // Serialize normally using the actual type
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }

    /// <summary>
    /// Cans the convert using the specified type to convert
    /// </summary>
    /// <param name="typeToConvert">The type to convert</param>
    /// <returns>The bool</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IEnumerable).IsAssignableFrom(typeToConvert)
            && typeToConvert != typeof(string); // Exclude strings
    }
}

/// <summary>
/// The flexible string converter class
/// </summary>
/// <seealso cref="JsonConverter{string?}"/>
public class FlexibleStringConverter : JsonConverter<string?>
{
    /// <summary>
    /// Reads the reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The options</param>
    /// <exception cref="JsonException">Cannot convert token type {reader.TokenType} to string?</exception>
    /// <returns>The string</returns>
    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();

            case JsonTokenType.StartArray:
                // Read the array and extract the first (and ideally only) element
                var array = JsonSerializer.Deserialize<string[]>(ref reader, options);
                return array?.Length > 0 ? array[0] : null;

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException($"Cannot convert token type {reader.TokenType} to string?");
        }
    }

    /// <summary>
    /// Writes the writer
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    /// <param name="options">The options</param>
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (!string.IsNullOrEmpty(value))
            writer.WriteStringValue(value);
        else
            writer.WriteNullValue();
    }
}

/// <summary>
/// The flexible converter class
/// </summary>
/// <seealso cref="JsonConverter{T?}"/>
public class FlexibleConverter<T> : JsonConverter<T?>
    where T : struct
{
    /// <summary>
    /// Reads the reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The options</param>
    /// <exception cref="JsonException">Cannot convert token type {reader.TokenType} to {typeof(T).Name}?</exception>
    /// <returns>The</returns>
    public override T? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            case JsonTokenType.String:
            case JsonTokenType.True:
            case JsonTokenType.False:
                // Handle direct value conversion
                return ConvertToType(ref reader, typeof(T));

            case JsonTokenType.StartArray:
                // Read the array and extract the first element
                using (var doc = JsonDocument.ParseValue(ref reader))
                {
                    var array = doc.RootElement;
                    if (array.GetArrayLength() > 0)
                    {
                        var firstElement = array[0];
                        return ConvertElementToType(firstElement, typeof(T));
                    }
                    return null;
                }

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException(
                    $"Cannot convert token type {reader.TokenType} to {typeof(T).Name}?"
                );
        }
    }

    /// <summary>
    /// Writes the writer
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    /// <param name="options">The options</param>
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            WriteValue(writer, value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }

    /// <summary>
    /// Converts the to type using the specified reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="targetType">The target type</param>
    /// <exception cref="JsonException">Cannot convert value to {targetType.Name} </exception>
    /// <returns>The</returns>
    private T? ConvertToType(ref Utf8JsonReader reader, Type targetType)
    {
        try
        {
            if (targetType == typeof(int))
                return (T?)(object?)reader.GetInt32();
            else if (targetType == typeof(long))
                return (T?)(object?)reader.GetInt64();
            else if (targetType == typeof(double))
                return (T?)(object?)reader.GetDouble();
            else if (targetType == typeof(float))
                return (T?)(object?)reader.GetSingle();
            else if (targetType == typeof(decimal))
                return (T?)(object?)reader.GetDecimal();
            else if (targetType == typeof(bool))
                return (T?)(object?)reader.GetBoolean();
            else if (targetType == typeof(DateTime))
                return (T?)(object?)reader.GetDateTime();
            else if (targetType == typeof(DateTimeOffset))
                return (T?)(object?)reader.GetDateTimeOffset();
            else if (targetType == typeof(Guid))
                return (T?)(object?)reader.GetGuid();
            else if (targetType.IsEnum)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var enumString = reader.GetString();
                    return (T?)(object?)Enum.Parse(targetType, enumString!, true);
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    var enumValue = reader.GetInt32();
                    return (T?)(object?)Enum.ToObject(targetType, enumValue);
                }
            }

            // Fallback: try to deserialize as the target type
            var json = JsonSerializer.Serialize(GetReaderValue(ref reader));
            return JsonSerializer.Deserialize<T?>(json);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Cannot convert value to {targetType.Name}", ex);
        }
    }

    /// <summary>
    /// Converts the element to type using the specified element
    /// </summary>
    /// <param name="element">The element</param>
    /// <param name="targetType">The target type</param>
    /// <exception cref="JsonException">Cannot convert array element to {targetType.Name} </exception>
    /// <returns>The</returns>
    private T? ConvertElementToType(JsonElement element, Type targetType)
    {
        try
        {
            if (targetType == typeof(int))
                return (T?)(object?)element.GetInt32();
            else if (targetType == typeof(long))
                return (T?)(object?)element.GetInt64();
            else if (targetType == typeof(double))
                return (T?)(object?)element.GetDouble();
            else if (targetType == typeof(float))
                return (T?)(object?)element.GetSingle();
            else if (targetType == typeof(decimal))
                return (T?)(object?)element.GetDecimal();
            else if (targetType == typeof(bool))
                return (T?)(object?)element.GetBoolean();
            else if (targetType == typeof(DateTime))
                return (T?)(object?)element.GetDateTime();
            else if (targetType == typeof(DateTimeOffset))
                return (T?)(object?)element.GetDateTimeOffset();
            else if (targetType == typeof(Guid))
                return (T?)(object?)element.GetGuid();
            else if (targetType.IsEnum)
            {
                if (element.ValueKind == JsonValueKind.String)
                {
                    var enumString = element.GetString();
                    return (T?)(object?)Enum.Parse(targetType, enumString!, true);
                }
                else if (element.ValueKind == JsonValueKind.Number)
                {
                    var enumValue = element.GetInt32();
                    return (T?)(object?)Enum.ToObject(targetType, enumValue);
                }
            }

            // Fallback: deserialize the element
            return element.Deserialize<T?>();
        }
        catch (Exception ex)
        {
            throw new JsonException($"Cannot convert array element to {targetType.Name}", ex);
        }
    }

    /// <summary>
    /// Writes the value using the specified writer
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    private void WriteValue(Utf8JsonWriter writer, T value)
    {
        var type = typeof(T);

        if (type == typeof(int))
            writer.WriteNumberValue((int)(object)value);
        else if (type == typeof(long))
            writer.WriteNumberValue((long)(object)value);
        else if (type == typeof(double))
            writer.WriteNumberValue((double)(object)value);
        else if (type == typeof(float))
            writer.WriteNumberValue((float)(object)value);
        else if (type == typeof(decimal))
            writer.WriteNumberValue((decimal)(object)value);
        else if (type == typeof(bool))
            writer.WriteBooleanValue((bool)(object)value);
        else if (type == typeof(DateTime))
            writer.WriteStringValue((DateTime)(object)value);
        else if (type == typeof(DateTimeOffset))
            writer.WriteStringValue((DateTimeOffset)(object)value);
        else if (type == typeof(Guid))
            writer.WriteStringValue((Guid)(object)value);
        else if (type.IsEnum)
            writer.WriteStringValue(value.ToString());
        else
            JsonSerializer.Serialize(writer, value);
    }

    /// <summary>
    /// Gets the reader value using the specified reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <exception cref="JsonException">Unsupported token type: {reader.TokenType}</exception>
    /// <returns>The object</returns>
    private object GetReaderValue(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDecimal(),
            JsonTokenType.String => reader.GetString()!,
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            _ => throw new JsonException($"Unsupported token type: {reader.TokenType}"),
        };
    }
}

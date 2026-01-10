using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest;

/// <summary>
/// The enum member json converter class
/// </summary>
/// <seealso cref="JsonConverter{T}"/>
internal class EnumMemberJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    /// <summary>
    /// The enum to string
    /// </summary>
    private readonly Dictionary<T, string> _enumToString = new();

    /// <summary>
    /// The string to enum
    /// </summary>
    private readonly Dictionary<string, T> _stringToEnum = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumMemberJsonConverter{T}"/> class
    /// </summary>
    public EnumMemberJsonConverter()
    {
        var type = typeof(T);
        foreach (var field in type.GetFields())
        {
            if (field.IsStatic && field.IsPublic)
            {
                var enumValue = (T)field.GetValue(null)!;
                var enumMemberAttr = field.GetCustomAttribute<EnumMemberAttribute>();

                string stringValue = enumMemberAttr?.Value ?? enumValue.ToString();

                _enumToString[enumValue] = stringValue;
                _stringToEnum[stringValue] = enumValue;
            }
        }
    }

    /// <summary>
    /// Reads the reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The options</param>
    /// <exception cref="WeaviateClientException"></exception>
    /// <returns>The</returns>
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var stringValue = reader.GetString();
        if (stringValue != null && _stringToEnum.TryGetValue(stringValue, out var enumValue))
        {
            return enumValue;
        }

        throw new WeaviateClientException(
            $"Unable to deserialize \"{stringValue}\" to enum {typeof(T).Name}. "
                + $"This may indicate the Weaviate server returned a value not yet supported by this client version. "
                + $"Consider upgrading the Weaviate C# client library.",
            new JsonException($"Unable to convert \"{stringValue}\" to enum {typeof(T)}")
        );
    }

    /// <summary>
    /// Writes the writer
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    /// <param name="options">The options</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_enumToString[value]);
    }
}

/// <summary>
/// The enum member json converter factory class
/// </summary>
/// <seealso cref="JsonConverterFactory"/>
internal class EnumMemberJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Cans the convert using the specified type to convert
    /// </summary>
    /// <param name="typeToConvert">The type to convert</param>
    /// <returns>The bool</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    /// <summary>
    /// Creates the converter using the specified type to convert
    /// </summary>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The options</param>
    /// <returns>The json converter</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumMemberJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

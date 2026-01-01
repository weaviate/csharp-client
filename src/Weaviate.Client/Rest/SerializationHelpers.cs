using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest;

internal class EnumMemberJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private readonly Dictionary<T, string> _enumToString = new();
    private readonly Dictionary<string, T> _stringToEnum = new();

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

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_enumToString[value]);
    }
}

internal class EnumMemberJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumMemberJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

using Google.Protobuf.WellKnownTypes;
using Weaviate.Client.Models;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "phoneNumber" data type (no array variant).
/// </summary>
internal class PhonePropertyConverter : PropertyConverterBase
{
    /// <summary>
    /// Gets the value of the data type
    /// </summary>
    public override string DataType => "phoneNumber";

    /// <summary>
    /// Gets the value of the supports array
    /// </summary>
    public override bool SupportsArray => false;

    /// <summary>
    /// Gets the value of the supported types
    /// </summary>
    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(PhoneNumber)];

    /// <summary>
    /// Returns the rest using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The object</returns>
    public override object? ToRest(object? value)
    {
        if (value is not PhoneNumber phone)
            return null;

        return phone.ToDto();
    }

    /// <summary>
    /// Returns the grpc using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The value</returns>
    public override Value ToGrpc(object? value)
    {
        if (value is not PhoneNumber phone)
            return Value.ForNull();

        var phoneStruct = new Struct();
        phoneStruct.Fields["input"] = Value.ForString(phone.Input ?? "");

        if (!string.IsNullOrEmpty(phone.DefaultCountry))
            phoneStruct.Fields["defaultCountry"] = Value.ForString(phone.DefaultCountry);

        return Value.ForStruct(phoneStruct);
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

        // If already a PhoneNumber model, return it
        if (value is PhoneNumber phone)
            return phone;

        // Handle REST DTO type
        if (value is Rest.Dto.PhoneNumber dtoPhone)
        {
            return dtoPhone.ToModel();
        }

        // Handle dictionary (both nullable and non-nullable)
        if (value is IDictionary<string, object?> dictNullable)
        {
            return CreatePhoneFromDict(dictNullable);
        }

        if (value is IDictionary<string, object> dict)
        {
            var converted = dict.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
            return CreatePhoneFromDict(converted);
        }

        return null;
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

        if (value.KindCase == Value.KindOneofCase.StructValue)
        {
            var s = value.StructValue;
            var dict = s.Fields.ToDictionary(
                kvp => kvp.Key,
                kvp => (object?)GetValueFromProto(kvp.Value)
            );
            return CreatePhoneFromDict(dict);
        }

        return null;
    }

    /// <summary>
    /// Creates the phone from dict using the specified dict
    /// </summary>
    /// <param name="dict">The dict</param>
    /// <returns>The phone</returns>
    private static PhoneNumber CreatePhoneFromDict(IDictionary<string, object?> dict)
    {
        var input = dict.TryGetValue("input", out var inputVal) ? inputVal?.ToString() ?? "" : "";

        var phone = new PhoneNumber(input)
        {
            DefaultCountry = dict.TryGetValue("defaultCountry", out var dc) ? dc?.ToString() : null,
            InternationalFormatted = dict.TryGetValue("internationalFormatted", out var intFmt)
                ? intFmt?.ToString()
                : null,
            NationalFormatted = dict.TryGetValue("nationalFormatted", out var natFmt)
                ? natFmt?.ToString()
                : null,
            CountryCode =
                dict.TryGetValue("countryCode", out var cc) && cc != null
                    ? Convert.ToUInt64(cc)
                    : null,
            National =
                dict.TryGetValue("national", out var nat) && nat != null
                    ? Convert.ToUInt64(nat)
                    : null,
            Valid =
                dict.TryGetValue("valid", out var valid) && valid != null
                    ? Convert.ToBoolean(valid)
                    : null,
        };

        return phone;
    }

    /// <summary>
    /// Gets the value from proto using the specified v
    /// </summary>
    /// <param name="v">The </param>
    /// <returns>The object</returns>
    private static object? GetValueFromProto(Value v)
    {
        return v.KindCase switch
        {
            Value.KindOneofCase.StringValue => v.StringValue,
            Value.KindOneofCase.NumberValue => v.NumberValue,
            Value.KindOneofCase.BoolValue => v.BoolValue,
            Value.KindOneofCase.NullValue => null,
            _ => null,
        };
    }
}

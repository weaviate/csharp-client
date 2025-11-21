using Google.Protobuf.WellKnownTypes;
using Weaviate.Client.Models;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "phoneNumber" data type (no array variant).
/// </summary>
public class PhonePropertyConverter : PropertyConverterBase
{
    public override string DataType => "phoneNumber";
    public override bool SupportsArray => false;

    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(PhoneNumber)];

    public override object? ToRest(object? value)
    {
        if (value is not PhoneNumber phone)
            return null;

        return phone.ToDto();
    }

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

using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "object" and "object[]" data types (nested objects).
/// Delegates property conversion to the PropertyConverterRegistry.
/// </summary>
internal class ObjectPropertyConverter : PropertyConverterBase
{
    private readonly Func<PropertyConverterRegistry> _registryFactory;

    public ObjectPropertyConverter(Func<PropertyConverterRegistry> registryFactory)
    {
        _registryFactory = registryFactory;
    }

    public override string DataType => "object";

    public override IReadOnlyList<System.Type> SupportedTypes =>
        [typeof(object), typeof(IDictionary<string, object>)];

    public override object? ToRest(object? value)
    {
        if (value is null)
            return null;

        var registry = _registryFactory();
        return registry.SerializeToRest(value);
    }

    public override Value ToGrpc(object? value)
    {
        if (value is null)
            return Value.ForNull();

        var registry = _registryFactory();
        var dict = registry.SerializeToRest(value);

        if (dict is IDictionary<string, object?> propDict)
        {
            var structValue = new Struct();
            foreach (var kvp in propDict)
            {
                structValue.Fields[kvp.Key] = ConvertToProtoValue(kvp.Value, registry);
            }
            return Value.ForStruct(structValue);
        }

        return Value.ForNull();
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        var registry = _registryFactory();

        // Handle both nullable and non-nullable dictionary types
        if (value is IDictionary<string, object?> dict)
        {
            return registry.BuildConcreteTypeFromProperties(dict, targetType);
        }

        if (value is IDictionary<string, object> dictNonNullable)
        {
            var converted = dictNonNullable.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
            return registry.BuildConcreteTypeFromProperties(converted, targetType);
        }

        return null;
    }

    public override object? FromRestArray(IEnumerable<object?> values, System.Type elementType)
    {
        var registry = _registryFactory();
        var items = new List<object?>();

        foreach (var value in values)
        {
            if (value is IDictionary<string, object?> dict)
            {
                items.Add(registry.BuildConcreteTypeFromProperties(dict, elementType));
            }
            else if (value is IDictionary<string, object> dictNonNullable)
            {
                var converted = dictNonNullable.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object?)kvp.Value
                );
                items.Add(registry.BuildConcreteTypeFromProperties(converted, elementType));
            }
            else
            {
                items.Add(value);
            }
        }

        return CreateTypedArray(items, elementType);
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        if (value.KindCase != Value.KindOneofCase.StructValue)
            return null;

        var registry = _registryFactory();
        var dict = ConvertStructToDict(value.StructValue);
        return registry.BuildConcreteTypeFromProperties(dict, targetType);
    }

    private static Value ConvertToProtoValue(object? value, PropertyConverterRegistry registry)
    {
        if (value is null)
            return Value.ForNull();

        var converter = registry.GetConverterForType(value.GetType());
        return converter?.ToGrpc(value) ?? Value.ForNull();
    }

    private static IDictionary<string, object?> ConvertStructToDict(Struct s)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var kvp in s.Fields)
        {
            dict[kvp.Key] = ConvertProtoValueToObject(kvp.Value);
        }

        return dict;
    }

    private static object? ConvertProtoValueToObject(Value v)
    {
        return v.KindCase switch
        {
            Value.KindOneofCase.NullValue => null,
            Value.KindOneofCase.NumberValue => v.NumberValue,
            Value.KindOneofCase.StringValue => v.StringValue,
            Value.KindOneofCase.BoolValue => v.BoolValue,
            Value.KindOneofCase.StructValue => ConvertStructToDict(v.StructValue),
            Value.KindOneofCase.ListValue => v
                .ListValue.Values.Select(ConvertProtoValueToObject)
                .ToList(),
            _ => null,
        };
    }
}

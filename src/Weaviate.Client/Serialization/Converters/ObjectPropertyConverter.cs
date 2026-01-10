using Google.Protobuf.WellKnownTypes;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "object" and "object[]" data types (nested objects).
/// Delegates property conversion to the PropertyConverterRegistry.
/// </summary>
internal class ObjectPropertyConverter : PropertyConverterBase
{
    /// <summary>
    /// The registry factory
    /// </summary>
    private readonly Func<PropertyConverterRegistry> _registryFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectPropertyConverter"/> class
    /// </summary>
    /// <param name="registryFactory">The registry factory</param>
    public ObjectPropertyConverter(Func<PropertyConverterRegistry> registryFactory)
    {
        _registryFactory = registryFactory;
    }

    /// <summary>
    /// Gets the value of the data type
    /// </summary>
    public override string DataType => "object";

    /// <summary>
    /// Gets the value of the supported types
    /// </summary>
    public override IReadOnlyList<System.Type> SupportedTypes =>
        [typeof(object), typeof(IDictionary<string, object>)];

    /// <summary>
    /// Returns the rest using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The object</returns>
    public override object? ToRest(object? value)
    {
        if (value is null)
            return null;

        var registry = _registryFactory();
        return registry.SerializeToRest(value);
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

    /// <summary>
    /// Creates the rest array using the specified values
    /// </summary>
    /// <param name="values">The values</param>
    /// <param name="elementType">The element type</param>
    /// <returns>The object</returns>
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

        if (value.KindCase != Value.KindOneofCase.StructValue)
            return null;

        var registry = _registryFactory();
        var dict = ConvertStructToDict(value.StructValue);
        return registry.BuildConcreteTypeFromProperties(dict, targetType);
    }

    /// <summary>
    /// Converts the to proto value using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="registry">The registry</param>
    /// <returns>The value</returns>
    private static Value ConvertToProtoValue(object? value, PropertyConverterRegistry registry)
    {
        if (value is null)
            return Value.ForNull();

        var converter = registry.GetConverterForType(value.GetType());
        return converter?.ToGrpc(value) ?? Value.ForNull();
    }

    /// <summary>
    /// Converts the struct to dict using the specified s
    /// </summary>
    /// <param name="s">The </param>
    /// <returns>The dict</returns>
    private static IDictionary<string, object?> ConvertStructToDict(Struct s)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var kvp in s.Fields)
        {
            dict[kvp.Key] = ConvertProtoValueToObject(kvp.Value);
        }

        return dict;
    }

    /// <summary>
    /// Converts the proto value to object using the specified v
    /// </summary>
    /// <param name="v">The </param>
    /// <returns>The object</returns>
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

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Weaviate.Client.Models;

namespace Weaviate.Client;

internal class ObjectHelper
{
    public static object? ConvertJsonElement(JsonElement? e)
    {
        if (e is null)
        {
            return null;
        }

        var element = e.Value;

        if (element.ValueKind == JsonValueKind.Object)
        {
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object?>)expando;

            foreach (var property in element.EnumerateObject())
            {
                dictionary[property.Name] = ConvertJsonElement(property.Value);
            }

            return expando;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            return element
                .EnumerateArray()
                .OfType<JsonElement?>()
                .Select(ConvertJsonElement)
                .ToArray();
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out int i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString(),
        };
    }

    public static string MakeBeaconSource(string collection, Guid fromUuid, string fromProperty) =>
        $"weaviate://localhost/{collection}/{fromUuid}/{fromProperty}";

    public static IDictionary<string, string>[] MakeBeacons(IEnumerable<Guid> guids)
    {
        return
        [
            .. guids.Select(uuid => new Dictionary<string, string>
            {
                { "beacon", $"weaviate://localhost/{uuid}" },
            }),
        ];
    }

    internal static T? UnmarshallProperties<T>(IDictionary<string, object?> dict)
        where T : new()
    {
        ArgumentNullException.ThrowIfNull(dict);

        // Create an instance of T using the default constructor
        var instance = new T();

        if (instance is IDictionary<string, object?> target)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value?.GetType() == typeof(Rest.Dto.GeoCoordinates))
                {
                    var value = (Rest.Dto.GeoCoordinates)kvp.Value;
                    target[kvp.Key.Capitalize()] = value.ToModel();
                }
                if (kvp.Value?.GetType() == typeof(Rest.Dto.PhoneNumber))
                {
                    var value = (Rest.Dto.PhoneNumber)kvp.Value;
                    target[kvp.Key.Capitalize()] = value.ToModel();
                }
                else if (kvp.Value is IDictionary<string, object?> subDict)
                {
                    object? nestedValue = UnmarshallProperties<ExpandoObject>(subDict);

                    target[kvp.Key.Capitalize()] = nestedValue ?? subDict;
                }
                else
                {
                    target[kvp.Key.Capitalize()] = kvp.Value;
                }
            }
            return instance;
        }

        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToArray();

        foreach (var property in properties)
        {
            var matchingKey = dict.Keys.FirstOrDefault(k =>
                string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase)
            );

            if (matchingKey is null)
            {
                continue;
            }

            var value = dict[matchingKey];

            try
            {
                // Convert the value to the target property type
                var convertedValue = ConvertValue(value, property.PropertyType);
                property.SetValue(instance, convertedValue);
            }
            catch (Exception ex)
            {
                // Skip if conversion fails
                Debug.WriteLine($"Failed to convert property {property.Name}: {ex.Message}");
                continue;
            }
        }

        return instance;
    }

    private static object? ConvertValue(object? value, System.Type targetType)
    {
        // Handle null values
        if (value == null)
        {
            if (IsNullableType(targetType) || !targetType.IsValueType)
            {
                return null;
            }
            // For non-nullable value types, return default value
            return Activator.CreateInstance(targetType);
        }

        // If types already match, return as-is
        if (targetType.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        // Handle nullable types
        if (IsNullableType(targetType))
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType)!;
            return ConvertValue(value, underlyingType);
        }

        if (targetType == typeof(GeoCoordinate) && value is Rest.Dto.GeoCoordinates geo)
        {
            return geo.ToModel();
        }

        if (targetType == typeof(Models.PhoneNumber) && value is Rest.Dto.PhoneNumber phoneNumber)
        {
            return phoneNumber.ToModel();
        }

        if (targetType == typeof(byte[]) && value is string base64String)
        {
            return Convert.FromBase64String(base64String);
        }

        if (targetType == typeof(DateTime) && value is string dateString)
        {
            return DateTime.SpecifyKind(DateTime.Parse(dateString), DateTimeKind.Utc);
        }

        if (targetType == typeof(DateTime) && value is DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        // Handle nested objects (dictionaries -> custom types)
        if (
            value is IDictionary<string, object?> nestedDict
            && !typeof(IDictionary<string, object?>).IsAssignableFrom(targetType)
        )
        {
            var method = typeof(ObjectHelper)
                .GetMethod("UnmarshallProperties", BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(targetType);
            return method.Invoke(null, new object[] { nestedDict });
        }

        // Handle collections
        if (
            IsCollectionType(targetType)
            && value is System.Collections.IEnumerable enumerable
            && !(value is string)
        )
        {
            return ConvertCollection(enumerable, targetType);
        }

        // Handle enums
        if (targetType.IsEnum)
        {
            if (value is string stringValue)
            {
                return System.Enum.Parse(targetType, stringValue, true);
            }
            return System.Enum.ToObject(targetType, value);
        }

        // Try TypeConverter first (handles more cases than Convert.ChangeType)
        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(value.GetType()))
        {
            return converter.ConvertFrom(value);
        }

        // Fallback to Convert.ChangeType for basic types
        return Convert.ChangeType(value, targetType);
    }

    private static bool IsNullableType(System.Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static bool IsCollectionType(System.Type type)
    {
        return type.IsArray
            || (
                type.IsGenericType
                && (
                    type.GetGenericTypeDefinition() == typeof(List<>)
                    || type.GetGenericTypeDefinition() == typeof(IList<>)
                    || type.GetGenericTypeDefinition() == typeof(ICollection<>)
                    || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                )
            );
    }

    private static object? ConvertCollection(
        System.Collections.IEnumerable source,
        System.Type targetType
    )
    {
        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType()!;
            var items = new List<object?>();

            foreach (var item in source)
            {
                items.Add(ConvertValue(item, elementType));
            }

            var array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                array.SetValue(items[i], i);
            }
            return array;
        }

        if (targetType.IsGenericType)
        {
            var elementType = targetType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (System.Collections.IList)Activator.CreateInstance(listType)!;

            foreach (var item in source)
            {
                list.Add(ConvertValue(item, elementType));
            }

            return list;
        }

        // Fallback - convert to object array
        var fallbackItems = new List<object?>();
        foreach (var item in source)
        {
            fallbackItems.Add(item);
        }
        return fallbackItems.ToArray();
    }

    internal static IDictionary<string, object> BuildDataTransferObject(object? data)
    {
        var obj = new ExpandoObject();
        var propDict = obj as IDictionary<string, object?>;

        if (data is null)
        {
            return propDict!;
        }

        foreach (var propertyInfo in data.GetType().GetProperties())
        {
            if (!propertyInfo.CanRead)
                continue; // skip non-readable properties

            var value = propertyInfo.GetValue(data);
            var propertyName = propertyInfo.Name.Decapitalize();

            if (value is null)
                continue;

            var dataType = PropertyHelper.DataTypeForType(propertyInfo.PropertyType);

            if (dataType == DataType.GeoCoordinate && value is GeoCoordinate geoValue)
            {
                propDict[propertyName] = geoValue.ToDto();
                continue;
            }
            if (dataType == DataType.PhoneNumber && value is Models.PhoneNumber phoneValue)
            {
                propDict[propertyName] = phoneValue.ToDto();
                continue;
            }
            if (dataType == DataType.Date && value is DateTime dateValue)
            {
                propDict[propertyName] = dateValue.ToUniversalTime().ToString("o");
                continue;
            }
            if (dataType == DataType.DateArray && value is IEnumerable<DateTime> dateValues)
            {
                propDict[propertyName] = dateValues
                    .Select(d => d.ToUniversalTime().ToString("o"))
                    .ToArray();
                continue;
            }
            if (dataType == DataType.Uuid && value is Guid guidValue)
            {
                propDict[propertyName] = guidValue.ToString();
                continue;
            }
            if (dataType == DataType.UuidArray && value is IEnumerable<Guid> guidValues)
            {
                propDict[propertyName] = guidValues.Select(g => g.ToString()).ToArray();
                continue;
            }
            if (dataType == DataType.Text && value is string stringValue)
            {
                propDict[propertyName] = stringValue;
                continue;
            }
            if (dataType == DataType.TextArray && value is IEnumerable<string> stringValues)
            {
                propDict[propertyName] = stringValues.ToArray();
                continue;
            }
            if (dataType == DataType.Bool && value is bool boolValue)
            {
                propDict[propertyName] = boolValue;
                continue;
            }
            if (dataType == DataType.BoolArray && value is IEnumerable<bool> boolValues)
            {
                propDict[propertyName] = boolValues.ToArray();
                continue;
            }
            // For numeric types, check both int/long/float/double as needed
            if (dataType == DataType.Int)
            {
                propDict[propertyName] = Convert.ToInt64(value);
                continue;
            }
            if (dataType == DataType.IntArray)
            {
                var values = (IEnumerable)value;
                propDict[propertyName] = values.Cast<object>().Select(Convert.ToInt64).ToArray();
                continue;
            }
            if (dataType == DataType.Number)
            {
                propDict[propertyName] = Convert.ToDouble(value);
                continue;
            }
            if (dataType == DataType.NumberArray)
            {
                var values = (IEnumerable)value;
                propDict[propertyName] = values.Cast<object>().Select(Convert.ToDouble).ToArray();
                continue;
            }
            if (dataType == DataType.Number)
            {
                propDict[propertyName] = Convert.ToDouble(value);
                continue;
            }
            if (dataType == DataType.Blob && value is byte[] blobValue)
            {
                propDict[propertyName] = Convert.ToBase64String(blobValue);
                continue;
            }
            if (dataType == DataType.ObjectArray && value is Array array)
            {
                var arrayList = new List<object?>();
                foreach (var item in array)
                    arrayList.Add(BuildDataTransferObject(item));
                propDict[propertyName] = arrayList;
                continue;
            }
            if (dataType == DataType.Object)
            {
                propDict[propertyName] = BuildDataTransferObject(value);
                continue;
            }

            throw new WeaviateClientException(
                $"Unsupported property type '{propertyInfo.PropertyType.Name}' for property '{propertyInfo.Name}'. Check the documentation for supported value types."
            );
        }

        return propDict!;
    }

    internal static V1.BatchObject.Types.Properties BuildBatchProperties<TProps>(TProps data)
    {
        return BuildBatchProperties(data, []);
    }

    internal static V1.BatchObject.Types.Properties BuildBatchProperties<TProps>(
        TProps data,
        HashSet<int> visitedObjects
    )
    {
        var props = new V1.BatchObject.Types.Properties();

        if (data is null)
        {
            return props;
        }

        // Check for circular references
        if (!data.GetType().IsValueType)
        {
            var objectHash = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(data);
            if (visitedObjects.Contains(objectHash))
            {
                // Circular reference detected, return empty properties
                return props;
            }
            visitedObjects.Add(objectHash);
        }

        Google.Protobuf.WellKnownTypes.Struct? nonRefProps = null;

        var type = typeof(TProps);

        if (type == typeof(object))
        {
            type = data.GetType();
        }

        foreach (var propertyInfo in type.GetProperties())
        {
            if (propertyInfo is null)
            {
                continue;
            }

            if (!propertyInfo.CanRead)
                continue; // skip non-readable properties

            var value = propertyInfo.GetValue(data);

            if (value is null)
            {
                continue;
            }

            var propType = propertyInfo.PropertyType;
            var propName = propertyInfo.Name.Decapitalize();

            // Handle arrays and collections
            if (
                (propType.IsArray && propType != typeof(byte[]))
                || (
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(propType)
                    && !(propType == typeof(string) || propType == typeof(byte[]))
                )
            )
            {
                var elementType = propType.IsArray
                    ? propType.GetElementType()
                    : propType.GetGenericArguments().FirstOrDefault();

                // Handle array of objects (nested objects)
                if (
                    elementType != null
                    && !elementType.IsNativeType()
                    && value is System.Collections.IEnumerable objEnumerable
                )
                {
                    var listValue = new Google.Protobuf.WellKnownTypes.ListValue();
                    foreach (var item in objEnumerable)
                    {
                        if (item != null)
                        {
                            var structValue = new Google.Protobuf.WellKnownTypes.Struct();
                            foreach (var nestedProp in item.GetType().GetProperties())
                            {
                                if (!nestedProp.CanRead)
                                    continue;
                                var nestedVal = nestedProp.GetValue(item);
                                if (nestedVal == null)
                                    continue;
                                var nestedPropName = nestedProp.Name.Decapitalize();

                                if (nestedProp.PropertyType.IsNativeType())
                                {
                                    structValue.Fields[nestedPropName] = ConvertToProtoValue(
                                        nestedVal
                                    );
                                }
                                else
                                {
                                    structValue.Fields[nestedPropName] = ConvertToProtoValue(
                                        BuildBatchProperties(
                                            nestedVal,
                                            visitedObjects
                                        ).NonRefProperties
                                    );
                                }
                            }
                            listValue.Values.Add(
                                Google.Protobuf.WellKnownTypes.Value.ForStruct(structValue)
                            );
                        }
                    }
                    nonRefProps ??= new();
                    nonRefProps.Fields.Add(
                        propName,
                        Google.Protobuf.WellKnownTypes.Value.ForList(listValue.Values.ToArray())
                    );
                    continue;
                }

                // Handle primitive arrays/collections
                switch (value)
                {
                    case bool[] v:
                        props.BooleanArrayProperties.Add(
                            new V1.BooleanArrayProperties() { PropName = propName, Values = { v } }
                        );
                        break;
                    case int[] v:
                        props.IntArrayProperties.Add(
                            new V1.IntArrayProperties()
                            {
                                PropName = propName,
                                Values = { v.Select(Convert.ToInt64) },
                            }
                        );
                        break;
                    case long[] v:
                        props.IntArrayProperties.Add(
                            new V1.IntArrayProperties() { PropName = propName, Values = { v } }
                        );
                        break;
                    case double[] v:
                        props.NumberArrayProperties.Add(
                            new V1.NumberArrayProperties()
                            {
                                PropName = propName,
                                ValuesBytes = v.ToByteString(),
                            }
                        );
                        break;
                    case float[] v:
                        props.NumberArrayProperties.Add(
                            new V1.NumberArrayProperties()
                            {
                                PropName = propName,
                                ValuesBytes = v.Select(Convert.ToDouble).ToByteString(),
                            }
                        );
                        break;
                    case string[] v:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties() { PropName = propName, Values = { v } }
                        );
                        break;
                    case Guid[] v:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propName,
                                Values = { v.Select(g => g.ToString()) },
                            }
                        );
                        break;
                    case DateTime[] v:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propName,
                                Values = { v.Select(d => d.ToUniversalTime().ToString("o")) },
                            }
                        );
                        break;
                    case DateTimeOffset[] v:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propName,
                                Values = { v.Select(dto => dto.ToUniversalTime().ToString("o")) },
                            }
                        );
                        break;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<bool> bools:
                        props.BooleanArrayProperties.Add(
                            new V1.BooleanArrayProperties()
                            {
                                PropName = propName,
                                Values = { bools },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<int> ints:
                        props.IntArrayProperties.Add(
                            new V1.IntArrayProperties()
                            {
                                PropName = propName,
                                Values = { ints.Select(Convert.ToInt64) },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<long> longs:
                        props.IntArrayProperties.Add(
                            new V1.IntArrayProperties() { PropName = propName, Values = { longs } }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<double> doubles:
                        props.NumberArrayProperties.Add(
                            new V1.NumberArrayProperties()
                            {
                                PropName = propName,
                                ValuesBytes = doubles.ToByteString(),
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<float> floats:
                        props.NumberArrayProperties.Add(
                            new V1.NumberArrayProperties()
                            {
                                PropName = propName,
                                ValuesBytes = floats
                                    .Select(f => Convert.ToDouble(f))
                                    .ToByteString(),
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<string> strings:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propName,
                                Values = { strings },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<Guid> guids:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propName,
                                Values = { guids.Select(g => g.ToString()) },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<DateTime> dateTimes:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propName,
                                Values =
                                {
                                    dateTimes.Select(dt => dt.ToUniversalTime().ToString("o")),
                                },
                            }
                        );
                        continue;
                    case System.Collections.IEnumerable enumerable
                        when enumerable is IEnumerable<DateTimeOffset> dateTimeOffsets:
                        props.TextArrayProperties.Add(
                            new V1.TextArrayProperties()
                            {
                                PropName = propName,
                                Values =
                                {
                                    dateTimeOffsets.Select(dto =>
                                        dto.ToUniversalTime().ToString("o")
                                    ),
                                },
                            }
                        );
                        continue;
                    default:
                        throw new WeaviateClientException(
                            $"Unsupported array type '{value.GetType().GetElementType()?.Name ?? value.GetType().Name}' for property '{propName}'. Check the documentation for supported array value types."
                        );
                }
                continue;
            }

            // Handle nested object
            if (!propType.IsNativeType())
            {
                nonRefProps ??= new();
                var nestedStruct = BuildBatchProperties(value, visitedObjects).NonRefProperties;
                if (nestedStruct != null)
                {
                    nonRefProps.Fields.Add(
                        propName,
                        Google.Protobuf.WellKnownTypes.Value.ForStruct(nestedStruct)
                    );
                }
                continue;
            }

            // Handle native types
            if (propType.IsNativeType())
            {
                nonRefProps ??= new();
                nonRefProps.Fields.Add(propName, ConvertToProtoValue(value));
            }
        }

        props.NonRefProperties = nonRefProps;

        return props;
    }

    /// <summary>
    /// Converts any IMessage to a Google.Protobuf.WellKnownTypes.Struct
    /// efficiently using reflection.
    /// </summary>
    public static Struct ToStruct(IMessage message)
    {
        var structFields = new Struct().Fields;

        foreach (var field in message.Descriptor.Fields.InDeclarationOrder())
        {
            var value = field.Accessor.GetValue(message);

            // 1. Skip nulls (applies to message types)
            if (value == null)
            {
                continue;
            }

            // 2. Skip fields that are set to their default "zero" value
            if (field.IsRepeated)
            {
                if (value is IList list && list.Count == 0)
                    continue;
            }
            else if (field.IsMap)
            {
                if (value is IDictionary map && map.Count == 0)
                    continue;
            }
            else
            {
                // Check for scalar defaults
                switch (field.FieldType)
                {
                    case FieldType.Bool:
                        if (value is false)
                            continue;
                        break;
                    case FieldType.String:
                        if (value is "")
                            continue;
                        break;
                    case FieldType.Bytes:
                        if (value is ByteString bs && bs.IsEmpty)
                            continue;
                        break;
                    case FieldType.Double:
                        if (value is 0.0d)
                            continue;
                        break;
                    case FieldType.Float:
                        if (value is 0.0f)
                            continue;
                        break;
                    case FieldType.Int64:
                    case FieldType.SFixed64:
                    case FieldType.SInt64:
                        if (value is 0L)
                            continue;
                        break;
                    case FieldType.UInt64:
                    case FieldType.Fixed64:
                        if (value is 0UL)
                            continue;
                        break;
                    case FieldType.Int32:
                    case FieldType.SFixed32:
                    case FieldType.SInt32:
                        if (value is 0)
                            continue;
                        break;
                    case FieldType.UInt32:
                    case FieldType.Fixed32:
                        if (value is 0U)
                            continue;
                        break;
                    case FieldType.Enum:
                        // The default for an enum is its 0-value.
                        // The accessor returns the C# enum, so we cast to int.
                        if (Convert.ToInt32(value) == 0)
                            continue;
                        break;
                    // FieldType.Message is handled by the null check above
                }
            }

            // If we got here, the value is not default. Add it.
            // Pass the 'field' so we can handle enums correctly.
            structFields[field.JsonName] = ConvertToProtoValue(value, field);
        }

        return new Struct { Fields = { structFields } };
    }

    /// <summary>
    /// Recursively converts a C# object into a Protobuf Value.
    /// </summary>
    private static Value ConvertToProtoValue(object value, FieldDescriptor field)
    {
        switch (value)
        {
            // Handle nested messages
            case IMessage messageValue:
                return Value.ForStruct(ToStruct(messageValue));

            // Handle repeated fields (lists)
            case IList listValue:
                var list = new ListValue();
                foreach (var item in listValue)
                {
                    // Pass the field descriptor for nested enum lookups
                    list.Values.Add(ConvertToProtoValue(item, field));
                }
                return Value.ForList(list.Values.ToArray());

            // Handle primitive types
            case bool boolValue:
                return Value.ForBool(boolValue);
            case string stringValue:
                return Value.ForString(stringValue);
            case int:
            case uint:
            case long:
            case ulong:
            case float:
            case double:
                return Value.ForNumber(Convert.ToDouble(value));

            case ByteString byteStringValue:
                return Value.ForString(byteStringValue.ToBase64());

            // Handle Enums (serialize as string name)
            case System.Enum enumValue:
                // Find the string name from the enum's descriptor
                var enumDescriptor = field.EnumType.FindValueByNumber(Convert.ToInt32(value));
                if (enumDescriptor != null)
                {
                    // Standard case: "MY_ENUM_VALUE"
                    return Value.ForString(enumDescriptor.Name);
                }
                else
                {
                    // Unrecognized enum value: serialize as its number
                    return Value.ForNumber(Convert.ToDouble(value));
                }

            case null:
                return Value.ForNull();

            // Fallback for unknown types
            default:
                return Value.ForString(value.ToString());
        }
    }

    // Helper method to convert C# objects to protobuf Values
    internal static Value ConvertToProtoValue(object obj)
    {
        return obj switch
        {
            null => Value.ForNull(),
            // byte[] ba => Value.ForString(Convert.ToBase64String(ba)),
            bool b => Value.ForBool(b),
            int i => Value.ForNumber(i),
            long l => Value.ForNumber(l),
            float f => Value.ForNumber(f),
            double d => Value.ForNumber(d),
            decimal dec => Value.ForNumber((double)dec),
            string s => Value.ForString(s),
            DateTime dt => Value.ForString(dt.ToUniversalTime().ToString("o")),
            Guid uuid => Value.ForString(uuid.ToString()),
            GeoCoordinate v => Value.ForStruct(
                ToStruct(new V1.GeoCoordinate() { Latitude = v.Latitude, Longitude = v.Longitude })
            ),
            Models.PhoneNumber pn => Value.ForStruct(
                ToStruct(
                    new V1.PhoneNumber()
                    {
                        CountryCode = pn.CountryCode ?? 0,
                        DefaultCountry = pn.DefaultCountry,
                        Input = pn.Input,
                        InternationalFormatted = pn.InternationalFormatted,
                        National = pn.National ?? 0,
                        NationalFormatted = pn.NationalFormatted,
                        Valid = pn.Valid ?? false,
                    }
                )
            ),
            Google.Protobuf.WellKnownTypes.Struct s => Value.ForStruct(s),
            byte[] ba => Value.ForString(Convert.ToBase64String(ba)),
            _ => throw new ArgumentException($"Unsupported type: {obj.GetType()}"),
        };
    }

    internal static Guid GuidFromByteString(Google.Protobuf.ByteString x)
    {
        byte[] bytes = x.ToByteArray();
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes, 0, 4); // Reverse first 4 bytes
            Array.Reverse(bytes, 4, 2); // Reverse next 2 bytes
            Array.Reverse(bytes, 6, 2); // Reverse next 2 bytes
        }
        return new Guid(bytes);
    }
}

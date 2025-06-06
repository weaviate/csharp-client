namespace Weaviate.Client.Models;

internal class PropertyHelper
{
    internal static Property For(Type t, string name)
    {
        // Handle nullable types - get the underlying type
        Type actualType = Nullable.GetUnderlyingType(t) ?? t;

        // Handle special types first
        if (actualType == typeof(Guid))
        {
            return Property.Uuid(name);
        }

        if (actualType == typeof(GeoCoordinate))
        {
            return Property.GeoCoordinate(name);
        }

        if (actualType == typeof(PhoneNumber))
        {
            return Property.PhoneNumber(name);
        }

        // Handle primitive types
        Func<string, Property>? f = Type.GetTypeCode(actualType) switch
        {
            TypeCode.String => Property.Text,
            TypeCode.Int16 => Property.Int,
            TypeCode.UInt16 => Property.Int,
            TypeCode.Int32 => Property.Int,
            TypeCode.UInt32 => Property.Int,
            TypeCode.Int64 => Property.Int,
            TypeCode.UInt64 => Property.Int,
            TypeCode.DateTime => Property.Date,
            TypeCode.Boolean => Property.Bool,
            TypeCode.Char => Property.Text,
            TypeCode.SByte => null,
            TypeCode.Byte => null,
            TypeCode.Single => Property.Number,
            TypeCode.Double => Property.Number,
            TypeCode.Decimal => Property.Number,
            TypeCode.Empty => null,
            TypeCode.Object => null,
            TypeCode.DBNull => null,
            _ => null,
        };

        if (f is not null)
        {
            return f(name);
        }

        // Handle arrays and collections
        if (IsArrayOrCollection(actualType, out Type? elementType))
        {
            return HandleCollectionType(elementType, name);
        }

        throw new NotSupportedException($"Type {t.Name} not supported");
    }

    private static bool IsArrayOrCollection(Type type, out Type? elementType)
    {
        elementType = null;

        // Handle arrays
        if (type.IsArray)
        {
            elementType = type.GetElementType();
            return elementType != null;
        }

        // Handle generic collections (List<T>, IEnumerable<T>, etc.)
        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();

            // Check for common collection interfaces and types
            if (
                genericTypeDef == typeof(IEnumerable<>)
                || genericTypeDef == typeof(ICollection<>)
                || genericTypeDef == typeof(IList<>)
                || genericTypeDef == typeof(List<>)
                || genericTypeDef == typeof(HashSet<>)
                || genericTypeDef == typeof(ISet<>)
            )
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
        }

        // Check if type implements IEnumerable<T>
        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            );

        if (enumerableInterface != null)
        {
            elementType = enumerableInterface.GetGenericArguments()[0];
            return true;
        }

        return false;
    }

    private static Property HandleCollectionType(Type? elementType, string name)
    {
        if (elementType == null)
            return null!; // or throw an exception

        // Handle special collection element types
        if (elementType == typeof(Guid))
        {
            return Property.UuidArray(name); // Assuming you have array-specific methods
        }

        // Handle primitive collection element types
        Func<string, Property>? f = Type.GetTypeCode(elementType) switch
        {
            TypeCode.String => Property.TextArray,
            TypeCode.Int16 => Property.IntArray,
            TypeCode.UInt16 => Property.IntArray,
            TypeCode.Int32 => Property.IntArray,
            TypeCode.UInt32 => Property.IntArray,
            TypeCode.Int64 => Property.IntArray,
            TypeCode.UInt64 => Property.IntArray,
            TypeCode.DateTime => Property.DateArray,
            TypeCode.Boolean => Property.BoolArray,
            TypeCode.Char => Property.TextArray,
            TypeCode.Single => Property.NumberArray,
            TypeCode.Double => Property.NumberArray,
            TypeCode.Decimal => Property.NumberArray,
            _ => null,
        };

        return f!(name);
    }
}

public static class DataType
{
    public static string Text { get; } = "text";
    public static string TextArray { get; } = "text[]";
    public static string Int { get; } = "int";
    public static string IntArray { get; } = "int[]";
    public static string Bool { get; } = "boolean";
    public static string BoolArray { get; } = "boolean[]";
    public static string Number { get; } = "number";
    public static string NumberArray { get; } = "number[]";
    public static string Date { get; } = "date";
    public static string DateArray { get; } = "date[]";
    public static string Uuid { get; } = "uuid";
    public static string UuidArray { get; } = "uuid[]";
    public static string GeoCoordinate { get; } = "geoCoordinates";
    public static string Blob { get; } = "blob";
    public static string PhoneNumber { get; } = "phone";
    public static string Object { get; } = "object";
    public static string ObjectArray { get; } = "object[]";
}

public record ReferenceProperty
{
    public required string Name { get; set; }
    public required string TargetCollection { get; set; }

    public static implicit operator Property(ReferenceProperty p)
    {
        return new Property { Name = p.Name, DataType = { p.TargetCollection.Capitalize() } };
    }
}

public partial record Property
{
    public required string Name { get; set; }
    public IList<string> DataType { get; set; } = new List<string>();
    public string? Description { get; set; }
    public bool? IndexFilterable { get; set; }
    public bool? IndexInverted { get; set; }
    public bool? IndexRangeFilters { get; set; }
    public bool? IndexSearchable { get; set; }

    public static Property Text(string name) =>
        new() { Name = name, DataType = { Models.DataType.Text } };

    public static Property TextArray(string name) =>
        new() { Name = name, DataType = { Models.DataType.TextArray } };

    public static Property Int(string name) =>
        new() { Name = name, DataType = { Models.DataType.Int } };

    public static Property IntArray(string name) =>
        new() { Name = name, DataType = { Models.DataType.IntArray } };

    public static Property Bool(string name) =>
        new() { Name = name, DataType = { Models.DataType.Bool } };

    public static Property BoolArray(string name) =>
        new() { Name = name, DataType = { Models.DataType.BoolArray } };

    public static Property Number(string name) =>
        new() { Name = name, DataType = { Models.DataType.Number } };

    public static Property NumberArray(string name) =>
        new() { Name = name, DataType = { Models.DataType.NumberArray } };

    public static Property Date(string name) =>
        new() { Name = name, DataType = { Models.DataType.Date } };

    public static Property DateArray(string name) =>
        new() { Name = name, DataType = { Models.DataType.DateArray } };

    public static Property Uuid(string name) =>
        new() { Name = name, DataType = { Models.DataType.Uuid } };

    public static Property UuidArray(string name) =>
        new() { Name = name, DataType = { Models.DataType.UuidArray } };

    public static Property GeoCoordinate(string name) =>
        new() { Name = name, DataType = { Models.DataType.GeoCoordinate } };

    public static Property Blob(string name) =>
        new() { Name = name, DataType = { Models.DataType.Blob } };

    public static Property PhoneNumber(string name) =>
        new() { Name = name, DataType = { Models.DataType.PhoneNumber } };

    public static Property Object(string name) =>
        new() { Name = name, DataType = { Models.DataType.Object } };

    public static Property ObjectArray(string name) =>
        new() { Name = name, DataType = { Models.DataType.ObjectArray } };

    public static ReferenceProperty Reference(string name, string targetCollection) =>
        new() { Name = name, TargetCollection = targetCollection };

    public static Property For<TField>(string name) => PropertyHelper.For(typeof(TField), name);

    // Extract collection properties from type specified by TData.
    public static IList<Property> FromCollection<TData>()
    {
        return
        [
            .. typeof(TData)
                .GetProperties()
                .Select(x => PropertyHelper.For(x.PropertyType, x.Name)),
        ];
    }
}

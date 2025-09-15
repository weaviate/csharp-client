namespace Weaviate.Client.Models;

public enum PropertyTokenization
{
    Word = 0,
    Lowercase = 1,
    Whitespace = 2,
    Field = 3,
    Trigram = 4,
    Gse = 5,
    Kagome_kr = 6,
    Kagome_ja = 7,
    Gse_ch = 8,
}

public delegate Property PropertyFactory(
    string name,
    string? description = null,
    bool? indexFilterable = null,
    bool? indexRangeFilters = null,
    bool? indexSearchable = null,
    PropertyTokenization? tokenization = null
);

internal class PropertyHelper
{
    internal static PropertyFactory Factory(string dataType) =>
        (name, description, indexFilterable, indexRangeFilters, indexSearchable, tokenization) =>
            new Property
            {
                Name = name,
                DataType = { dataType },
                Description = description,
                IndexFilterable = indexFilterable,
                IndexRangeFilters = indexRangeFilters,
                IndexSearchable = indexSearchable,
                PropertyTokenization = tokenization,
            };

    internal static PropertyFactory ForType(Type t)
    {
        // Handle nullable types - get the underlying type
        Type actualType = Nullable.GetUnderlyingType(t) ?? t;

        // Handle special types first
        if (actualType == typeof(Guid))
        {
            return Property.Uuid;
        }

        if (actualType == typeof(GeoCoordinate))
        {
            return Property.GeoCoordinate;
        }

        if (actualType == typeof(PhoneNumber))
        {
            return Property.PhoneNumber;
        }

        // Handle primitive types
        PropertyFactory? f = Type.GetTypeCode(actualType) switch
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
            return f;
        }

        // Handle arrays and collections
        if (IsArrayOrCollection(actualType, out Type? elementType))
        {
            return HandleCollectionType(elementType);
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

    private static PropertyFactory HandleCollectionType(Type? elementType)
    {
        if (elementType == null)
            return null!; // or throw an exception

        // Handle special collection element types
        if (elementType == typeof(Guid))
        {
            return Property.UuidArray; // Assuming you have array-specific methods
        }

        // Handle primitive collection element types
        PropertyFactory? f = Type.GetTypeCode(elementType) switch
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

        return f!;
    }
}

public static class DataType
{
    public static string Text => "text";
    public static string TextArray => "text[]";
    public static string Int => "int";
    public static string IntArray => "int[]";
    public static string Bool => "boolean";
    public static string BoolArray => "boolean[]";
    public static string Number => "number";
    public static string NumberArray => "number[]";
    public static string Date => "date";
    public static string DateArray => "date[]";
    public static string Uuid => "uuid";
    public static string UuidArray => "uuid[]";
    public static string GeoCoordinate => "geoCoordinates";
    public static string Blob => "blob";
    public static string PhoneNumber => "phone";
    public static string Object => "object";
    public static string ObjectArray => "object[]";
}

public record Reference(string Name, string TargetCollection, string? Description = null)
{
    public static implicit operator Property(Reference p)
    {
        return new Property
        {
            Name = p.Name,
            DataType = { p.TargetCollection.Capitalize() },
            Description = p.Description,
        };
    }

    public static implicit operator Reference(Property p)
    {
        return new Reference(
            p.Name,
            p.DataType.First(t => char.IsUpper(t.First())).Decapitalize(),
            p.Description
        );
    }
}

public static class Property<TField>
{
    public static PropertyFactory New => PropertyHelper.ForType(typeof(TField));
}

public record Property : IEquatable<Property>
{
    private string _name = string.Empty;

    public required string Name
    {
        get => _name;
        set => _name = value.Decapitalize();
    }
    public IList<string> DataType { get; set; } = new List<string>();
    public string? Description { get; set; }
    public bool? IndexFilterable { get; set; }

    [Obsolete]
    public bool? IndexInverted { get; internal set; }
    public bool? IndexRangeFilters { get; set; }
    public bool? IndexSearchable { get; set; }
    public PropertyTokenization? PropertyTokenization { get; internal set; }

    public static PropertyFactory Text => PropertyHelper.Factory(Models.DataType.Text);
    public static PropertyFactory TextArray => PropertyHelper.Factory(Models.DataType.TextArray);
    public static PropertyFactory Int => PropertyHelper.Factory(Models.DataType.Int);
    public static PropertyFactory IntArray => PropertyHelper.Factory(Models.DataType.IntArray);
    public static PropertyFactory Bool => PropertyHelper.Factory(Models.DataType.Bool);
    public static PropertyFactory BoolArray => PropertyHelper.Factory(Models.DataType.BoolArray);
    public static PropertyFactory Number => PropertyHelper.Factory(Models.DataType.Number);
    public static PropertyFactory NumberArray =>
        PropertyHelper.Factory(Models.DataType.NumberArray);
    public static PropertyFactory Date => PropertyHelper.Factory(Models.DataType.Date);
    public static PropertyFactory DateArray => PropertyHelper.Factory(Models.DataType.DateArray);
    public static PropertyFactory Uuid => PropertyHelper.Factory(Models.DataType.Uuid);
    public static PropertyFactory UuidArray => PropertyHelper.Factory(Models.DataType.UuidArray);
    public static PropertyFactory GeoCoordinate =>
        PropertyHelper.Factory(Models.DataType.GeoCoordinate);
    public static PropertyFactory Blob => PropertyHelper.Factory(Models.DataType.Blob);
    public static PropertyFactory PhoneNumber =>
        PropertyHelper.Factory(Models.DataType.PhoneNumber);
    public static PropertyFactory Object => PropertyHelper.Factory(Models.DataType.Object);
    public static PropertyFactory ObjectArray =>
        PropertyHelper.Factory(Models.DataType.ObjectArray);

    public static Reference Reference(
        string name,
        string targetCollection,
        string? description = null
    ) => new(name, targetCollection, description);

    // Extract collection properties from type specified by TData.
    public static IEnumerable<Property> FromCollection<TData>()
    {
        return typeof(TData)
            .GetProperties()
            .Select(x => PropertyHelper.ForType(x.PropertyType)(x.Name));
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(DataType);
        hash.Add(Description);
#pragma warning disable CS0612 // Type or member is obsolete
        hash.Add(IndexInverted);
#pragma warning restore CS0612 // Type or member is obsolete
        hash.Add(IndexFilterable);
        hash.Add(IndexRangeFilters);
        hash.Add(IndexSearchable);
        hash.Add(PropertyTokenization);
        return hash.ToHashCode();
    }

    public virtual bool Equals(Property? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Name == other.Name
            && DataType.SequenceEqual(other.DataType)
            && Description == other.Description
            && IndexFilterable == other.IndexFilterable
            && IndexRangeFilters == other.IndexRangeFilters
            && IndexSearchable == other.IndexSearchable
            && PropertyTokenization == other.PropertyTokenization;
    }
}

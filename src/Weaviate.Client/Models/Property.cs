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
    PropertyTokenization? tokenization = null,
    Property[]? subProperties = null
);

internal class PropertyHelper
{
    internal static PropertyFactory Factory(string dataType) =>
        (
            name,
            description,
            indexFilterable,
            indexRangeFilters,
            indexSearchable,
            tokenization,
            subProperties
        ) =>
            new Property
            {
                Name = name,
                DataType = { dataType },
                Description = description,
                IndexFilterable = indexFilterable,
                IndexRangeFilters = indexRangeFilters,
                IndexSearchable = indexSearchable,
                PropertyTokenization = tokenization,
                NestedProperties =
                    (dataType == Models.DataType.Object || dataType == Models.DataType.ObjectArray)
                        ? subProperties
                        : null,
            };

    internal static string DataTypeForCollectionType(Type? elementType)
    {
        if (elementType == null)
            return null!; // or throw an exception

        // Handle special collection element types
        if (elementType == typeof(Guid))
        {
            return DataType.UuidArray; // Assuming you have array-specific methods
        }

        if (elementType == typeof(String))
        {
            return DataType.TextArray; // Assuming you have array-specific methods
        }

        var tc = Type.GetTypeCode(elementType);

        // Handle primitive collection element types
        string? f = tc switch
        {
            TypeCode.Int16 => DataType.IntArray,
            TypeCode.UInt16 => DataType.IntArray,
            TypeCode.Int32 => DataType.IntArray,
            TypeCode.UInt32 => DataType.IntArray,
            TypeCode.Int64 => DataType.IntArray,
            TypeCode.UInt64 => DataType.IntArray,
            TypeCode.DateTime => DataType.DateArray,
            TypeCode.Boolean => DataType.BoolArray,
            TypeCode.Byte => DataType.Blob,
            TypeCode.SByte => DataType.Blob,
            TypeCode.Char => DataType.TextArray,
            TypeCode.Single => DataType.NumberArray,
            TypeCode.Double => DataType.NumberArray,
            TypeCode.Decimal => DataType.NumberArray,
            TypeCode.Object => DataType.ObjectArray,
            _ => null,
        };

        return f!;
    }

    internal static string DataTypeForType(Type t)
    {
        // Handle nullable types - get the underlying type
        Type actualType = Nullable.GetUnderlyingType(t) ?? t;

        // Handle special types first
        if (actualType == typeof(Guid))
        {
            return DataType.Uuid;
        }

        if (actualType == typeof(GeoCoordinate))
        {
            return DataType.GeoCoordinate;
        }

        if (actualType == typeof(PhoneNumber))
        {
            return DataType.PhoneNumber;
        }

        // String must be handled early as it is also IEnumerable<char>,
        // which would be mistaken for a collection type.
        if (actualType == typeof(String))
        {
            return DataType.Text;
        }

        // Handle arrays and collections
        if (IsArrayOrCollection(actualType, out Type? elementType))
        {
            return DataTypeForCollectionType(elementType);
        }

        var tc = Type.GetTypeCode(actualType);

        // Handle primitive types
        string? f = tc switch
        {
            TypeCode.String => DataType.Text,
            TypeCode.Int16 => DataType.Int,
            TypeCode.UInt16 => DataType.Int,
            TypeCode.Int32 => DataType.Int,
            TypeCode.UInt32 => DataType.Int,
            TypeCode.Int64 => DataType.Int,
            TypeCode.UInt64 => DataType.Int,
            TypeCode.DateTime => DataType.Date,
            TypeCode.Boolean => DataType.Bool,
            TypeCode.Char => DataType.Text,
            TypeCode.SByte => DataType.Blob,
            TypeCode.Byte => DataType.Blob,
            TypeCode.Single => DataType.Number,
            TypeCode.Double => DataType.Number,
            TypeCode.Decimal => DataType.Number,
            _ => null,
        };

        if (f is not null)
        {
            return f;
        }

        if (tc == TypeCode.Object)
        {
            return DataType.Object;
        }

        throw new WeaviateClientException(
            new NotSupportedException($"Type {t.Name} not supported")
        );
    }

    internal static PropertyFactory ForType(Type t)
    {
        var dataType = DataTypeForType(t);

        return PropertyHelper.Factory(dataType);
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
}

public static class DataType
{
    public const string Text = "text";
    public const string TextArray = "text[]";
    public const string Int = "int";
    public const string IntArray = "int[]";
    public const string Bool = "boolean";
    public const string BoolArray = "boolean[]";
    public const string Number = "number";
    public const string NumberArray = "number[]";
    public const string Date = "date";
    public const string DateArray = "date[]";
    public const string Uuid = "uuid";
    public const string UuidArray = "uuid[]";
    public const string GeoCoordinate = "geoCoordinates";
    public const string Blob = "blob";
    public const string PhoneNumber = "phoneNumber";
    public const string Object = "object";
    public const string ObjectArray = "object[]";
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
    public bool? IndexFilterable { get; internal set; }

    [Obsolete]
    public bool? IndexInverted { get; internal set; }
    public bool? IndexRangeFilters { get; internal set; }
    public bool? IndexSearchable { get; internal set; }
    public PropertyTokenization? PropertyTokenization { get; internal set; }
    public Property[]? NestedProperties { get; internal set; }

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

    // Extract collection properties from type specified by TData, supporting nested properties up to maxDepth.
    public static Property[] FromClass<TData>(int maxDepth = 1)
    {
        return FromClass(typeof(TData), maxDepth);
    }

    public static Property[] FromClass(Type type, int maxDepth = 1)
    {
        string dataType = PropertyHelper.DataTypeForType(type);
        return FromClass(type, dataType, maxDepth, new Dictionary<Type, int>())
            ?? Array.Empty<Property>();
    }

    private static Property[]? FromClass(
        Type type,
        string dataType,
        int maxDepth,
        Dictionary<Type, int> seenTypes
    )
    {
        int currentDepth = seenTypes.TryGetValue(type, out int prevDepth) ? prevDepth + 1 : 0;

        if (maxDepth < 0 || currentDepth > maxDepth)
            return null;

        seenTypes[type] = currentDepth;

        if (type.IsArray || dataType == Models.DataType.ObjectArray)
        {
            type =
                type.GetElementType()
                ?? throw new WeaviateClientException("Can't get element type");
        }

        var props = type.GetProperties()
            .Where(x => x.CanRead && x.CanWrite)
            .Select(x =>
            {
                var dataTypeProp = PropertyHelper.DataTypeForType(x.PropertyType);

                Property[]? subProperties = null;

                if (
                    dataTypeProp == Models.DataType.Object
                    || dataTypeProp == Models.DataType.ObjectArray
                )
                {
                    subProperties = FromClass(
                        x.PropertyType,
                        dataTypeProp,
                        maxDepth,
                        new Dictionary<Type, int>(seenTypes)
                    );

                    if (subProperties == null || subProperties.Length == 0)
                    {
                        return null;
                    }
                }

                var factory = PropertyHelper.Factory(dataTypeProp);

                return factory(x.Name, subProperties: subProperties);
            })
            .Where(p => p != null)
            .Select(p => p!)
            .ToArray();

        return props;
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
        hash.Add(NestedProperties);
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
            && PropertyTokenization == other.PropertyTokenization
            && (
                (NestedProperties == null && other.NestedProperties == null)
                || (
                    NestedProperties != null
                    && other.NestedProperties != null
                    && NestedProperties.SequenceEqual(other.NestedProperties)
                )
            );
    }
}

namespace Weaviate.Client.Models;

/// <summary>
/// Text tokenization strategy for indexing and searching text properties.
/// </summary>
public enum PropertyTokenization
{
    /// <summary>
    /// Tokenizes on word boundaries (default for most text properties).
    /// </summary>
    Word = 0,

    /// <summary>
    /// Tokenizes and converts to lowercase for case-insensitive search.
    /// </summary>
    Lowercase = 1,

    /// <summary>
    /// Tokenizes only on whitespace, preserving punctuation and case.
    /// </summary>
    Whitespace = 2,

    /// <summary>
    /// Treats entire field as a single token (useful for exact matching).
    /// </summary>
    Field = 3,

    /// <summary>
    /// Tokenizes into overlapping trigrams (3-character sequences) for fuzzy matching.
    /// </summary>
    Trigram = 4,

    /// <summary>
    /// Chinese text tokenization using GSE (Go Simple Efficient) segmentation.
    /// </summary>
    Gse = 5,

    /// <summary>
    /// Korean text tokenization using Kagome.
    /// </summary>
    Kagome_kr = 6,

    /// <summary>
    /// Japanese text tokenization using Kagome.
    /// </summary>
    Kagome_ja = 7,

    /// <summary>
    /// Chinese text tokenization using GSE with Chinese-specific settings.
    /// </summary>
    Gse_ch = 8,
}

/// <summary>
/// Factory delegate for creating <see cref="Property"/> instances with fluent configuration.
/// </summary>
/// <param name="name">The name of the property.</param>
/// <param name="description">Optional description of the property's purpose.</param>
/// <param name="indexFilterable">Whether this property can be used in filters.</param>
/// <param name="indexRangeFilters">Whether range filters (greater than, less than) are enabled.</param>
/// <param name="indexSearchable">Whether this property is included in keyword search.</param>
/// <param name="tokenization">The tokenization strategy for text properties.</param>
/// <param name="subProperties">Nested properties for object/objectArray types.</param>
/// <returns>A configured <see cref="Property"/> instance.</returns>
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
    internal static PropertyFactory Factory(DataType dataType) =>
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
                DataType = dataType,
                Description = description,
                IndexFilterable = indexFilterable,
                IndexRangeFilters = indexRangeFilters,
                IndexSearchable = indexSearchable,
                PropertyTokenization = tokenization,
                NestedProperties =
                    (dataType == DataType.Object || dataType == DataType.ObjectArray)
                        ? subProperties
                        : null,
            };

    internal static DataType DataTypeForCollectionType(Type? elementType)
    {
        if (elementType == null)
            throw new WeaviateClientException("Element type cannot be null");

        // Handle special collection element types
        if (elementType == typeof(Guid))
        {
            return DataType.UuidArray;
        }

        if (elementType == typeof(String))
        {
            return DataType.TextArray;
        }

        // Enums serialize as text by default (EnumMember or name)
        if (elementType.IsEnum)
        {
            return DataType.TextArray;
        }

        var tc = Type.GetTypeCode(elementType);

        // Handle primitive collection element types
        DataType? f = tc switch
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

        if (f.HasValue)
            return f.Value;

        throw new WeaviateClientException(
            new NotSupportedException($"Collection element type {elementType.Name} not supported")
        );
    }

    internal static DataType DataTypeForType(Type t)
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

        // Enums serialize as text (EnumMember or name)
        if (actualType.IsEnum)
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
        DataType? f = tc switch
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

        if (f.HasValue)
        {
            return f.Value;
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

public enum DataType
{
    [System.Runtime.Serialization.EnumMember(Value = "unknown")]
    Unknown,

    [System.Runtime.Serialization.EnumMember(Value = "text")]
    Text,

    [System.Runtime.Serialization.EnumMember(Value = "text[]")]
    TextArray,

    [System.Runtime.Serialization.EnumMember(Value = "int")]
    Int,

    [System.Runtime.Serialization.EnumMember(Value = "int[]")]
    IntArray,

    [System.Runtime.Serialization.EnumMember(Value = "boolean")]
    Bool,

    [System.Runtime.Serialization.EnumMember(Value = "boolean[]")]
    BoolArray,

    [System.Runtime.Serialization.EnumMember(Value = "number")]
    Number,

    [System.Runtime.Serialization.EnumMember(Value = "number[]")]
    NumberArray,

    [System.Runtime.Serialization.EnumMember(Value = "date")]
    Date,

    [System.Runtime.Serialization.EnumMember(Value = "date[]")]
    DateArray,

    [System.Runtime.Serialization.EnumMember(Value = "uuid")]
    Uuid,

    [System.Runtime.Serialization.EnumMember(Value = "uuid[]")]
    UuidArray,

    [System.Runtime.Serialization.EnumMember(Value = "geoCoordinates")]
    GeoCoordinate,

    [System.Runtime.Serialization.EnumMember(Value = "blob")]
    Blob,

    [System.Runtime.Serialization.EnumMember(Value = "phoneNumber")]
    PhoneNumber,

    [System.Runtime.Serialization.EnumMember(Value = "object")]
    Object,

    [System.Runtime.Serialization.EnumMember(Value = "object[]")]
    ObjectArray,
}

internal static class DataTypeExtensions
{
    internal static string ToEnumMemberValue(this DataType dataType)
    {
        var type = dataType.GetType();
        var memberInfo = type.GetMember(dataType.ToString());
        var attributes = memberInfo[0]
            .GetCustomAttributes(typeof(System.Runtime.Serialization.EnumMemberAttribute), false);

        if (attributes.Length > 0)
        {
            var attribute = (System.Runtime.Serialization.EnumMemberAttribute)attributes[0];
            return attribute.Value ?? dataType.ToString();
        }

        return dataType.ToString();
    }

    internal static DataType? ToDataTypeEnum(this string value)
    {
        foreach (DataType dt in Enum.GetValues(typeof(DataType)))
        {
            if (dt.ToEnumMemberValue() == value)
                return dt;
        }
        return null;
    }
}

internal interface IReferenceBase
{
    string Name { get; }
    IList<string> TargetCollections { get; }
    string? Description { get; }
}

/// <summary>
/// Defines a cross-reference property that links to objects in another collection.
/// </summary>
/// <param name="Name">The name of the reference property.</param>
/// <param name="TargetCollection">The name of the collection this reference points to.</param>
/// <param name="Description">Optional description of the reference relationship.</param>
/// <remarks>
/// References enable relationships between objects in different collections, similar to foreign keys in relational databases.
/// Use <see cref="Property.Reference(string, string, string?)"/> to create reference properties.
/// </remarks>
public record Reference(string Name, string TargetCollection, string? Description = null)
    : IReferenceBase
{
    /// <summary>
    /// Gets or sets the description of this reference relationship.
    /// </summary>
    public string? Description { get; set; } = Description;

    IList<string> IReferenceBase.TargetCollections { get; } = [TargetCollection];
}

/// <summary>
/// Generic helper for creating type-safe property factories based on a C# type.
/// </summary>
/// <typeparam name="TField">The C# type to infer the Weaviate data type from.</typeparam>
public static class Property<TField>
{
    /// <summary>
    /// Gets a property factory that automatically infers the <see cref="DataType"/> from the generic type parameter.
    /// </summary>
    public static PropertyFactory New => PropertyHelper.ForType(typeof(TField));
}

/// <summary>
/// Defines a property (data field) in a Weaviate collection schema.
/// </summary>
/// <remarks>
/// Properties define the structure of data objects stored in a collection.
/// Each property has a data type, optional indexing configuration, and optional nested properties for object types.
/// Use the static factory methods (e.g., <see cref="Text"/>, <see cref="Int"/>, <see cref="Bool"/>)
/// or the generic <see cref="Property{TField}.New"/> for type-safe property creation.
/// </remarks>
/// <example>
/// <code>
/// var titleProp = Property.Text("title", description: "Article title", indexSearchable: true);
/// var ageProp = Property.Int("age", indexFilterable: true, indexRangeFilters: true);
/// var tagsProp = Property.TextArray("tags", indexFilterable: true);
/// </code>
/// </example>
public record Property : IEquatable<Property>
{
    private string _name = string.Empty;

    /// <summary>
    /// Gets or sets the name of the property. Names are automatically decapitalized for Weaviate compatibility.
    /// </summary>
    public required string Name
    {
        get => _name;
        set => _name = value.Decapitalize();
    }

    /// <summary>
    /// Gets the data type of this property (e.g., Text, Int, Bool, Object).
    /// </summary>
    public required DataType DataType { get; init; }

    /// <summary>
    /// Gets or sets an optional description explaining the property's purpose.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets or sets whether this property can be used in filter expressions. When null, uses Weaviate's default.
    /// </summary>
    public bool? IndexFilterable { get; init; }

    /// <summary>
    /// Obsolete. Use <see cref="IndexFilterable"/> instead.
    /// </summary>
    [Obsolete("Use IndexFilterable instead")]
    public bool? IndexInverted { get; init; }

    /// <summary>
    /// Gets or sets whether range filters (&gt;, &lt;, &gt;=, &lt;=) are enabled for this property. When null, uses Weaviate's default.
    /// </summary>
    public bool? IndexRangeFilters { get; init; }

    /// <summary>
    /// Gets or sets whether this property is included in BM25 keyword search. When null, uses Weaviate's default.
    /// </summary>
    public bool? IndexSearchable { get; init; }

    /// <summary>
    /// Gets or sets the tokenization strategy for text properties. When null, uses Weaviate's default tokenization.
    /// </summary>
    public PropertyTokenization? PropertyTokenization { get; init; }

    /// <summary>
    /// Gets or sets nested properties for Object and ObjectArray data types.
    /// Defines the schema for nested objects.
    /// </summary>
    public Property[]? NestedProperties { get; init; }

    /// <summary>
    /// Gets or sets whether to skip vectorization for this property's values. Defaults to false.
    /// When true, this property's data is not included in the object's vector representation.
    /// </summary>
    public bool SkipVectorization { get; init; } = false;

    /// <summary>
    /// Gets or sets whether to include the property name in vectorization. Defaults to true.
    /// When true, the property name itself is included in the vector representation for better context.
    /// </summary>
    public bool VectorizePropertyName { get; init; } = true;

    /// <summary>Gets a factory for creating text properties.</summary>
    public static PropertyFactory Text => PropertyHelper.Factory(DataType.Text);

    /// <summary>Gets a factory for creating text array properties.</summary>
    public static PropertyFactory TextArray => PropertyHelper.Factory(DataType.TextArray);

    /// <summary>Gets a factory for creating integer properties.</summary>
    public static PropertyFactory Int => PropertyHelper.Factory(DataType.Int);

    /// <summary>Gets a factory for creating integer array properties.</summary>
    public static PropertyFactory IntArray => PropertyHelper.Factory(DataType.IntArray);

    /// <summary>Gets a factory for creating boolean properties.</summary>
    public static PropertyFactory Bool => PropertyHelper.Factory(DataType.Bool);

    /// <summary>Gets a factory for creating boolean array properties.</summary>
    public static PropertyFactory BoolArray => PropertyHelper.Factory(DataType.BoolArray);

    /// <summary>Gets a factory for creating number (floating-point) properties.</summary>
    public static PropertyFactory Number => PropertyHelper.Factory(DataType.Number);

    /// <summary>Gets a factory for creating number array properties.</summary>
    public static PropertyFactory NumberArray => PropertyHelper.Factory(DataType.NumberArray);

    /// <summary>Gets a factory for creating date/time properties.</summary>
    public static PropertyFactory Date => PropertyHelper.Factory(DataType.Date);

    /// <summary>Gets a factory for creating date/time array properties.</summary>
    public static PropertyFactory DateArray => PropertyHelper.Factory(DataType.DateArray);

    /// <summary>Gets a factory for creating UUID properties.</summary>
    public static PropertyFactory Uuid => PropertyHelper.Factory(DataType.Uuid);

    /// <summary>Gets a factory for creating UUID array properties.</summary>
    public static PropertyFactory UuidArray => PropertyHelper.Factory(DataType.UuidArray);

    /// <summary>Gets a factory for creating geo-coordinate properties.</summary>
    public static PropertyFactory GeoCoordinate => PropertyHelper.Factory(DataType.GeoCoordinate);

    /// <summary>Gets a factory for creating blob (binary data) properties.</summary>
    public static PropertyFactory Blob => PropertyHelper.Factory(DataType.Blob);

    /// <summary>Gets a factory for creating phone number properties.</summary>
    public static PropertyFactory PhoneNumber => PropertyHelper.Factory(DataType.PhoneNumber);

    /// <summary>Gets a factory for creating nested object properties.</summary>
    public static PropertyFactory Object => PropertyHelper.Factory(DataType.Object);

    /// <summary>Gets a factory for creating object array properties.</summary>
    public static PropertyFactory ObjectArray => PropertyHelper.Factory(DataType.ObjectArray);

    /// <summary>
    /// Initializes a new instance of the <see cref="Property"/> record.
    /// </summary>
    public Property() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Property"/> record with the specified name and data type.
    /// </summary>
    /// <param name="name">The name of the property. Will be automatically decapitalized.</param>
    /// <param name="dataType">The data type of the property.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public Property(string name, DataType dataType)
    {
        Name = name;
        DataType = dataType;
    }

    /// <summary>
    /// Creates a cross-reference property that links to objects in another collection.
    /// </summary>
    /// <param name="name">The name of the reference property.</param>
    /// <param name="targetCollection">The name of the collection this reference points to.</param>
    /// <param name="description">Optional description of the reference relationship.</param>
    /// <returns>A <see cref="Reference"/> instance representing the cross-reference property.</returns>
    /// <example>
    /// <code>
    /// var authorRef = Property.Reference("author", "Author", "The author who wrote this article");
    /// var categoryRef = Property.Reference("category", "Category");
    /// </code>
    /// </example>
    public static Reference Reference(
        string name,
        string targetCollection,
        string? description = null
    ) => new(name, targetCollection, description);

    /// <summary>
    /// Extracts property definitions from a C# class using reflection.
    /// </summary>
    /// <typeparam name="TData">The C# type to extract properties from.</typeparam>
    /// <param name="maxDepth">Maximum depth for nested object properties. Default is 1.</param>
    /// <returns>An array of <see cref="Property"/> instances representing the class's properties.</returns>
    /// <remarks>
    /// This method uses reflection to automatically generate Weaviate property definitions from a C# class.
    /// Properties are mapped based on their .NET types to corresponding Weaviate data types.
    /// For nested objects, properties are extracted recursively up to the specified <paramref name="maxDepth"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class Article
    /// {
    ///     public string Title { get; set; }
    ///     public int Views { get; set; }
    ///     public DateTime PublishedAt { get; set; }
    /// }
    ///
    /// var properties = Property.FromClass&lt;Article&gt;();
    /// </code>
    /// </example>
    public static Property[] FromClass<TData>(int maxDepth = 1)
    {
        return FromClass(typeof(TData), maxDepth);
    }

    /// <summary>
    /// Extracts property definitions from a C# type using reflection.
    /// </summary>
    /// <param name="type">The C# type to extract properties from.</param>
    /// <param name="maxDepth">Maximum depth for nested object properties. Default is 1.</param>
    /// <returns>An array of <see cref="Property"/> instances representing the type's properties.</returns>
    /// <remarks>
    /// This method uses reflection to automatically generate Weaviate property definitions from a C# type.
    /// Properties are mapped based on their .NET types to corresponding Weaviate data types.
    /// For nested objects, properties are extracted recursively up to the specified <paramref name="maxDepth"/>.
    /// </remarks>
    public static Property[] FromClass(Type type, int maxDepth = 1)
    {
        DataType dataType = PropertyHelper.DataTypeForType(type);
        return FromClass(type, dataType, maxDepth, new Dictionary<Type, int>())
            ?? Array.Empty<Property>();
    }

    private static Property[]? FromClass(
        Type type,
        DataType dataType,
        int maxDepth,
        Dictionary<Type, int> seenTypes
    )
    {
        int currentDepth = seenTypes.TryGetValue(type, out int prevDepth) ? prevDepth + 1 : 0;

        if (maxDepth < 0 || currentDepth > maxDepth)
            return null;

        seenTypes[type] = currentDepth;

        if (type.IsArray || dataType == DataType.ObjectArray)
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

                if (dataTypeProp == DataType.Object || dataTypeProp == DataType.ObjectArray)
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
        hash.Add(SkipVectorization);
        hash.Add(VectorizePropertyName);
        return hash.ToHashCode();
    }

    public virtual bool Equals(Property? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Name == other.Name
            && DataType == other.DataType
            && Description == other.Description
            && IndexFilterable == other.IndexFilterable
            && IndexRangeFilters == other.IndexRangeFilters
            && IndexSearchable == other.IndexSearchable
            && PropertyTokenization == other.PropertyTokenization
            && SkipVectorization == other.SkipVectorization
            && VectorizePropertyName == other.VectorizePropertyName
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

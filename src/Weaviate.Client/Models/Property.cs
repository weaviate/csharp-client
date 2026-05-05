namespace Weaviate.Client.Models;

/// <summary>
/// Specifies which inverted index to drop from a collection property.
/// Used with DELETE /schema/{className}/properties/{propertyName}/index/{indexName}.
/// </summary>
public enum PropertyIndexType
{
    /// <summary>
    /// The filterable Roaring Bitmap index (used in <c>where</c> filters).
    /// </summary>
    Filterable,

    /// <summary>
    /// The searchable BM25 / full-text index.
    /// </summary>
    Searchable,

    /// <summary>
    /// The range-based Roaring Bitmap index (used for numeric/date range queries).
    /// </summary>
    RangeFilters,
}

/// <summary>
/// Extension methods for <see cref="PropertyIndexType"/>.
/// </summary>
internal static class PropertyIndexTypeExtensions
{
    /// <summary>
    /// Converts a <see cref="PropertyIndexType"/> to its generated <see cref="Rest.Dto.IndexName"/> counterpart,
    /// which carries the correct <c>[JsonStringEnumMemberName]</c> API string values.
    /// </summary>
    internal static Rest.Dto.IndexName ToDto(this PropertyIndexType indexType) =>
        indexType switch
        {
            PropertyIndexType.Filterable => Rest.Dto.IndexName.Filterable,
            PropertyIndexType.Searchable => Rest.Dto.IndexName.Searchable,
            PropertyIndexType.RangeFilters => Rest.Dto.IndexName.RangeFilters,
            _ => throw new ArgumentOutOfRangeException(nameof(indexType), indexType, null),
        };

    /// <summary>
    /// Converts a <see cref="PropertyIndexType"/> to its API path segment string,
    /// delegating to the generated <see cref="Rest.Dto.IndexName"/> enum for the canonical value.
    /// </summary>
    internal static string ToApiString(this PropertyIndexType indexType) =>
        indexType.ToDto().ToEnumMemberString();
}

/// <summary>
/// Specifies the tokenization strategy for a property.
/// </summary>
public enum PropertyTokenization
{
    /// <summary>
    /// Word tokenization.
    /// </summary>
    Word = 0,

    /// <summary>
    /// Lowercase tokenization.
    /// </summary>
    Lowercase = 1,

    /// <summary>
    /// Whitespace tokenization.
    /// </summary>
    Whitespace = 2,

    /// <summary>
    /// Field tokenization.
    /// </summary>
    Field = 3,

    /// <summary>
    /// Trigram tokenization.
    /// </summary>
    Trigram = 4,

    /// <summary>
    /// Gse tokenization.
    /// </summary>
    Gse = 5,

    /// <summary>
    /// Kagome Korean tokenization.
    /// </summary>
    Kagome_kr = 6,

    /// <summary>
    /// Kagome Japanese tokenization.
    /// </summary>
    Kagome_ja = 7,

    /// <summary>
    /// Gse Chinese tokenization.
    /// </summary>
    Gse_ch = 8,
}

/// <summary>
/// Delegate for creating a <see cref="Property"/> instance.
/// </summary>
/// <param name="name">The property name.</param>
/// <param name="description">The property description.</param>
/// <param name="indexFilterable">Whether the property is filterable.</param>
/// <param name="indexRangeFilters">Whether the property supports range filters.</param>
/// <param name="indexSearchable">Whether the property is searchable.</param>
/// <param name="tokenization">The tokenization strategy.</param>
/// <param name="subProperties">The sub-properties.</param>
/// <returns>A new <see cref="Property"/> instance.</returns>
public delegate Property PropertyFactory(
    string name,
    string? description = null,
    bool? indexFilterable = null,
    bool? indexRangeFilters = null,
    bool? indexSearchable = null,
    PropertyTokenization? tokenization = null,
    Property[]? subProperties = null
);

/// <summary>
/// The property helper class
/// </summary>
internal class PropertyHelper
{
    /// <summary>
    /// Factories the data type
    /// </summary>
    /// <param name="dataType">The data type</param>
    /// <returns>The property factory</returns>
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

    /// <summary>
    /// Datas the type for collection type using the specified element type
    /// </summary>
    /// <param name="elementType">The element type</param>
    /// <exception cref="WeaviateClientException"></exception>
    /// <exception cref="WeaviateClientException">Element type cannot be null</exception>
    /// <returns>The data type</returns>
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

    /// <summary>
    /// Datas the type for type using the specified t
    /// </summary>
    /// <param name="t">The </param>
    /// <exception cref="WeaviateClientException"></exception>
    /// <returns>The data type</returns>
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

    /// <summary>
    /// Fors the type using the specified t
    /// </summary>
    /// <param name="t">The </param>
    /// <returns>The property factory</returns>
    internal static PropertyFactory ForType(Type t)
    {
        var dataType = DataTypeForType(t);

        return PropertyHelper.Factory(dataType);
    }

    /// <summary>
    /// Ises the array or collection using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="elementType">The element type</param>
    /// <returns>The bool</returns>
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

/// <summary>
/// Specifies the data type of a property.
/// </summary>
public enum DataType
{
    /// <summary>Unknown data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("unknown")]
    Unknown,

    /// <summary>Text data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("text")]
    Text,

    /// <summary>Text array data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("text[]")]
    TextArray,

    /// <summary>Integer data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("int")]
    Int,

    /// <summary>Integer array data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("int[]")]
    IntArray,

    /// <summary>Boolean data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("boolean")]
    Bool,

    /// <summary>Boolean array data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("boolean[]")]
    BoolArray,

    /// <summary>Number data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("number")]
    Number,

    /// <summary>Number array data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("number[]")]
    NumberArray,

    /// <summary>Date data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("date")]
    Date,

    /// <summary>Date array data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("date[]")]
    DateArray,

    /// <summary>UUID data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("uuid")]
    Uuid,

    /// <summary>UUID array data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("uuid[]")]
    UuidArray,

    /// <summary>Geo coordinate data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("geoCoordinates")]
    GeoCoordinate,

    /// <summary>Blob data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("blob")]
    Blob,

    /// <summary>Phone number data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("phoneNumber")]
    PhoneNumber,

    /// <summary>Object data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("object")]
    Object,

    /// <summary>Object array data type.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("object[]")]
    ObjectArray,

    /// <summary>Blob hash data type. Stores the hash of a blob instead of the blob data itself. Introduced in Weaviate v1.37.</summary>
    [System.Text.Json.Serialization.JsonStringEnumMemberName("blobHash")]
    BlobHash,
}

/// <summary>
/// The data type extensions class
/// </summary>
internal static class DataTypeExtensions
{
    /// <summary>
    /// Returns the enum member value using the specified data type
    /// </summary>
    /// <param name="dataType">The data type</param>
    /// <returns>The string</returns>
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

    /// <summary>
    /// Returns the data type enum using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The data type</returns>
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

/// <summary>
/// The reference base interface
/// </summary>
internal interface IReferenceBase
{
    /// <summary>
    /// Gets the value of the name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the value of the target collections
    /// </summary>
    IList<string> TargetCollections { get; }

    /// <summary>
    /// Gets the value of the description
    /// </summary>
    string? Description { get; }
}

/// <summary>
/// Represents a reference to another collection.
/// </summary>
/// <param name="Name">The name of the reference.</param>
/// <param name="TargetCollection">The target collection name.</param>
/// <param name="Description">The description of the reference.</param>
public record Reference(string Name, string TargetCollection, string? Description = null)
    : IReferenceBase
{
    /// <summary>
    /// Gets or sets the description of the reference.
    /// </summary>
    public string? Description { get; set; } = Description;

    /// <summary>
    /// Gets the value of the target collections
    /// </summary>
    IList<string> IReferenceBase.TargetCollections { get; } = [TargetCollection];
}

/// <summary>
/// Provides a strongly-typed property factory for the specified field type.
/// </summary>
public static class Property<TField>
{
    /// <summary>
    /// Gets a property factory for the field type.
    /// </summary>
    public static PropertyFactory New => PropertyHelper.ForType(typeof(TField));
}

/// <summary>
/// Represents a property in a collection schema.
/// </summary>
public record Property : IEquatable<Property>
{
    /// <summary>
    /// The empty
    /// </summary>
    private string _name = string.Empty;

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public required string Name
    {
        get => _name;
        set => _name = value.Decapitalize();
    }

    /// <summary>
    /// Gets the data type of the property.
    /// </summary>
    public required DataType DataType { get; init; }

    /// <summary>
    /// Gets or sets the description of the property.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets whether the property is filterable in queries.
    /// </summary>
    public bool? IndexFilterable { get; init; }

    /// <summary>
    /// Gets whether inverted indexing is enabled for the property.
    /// </summary>
    [Obsolete]
    public bool? IndexInverted { get; init; }

    /// <summary>
    /// Gets whether range filter indexing is enabled for the property.
    /// </summary>
    public bool? IndexRangeFilters { get; init; }

    /// <summary>
    /// Gets whether the property is searchable.
    /// </summary>
    public bool? IndexSearchable { get; init; }

    /// <summary>
    /// Gets the tokenization strategy for the property.
    /// </summary>
    public PropertyTokenization? PropertyTokenization { get; init; }

    /// <summary>
    /// Gets the nested properties for object and object array types.
    /// </summary>
    public Property[]? NestedProperties { get; init; }

    /// <summary>
    /// Gets whether to skip vectorization for this property.
    /// </summary>
    public bool SkipVectorization { get; init; } = false;

    /// <summary>
    /// Gets whether to include the property name in vectorization.
    /// </summary>
    public bool VectorizePropertyName { get; init; } = true;

    /// <summary>
    /// Optional property-level text analyzer configuration. When set, the property's
    /// indexed and query tokens are post-processed according to the configured
    /// ASCII-folding and stopword preset. Requires Weaviate ≥ 1.37.0.
    /// </summary>
    public TextAnalyzerConfig? TextAnalyzer { get; init; }

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

    /// <summary>Gets a factory for creating number properties.</summary>
    public static PropertyFactory Number => PropertyHelper.Factory(DataType.Number);

    /// <summary>Gets a factory for creating number array properties.</summary>
    public static PropertyFactory NumberArray => PropertyHelper.Factory(DataType.NumberArray);

    /// <summary>Gets a factory for creating date properties.</summary>
    public static PropertyFactory Date => PropertyHelper.Factory(DataType.Date);

    /// <summary>Gets a factory for creating date array properties.</summary>
    public static PropertyFactory DateArray => PropertyHelper.Factory(DataType.DateArray);

    /// <summary>Gets a factory for creating UUID properties.</summary>
    public static PropertyFactory Uuid => PropertyHelper.Factory(DataType.Uuid);

    /// <summary>Gets a factory for creating UUID array properties.</summary>
    public static PropertyFactory UuidArray => PropertyHelper.Factory(DataType.UuidArray);

    /// <summary>Gets a factory for creating geo coordinate properties.</summary>
    public static PropertyFactory GeoCoordinate => PropertyHelper.Factory(DataType.GeoCoordinate);

    /// <summary>Gets a factory for creating blob properties.</summary>
    public static PropertyFactory Blob => PropertyHelper.Factory(DataType.Blob);

    /// <summary>Gets a factory for creating phone number properties.</summary>
    public static PropertyFactory PhoneNumber => PropertyHelper.Factory(DataType.PhoneNumber);

    /// <summary>Gets a factory for creating object properties.</summary>
    public static PropertyFactory Object => PropertyHelper.Factory(DataType.Object);

    /// <summary>Gets a factory for creating object array properties.</summary>
    public static PropertyFactory ObjectArray => PropertyHelper.Factory(DataType.ObjectArray);

    /// <summary>Gets a factory for creating blob hash properties.</summary>
    public static PropertyFactory BlobHash => PropertyHelper.Factory(DataType.BlobHash);

    /// <summary>
    /// Initializes a new instance of the <see cref="Property"/> class.
    /// </summary>
    public Property() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Property"/> class with specified name and data type.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="dataType">The data type of the property.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public Property(string name, DataType dataType)
    {
        Name = name;
        DataType = dataType;
    }

    /// <summary>
    /// Creates a reference to another collection.
    /// </summary>
    /// <param name="name">The name of the reference.</param>
    /// <param name="targetCollection">The target collection name.</param>
    /// <param name="description">Optional description of the reference.</param>
    /// <returns>A new <see cref="Reference"/> instance.</returns>
    public static Reference Reference(
        string name,
        string targetCollection,
        string? description = null
    ) => new(name, targetCollection, description);

    /// <summary>
    /// Extracts collection properties from the specified type, supporting nested properties up to maxDepth.
    /// </summary>
    /// <typeparam name="TData">The type to extract properties from.</typeparam>
    /// <param name="maxDepth">Maximum depth for nested properties (default is 1).</param>
    /// <returns>An array of properties extracted from the type.</returns>
    public static Property[] FromClass<TData>(int maxDepth = 1)
    {
        return FromClass(typeof(TData), maxDepth);
    }

    /// <summary>
    /// Extracts collection properties from the specified type, supporting nested properties up to maxDepth.
    /// </summary>
    /// <param name="type">The type to extract properties from.</param>
    /// <param name="maxDepth">Maximum depth for nested properties (default is 1).</param>
    /// <returns>An array of properties extracted from the type.</returns>
    public static Property[] FromClass(Type type, int maxDepth = 1)
    {
        DataType dataType = PropertyHelper.DataTypeForType(type);
        return FromClass(type, dataType, maxDepth, new Dictionary<Type, int>())
            ?? Array.Empty<Property>();
    }

    /// <summary>
    /// Creates the type
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="dataType">The data type</param>
    /// <param name="maxDepth">The max depth</param>
    /// <param name="seenTypes">The seen types</param>
    /// <exception cref="WeaviateClientException">Can't get element type</exception>
    /// <returns>The props</returns>
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

    /// <summary>
    /// Returns a hash code for this property based on its configuration.
    /// </summary>
    /// <returns>A hash code for the current property.</returns>
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
        hash.Add(TextAnalyzer);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether the specified property is equal to the current property.
    /// </summary>
    /// <param name="other">The property to compare with the current property.</param>
    /// <returns>True if the specified property is equal to the current property; otherwise, false.</returns>
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
            && EqualityComparer<TextAnalyzerConfig?>.Default.Equals(
                TextAnalyzer,
                other.TextAnalyzer
            )
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

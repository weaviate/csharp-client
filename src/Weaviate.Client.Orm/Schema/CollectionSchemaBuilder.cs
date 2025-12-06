using System.Reflection;
using Weaviate.Client.Models;
using Weaviate.Client.Orm.Attributes;
using Weaviate.Client.Orm.Internal;

namespace Weaviate.Client.Orm.Schema;

/// <summary>
/// Builds a <see cref="CollectionConfig"/> from a C# class decorated with ORM attributes.
/// </summary>
public static class CollectionSchemaBuilder
{
    /// <summary>
    /// Creates a <see cref="CollectionConfig"/> from a C# class with ORM attributes.
    /// </summary>
    /// <typeparam name="T">The class type representing the collection.</typeparam>
    /// <returns>A fully configured <see cref="CollectionConfig"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required attributes are missing or invalid.</exception>
    public static CollectionConfig FromClass<T>()
        where T : class
    {
        var type = typeof(T);

        // Get collection-level attributes
        var collectionAttr = type.GetCustomAttribute<WeaviateCollectionAttribute>();
        var invertedIndexAttr = type.GetCustomAttribute<InvertedIndexAttribute>();

#pragma warning disable CS8601 // Possible null reference assignment.
        // Build the config
        var config = new CollectionConfig
        {
            Name = collectionAttr?.Name ?? type.Name,
            Description = collectionAttr?.Description,
            Properties = BuildProperties(type),
            References = BuildReferences(type),
            VectorConfig = VectorConfigBuilder.BuildVectorConfigs(type),
            InvertedIndexConfig =
                BuildInvertedIndexConfig(invertedIndexAttr) ?? new InvertedIndexConfig(),
            ReplicationConfig = new ReplicationConfig(), // Explicitly initialize
            MultiTenancyConfig = new MultiTenancyConfig(), // Explicitly initialize
        };
#pragma warning restore CS8601 // Possible null reference assignment.

        return config;
    }

    /// <summary>
    /// Builds the property array from class properties with [Property] attributes.
    /// </summary>
    private static Property[] BuildProperties(Type type)
    {
        var properties = new List<Property>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip properties with vector or reference attributes
            if (IsVectorProperty(prop) || IsReferenceProperty(prop))
                continue;

            var propAttr = prop.GetCustomAttribute<PropertyAttribute>();
            if (propAttr == null)
                continue;

            var property = BuildProperty(prop, propAttr);
            if (property != null)
                properties.Add(property);
        }

        return properties.ToArray();
    }

    /// <summary>
    /// Builds a single Property from a PropertyInfo and PropertyAttribute.
    /// </summary>
    private static Property? BuildProperty(PropertyInfo prop, PropertyAttribute propAttr)
    {
        var propertyName = PropertyHelper.ToCamelCase(prop.Name);
        var indexAttr = prop.GetCustomAttribute<IndexAttribute>();
        var tokenAttr = prop.GetCustomAttribute<TokenizationAttribute>();
        var nestedAttr = prop.GetCustomAttribute<NestedTypeAttribute>();

        return propAttr.DataType switch
        {
            DataType.Text => Property.Text(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable,
                indexSearchable: indexAttr?.Searchable,
                tokenization: tokenAttr?.Tokenization
            ),

            DataType.TextArray => Property.TextArray(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable,
                indexSearchable: indexAttr?.Searchable,
                tokenization: tokenAttr?.Tokenization
            ),

            DataType.Int => Property.Int(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable,
                indexRangeFilters: indexAttr?.RangeFilters
            ),

            DataType.IntArray => Property.IntArray(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.Number => Property.Number(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable,
                indexRangeFilters: indexAttr?.RangeFilters
            ),

            DataType.NumberArray => Property.NumberArray(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.Bool => Property.Bool(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.BoolArray => Property.BoolArray(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.Date => Property.Date(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.DateArray => Property.DateArray(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.Uuid => Property.Uuid(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.UuidArray => Property.UuidArray(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.GeoCoordinate => Property.GeoCoordinate(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.Blob => Property.Blob(propertyName, description: propAttr.Description),

            DataType.PhoneNumber => Property.PhoneNumber(
                propertyName,
                description: propAttr.Description,
                indexFilterable: indexAttr?.Filterable
            ),

            DataType.Object => nestedAttr != null
                ? Property.Object(
                    propertyName,
                    description: propAttr.Description,
                    subProperties: BuildProperties(nestedAttr.NestedType)
                )
                : throw new InvalidOperationException(
                    $"Property '{prop.Name}' with DataType.Object must have a [NestedType] attribute"
                ),

            DataType.ObjectArray => nestedAttr != null
                ? Property.ObjectArray(
                    propertyName,
                    description: propAttr.Description,
                    subProperties: BuildProperties(nestedAttr.NestedType)
                )
                : throw new InvalidOperationException(
                    $"Property '{prop.Name}' with DataType.ObjectArray must have a [NestedType] attribute"
                ),

            _ => throw new NotSupportedException($"DataType {propAttr.DataType} is not supported"),
        };
    }

    /// <summary>
    /// Builds the reference array from properties with [Reference] attributes.
    /// </summary>
    private static Reference[] BuildReferences(Type type)
    {
        var references = new List<Reference>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var refAttr = prop.GetCustomAttribute<ReferenceAttribute>();
            if (refAttr == null)
                continue;

            var propertyName = PropertyHelper.ToCamelCase(prop.Name);

            references.Add(
                new Reference(
                    Name: propertyName,
                    TargetCollection: refAttr.TargetCollection,
                    Description: refAttr.Description
                )
            );
        }

        return references.ToArray();
    }

    /// <summary>
    /// Builds inverted index configuration from attribute.
    /// </summary>
    private static InvertedIndexConfig? BuildInvertedIndexConfig(InvertedIndexAttribute? attr)
    {
        if (attr == null)
            return null;

        return new InvertedIndexConfig
        {
            CleanupIntervalSeconds = attr.CleanupIntervalSeconds,
            IndexNullState = attr.IndexNullState,
            IndexPropertyLength = attr.IndexPropertyLength,
            IndexTimestamps = attr.IndexTimestamps,
        };
    }

    /// <summary>
    /// Checks if a property is a vector property (has VectorAttribute).
    /// </summary>
    private static bool IsVectorProperty(PropertyInfo prop)
    {
        return prop.GetCustomAttributes()
            .Any(a =>
                a.GetType().IsGenericType
                && a.GetType().GetGenericTypeDefinition() == typeof(VectorAttribute<>)
            );
    }

    /// <summary>
    /// Checks if a property is a reference property (has ReferenceAttribute).
    /// </summary>
    private static bool IsReferenceProperty(PropertyInfo prop)
    {
        return prop.GetCustomAttribute<ReferenceAttribute>() != null;
    }
}

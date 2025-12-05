using System.Reflection;
using Weaviate.Client.Models;
using Weaviate.Client.Orm.Attributes;
using Weaviate.Client.Orm.Internal;

namespace Weaviate.Client.Orm.Schema;

/// <summary>
/// Builds vector configurations from properties decorated with VectorAttribute&lt;T&gt;.
/// </summary>
internal static class VectorConfigBuilder
{
    /// <summary>
    /// Builds all vector configurations from a type's properties.
    /// </summary>
    /// <param name="type">The class type to scan for vector properties.</param>
    /// <returns>A VectorConfigList with all configured vectors, or null if none found.</returns>
    public static VectorConfigList? BuildVectorConfigs(Type type)
    {
        var configs = new List<VectorConfig>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var vectorAttr = GetVectorAttribute(prop);
            if (vectorAttr == null)
                continue;

            var config = BuildVectorConfig(prop, vectorAttr);
            if (config != null)
                configs.Add(config);
        }

        return configs.Count > 0 ? new VectorConfigList(configs.ToArray()) : null;
    }

    /// <summary>
    /// Builds a single VectorConfig from a property and its VectorAttribute.
    /// </summary>
    private static VectorConfig? BuildVectorConfig(
        PropertyInfo prop,
        VectorAttributeBase vectorAttr
    )
    {
        // Vector name comes from property name
        var vectorName = PropertyHelper.ToCamelCase(prop.Name);

        // Create the vectorizer instance
        var vectorizer = CreateVectorizer(vectorAttr);
        if (vectorizer == null)
            return null;

        // TODO: Build vector index config from VectorIndexAttribute (if present)
        // For now, we'll use default index config

        return new VectorConfig(
            name: vectorName,
            vectorizer: vectorizer,
            vectorIndexConfig: null // Will be enhanced in future
        );
    }

    /// <summary>
    /// Creates and configures a vectorizer from a VectorAttribute.
    /// </summary>
    private static VectorizerConfig? CreateVectorizer(VectorAttributeBase attr)
    {
        var vectorizerType = attr.VectorizerType;

        // Create instance of vectorizer
        var vectorizer = Activator.CreateInstance(vectorizerType) as VectorizerConfig;
        if (vectorizer == null)
            throw new InvalidOperationException(
                $"Failed to create vectorizer of type {vectorizerType.Name}"
            );

        // Map common properties
        MapCommonProperties(attr, vectorizer);

        // Map type-specific properties using reflection
        MapVectorizerSpecificProperties(attr, vectorizer);

        return vectorizer;
    }

    /// <summary>
    /// Maps common properties that all vectorizers support.
    /// </summary>
    private static void MapCommonProperties(VectorAttributeBase attr, VectorizerConfig vectorizer)
    {
        // SourceProperties
        if (attr.SourceProperties != null && attr.SourceProperties.Length > 0)
        {
            vectorizer.SourceProperties = attr
                .SourceProperties.Select(PropertyHelper.ToCamelCase)
                .ToList();
        }

        // VectorizeCollectionName
        if (attr.VectorizeCollectionName)
        {
            // Try to set this property if it exists on the vectorizer
            var vecProp = vectorizer.GetType().GetProperty("VectorizeCollectionName");
            if (vecProp != null && vecProp.CanWrite)
            {
                vecProp.SetValue(vectorizer, true);
            }
        }
    }

    /// <summary>
    /// Maps vectorizer-specific properties from attribute to vectorizer instance.
    /// Uses reflection to handle different vectorizer types dynamically.
    /// </summary>
    private static void MapVectorizerSpecificProperties(
        VectorAttributeBase attr,
        VectorizerConfig vectorizer
    )
    {
        // Get all properties from the attribute
        var attrProperties = attr.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var attrProp in attrProperties)
        {
            // Skip properties we've already handled or that are from the base class
            if (attrProp.DeclaringType == typeof(VectorAttributeBase))
                continue;

            var attrValue = attrProp.GetValue(attr);
            if (attrValue == null)
                continue;

            // Try to find a matching property on the vectorizer
            var vectorizerProp = vectorizer
                .GetType()
                .GetProperty(attrProp.Name, BindingFlags.Public | BindingFlags.Instance);

            if (vectorizerProp != null && vectorizerProp.CanWrite)
            {
                try
                {
                    // Handle type conversion if needed
                    var convertedValue = ConvertValue(attrValue, vectorizerProp.PropertyType);
                    vectorizerProp.SetValue(vectorizer, convertedValue);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to set property '{attrProp.Name}' on vectorizer '{vectorizer.GetType().Name}': {ex.Message}",
                        ex
                    );
                }
            }
        }
    }

    /// <summary>
    /// Converts a value to the target type, handling special cases.
    /// </summary>
    private static object? ConvertValue(object value, Type targetType)
    {
        if (value == null)
            return null;

        var sourceType = value.GetType();

        // If types match, no conversion needed
        if (targetType.IsAssignableFrom(sourceType))
            return value;

        // Handle nullable types
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType)!;
            return ConvertValue(value, underlyingType);
        }

        // Handle string arrays to ICollection<string>
        if (
            value is string[] stringArray
            && targetType.IsGenericType
            && targetType.GetGenericTypeDefinition() == typeof(ICollection<>)
        )
        {
            return stringArray.ToList();
        }

        // Try standard conversion
        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            // If conversion fails, return the original value and let the caller handle it
            return value;
        }
    }

    /// <summary>
    /// Gets the VectorAttribute from a property, if it exists.
    /// </summary>
    private static VectorAttributeBase? GetVectorAttribute(PropertyInfo prop)
    {
        return prop.GetCustomAttributes()
                .FirstOrDefault(a =>
                    a.GetType().IsGenericType
                    && a.GetType().GetGenericTypeDefinition() == typeof(VectorAttribute<>)
                ) as VectorAttributeBase;
    }
}

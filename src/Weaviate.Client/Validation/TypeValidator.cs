using System.Reflection;
using Weaviate.Client.Models;
using Weaviate.Client.Serialization;

namespace Weaviate.Client.Validation;

/// <summary>
/// Validates C# types against Weaviate collection schemas to ensure type compatibility.
/// </summary>
public class TypeValidator
{
    private readonly PropertyConverterRegistry _registry;

    /// <summary>
    /// Default shared instance using the default PropertyConverterRegistry.
    /// </summary>
    public static TypeValidator Default { get; } =
        new TypeValidator(PropertyConverterRegistry.Default);

    /// <summary>
    /// Creates a new TypeValidator with the specified PropertyConverterRegistry.
    /// </summary>
    public TypeValidator(PropertyConverterRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Validates that a C# type is compatible with a collection's schema.
    /// </summary>
    /// <typeparam name="T">The C# type to validate.</typeparam>
    /// <param name="schema">The collection schema from the server.</param>
    /// <returns>A validation result containing any errors and warnings.</returns>
    public ValidationResult ValidateType<T>(CollectionConfig schema) =>
        ValidateType(typeof(T), schema);

    /// <summary>
    /// Validates that a C# type is compatible with a collection's schema.
    /// </summary>
    /// <param name="type">The C# type to validate.</param>
    /// <param name="schema">The collection schema from the server.</param>
    /// <returns>A validation result containing any errors and warnings.</returns>
    public ValidationResult ValidateType(Type type, CollectionConfig schema)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        // If type is anonymous, only get readable properties; otherwise, get readable and writable
        var isAnonymousType =
            type.IsGenericType
            && type.Name.Contains("AnonymousType")
            && (
                type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase)
                || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase)
            )
            && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;

        var typeProperties = isAnonymousType
            ? type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList()
            : type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToList();

        // Validate each C# property against schema
        foreach (var prop in typeProperties)
        {
            ValidateProperty(prop, schema.Properties, errors, warnings);
        }

        // Check for schema properties that aren't in the C# type
        // (might indicate missing required properties)
        foreach (var schemaProp in schema.Properties ?? Array.Empty<Property>())
        {
            var propName = schemaProp.Name;
            var matchingCsProp = typeProperties.FirstOrDefault(p =>
                string.Equals(ToCamelCase(p.Name), propName, StringComparison.OrdinalIgnoreCase)
            );

            if (matchingCsProp == null)
            {
                // Schema has a property not in C# type - this might be okay if it's optional
                warnings.Add(
                    new ValidationWarning
                    {
                        PropertyName = propName,
                        Message =
                            $"Schema property '{propName}' not found in C# type '{type.Name}'. "
                            + "This property will not be populated when deserializing.",
                        WarningType = ValidationWarningType.ExtraProperty,
                    }
                );
            }
        }

        return ValidationResult.WithIssues(errors, warnings);
    }

    private void ValidateProperty(
        PropertyInfo csProperty,
        IList<Property>? schemaProperties,
        List<ValidationError> errors,
        List<ValidationWarning> warnings
    )
    {
        var propName = ToCamelCase(csProperty.Name);

        // Find matching schema property (case-insensitive)
        var schemaProp = schemaProperties?.FirstOrDefault(p =>
            string.Equals(p.Name, propName, StringComparison.OrdinalIgnoreCase)
        );

        if (schemaProp == null)
        {
            // C# property not in schema - will be ignored during serialization
            warnings.Add(
                new ValidationWarning
                {
                    PropertyName = csProperty.Name,
                    Message =
                        $"Property '{csProperty.Name}' exists in C# type but not in schema. "
                        + "It will be ignored during serialization.",
                    WarningType = ValidationWarningType.ExtraProperty,
                }
            );
            return;
        }

        // Get expected Weaviate data type for this C# property
        string expectedDataType;
        try
        {
            expectedDataType = PropertyHelper.DataTypeForType(csProperty.PropertyType);
        }
        catch (WeaviateClientException)
        {
            errors.Add(
                new ValidationError
                {
                    PropertyName = csProperty.Name,
                    Message =
                        $"Property '{csProperty.Name}' has unsupported type '{csProperty.PropertyType.Name}'.",
                    ErrorType = ValidationErrorType.UnsupportedType,
                    ActualType = csProperty.PropertyType.FullName,
                }
            );
            return;
        }

        // Schema DataType is a list (can have multiple types for cross-references)
        // For data properties, typically just one type
        if (schemaProp.DataType == null || schemaProp.DataType.Count == 0)
        {
            warnings.Add(
                new ValidationWarning
                {
                    PropertyName = csProperty.Name,
                    Message = $"Schema property '{schemaProp.Name}' has no data type defined.",
                    WarningType = ValidationWarningType.CompatibleTypeDifference,
                }
            );
            return;
        }

        var schemaDataType = schemaProp.DataType[0]; // Primary type

        // Compare expected vs actual data types
        if (!AreTypesCompatible(expectedDataType, schemaDataType))
        {
            // Check if it's an array mismatch specifically
            var isArrayMismatch =
                (expectedDataType.EndsWith("[]") && !schemaDataType.EndsWith("[]"))
                || (!expectedDataType.EndsWith("[]") && schemaDataType.EndsWith("[]"));

            errors.Add(
                new ValidationError
                {
                    PropertyName = csProperty.Name,
                    Message =
                        $"Property '{csProperty.Name}' type mismatch: "
                        + $"expected '{schemaDataType}' but C# type maps to '{expectedDataType}'.",
                    ErrorType = isArrayMismatch
                        ? ValidationErrorType.ArrayMismatch
                        : ValidationErrorType.TypeMismatch,
                    ExpectedType = schemaDataType,
                    ActualType = expectedDataType,
                }
            );
        }

        // Validate nested objects recursively
        if (expectedDataType == DataType.Object || expectedDataType == DataType.ObjectArray)
        {
            ValidateNestedObject(
                csProperty,
                schemaProp,
                expectedDataType == DataType.ObjectArray,
                errors,
                warnings
            );
        }
    }

    private void ValidateNestedObject(
        PropertyInfo csProperty,
        Property schemaProp,
        bool isArray,
        List<ValidationError> errors,
        List<ValidationWarning> warnings
    )
    {
        // Get the element type (unwrap array/collection if needed)
        var elementType = isArray
            ? GetCollectionElementType(csProperty.PropertyType)
            : csProperty.PropertyType;

        if (elementType == null)
        {
            errors.Add(
                new ValidationError
                {
                    PropertyName = csProperty.Name,
                    Message =
                        $"Property '{csProperty.Name}' is an array but element type could not be determined.",
                    ErrorType = ValidationErrorType.NestedObjectMismatch,
                }
            );
            return;
        }

        // If schema has nested properties defined, validate them recursively
        if (schemaProp.NestedProperties != null && schemaProp.NestedProperties.Length > 0)
        {
            var nestedProps = elementType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var nestedProp in nestedProps)
            {
                ValidateProperty(nestedProp, schemaProp.NestedProperties, errors, warnings);
            }
        }
    }

    private bool AreTypesCompatible(string csDataType, string schemaDataType)
    {
        // Exact match
        if (string.Equals(csDataType, schemaDataType, StringComparison.OrdinalIgnoreCase))
            return true;

        // Some compatible differences:
        // - int/number can sometimes be interchangeable depending on the data
        // - For now, we'll be strict and require exact matches

        return false;
    }

    private Type? GetCollectionElementType(Type type)
    {
        // Handle arrays
        if (type.IsArray)
            return type.GetElementType();

        // Handle generic collections (List<T>, IEnumerable<T>, etc.)
        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 1)
                return genericArgs[0];
        }

        return null;
    }

    private string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}

using System.Reflection;
using Weaviate.Client.CollectionMapper.Attributes;
using Weaviate.Client.CollectionMapper.Internal;
using Weaviate.Client.Models;
using PropertyHelper = Weaviate.Client.CollectionMapper.Internal.PropertyHelper;

namespace Weaviate.Client.CollectionMapper.Schema;

/// <summary>
/// Builds a <see cref="CollectionCreateParams"/> from a C# class decorated with ORM attributes.
/// </summary>
public static class CollectionSchemaBuilder
{
    /// <summary>
    /// Creates a <see cref="CollectionCreateParams"/> from a C# class with ORM attributes.
    /// </summary>
    /// <typeparam name="T">The class type representing the collection.</typeparam>
    /// <returns>A fully configured <see cref="CollectionCreateParams"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required attributes are missing or invalid.</exception>
    public static CollectionCreateParams FromClass<T>()
        where T : class
    {
        var type = typeof(T);

        // Get collection-level attributes
        var collectionAttr = type.GetCustomAttribute<WeaviateCollectionAttribute>();
        var invertedIndexAttr = type.GetCustomAttribute<InvertedIndexAttribute>();
        var generativeAttr = GetGenerativeAttribute(type);
        var rerankerAttr = GetRerankerAttribute(type);

#pragma warning disable CS8601 // Possible null reference assignment.
        // Build the config
        var config = new CollectionCreateParams
        {
            Name = collectionAttr?.Name ?? type.Name,
            Description = collectionAttr?.Description,
            Properties = BuildProperties(type),
            References = BuildReferences(type),
            VectorConfig = VectorConfigBuilder.BuildVectorConfigs(type),
            InvertedIndexConfig =
                BuildInvertedIndexConfig(invertedIndexAttr) ?? new InvertedIndexConfig(),
            ReplicationConfig = BuildReplicationConfig(collectionAttr),
            MultiTenancyConfig = BuildMultiTenancyConfig(collectionAttr),
            ShardingConfig = BuildShardingConfig(collectionAttr),
            GenerativeConfig = BuildGenerativeConfig(generativeAttr, type),
            RerankerConfig = BuildRerankerConfig(rerankerAttr, type),
        };
#pragma warning restore CS8601 // Possible null reference assignment.

        // Apply CollectionConfigMethod if specified
        if (
            collectionAttr?.CollectionConfigMethod != null
            && !string.IsNullOrWhiteSpace(collectionAttr.CollectionConfigMethod)
        )
        {
            config = InvokeCollectionConfigMethod(
                collectionAttr.CollectionConfigMethod,
                type,
                config,
                collectionAttr.ConfigMethodClass
            );
        }

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
        // Use custom name from attribute, or convert C# property name to camelCase
        var propertyName = propAttr.Name ?? PropertyHelper.ToCamelCase(prop.Name);
        var indexAttr = prop.GetCustomAttribute<IndexAttribute>();
        var tokenAttr = prop.GetCustomAttribute<TokenizationAttribute>();
        var nestedAttr = prop.GetCustomAttribute<NestedTypeAttribute>();

        // Infer DataType from property type if not explicitly specified
        var dataType =
            propAttr.DataType == DataType.Unknown
                ? InferDataType(prop.PropertyType)
                : propAttr.DataType;

        return dataType switch
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

            DataType.Object => Property.Object(
                propertyName,
                description: propAttr.Description,
                subProperties: BuildProperties(GetNestedType(prop, nestedAttr))
            ),

            DataType.ObjectArray => Property.ObjectArray(
                propertyName,
                description: propAttr.Description,
                subProperties: BuildProperties(GetNestedType(prop, nestedAttr))
            ),

            _ => throw new NotSupportedException($"DataType {dataType} is not supported"),
        };
    }

    /// <summary>
    /// Gets the nested type for an Object or ObjectArray property.
    /// First checks for [NestedType] attribute (for backward compatibility),
    /// then infers from the property type.
    /// </summary>
    private static Type GetNestedType(PropertyInfo prop, NestedTypeAttribute? nestedAttr)
    {
        // If [NestedType] is explicitly specified, use it (backward compatibility)
        if (nestedAttr != null)
            return nestedAttr.NestedType;

        // Infer from property type
        var propType = prop.PropertyType;

        // Handle List<T>, IList<T>, IEnumerable<T>, etc. for ObjectArray
        if (propType.IsGenericType)
        {
            var genericTypeDef = propType.GetGenericTypeDefinition();
            if (
                genericTypeDef == typeof(List<>)
                || genericTypeDef == typeof(IList<>)
                || genericTypeDef == typeof(IEnumerable<>)
                || genericTypeDef == typeof(ICollection<>)
            )
            {
                return propType.GetGenericArguments()[0];
            }
        }

        // For non-generic (single Object), use the property type directly
        return propType;
    }

    /// <summary>
    /// Infers the Weaviate DataType from a C# property type.
    /// </summary>
    /// <param name="propertyType">The C# property type to infer from.</param>
    /// <returns>The inferred DataType.</returns>
    /// <exception cref="NotSupportedException">Thrown when the type cannot be mapped to a Weaviate DataType.</exception>
    private static DataType InferDataType(Type propertyType)
    {
        // Handle nullable types by extracting the underlying type
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        // Check for array/collection types
        if (propertyType.IsArray || IsGenericCollection(propertyType))
        {
            var elementType = propertyType.IsArray
                ? propertyType.GetElementType()!
                : propertyType.GetGenericArguments()[0];

            // Unwrap nullable element types
            elementType = Nullable.GetUnderlyingType(elementType) ?? elementType;

            return elementType.Name switch
            {
                nameof(String) => DataType.TextArray,
                nameof(Int32) or nameof(Int64) => DataType.IntArray,
                nameof(Boolean) => DataType.BoolArray,
                nameof(Double) or nameof(Single) or nameof(Decimal) => DataType.NumberArray,
                nameof(DateTime) => DataType.DateArray,
                nameof(Guid) => DataType.UuidArray,
                _ when elementType.IsClass => DataType.ObjectArray,
                _ => throw new NotSupportedException(
                    $"Cannot infer Weaviate DataType for array/collection of type {elementType.Name}. "
                        + "Please specify the DataType explicitly in the [Property] attribute."
                ),
            };
        }

        // Handle single-value types
        return underlyingType.Name switch
        {
            nameof(String) => DataType.Text,
            nameof(Int32) or nameof(Int64) => DataType.Int,
            nameof(Boolean) => DataType.Bool,
            nameof(Double) or nameof(Single) or nameof(Decimal) => DataType.Number,
            nameof(DateTime) => DataType.Date,
            nameof(Guid) => DataType.Uuid,
            nameof(Byte) when propertyType.IsArray => DataType.Blob,
            _ when underlyingType == typeof(GeoCoordinate)
                    || underlyingType.Name == nameof(GeoCoordinate) => DataType.GeoCoordinate,
            _ when underlyingType == typeof(PhoneNumber)
                    || underlyingType.Name == nameof(PhoneNumber) => DataType.PhoneNumber,
            _ when underlyingType.IsClass => DataType.Object,
            _ => throw new NotSupportedException(
                $"Cannot infer Weaviate DataType for C# type {underlyingType.Name}. "
                    + "Please specify the DataType explicitly in the [Property] attribute."
            ),
        };
    }

    /// <summary>
    /// Checks if a type is a generic collection (List, IList, IEnumerable, ICollection).
    /// </summary>
    private static bool IsGenericCollection(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericTypeDef = type.GetGenericTypeDefinition();
        return genericTypeDef == typeof(List<>)
            || genericTypeDef == typeof(IList<>)
            || genericTypeDef == typeof(IEnumerable<>)
            || genericTypeDef == typeof(ICollection<>);
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
    /// Builds multi-tenancy configuration from collection attribute.
    /// </summary>
    private static MultiTenancyConfig BuildMultiTenancyConfig(
        WeaviateCollectionAttribute? collectionAttr
    )
    {
        if (collectionAttr == null)
            return new MultiTenancyConfig { Enabled = false };

        // If MultiTenancyEnabled is not set, default to false
        var enabled = collectionAttr.MultiTenancyEnabled ?? false;

        return new MultiTenancyConfig
        {
            Enabled = enabled,
            AutoTenantCreation = collectionAttr.AutoTenantCreation ?? false,
            AutoTenantActivation = collectionAttr.AutoTenantActivation ?? false,
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

    /// <summary>
    /// Gets the GenerativeAttributeBase from a class type.
    /// </summary>
    private static GenerativeAttributeBase? GetGenerativeAttribute(Type type)
    {
        return type.GetCustomAttributes().OfType<GenerativeAttributeBase>().FirstOrDefault();
    }

    /// <summary>
    /// Gets the RerankerAttributeBase from a class type.
    /// </summary>
    private static RerankerAttributeBase? GetRerankerAttribute(Type type)
    {
        return type.GetCustomAttributes().OfType<RerankerAttributeBase>().FirstOrDefault();
    }

    /// <summary>
    /// Builds sharding configuration from collection attribute.
    /// </summary>
    private static ShardingConfig? BuildShardingConfig(WeaviateCollectionAttribute? collectionAttr)
    {
        if (collectionAttr == null)
            return null;

        // Check if any sharding properties are set (sentinel value is -1)
        if (
            collectionAttr.ShardingDesiredCount == -1
            && collectionAttr.ShardingVirtualPerPhysical == -1
            && collectionAttr.ShardingDesiredVirtualCount == -1
            && collectionAttr.ShardingKey == null
        )
        {
            return null; // Use Weaviate defaults
        }

        var config = new ShardingConfig();

        if (collectionAttr.ShardingDesiredCount != -1)
            config.DesiredCount = collectionAttr.ShardingDesiredCount;

        if (collectionAttr.ShardingVirtualPerPhysical != -1)
            config.VirtualPerPhysical = collectionAttr.ShardingVirtualPerPhysical;

        if (collectionAttr.ShardingDesiredVirtualCount != -1)
            config.DesiredVirtualCount = collectionAttr.ShardingDesiredVirtualCount;

        if (collectionAttr.ShardingKey != null)
            config.Key = collectionAttr.ShardingKey;

        return config;
    }

    /// <summary>
    /// Builds replication configuration from collection attribute.
    /// </summary>
    private static ReplicationConfig BuildReplicationConfig(
        WeaviateCollectionAttribute? collectionAttr
    )
    {
        var config = new ReplicationConfig();

        if (collectionAttr == null)
            return config;

        if (collectionAttr.ReplicationFactor != -1)
            config.Factor = collectionAttr.ReplicationFactor;

        config.AsyncEnabled = collectionAttr.ReplicationAsyncEnabled;

        return config;
    }

    /// <summary>
    /// Builds generative configuration from attribute.
    /// </summary>
    private static IGenerativeConfig? BuildGenerativeConfig(
        GenerativeAttributeBase? attr,
        Type declaringType
    )
    {
        if (attr == null)
            return null;

        var moduleType = attr.ModuleType;

        // Create instance using parameterless constructor or record syntax
        var generativeConfig = Activator.CreateInstance(moduleType) as IGenerativeConfig;
        if (generativeConfig == null)
        {
            throw new InvalidOperationException(
                $"Could not create instance of generative module type '{moduleType.FullName}'. "
                    + "Ensure the type has a parameterless constructor."
            );
        }

        // Use reflection to set properties from attribute
        var attrType = attr.GetType();
        foreach (
            var attrProp in attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
        )
        {
            if (attrProp.Name == nameof(GenerativeAttributeBase.ModuleType))
                continue;
            if (attrProp.Name == "ConfigMethod" || attrProp.Name == "ConfigMethodClass")
                continue;

            var value = attrProp.GetValue(attr);
            if (value == null)
                continue;

            // Skip sentinel values (-1 for int/double, -999 for special cases)
            if (value is int intValue && (intValue == -1 || intValue == -999))
                continue;
            if (value is double doubleValue && doubleValue == -1)
                continue;

            // Find matching property in generative config
            var configProp = moduleType.GetProperty(attrProp.Name);
            if (configProp != null && configProp.CanWrite)
            {
                configProp.SetValue(generativeConfig, value);
            }
        }

        // Apply ConfigMethod if specified
        var configMethodProp = attrType.GetProperty("ConfigMethod");
        if (configMethodProp != null)
        {
            var configMethodValue = configMethodProp.GetValue(attr) as string;
            if (!string.IsNullOrWhiteSpace(configMethodValue))
            {
                var configMethodClassProp = attrType.GetProperty("ConfigMethodClass");
                var configMethodClass = configMethodClassProp?.GetValue(attr) as Type;

                generativeConfig = InvokeGenerativeConfigMethod(
                    configMethodValue,
                    declaringType,
                    generativeConfig,
                    configMethodClass
                );
            }
        }

        return generativeConfig;
    }

    /// <summary>
    /// Builds reranker configuration from attribute.
    /// </summary>
    private static IRerankerConfig? BuildRerankerConfig(
        RerankerAttributeBase? attr,
        Type declaringType
    )
    {
        if (attr == null)
            return null;

        var moduleType = attr.ModuleType;

        // Create instance
        var rerankerConfig = Activator.CreateInstance(moduleType) as IRerankerConfig;
        if (rerankerConfig == null)
        {
            throw new InvalidOperationException(
                $"Could not create instance of reranker module type '{moduleType.FullName}'. "
                    + "Ensure the type has a parameterless constructor."
            );
        }

        // Use reflection to set properties from attribute
        var attrType = attr.GetType();
        foreach (
            var attrProp in attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
        )
        {
            if (attrProp.Name == nameof(RerankerAttributeBase.ModuleType))
                continue;
            if (attrProp.Name == "ConfigMethod" || attrProp.Name == "ConfigMethodClass")
                continue;

            var value = attrProp.GetValue(attr);
            if (value == null)
                continue;

            // Skip sentinel values
            if (value is int intValue && (intValue == -1 || intValue == -999))
                continue;

            // Find matching property in reranker config
            var configProp = moduleType.GetProperty(attrProp.Name);
            if (configProp != null && configProp.CanWrite)
            {
                configProp.SetValue(rerankerConfig, value);
            }
        }

        // Apply ConfigMethod if specified
        var configMethodProp = attrType.GetProperty("ConfigMethod");
        if (configMethodProp != null)
        {
            var configMethodValue = configMethodProp.GetValue(attr) as string;
            if (!string.IsNullOrWhiteSpace(configMethodValue))
            {
                var configMethodClassProp = attrType.GetProperty("ConfigMethodClass");
                var configMethodClass = configMethodClassProp?.GetValue(attr) as Type;

                rerankerConfig = InvokeRerankerConfigMethod(
                    configMethodValue,
                    declaringType,
                    rerankerConfig,
                    configMethodClass
                );
            }
        }

        return rerankerConfig;
    }

    /// <summary>
    /// Invokes a generative config method with the signature: static TModule MethodName(TModule prebuilt)
    /// </summary>
    private static IGenerativeConfig InvokeGenerativeConfigMethod(
        string methodName,
        Type declaringType,
        IGenerativeConfig prebuiltConfig,
        Type? configMethodClass = null
    )
    {
        Type targetType;
        string actualMethodName;

        // If ConfigMethodClass is provided, use it (type-safe approach)
        if (configMethodClass != null)
        {
            targetType = configMethodClass;
            actualMethodName = methodName;
        }
        // Legacy support: Parse method name - can be "MethodName" or "ClassName.MethodName"
        else if (methodName.Contains('.'))
        {
            var parts = methodName.Split('.');
            var className = string.Join(".", parts.Take(parts.Length - 1));
            actualMethodName = parts.Last();

            // Try to find the class in the same namespace or assembly
            targetType =
                declaringType.Assembly.GetType(className)
                ?? declaringType
                    .Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == className || t.FullName == className)
                ?? Type.GetType(className)
                ?? throw new InvalidOperationException(
                    $"Could not find type '{className}' for generative ConfigMethod."
                );
        }
        else
        {
            // Method in same class
            targetType = declaringType;
            actualMethodName = methodName;
        }

        var configType = prebuiltConfig.GetType();
        var method = targetType.GetMethod(
            actualMethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { configType },
            null
        );

        if (method == null)
        {
            throw new InvalidOperationException(
                $"Could not find static method '{actualMethodName}' in type '{targetType.FullName}' "
                    + $"with signature: static {configType.Name} {actualMethodName}({configType.Name} prebuilt)"
            );
        }

        // Validate return type
        if (method.ReturnType != configType)
        {
            throw new InvalidOperationException(
                $"ConfigMethod '{actualMethodName}' has invalid signature. "
                    + $"Expected return type '{configType.Name}', got '{method.ReturnType.Name}'."
            );
        }

        var result = method.Invoke(null, new object[] { prebuiltConfig });
        return (IGenerativeConfig)result!; // Non-null because we validated the signature
    }

    /// <summary>
    /// Invokes a reranker config method with the signature: static TModule MethodName(TModule prebuilt)
    /// </summary>
    private static IRerankerConfig InvokeRerankerConfigMethod(
        string methodName,
        Type declaringType,
        IRerankerConfig prebuiltConfig,
        Type? configMethodClass = null
    )
    {
        Type targetType;
        string actualMethodName;

        // If ConfigMethodClass is provided, use it (type-safe approach)
        if (configMethodClass != null)
        {
            targetType = configMethodClass;
            actualMethodName = methodName;
        }
        // Legacy support: Parse method name - can be "MethodName" or "ClassName.MethodName"
        else if (methodName.Contains('.'))
        {
            var parts = methodName.Split('.');
            var className = string.Join(".", parts.Take(parts.Length - 1));
            actualMethodName = parts.Last();

            // Try to find the class
            targetType =
                declaringType.Assembly.GetType(className)
                ?? declaringType
                    .Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == className || t.FullName == className)
                ?? Type.GetType(className)
                ?? throw new InvalidOperationException(
                    $"Could not find type '{className}' for reranker ConfigMethod."
                );
        }
        else
        {
            // Method in same class
            targetType = declaringType;
            actualMethodName = methodName;
        }

        var configType = prebuiltConfig.GetType();
        var method = targetType.GetMethod(
            actualMethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { configType },
            null
        );

        if (method == null)
        {
            throw new InvalidOperationException(
                $"Could not find static method '{actualMethodName}' in type '{targetType.FullName}' "
                    + $"with signature: static {configType.Name} {actualMethodName}({configType.Name} prebuilt)"
            );
        }

        // Validate return type
        if (method.ReturnType != configType)
        {
            throw new InvalidOperationException(
                $"ConfigMethod '{actualMethodName}' has invalid signature. "
                    + $"Expected return type '{configType.Name}', got '{method.ReturnType.Name}'."
            );
        }

        var result = method.Invoke(null, new object[] { prebuiltConfig });
        return (IRerankerConfig)result!; // Non-null because we validated the signature
    }

    /// <summary>
    /// Invokes a collection config method with the signature: static CollectionCreateParams MethodName(CollectionCreateParams prebuilt)
    /// </summary>
    private static CollectionCreateParams InvokeCollectionConfigMethod(
        string methodName,
        Type declaringType,
        CollectionCreateParams prebuiltConfig,
        Type? configMethodClass = null
    )
    {
        Type targetType;
        string actualMethodName;

        // If ConfigMethodClass is provided, use it (type-safe approach)
        if (configMethodClass != null)
        {
            targetType = configMethodClass;
            actualMethodName = methodName;
        }
        // Legacy support: Parse method name - can be "MethodName" or "ClassName.MethodName"
        else if (methodName.Contains('.'))
        {
            var parts = methodName.Split('.');
            var className = string.Join(".", parts.Take(parts.Length - 1));
            actualMethodName = parts.Last();

            // Try to find the class
            targetType =
                declaringType.Assembly.GetType(className)
                ?? declaringType
                    .Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == className || t.FullName == className)
                ?? Type.GetType(className)
                ?? throw new InvalidOperationException(
                    $"Could not find type '{className}' for CollectionConfigMethod."
                );
        }
        else
        {
            // Method in same class
            targetType = declaringType;
            actualMethodName = methodName;
        }

        var method = targetType.GetMethod(
            actualMethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { typeof(CollectionCreateParams) },
            null
        );

        if (method == null)
        {
            throw new InvalidOperationException(
                $"Could not find static method '{actualMethodName}' in type '{targetType.FullName}' "
                    + $"with signature: static CollectionCreateParams {actualMethodName}(CollectionCreateParams prebuilt)"
            );
        }

        // Validate return type
        if (method.ReturnType != typeof(CollectionCreateParams))
        {
            throw new InvalidOperationException(
                $"CollectionConfigMethod '{actualMethodName}' has invalid signature. "
                    + $"Expected return type 'CollectionCreateParams', got '{method.ReturnType.Name}'."
            );
        }

        var result = method.Invoke(null, new object[] { prebuiltConfig });
        return (CollectionCreateParams)result!; // Non-null because we validated the signature
    }
}

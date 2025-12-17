using System.Reflection;
using Weaviate.Client.CollectionMapper.Attributes;
using Weaviate.Client.CollectionMapper.Internal;
using Weaviate.Client.Models;
using PropertyHelper = Weaviate.Client.CollectionMapper.Internal.PropertyHelper;

namespace Weaviate.Client.CollectionMapper.Schema;

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
        // Vector name comes from Name property if specified, otherwise from property name
        var vectorName = !string.IsNullOrWhiteSpace(vectorAttr.Name)
            ? vectorAttr.Name
            : PropertyHelper.ToCamelCase(prop.Name);

        // Create the vectorizer instance
        var vectorizer = CreateVectorizer(vectorAttr, prop.DeclaringType!, vectorName);
        if (vectorizer == null)
            return null;

        // Build vector index config from VectorIndex attribute on the property
        var vectorIndexConfig = BuildVectorIndexConfig(prop);

        return new VectorConfig(
            name: vectorName,
            vectorizer: vectorizer,
            vectorIndexConfig: vectorIndexConfig
        );
    }

    /// <summary>
    /// Creates and configures a vectorizer from a VectorAttribute.
    /// </summary>
    private static VectorizerConfig? CreateVectorizer(
        VectorAttributeBase attr,
        Type declaringType,
        string vectorName
    )
    {
        var vectorizerType = attr.VectorizerType;

        // Validate SelfProvided doesn't have configuration properties
        ValidateSelfProvidedConfiguration(attr, vectorizerType);

        // Create instance of vectorizer
        var vectorizer =
            Activator.CreateInstance(vectorizerType, nonPublic: true) as VectorizerConfig;
        if (vectorizer == null)
            throw new InvalidOperationException(
                $"Failed to create vectorizer of type {vectorizerType.Name}"
            );

        // Map common properties
        MapCommonProperties(attr, vectorizer);

        // Map type-specific properties using reflection
        MapVectorizerSpecificProperties(attr, vectorizer);

        // Invoke ConfigMethod if specified (check using reflection since we can't cast to the generic type)
        var configMethodProp = attr.GetType().GetProperty("ConfigMethod");
        if (configMethodProp != null)
        {
            var configMethodValue = configMethodProp.GetValue(attr) as string;
            if (!string.IsNullOrWhiteSpace(configMethodValue))
            {
                // Check for ConfigMethodClass property
                var configMethodClassProp = attr.GetType().GetProperty("ConfigMethodClass");
                var configMethodClass = configMethodClassProp?.GetValue(attr) as Type;

                vectorizer = InvokeConfigMethod(
                    configMethodValue,
                    declaringType,
                    vectorName,
                    vectorizer,
                    configMethodClass
                );
            }
        }

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
    /// Builds VectorIndexConfig from VectorIndex attribute on the property.
    /// </summary>
    private static VectorIndexConfig? BuildVectorIndexConfig(PropertyInfo prop)
    {
        // Get VectorIndex attribute
        var vectorIndexAttr = GetVectorIndexAttribute(prop);
        if (vectorIndexAttr == null)
            return null; // Use Weaviate defaults

        var indexType = vectorIndexAttr.IndexConfigType;

        // Build appropriate config based on index type
        if (indexType == typeof(VectorIndex.HNSW))
        {
            return BuildHnswIndexConfig(vectorIndexAttr, prop);
        }
        else if (indexType == typeof(VectorIndex.Flat))
        {
            return BuildFlatIndexConfig(vectorIndexAttr, prop);
        }
        else if (indexType == typeof(VectorIndex.Dynamic))
        {
            return BuildDynamicIndexConfig(vectorIndexAttr, prop);
        }

        return null;
    }

    /// <summary>
    /// Builds HNSW index configuration from VectorIndex attribute.
    /// </summary>
    private static VectorIndex.HNSW BuildHnswIndexConfig(
        VectorIndexAttributeBase indexAttr,
        PropertyInfo prop
    )
    {
        var attrType = indexAttr.GetType();
        var config = new VectorIndex.HNSW();

        // Set distance if specified
        if (indexAttr.Distance != null)
        {
            config.Distance = ConvertDistance(indexAttr.Distance.Value);
        }

        // HNSW parameters
        SetIfNotNull(
            config,
            GetPropertyValue<int?>(attrType, indexAttr, "EfConstruction"),
            v => config.EfConstruction = v
        );
        SetIfNotNull(config, GetPropertyValue<int?>(attrType, indexAttr, "Ef"), v => config.Ef = v);
        SetIfNotNull(
            config,
            GetPropertyValue<int?>(attrType, indexAttr, "MaxConnections"),
            v => config.MaxConnections = v
        );
        SetIfNotNull(
            config,
            GetPropertyValue<int?>(attrType, indexAttr, "DynamicEfMin"),
            v => config.DynamicEfMin = v
        );
        SetIfNotNull(
            config,
            GetPropertyValue<int?>(attrType, indexAttr, "DynamicEfMax"),
            v => config.DynamicEfMax = v
        );
        SetIfNotNull(
            config,
            GetPropertyValue<int?>(attrType, indexAttr, "DynamicEfFactor"),
            v => config.DynamicEfFactor = v
        );
        SetIfNotNull(
            config,
            GetPropertyValue<int?>(attrType, indexAttr, "FlatSearchCutoff"),
            v => config.FlatSearchCutoff = v
        );
        SetIfNotNull(
            config,
            GetPropertyValue<long?>(attrType, indexAttr, "VectorCacheMaxObjects"),
            v => config.VectorCacheMaxObjects = v
        );

        // Quantizer
        config.Quantizer = BuildQuantizer(prop);

        // Multi-vector config (encoding)
        config.MultiVector = BuildMultiVectorConfig(prop);

        return config;
    }

    /// <summary>
    /// Builds Flat index configuration from VectorIndex attribute.
    /// </summary>
    private static VectorIndex.Flat BuildFlatIndexConfig(
        VectorIndexAttributeBase indexAttr,
        PropertyInfo prop
    )
    {
        var attrType = indexAttr.GetType();
        var config = new VectorIndex.Flat();

        if (indexAttr.Distance != null)
        {
            config.Distance = ConvertDistance(indexAttr.Distance.Value);
        }

        SetIfNotNull(
            config,
            GetPropertyValue<long?>(attrType, indexAttr, "VectorCacheMaxObjects"),
            v => config.VectorCacheMaxObjects = v
        );

        // Quantizer (Flat only supports BQ and RQ)
        var quantizer = BuildQuantizer(prop);
        if (quantizer is VectorIndexConfig.QuantizerConfigFlat flatQuantizer)
        {
            config.Quantizer = flatQuantizer;
        }

        // Note: Flat index does not support MultiVector configuration

        return config;
    }

    /// <summary>
    /// Builds Dynamic index configuration from VectorIndex attribute.
    /// </summary>
    private static VectorIndex.Dynamic BuildDynamicIndexConfig(
        VectorIndexAttributeBase indexAttr,
        PropertyInfo prop
    )
    {
        var attrType = indexAttr.GetType();
        var config = new VectorIndex.Dynamic
        {
            Hnsw = BuildHnswIndexConfig(indexAttr, prop),
            Flat = BuildFlatIndexConfig(indexAttr, prop),
        };

        if (indexAttr.Distance != null)
        {
            config.Distance = ConvertDistance(indexAttr.Distance.Value);
        }

        SetIfNotNull(
            config,
            GetPropertyValue<int?>(attrType, indexAttr, "Threshold"),
            v => config.Threshold = v
        );

        return config;
    }

    /// <summary>
    /// Builds quantizer configuration from Quantizer attribute on the property.
    /// </summary>
    private static VectorIndexConfig.QuantizerConfigBase? BuildQuantizer(PropertyInfo prop)
    {
        var quantizerAttr = GetQuantizerAttribute(prop);
        if (quantizerAttr == null)
            return null;

        return quantizerAttr switch
        {
            QuantizerBQ bq => BuildBQQuantizer(bq),
            QuantizerPQ pq => BuildPQQuantizer(pq),
            QuantizerSQ sq => BuildSQQuantizer(sq),
            QuantizerRQ rq => BuildRQQuantizer(rq),
            _ => null,
        };
    }

    private static VectorIndex.Quantizers.BQ BuildBQQuantizer(QuantizerBQ attr)
    {
        var config = new VectorIndex.Quantizers.BQ();
        SetIfNotNull(config, attr.RescoreLimit, v => config.RescoreLimit = v);
        SetIfNotNull(config, attr.Cache, v => config.Cache = v);
        return config;
    }

    private static VectorIndex.Quantizers.RQ BuildRQQuantizer(QuantizerRQ attr)
    {
        var config = new VectorIndex.Quantizers.RQ();
        SetIfNotNull(config, attr.RescoreLimit, v => config.RescoreLimit = v);
        SetIfNotNull(config, attr.Bits, v => config.Bits = v);
        SetIfNotNull(config, attr.Cache, v => config.Cache = v);
        return config;
    }

    private static VectorIndex.Quantizers.SQ BuildSQQuantizer(QuantizerSQ attr)
    {
        var config = new VectorIndex.Quantizers.SQ();
        SetIfNotNull(config, attr.RescoreLimit, v => config.RescoreLimit = v);
        SetIfNotNull(config, attr.TrainingLimit, v => config.TrainingLimit = v);
        return config;
    }

    private static VectorIndex.Quantizers.PQ BuildPQQuantizer(QuantizerPQ attr)
    {
        var config = new VectorIndex.Quantizers.PQ();
        SetIfNotNull(config, attr.Segments, v => config.Segments = v);
        SetIfNotNull(config, attr.Centroids, v => config.Centroids = v);
        SetIfNotNull(config, attr.BitCompression, v => config.BitCompression = v);
        SetIfNotNull(config, attr.TrainingLimit, v => config.TrainingLimit = v);
        // Note: PQ does not have RescoreLimit property (only BQ, RQ, and SQ have it)

        // Build encoder config from PQ attribute properties
        config.Encoder = new VectorIndex.Quantizers.PQ.EncoderConfig
        {
            Type = ConvertEncoderType(attr.EncoderType),
            Distribution = ConvertEncoderDistribution(attr.EncoderDistribution),
        };

        return config;
    }

    /// <summary>
    /// Builds multi-vector configuration from Encoding attribute on the property.
    /// </summary>
    private static VectorIndexConfig.MultiVectorConfig? BuildMultiVectorConfig(PropertyInfo prop)
    {
        var encodingAttr = prop.GetCustomAttribute<EncodingAttribute>();
        if (encodingAttr == null)
            return null;

        // MuveraEncoding properties are init-only, must use object initializer
        var encoding = new VectorIndexConfig.MuveraEncoding
        {
            KSim = encodingAttr.KSim,
            DProjections = encodingAttr.DProjections,
            Repetitions = encodingAttr.Repetitions,
        };

        return new VectorIndexConfig.MultiVectorConfig { Encoding = encoding };
    }

    /// <summary>
    /// Converts ORM distance enum to Weaviate distance enum.
    /// </summary>
    private static VectorIndexConfig.VectorDistance ConvertDistance(VectorDistance distance)
    {
        return distance switch
        {
            VectorDistance.Cosine => VectorIndexConfig.VectorDistance.Cosine,
            VectorDistance.Dot => VectorIndexConfig.VectorDistance.Dot,
            VectorDistance.L2Squared => VectorIndexConfig.VectorDistance.L2Squared,
            VectorDistance.Hamming => VectorIndexConfig.VectorDistance.Hamming,
            _ => VectorIndexConfig.VectorDistance.Cosine, // Default
        };
    }

    /// <summary>
    /// Converts ORM encoder type to Weaviate encoder type.
    /// </summary>
    private static VectorIndex.Quantizers.EncoderType ConvertEncoderType(PQEncoderType encoderType)
    {
        return encoderType switch
        {
            PQEncoderType.Kmeans => VectorIndex.Quantizers.EncoderType.Kmeans,
            PQEncoderType.Tile => VectorIndex.Quantizers.EncoderType.Tile,
            _ => VectorIndex.Quantizers.EncoderType.Tile,
        };
    }

    /// <summary>
    /// Converts ORM encoder distribution to Weaviate encoder distribution.
    /// </summary>
    private static VectorIndex.Quantizers.DistributionType ConvertEncoderDistribution(
        PQEncoderDistribution distribution
    )
    {
        return distribution switch
        {
            PQEncoderDistribution.LogNormal => VectorIndex.Quantizers.DistributionType.LogNormal,
            PQEncoderDistribution.Normal => VectorIndex.Quantizers.DistributionType.Normal,
            _ => VectorIndex.Quantizers.DistributionType.LogNormal,
        };
    }

    /// <summary>
    /// Gets the VectorIndex attribute from a property, if it exists.
    /// </summary>
    private static VectorIndexAttributeBase? GetVectorIndexAttribute(PropertyInfo prop)
    {
        return prop.GetCustomAttributes()
                .FirstOrDefault(a =>
                    a.GetType().IsGenericType
                    && a.GetType().GetGenericTypeDefinition() == typeof(VectorIndexAttribute<>)
                ) as VectorIndexAttributeBase;
    }

    /// <summary>
    /// Gets the Quantizer attribute from a property, if it exists.
    /// </summary>
    private static QuantizerAttribute? GetQuantizerAttribute(PropertyInfo prop)
    {
        return prop.GetCustomAttribute<QuantizerAttribute>();
    }

    /// <summary>
    /// Gets property value using reflection.
    /// </summary>
    private static T? GetPropertyValue<T>(Type attrType, object attr, string propertyName)
    {
        var prop = attrType.GetProperty(propertyName);
        if (prop == null)
            return default;

        var value = prop.GetValue(attr);
        if (value == null)
            return default;

        return (T)value;
    }

    /// <summary>
    /// Sets property value if not null.
    /// </summary>
    private static void SetIfNotNull<T>(object target, T? value, Action<T> setter)
        where T : struct
    {
        if (value.HasValue)
        {
            setter(value.Value);
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

    /// <summary>
    /// Validates that SelfProvided vectorizer doesn't have configuration properties set.
    /// </summary>
    private static void ValidateSelfProvidedConfiguration(
        VectorAttributeBase attr,
        Type vectorizerType
    )
    {
        // Check if this is SelfProvided vectorizer
        if (vectorizerType.Name != "SelfProvided")
            return;

        // SelfProvided should not have any configuration properties
        var hasConfig =
            attr.SourceProperties != null && attr.SourceProperties.Length > 0
            || attr.VectorizeCollectionName;

        // Check vectorizer-specific properties if it's a VectorAttribute<T>
        if (attr.GetType().IsGenericType)
        {
            var attrType = attr.GetType();
            var modelProp = attrType.GetProperty("Model");
            var dimensionsProp = attrType.GetProperty("Dimensions");
            var baseUrlProp = attrType.GetProperty("BaseURL");

            hasConfig =
                hasConfig
                || (modelProp?.GetValue(attr) != null)
                || (dimensionsProp?.GetValue(attr) != null)
                || (baseUrlProp?.GetValue(attr) != null);
        }

        if (hasConfig)
        {
            throw new InvalidOperationException(
                $"SelfProvided vectorizer should not have Model, Dimensions, BaseURL, "
                    + $"SourceProperties, or VectorizeCollectionName configured. "
                    + $"These properties are ignored for self-provided vectors."
            );
        }
    }

    /// <summary>
    /// Invokes a custom configuration method on the vectorizer.
    /// </summary>
    /// <param name="methodName">Method name (e.g., "ConfigureVector" or "ClassName.MethodName" for legacy support)</param>
    /// <param name="declaringType">The type that declares the property with the vector attribute</param>
    /// <param name="vectorName">The vector name</param>
    /// <param name="prebuiltVectorizer">The pre-built vectorizer with attribute properties already set</param>
    /// <param name="configMethodClass">Optional type containing the method (if null, uses declaringType)</param>
    /// <returns>The configured vectorizer</returns>
    private static VectorizerConfig InvokeConfigMethod(
        string methodName,
        Type declaringType,
        string vectorName,
        VectorizerConfig prebuiltVectorizer,
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
            if (parts.Length != 2)
            {
                throw new InvalidOperationException(
                    $"Invalid ConfigMethod format: '{methodName}'. "
                        + $"Expected 'MethodName' or 'ClassName.MethodName'"
                );
            }

            var className = parts[0];
            actualMethodName = parts[1];

            // Try to find the class in the same assembly as the declaring type
            targetType =
                declaringType.Assembly.GetType(className)
                ?? declaringType.Assembly.GetType($"{declaringType.Namespace}.{className}")
                ?? throw new InvalidOperationException(
                    $"Could not find type '{className}' for ConfigMethod '{methodName}'"
                );
        }
        else
        {
            // Method in same class
            targetType = declaringType;
            actualMethodName = methodName;
        }

        // Find the method - must be static
        var method = targetType.GetMethod(
            actualMethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
        );

        if (method == null)
        {
            throw new InvalidOperationException(
                $"Could not find static method '{actualMethodName}' on type '{targetType.Name}' "
                    + $"for ConfigMethod '{methodName}'"
            );
        }

        // Validate method signature: static TVectorizer MethodName(string vectorName, TVectorizer prebuilt)
        var parameters = method.GetParameters();
        if (
            parameters.Length != 2
            || parameters[0].ParameterType != typeof(string)
            || !parameters[1].ParameterType.IsAssignableFrom(prebuiltVectorizer.GetType())
        )
        {
            throw new InvalidOperationException(
                $"ConfigMethod '{methodName}' has invalid signature. "
                    + $"Expected: static {prebuiltVectorizer.GetType().Name} {actualMethodName}(string vectorName, {prebuiltVectorizer.GetType().Name} prebuilt)"
            );
        }

        if (
            method.ReturnType == typeof(void)
            || !method.ReturnType.IsAssignableFrom(prebuiltVectorizer.GetType())
        )
        {
            throw new InvalidOperationException(
                $"ConfigMethod '{methodName}' must return {prebuiltVectorizer.GetType().Name}"
            );
        }

        // Invoke the method
        try
        {
            var result = method.Invoke(null, new object[] { vectorName, prebuiltVectorizer });
            if (result is VectorizerConfig configuredVectorizer)
            {
                return configuredVectorizer;
            }

            throw new InvalidOperationException(
                $"ConfigMethod '{methodName}' returned null or invalid type"
            );
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                $"ConfigMethod '{methodName}' threw an exception: {ex.InnerException?.Message}",
                ex.InnerException
            );
        }
    }
}

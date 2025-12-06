# Vectorizer Configuration Enhancement Proposal

**Date:** 2025-12-06
**Status:** Proposed
**Goal:** Achieve 100% feature parity with manual `CollectionConfig` creation

---

## 1. Named Vector Support ✅

### Problem
Currently, vector names always come from property names. No way to specify custom vector names for existing collections.

### Solution
Add `Name` property to `VectorAttribute<TVectorizer>`:

```csharp
public abstract class VectorAttributeBase : Attribute
{
    public abstract Type VectorizerType { get; }

    /// <summary>
    /// Gets or sets the vector name in the collection schema.
    /// If not specified, the property name (converted to camelCase) will be used.
    /// </summary>
    public string? Name { get; set; }

    public string[]? SourceProperties { get; set; }
    public bool VectorizeCollectionName { get; set; }
}
```

### Usage Examples

```csharp
// Default: vector name = "contentEmbedding" (from property name)
[Vector<Vectorizer.Text2VecOpenAI>(Model = "text-embedding-3-small")]
public float[]? ContentEmbedding { get; set; }

// Custom: vector name = "legacy_content_vector" (for existing collections)
[Vector<Vectorizer.Text2VecOpenAI>(
    Name = "legacy_content_vector",
    Model = "text-embedding-3-small")]
public float[]? ContentEmbedding { get; set; }

// Working with existing collection that has different naming
public class Article
{
    // Existing collection has vectors: "title_vec", "content_vec"
    [Vector<Vectorizer.Text2VecOpenAI>(Name = "title_vec")]
    public float[]? TitleVector { get; set; }

    [Vector<Vectorizer.Text2VecOpenAI>(Name = "content_vec")]
    public float[]? ContentVector { get; set; }
}
```

### Implementation

Update `VectorConfigBuilder.BuildVectorConfig()`:

```csharp
private static VectorConfig? BuildVectorConfig(PropertyInfo prop, VectorAttributeBase vectorAttr)
{
    // Use custom name if specified, otherwise use property name
    var vectorName = vectorAttr.Name ?? PropertyHelper.ToCamelCase(prop.Name);

    var vectorizer = CreateVectorizer(vectorAttr);
    var vectorIndexConfig = BuildVectorIndexConfig(prop);

    return new VectorConfig(
        name: vectorName,
        vectorizer: vectorizer,
        vectorIndexConfig: vectorIndexConfig
    );
}
```

---

## 2. Method-Based Configuration Builder 🎯

### Problem
Current `ConfigBuilder` property requires a separate class implementing `IVectorConfigBuilder<T>`. This is verbose and cumbersome for simple customizations.

### Solution
Add `ConfigMethod` property that accepts a method name (static method in same class or different class):

```csharp
public class VectorAttribute<TVectorizer> : VectorAttributeBase
    where TVectorizer : VectorizerConfig
{
    /// <summary>
    /// Gets or sets the name of a static method that configures the vectorizer.
    /// Method signature: static TVectorizer MethodName(string vectorName, TVectorizer prebuilt)
    ///
    /// - Can be method in same class: ConfigMethod = nameof(MyConfigMethod)
    /// - Can be method in another class: ConfigMethod = "MyClass.MyConfigMethod"
    ///
    /// The method receives:
    /// 1. vectorName - The name of the vector being configured
    /// 2. prebuilt - Vectorizer instance with properties already set from attributes
    ///
    /// The method should return the configured vectorizer instance.
    /// </summary>
    public string? ConfigMethod { get; set; }

    // ... existing properties (Model, Dimensions, etc.)
}
```

### Usage Examples

#### Example 1: Method in Same Class

```csharp
public class Article
{
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-small",
        Dimensions = 1536,
        SourceProperties = [nameof(Title), nameof(Content)],
        ConfigMethod = nameof(ConfigureContentVector))]
    public float[]? ContentEmbedding { get; set; }

    [Vector<Vectorizer.Text2VecCohere>(
        Model = "embed-multilingual-v3.0",
        ConfigMethod = nameof(ConfigureTitleVector))]
    public float[]? TitleVector { get; set; }

    // Configuration method for OpenAI vectorizer
    public static Vectorizer.Text2VecOpenAI ConfigureContentVector(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt)
    {
        // Properties from attribute are already set
        Console.WriteLine($"Configuring {vectorName}");
        Console.WriteLine($"Model: {prebuilt.Model}"); // "text-embedding-3-small"
        Console.WriteLine($"Dimensions: {prebuilt.Dimensions}"); // 1536

        // Add OpenAI-specific configuration
        prebuilt.Type = "text";
        prebuilt.VectorizeClassName = false;

        // Conditional configuration based on vector name
        if (vectorName == "contentEmbedding")
        {
            prebuilt.BaseURL = "https://custom-openai-proxy.com";
        }

        return prebuilt;
    }

    // Configuration method for Cohere vectorizer
    public static Vectorizer.Text2VecCohere ConfigureTitleVector(
        string vectorName,
        Vectorizer.Text2VecCohere prebuilt)
    {
        // Add Cohere-specific configuration
        prebuilt.Truncate = "END";
        prebuilt.BaseURL = "https://api.cohere.ai";

        return prebuilt;
    }
}
```

#### Example 2: Shared Configuration in Separate Class

```csharp
// Shared configuration logic for all collections
public static class VectorConfigurations
{
    public static Vectorizer.Text2VecOpenAI ConfigureOpenAI(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt)
    {
        // Organization-wide OpenAI configuration
        prebuilt.Type = "text";
        prebuilt.VectorizeClassName = false;
        prebuilt.BaseURL = Environment.GetEnvironmentVariable("OPENAI_PROXY_URL")
                           ?? "https://api.openai.com";

        return prebuilt;
    }

    public static Vectorizer.Text2VecHuggingFace ConfigureHuggingFace(
        string vectorName,
        Vectorizer.Text2VecHuggingFace prebuilt)
    {
        // Organization-wide HuggingFace configuration
        prebuilt.WaitForModel = true;
        prebuilt.UseGPU = true;
        prebuilt.UseCache = true;

        return prebuilt;
    }
}

// Use in multiple collections
public class Article
{
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-small",
        ConfigMethod = "VectorConfigurations.ConfigureOpenAI")]
    public float[]? ContentEmbedding { get; set; }
}

public class Product
{
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-large",
        ConfigMethod = "VectorConfigurations.ConfigureOpenAI")]
    public float[]? DescriptionEmbedding { get; set; }
}

public class Document
{
    [Vector<Vectorizer.Text2VecHuggingFace>(
        Model = "sentence-transformers/all-MiniLM-L6-v2",
        ConfigMethod = "VectorConfigurations.ConfigureHuggingFace")]
    public float[]? ContentVector { get; set; }
}
```

#### Example 3: Complex Conditional Logic

```csharp
public class MultilanguageArticle
{
    public string Language { get; set; }

    [Vector<Vectorizer.Text2VecCohere>(
        SourceProperties = [nameof(Title), nameof(Content)],
        ConfigMethod = nameof(ConfigureByLanguage))]
    public float[]? Embedding { get; set; }

    public static Vectorizer.Text2VecCohere ConfigureByLanguage(
        string vectorName,
        Vectorizer.Text2VecCohere prebuilt)
    {
        // Different models for different languages
        // (Note: This is pseudocode - actual implementation would need context)
        prebuilt.Model = "embed-multilingual-v3.0";
        prebuilt.Truncate = "END";

        return prebuilt;
    }
}
```

### Implementation

```csharp
// In VectorConfigBuilder.cs
private static VectorizerConfig? CreateVectorizer(
    VectorAttributeBase attr,
    PropertyInfo prop)
{
    var vectorizerType = attr.VectorizerType;
    var vectorizer = Activator.CreateInstance(vectorizerType) as VectorizerConfig;

    if (vectorizer == null)
        throw new InvalidOperationException($"Failed to create vectorizer of type {vectorizerType.Name}");

    // Map common properties from attributes
    MapCommonProperties(attr, vectorizer);
    MapVectorizerSpecificProperties(attr, vectorizer);

    // Check if we have a generic VectorAttribute<T> with ConfigMethod
    var configMethod = GetConfigMethod(attr);
    if (!string.IsNullOrEmpty(configMethod))
    {
        var vectorName = attr.Name ?? PropertyHelper.ToCamelCase(prop.Name);
        vectorizer = InvokeConfigMethod(configMethod, vectorName, vectorizer, attr, prop);
    }

    return vectorizer;
}

private static string? GetConfigMethod(VectorAttributeBase attr)
{
    // Use reflection to get ConfigMethod property if it exists
    var configMethodProp = attr.GetType().GetProperty("ConfigMethod");
    return configMethodProp?.GetValue(attr) as string;
}

private static VectorizerConfig InvokeConfigMethod(
    string methodPath,
    string vectorName,
    VectorizerConfig prebuilt,
    VectorAttributeBase attr,
    PropertyInfo prop)
{
    // Parse method path: "ClassName.MethodName" or just "MethodName"
    var parts = methodPath.Split('.');
    Type declaringType;
    string methodName;

    if (parts.Length == 1)
    {
        // Method in same class - get from property's declaring type
        declaringType = prop.DeclaringType!;
        methodName = parts[0];
    }
    else if (parts.Length == 2)
    {
        // Method in different class: "ClassName.MethodName"
        var className = parts[0];
        methodName = parts[1];

        // Try to resolve type - first in same namespace, then globally
        var currentNamespace = prop.DeclaringType!.Namespace;
        declaringType = Type.GetType($"{currentNamespace}.{className}")
                       ?? Type.GetType(className);

        if (declaringType == null)
            throw new InvalidOperationException(
                $"Type '{className}' not found. Use fully qualified name.");
    }
    else
    {
        // Fully qualified: "Namespace.ClassName.MethodName"
        methodName = parts.Last();
        var fullClassName = string.Join(".", parts.Take(parts.Length - 1));
        declaringType = Type.GetType(fullClassName);

        if (declaringType == null)
            throw new InvalidOperationException($"Type '{fullClassName}' not found.");
    }

    // Find the static method with correct signature
    var method = declaringType.GetMethod(
        methodName,
        BindingFlags.Public | BindingFlags.Static,
        null,
        new[] { typeof(string), prebuilt.GetType() },
        null);

    if (method == null)
    {
        throw new InvalidOperationException(
            $"Static method '{declaringType.Name}.{methodName}' not found. " +
            $"Expected signature: static {prebuilt.GetType().Name} {methodName}(string vectorName, {prebuilt.GetType().Name} prebuilt)");
    }

    // Validate return type
    if (!typeof(VectorizerConfig).IsAssignableFrom(method.ReturnType))
    {
        throw new InvalidOperationException(
            $"Method '{declaringType.Name}.{methodName}' must return {prebuilt.GetType().Name}");
    }

    try
    {
        // Invoke the configuration method
        return (VectorizerConfig)method.Invoke(null, new object[] { vectorName, prebuilt })!;
    }
    catch (TargetInvocationException ex)
    {
        throw new InvalidOperationException(
            $"Error invoking configuration method '{declaringType.Name}.{methodName}': {ex.InnerException?.Message}",
            ex.InnerException);
    }
}
```

### Benefits

1. **Type Safety** - Method signature enforces correct types
2. **Compile-Time Validation** - `nameof()` prevents typos
3. **IntelliSense Support** - Method name autocomplete
4. **Reusability** - Share configuration logic across collections
5. **Testability** - Configuration methods can be unit tested
6. **Conditional Logic** - Access to vector name for conditional configuration
7. **Pre-built Access** - Get attribute values, add custom properties
8. **Organization** - Centralize configuration in static classes

---

## 3. 100% Feature Parity with Manual CollectionConfig 🎯

### Current Gaps

Based on `CollectionConfig` (Collection.cs:3-68), the ORM currently **DOES NOT SUPPORT**:

1. ❌ **ModuleConfig** - Module-specific configuration
2. ❌ **RerankerConfig** - Reranker configuration
3. ❌ **GenerativeConfig** - Generative AI configuration
4. ❌ **ReplicationConfig** - Replication settings (partially supported, only defaults)
5. ❌ **ShardingConfig** - Sharding configuration

### Proposed Attributes

#### ReplicationConfigAttribute

```csharp
/// <summary>
/// Configures replication settings for the collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ReplicationAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the replication factor (number of replicas).
    /// Default: 1
    /// </summary>
    public int Factor { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether async repair is enabled.
    /// Default: true
    /// </summary>
    public bool AsyncEnabled { get; set; } = true;
}

// Usage
[WeaviateCollection("Articles")]
[Replication(Factor = 3, AsyncEnabled = true)]
public class Article { }
```

#### ShardingConfigAttribute

```csharp
/// <summary>
/// Configures sharding settings for the collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ShardingAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the virtual shard count.
    /// Default: 128
    /// </summary>
    public int VirtualPerPhysical { get; set; } = 128;

    /// <summary>
    /// Gets or sets the desired shard count.
    /// Default: 1
    /// </summary>
    public int DesiredCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the actual shard count (read-only, set by system).
    /// </summary>
    public int? ActualCount { get; set; }

    /// <summary>
    /// Gets or sets the desired virtual shard count.
    /// </summary>
    public int? DesiredVirtualCount { get; set; }

    /// <summary>
    /// Gets or sets the actual virtual shard count (read-only).
    /// </summary>
    public int? ActualVirtualCount { get; set; }
}

// Usage
[WeaviateCollection("LargeDataset")]
[Sharding(DesiredCount = 4, VirtualPerPhysical = 256)]
public class LargeDocument { }
```

#### GenerativeConfigAttribute

```csharp
/// <summary>
/// Configures generative AI module for the collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerativeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the generative module type.
    /// </summary>
    public GenerativeModuleType Module { get; set; }

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the temperature (for supported models).
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the max tokens (for supported models).
    /// </summary>
    public int? MaxTokens { get; set; }
}

public enum GenerativeModuleType
{
    OpenAI,
    Cohere,
    Palm,
    Anthropic,
    Anyscale,
    AWS,
    Ollama
}

// Usage
[WeaviateCollection("Articles")]
[Generative(
    Module = GenerativeModuleType.OpenAI,
    Model = "gpt-4-turbo",
    Temperature = 0.7,
    MaxTokens = 500)]
public class Article { }
```

#### RerankerConfigAttribute

```csharp
/// <summary>
/// Configures reranker module for the collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RerankerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the reranker module type.
    /// </summary>
    public RerankerModuleType Module { get; set; }

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string? Model { get; set; }
}

public enum RerankerModuleType
{
    Cohere,
    Transformers,
    VoyageAI,
    JinaAI
}

// Usage
[WeaviateCollection("SearchResults")]
[Reranker(
    Module = RerankerModuleType.Cohere,
    Model = "rerank-english-v3.0")]
public class SearchResult { }
```

### Alternative: Collection Configuration Method

For maximum flexibility, allow a static method to configure the entire collection:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CollectionConfigMethodAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of a static method that configures the collection.
    /// Method signature: static void MethodName(CollectionConfig config)
    /// </summary>
    public string MethodName { get; set; }

    public CollectionConfigMethodAttribute(string methodName)
    {
        MethodName = methodName;
    }
}

// Usage
[WeaviateCollection("Articles")]
[CollectionConfigMethod(nameof(ConfigureCollection))]
public class Article
{
    public static void ConfigureCollection(CollectionConfig config)
    {
        // Full access to configure anything
        config.ReplicationConfig = new ReplicationConfig { Factor = 3 };
        config.ShardingConfig = new ShardingConfig
        {
            DesiredCount = 4,
            VirtualPerPhysical = 256
        };
        config.GenerativeConfig = new Generative.OpenAI
        {
            Model = "gpt-4-turbo",
            Temperature = 0.7
        };
    }
}
```

---

## Implementation Priority

### Phase 1: Core Enhancements (Immediate)
1. ✅ Add `Name` property to `VectorAttributeBase`
2. ✅ Add `ConfigMethod` property to `VectorAttribute<TVectorizer>`
3. ✅ Implement method invocation in `VectorConfigBuilder`
4. ✅ Add tests for named vectors and config methods
5. ✅ Update documentation

### Phase 2: Collection-Level Config (Next Sprint)
1. Add `ReplicationAttribute`
2. Add `ShardingAttribute`
3. Add `GenerativeAttribute`
4. Add `RerankerAttribute`
5. Update `CollectionSchemaBuilder` to handle new attributes
6. Add comprehensive tests

### Phase 3: Advanced Features (Future)
1. Add `ModuleConfig` support (if needed)
2. Add `CollectionConfigMethodAttribute` for maximum flexibility
3. Consider fluent API alternative to attributes
4. Performance optimizations

---

## Testing Strategy

### Unit Tests

```csharp
[Fact]
public void NamedVector_ShouldUseCustomName()
{
    var config = CollectionSchemaBuilder.FromClass<TestClass>();
    Assert.Contains(config.VectorConfig, v => v.Key == "custom_name");
}

[Fact]
public void ConfigMethod_ShouldInvokeAndApplyChanges()
{
    var config = CollectionSchemaBuilder.FromClass<TestClass>();
    var vector = config.VectorConfig["embedding"];
    var openAI = vector.Vectorizer as Vectorizer.Text2VecOpenAI;
    Assert.Equal("text", openAI.Type);
}

[Fact]
public void ConfigMethod_InDifferentClass_ShouldResolve()
{
    var config = CollectionSchemaBuilder.FromClass<TestClass2>();
    // Verify configuration from external class was applied
}
```

### Integration Tests

```csharp
[Fact]
public async Task CreateCollection_WithNamedVectors_ShouldWork()
{
    var config = CollectionSchemaBuilder.FromClass<Article>();
    await client.Collections.CreateAsync(config);

    var retrieved = await client.Collections.GetAsync("Article");
    Assert.Equal("legacy_vector", retrieved.VectorConfig.Keys.First());
}
```

---

## Documentation Requirements

1. Update `docs/orm_guide.md` with all new examples
2. Create migration guide for existing code
3. Add FAQ section for common scenarios
4. Document performance implications
5. Add troubleshooting section

---

## Backwards Compatibility

All changes are **100% backwards compatible**:

- Existing code without `Name` property continues to work (uses property name)
- Existing code without `ConfigMethod` continues to work (uses attributes only)
- All new attributes are optional
- No breaking changes to existing APIs

---

## Success Criteria

✅ Can configure any vectorizer property declaratively or programmatically
✅ Can work with existing collections with different naming conventions
✅ Can share configuration logic across collections
✅ Can configure replication, sharding, generative, and reranker settings
✅ 100% feature parity with manual `CollectionConfig` creation
✅ IntelliSense guides the entire development experience
✅ All existing code continues to work without changes

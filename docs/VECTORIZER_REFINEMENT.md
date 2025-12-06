# Vectorizer Configuration - Refined Proposal

**Date:** 2025-12-06
**Status:** Refined based on codebase analysis

---

## Analysis Results

### Vectorizer Categories

After analyzing the codebase, vectorizers fall into these categories:

#### 1. **Self-Provided** (No Configuration)
- `Vectorizer.SelfProvided` - Empty record, NO properties at all
- Used when vectors are provided externally
- Should NOT allow `SourceProperties`, `Model`, etc.

#### 2. **Single-Vector Text Vectorizers** (1D arrays: `float[]`)
- `Text2VecOpenAI`, `Text2VecCohere`, `Text2VecHuggingFace`, `Text2VecTransformers`
- `Text2VecAWS`, `Text2VecAzureOpenAI`, `Text2VecContextionary`, `Text2VecDatabricks`
- `Text2VecJinaAI`, `Text2VecNvidia`, `Text2VecMistral`, `Text2VecModel2Vec`
- `Text2VecMorph`, `Text2VecOllama`, `Text2VecGoogle`, `Text2VecVoyageAI`, `Text2VecWeaviate`

**Common properties (from specific vectorizer records):**
- Model, BaseURL, Dimensions
- VectorizeCollectionName (boolean)
- Vectorizer-specific properties (Type, Truncate, WaitForModel, etc.)

**Property from BASE class (VectorizerConfig):**
- `SourceProperties` (ICollection<string>) - **Inherited by ALL** including SelfProvided!

#### 3. **Single-Vector Multi-Modal Vectorizers** (1D arrays: `float[]`)
- `Multi2VecClip`, `Multi2VecCohere`, `Multi2VecBind`, `Multi2VecGoogle`
- `Multi2VecAWS`, `Multi2VecJinaAI`, `Multi2VecVoyageAI`, `Multi2VecNvidia`
- `Img2VecNeural`

**Common properties:**
- TextFields, ImageFields, VideoFields, AudioFields, etc.
- VectorizeCollectionName
- Weights (for balancing modalities)
- InferenceUrl, Model, BaseURL

#### 4. **Multi-Vector Vectorizers** (2D arrays: `float[,]` or `float[][]`)
- `Text2MultiVecJinaAI` - Text to multi-vector (ColBERT-style)
- `Multi2MultiVecJinaAI` - Multi-modal to multi-vector

**Special requirements:**
- MUST be used with 2D array properties (`float[,]` or `float[][]`)
- MUST have `MultiVector` config in index configuration
- Cannot be used with regular `float[]` properties

#### 5. **Reference-Based Vectorizers** (1D arrays: `float[]`)
- `Ref2VecCentroid` - Aggregates vectors from references

**Special properties:**
- ReferenceProperties (which references to aggregate)
- Method (aggregation method)

---

## Problem with Current Approach

### Issue 1: SourceProperties on SelfProvided

Current `VectorAttribute<TVectorizer>` allows this:

```csharp
[Vector<Vectorizer.SelfProvided>(
    SourceProperties = [nameof(Title), nameof(Content)]  // ❌ WRONG! SelfProvided has no properties
)]
public float[]? Embedding { get; set; }
```

**Why this is wrong:**
- `SelfProvided` is an empty record with NO configuration
- `SourceProperties` is in the BASE class `VectorizerConfig`, but it makes no sense for SelfProvided
- Setting `SourceProperties` on SelfProvided does nothing and is misleading

### Issue 2: Multi-Vector vs Single-Vector Type Safety

Current approach allows this:

```csharp
// ❌ WRONG! Text2MultiVecJinaAI should only be for float[,] or float[][]
[Vector<Vectorizer.Text2MultiVecJinaAI>(Model = "jina-colbert-v1-en")]
public float[]? Embedding { get; set; }  // Should be float[,]!

// ❌ WRONG! Text2VecOpenAI should only be for float[]
[Vector<Vectorizer.Text2VecOpenAI>(Model = "text-embedding-3-small")]
public float[,]? Embedding { get; set; }  // Should be float[]!
```

---

## Refined Solution

### 1. Separate Attribute Hierarchies for Single vs Multi-Vector

```csharp
// Base class - no direct usage
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class VectorAttributeBase : Attribute
{
    public abstract Type VectorizerType { get; }

    /// <summary>
    /// Gets or sets the vector name in the collection schema.
    /// If not specified, the property name (converted to camelCase) will be used.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a static method name for custom configuration.
    /// Signature: static TVectorizer MethodName(string vectorName, TVectorizer prebuilt)
    /// </summary>
    public string? ConfigMethod { get; set; }
}

// For regular (single) vectors - ONLY for float[] properties
public class VectorAttribute<TVectorizer> : VectorAttributeBase
    where TVectorizer : VectorizerConfig
{
    public override Type VectorizerType => typeof(TVectorizer);

    // Properties that make sense for TEXT vectorizers
    public string? Model { get; set; }
    public int? Dimensions { get; set; }
    public string? BaseURL { get; set; }
    public string[]? SourceProperties { get; set; }
    public bool? VectorizeCollectionName { get; set; }

    // Properties for multi-modal vectorizers
    public string[]? TextFields { get; set; }
    public string[]? ImageFields { get; set; }
    public string[]? VideoFields { get; set; }

    // Reference vectorizer properties
    public string[]? ReferenceProperties { get; set; }
}

// For multi-vectors (ColBERT) - ONLY for float[,] or float[][] properties
public class MultiVectorAttribute<TVectorizer> : VectorAttributeBase
    where TVectorizer : VectorizerConfig
{
    public override Type VectorizerType => typeof(TVectorizer);

    // Properties for multi-vector vectorizers (JinaAI ColBERT)
    public string? Model { get; set; }
    public string? BaseURL { get; set; }
    public int? Dimensions { get; set; }
    public bool? VectorizeCollectionName { get; set; }

    // For Multi2MultiVecJinaAI
    public string[]? TextFields { get; set; }
    public string[]? ImageFields { get; set; }
}

// For self-provided vectors - NO configuration properties!
public class SelfProvidedVectorAttribute : VectorAttributeBase
{
    public override Type VectorizerType => typeof(Vectorizer.SelfProvided);

    // NO properties! Self-provided has no configuration.
}
```

### 2. Usage Examples

```csharp
public class Article
{
    // ✅ CORRECT: Regular text vectorizer with float[]
    [Vector<Vectorizer.Text2VecOpenAI>(
        Name = "contentEmbedding",
        Model = "text-embedding-3-small",
        Dimensions = 1536,
        SourceProperties = [nameof(Title), nameof(Content)]
    )]
    public float[]? ContentEmbedding { get; set; }

    // ✅ CORRECT: Multi-modal vectorizer with float[]
    [Vector<Vectorizer.Multi2VecClip>(
        TextFields = [nameof(Description)],
        ImageFields = [nameof(ImageUrl)]
    )]
    public float[]? ImageTextEmbedding { get; set; }

    // ✅ CORRECT: Self-provided vector (no config)
    [SelfProvidedVector]  // or [SelfProvidedVector(Name = "custom")]
    public float[]? CustomEmbedding { get; set; }

    // ✅ CORRECT: ColBERT multi-vector with float[,]
    [MultiVector<Vectorizer.Text2MultiVecJinaAI>(
        Model = "jina-colbert-v1-en",
        Dimensions = 128
    )]
    public float[,]? ColBERTEmbedding { get; set; }

    // ❌ COMPILATION ERROR: Text2VecOpenAI not allowed with MultiVectorAttribute
    [MultiVector<Vectorizer.Text2VecOpenAI>(...)]  // Won't compile!
    public float[,]? WrongEmbedding { get; set; }

    // ❌ COMPILATION ERROR: Text2MultiVecJinaAI not allowed with VectorAttribute
    [Vector<Vectorizer.Text2MultiVecJinaAI>(...)]  // Won't compile!
    public float[]? WrongEmbedding2 { get; set; }
}
```

### 3. Runtime Validation

Add validation in `VectorConfigBuilder` to ensure:
1. `VectorAttribute<T>` is ONLY on `float[]` properties
2. `MultiVectorAttribute<T>` is ONLY on `float[,]` or `float[][]` properties
3. `SelfProvidedVectorAttribute` can be on any float array type

```csharp
private static VectorConfig? BuildVectorConfig(PropertyInfo prop, VectorAttributeBase vectorAttr)
{
    // Validate property type matches attribute type
    var propType = prop.PropertyType;
    var elementType = propType.IsArray ? propType.GetElementType() : propType;

    if (vectorAttr is VectorAttribute<>)
    {
        // Must be float[] (1D array)
        if (propType != typeof(float[]) && propType != typeof(float?[]))
        {
            throw new InvalidOperationException(
                $"Property '{prop.Name}' uses VectorAttribute but is not float[]. " +
                $"For multi-vectors (float[,] or float[][]), use MultiVectorAttribute instead.");
        }
    }
    else if (vectorAttr is MultiVectorAttribute<>)
    {
        // Must be float[,] or float[][]
        if (propType != typeof(float[,]) && propType != typeof(float[][]) &&
            propType != typeof(float[,]?) && propType != typeof(float[]?[]))
        {
            throw new InvalidOperationException(
                $"Property '{prop.Name}' uses MultiVectorAttribute but is not float[,] or float[][]. " +
                $"For single vectors (float[]), use VectorAttribute instead.");
        }
    }

    // ... rest of build logic
}
```

### 4. Type Constraints for Multi-Vector

Add compile-time constraints:

```csharp
// Constraint: Only allow multi-vector vectorizers
public class MultiVectorAttribute<TVectorizer> : VectorAttributeBase
    where TVectorizer : VectorizerConfig, IMultiVectorVectorizer  // ← New marker interface
{
    // ...
}

// In Weaviate.Client/Models/Vectorizer.cs:
public partial record Text2MultiVecJinaAI : IMultiVectorVectorizer { }
public partial record Multi2MultiVecJinaAI : IMultiVectorVectorizer { }

// Marker interface
public interface IMultiVectorVectorizer { }
```

---

## Alternative: Keep Generic but Add Validation

If we want to keep the current generic approach but add validation:

```csharp
public class VectorAttribute<TVectorizer> : VectorAttributeBase
    where TVectorizer : VectorizerConfig
{
    // Keep all properties, but document which are valid for which vectorizers

    /// <summary>
    /// Gets or sets the source properties to vectorize.
    /// NOTE: Not applicable to SelfProvided vectorizer.
    /// </summary>
    public string[]? SourceProperties { get; set; }

    /// <summary>
    /// Gets or sets the model name.
    /// NOTE: Not applicable to SelfProvided vectorizer.
    /// </summary>
    public string? Model { get; set; }

    // ... other properties
}

// Add runtime validation
private static VectorizerConfig? CreateVectorizer(VectorAttributeBase attr, PropertyInfo prop)
{
    var vectorizerType = attr.VectorizerType;

    // Special case: SelfProvided should have NO configuration
    if (vectorizerType == typeof(Vectorizer.SelfProvided))
    {
        var vectorAttr = attr as VectorAttribute<Vectorizer.SelfProvided>;
        if (vectorAttr != null)
        {
            // Warn if properties are set on SelfProvided
            if (vectorAttr.SourceProperties != null ||
                vectorAttr.Model != null ||
                vectorAttr.Dimensions != null ||
                vectorAttr.BaseURL != null)
            {
                throw new InvalidOperationException(
                    $"SelfProvided vectorizer on property '{prop.Name}' should not have " +
                    $"Model, Dimensions, BaseURL, or SourceProperties configured. " +
                    $"These properties are ignored for self-provided vectors.");
            }
        }

        return new Vectorizer.SelfProvided();
    }

    // ... rest of logic
}
```

---

## Recommended Approach

### Phase 1: Add Validation (Immediate) ✅

1. Add `Name` property to `VectorAttributeBase`
2. Add `ConfigMethod` property to `VectorAttribute<TVectorizer>`
3. Add runtime validation to prevent:
   - Setting properties on `SelfProvided`
   - Using multi-vector vectorizers with `float[]`
   - Using single-vector vectorizers with `float[,]`

### Phase 2: Separate Attributes (Future)

1. Create `SelfProvidedVectorAttribute`
2. Create `MultiVectorAttribute<T>`
3. Add marker interface `IMultiVectorVectorizer`
4. Deprecate (but don't remove) generic usage for SelfProvided

---

## Updated Examples

```csharp
// Phase 1: Current approach with validation
public class Article
{
    // ✅ Self-provided - no configuration
    [Vector<Vectorizer.SelfProvided>]  // No properties allowed
    public float[]? CustomEmbedding { get; set; }

    // ✅ OpenAI with custom config method
    [Vector<Vectorizer.Text2VecOpenAI>(
        Name = "contentVector",
        Model = "text-embedding-3-small",
        ConfigMethod = nameof(ConfigureOpenAI)
    )]
    public float[]? ContentEmbedding { get; set; }

    public static Vectorizer.Text2VecOpenAI ConfigureOpenAI(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt)
    {
        // Model is already set from attribute
        prebuilt.Type = "text";  // OpenAI-specific property
        prebuilt.VectorizeClassName = false;
        return prebuilt;
    }
}
```

---

## Benefits

### Type Safety
- ✅ Compile-time: Correct vectorizer types
- ✅ Runtime: Property type validation
- ✅ Prevents meaningless configurations (SelfProvided with Model)

### Developer Experience
- ✅ IntelliSense shows only valid properties
- ✅ Clear error messages when misused
- ✅ Documentation explains constraints

### Flexibility
- ✅ ConfigMethod for vectorizer-specific properties
- ✅ Name property for existing collections
- ✅ Backward compatible (with warnings)

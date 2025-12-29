# Vector Search API Changelog

## Version TBD - API Consolidation

### Summary

Consolidated vector search API from 35+ overloads to ~20 core overloads with extensive implicit conversions.

### Changes

#### New Types

- `VectorSearchInput` - Central type for all vector search inputs with extensive implicit conversions
- `VectorSearchInput.Builder` - Lambda builder for complex multi-target scenarios
- `VectorSearchInput.FactoryFn` - Delegate for creating VectorSearchInput via lambda expressions with implicit conversion
- `HybridVectorInput` - Discriminated union for hybrid search vector inputs (VectorSearchInput, NearTextInput, or NearVectorInput)
- `NearVectorInput` - Wrapper for vector input with optional thresholds (replaces `HybridNearVector`)
- `NearTextInput` - Server-side vectorization with target vectors (replaces `HybridNearText`)
- `TargetVectors` - Static factory methods for target vector configuration
- `TargetVectorsBuilder` - Lambda builder for target vectors

#### Removed Types

- `IHybridVectorInput` - Marker interface removed in favor of `HybridVectorInput`
- `INearVectorInput` - Marker interface removed
- `HybridNearVector` - Renamed to `NearVectorInput`
- `HybridNearText` - Renamed to `NearTextInput`

#### Overload Reduction

| Client | Method | Before | After |
|--------|--------|--------|-------|
| QueryClient | NearVector | 10+ | 4 |
| QueryClient | Hybrid | 4+ | 7 |
| GenerateClient | NearVector | 6+ | 4 |
| GenerateClient | Hybrid | 4+ | 6 |
| AggregateClient | NearVector | 6+ | 4 |
| AggregateClient | Hybrid | 3+ | 2 |

#### API Changes

1. **HybridVectorInput discriminated union for vectors parameter**
   - Before: `Hybrid(string? query, IHybridVectorInput? vectors, ...)`
   - After: `Hybrid(string? query, HybridVectorInput? vectors, ...)` - accepts VectorSearchInput, NearTextInput, or NearVectorInput via implicit conversions

2. **VectorSearchInput replaces multiple input types**
   - Before: Separate overloads for `float[]`, `Vector`, `Vectors`, tuple enumerables
   - After: Single `VectorSearchInput` with implicit conversions from all types

3. **Builder replaces separate overloads**
   - Before: `VectorSearchInputBuilder` as standalone class
   - After: `VectorSearchInput.Builder` as nested class

4. **FactoryFn delegate enables lambda syntax**
   - New: `VectorSearchInput.FactoryFn` delegate with implicit conversion to `VectorSearchInput`
   - Allows: `vectors: b => b.Sum(("title", vec1), ("desc", vec2))` syntax

5. **Targets embedded in VectorSearchInput**
   - Before: Separate `targets` parameter on some methods
   - After: Targets configured via `VectorSearchInput.Builder` methods (Sum, ManualWeights, etc.)

6. **TargetVectors uses static factory methods**
   - Before: `new SimpleTargetVectors(["title", "description"])`
   - After: `TargetVectors.Sum("title", "description")` or `new[] { "title", "description" }` (implicit)

7. **NearTextInput includes TargetVectors**
   - Before: Separate `targetVector` parameter on Hybrid methods
   - After: `NearTextInput.TargetVectors` property is single source of truth

### Migration Examples

**Simple hybrid search:**

```csharp
// Before
await collection.Query.Hybrid("search query");

// After (implicit conversion from string)
await collection.Query.Hybrid("search query");
```

**Hybrid with vectors:**

```csharp
// Before
await collection.Query.Hybrid("search query", vectors: new[] { 1f, 2f, 3f });

// After (tuple implicit conversion)
await collection.Query.Hybrid(("search query", new[] { 1f, 2f, 3f }));
```

**Hybrid with target vectors:**

```csharp
// Before
await collection.Query.Hybrid(
    "search query",
    vectors: new HybridNearText("banana"),
    targetVector: ["title", "description"] // separate parameter
);

// After (targets inside NearTextInput)
await collection.Query.Hybrid(
    new NearTextInput(
        "banana",
        TargetVectors: TargetVectors.Sum("title", "description")
    )
);

// Or with implicit string array conversion
await collection.Query.Hybrid(
    new NearTextInput(
        "banana",
        TargetVectors: new[] { "title", "description" }
    )
);
```

**Lambda builder for vectors:**

```csharp
await collection.Query.Hybrid(
    "search query",
    v => v.Sum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);
```

**Target vectors with weights:**

```csharp
// Use static factory methods
var targets = TargetVectors.ManualWeights(("title", 1.2), ("desc", 0.8));
await collection.Query.Hybrid(
    new NearTextInput("banana", TargetVectors: targets)
);

// Or with RelativeScore
var targets = TargetVectors.RelativeScore(("title", 0.7), ("desc", 0.3));
```

**Simple vector search (no changes needed):**

```csharp
// Works unchanged via implicit conversion
await collection.Query.NearVector(new[] { 1f, 2f, 3f });
```

**Named vector with target:**

```csharp
// Before
await collection.Query.NearVector(new[] { 1f, 2f, 3f }, targetVector: "title");

// After
await collection.Query.NearVector(v => v.Single("title", new[] { 1f, 2f, 3f }));
```

**Multi-target with weights:**

```csharp
await collection.Query.NearVector(
    v => v.ManualWeights(
        ("title", 1.2, new[] { 1f, 2f }),
        ("desc", 0.8, new[] { 3f, 4f })
    )
);
```

### Breaking Changes

- `IHybridVectorInput` and `INearVectorInput` interfaces removed
- `HybridNearVector` renamed to `NearVectorInput`
- `HybridNearText` renamed to `NearTextInput`
- `Hybrid` method `vectors` parameter now uses `HybridVectorInput?` discriminated union type
- `SimpleTargetVectors` and `WeightedTargetVectors` constructors are now internal
- `VectorSearchInput.Builder` constructor is now internal (use `FactoryFn` lambda syntax instead)
- `targetVector` parameter removed from Hybrid methods - use `NearTextInput.TargetVectors` instead
- Removed tuple enumerable overloads (`IEnumerable<(string, Vector)>`)
- `targetVector` string parameter removed from NearVector methods (use builder instead)
- Overload resolution may change in edge cases with implicit conversions

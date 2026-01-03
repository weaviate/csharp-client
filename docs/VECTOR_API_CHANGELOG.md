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
- `HybridVectorInput.FactoryFn` - Delegate for creating HybridVectorInput via lambda builder with `.NearVector()` or `.NearText()` methods
- `NearVectorInput` - Wrapper for vector input with optional thresholds (replaces `HybridNearVector`)
- `NearVectorInput.FactoryFn` - Delegate for creating NearVectorInput via lambda builder with target vector configuration
- `NearTextInput` - Server-side vectorization with target vectors (replaces `HybridNearText`)
- `NearTextInput.FactoryFn` - Delegate for creating NearTextInput via lambda builder with target vector configuration
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
| QueryClient | NearText | 2 | 4 |
| QueryClient | Hybrid | 4+ | 9 |
| GenerateClient | NearVector | 6+ | 4 |
| GenerateClient | NearText | 2 | 4 |
| GenerateClient | Hybrid | 4+ | 8 |
| AggregateClient | NearVector | 6+ | 4 |
| AggregateClient | NearText | 2 | 4 |
| AggregateClient | Hybrid | 3+ | 4 |
| TypedQueryClient | NearText | 2 | 4 |
| TypedGenerateClient | NearText | 2 | 4 |

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

8. **Convenience overloads for text-only Hybrid search**
   - New: Overloads without `vectors` parameter for all Hybrid methods
   - These delegate to main Hybrid method with `vectors: null`
   - Simplifies pure text search: `Hybrid("query")` instead of `Hybrid("query", vectors: null)`

9. **Convenience overloads for NearText target vectors**
   - New: Overloads accepting `TargetVectors?` for all NearText methods (QueryClient, GenerateClient, AggregateClient, TypedQueryClient, TypedGenerateClient)
   - Allows passing string arrays directly: `targets: new[] { "vec1", "vec2" }`
   - Allows static factory methods: `targets: TargetVectors.Sum("vec1", "vec2")`
   - Lambda builder syntax still available: `targets: tv => tv.Sum("vec1", "vec2")`
   - Matches pattern already established by NearVector methods with VectorSearchInput

10. **NearVectorInput FactoryFn constructor for consistency**
   - New: Constructor accepting `VectorSearchInput.FactoryFn` for lambda builder syntax
   - Enables: `new NearVectorInput(v => v.Sum(("title", vec1), ("desc", vec2)))`
   - Matches pattern established by NearTextInput

11. **HybridVectorInput lambda builder for unified target vector syntax**
   - New: `HybridVectorInput.FactoryFn` delegate enables lambda builder pattern for Hybrid search
   - Syntax: `v => v.NearVector(certainty: 0.8).ManualWeights(("title", 1.2, vec1), ("desc", 0.8, vec2))`
   - Syntax: `v => v.NearText(["query"]).ManualWeights(("title", 1.2), ("desc", 0.8))`
   - Eliminates need to construct `NearVectorInput` or `NearTextInput` explicitly
   - Available across all clients: QueryClient, GenerateClient, AggregateClient, TypedQueryClient
   - Unifies target vector configuration directly within the Hybrid method call

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

**NearText with target vectors:**

```csharp
// Before: lambda builder required
await collection.Query.NearText(
    "search query",
    targets: tv => tv.Sum("title", "description")
);

// After: multiple options available
// Option 1: String array (simplest)
await collection.Query.NearText(
    "search query",
    targets: new[] { "title", "description" }
);

// Option 2: Static factory method
await collection.Query.NearText(
    "search query",
    targets: TargetVectors.Sum("title", "description")
);

// Option 3: Lambda builder (still works)
await collection.Query.NearText(
    "search query",
    targets: tv => tv.Sum("title", "description")
);
```

**Simple vector search (no changes needed):**

```csharp
// Works unchanged via implicit conversion
await collection.Query.NearVector(new[] { 1f, 2f, 3f });
```

**NearVectorInput with lambda builder:**

```csharp
// Before: only accepts VectorSearchInput directly
new NearVectorInput(new[] { 1f, 2f, 3f });

// After: also accepts lambda builder
new NearVectorInput(
    v => v.Sum(("title", vec1), ("desc", vec2)),
    Certainty: 0.8f
);
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

**Hybrid with NearVector and target vectors (NEW unified syntax):**

```csharp
// Before: construct NearVectorInput explicitly
await collection.Query.Hybrid(
    "test",
    new NearVectorInput(
        VectorSearchInput.Combine(
            TargetVectors.ManualWeights(("title", 1.2), ("desc", 0.8)),
            ("title", new[] { 1f, 2f }),
            ("desc", new[] { 3f, 4f })
        ),
        Certainty: 0.8f
    )
);

// After: lambda builder unifies configuration
await collection.Query.Hybrid(
    "test",
    v => v.NearVector(certainty: 0.8f)
        .ManualWeights(
            ("title", 1.2, new[] { 1f, 2f }),
            ("desc", 0.8, new[] { 3f, 4f })
        )
);
```

**Hybrid with NearText and target vectors (NEW unified syntax):**

```csharp
// Before: construct NearTextInput explicitly
await collection.Query.Hybrid(
    "test",
    new NearTextInput(
        ["concept1", "concept2"],
        TargetVectors: TargetVectors.ManualWeights(("title", 1.2), ("desc", 0.8))
    )
);

// After: lambda builder unifies configuration
await collection.Query.Hybrid(
    "test",
    v => v.NearText(["concept1", "concept2"])
        .ManualWeights(("title", 1.2), ("desc", 0.8))
);
```

**Hybrid with NearText including Move parameters:**

```csharp
await collection.Query.Hybrid(
    "test",
    v => v.NearText(
            ["concept"],
            certainty: 0.7f,
            moveTo: new Move(concepts: "positive", force: 0.5f),
            moveAway: new Move(concepts: "negative", force: 0.3f)
        )
        .Sum("title", "description")
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

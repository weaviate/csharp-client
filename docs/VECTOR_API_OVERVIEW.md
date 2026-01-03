# Vector Search API Reference

Reference documentation for the vector search API in the Weaviate C# client.

## API Overview

| Client          | Method     | Description                              |
|-----------------|------------|------------------------------------------|
| QueryClient     | NearVector | Vector similarity search                 |
| QueryClient     | NearText   | Text-based semantic search               |
| QueryClient     | Hybrid     | Combined keyword + vector search         |
| GenerateClient  | NearVector | Vector search with generative AI         |
| GenerateClient  | NearText   | Text search with generative AI           |
| GenerateClient  | Hybrid     | Hybrid search with generative AI         |
| AggregateClient | NearVector | Vector search with aggregation           |
| AggregateClient | NearText   | Text search with aggregation             |
| AggregateClient | Hybrid     | Hybrid search with aggregation           |

---

## QueryClient.NearVector

Vector similarity search using vector embeddings.

### Method Signatures

```csharp
// Direct vector input
Task<WeaviateResult> NearVector(
    VectorSearchInput vectors,
    ...
)

// With GroupBy
Task<GroupByResult> NearVector(
    VectorSearchInput vectors,
    GroupByRequest groupBy,
    ...
)

// Lambda builder
Task<WeaviateResult> NearVector(
    Func<VectorSearchInput.Builder, VectorSearchInput> vectorsBuilder,
    ...
)

// Lambda builder with GroupBy
Task<GroupByResult> NearVector(
    Func<VectorSearchInput.Builder, VectorSearchInput> vectorsBuilder,
    GroupByRequest groupBy,
    ...
)
```

### Examples

```csharp
// Simple float array (implicit conversion)
await collection.Query.NearVector(new[] { 1f, 2f, 3f });

// Named vectors for multi-vector collections
await collection.Query.NearVector(
    new Vectors {
        { "title", new[] { 1f, 2f } },
        { "description", new[] { 3f, 4f } }
    }
);

// Lambda builder - Sum combination
await collection.Query.NearVector(
    v => v.Sum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - ManualWeights
await collection.Query.NearVector(
    v => v.ManualWeights(
        ("title", 1.2, new[] { 1f, 2f }),
        ("description", 0.8, new[] { 3f, 4f })
    )
);

// Lambda builder - Average
await collection.Query.NearVector(
    v => v.Average(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - Minimum
await collection.Query.NearVector(
    v => v.Minimum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - RelativeScore
await collection.Query.NearVector(
    v => v.RelativeScore(
        ("title", 0.7, new[] { 1f, 2f }),
        ("description", 0.3, new[] { 3f, 4f })
    )
);

// Multi-vector (ColBERT-style)
await collection.Query.NearVector(
    ("colbert", new[,] {
        { 1f, 2f },
        { 3f, 4f }
    })
);

// With certainty threshold
await collection.Query.NearVector(
    new[] { 1f, 2f, 3f },
    certainty: 0.7f
);

// With distance threshold
await collection.Query.NearVector(
    new[] { 1f, 2f, 3f },
    distance: 0.3f
);

// With GroupBy
await collection.Query.NearVector(
    new[] { 1f, 2f, 3f },
    new GroupByRequest("category", objectsPerGroup: 3)
);
```

---

## QueryClient.NearText

Text-based semantic search with server-side vectorization.

### Method Signatures

```csharp
// Simple text search
Task<WeaviateResult> NearText(
    AutoArray<string> text,
    ...
)

// With GroupBy
Task<GroupByResult> NearText(
    AutoArray<string> text,
    GroupByRequest groupBy,
    ...
)

// Lambda builder for NearTextInput
Task<WeaviateResult> NearText(
    NearTextInput.FactoryFn query,
    ...
)

// Lambda builder with GroupBy
Task<GroupByResult> NearText(
    NearTextInput.FactoryFn query,
    GroupByRequest groupBy,
    ...
)

// Extension: NearTextInput record
Task<WeaviateResult> NearText(
    NearTextInput input,
    ...
)
```

### Examples

```csharp
// Simple text search
await collection.Query.NearText("banana");

// Multiple search terms
await collection.Query.NearText(new[] { "banana", "tropical", "fruit" });

// With certainty
await collection.Query.NearText(
    "banana",
    certainty: 0.7f
);

// With move parameters
await collection.Query.NearText(
    "banana",
    moveTo: new Move("fruit", force: 0.5f),
    moveAway: new Move("vegetable", force: 0.3f)
);

// Lambda builder - with target vectors (Sum)
await collection.Query.NearText(
    q => q(["banana"], certainty: 0.7f)
        .Sum("title", "description")
);

// Lambda builder - with target vectors (Average)
await collection.Query.NearText(
    q => q(["tropical", "fruit"])
        .Average("title", "description", "category")
);

// Lambda builder - with target vectors (ManualWeights)
await collection.Query.NearText(
    q => q(["search query"])
        .ManualWeights(("title", 0.8), ("description", 0.2))
);

// Lambda builder - with move parameters and targets
await collection.Query.NearText(
    q => q(["banana"], moveTo: new Move("fruit", 0.5f))
        .Sum("title", "description")
);

// Using NearTextInput record
await collection.Query.NearText(
    new NearTextInput(
        "banana",
        TargetVectors: TargetVectors.Sum("title", "description"),
        Certainty: 0.7f
    )
);

// With GroupBy
await collection.Query.NearText(
    "banana",
    new GroupByRequest("category", objectsPerGroup: 3)
);

// Lambda builder with GroupBy
await collection.Query.NearText(
    q => q(["banana"]).Sum("title", "description"),
    groupBy: new GroupByRequest("category", objectsPerGroup: 3)
);
```

---

## QueryClient.Hybrid

Combined keyword (BM25) and vector search with configurable fusion.

### Method Signatures

```csharp
// Text-only hybrid search
Task<WeaviateResult> Hybrid(
    string query,
    ...
)

// With vectors
Task<WeaviateResult> Hybrid(
    string? query,
    HybridVectorInput? vectors,
    ...
)

// Lambda builder for vectors (extension)
Task<WeaviateResult> Hybrid(
    string? query,
    HybridVectorInput.FactoryFn? vectors,
    ...
)
```

### Examples

```csharp
// Text-only search
await collection.Query.Hybrid("search query");

// With alpha parameter (0 = keyword only, 1 = vector only)
await collection.Query.Hybrid(
    "search query",
    alpha: 0.5f
);

// With fusion type
await collection.Query.Hybrid(
    "search query",
    fusionType: HybridFusion.RelativeScore
);

// Text + simple vector
await collection.Query.Hybrid(
    "search query",
    new[] { 1f, 2f, 3f }
);

// Text + named vectors
await collection.Query.Hybrid(
    "search query",
    new Vectors {
        { "title", new[] { 1f, 2f } },
        { "description", new[] { 3f, 4f } }
    }
);

// Lambda builder - NearVector with Sum
await collection.Query.Hybrid(
    "search query",
    v => v.NearVector().Sum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - NearVector with ManualWeights
await collection.Query.Hybrid(
    "search query",
    v => v.NearVector(certainty: 0.7f).ManualWeights(
        ("title", 1.2, new[] { 1f, 2f }),
        ("description", 0.8, new[] { 3f, 4f })
    )
);

// NearText with server-side vectorization
await collection.Query.Hybrid(
    new NearTextInput("banana")
);

// NearText with target vectors
await collection.Query.Hybrid(
    new NearTextInput(
        "banana",
        TargetVectors: TargetVectors.Sum("title", "description")
    )
);

// Lambda builder - NearText with targets
await collection.Query.Hybrid(
    query: null,
    vectors: v => v.NearText()
        .Query(["banana"], certainty: 0.7f)
        .Sum("title", "description")
);

// Multi-vector (ColBERT)
await collection.Query.Hybrid(
    "search query",
    ("colbert", new[,] {
        { 1f, 2f },
        { 3f, 4f }
    })
);

// With GroupBy
await collection.Query.Hybrid(
    "search query",
    new GroupByRequest("category", objectsPerGroup: 3)
);
```

---

## GenerateClient Methods

Same signatures as QueryClient but with additional generative parameters: `singlePrompt` and `groupedTask`.

### Examples

```csharp
// NearVector with generation
await collection.Generate.NearVector(
    new[] { 1f, 2f, 3f },
    singlePrompt: "Summarize this item"
);

// NearText with generation
await collection.Generate.NearText(
    "banana",
    groupedTask: new GroupedTask("Compare all results")
);

// Hybrid with generation
await collection.Generate.Hybrid(
    "search query",
    v => v.NearVector().Sum(
        ("title", new[] { 1f, 2f }),
        ("desc", new[] { 3f, 4f })
    ),
    singlePrompt: "Summarize",
    groupedTask: new GroupedTask("Compare all")
);
```

---

## AggregateClient Methods

Same signatures as QueryClient but return aggregation results and support `returnMetrics` parameter.

### Examples

```csharp
// NearVector with aggregation
await collection.Aggregate.NearVector(
    new[] { 1f, 2f, 3f },
    returnMetrics: [Metrics.ForProperty("price").Number(mean: true, sum: true)]
);

// NearText with aggregation
await collection.Aggregate.NearText(
    "banana",
    returnMetrics: [Metrics.ForProperty("price").Number(mean: true)]
);

// Hybrid with aggregation
await collection.Aggregate.Hybrid(
    "search query",
    new[] { 1f, 2f, 3f },
    returnMetrics: [Metrics.ForProperty("quantity").Number(sum: true)]
);
```

---

## Core Types

### VectorSearchInput

Central type for vector search inputs with collection initializer syntax and many implicit conversions.

**Key implicit conversions:**
- `float[]`, `double[]` → single unnamed vector
- `Vectors` → named vectors collection
- `(string, float[])` → single named vector
- `(string, float[,])` → single named multi-vector (ColBERT)
- `Dictionary<string, float[]>` → multiple named vectors

**Examples:**

```csharp
// Simple array
VectorSearchInput input = new[] { 1f, 2f, 3f };

// Named vectors
VectorSearchInput input = new Vectors {
    { "title", new[] { 1f, 2f } },
    { "description", new[] { 3f, 4f } }
};

// Tuple
VectorSearchInput input = ("title", new[] { 1f, 2f });
```

### VectorSearchInput.Builder

Builder for multi-target combinations via lambda syntax.

**Methods:**
- `Sum(params (string, Vector)[] targets)` - Sum vectors
- `Average(params (string, Vector)[] targets)` - Average vectors
- `Minimum(params (string, Vector)[] targets)` - Minimum values
- `ManualWeights(params (string, double, Vector)[] targets)` - Custom weights
- `RelativeScore(params (string, double, Vector)[] targets)` - Relative score weights

**Usage:**

```csharp
v => v.Sum(("title", vec1), ("desc", vec2))
v => v.ManualWeights(("title", 1.5, vec1), ("desc", 0.5, vec2))
```

---

### NearTextInput

Text search configuration with optional target vectors and move parameters.

**Properties:**
- `Query` - Text query (string or string[])
- `TargetVectors` - Target vectors for multi-vector collections
- `Certainty` - Minimum certainty (0-1)
- `Distance` - Maximum distance
- `MoveTo` / `MoveAway` - Concept movement

**Creation:**

```csharp
// Simple
new NearTextInput("banana")

// With targets
new NearTextInput(
    "banana",
    TargetVectors: TargetVectors.Sum("title", "description")
)

// Lambda builder
q => q(["banana"], certainty: 0.7f)
    .Sum("title", "description")
```

---

### HybridVectorInput

Discriminated union for hybrid search vectors. Can be `VectorSearchInput`, `NearTextInput`, or `NearVectorInput`.

**Lambda Builder (via FactoryFn):**

```csharp
// NearVector
v => v.NearVector().Sum(
    ("title", new[] { 1f, 2f }),
    ("desc", new[] { 3f, 4f })
)

// NearVector with certainty
v => v.NearVector(certainty: 0.7f).ManualWeights(
    ("title", 1.2, new[] { 1f, 2f }),
    ("desc", 0.8, new[] { 3f, 4f })
)

// NearText
v => v.NearText()
    .Query(["banana"], certainty: 0.7f)
    .Sum("title", "description")
```

---

### TargetVectors

Specifies which named vectors to target and how to combine them.

**Factory Methods:**

```csharp
TargetVectors.Sum("title", "description")
TargetVectors.Average("title", "description")
TargetVectors.Minimum("title", "description")
TargetVectors.ManualWeights(("title", 1.5), ("desc", 0.5))
TargetVectors.RelativeScore(("title", 0.7), ("desc", 0.3))

// Implicit from array
TargetVectors targets = new[] { "title", "description" };
```

---

## Best Practices

1. **Use implicit conversions** for simple cases:
   ```csharp
   await collection.Query.NearVector(new[] { 1f, 2f, 3f });
   ```

2. **Use lambda builders** for multi-target with combinations:
   ```csharp
   await collection.Query.NearVector(
       v => v.Sum(("title", vec1), ("desc", vec2))
   );
   ```

3. **For NearText with targets**, use lambda builder:
   ```csharp
   await collection.Query.NearText(
       q => q(["banana"]).Sum("title", "description")
   );
   ```

4. **For Hybrid with complex vectors**, use HybridVectorInput lambda:
   ```csharp
   await collection.Query.Hybrid(
       "search text",
       v => v.NearVector().ManualWeights(
           ("title", 1.2, vec1),
           ("desc", 0.8, vec2)
       )
   );
   ```

5. **Set appropriate thresholds**:
   - `certainty`: 0.7-0.9 for high-quality results
   - `distance`: 0.1-0.3 for similar items (lower is closer)
   - `alpha`: 0.5 for balanced hybrid, 0.75 for more vector influence

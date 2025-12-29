# Vector Search API Reference

Reference documentation for the vector search API in the Weaviate C# client.

## API Overview

| Client          | Method     | Overloads | Notes                                   |
|-----------------|------------|-----------|-----------------------------------------|
| QueryClient     | NearVector | 4         | Vector-only search with lambda builders |
| QueryClient     | Hybrid     | 7+        | Text + vectors, uses HybridInput        |
| GenerateClient  | NearVector | 4         | With generative AI prompts              |
| GenerateClient  | Hybrid     | 6+        | Hybrid + generative, uses HybridInput   |
| AggregateClient | NearVector | 4         | With aggregation                        |
| AggregateClient | Hybrid     | 2         | Hybrid + aggregation                    |
---

## QueryClient.NearVector

```csharp
Task<WeaviateResult> NearVector(VectorSearchInput vectors, ...)
Task<GroupByResult> NearVector(VectorSearchInput vectors, GroupByRequest groupBy, ...)
Task<WeaviateResult> NearVector(Func<VectorSearchInput.Builder, VectorSearchInput> vectors, ...)
Task<GroupByResult> NearVector(Func<VectorSearchInput.Builder, VectorSearchInput> vectors, GroupByRequest groupBy, ...)
```

**Examples:**

```csharp
// Simple float array (implicit conversion)
await collection.Query.NearVector(new[] { 1f, 2f, 3f });

// Named vectors
await collection.Query.NearVector(
    new Vectors {
        { "title", new[] { 1f, 2f } },
        { "description", new[] { 3f, 4f } }
    }
);

// Lambda builder - multi-target with Sum combination
await collection.Query.NearVector(
    v => v.Sum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - weighted targets
await collection.Query.NearVector(
    v => v.ManualWeights(
        ("title", 1.2, new[] { 1f, 2f }),
        ("description", 0.8, new[] { 3f, 4f })
    )
);

// Multi-vector (ColBERT-style) - implicit conversion from tuple
await collection.Query.NearVector(
    ("colbert", new[,] {
        { 1f, 2f },
        { 3f, 4f }
    })
);

// With grouping
await collection.Query.NearVector(
    new[] { 1f, 2f, 3f },
    new GroupByRequest("category", objectsPerGroup: 3)
);
```

---

## QueryClient.Hybrid

```csharp
// Core overloads with HybridInput
Task<WeaviateResult> Hybrid(HybridInput input, ...)
Task<GroupByResult> Hybrid(HybridInput input, GroupByRequest groupBy, ...)

// Legacy Vectors overload
Task<WeaviateResult> Hybrid(string? query, Vectors vectors, ...)
Task<GroupByResult> Hybrid(string? query, GroupByRequest groupBy, Vectors vectors, ...)

// Lambda builder convenience overloads
Task<WeaviateResult> Hybrid(string? query, Func<VectorSearchInput.Builder, VectorSearchInput> vectorsBuilder, ...)
Task<WeaviateResult> Hybrid(NearTextInput nearText, Func<VectorSearchInput.Builder, VectorSearchInput> vectorsBuilder, ...)
Task<WeaviateResult> Hybrid(string query, Func<TargetVectorsBuilder, TargetVectors> targetVectorsBuilder, ...)
```

**Examples:**

```csharp
// Text-only search (implicit conversion from string)
await collection.Query.Hybrid("search query");

// Text + vectors (implicit conversion from tuple)
await collection.Query.Hybrid(("search query", new[] { 1f, 2f, 3f }));

// Named vectors with text query
await collection.Query.Hybrid(
    ("search query", new Vectors {
        { "title", new[] { 1f, 2f } },
        { "description", new[] { 3f, 4f } }
    })
);

// Lambda builder - multi-target with Sum combination
await collection.Query.Hybrid(
    "search query",
    v => v.Sum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// NearText with server-side vectorization (implicit conversion)
await collection.Query.Hybrid(new NearTextInput("banana"));

// NearText with target vectors (using static method)
await collection.Query.Hybrid(
    new NearTextInput(
        "banana",
        TargetVectors: TargetVectors.Sum("title", "description")
    )
);

// NearText with target vectors (using implicit conversion)
await collection.Query.Hybrid(
    new NearTextInput(
        "banana",
        TargetVectors: new[] { "title", "description" }
    )
);

// NearText + vector search input
await collection.Query.Hybrid(
    new HybridInput(
        nearText: new NearTextInput("banana"),
        vectors: new VectorSearchInput {
            { "image", new[] { 1f, 2f, 3f } }
        }
    )
);

// Vector-only search (implicit conversion from VectorSearchInput)
await collection.Query.Hybrid(new[] { 1f, 2f, 3f });

// Multi-vector (ColBERT-style)
await collection.Query.Hybrid(
    new VectorSearchInput {
        ("colbert", new[,] {
            { 1f, 2f },
            { 3f, 4f }
        })
    }
);

// TargetVectors builder with text query
await collection.Query.Hybrid(
    "banana",
    tv => tv.Sum(["title", "description"])
);
```

---

## GenerateClient.NearVector

```csharp
Task<GenerativeWeaviateResult> NearVector(VectorSearchInput vectors, ..., SinglePrompt? singlePrompt, GroupedTask? groupedTask, ...)
Task<GenerativeGroupByResult> NearVector(VectorSearchInput vectors, GroupByRequest groupBy, ...)
Task<GenerativeWeaviateResult> NearVector(Func<VectorSearchInput.Builder, VectorSearchInput> vectors, ...)
Task<GenerativeGroupByResult> NearVector(Func<VectorSearchInput.Builder, VectorSearchInput> vectors, GroupByRequest groupBy, ...)
```

**Examples:**

```csharp
// Simple with generative prompt
await collection.Generate.NearVector(
    new[] { 1f, 2f, 3f },
    singlePrompt: "Summarize this item"
);

// Lambda builder with grouped task
await collection.Generate.NearVector(
    v => v.Sum(("title", new[] { 1f, 2f }), ("desc", new[] { 3f, 4f })),
    groupedTask: new GroupedTask("Summarize all items")
);
```

---

## AggregateClient.NearVector

```csharp
Task<AggregateResult> NearVector(VectorSearchInput vectors, ...)
Task<AggregateGroupByResult> NearVector(VectorSearchInput vectors, Aggregate.GroupBy groupBy, ...)
Task<AggregateResult> NearVector(Func<VectorSearchInput.Builder, VectorSearchInput> vectors, ...)
Task<AggregateGroupByResult> NearVector(Func<VectorSearchInput.Builder, VectorSearchInput> vectors, Aggregate.GroupBy groupBy, ...)
```

**Examples:**

```csharp
// Simple aggregate
await collection.Aggregate.NearVector(
    new[] { 1f, 2f, 3f },
    returnMetrics: [Metrics.ForProperty("price").Number(mean: true)]
);

// With grouping
await collection.Aggregate.NearVector(
    new[] { 1f, 2f, 3f },
    groupBy: new Aggregate.GroupBy("category", limit: 10),
    returnMetrics: [Metrics.ForProperty("price").Number(mean: true)]
);
```

---

## AggregateClient.Hybrid

```csharp
Task<AggregateResult> Hybrid(string? query = null, float alpha = 0.7f, Vectors? vectors = null, ...)
Task<AggregateGroupByResult> Hybrid(string? query, Aggregate.GroupBy groupBy, float alpha = 0.7f, Vectors? vectors = null, ...)
```

---

## VectorSearchInput

Central type for vector search inputs. Supports collection initializer syntax and implicit conversions.

### Implicit Conversions

| From Type                                    | Example                                                 |
|----------------------------------------------|---------------------------------------------------------|
| `float[]`                                    | `new[] { 1f, 2f, 3f }`                                  |
| `double[]`                                   | `new[] { 1.0, 2.0, 3.0 }`                               |
| `Vector`                                     | `new Vector(new VectorSingle<float>([1f, 2f]))`         |
| `Vectors`                                    | `new Vectors { { "name", values } }`                    |
| `NamedVector`                                | `new NamedVector("name", values)`                       |
| `NamedVector[]`                              | `new[] { namedVector1, namedVector2 }`                  |
| `Dictionary<string, float[]>`                | `new Dictionary<string, float[]> { ["name"] = values }` |
| `Dictionary<string, double[]>`               | Same pattern                                            |
| `Dictionary<string, float[,]>`               | For multi-vectors (ColBERT)                             |
| `Dictionary<string, double[,]>`              | For multi-vectors (ColBERT)                             |
| `Dictionary<string, Vector[]>`               | For multiple vectors per name                           |
| `Dictionary<string, IEnumerable<float[]>>`   | For multiple vectors per name                           |
| `Dictionary<string, IEnumerable<double[]>>`  | For multiple vectors per name                           |
| `Dictionary<string, IEnumerable<float[,]>>`  | For multiple multi-vectors                              |
| `Dictionary<string, IEnumerable<double[,]>>` | For multiple multi-vectors                              |
| `(string, float[])` tuple                    | `("name", new[] { 1f, 2f })`                            |
| `(string, double[])` tuple                   | `("name", new[] { 1.0, 2.0 })`                          |
| `(string, float[,])` tuple                   | `("colbert", multiVectorArray)` for ColBERT             |
| `(string, double[,])` tuple                  | `("colbert", multiVectorArray)` for ColBERT             |
| `FactoryFn`                                  | `b => b.Sum(("title", vec1), ("desc", vec2))`           |

### Collection Initializer Syntax

```csharp
var input = new VectorSearchInput {
    { "title", new[] { 1f, 2f } },
    { "description", new[] { 3f, 4f } }
};

// C# 12+ collection expression
VectorSearchInput input = [
    new NamedVector("title", new[] { 1f, 2f }),
    new NamedVector("description", new[] { 3f, 4f })
];
```

---

## VectorSearchInput.Builder

Builder for multi-target vector search combinations using lambda syntax.

| Method               | Description                              |
|----------------------|------------------------------------------|
| `Sum(...)`           | Multi-target with Sum combination        |
| `Average(...)`       | Multi-target with Average combination    |
| `Minimum(...)`       | Multi-target with Minimum combination    |
| `ManualWeights(...)` | Multi-target with manual weights         |
| `RelativeScore(...)` | Multi-target with relative score weights |

**Examples:**

```csharp
// Sum combination
v => v.Sum(
    ("title", new[] { 1f, 2f }),
    ("description", new[] { 3f, 4f })
)

// Manual weights
v => v.ManualWeights(
    ("title", 1.5, new[] { 1f, 2f }),
    ("description", 0.5, new[] { 3f, 4f })
)

// Relative score
v => v.RelativeScore(
    ("title", 0.7, new[] { 1f, 2f }),
    ("description", 0.3, new[] { 3f, 4f })
)
```

---

## VectorSearchInput.FactoryFn

Delegate type for creating `VectorSearchInput` using a builder pattern. Enables passing lambda expressions directly where `VectorSearchInput` is expected via implicit conversion.

```csharp
public delegate VectorSearchInput FactoryFn(Builder builder);
```

**Example Usage:**

```csharp
// Declare and use explicitly
VectorSearchInput.FactoryFn factory = b => b.Sum(
    ("title", new[] { 1f, 2f }),
    ("description", new[] { 3f, 4f })
);
VectorSearchInput input = factory; // implicit conversion

// Or use directly in method calls (implicit conversion)
await collection.Query.Hybrid(
    query: "search",
    vectors: b => b.Sum(("title", vec1), ("desc", vec2))
);

// All combination methods work with FactoryFn
VectorSearchInput.FactoryFn sumFactory = b => b.Sum(("a", vec1), ("b", vec2));
VectorSearchInput.FactoryFn avgFactory = b => b.Average(("a", vec1), ("b", vec2));
VectorSearchInput.FactoryFn minFactory = b => b.Minimum(("a", vec1), ("b", vec2));
VectorSearchInput.FactoryFn manualFactory = b => b.ManualWeights(("a", 0.7, vec1), ("b", 0.3, vec2));
VectorSearchInput.FactoryFn relFactory = b => b.RelativeScore(("a", 0.8, vec1), ("b", 0.2, vec2));
```

---

## HybridVectorInput

Discriminated union for hybrid search vector inputs. Can hold exactly one of: `VectorSearchInput`, `NearTextInput`, or `NearVectorInput`. Used as the `vectors` parameter in Hybrid search methods.

### Factory Methods

```csharp
HybridVectorInput.FromVectorSearch(VectorSearchInput vectorSearch)
HybridVectorInput.FromNearText(NearTextInput nearText)
HybridVectorInput.FromNearVector(NearVectorInput nearVector)
```

### Implicit Conversions

| From Type             | Example                                     |
|-----------------------|---------------------------------------------|
| `VectorSearchInput`   | `new VectorSearchInput { { "name", vec } }` |
| `NearTextInput`       | `new NearTextInput("banana")`               |
| `NearVectorInput`     | `new NearVectorInput(vectors)`              |
| `float[]`             | `new[] { 1f, 2f, 3f }`                      |
| `double[]`            | `new[] { 1.0, 2.0, 3.0 }`                   |
| `Vectors`             | `new Vectors { { "name", vec } }`           |
| `Vector`              | `new Vector(...)`                           |
| `NamedVector`         | `new NamedVector("name", values)`           |
| `string`              | `"search text"` (creates NearText)          |
| `(string, float[])`   | `("name", new[] { 1f, 2f })`                |
| `(string, double[])`  | `("name", new[] { 1.0, 2.0 })`              |
| `(string, float[,])`  | `("colbert", multiVectorArray)` for ColBERT |
| `(string, double[,])` | `("colbert", multiVectorArray)` for ColBERT |

**Examples:**

```csharp
// Implicit from float array
await collection.Query.Hybrid(query: "text", vectors: new[] { 1f, 2f, 3f });

// Implicit from string (becomes NearText)
await collection.Query.Hybrid(query: null, vectors: (HybridVectorInput)"semantic search");

// Implicit from NearTextInput
await collection.Query.Hybrid(query: null, vectors: new NearTextInput("banana"));

// Implicit from VectorSearchInput
await collection.Query.Hybrid(query: null, vectors: new VectorSearchInput { { "title", vec } });

// Implicit from tuple
await collection.Query.Hybrid(query: null, vectors: ("myVector", new[] { 1f, 2f }));

// Explicit factory method
await collection.Query.Hybrid(
    query: null,
    vectors: HybridVectorInput.FromVectorSearch(vectorSearchInput)
);
```

---

## HybridInput (Legacy)

Central type for hybrid search parameters. Contains the query text and vector inputs. Supports implicit conversions from common types.

### Implicit Conversions

| From Type                            | Example                        | Description                 |
|--------------------------------------|--------------------------------|-----------------------------|
| `string`                             | `"search query"`               | Text-only search            |
| `VectorSearchInput`                  | `new[] { 1f, 2f, 3f }`         | Vector-only search          |
| `NearTextInput`                      | `new NearTextInput("banana")`  | Server-side vectorization   |
| `NearVectorInput`                    | `new NearVectorInput(vectors)` | Vector search with metadata |
| `(string, VectorSearchInput)`        | `("query", vectors)`           | Text + vectors              |
| `(NearTextInput, VectorSearchInput)` | `(nearText, vectors)`          | Near-text + vectors         |
| `(string, TargetVectors)`            | `("query", targetVectors)`     | Text with target vectors    |

**Direct Construction:**

```csharp
// Text + vectors
var input = new HybridInput(
    query: "search query",
    vectors: new VectorSearchInput { ... }
);

// Near-text + vectors
var input = new HybridInput(
    nearText: new NearTextInput("banana"),
    vectors: new VectorSearchInput { ... }
);

// Near-vector only
var input = new HybridInput(
    nearVector: new NearVectorInput(vectors)
);
```

**Key Features:**
- Query can be either plain text (`query`) or server-vectorized (`NearTextInput`)
- Vectors can be provided as `VectorSearchInput` or `NearVectorInput`
- `NearTextInput` and `NearVectorInput` cannot be used together (throws `ArgumentException`)
- Target vectors for text search are specified in `NearTextInput.TargetVectors`
- Target vectors for vector search are embedded in `VectorSearchInput`

---

## NearVectorInput

Wrapper for vector input in hybrid or near-vector searches with optional thresholds.

```csharp
// From VectorSearchInput (implicit conversion)
NearVectorInput input = new[] { 1f, 2f, 3f };

// With thresholds
new NearVectorInput(
    Vector: vectorSearchInput,
    Certainty: 0.8f,
    Distance: null
)

// With lambda builder
new NearVectorInput(
    v => v.Sum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
)
```

---

## NearTextInput

Server-side vectorization for hybrid or near-text searches.

```csharp
// Simple text (implicit conversion from string)
NearTextInput input = "banana";

// With target vectors (using static factory)
new NearTextInput(
    Query: "banana",
    TargetVectors: TargetVectors.Sum("title", "description")
)

// With target vectors (using implicit conversion from string[])
new NearTextInput(
    Query: "banana",
    TargetVectors: new[] { "title", "description" }
)

// With move parameters
new NearTextInput(
    Query: "banana",
    MoveTo: new Move("apple", force: 0.5f),
    MoveAway: new Move("orange", force: 0.3f)
)

// With thresholds
new NearTextInput(
    Query: "banana",
    Certainty: 0.8f,
    Distance: null
)
```

**Important**: For hybrid searches, target vectors should be specified in `NearTextInput.TargetVectors`, not passed separately. This is the single source of truth for text-based target vectors.

---

## TargetVectors

Specifies which named vectors to target in a multi-vector search and how to combine them.

**Note**: Use static factory methods or implicit conversion instead of constructors directly.

### Static Factory Methods

```csharp
// Simple combinations from string array
TargetVectors targets = TargetVectors.Sum("title", "description");
TargetVectors targets = TargetVectors.Average("title", "description");
TargetVectors targets = TargetVectors.Minimum("title", "description");

// Implicit conversion from string array (no combination method)
TargetVectors targets = new[] { "title", "description" };

// Weighted combinations
TargetVectors targets = TargetVectors.ManualWeights(
    ("title", 1.5),
    ("description", 0.5)
);

TargetVectors targets = TargetVectors.RelativeScore(
    ("title", 0.7),
    ("description", 0.3)
);

// From VectorSearchInput
TargetVectors targets = TargetVectors.Sum(vectorSearchInput);
TargetVectors targets = TargetVectors.Average(vectorSearchInput);
TargetVectors targets = TargetVectors.Minimum(vectorSearchInput);
```

### TargetVectorsBuilder (Lambda Syntax)

Builder for creating target vectors with lambda expressions.

```csharp
// Sum combination
tv => tv.Sum("title", "description")

// Average combination
tv => tv.Average("title", "description")

// Minimum combination
tv => tv.Minimum("title", "description")

// Manual weights
tv => tv.ManualWeights(
    ("title", 1.5),
    ("description", 0.5)
)

// Relative score
tv => tv.RelativeScore(
    ("title", 0.7),
    ("description", 0.3)
)

// Targets without combination (for lambda use)
tv => tv.Targets("title", "description")
```

---

## HybridNearVector (Legacy)

> **Note**: This type has been renamed to `NearVectorInput`. See the NearVectorInput section above.

---

## HybridNearText (Legacy)

> **Note**: This type has been renamed to `NearTextInput`. See the NearTextInput section above.

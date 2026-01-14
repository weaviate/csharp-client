# Vector Search API Reference

Reference documentation for the vector search API in the Weaviate C# client.

## API Overview

| Client          | Method     | Description                                  |
|-----------------|------------|----------------------------------------------|
| QueryClient     | NearVector | Vector similarity search                     |
| QueryClient     | NearText   | Text-based semantic search                   |
| QueryClient     | NearMedia  | Media-based search (Image/Video/Audio/etc)   |
| QueryClient     | Hybrid     | Combined keyword + vector search             |
| GenerateClient  | NearVector | Vector search with generative AI             |
| GenerateClient  | NearText   | Text search with generative AI               |
| GenerateClient  | NearMedia  | Media search with generative AI              |
| GenerateClient  | Hybrid     | Hybrid search with generative AI             |
| AggregateClient | NearVector | Vector search with aggregation               |
| AggregateClient | NearText   | Text search with aggregation                 |
| AggregateClient | NearMedia  | Media search with aggregation                |
| AggregateClient | Hybrid     | Hybrid search with aggregation               |

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
    v => v.TargetVectorsSum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - ManualWeights
await collection.Query.NearVector(
    v => v.TargetVectorsManualWeights(
        ("title", 1.2, new[] { 1f, 2f }),
        ("description", 0.8, new[] { 3f, 4f })
    )
);

// Lambda builder - Average
await collection.Query.NearVector(
    v => v.TargetVectorsAverage(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - Minimum
await collection.Query.NearVector(
    v => v.TargetVectorsMinimum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - RelativeScore
await collection.Query.NearVector(
    v => v.TargetVectorsRelativeScore(
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
    AutoArray<string> query,
    ...
)

// With GroupBy
Task<GroupByResult> NearText(
    AutoArray<string> query,
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
    NearTextInput query,
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
    moveTo: new Move("fruit", 0.5f),
    moveAway: new Move("vegetable", 0.3f)
);

// Lambda builder - with target vectors (Sum)
await collection.Query.NearText(
    q => q(["banana"], certainty: 0.7f)
        .TargetVectorsSum("title", "description")
);

// Lambda builder - with target vectors (Average)
await collection.Query.NearText(
    q => q(["tropical", "fruit"])
        .TargetVectorsAverage("title", "description", "category")
);

// Lambda builder - with target vectors (ManualWeights)
await collection.Query.NearText(
    q => q(["search query"])
        .TargetVectorsManualWeights(("title", 0.8), ("description", 0.2))
);

// Lambda builder - with move parameters and targets
await collection.Query.NearText(
    q => q(["banana"], moveTo: new Move("fruit", 0.5f))
        .TargetVectorsSum("title", "description")
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
    q => q(["banana"]).TargetVectorsSum("title", "description"),
    groupBy: new GroupByRequest("category", objectsPerGroup: 3)
);
```

---

## QueryClient.NearMedia

Media-based semantic search supporting Image, Video, Audio, Thermal, Depth, and IMU data.

### Method Signatures

```csharp
// Lambda builder (recommended)
Task<WeaviateResult> NearMedia(
    NearMediaInput.FactoryFn query,
    ...
)

// Lambda builder with GroupBy (recommended)
Task<GroupByResult> NearMedia(
    NearMediaInput.FactoryFn query,
    GroupByRequest groupBy,
    ...
)

// Direct byte[] input (alternative for non-lambda preference)
Task<WeaviateResult> NearMedia(
    byte[] media,
    NearMediaType mediaType,
    double? certainty = null,
    double? distance = null,
    TargetVectors.FactoryFn? targets = null,
    ...
)

// Direct byte[] input with GroupBy (alternative)
Task<GroupByResult> NearMedia(
    byte[] media,
    NearMediaType mediaType,
    GroupByRequest groupBy,
    double? certainty = null,
    double? distance = null,
    TargetVectors.FactoryFn? targets = null,
    ...
)
```

### Examples

#### Lambda Builder Approach (Recommended)

```csharp
// Simple image search (no target vectors)
await collection.Query.NearMedia(m => m.Image(imageBytes));

// Video search
await collection.Query.NearMedia(m => m.Video(videoBytes));

// Audio search
await collection.Query.NearMedia(m => m.Audio(audioBytes));

// Thermal search
await collection.Query.NearMedia(m => m.Thermal(thermalBytes));

// Depth search
await collection.Query.NearMedia(m => m.Depth(depthBytes));

// IMU search
await collection.Query.NearMedia(m => m.IMU(imuBytes));

// With certainty
await collection.Query.NearMedia(m => m.Image(imageBytes, certainty: 0.8f));

// With distance
await collection.Query.NearMedia(m => m.Image(imageBytes, distance: 0.3f));

// With target vectors - Sum
await collection.Query.NearMedia(
    m => m.Image(imageBytes).TargetVectorsSum("title", "description")
);

// With target vectors - Average
await collection.Query.NearMedia(
    m => m.Video(videoBytes, certainty: 0.7f).TargetVectorsAverage("visual", "audio", "metadata")
);

// With target vectors - ManualWeights
await collection.Query.NearMedia(
    m => m.Audio(audioBytes, distance: 0.3f)
        .TargetVectorsManualWeights(("title", 1.2), ("description", 0.8))
);

// With target vectors - Minimum
await collection.Query.NearMedia(
    m => m.Image(imageBytes).TargetVectorsMinimum("v1", "v2", "v3")
);

// With target vectors - RelativeScore
await collection.Query.NearMedia(
    m => m.Video(videoBytes)
        .TargetVectorsRelativeScore(("visual", 0.7), ("audio", 0.3))
);

// With GroupBy
await collection.Query.NearMedia(
    m => m.Image(imageBytes).TargetVectorsSum("visual", "semantic"),
    groupBy: new GroupByRequest("category", objectsPerGroup: 5)
);

// With filters and other parameters
await collection.Query.NearMedia(
    m => m.Image(imageBytes, certainty: 0.8f).TargetVectorsSum("v1", "v2"),
    filters: Filter.ByProperty("status").Equal("active"),
    limit: 10,
    offset: 0,
    autoLimit: 3,
    returnMetadata: new MetadataQuery { Distance = true, Certainty = true }
);
```

#### Alternative: Direct byte[] Approach

For those who prefer not to use lambdas, you can use the direct byte[] overloads:

```csharp
// Simple search without target vectors
await collection.Query.NearMedia(
    imageBytes,
    NearMediaType.Image,
    certainty: 0.8
);

// With target vectors using TargetVectors.FactoryFn
await collection.Query.NearMedia(
    imageBytes,
    NearMediaType.Image,
    certainty: 0.8,
    targetVectors: t => t.TargetVectorsSum("title", "description")
);

// Video with distance threshold
await collection.Query.NearMedia(
    videoBytes,
    NearMediaType.Video,
    distance: 0.3
);

// With GroupBy
await collection.Query.NearMedia(
    imageBytes,
    NearMediaType.Image,
    groupBy: new GroupByRequest("category", objectsPerGroup: 5),
    certainty: 0.8
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
    v => v.NearVector().TargetVectorsSum(
        ("title", new[] { 1f, 2f }),
        ("description", new[] { 3f, 4f })
    )
);

// Lambda builder - NearVector with ManualWeights
await collection.Query.Hybrid(
    "search query",
    v => v.NearVector(certainty: 0.7f).TargetVectorsManualWeights(
        ("title", 1.2, new[] { 1f, 2f }),
        ("description", 0.8, new[] { 3f, 4f })
    )
);

// NearText with server-side vectorization (implicit conversion to HybridVectorInput)
await collection.Query.Hybrid(
    query: null,  // Keyword search query (null for vector-only search)
    vectors: new NearTextInput("banana")  // Implicitly converts to HybridVectorInput
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
    vectors: v => v.NearText(["banana"], certainty: 0.7f).Sum("title", "description")
);

// NearVector with NearVectorInput record (with certainty)
await collection.Query.Hybrid(
    query: null,
    vectors: new NearVectorInput(
        new[] { 1f, 2f, 3f },
        Certainty: 0.8f
    )
);

// NearVector with multi-target vectors using NearVectorInput
await collection.Query.Hybrid(
    query: "search query",
    vectors: new NearVectorInput(
        VectorSearchInput.Combine(
            TargetVectors.ManualWeights(("title", 0.7), ("description", 0.3)),
            ("title", new[] { 1f, 2f }),
            ("description", new[] { 3f, 4f })
        ),
        Distance: 0.5f
    )
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

// NearMedia with generation
await collection.Generate.NearMedia(
    m => m.Image(imageBytes),
    singlePrompt: "Describe this image"
);

// NearMedia with target vectors and generation
await collection.Generate.NearMedia(
    m => m.Video(videoBytes, certainty: 0.8f).Sum("visual", "audio"),
    groupedTask: new GroupedTask("Summarize all videos")
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

// NearMedia with aggregation
await collection.Aggregate.NearMedia(
    m => m.Image(imageBytes).Sum("visual", "semantic"),
    returnMetrics: [Metrics.ForProperty("category").Text(count: true)]
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

### NearMediaInput

Input for near-media searches supporting multiple media types with optional target vectors.

**Properties:**
- `Media` - Media content as byte array
- `Type` - Media type (Image, Video, Audio, Thermal, Depth, IMU)
- `TargetVectors` - Target vectors for multi-vector collections
- `Certainty` - Minimum certainty (0-1)
- `Distance` - Maximum distance

**Lambda Builder (via FactoryFn):**

```csharp
// Simple media without targets
m => m.Image(imageBytes)
m => m.Video(videoBytes)
m => m.Audio(audioBytes)

// With certainty/distance
m => m.Image(imageBytes, certainty: 0.8f)
m => m.Video(videoBytes, distance: 0.3f)

// With target vectors - Sum
m => m.Image(imageBytes).Sum("visual", "semantic")

// With target vectors - ManualWeights
m => m.Audio(audioBytes, certainty: 0.7f)
    .ManualWeights(("title", 1.2), ("description", 0.8))

// With target vectors - Average
m => m.Video(videoBytes).Average("visual", "audio", "metadata")

// With target vectors - Minimum
m => m.Image(imageBytes).Minimum("v1", "v2", "v3")

// With target vectors - RelativeScore
m => m.Thermal(thermalBytes)
    .RelativeScore(("thermal", 0.7), ("visual", 0.3))
```

**Direct Construction:**

```csharp
// Simple
new NearMediaInput(imageBytes, NearMediaType.Image)

// With targets
new NearMediaInput(
    imageBytes,
    NearMediaType.Image,
    TargetVectors: TargetVectors.Sum("visual", "semantic"),
    Certainty: 0.8f
)
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

4. **For NearMedia**, use lambda builder for concise syntax:
   ```csharp
   // Recommended
   await collection.Query.NearMedia(m => m.Image(imageBytes).Sum("v1", "v2"));

   // Alternative (for non-lambda preference)
   await collection.Query.NearMedia(
       imageBytes,
       NearMediaType.Image,
       targets: t => t.Sum("v1", "v2")
   );
   ```

5. **Target vector combinations**: Choose the right method for your use case
   - `Sum()` - Add all vectors (default, best for most cases)
   - `Average()` - Average all vectors (normalized sum)
   - `ManualWeights()` - Custom weights per vector
   - `Minimum()` - Minimum value across vectors
   - `RelativeScore()` - Weighted combination based on scores

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

# Weaviate.Client.Orm

A declarative ORM layer for the Weaviate C# client, providing attribute-based schema definition and type-safe LINQ-style queries.

## Status

✅ **Production Ready** - Complete ORM implementation with 100% feature parity with manual CollectionConfig creation.

## Features

### ✅ Fully Implemented
- **Attribute-based collection schema definition** - Define schemas with C# attributes
- **Property configuration** - All data types, indexing, tokenization
- **Named vector configuration** - 47+ vectorizer types with full customization
- **Vector indexes** - HNSW, Flat, Dynamic with all configuration options
- **Quantizers** - BQ, PQ, SQ, RQ with type-safe attributes
- **Multi-vector (ColBERT)** - Encoding configuration for multi-vector embeddings
- **Generative AI (RAG)** - 15+ providers (OpenAI, Anthropic, Cohere, AWS, Azure, Google, etc.)
- **Rerankers** - 6 providers (Cohere, VoyageAI, JinaAI, Nvidia, ContextualAI, Transformers)
- **Reference definitions** - Single, multi, and ID-only references
- **Nested object support** - Nested properties with automatic type inference
- **Inverted index configuration** - Timestamps, null state, property length
- **Multi-tenancy** - Full multi-tenancy support with auto-tenant creation/activation
- **Sharding & Replication** - Direct configuration in collection attribute
- **Type-safe LINQ queries** - Lambda expressions with expression tree conversion
- **Vector search** - NearText, NearVector, Hybrid search
- **Object mapping** - Automatic vector/reference extraction and injection
- **Data operations** - Insert, InsertMany, Replace, Update, Delete with auto-mapping
- **Schema migrations** - Safe incremental updates with breaking change detection
- **Property name conversion** - PascalCase → camelCase (customizable)
- **ConfigMethod support** - Advanced customization for vectors, generative, and collections

## Quick Start

### 1. Define Your Model

```csharp
using Weaviate.Client.Orm.Attributes;
using Weaviate.Client.Models;

[WeaviateCollection("Articles", Description = "Blog articles")]
[InvertedIndex(IndexTimestamps = true)]
public class Article
{
    [Property(DataType.Text)]
    [Index(Filterable = true, Searchable = true)]
    [Tokenization(PropertyTokenization.Word)]
    public string Title { get; set; } = string.Empty;

    [Property(DataType.Text)]
    [Index(Searchable = true)]
    public string Content { get; set; } = string.Empty;

    [Property(DataType.Int)]
    [Index(Filterable = true, RangeFilters = true)]
    public int WordCount { get; set; }

    [Property(DataType.Date)]
    [Index(Filterable = true)]
    public DateTime PublishedAt { get; set; }

    // Named vector - property name becomes vector name
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-ada-002",
        Dimensions = 1536,
        SourceProperties = [nameof(Title), nameof(Content)]
    )]
    public float[]? TitleContentEmbedding { get; set; }

    // Reference to another collection
    [Reference("Category")]
    public Category? Category { get; set; }
}

[WeaviateCollection("Category")]
public class Category
{
    [Property(DataType.Text)]
    public string Name { get; set; } = string.Empty;
}
```

### 2. Create Collection from Class

```csharp
using Weaviate.Client.Orm.Extensions;

var client = new WeaviateClient(new WeaviateConfig { Host = "localhost:8080" });

// Create collection from class attributes
var collection = await client.Collections.CreateFromClass<Article>();
```

## Supported Attributes

### Collection-Level Attributes

- `[WeaviateCollection]` - Define collection name, description, and configuration
  - Name, Description
  - ShardingDesiredCount, ShardingVirtualPerPhysical, ShardingDesiredVirtualCount, ShardingKey
  - ReplicationFactor, ReplicationAsyncEnabled
  - MultiTenancyEnabled, AutoTenantCreation, AutoTenantActivation
  - CollectionConfigMethod for advanced customization
- `[InvertedIndex]` - Configure inverted index settings
  - IndexTimestamps, IndexNullState, IndexPropertyLength, CleanupIntervalSeconds
- `[Generative<TModule>]` - Configure generative AI (RAG) module
  - Supports 15+ providers: OpenAI, Anthropic, Cohere, AWS, Azure, Google, Mistral, Ollama, etc.
  - Common properties: Model, MaxTokens, Temperature, TopP, BaseURL
  - Provider-specific: ResourceName/DeploymentId (Azure), Region/Service (AWS), ProjectId (Google)
- `[Reranker<TModule>]` - Configure reranker module
  - Supports 6 providers: Cohere, VoyageAI, JinaAI, Nvidia, ContextualAI, Transformers
  - Properties: Model, BaseURL, Instruction, TopN

### Property-Level Attributes

- `[Property]` - Define Weaviate property with data type
  - DataType, Description, Name (custom property name)
- `[Index]` - Configure filtering, searching, and range filters
  - Filterable, Searchable, RangeFilters
- `[Tokenization]` - Specify tokenization strategy for text properties
  - Word, Lowercase, Whitespace, Field, Trigram, GSE, Kagome, etc.
- `[NestedType]` - Define nested object structure (optional, auto-inferred)
  - Only needed for polymorphic scenarios (interfaces/base classes)

### Vector Configuration

- `[Vector<TVectorizer>]` - Define named vector with vectorizer configuration
  - Property name becomes vector name (customizable with Name property)
  - Property type determines single (`float[]`) vs multi-vector (`float[,]`)
  - Supports all 47+ Weaviate vectorizer types
  - ConfigMethod for advanced customization
  - ConfigMethodClass for type-safe cross-class methods
- `[VectorIndex<TIndexConfig>]` - Configure vector index
  - HNSW: EfConstruction, MaxConnections, Ef, DynamicEfMin, DynamicEfMax, etc.
  - Flat: Simple flat index configuration
  - Dynamic: Threshold-based index selection
  - Distance metrics: Cosine, Dot, L2Squared, Hamming, Manhattan
- `[QuantizerBQ]` - Binary Quantization
  - Cache, RescoreLimit
- `[QuantizerPQ]` - Product Quantization
  - Segments, Centroids, Encoder configuration
- `[QuantizerSQ]` - Scalar Quantization
  - TrainingLimit, RescoreLimit
- `[QuantizerRQ]` - Residual Quantization
  - Bits, Cache, RescoreLimit
- `[Encoding]` - Multi-vector (ColBERT) encoding
  - KSim, DProjections, Repetitions

### References

- `[Reference]` - Define cross-reference to another collection
  - Supports single references (`Category?`)
  - Supports ID-only references (`Guid?`)
  - Supports multi-references (`List<Article>?`)

## Supported Data Types

All Weaviate data types are supported:
- `DataType.Text`, `DataType.TextArray`
- `DataType.Int`, `DataType.IntArray`
- `DataType.Number`, `DataType.NumberArray`
- `DataType.Bool`, `DataType.BoolArray`
- `DataType.Date`, `DataType.DateArray`
- `DataType.Uuid`, `DataType.UuidArray`
- `DataType.GeoCoordinate`
- `DataType.PhoneNumber`
- `DataType.Blob`
- `DataType.Object`, `DataType.ObjectArray`

## Supported Vectorizers

All Weaviate vectorizers are supported via generic type parameter:

**Text Vectorizers:**
- `Vectorizer.Text2VecOpenAI`
- `Vectorizer.Text2VecCohere`
- `Vectorizer.Text2VecHuggingFace`
- `Vectorizer.Text2VecTransformers`
- `Vectorizer.Text2VecAWS`
- `Vectorizer.Text2VecGoogle`
- `Vectorizer.Text2VecJinaAI`
- `Vectorizer.Text2VecOllama`
- And 10+ more...

**Multi-Modal Vectorizers:**
- `Vectorizer.Multi2VecClip`
- `Vectorizer.Multi2VecCohere`
- `Vectorizer.Multi2VecBind`
- `Vectorizer.Multi2VecGoogle`
- And more...

**Special Vectorizers:**
- `Vectorizer.SelfProvided` - For user-provided vectors
- `Vectorizer.Ref2VecCentroid` - For reference-based vectorization

## Advanced Examples

### Vector Index with Quantization

```csharp
[WeaviateCollection("Documents")]
public class Document
{
    [Property(DataType.Text)]
    public string Content { get; set; } = string.Empty;

    // Vector with HNSW index and BQ quantization
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-large",
        SourceProperties = [nameof(Content)]
    )]
    [VectorIndex<VectorIndexConfig.Hnsw>(
        DistanceMetric = DistanceMetric.Cosine,
        EfConstruction = 128,
        MaxConnections = 64
    )]
    [QuantizerBQ(Cache = true, RescoreLimit = 200)]
    public float[]? ContentEmbedding { get; set; }
}
```

### Generative AI (RAG) Configuration

```csharp
[WeaviateCollection("KnowledgeBase")]
[Generative<GenerativeConfig.OpenAI>(
    Model = "gpt-4",
    MaxTokens = 500,
    Temperature = 0.7
)]
[Reranker<Reranker.Cohere>(Model = "rerank-english-v2.0")]
public class KnowledgeArticle
{
    [Property(DataType.Text)]
    public string Title { get; set; } = string.Empty;

    [Property(DataType.Text)]
    public string Content { get; set; } = string.Empty;

    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-ada-002",
        SourceProperties = [nameof(Content)]
    )]
    public float[]? ContentVector { get; set; }
}
```

### Multi-Tenancy Configuration

```csharp
[WeaviateCollection(
    "TenantData",
    MultiTenancyEnabled = true,
    AutoTenantCreation = true,
    AutoTenantActivation = true
)]
public class TenantDocument
{
    [Property(DataType.Text)]
    public string Title { get; set; } = string.Empty;

    [Property(DataType.Text)]
    public string Content { get; set; } = string.Empty;
}
```

### Sharding and Replication

```csharp
[WeaviateCollection(
    "HighAvailabilityData",
    ShardingDesiredCount = 3,
    ReplicationFactor = 2,
    ReplicationAsyncEnabled = true
)]
public class HADocument
{
    [Property(DataType.Text)]
    public string Title { get; set; } = string.Empty;

    [Property(DataType.Text)]
    public string Content { get; set; } = string.Empty;
}
```

### Multi-Vector Collection

```csharp
[WeaviateCollection("Products")]
public class Product
{
    [Property(DataType.Text)]
    public string Name { get; set; } = string.Empty;

    [Property(DataType.Text)]
    public string Description { get; set; } = string.Empty;

    [Property(DataType.Blob)]
    public byte[]? ProductImage { get; set; }

    // Text-only vector
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-ada-002",
        SourceProperties = [nameof(Name), nameof(Description)]
    )]
    public float[]? TextEmbedding { get; set; }

    // Multi-modal vector (text + image)
    [Vector<Vectorizer.Multi2VecClip>(
        TextFields = [nameof(Name), nameof(Description)],
        ImageFields = [nameof(ProductImage)]
    )]
    public float[]? MultiModalEmbedding { get; set; }

    // Custom vector you provide
    [Vector<Vectorizer.SelfProvided>()]
    public float[]? CustomEmbedding { get; set; }
}
```

### Nested Objects

```csharp
[WeaviateCollection("BlogPost")]
public class BlogPost
{
    [Property(DataType.Text)]
    public string Title { get; set; } = string.Empty;

    [Property(DataType.Object)]
    [NestedType(typeof(Author))]
    public Author Author { get; set; } = new();

    [Property(DataType.ObjectArray)]
    [NestedType(typeof(Comment))]
    public List<Comment> Comments { get; set; } = new();
}

public class Author
{
    [Property(DataType.Text)]
    public string Name { get; set; } = string.Empty;

    [Property(DataType.Text)]
    public string Email { get; set; } = string.Empty;
}

public class Comment
{
    [Property(DataType.Text)]
    public string Text { get; set; } = string.Empty;

    [Property(DataType.Date)]
    public DateTime PostedAt { get; set; }
}
```

### ConfigMethod for Advanced Customization

```csharp
[WeaviateCollection("Articles")]
public class Article
{
    [Property(DataType.Text)]
    public string Title { get; set; } = string.Empty;

    // Use ConfigMethod for properties not available as attribute parameters
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-ada-002",
        SourceProperties = [nameof(Title)],
        ConfigMethod = nameof(CustomizeVector)
    )]
    public float[]? TitleEmbedding { get; set; }

    // ConfigMethod signature: static TVectorizer MethodName(string vectorName, TVectorizer prebuilt)
    private static Vectorizer.Text2VecOpenAI CustomizeVector(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt)
    {
        // Access any property not exposed in attribute
        prebuilt.VectorizeCollectionName = false;
        return prebuilt;
    }
}
```

### Schema Migrations

```csharp
// Check for schema changes without applying them
var plan = await client.Collections.CheckMigrate<Article>();
Console.WriteLine(plan.GetSummary());
// Output:
// Migration plan for 'Article' (2 changes):
//   ✓ AddProperty: Add property 'tags' (TEXT_ARRAY)
//   ✓ AddVector: Add vector 'contentEmbedding'

if (plan.HasChanges)
{
    if (plan.IsSafe)
    {
        // Apply safe (additive) changes
        await client.Collections.Migrate<Article>();
        Console.WriteLine("Migration applied successfully");
    }
    else
    {
        // Breaking changes detected - require explicit confirmation
        Console.WriteLine("Breaking changes detected:");
        Console.WriteLine(plan.GetSummary());

        // Apply with allowBreakingChanges=true (USE WITH CAUTION)
        await client.Collections.Migrate<Article>(allowBreakingChanges: true);
    }
}
```

## Query Builder API (OrmQueryClient)

The `OrmQueryClient<T>` provides a fluent, type-safe API for building Weaviate queries. Access it via `collection.Query<T>()`. All methods are chainable.

### Basic Usage

```csharp
var collection = await client.Collections.CreateFromClass<Article>();

// Simple query
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 100)
    .Limit(10)
    .ExecuteAsync();
```

### Filter Methods

#### `Where(Expression<Func<T, bool>> predicate)`
Type-safe filtering with lambda expressions. Multiple `Where` calls are combined with AND logic.

**Supported Operators:**
- Comparison: `==`, `!=`, `>`, `<`, `>=`, `<=`
- Logical: `&&` (AND), `||` (OR)
- Methods: `.Contains()`, `.ContainsAny()`, `.ContainsAll()`
- Nested properties: `a.Category.Name`

```csharp
// Single condition
.Where(a => a.WordCount > 100)

// Multiple conditions (AND)
.Where(a => a.WordCount > 100 && a.PublishedAt > DateTime.UtcNow.AddDays(-7))

// Nested properties
.Where(a => a.Category.Name == "Technology")

// String operations
.Where(a => a.Tags.Contains("AI"))
```

### Vector Search Methods

#### `NearText(string text, Expression<Func<T, object>>? vector, float? certainty, float? distance)`
Text-to-vector search using Weaviate's vectorizers.

```csharp
// Basic near text
.NearText("artificial intelligence")

// With named vector
.NearText("machine learning", vector: a => a.TitleEmbedding)

// With certainty threshold (0-1, higher = more certain)
.NearText("AI", certainty: 0.7f)

// With distance threshold (lower = more similar)
.NearText("neural networks", distance: 0.3f)
```

#### `NearVector(float[] vectorValues, Expression<Func<T, object>>? vector, float? certainty, float? distance)`
Vector similarity search with a provided vector.

```csharp
float[] queryVector = GetEmbedding("my query");

.NearVector(queryVector)
.NearVector(queryVector, vector: a => a.ContentEmbedding)
.NearVector(queryVector, certainty: 0.8f)
```

#### `Hybrid(string query, Expression<Func<T, object>>? vector, float? alpha)`
Combines BM25 keyword search with vector search.

**Alpha:** `0.0` = keyword only, `0.5` = balanced, `1.0` = vector only

```csharp
// Balanced hybrid
.Hybrid("machine learning")

// Favor keyword search
.Hybrid("specific terms", alpha: 0.3f)

// Favor vector search
.Hybrid("conceptual query", alpha: 0.8f)
```

### Result Control Methods

#### `Limit(uint limit)` - Limit results
```csharp
.Limit(10)  // Top 10 results
```

#### `Sort<TProp>(Expression<Func<T, TProp>> property, bool descending)` - Sort results
```csharp
.Sort(a => a.PublishedAt)               // Ascending
.Sort(a => a.PublishedAt, descending: true)  // Descending
```

### Include Methods

#### `WithVectors(params Expression<Func<T, object>>[] vectors)` - Include vectors in results
Without this, vector properties return null.

```csharp
.WithVectors(a => a.TitleEmbedding)
.WithVectors(a => a.TitleEmbedding, a => a.ContentEmbedding)

// Vectors are populated
foreach (var article in results)
{
    float[]? embedding = article.TitleEmbedding; // Not null!
}
```

#### `WithReferences(params Expression<Func<T, object>>[] references)` - Expand references
Populates reference properties with full objects.

```csharp
.WithReferences(a => a.Category)
.WithReferences(a => a.Category, a => a.Author)

// References are populated
foreach (var article in results)
{
    Category? category = article.Category; // Fully hydrated!
}
```

#### `Select(Expression<Func<T, object>> selector)` - Property projection
```csharp
.Select(a => new { a.Title, a.PublishedAt })
```

#### `WithMetadata(MetadataQuery metadata)` - Include metadata
Requires `ExecuteWithMetadataAsync()`.

```csharp
.WithMetadata(MetadataQuery.Distance | MetadataQuery.Certainty)

var results = await query.ExecuteWithMetadataAsync();
foreach (var obj in results.Objects)
{
    Console.WriteLine($"Distance: {obj.Metadata?.Distance}");
}
```

### Execution Methods

#### `ExecuteAsync()` - Returns `IEnumerable<T>`
Standard execution with automatic object mapping.

```csharp
IEnumerable<Article> results = await collection.Query<Article>()
    .Where(a => a.WordCount > 100)
    .ExecuteAsync();
```

#### `ExecuteWithMetadataAsync()` - Returns `WeaviateResult<WeaviateObject<T>>`
Returns full result with metadata (distance, certainty, etc.).

```csharp
var result = await collection.Query<Article>()
    .NearText("AI")
    .WithMetadata(MetadataQuery.Distance | MetadataQuery.Certainty)
    .ExecuteWithMetadataAsync();

foreach (var obj in result.Objects)
{
    Console.WriteLine($"{obj.Properties.Title}");
    Console.WriteLine($"  Distance: {obj.Metadata?.Distance}");
    Console.WriteLine($"  Certainty: {obj.Metadata?.Certainty}");
}
```

### Complete Query Examples

#### Complex Filtered Query
```csharp
var articles = await collection.Query<Article>()
    .Where(a => a.WordCount >= 500 && a.WordCount <= 2000)
    .Where(a => a.PublishedAt > DateTime.UtcNow.AddMonths(-6))
    .Where(a => a.Category.Name == "Technology")
    .Sort(a => a.PublishedAt, descending: true)
    .Limit(50)
    .ExecuteAsync();
```

#### Vector Search with Full Context
```csharp
var relevant = await collection.Query<Article>()
    .Where(a => a.WordCount > 300)
    .NearText("machine learning", vector: a => a.ContentEmbedding, certainty: 0.75f)
    .WithReferences(a => a.Category, a => a.Author)
    .WithVectors(a => a.ContentEmbedding)
    .Sort(a => a.PublishedAt, descending: true)
    .Limit(20)
    .ExecuteAsync();

foreach (var article in relevant)
{
    Console.WriteLine($"{article.Title} by {article.Author?.Name}");
    Console.WriteLine($"Category: {article.Category?.Name}");
    Console.WriteLine($"Embedding: {article.ContentEmbedding?.Length} dims");
}
```

#### Hybrid Search with Metadata
```csharp
var results = await collection.Query<Article>()
    .Hybrid("neural networks deep learning", alpha: 0.6f)
    .Where(a => a.WordCount > 200)
    .WithMetadata(MetadataQuery.Score | MetadataQuery.Distance)
    .Limit(15)
    .ExecuteWithMetadataAsync();

foreach (var obj in results.Objects)
{
    Console.WriteLine($"{obj.Properties.Title}");
    Console.WriteLine($"  Score: {obj.Metadata?.Score}");
    Console.WriteLine($"  Distance: {obj.Metadata?.Distance}");
}
```

## Complete Documentation

For comprehensive guides and examples, see:
- **[ORM Guide](../../docs/orm_guide.md)** - Complete usage guide with detailed examples
- **[ORM Status](../../docs/orm_status.md)** - Implementation status and feature list
- **[ORM Changelog](../../docs/orm_changelog.md)** - Development history and decisions

## Dependencies

- `Weaviate.Client` - Core Weaviate client (referenced as project dependency)
- `Humanizer.Core` - String transformations (PascalCase → camelCase)

## License

Same as Weaviate C# client - BSD 3-Clause

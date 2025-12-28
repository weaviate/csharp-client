# CollectionMapper - Getting Started Guide

Welcome to the Weaviate CollectionMapper! This guide will help you get up and running with type-safe, attribute-based schema definition and LINQ-style queries for Weaviate.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Basic Concepts](#basic-concepts)
- [Schema Definition](#schema-definition)
- [Data Operations](#data-operations)
- [Querying](#querying)
- [Advanced Topics](#advanced-topics)

---

## Installation

### Prerequisites

- .NET 8.0 or .NET 9.0
- Weaviate instance (local or cloud)

### Add Package References

```xml
<ItemGroup>
  <ProjectReference Include="..\Weaviate.Client\Weaviate.Client.csproj" />
  <ProjectReference Include="..\Weaviate.Client.CollectionMapper\Weaviate.Client.CollectionMapper.csproj" />
</ItemGroup>
```

> **Note**: CollectionMapper will be available as a NuGet package in the future.

---

## Quick Start

Here's a complete example to get you started in 5 minutes:

```csharp
using Weaviate.Client;
using Weaviate.Client.CollectionMapper;
using Weaviate.Client.CollectionMapper.Attributes;
using Weaviate.Client.Models;

// 1. Define your entity with attributes
[WeaviateCollection("Articles")]
public class Article
{
    [Property(DataType.Text)]
    public string Title { get; set; } = "";

    [Property(DataType.Text)]
    public string Content { get; set; } = "";

    [Property(DataType.Int)]
    public int WordCount { get; set; }

    [Property(DataType.Date)]
    public DateTime PublishedAt { get; set; }

    [Vector<Vectorizer.Text2VecOpenAI>(Model = "ada-002")]
    public float[]? Embedding { get; set; }
}

// 2. Connect to Weaviate
var client = new WeaviateClient("http://localhost:8080");

// 3. Create the collection from your class
var collection = await client.Collections.CreateFromClass<Article>();

// 4. Insert data
var article = new Article
{
    Title = "Getting Started with Weaviate",
    Content = "Weaviate is a vector database...",
    WordCount = 1500,
    PublishedAt = DateTime.UtcNow
};

await collection.Data.Insert(article);

// 5. Query with LINQ-style syntax
var results = await collection.Query<Article>()
    .NearText("vector databases")
    .Where(a => a.WordCount > 1000)
    .Limit(10)
    .ExecuteAsync();

foreach (var result in results.Objects)
{
    Console.WriteLine($"{result.Properties.Title} ({result.Properties.WordCount} words)");
}
```

---

## Basic Concepts

### What is CollectionMapper?

CollectionMapper is a type-safe layer on top of the Weaviate C# client that provides:

✨ **Attribute-Based Schema Definition** - Define your Weaviate schema using C# attributes
✨ **Automatic Object Mapping** - Convert between C# objects and Weaviate objects
✨ **Type-Safe LINQ Queries** - Write queries using familiar C# LINQ expressions
✨ **Vector Management** - Automatic handling of vectors and named vectors
✨ **Reference Handling** - Automatic resolution of cross-references

### Architecture

```
┌─────────────────────────────────────┐
│  Your C# Application                │
├─────────────────────────────────────┤
│  CollectionMapper (Type-Safe Layer) │
│  - Attribute Processing             │
│  - Object Mapping                   │
│  - LINQ Query Building              │
├─────────────────────────────────────┤
│  Weaviate.Client (Core Client)      │
│  - gRPC/REST Communication          │
│  - API Models                       │
├─────────────────────────────────────┤
│  Weaviate Database                  │
└─────────────────────────────────────┘
```

### Key Benefits

| Feature | Without CollectionMapper | With CollectionMapper |
|---------|--------------------------|----------------------|
| Schema Definition | Manual JSON/code | Declarative attributes |
| Type Safety | Runtime errors | Compile-time checking |
| Query Building | String concatenation | LINQ expressions |
| Object Mapping | Manual conversion | Automatic |
| Refactoring | Error-prone | Safe & easy |

---

## Schema Definition

### Collection Attributes

#### `[WeaviateCollection]`

Specifies the Weaviate collection name:

```csharp
[WeaviateCollection("Articles")]  // Collection will be named "Articles"
public class Article { }

// If omitted, class name is used:
public class Article { }  // Collection will be named "Article"
```

**Additional Options:**
```csharp
[WeaviateCollection(
    "Products",
    Description = "Product catalog",
    InvertedIndexConfig = new InvertedIndexConfig { /* ... */ }
)]
public class Product { }
```

---

### Property Attributes

#### `[Property]`

Defines a Weaviate property:

```csharp
public class Article
{
    [Property(DataType.Text)]
    public string Title { get; set; }

    [Property(DataType.Text)]
    public string Content { get; set; }

    [Property(DataType.Int)]
    public int WordCount { get; set; }

    [Property(DataType.Number)]
    public double Rating { get; set; }

    [Property(DataType.Bool)]
    public bool IsPublished { get; set; }

    [Property(DataType.Date)]
    public DateTime PublishedAt { get; set; }

    [Property(DataType.Text, IsArray = true)]
    public string[] Tags { get; set; }
}
```

**Supported Data Types:**
- `DataType.Text` → `string`
- `DataType.Int` → `int`, `long`
- `DataType.Number` → `float`, `double`
- `DataType.Bool` → `bool`
- `DataType.Date` → `DateTime`
- `DataType.Uuid` → `Guid`
- `DataType.Blob` → `byte[]`
- `DataType.GeoCoordinates` → Custom type

**Array Properties:**
```csharp
[Property(DataType.Text, IsArray = true)]
public string[] Tags { get; set; }

[Property(DataType.Int, IsArray = true)]
public int[] Ratings { get; set; }
```

---

### Vector Attributes

#### `[Vector<TVectorizer>]`

Defines a vector property with automatic vectorization:

```csharp
public class Article
{
    // OpenAI embedding
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "ada-002",
        Dimensions = 1536,
        TextFields = new[] { "title", "content" }
    )]
    public float[]? Embedding { get; set; }

    // Cohere embedding
    [Vector<Vectorizer.Text2VecCohere>(
        Model = "embed-english-v3.0"
    )]
    public float[]? CohereEmbedding { get; set; }

    // Hugging Face
    [Vector<Vectorizer.Text2VecHuggingFace>(
        Model = "sentence-transformers/all-MiniLM-L6-v2"
    )]
    public float[]? HuggingFaceEmbedding { get; set; }
}
```

**Supported Vectorizers** (47+ total):

| Category | Vectorizers |
|----------|-------------|
| **Text** | OpenAI, Cohere, HuggingFace, Transformers, ContextionaryQuery, GPT4All, PaLM, Ollama, Jinaai, VoyageAI |
| **Multi-modal** | CLIP, Img2Vec, Bind, Multi2VecPaLM, Multi2VecCLIP, Multi2VecGoogleSTT |
| **Reference-based** | Ref2VecCentroid |

**Named Vectors:**
```csharp
public class Article
{
    [Vector<Vectorizer.Text2VecOpenAI>(
        TextFields = new[] { "title" }
    )]
    public float[]? TitleEmbedding { get; set; }  // Named: "titleEmbedding"

    [Vector<Vectorizer.Text2VecOpenAI>(
        TextFields = new[] { "content" }
    )]
    public float[]? ContentEmbedding { get; set; }  // Named: "contentEmbedding"
}
```

#### Vector Index Configuration

```csharp
[VectorIndex<VectorIndexType.HNSW>(
    DistanceMetric = VectorDistance.Cosine,
    EfConstruction = 128,
    MaxConnections = 64
)]
[Vector<Vectorizer.Text2VecOpenAI>()]
public float[]? Embedding { get; set; }
```

**Index Types:**
- `VectorIndexType.HNSW` - Hierarchical Navigable Small World (default, fast)
- `VectorIndexType.Flat` - Brute-force search (100% recall, slower)
- `VectorIndexType.Dynamic` - Auto-switches based on data size

**Distance Metrics:**
- `VectorDistance.Cosine` - Cosine similarity (default for most)
- `VectorDistance.Dot` - Dot product
- `VectorDistance.L2Squared` - Euclidean distance
- `VectorDistance.Hamming` - Hamming distance (binary vectors)
- `VectorDistance.Manhattan` - Manhattan distance

#### Quantization

Reduce memory usage with quantization:

```csharp
[Quantizer<QuantizerType.BQ>()]  // Binary Quantization
[Vector<Vectorizer.Text2VecOpenAI>()]
public float[]? Embedding { get; set; }
```

**Quantizer Types:**
- `QuantizerType.BQ` - Binary Quantization (up to 32x compression)
- `QuantizerType.PQ` - Product Quantization (configurable compression)
- `QuantizerType.SQ` - Scalar Quantization (4x compression)

---

### Reference Attributes

#### `[Reference]`

Defines a cross-reference to another collection:

```csharp
public class Article
{
    [Property(DataType.Text)]
    public string Title { get; set; }

    // Single reference
    [Reference(typeof(Author))]
    public Author? Author { get; set; }

    // Multiple references
    [Reference(typeof(Category), IsArray = true)]
    public List<Category>? Categories { get; set; }
}

public class Author
{
    [Property(DataType.Text)]
    public string Name { get; set; }
}

public class Category
{
    [Property(DataType.Text)]
    public string Name { get; set; }
}
```

**Reference-Only (No Nested Object):**
```csharp
[Reference(typeof(Author), ReferencesOnly = true)]
public Guid? AuthorId { get; set; }  // Just stores the UUID reference
```

---

## Data Operations

### Insert

```csharp
// Single insert
var article = new Article
{
    Title = "AI in 2024",
    Content = "...",
    WordCount = 2000,
    PublishedAt = DateTime.UtcNow
};

await collection.Data.Insert(article);
```

### Insert Many (Batch)

```csharp
var articles = new List<Article>
{
    new() { Title = "Article 1", Content = "...", WordCount = 1000 },
    new() { Title = "Article 2", Content = "...", WordCount = 1500 },
    new() { Title = "Article 3", Content = "...", WordCount = 2000 },
};

await collection.Data.InsertMany(articles);
```

### Update

```csharp
article.WordCount = 2100;
article.Content += "\n\nUpdated content...";

await collection.Data.Update(article, articleId);
```

### Replace

```csharp
var newArticle = new Article
{
    Title = "Completely New Title",
    Content = "Completely new content",
    WordCount = 3000,
    PublishedAt = DateTime.UtcNow
};

await collection.Data.Replace(newArticle, articleId);
```

### Delete

```csharp
await collection.Data.Delete(articleId);

// Batch delete
await collection.Data.DeleteMany(new[] { id1, id2, id3 });
```

---

## Querying

### Basic Queries

```csharp
// Fetch all (with limit)
var results = await collection.Query<Article>()
    .Limit(10)
    .ExecuteAsync();
```

### Filtering

```csharp
// Simple filter
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000)
    .Limit(10)
    .ExecuteAsync();

// Multiple filters (AND logic)
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000)
    .Where(a => a.IsPublished == true)
    .Limit(10)
    .ExecuteAsync();

// Complex expressions
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000 && a.Rating >= 4.0)
    .Limit(10)
    .ExecuteAsync();
```

**Supported Operators:**
- Comparison: `==`, `!=`, `>`, `>=`, `<`, `<=`
- Logical: `&&`, `||`, `!`
- String: `.Contains()`, `.StartsWith()`, `.EndsWith()`
- Collections: `.Any()`, `.All()`

### Vector Search

#### Near Text

```csharp
// Search by text (server-side vectorization)
var results = await collection.Query<Article>()
    .NearText("machine learning tutorials")
    .Limit(10)
    .ExecuteAsync();

// With target vector
var results = await collection.Query<Article>()
    .NearText("AI", vector: a => a.TitleEmbedding)
    .Limit(10)
    .ExecuteAsync();

// With certainty threshold
var results = await collection.Query<Article>()
    .NearText("AI", certainty: 0.7f)
    .Limit(10)
    .ExecuteAsync();
```

#### Near Vector

```csharp
// Search by vector
float[] queryVector = GetEmbeddingFromSomewhere();

var results = await collection.Query<Article>()
    .NearVector(queryVector)
    .Limit(10)
    .ExecuteAsync();

// With distance threshold
var results = await collection.Query<Article>()
    .NearVector(queryVector, distance: 0.3f)
    .Limit(10)
    .ExecuteAsync();
```

#### Hybrid Search

```csharp
// Combines keyword (BM25) and vector search
var results = await collection.Query<Article>()
    .Hybrid("machine learning", alpha: 0.5f)  // 0 = keyword, 1 = vector
    .Limit(10)
    .ExecuteAsync();

// With filter
var results = await collection.Query<Article>()
    .Hybrid("AI")
    .Where(a => a.PublishedAt > DateTime.Now.AddDays(-30))
    .Limit(10)
    .ExecuteAsync();
```

### Sorting

```csharp
// Sort ascending
var results = await collection.Query<Article>()
    .Sort(a => a.PublishedAt)
    .Limit(10)
    .ExecuteAsync();

// Sort descending
var results = await collection.Query<Article>()
    .Sort(a => a.PublishedAt, descending: true)
    .Limit(10)
    .ExecuteAsync();
```

### Including Vectors

```csharp
// Include specific vector
var results = await collection.Query<Article>()
    .WithVectors(a => a.Embedding)
    .Limit(10)
    .ExecuteAsync();

// Include multiple vectors
var results = await collection.Query<Article>()
    .WithVectors(a => a.TitleEmbedding)
    .WithVectors(a => a.ContentEmbedding)
    .Limit(10)
    .ExecuteAsync();

// Access vectors in results
foreach (var obj in results.Objects)
{
    var embedding = obj.Vectors?["embedding"];  // Named vector access
}
```

### Including References

```csharp
// Include single reference
var results = await collection.Query<Article>()
    .WithReferences(a => a.Author)
    .Limit(10)
    .ExecuteAsync();

// Include multiple references
var results = await collection.Query<Article>()
    .WithReferences(a => a.Author)
    .WithReferences(a => a.Categories)
    .Limit(10)
    .ExecuteAsync();

// Access references in results
foreach (var obj in results.Objects)
{
    var author = obj.Properties.Author;  // Nested object
    var categories = obj.Properties.Categories;  // List of nested objects
}
```

---

## Advanced Topics

### Generative Search (RAG)

```csharp
// Single result generation
var results = await collection.Generate<Article>()
    .NearText("quantum computing")
    .SinglePrompt("Summarize this article in 2 sentences")
    .Limit(5)
    .ExecuteAsync();

foreach (var obj in results.Objects)
{
    Console.WriteLine($"Article: {obj.Properties.Title}");
    Console.WriteLine($"Summary: {obj.Generated}");
}

// Grouped task
var result = await collection.Generate<Article>()
    .NearText("climate change")
    .GroupedTask("Create a report summarizing these articles")
    .Limit(10)
    .ExecuteAsync();

Console.WriteLine(result.GeneratedGrouped);
```

### Schema Migrations

```csharp
// Automatic migration detection
var plan = await collection.GenerateMigrationPlan<ArticleV2>();

// Review planned changes
foreach (var change in plan.Changes)
{
    Console.WriteLine($"{change.Type}: {change.Description}");
}

// Apply migration
await collection.Migrate<ArticleV2>();
```

**Migration Types Supported:**
- Add new properties
- Add new vectors
- Update vector configurations
- Update index configurations
- Remove properties/vectors (with confirmation)

### Custom Configuration Methods

```csharp
[WeaviateCollection("Articles")]
[CollectionConfigMethod(typeof(ArticleConfig), nameof(ArticleConfig.Customize))]
public class Article
{
    // ... properties
}

public static class ArticleConfig
{
    public static CollectionCreateParams Customize(CollectionCreateParams config)
    {
        config.InvertedIndexConfig = new InvertedIndexConfig
        {
            Bm25 = new BM25Config { K1 = 1.5f, B = 0.75f },
            Stopwords = new StopwordsConfig { Preset = "en" }
        };
        return config;
    }
}
```

### Multi-Tenancy

```csharp
// Enable multi-tenancy on collection
[WeaviateCollection("Articles", MultiTenancy = true)]
public class Article { }

// Work with specific tenant
var tenant = await collection.Tenants.Create("tenant-123");

// Query for specific tenant
var results = await collection.Query<Article>()
    .ForTenant("tenant-123")
    .Limit(10)
    .ExecuteAsync();
```

---

## Best Practices

### 1. Property Naming

✅ **Do:**
```csharp
[Property(DataType.Text)]
public string Title { get; set; }  // Will be "title" in Weaviate (camelCase)
```

❌ **Don't:**
```csharp
public string title { get; set; }  // C# naming convention violation
```

### 2. Nullable Vectors

✅ **Do:**
```csharp
[Vector<Vectorizer.Text2VecOpenAI>()]
public float[]? Embedding { get; set; }  // Nullable - won't be sent if null
```

❌ **Don't:**
```csharp
[Vector<Vectorizer.Text2VecOpenAI>()]
public float[] Embedding { get; set; }  // Non-nullable might cause issues
```

### 3. Batch Operations

✅ **Do:**
```csharp
await collection.Data.InsertMany(largeList);  // Optimized batch insert
```

❌ **Don't:**
```csharp
foreach (var item in largeList)
{
    await collection.Data.Insert(item);  // Slow, many round-trips
}
```

### 4. Query Filters Early

✅ **Do:**
```csharp
await collection.Query<Article>()
    .Where(a => a.IsPublished)  // Filter first
    .NearText("AI")              // Then vector search
    .Limit(10)
    .ExecuteAsync();
```

❌ **Don't:**
```csharp
await collection.Query<Article>()
    .NearText("AI")              // Vector search on all
    .Where(a => a.IsPublished)  // Then filter
    .Limit(10)
    .ExecuteAsync();
```

### 5. Include Only What You Need

✅ **Do:**
```csharp
await collection.Query<Article>()
    .WithReferences(a => a.Author)  // Only author
    .Limit(10)
    .ExecuteAsync();
```

❌ **Don't:**
```csharp
await collection.Query<Article>()
    .WithReferences(a => a.Author)
    .WithReferences(a => a.Categories)
    .WithReferences(a => a.Tags)
    .WithReferences(a => a.Comments)  // Too many references = slow
    .Limit(10)
    .ExecuteAsync();
```

---

## Troubleshooting

### Common Issues

#### 1. "Collection already exists"

```csharp
// Check if exists first
var exists = await client.Schema.Exists("Articles");
if (!exists)
{
    await client.Collections.CreateFromClass<Article>();
}
```

#### 2. "Property type mismatch"

Make sure your C# types match Weaviate types:

| Weaviate | C# |
|----------|------|
| text | string |
| int | int, long |
| number | float, double |
| boolean | bool |
| date | DateTime |
| uuid | Guid |

#### 3. "Vector dimensions mismatch"

```csharp
// Specify dimensions explicitly
[Vector<Vectorizer.Text2VecOpenAI>(
    Dimensions = 1536  // Must match your model
)]
public float[]? Embedding { get; set; }
```

---

## Next Steps

- 📖 [API Reference](./collection_mapper_guide.md) - Detailed API documentation
- 🚀 [Advanced Features](./collection_mapper_future_features.md) - Upcoming capabilities
- 🔧 [Schema Migrations](./collection_mapper_status.md) - Migration guide
- 📝 [Examples](../src/Weaviate.Client.CollectionMapper/Examples.cs) - More code examples

---

## Support

- **GitHub Issues**: [weaviate/csharp-client](https://github.com/weaviate/csharp-client/issues)
- **Documentation**: [Weaviate Docs](https://weaviate.io/developers/weaviate)
- **Community**: [Weaviate Slack](https://weaviate.io/slack)

---

**Happy Mapping! 🎉**

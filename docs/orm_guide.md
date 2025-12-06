# Weaviate.Client.Orm - Complete Guide

**Version:** 1.0.0
**Last Updated:** 2025-12-06

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Quick Start](#quick-start)
4. [Defining Models](#defining-models)
5. [Creating Collections](#creating-collections)
6. [Data Operations](#data-operations)
7. [Querying Data](#querying-data)
8. [Schema Migrations](#schema-migrations)
9. [Advanced Topics](#advanced-topics)
10. [API Reference](#api-reference)
11. [Best Practices](#best-practices)

---

## Introduction

The Weaviate ORM layer provides a type-safe, attribute-based approach to working with Weaviate collections in C#. It eliminates the need for manual schema definitions and provides LINQ-style querying, automatic object mapping, and schema migration support.

### Key Features

- **Attribute-based schema definition** - Define collections using C# classes with attributes
- **Type-safe LINQ queries** - Query using lambda expressions instead of strings
- **Automatic object mapping** - Seamless conversion between C# objects and Weaviate data
- **Vector support** - First-class support for all 47+ Weaviate vectorizers
- **Reference handling** - Automatic extraction and injection of cross-references
- **Schema migrations** - Safe, incremental schema updates with breaking change detection
- **Zero external dependencies** (except Humanizer for string transformations)

### Design Philosophy

The ORM is built as an extension layer on top of the existing `Weaviate.Client` library. It never modifies the core client and uses extension methods to add ORM capabilities while maintaining full access to the underlying client API.

---

## Installation

```bash
dotnet add package Weaviate.Client.Orm
```

Or add to your `.csproj`:

```xml
<PackageReference Include="Weaviate.Client.Orm" Version="1.0.0" />
```

---

## Quick Start

### 1. Define a Model

```csharp
using Weaviate.Client.Orm.Attributes;

[WeaviateCollection("Articles")]
public class Article
{
    [Property(DataType.Text)]
    [Index(Filterable = true, Searchable = true)]
    public string Title { get; set; } = string.Empty;

    [Property(DataType.Text)]
    public string Content { get; set; } = string.Empty;

    [Property(DataType.Int)]
    [Index(Filterable = true)]
    public int WordCount { get; set; }

    [Property(DataType.Date)]
    public DateTime PublishedAt { get; set; }

    // Named vector for semantic search
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-small",
        Dimensions = 1536,
        SourceProperties = [nameof(Title), nameof(Content)]
    )]
    public float[]? ContentEmbedding { get; set; }
}
```

### 2. Create Collection

```csharp
using Weaviate.Client;
using Weaviate.Client.Orm.Extensions;

var client = new WeaviateClient("http://localhost:8080");

// Create collection from class definition
await client.Collections.CreateFromClass<Article>();
```

### 3. Insert Data

```csharp
var collection = client.Collections.Use("Articles");

var article = new Article
{
    Title = "Introduction to Weaviate",
    Content = "Weaviate is an open-source vector database...",
    WordCount = 1500,
    PublishedAt = DateTime.UtcNow
};

var id = await collection.Data.Insert(article);
```

### 4. Query Data

```csharp
// Vector search with filters
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000)
    .NearText("vector databases", vector: a => a.ContentEmbedding)
    .Limit(10)
    .ExecuteAsync();

foreach (var article in results)
{
    Console.WriteLine($"{article.Title} - {article.WordCount} words");
}
```

---

## Defining Models

### Collection-Level Attributes

#### `[WeaviateCollection]`

Defines collection name and description.

```csharp
// Explicit name
[WeaviateCollection("Articles", Description = "Blog articles and posts")]
public class Article { }

// Use class name as collection name
[WeaviateCollection]
public class Product { }
```

#### `[InvertedIndex]`

Configures inverted index settings for the collection.

```csharp
[InvertedIndex(
    IndexTimestamps = true,
    IndexNullState = true,
    IndexPropertyLength = true,
    CleanupIntervalSeconds = 60
)]
public class Article { }
```

### Property Attributes

#### `[Property]`

Marks a property for storage in Weaviate. Required for all persisted properties.

```csharp
[Property(DataType.Text, Description = "Article title")]
public string Title { get; set; }

[Property(DataType.Int)]
public int ViewCount { get; set; }

[Property(DataType.Date)]
public DateTime CreatedAt { get; set; }

[Property(DataType.TextArray)]
public List<string> Tags { get; set; }

[Property(DataType.Number)]
public double Rating { get; set; }

[Property(DataType.Bool)]
public bool IsPublished { get; set; }
```

**Supported Data Types:**
- `DataType.Text` - String
- `DataType.Int` - Integer
- `DataType.Number` - Double/Float
- `DataType.Bool` - Boolean
- `DataType.Date` - DateTime
- `DataType.Uuid` - Guid
- `DataType.TextArray` - List&lt;string&gt;
- `DataType.IntArray` - List&lt;int&gt;
- `DataType.NumberArray` - List&lt;double&gt;
- `DataType.BoolArray` - List&lt;bool&gt;
- `DataType.DateArray` - List&lt;DateTime&gt;
- `DataType.UuidArray` - List&lt;Guid&gt;
- `DataType.Object` - Nested object (requires `[NestedType]`)
- `DataType.ObjectArray` - List of nested objects

#### `[Index]`

Configures indexing behavior for filtering and searching.

```csharp
[Property(DataType.Text)]
[Index(Filterable = true, Searchable = true)]
public string Title { get; set; }

[Property(DataType.Int)]
[Index(Filterable = true, RangeFilters = true)]
public int Price { get; set; }

[Property(DataType.Date)]
[Index(Filterable = true)]
public DateTime CreatedAt { get; set; }
```

#### `[Tokenization]`

Specifies tokenization strategy for text properties.

```csharp
[Property(DataType.Text)]
[Tokenization(PropertyTokenization.Word)]  // Default for full-text search
public string Description { get; set; }

[Property(DataType.Text)]
[Tokenization(PropertyTokenization.Field)]  // Exact matching
public string Sku { get; set; }

[Property(DataType.Text)]
[Tokenization(PropertyTokenization.Lowercase)]  // Case-insensitive matching
public string Email { get; set; }
```

**Available Tokenization Options:**
- `Word` - Full-text search (default)
- `Lowercase` - Case-insensitive exact matching
- `Whitespace` - Split on whitespace
- `Field` - Exact field matching
- `Trigram` - N-gram matching for fuzzy search

### Vector Attributes

#### `[Vector<TVectorizer>]`

Defines named vector configurations using generic type parameters.

```csharp
// OpenAI text embeddings
[Vector<Vectorizer.Text2VecOpenAI>(
    Model = "text-embedding-3-small",
    Dimensions = 1536,
    SourceProperties = [nameof(Title), nameof(Content)]
)]
public float[]? ContentEmbedding { get; set; }

// Cohere embeddings
[Vector<Vectorizer.Text2VecCohere>(
    Model = "embed-multilingual-v3.0",
    SourceProperties = [nameof(Title)]
)]
public float[]? TitleEmbedding { get; set; }

// Hugging Face embeddings
[Vector<Vectorizer.Text2VecHuggingFace>(
    Model = "sentence-transformers/all-MiniLM-L6-v2",
    SourceProperties = [nameof(Description)]
)]
public float[]? DescriptionVector { get; set; }

// Self-provided vectors (no vectorizer)
[Vector<Vectorizer.SelfProvided>]
public float[]? CustomVector { get; set; }
```

**Common Vectorizer Properties:**
- `Name` - Custom vector name (default: camelCase property name)
- `Model` - Model name/identifier
- `Dimensions` - Vector dimensions
- `SourceProperties` - Properties to vectorize
- `BaseURL` - Custom API endpoint
- `TextFields` - Text fields for multi-modal vectorizers
- `ImageFields` - Image fields for multi-modal vectorizers
- `VideoFields` - Video fields for multi-modal vectorizers
- `ConfigMethod` - Static method name for advanced configuration

**Named Vectors:**

By default, the vector name in Weaviate matches your property name (converted to camelCase). You can override this using the `Name` property:

```csharp
// Default: vector name will be "contentEmbedding"
[Vector<Vectorizer.Text2VecOpenAI>(Model = "text-embedding-3-small")]
public float[]? ContentEmbedding { get; set; }

// Custom name: vector name will be "main_vector"
[Vector<Vectorizer.Text2VecOpenAI>(
    Name = "main_vector",
    Model = "text-embedding-3-small"
)]
public float[]? ContentEmbedding { get; set; }
```

This is useful when working with existing collections that have specific vector names.

**Advanced Configuration with ConfigMethod:**

For vectorizer-specific properties not available as attribute parameters, use `ConfigMethod` to specify a static method that configures the vectorizer:

```csharp
public class Article
{
    // Same class method
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-small",
        ConfigMethod = nameof(ConfigureContentVector)
    )]
    public float[]? ContentEmbedding { get; set; }

    // Different class method (type-safe with ConfigMethodClass)
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-small",
        ConfigMethod = nameof(VectorConfigurations.ConfigureOpenAI),
        ConfigMethodClass = typeof(VectorConfigurations)
    )]
    public float[]? TitleVector { get; set; }

    // Different class method (legacy string syntax - also supported)
    [Vector<Vectorizer.Text2VecCohere>(
        Model = "embed-multilingual-v3.0",
        ConfigMethod = "VectorConfigurations.ConfigureCohere"
    )]
    public float[]? DescriptionVector { get; set; }

    // Configuration method receives vector name and pre-built vectorizer
    public static Vectorizer.Text2VecOpenAI ConfigureContentVector(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt)
    {
        // Model is already set from attribute
        prebuilt.Type = "text";  // OpenAI-specific property
        prebuilt.VectorizeCollectionName = false;
        return prebuilt;
    }
}

// External configuration class
public static class VectorConfigurations
{
    public static Vectorizer.Text2VecOpenAI ConfigureOpenAI(
        string vectorName,
        Vectorizer.Text2VecOpenAI prebuilt)
    {
        prebuilt.Type = "code";  // For code embeddings
        prebuilt.BaseURL = "https://custom-api.example.com";
        return prebuilt;
    }

    public static Vectorizer.Text2VecCohere ConfigureCohere(
        string vectorName,
        Vectorizer.Text2VecCohere prebuilt)
    {
        prebuilt.Truncate = "END";  // Cohere-specific property
        return prebuilt;
    }
}
```

**Recommendation:** Use `ConfigMethodClass = typeof(ClassName)` for type safety and better IntelliSense support when referencing methods in different classes.

**Supported Vectorizers (47+ types):**
- Text2Vec: OpenAI, Cohere, HuggingFace, Transformers, Contextionary, GPT4All, Ollama, JinaAI, VoyageAI
- Multi2Vec: CLIP, Bind, Cohere, GooglePalm, JinaAI, Ollama, VoyageAI
- Img2Vec: Neural
- Ref2Vec: Centroid
- And more...

#### `[VectorIndex<TIndexConfig>]`

Configures vector index type and parameters. Supports HNSW, Flat, and Dynamic indexes.

```csharp
// HNSW index (default, best for most use cases)
[Vector<Vectorizer.Text2VecOpenAI>(Model = "text-embedding-3-small")]
[VectorIndex<VectorIndex.HNSW>(
    Distance = VectorDistance.Cosine,
    EfConstruction = 256,
    MaxConnections = 64,
    Ef = 100
)]
public float[]? ContentEmbedding { get; set; }

// Flat index (brute-force, exact search)
[Vector<Vectorizer.Text2VecCohere>(Model = "embed-english-v3.0")]
[VectorIndex<VectorIndex.Flat>(
    Distance = VectorDistance.Dot,
    VectorCacheMaxObjects = 100000
)]
public float[]? TitleVector { get; set; }

// Dynamic index (switches from Flat to HNSW based on size)
[Vector<Vectorizer.SelfProvided>()]
[VectorIndex<VectorIndex.Dynamic>(
    Distance = VectorDistance.L2Squared,
    Threshold = 10000  // Switch to HNSW at 10k objects
)]
public float[]? CustomVector { get; set; }
```

**Distance Metrics:**
- `Cosine` - Cosine similarity (most common, range 0-2)
- `Dot` - Dot product (for normalized embeddings)
- `L2Squared` - Euclidean distance squared (faster than L2)
- `Hamming` - Hamming distance (for binary vectors)

**HNSW Parameters:**
- `EfConstruction` - Build-time quality/speed tradeoff (default: 128)
- `MaxConnections` - Graph connectivity (default: 64)
- `Ef` - Query-time quality/speed tradeoff (default: -1)
- `DynamicEfMin`, `DynamicEfMax`, `DynamicEfFactor` - Dynamic ef adjustment
- `FlatSearchCutoff` - Brute-force threshold
- `VectorCacheMaxObjects` - Number of vectors to cache

**Flat Parameters:**
- `VectorCacheMaxObjects` - Number of vectors to cache

**Dynamic Parameters:**
- `Threshold` - Object count to switch from Flat to HNSW

#### `[QuantizerBQ]`, `[QuantizerPQ]`, `[QuantizerSQ]`, `[QuantizerRQ]`

Configures vector compression for memory savings and performance.

```csharp
// Binary Quantization (BQ) - fastest, 1 bit per dimension
[Vector<Vectorizer.Text2VecOpenAI>(Model = "text-embedding-ada-002")]
[VectorIndex<VectorIndex.HNSW>(Distance = VectorDistance.Cosine)]
[QuantizerBQ(
    RescoreLimit = 200,
    Cache = true
)]
public float[]? ContentEmbedding { get; set; }

// Product Quantization (PQ) - best compression/accuracy balance
[Vector<Vectorizer.Text2VecCohere>(Model = "embed-multilingual-v3.0")]
[VectorIndex<VectorIndex.HNSW>(Distance = VectorDistance.Cosine)]
[QuantizerPQ(
    Segments = 96,
    Centroids = 256,
    EncoderType = PQEncoderType.Kmeans,
    EncoderDistribution = PQEncoderDistribution.LogNormal,
    TrainingLimit = 100000
)]
public float[]? TitleVector { get; set; }

// Scalar Quantization (SQ) - simple 8-bit compression
[Vector<Vectorizer.SelfProvided>()]
[VectorIndex<VectorIndex.HNSW>(Distance = VectorDistance.L2Squared)]
[QuantizerSQ(
    RescoreLimit = 100,
    TrainingLimit = 100000
)]
public float[]? CustomVector { get; set; }

// Residual Quantization (RQ) - advanced compression
[Vector<Vectorizer.Text2VecTransformers>(Model = "sentence-transformers/all-MiniLM-L6-v2")]
[VectorIndex<VectorIndex.HNSW>(Distance = VectorDistance.Cosine)]
[QuantizerRQ(
    RescoreLimit = 150,
    Cache = true,
    Bits = 8
)]
public float[]? SentenceEmbedding { get; set; }
```

**Quantizer Properties:**
- `RescoreLimit` - Number of candidates to rescore (BQ, SQ, RQ only)
- `Cache` - Cache quantized vectors (BQ, RQ only)
- `Bits` - Bits for compression (RQ only)
- `Segments` - PQ segments (PQ only, default: 0/auto)
- `Centroids` - PQ centroids (PQ only, default: 256)
- `BitCompression` - Bit compression (PQ only, default: false)
- `TrainingLimit` - Training limit (PQ, SQ only, default: 100000)
- `EncoderType` - PQ encoder type (PQ only: Kmeans, Tile)
- `EncoderDistribution` - PQ distribution (PQ only: LogNormal, Normal)

#### `[Encoding]`

Configures Muvera encoding for multi-vector embeddings (e.g., ColBERT).

```csharp
// Multi-vector with Muvera encoding
[Vector<Vectorizer.SelfProvided>()]
[VectorIndex<VectorIndex.HNSW>(Distance = VectorDistance.Cosine)]
[Encoding(
    KSim = 4,
    DProjections = 16,
    Repetitions = 10
)]
public float[,]? ColBERTEmbedding { get; set; }
```

**Encoding Parameters:**
- `KSim` - k-similarity parameter (default: 4)
- `DProjections` - Dimension projections (default: 16)
- `Repetitions` - Number of repetitions (default: 10)

### Reference Attributes

#### `[Reference]`

Defines cross-references to other collections.

```csharp
// Single reference
[Reference("Author")]
public Author? Author { get; set; }

// ID-only reference
[Reference("Category")]
public Guid? CategoryId { get; set; }

// Multi-reference (list of objects)
[Reference("Tags")]
public List<Tag>? Tags { get; set; }

// Multi-reference (list of IDs)
[Reference("RelatedArticles")]
public List<Guid>? RelatedArticleIds { get; set; }
```

### Nested Object Attributes

#### `[NestedType]` (Optional)

Defines nested object structures. **In most cases, the nested type is automatically inferred from the property type**, so this attribute is optional.

```csharp
public class Article
{
    // Nested object - type automatically inferred from property type
    [Property(DataType.Object)]
    public Author Author { get; set; }

    // Nested array - type automatically inferred from List<T>
    [Property(DataType.ObjectArray)]
    public List<Comment> Comments { get; set; }
}

public class Author
{
    [Property(DataType.Text)]
    public string Name { get; set; }

    [Property(DataType.Text)]
    public string Email { get; set; }
}

public class Comment
{
    [Property(DataType.Text)]
    public string Text { get; set; }

    [Property(DataType.Date)]
    public DateTime CreatedAt { get; set; }
}
```

**When to use `[NestedType]` explicitly:**

Only needed if you want to override the inferred type (e.g., when using interfaces):

```csharp
// Override inferred type
[Property(DataType.Object)]
[NestedType(typeof(ConcreteAuthor))]  // Override because property is interface
public IAuthor Author { get; set; }
```

---

## Creating Collections

### From Class Definition

```csharp
using Weaviate.Client.Orm.Extensions;

// Create collection
await client.Collections.CreateFromClass<Article>();

// Create with custom configuration
var config = CollectionSchemaBuilder.FromClass<Article>();
config.ReplicationConfig = new ReplicationConfig { Factor = 3 };
await client.Collections.Create(config);
```

### Property Name Conversion

The ORM automatically converts C# property names (PascalCase) to Weaviate property names (camelCase):

```csharp
public class Product
{
    [Property(DataType.Text)]
    public string ProductName { get; set; }  // → "productName" in Weaviate

    [Property(DataType.Int)]
    public int SKU { get; set; }  // → "sku" in Weaviate
}
```

---

## Data Operations

### Insert

#### Single Object

```csharp
var article = new Article
{
    Title = "Getting Started",
    Content = "...",
    WordCount = 500,
    PublishedAt = DateTime.UtcNow
};

// Auto-generate ID
var id = await collection.Data.Insert(article);

// Specify ID
var customId = Guid.NewGuid();
await collection.Data.Insert(article, customId);
```

#### Batch Insert

```csharp
var articles = new List<Article>
{
    new Article { Title = "Article 1", ... },
    new Article { Title = "Article 2", ... },
    new Article { Title = "Article 3", ... }
};

var result = await collection.Data.InsertMany(articles);

// Check for errors
if (result.Errors != null)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

### Update

#### Replace (Full Update)

```csharp
var article = await collection.Data.GetByID<Article>(id);
article.Title = "Updated Title";
article.WordCount = 600;

await collection.Data.Replace(article, id);
```

#### Partial Update

```csharp
// Only update specific fields
await collection.Data.Update(new Article
{
    WordCount = 750,
    PublishedAt = DateTime.UtcNow
}, id);
```

### Delete

#### Delete by ID

```csharp
await collection.Data.DeleteByID(id);
```

#### Bulk Delete with Filter

```csharp
// Type-safe bulk delete
var result = await collection.Data.DeleteMany<Article>(
    a => a.WordCount < 100 && a.PublishedAt < DateTime.Now.AddYears(-1)
);

Console.WriteLine($"Deleted {result.Successful} objects");
```

### Working with Vectors and References

Vectors and references are automatically extracted and injected:

```csharp
var article = new Article
{
    Title = "Test",
    ContentEmbedding = myVector,  // Automatically extracted to Vectors dictionary
    Author = author,               // Automatically extracted to References
    CategoryId = categoryId        // Automatically extracted to References
};

await collection.Data.Insert(article);

// On retrieval, vectors and references are automatically populated
var retrieved = await collection.Query<Article>()
    .WithVectors(a => a.ContentEmbedding)
    .WithReferences(a => a.Author)
    .ExecuteAsync();

// Vector and reference populated
Console.WriteLine(retrieved[0].ContentEmbedding?.Length);
Console.WriteLine(retrieved[0].Author?.Name);
```

---

## Querying Data

### Basic Filtering

```csharp
// Single condition
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000)
    .ExecuteAsync();

// Multiple conditions (AND)
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000)
    .Where(a => a.PublishedAt > DateTime.Now.AddDays(-7))
    .ExecuteAsync();

// Combined conditions
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000 && a.IsPublished == true)
    .ExecuteAsync();

// OR conditions
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 5000 || a.ViewCount > 10000)
    .ExecuteAsync();
```

### Supported Filter Operators

```csharp
// Equality
.Where(a => a.Category == "Technology")
.Where(a => a.IsPublished == true)

// Comparison
.Where(a => a.WordCount > 1000)
.Where(a => a.Rating >= 4.5)
.Where(a => a.Price < 100)

// Contains (string)
.Where(a => a.Title.Contains("Weaviate"))

// Contains (array)
.Where(a => a.Tags.Contains("AI"))

// ContainsAny
.Where(a => a.Tags.ContainsAny(new[] { "AI", "ML", "Database" }))

// ContainsAll
.Where(a => a.Tags.ContainsAll(new[] { "Vector", "Database" }))

// Date comparisons
.Where(a => a.PublishedAt > DateTime.Now.AddDays(-7))
.Where(a => a.CreatedAt < new DateTime(2024, 1, 1))
```

### Nested Property Filtering

```csharp
var results = await collection.Query<Article>()
    .Where(a => a.Author.Name == "John Doe")
    .Where(a => a.Author.Email.Contains("@example.com"))
    .ExecuteAsync();
```

### Vector Search

#### Near Text

```csharp
// Basic near text search
var results = await collection.Query<Article>()
    .NearText("machine learning tutorials")
    .ExecuteAsync();

// With named vector
var results = await collection.Query<Article>()
    .NearText("AI technology", vector: a => a.TitleEmbedding)
    .ExecuteAsync();

// With certainty threshold
var results = await collection.Query<Article>()
    .NearText("vector databases", certainty: 0.7f)
    .ExecuteAsync();

// With distance threshold
var results = await collection.Query<Article>()
    .NearText("semantic search", distance: 0.3f)
    .ExecuteAsync();
```

#### Near Vector

```csharp
float[] queryVector = GetEmbedding("my query");

var results = await collection.Query<Article>()
    .NearVector(queryVector, vector: a => a.ContentEmbedding)
    .ExecuteAsync();
```

#### Hybrid Search

Combines BM25 keyword search with vector search.

```csharp
// Equal weighting (alpha = 0.5)
var results = await collection.Query<Article>()
    .Hybrid("machine learning", alpha: 0.5f)
    .ExecuteAsync();

// More keyword-focused (alpha closer to 0)
var results = await collection.Query<Article>()
    .Hybrid("Python tutorial", alpha: 0.2f)
    .ExecuteAsync();

// More vector-focused (alpha closer to 1)
var results = await collection.Query<Article>()
    .Hybrid("data science", alpha: 0.8f)
    .ExecuteAsync();

// With named vector
var results = await collection.Query<Article>()
    .Hybrid("neural networks", alpha: 0.5f, vector: a => a.ContentEmbedding)
    .ExecuteAsync();
```

### Combining Filters and Vector Search

```csharp
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000)
    .Where(a => a.PublishedAt > DateTime.Now.AddDays(-30))
    .Where(a => a.IsPublished == true)
    .NearText("vector databases", vector: a => a.ContentEmbedding, certainty: 0.7f)
    .Limit(10)
    .ExecuteAsync();
```

### Including Vectors and References

```csharp
// Include vectors in results
var results = await collection.Query<Article>()
    .NearText("AI")
    .WithVectors(a => a.ContentEmbedding)
    .ExecuteAsync();

// Include references
var results = await collection.Query<Article>()
    .WithReferences(a => a.Author)
    .WithReferences(a => a.Category)
    .ExecuteAsync();

// Both vectors and references
var results = await collection.Query<Article>()
    .NearText("machine learning")
    .WithVectors(a => a.TitleEmbedding)
    .WithReferences(a => a.Author)
    .ExecuteAsync();
```

### Property Projection

```csharp
// Select specific properties
var results = await collection.Query<Article>()
    .Select(a => a.Title)
    .Select(a => a.PublishedAt)
    .ExecuteAsync();

// Note: Other properties will be null/default
```

### Sorting and Pagination

```csharp
// Sort ascending
var results = await collection.Query<Article>()
    .Sort(a => a.PublishedAt, descending: false)
    .ExecuteAsync();

// Sort descending
var results = await collection.Query<Article>()
    .Sort(a => a.WordCount, descending: true)
    .Limit(20)
    .ExecuteAsync();

// Pagination
var results = await collection.Query<Article>()
    .Limit(10)
    .Offset(20)
    .ExecuteAsync();
```

### Retrieving Metadata

```csharp
// Get metadata (distance, certainty, timestamps, etc.)
var results = await collection.Query<Article>()
    .NearText("vector search")
    .WithMetadata(MetadataQuery.Distance | MetadataQuery.Certainty | MetadataQuery.CreationTime)
    .ExecuteWithMetadataAsync();

foreach (var result in results)
{
    Console.WriteLine($"Distance: {result.Metadata?.Distance}");
    Console.WriteLine($"Certainty: {result.Metadata?.Certainty}");
    Console.WriteLine($"Created: {result.Metadata?.CreationTime}");

    var article = result.Properties;
    Console.WriteLine($"Title: {article.Title}");
}
```

---

## Schema Migrations

The ORM provides a safe migration system for evolving schemas over time.

### Checking for Migrations

```csharp
// Check what changes would be applied
var plan = await client.Collections.CheckMigrate<Article>();

// Display summary
Console.WriteLine(plan.GetSummary());

// Example output:
// Migration plan for 'Article' (3 changes):
//   ✓ AddProperty: Add property 'tags' (TEXT_ARRAY)
//   ✓ AddVector: Add vector 'titleEmbedding'
//   ⚠ RemoveProperty: Remove property 'oldField' (BREAKING - data will be lost)

// Check if safe
if (plan.IsSafe)
{
    Console.WriteLine("All changes are safe to apply");
}
else
{
    Console.WriteLine("WARNING: Contains breaking changes!");
}

// Inspect individual changes
foreach (var change in plan.Changes)
{
    Console.WriteLine($"{change.ChangeType}: {change.Description} (Safe: {change.IsSafe})");
}
```

### Applying Migrations

#### Safe Migration (Default)

```csharp
// Only applies safe (additive) changes
await client.Collections.Migrate<Article>();

// Throws InvalidOperationException if breaking changes detected
```

#### Migration with Breaking Changes

```csharp
// Allow breaking changes (USE WITH CAUTION)
try
{
    await client.Collections.Migrate<Article>(allowBreakingChanges: true);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Migration failed: {ex.Message}");
}
```

#### Skip Safety Check

```csharp
// Faster if you trust your changes
await client.Collections.Migrate<Article>(checkFirst: false);
```

### Creating Collection if Not Exists

```csharp
// Migrate will automatically create collection if it doesn't exist
await client.Collections.Migrate<Article>();
```

### Change Types

**Safe Changes (IsSafe = true):**
- `AddProperty` - Adding new properties
- `AddReference` - Adding new cross-references
- `AddVector` - Adding new vector configurations
- `UpdateDescription` - Updating collection description
- `UpdatePropertyDescription` - Updating property descriptions
- `UpdateReferenceDescription` - Updating reference descriptions
- `UpdateReplication` - Changing replication factor
- `UpdateMultiTenancy` - Changing multi-tenancy settings

**Breaking Changes (IsSafe = false):**
- `RemoveProperty` - Removing properties (data loss)
- `RemoveReference` - Removing references (data loss)
- `RemoveVector` - Removing vector configurations (data loss)
- `ModifyPropertyType` - Changing property data types (incompatible)

### Migration Workflow

```csharp
// 1. Update your model
[WeaviateCollection("Articles")]
public class Article
{
    // ... existing properties ...

    // New property added
    [Property(DataType.TextArray)]
    public List<string> Tags { get; set; } = new();
}

// 2. Check migration plan
var plan = await client.Collections.CheckMigrate<Article>();
Console.WriteLine(plan.GetSummary());

// 3. Review changes
if (!plan.IsSafe)
{
    Console.WriteLine("WARNING: Breaking changes detected!");
    foreach (var change in plan.Changes.Where(c => !c.IsSafe))
    {
        Console.WriteLine($"  - {change.Description}");
    }

    // Decide whether to proceed
    Console.Write("Continue? (y/n): ");
    if (Console.ReadLine()?.ToLower() != "y")
    {
        return;
    }
}

// 4. Apply migration
await client.Collections.Migrate<Article>(
    allowBreakingChanges: !plan.IsSafe
);

Console.WriteLine("Migration completed successfully!");
```

---

## Advanced Topics

### Multi-Vector Support

```csharp
public class Article
{
    // Title vector
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-small",
        Dimensions = 1536,
        SourceProperties = [nameof(Title)]
    )]
    public float[]? TitleEmbedding { get; set; }

    // Content vector
    [Vector<Vectorizer.Text2VecCohere>(
        Model = "embed-multilingual-v3.0",
        SourceProperties = [nameof(Content)]
    )]
    public float[]? ContentEmbedding { get; set; }

    // Custom vector
    [Vector<Vectorizer.SelfProvided>]
    public float[]? CustomVector { get; set; }
}

// Query specific vector
var results = await collection.Query<Article>()
    .NearText("technology", vector: a => a.TitleEmbedding)
    .ExecuteAsync();

// Include multiple vectors in results
var results = await collection.Query<Article>()
    .WithVectors(a => a.TitleEmbedding)
    .WithVectors(a => a.ContentEmbedding)
    .ExecuteAsync();
```

### Multi-Tenancy

```csharp
// Define collection with multi-tenancy
[WeaviateCollection("Articles")]
public class Article { ... }

// Create with multi-tenancy enabled
var config = CollectionSchemaBuilder.FromClass<Article>();
config.MultiTenancyConfig = new MultiTenancyConfig { Enabled = true };
await client.Collections.Create(config);

// Use tenant-specific collection
var tenantCollection = client.Collections.Use("Articles").ForTenant("tenant1");

await tenantCollection.Data.Insert(article);
var results = await tenantCollection.Query<Article>().ExecuteAsync();
```

### Custom Vector Index Configuration

```csharp
public class Article
{
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-small",
        Dimensions = 1536,
        SourceProperties = [nameof(Content)]
    )]
    public float[]? ContentEmbedding { get; set; }
}

// Customize HNSW parameters
var config = CollectionSchemaBuilder.FromClass<Article>();
var vectorConfig = config.VectorConfig["contentEmbedding"];
vectorConfig.VectorIndexConfig = new VectorIndex.HNSW
{
    Ef = 128,
    EfConstruction = 256,
    MaxConnections = 32
};

await client.Collections.Create(config);
```

### Replication

```csharp
var config = CollectionSchemaBuilder.FromClass<Article>();
config.ReplicationConfig = new ReplicationConfig
{
    Factor = 3,  // 3 replicas
    AsyncEnabled = true
};

await client.Collections.Create(config);
```

### Sharding

```csharp
var config = CollectionSchemaBuilder.FromClass<Article>();
config.ShardingConfig = new ShardingConfig
{
    VirtualPerPhysical = 128,
    DesiredCount = 3
};

await client.Collections.Create(config);
```

---

## API Reference

### Extension Methods

#### WeaviateClient Extensions

```csharp
// Create collection from class
Task CreateFromClass<T>(CancellationToken cancellationToken = default)
    where T : class
```

#### CollectionsClient Extensions

```csharp
// Check for schema migrations
Task<MigrationPlan> CheckMigrate<T>(CancellationToken cancellationToken = default)
    where T : class

// Apply schema migrations
Task<MigrationPlan> Migrate<T>(
    bool checkFirst = true,
    bool allowBreakingChanges = false,
    CancellationToken cancellationToken = default)
    where T : class
```

#### CollectionClient Extensions

```csharp
// Get ORM query builder
OrmQueryClient<T> Query<T>() where T : class
```

#### DataClient Extensions

```csharp
// Insert single object
Task<Guid> Insert<T>(T obj, Guid? id = null, CancellationToken cancellationToken = default)
    where T : class

// Insert multiple objects
Task<BatchInsertResponse> InsertMany<T>(
    IEnumerable<T> objects,
    CancellationToken cancellationToken = default)
    where T : class

// Replace object
Task Replace<T>(T obj, Guid id, CancellationToken cancellationToken = default)
    where T : class

// Update object
Task Update<T>(T obj, Guid id, CancellationToken cancellationToken = default)
    where T : class

// Delete by ID
Task DeleteByID(Guid id, CancellationToken cancellationToken = default)

// Delete with filter
Task<BatchDeleteResponse> DeleteMany<T>(
    Expression<Func<T, bool>> where,
    CancellationToken cancellationToken = default)
    where T : class
```

### OrmQueryClient

```csharp
// Filtering
OrmQueryClient<T> Where(Expression<Func<T, bool>> predicate)

// Vector search
OrmQueryClient<T> NearText(
    string text,
    Expression<Func<T, object>>? vector = null,
    float? certainty = null,
    float? distance = null)

OrmQueryClient<T> NearVector(
    float[] vector,
    Expression<Func<T, object>>? vectorProperty = null,
    float? certainty = null,
    float? distance = null)

// Hybrid search
OrmQueryClient<T> Hybrid(
    string query,
    float? alpha = null,
    Expression<Func<T, object>>? vector = null)

// Include data
OrmQueryClient<T> WithVectors(Expression<Func<T, object>> vectorProperty)
OrmQueryClient<T> WithReferences(Expression<Func<T, object>> referenceProperty)
OrmQueryClient<T> Select(Expression<Func<T, object>> property)
OrmQueryClient<T> WithMetadata(MetadataQuery metadata)

// Sorting and pagination
OrmQueryClient<T> Sort(Expression<Func<T, object>> property, bool descending = false)
OrmQueryClient<T> Limit(long limit)
OrmQueryClient<T> Offset(long offset)

// Execution
Task<List<T>> ExecuteAsync(CancellationToken cancellationToken = default)
Task<List<WeaviateObject<T>>> ExecuteWithMetadataAsync(
    CancellationToken cancellationToken = default)
```

### Schema Builder

```csharp
// Build collection config from class
CollectionConfig CollectionSchemaBuilder.FromClass<T>() where T : class
```

### Object Mapper

```csharp
// Convert from Weaviate format
T OrmObjectMapper.FromWeaviateObject<T>(WeaviateObject weaviateObject)
List<T> OrmObjectMapper.FromWeaviateObjects<T>(List<WeaviateObject> weaviateObjects)

// Check if mapping needed
bool OrmObjectMapper.RequiresMapping<T>()
```

---

## Best Practices

### 1. Use Meaningful Collection Names

```csharp
// Good
[WeaviateCollection("BlogPosts")]
public class BlogPost { }

// Avoid generic names
[WeaviateCollection("Items")]  // Too generic
public class Article { }
```

### 2. Add Descriptions for Documentation

```csharp
[WeaviateCollection("Products", Description = "E-commerce product catalog")]
public class Product
{
    [Property(DataType.Text, Description = "Product display name")]
    public string Name { get; set; }
}
```

### 3. Index Only What You Need

```csharp
// Index filterable properties
[Property(DataType.Text)]
[Index(Filterable = true, Searchable = true)]  // Will be filtered/searched
public string Title { get; set; }

// Don't index large text fields you won't filter on
[Property(DataType.Text)]
// No [Index] attribute - won't be indexed for filtering
public string FullContent { get; set; }
```

### 4. Choose Appropriate Tokenization

```csharp
// Full-text search
[Property(DataType.Text)]
[Tokenization(PropertyTokenization.Word)]
public string Description { get; set; }

// Exact matching (SKUs, emails, etc.)
[Property(DataType.Text)]
[Tokenization(PropertyTokenization.Field)]
public string Sku { get; set; }
```

### 5. Use Named Vectors Appropriately

```csharp
// Multiple vectors for different purposes
public class Product
{
    // Title vector for product name search
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-small",
        SourceProperties = [nameof(Name)]
    )]
    public float[]? NameEmbedding { get; set; }

    // Description vector for detailed search
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-3-large",
        SourceProperties = [nameof(Description)]
    )]
    public float[]? DescriptionEmbedding { get; set; }
}
```

### 6. Handle Migrations Safely

```csharp
// Always check migration plan first
var plan = await client.Collections.CheckMigrate<Article>();

if (!plan.IsSafe)
{
    // Log breaking changes
    logger.LogWarning("Breaking changes detected:");
    foreach (var change in plan.Changes.Where(c => !c.IsSafe))
    {
        logger.LogWarning($"  {change.Description}");
    }

    // Require explicit confirmation for breaking changes
    throw new InvalidOperationException(
        "Breaking changes detected. Set allowBreakingChanges=true to proceed."
    );
}

await client.Collections.Migrate<Article>();
```

### 7. Use Batch Operations for Large Datasets

```csharp
// Good - batch insert
var articles = LoadArticles();
await collection.Data.InsertMany(articles);

// Avoid - inserting one by one in a loop
foreach (var article in articles)
{
    await collection.Data.Insert(article);  // Slow!
}
```

### 8. Include Vectors/References Only When Needed

```csharp
// Good - only request what you need
var results = await collection.Query<Article>()
    .Where(a => a.PublishedAt > DateTime.Now.AddDays(-7))
    .ExecuteAsync();  // Vectors not included

// Avoid - requesting unnecessary data
var results = await collection.Query<Article>()
    .WithVectors(a => a.ContentEmbedding)  // Not needed if not using vectors
    .WithReferences(a => a.Author)          // Not needed if not displaying author
    .ExecuteAsync();
```

### 9. Use Type-Safe Queries

```csharp
// Good - type-safe lambda expressions
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 1000)
    .NearText("AI", vector: a => a.TitleEmbedding)
    .ExecuteAsync();

// Avoid - using raw filters if ORM is available
var filter = Filter.Where.Path("wordCount").GreaterThan(1000);
```

### 10. Leverage CancellationTokens

```csharp
public async Task<List<Article>> SearchArticles(
    string query,
    CancellationToken cancellationToken)
{
    return await collection.Query<Article>()
        .NearText(query)
        .Limit(10)
        .ExecuteAsync(cancellationToken);
}
```

---

## Common Patterns

### Pagination

```csharp
public async Task<PaginatedResult<Article>> GetArticles(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    var offset = page * pageSize;

    var results = await collection.Query<Article>()
        .Sort(a => a.PublishedAt, descending: true)
        .Limit(pageSize)
        .Offset(offset)
        .ExecuteAsync(cancellationToken);

    return new PaginatedResult<Article>
    {
        Items = results,
        Page = page,
        PageSize = pageSize
    };
}
```

### Search with Filters

```csharp
public async Task<List<Product>> SearchProducts(
    string searchQuery,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    List<string>? categories = null,
    CancellationToken cancellationToken = default)
{
    var query = collection.Query<Product>()
        .NearText(searchQuery, vector: p => p.DescriptionEmbedding);

    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice.Value);

    if (maxPrice.HasValue)
        query = query.Where(p => p.Price <= maxPrice.Value);

    if (categories?.Any() == true)
        query = query.Where(p => categories.Contains(p.Category));

    return await query.Limit(20).ExecuteAsync(cancellationToken);
}
```

### Recommendations

```csharp
public async Task<List<Article>> GetSimilarArticles(
    Guid articleId,
    int count = 5,
    CancellationToken cancellationToken = default)
{
    // Get source article
    var source = await collection.Data.GetByID<Article>(articleId);

    if (source?.ContentEmbedding == null)
        return new List<Article>();

    // Find similar articles
    var similar = await collection.Query<Article>()
        .NearVector(source.ContentEmbedding, vectorProperty: a => a.ContentEmbedding)
        .Where(a => a.Id != articleId)  // Exclude source article
        .Limit(count)
        .ExecuteAsync(cancellationToken);

    return similar;
}
```

---

## Troubleshooting

### Collection Already Exists

```csharp
try
{
    await client.Collections.CreateFromClass<Article>();
}
catch (Exception ex) when (ex.Message.Contains("already exists"))
{
    // Collection exists, use migration instead
    await client.Collections.Migrate<Article>();
}
```

### Property Name Mismatch

If property names don't match between C# and Weaviate:

```csharp
// C# uses PascalCase
public class Article
{
    [Property(DataType.Text)]
    public string Title { get; set; }  // → "title" in Weaviate (camelCase)
}

// When querying, use C# property names
.Where(a => a.Title.Contains("AI"))  // Automatically converted to "title"
```

### Vectors Not Populated

Vectors are only populated if explicitly requested:

```csharp
// Wrong - vectors will be null
var results = await collection.Query<Article>()
    .NearText("AI")
    .ExecuteAsync();

// Correct - vectors included
var results = await collection.Query<Article>()
    .NearText("AI")
    .WithVectors(a => a.ContentEmbedding)
    .ExecuteAsync();
```

### References Not Expanded

References only contain IDs by default:

```csharp
// Without WithReferences - only ID populated
var articles = await collection.Query<Article>().ExecuteAsync();
// articles[0].AuthorId has value, but articles[0].Author is null

// With WithReferences - full object populated
var articles = await collection.Query<Article>()
    .WithReferences(a => a.Author)
    .ExecuteAsync();
// articles[0].Author has full object
```

---

## Migration from Manual Schema Definitions

If you have existing collections created manually:

```csharp
// 1. Define model matching existing schema
[WeaviateCollection("ExistingCollection")]
public class ExistingModel
{
    [Property(DataType.Text)]
    public string Title { get; set; }

    // Match existing properties...
}

// 2. Check migration (should show no changes if model matches)
var plan = await client.Collections.CheckMigrate<ExistingModel>();
Console.WriteLine(plan.GetSummary());

// 3. Use ORM with existing collection
var collection = client.Collections.Use("ExistingCollection");
var results = await collection.Query<ExistingModel>()
    .Where(m => m.Title.Contains("test"))
    .ExecuteAsync();
```

---

## Performance Considerations

1. **Batch Operations**: Use `InsertMany` instead of multiple `Insert` calls
2. **Limit Results**: Always use `.Limit()` to avoid loading too much data
3. **Select Specific Properties**: Use `.Select()` to reduce data transfer
4. **Index Strategically**: Only index properties you'll filter on
5. **Vector Inclusion**: Only request vectors when needed with `.WithVectors()`
6. **Reference Expansion**: Only expand references when needed with `.WithReferences()`

---

## Examples Repository

For complete working examples, see:
- `src/Weaviate.Client.Orm/Examples.cs` - Comprehensive examples
- `src/Weaviate.Client.Orm.Tests/` - Unit tests demonstrating usage

---

## Support and Resources

- **GitHub Issues**: https://github.com/weaviate/weaviate-dotnet-client/issues
- **Weaviate Documentation**: https://weaviate.io/developers/weaviate
- **API Reference**: See inline XML documentation in your IDE

---

**Version:** 1.0.0
**Last Updated:** 2025-12-06

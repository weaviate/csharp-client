# Typed Client Wrappers

## Overview

The Weaviate C# client provides **strongly-typed wrappers** around the standard untyped client APIs. These typed wrappers give you compile-time type safety, IntelliSense support, and reduced runtime errors when working with Weaviate collections.

### Benefits

✅ **Compile-time Type Safety** - Catch type mismatches at compile time instead of runtime
✅ **IntelliSense Support** - Get autocomplete for properties and methods in your IDE
✅ **Reduced Boilerplate** - No need to cast `object` types or use dictionaries
✅ **Optional Validation** - Validate your C# types against collection schemas
✅ **Zero Performance Overhead** - Thin wrappers that delegate to the underlying clients

## Quick Start

### 1. Define Your Model Class

```csharp
public class Article
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public int ViewCount { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}
```

### 2. Get a Typed Collection Client

**Option A: Using `Collections.Use<T>()`**

```csharp
var articlesClient = client.Collections.Use<Article>("Articles");
```

**Option B: Using the `AsTyped<T>()` extension method**

```csharp
var untypedClient = client.Collections.Use("Articles");
var articlesClient = untypedClient.AsTyped<Article>();
```

### 3. Use Typed Operations

```csharp
// Insert with compile-time type safety
var article = new Article
{
    Title = "Getting Started",
    Content = "Welcome to Weaviate!",
    PublishedDate = DateTime.UtcNow,
    ViewCount = 0,
    Tags = new[] { "tutorial", "getting-started" }
};

Guid id = await articlesClient.Data.Insert(article);

// Query with strongly-typed results
var results = await articlesClient.Query.NearText("machine learning");

foreach (var result in results.Objects)
{
    // No casting needed! Properties are strongly typed
    Console.WriteLine($"Title: {result.Properties.Title}");
    Console.WriteLine($"Views: {result.Properties.ViewCount}");
}
```

## Core Components

### TypedCollectionClient<T>

The main entry point for typed operations on a collection.

```csharp
TypedCollectionClient<Article> articlesClient =
    client.Collections.Use<Article>("Articles");

// Access typed sub-clients
TypedDataClient<Article> data = articlesClient.Data;           // CRUD operations
TypedQueryClient<Article> query = articlesClient.Query;        // Search & queries
TypedGenerateClient<Article> generate = articlesClient.Generate; // RAG operations
AggregateClient aggregate = articlesClient.Aggregate;          // Aggregations (untyped)
CollectionConfigClient config = articlesClient.Config;         // Collection config
```

### TypedDataClient<T>

Provides type-safe CRUD operations.

```csharp
// Insert single object
Guid id = await articlesClient.Data.Insert(article);

// Insert many objects
var articles = new[] { article1, article2, article3 };
var response = await articlesClient.Data.InsertMany(articles);

// Replace an object
await articlesClient.Data.Replace(id, updatedArticle);

// Delete an object
await articlesClient.Data.DeleteByID(id);

// Delete many with filter
await articlesClient.Data.DeleteMany(
    Filter.ByProperty("viewCount").LessThan(10)
);
```

### TypedQueryClient<T>

Provides type-safe query and search operations.

```csharp
// Fetch objects
var allArticles = await articlesClient.Query.FetchObjects(limit: 100);

// Fetch by ID
var article = await articlesClient.Query.FetchObjectByID(id);

// Near text search
var searchResults = await articlesClient.Query.NearText(
    "artificial intelligence tutorials",
    limit: 10
);

// BM25 keyword search
var keywordResults = await articlesClient.Query.BM25(
    "machine learning",
    searchFields: new[] { "title", "content" }
);

// Hybrid search
var hybridResults = await articlesClient.Query.Hybrid(
    "deep learning",
    alpha: 0.5f  // 0 = pure keyword, 1 = pure vector
);

// All results are strongly typed
foreach (var result in searchResults.Objects)
{
    string title = result.Properties.Title;  // No casting!
    DateTime published = result.Properties.PublishedDate;
}
```

### TypedGenerateClient<T>

Provides type-safe Retrieval-Augmented Generation (RAG) operations.

```csharp
// Generate content for search results
var results = await articlesClient.Generate.NearText(
    "machine learning",
    prompt: new SinglePrompt("Summarize this article in one sentence"),
    limit: 5
);

foreach (var result in results.Objects)
{
    Console.WriteLine($"Title: {result.Properties.Title}");
    Console.WriteLine($"AI Summary: {result.Generated}");
}

// Group-based generation
var groupResults = await articlesClient.Generate.NearText(
    "tutorials",
    groupBy: new GroupByRequest("tags", 10),
    groupedTask: new GroupedTask(
        "Create a summary of all articles with these tags"
    )
);
```

## Type Validation

### Opt-in Validation

By default, typed clients do not validate your C# types against the collection schema for performance. Enable validation during development:

```csharp
// Validate on construction
var articlesClient = client.Collections.Use<Article>(
    "Articles",
    validateType: true  // Throws if type doesn't match schema
);

// Or validate explicitly
var articlesClient = client.Collections.Use<Article>("Articles");
var validation = await articlesClient.ValidateType();

if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"❌ {error.PropertyName}: {error.Message}");
        if (error.ExpectedType != null && error.ActualType != null)
        {
            Console.WriteLine($"   Expected: {error.ExpectedType}, Got: {error.ActualType}");
        }
    }
}
```

### Validation in Data Operations

You can also enable validation for individual insert operations:

```csharp
// Validate before inserting (only for untyped DataClient)
await untypedClient.Data.Insert(article, validate: true);
```

**Note:** The typed `TypedDataClient<T>` does not need the `validate` parameter because the type argument already provides compile-time type safety.

### Validation Result Details

```csharp
var validation = await articlesClient.ValidateType();

Console.WriteLine($"Valid: {validation.IsValid}");
Console.WriteLine($"Errors: {validation.Errors.Count}");
Console.WriteLine($"Warnings: {validation.Warnings.Count}");

// Get detailed message
Console.WriteLine(validation.GetDetailedMessage());

// Output example:
// Validation failed with 2 error(s):
//   ❌ Title: Property 'Title' type mismatch: expected 'text' but C# type maps to 'int'.
//      Expected: text, Got: int
//   ❌ Tags: Property 'Tags' type mismatch: expected 'text[]' but C# type maps to 'text'.
//      Expected: text[], Got: text
//
// 1 warning(s):
//   ⚠️  ExtraField: Property 'ExtraField' exists in C# type but not in schema.
//       It will be ignored during serialization.
```

### Validation Error Types

- **TypeMismatch**: Property type doesn't match schema (e.g., C# string but schema expects int)
- **ArrayMismatch**: Array vs non-array mismatch (e.g., C# string but schema expects string[])
- **RequiredPropertyMissing**: Schema property missing from C# type
- **UnsupportedType**: C# type has no registered converter
- **NestedObjectMismatch**: Nested object structure doesn't match schema

## Configuration & Multi-Tenancy

### Tenant Support

```csharp
// Create tenant-specific client
var tenant1Client = articlesClient.WithTenant("tenant1");

// All operations use the specified tenant
await tenant1Client.Data.Insert(article);
var results = await tenant1Client.Query.FetchObjects();

// Switch tenants
var tenant2Client = articlesClient.WithTenant("tenant2");
```

### Consistency Levels

```csharp
// Set consistency level
var consistentClient = articlesClient.WithConsistencyLevel(
    ConsistencyLevels.Quorum
);

await consistentClient.Data.Insert(article);
```

### Chaining Configuration

```csharp
var configuredClient = articlesClient
    .WithTenant("production")
    .WithConsistencyLevel(ConsistencyLevels.All);
```

## Iteration

Efficiently iterate over large collections:

```csharp
// Iterate through all articles
await foreach (var article in articlesClient.Iterator(cacheSize: 100))
{
    Console.WriteLine($"Processing: {article.Properties.Title}");
}

// Start from a specific ID
await foreach (var article in articlesClient.Iterator(after: lastId))
{
    // Process articles...
}
```

## Working with References

```csharp
public class Article
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    // Reference properties are managed separately
}

// Insert with references
await articlesClient.Data.Insert(
    article,
    references: new[]
    {
        new ObjectReference("author", new[] { authorId }),
        new ObjectReference("categories", new[] { categoryId1, categoryId2 })
    }
);

// Add references later
await articlesClient.Data.ReferenceAdd(
    articleId,
    "relatedArticles",
    relatedArticleId
);

// Query with references
var results = await articlesClient.Query.FetchObjects(
    returnReferences: new[]
    {
        new QueryReference("author"),
        new QueryReference("categories")
    }
);
```

## Working with Vectors

```csharp
// Insert with custom vectors
var vectors = new Vectors
{
    ["default"] = new float[] { 0.1f, 0.2f, 0.3f, /* ... */ }
};

await articlesClient.Data.Insert(article, vectors: vectors);

// Query near a vector
var vectorResults = await articlesClient.Query.NearVector(
    vectors,
    limit: 10
);

// Include vectors in results
var resultsWithVectors = await articlesClient.Query.FetchObjects(
    includeVectors: VectorQuery.All()
);

foreach (var result in resultsWithVectors.Objects)
{
    if (result.Vectors != null && result.Vectors.TryGetValue("default", out var vector))
    {
        Console.WriteLine($"Vector length: {vector.Length}");
    }
}
```

## Migration from Untyped to Typed

### Before (Untyped)

```csharp
var client = new WeaviateClient(config);
var collection = client.Collections.Use("Articles");

// Insert - requires object type
await collection.Data.Insert(new
{
    title = "Getting Started",
    content = "Welcome!",
    publishedDate = DateTime.UtcNow
});

// Query - results are WeaviateObject with object properties
var results = await collection.Query.FetchObjects();

foreach (var result in results.Objects)
{
    // Properties is Dictionary<string, object>
    var title = (string)result.Properties["title"];
    var content = (string)result.Properties["content"];
}
```

### After (Typed)

```csharp
var client = new WeaviateClient(config);
var collection = client.Collections.Use<Article>("Articles");

// Insert - strongly typed
await collection.Data.Insert(new Article
{
    Title = "Getting Started",
    Content = "Welcome!",
    PublishedDate = DateTime.UtcNow
});

// Query - results are WeaviateObject<Article>
var results = await collection.Query.FetchObjects();

foreach (var result in results.Objects)
{
    // Properties is Article - no casting needed!
    string title = result.Properties.Title;
    string content = result.Properties.Content;
}
```

## Best Practices

### 1. Use Type Validation During Development

```csharp
#if DEBUG
var articlesClient = client.Collections.Use<Article>(
    "Articles",
    validateType: true  // Catch schema mismatches early
);
#else
var articlesClient = client.Collections.Use<Article>("Articles");
#endif
```

### 2. Match Property Names to Schema

C# property names are automatically converted to camelCase:

- C# `Title` → schema `title`
- C# `PublishedDate` → schema `publishedDate`

```csharp
// Good - matches Weaviate convention
public class Article
{
    public string Title { get; set; }          // → title
    public DateTime PublishedDate { get; set; } // → publishedDate
}
```

### 3. Use Nullable Types Appropriately

```csharp
public class Article
{
    public string Title { get; set; } = string.Empty;  // Required
    public string? Subtitle { get; set; }              // Optional
    public DateTime PublishedDate { get; set; }        // Required
    public DateTime? UpdatedDate { get; set; }         // Optional
}
```

### 4. Handle Arrays Correctly

```csharp
public class Article
{
    public string[] Tags { get; set; } = Array.Empty<string>();
    public List<string> Categories { get; set; } = new();
    // Both map to text[] in Weaviate
}
```

### 5. Use the Untyped Property for Unsupported Operations

```csharp
var typedClient = client.Collections.Use<Article>("Articles");

// If typed client doesn't support something, fall back to untyped
var untypedClient = typedClient.Untyped;
await untypedClient.SomeUnsupportedOperation();
```

### 6. Reuse Typed Clients

```csharp
// Create once, reuse throughout your application
public class ArticleService
{
    private readonly TypedCollectionClient<Article> _articles;

    public ArticleService(WeaviateClient client)
    {
        _articles = client.Collections.Use<Article>("Articles");
    }

    public async Task<Guid> CreateArticle(Article article)
    {
        return await _articles.Data.Insert(article);
    }

    public async Task<IEnumerable<Article>> SearchArticles(string query)
    {
        var results = await _articles.Query.NearText(query, limit: 10);
        return results.Objects.Select(o => o.Properties);
    }
}
```

## Anonymous Types

Typed clients also work with anonymous types for read-only operations:

```csharp
var articlesClient = client.Collections.Use<dynamic>("Articles");

var results = await articlesClient.Query.NearText("tutorials");

foreach (var result in results.Objects)
{
    // Dynamic access
    Console.WriteLine($"Title: {result.Properties.title}");
}
```

## Performance Considerations

- **Zero Runtime Overhead**: Typed clients are thin wrappers with no performance penalty
- **Validation Cost**: Type validation makes HTTP requests to fetch schemas. Use caching:

  ```csharp
  // Schema cache has 5-minute TTL by default
  var schema = await articlesClient.GetCachedConfig();
  ```

- **Serialization**: Uses the same efficient serialization as untyped clients

## Troubleshooting

### Type Mismatch Errors

**Problem**: `InvalidOperationException: Object of type 'Article' does not conform to schema`

**Solution**: Run validation to see detailed errors:

```csharp
var validation = await articlesClient.ValidateType();
Console.WriteLine(validation.GetDetailedMessage());
```

### Missing Properties

**Problem**: Properties in Weaviate schema but not in C# type

**Solution**: Add the missing properties or review warnings:

```csharp
var validation = await articlesClient.ValidateType();
foreach (var warning in validation.Warnings)
{
    Console.WriteLine($"⚠️  {warning.Message}");
}
```

### Array Mismatches

**Problem**: C# has `string` but schema expects `string[]`

**Solution**: Update your C# type to use an array or collection:

```csharp
// Before
public string Tags { get; set; }

// After
public string[] Tags { get; set; } = Array.Empty<string>();
```

## API Reference

Full API documentation is available in the XML comments. Access it through IntelliSense in your IDE or generate API documentation with a tool like DocFX.

## See Also

- [Weaviate C# Client Documentation](https://weaviate.io/developers/weaviate/client-libraries/dotnet)
- [Weaviate Schema Documentation](https://weaviate.io/developers/weaviate/manage-data/collections)
- [Property Data Types](https://weaviate.io/developers/weaviate/config-refs/datatypes)

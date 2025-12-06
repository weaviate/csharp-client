# Next Steps for Weaviate C# ORM - Copilot Instructions

**Created:** 2025-12-06
**Context:** The Weaviate C# ORM has completed Phases 1-5 (schema building, CRUD operations, querying, data mapping, migrations) plus fine-tuning for vector configuration. The next major improvements are focused on making this the **ultimate tool for interacting with Weaviate data from C#**.

---

## Priority 1: Vectorizer-Specific Configuration Properties 🎯

**Problem:**
Currently, `VectorAttribute<TVectorizer>` only supports common properties like `Model`, `Dimensions`, `BaseURL`, `SourceProperties`. However, each vectorizer has unique configuration properties that aren't accessible:

### Examples of Missing Vectorizer-Specific Properties:

1. **Text2VecOpenAI**
   - `Type` - Model type (text or code)
   - `VectorizeCollectionName` - Include collection name in vectorization

2. **Text2VecCohere**
   - `Truncate` - Truncation strategy (NONE, START, END)
   - `BaseURL` - Custom endpoint (already supported)

3. **Text2VecHuggingFace**
   - `WaitForModel` - Wait if model is loading
   - `UseGPU` - Use GPU acceleration
   - `UseCache` - Cache results

4. **Text2VecTransformers**
   - `PoolingStrategy` - Pooling strategy (masked_mean, cls)
   - `InferenceUrl` - Custom inference endpoint

5. **Multi2VecCLIP**
   - `InferenceUrl` - Custom inference endpoint
   - `Weights` - Text/image weight balance

6. **Ref2VecCentroid**
   - `Method` - Aggregation method (mean)
   - `ReferenceProperties` - Which references to aggregate

### Current Workaround:
Use `ConfigBuilder` property to provide a custom builder class, but this is cumbersome:

```csharp
[Vector<Vectorizer.Text2VecOpenAI>(
    Model = "text-embedding-3-small",
    ConfigBuilder = typeof(MyCustomOpenAIBuilder)
)]
public float[]? Embedding { get; set; }

// Requires creating a separate builder class
public class MyCustomOpenAIBuilder : IVectorConfigBuilder<Vectorizer.Text2VecOpenAI>
{
    public Vectorizer.Text2VecOpenAI Build(VectorAttributeBase attr)
    {
        // Manual configuration...
    }
}
```

### Proposed Solutions:

#### **Option A: Vectorizer-Specific Attribute Subclasses (Recommended)**

Create specialized attribute classes for each major vectorizer:

```csharp
// Instead of generic VectorAttribute<T>, use specialized classes
[VectorText2VecOpenAI(
    Name = "contentEmbedding",  // Vector name (property name if omitted)
    Model = "text-embedding-3-small",
    Dimensions = 1536,
    Type = OpenAIModelType.Text,
    VectorizeCollectionName = false,
    SourceProperties = [nameof(Title), nameof(Content)]
)]
public float[]? ContentEmbedding { get; set; }

[VectorText2VecCohere(
    Name = "titleVector",
    Model = "embed-multilingual-v3.0",
    Truncate = CohereTruncate.End,
    SourceProperties = [nameof(Title)]
)]
public float[]? TitleVector { get; set; }

[VectorMulti2VecCLIP(
    Name = "imageTextEmbedding",
    TextFields = [nameof(Description)],
    ImageFields = [nameof(ImageUrl)],
    Weights = new { text = 0.7f, image = 0.3f }
)]
public float[]? ImageTextEmbedding { get; set; }
```

**Implementation:**
1. Create base class `VectorAttributeBase` (already exists)
2. Create subclasses for each vectorizer family:
   - `VectorText2VecOpenAIAttribute`
   - `VectorText2VecCohereAttribute`
   - `VectorText2VecHuggingFaceAttribute`
   - `VectorText2VecTransformersAttribute`
   - `VectorMulti2VecCLIPAttribute`
   - `VectorRef2VecCentroidAttribute`
   - etc.
3. Update `VectorConfigBuilder.CreateVectorizer()` to handle both generic `VectorAttribute<T>` and specific subclasses
4. Keep generic `VectorAttribute<T>` for backward compatibility and less common vectorizers

**Pros:**
- IntelliSense shows only valid properties for each vectorizer
- Compile-time validation
- Clear, discoverable API
- Easy to document

**Cons:**
- More attribute classes to maintain
- Need to keep in sync with Weaviate vectorizer updates

#### **Option B: Dynamic Properties via Dictionary**

Allow arbitrary properties through a dictionary:

```csharp
[Vector<Vectorizer.Text2VecOpenAI>(
    Model = "text-embedding-3-small",
    AdditionalConfig = new Dictionary<string, object>
    {
        ["type"] = "text",
        ["vectorizeCollectionName"] = false
    }
)]
public float[]? ContentEmbedding { get; set; }
```

**Pros:**
- Flexible, supports all vectorizers
- No need for specific classes

**Cons:**
- No IntelliSense
- No compile-time validation
- Typo-prone
- Hard to discover available options

#### **Option C: Hybrid Approach** ⭐ **RECOMMENDED**

Combine both approaches:
1. Create specialized attributes for **common vectorizers** (OpenAI, Cohere, HuggingFace, Transformers, CLIP)
2. Keep generic `VectorAttribute<T>` with `AdditionalConfig` dictionary for **less common vectorizers**

```csharp
// Common vectorizers: Use specialized attributes
[VectorText2VecOpenAI(
    Model = "text-embedding-3-small",
    Type = OpenAIModelType.Text,
    VectorizeCollectionName = false
)]
public float[]? ContentEmbedding { get; set; }

// Less common vectorizers: Use generic attribute with dictionary
[Vector<Vectorizer.Text2VecGPT4All>(
    Model = "all-MiniLM-L6-v2",
    AdditionalConfig = new Dictionary<string, object>
    {
        ["gpu_layers"] = 35,
        ["threads"] = 4
    }
)]
public float[]? GPT4AllEmbedding { get; set; }
```

---

## Priority 2: Enhanced Query Builder (LINQ-like Experience)

**Problem:**
Current query API uses raw `where` filter syntax and manual property path construction.

**Current State:**
```csharp
var results = await client.QueryAsync<Article>(q => q
    .Where(w => w
        .Path("title")
        .Like("*Weaviate*"))
    .Limit(10));
```

**Proposed Enhancement:**
```csharp
// LINQ-style syntax with compile-time safety
var results = await client.Query<Article>()
    .Where(a => a.Title.Contains("Weaviate"))
    .OrderBy(a => a.CreatedAt)
    .Limit(10)
    .ToListAsync();

// Vector search with type safety
var results = await client.Query<Article>()
    .NearVector(a => a.ContentEmbedding, queryVector)
    .WithDistance()
    .Limit(5)
    .ToListAsync();

// Hybrid search
var results = await client.Query<Article>()
    .HybridSearch("machine learning", alpha: 0.7f)
    .Where(a => a.IsPublished == true)
    .Limit(10)
    .ToListAsync();
```

**Implementation Approach:**
1. Create `QueryBuilder<T>` class with fluent API
2. Use Expression trees to parse lambda expressions into Weaviate GraphQL
3. Support common LINQ operators: `Where`, `OrderBy`, `Select`, `Take`, `Skip`
4. Special vector search methods: `NearVector`, `NearText`, `NearObject`, `Hybrid`

---

## Priority 3: Relationship Navigation (EF Core-like Experience)

**Problem:**
References are just IDs - no automatic loading of related objects.

**Current State:**
```csharp
public class Article
{
    [Reference("Author")]
    public Guid AuthorId { get; set; }  // Just an ID
}

// Manual loading required
var article = await client.GetAsync<Article>(articleId);
var author = await client.GetAsync<Author>(article.AuthorId);
```

**Proposed Enhancement:**
```csharp
public class Article
{
    // ID-only reference (current)
    [Reference("Author")]
    public Guid AuthorId { get; set; }

    // Navigation property (new)
    [Reference("Author")]
    public Author? Author { get; set; }  // Null until loaded

    // Collection navigation (new)
    [Reference("Comments")]
    public List<Comment>? Comments { get; set; }
}

// Eager loading
var article = await client.Query<Article>()
    .Include(a => a.Author)
    .Include(a => a.Comments)
    .FirstAsync(a => a.Id == articleId);

// Lazy loading (optional)
var article = await client.GetAsync<Article>(articleId);
// author is null
await client.LoadReferenceAsync(article, a => a.Author);
// author is now populated
```

**Implementation Approach:**
1. Detect navigation properties by `[Reference]` attribute on non-Guid properties
2. `Include()` adds reference fields to GraphQL query
3. `LoadReferenceAsync()` method for lazy loading
4. Consider change tracking for `SaveChanges()` pattern

---

## Priority 4: Batch Operations and Performance

**Problem:**
No built-in batching for bulk operations.

**Proposed Enhancement:**
```csharp
// Batch insert
var articles = GetArticles(); // 1000 articles
await client.BulkInsertAsync(articles, batchSize: 100);

// Batch update
await client.BulkUpdateAsync(articles);

// Batch delete
await client.BulkDeleteAsync<Article>(articleIds);

// Transaction-like batching
using (var batch = client.BeginBatch())
{
    batch.Insert(article1);
    batch.Update(article2);
    batch.Delete<Article>(id3);

    await batch.CommitAsync();
}
```

---

## Priority 5: Migration Enhancements

**Current State:** ✅ Migrations work (CheckMigrate/Migrate)

**Proposed Enhancements:**
1. **Migration History Table**
   - Track applied migrations
   - Rollback support
   - Version tracking

2. **Data Migrations**
   - Not just schema changes
   - Transform existing data during migrations
   - Example: Rename property + copy data

3. **Migration Scripts**
   - Generate C# migration files (like EF Core)
   - Version control migrations
   - Team collaboration

```csharp
// Generated migration file
public class AddContentEmbeddingMigration : Migration
{
    public override void Up()
    {
        AddVectorProperty<Article>(
            a => a.ContentEmbedding,
            vectorizer: new Text2VecOpenAI { Model = "text-embedding-3-small" }
        );
    }

    public override void Down()
    {
        RemoveVectorProperty<Article>(a => a.ContentEmbedding);
    }
}
```

---

## Priority 6: Advanced Vector Operations

**Proposed Enhancements:**
1. **Multi-vector queries**
   ```csharp
   var results = await client.Query<Article>()
       .NearVector(a => a.TitleEmbedding, titleVector, weight: 0.3f)
       .NearVector(a => a.ContentEmbedding, contentVector, weight: 0.7f)
       .Limit(10)
       .ToListAsync();
   ```

2. **Generative search integration**
   ```csharp
   var results = await client.Query<Article>()
       .NearText("quantum computing")
       .GenerateAnswer("Explain quantum computing")
       .ToListAsync();

   foreach (var article in results)
   {
       Console.WriteLine(article.GeneratedAnswer);
   }
   ```

3. **Vector aggregations**
   ```csharp
   var centroid = await client.Query<Article>()
       .Where(a => a.Category == "AI")
       .AggregateVector(a => a.ContentEmbedding)
       .ToListAsync();
   ```

---

## Priority 7: Developer Experience Improvements

1. **Fluent Schema Builder** (alternative to attributes)
   ```csharp
   var schema = SchemaBuilder.For<Article>()
       .WithProperty(a => a.Title, p => p.AsText().Searchable())
       .WithProperty(a => a.Content, p => p.AsText())
       .WithVector(a => a.ContentEmbedding, v => v
           .UseOpenAI("text-embedding-3-small")
           .WithDimensions(1536)
           .FromProperties(nameof(Title), nameof(Content)))
       .WithIndex(a => a.ContentEmbedding, i => i
           .UseHNSW()
           .WithDistance(Distance.Cosine)
           .WithQuantizer<BQ>(q => q.RescoreLimit(200)))
       .Build();
   ```

2. **Validation Attributes**
   ```csharp
   [Property(DataType.Text)]
   [Required]
   [StringLength(1000)]
   public string Title { get; set; }

   [Property(DataType.Int)]
   [Range(0, 5)]
   public int Rating { get; set; }
   ```

3. **Auto-vectorization on Insert**
   ```csharp
   // Automatically call vectorizer API before insert
   var article = new Article
   {
       Title = "Hello World",
       Content = "..."
       // ContentEmbedding is null
   };

   await client.InsertAsync(article, autoVectorize: true);
   // ContentEmbedding is now populated by calling OpenAI
   ```

---

## Implementation Roadmap

### Phase 6: Vectorizer-Specific Configuration (2-3 days)
- [ ] Create specialized vectorizer attributes for top 5 vectorizers
- [ ] Add `AdditionalConfig` dictionary to generic `VectorAttribute<T>`
- [ ] Update VectorConfigBuilder to handle both approaches
- [ ] Add comprehensive tests
- [ ] Document all vectorizer-specific properties

### Phase 7: Query Builder Enhancement (3-4 days)
- [ ] Create LINQ-style QueryBuilder<T>
- [ ] Expression tree parsing for Where clauses
- [ ] Vector search methods (NearVector, NearText, Hybrid)
- [ ] Pagination and ordering
- [ ] Tests and documentation

### Phase 8: Relationship Navigation (4-5 days)
- [ ] Detect navigation properties
- [ ] Implement Include() for eager loading
- [ ] Implement LoadReferenceAsync() for lazy loading
- [ ] GraphQL query generation with references
- [ ] Tests for complex reference scenarios

### Phase 9: Batch Operations (2-3 days)
- [ ] BulkInsertAsync with configurable batch size
- [ ] BulkUpdateAsync and BulkDeleteAsync
- [ ] Transaction-like batch API
- [ ] Performance benchmarks

### Phase 10: Production Readiness (3-4 days)
- [ ] Connection pooling and retry logic
- [ ] Logging and diagnostics
- [ ] Performance monitoring
- [ ] Comprehensive error handling
- [ ] Production deployment guide

---

## Key Architectural Principles

1. **Type Safety First** - Leverage C# type system for compile-time validation
2. **IntelliSense Driven** - Discoverable API through IDE autocomplete
3. **Convention over Configuration** - Sensible defaults, explicit overrides
4. **Backward Compatibility** - Don't break existing code
5. **Performance** - Efficient GraphQL queries, batching, caching
6. **Testability** - Comprehensive unit and integration tests

---

## Questions to Resolve

1. **Vectorizer Attributes:** Should we create specialized attributes for ALL 47+ vectorizers or just the top 10?
2. **LINQ Limitations:** Which LINQ operators are feasible given Weaviate's GraphQL API?
3. **Change Tracking:** Do we implement EF Core-style change tracking for `SaveChanges()`?
4. **Async Everywhere:** Should we support synchronous methods or async-only?
5. **Dependency Injection:** Should we provide built-in DI integration for ASP.NET Core?

---

## Success Criteria

The Weaviate C# ORM will be the **ultimate tool** when:

✅ Developers can configure ANY vectorizer property declaratively
✅ Queries feel like LINQ, not raw GraphQL
✅ Navigation properties work like EF Core
✅ Bulk operations are trivial
✅ IntelliSense guides the entire development experience
✅ Type safety prevents 90% of runtime errors
✅ Documentation is comprehensive with real-world examples
✅ Performance matches or exceeds direct API usage

---

**Next Session:** Start with Priority 1 (Vectorizer-Specific Configuration) using the hybrid approach.

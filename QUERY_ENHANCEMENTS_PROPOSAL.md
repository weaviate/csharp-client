# Query API Enhancement Proposal

## Executive Summary

This document proposes practical enhancements to make the Weaviate C# client more idiomatic while respecting that vector databases have fundamentally different operations than SQL databases. We should **not** force SQL/LINQ patterns onto vector-specific operations, but we **can** make filtering, sorting, and projections more fluent and type-safe.

---

## Current API Analysis

### What's Already Good ✅

1. **Filter API** - Already has excellent fluent interface:
   ```csharp
   var filter = Filter.Property("price").GreaterThan(100)
                & Filter.Property("category").Equal("electronics");
   ```
   - Typed filters with `TypedValue<T>`, `TypedGuid`, etc.
   - Operator overloading (`&` for AND, `|` for OR)
   - Nice property access via `Filter.Property(name)`

2. **Async Enumerable Support** - Already has `Iterator()` for streaming large result sets

3. **Query Methods** - Clear, purpose-specific methods:
   - `FetchObjects()` - Basic retrieval
   - `NearText()`, `NearVector()`, `NearObject()` - Vector similarity
   - `BM25()` - Full-text search
   - `Hybrid()` - Combined vector + text search

4. **Dependency Injection** - Just completed comprehensive DI support with:
   - Async initialization
   - Multiple named clients
   - Configuration-based setup

### What Could Be More Idiomatic 🔧

1. **Too Many Parameters** - Methods have 10-15+ parameters:
   ```csharp
   await query.FetchObjects(
       limit: 100,
       filters: filter,
       sort: sort,
       rerank: rerank,
       tenant: "tenant1",
       returnProperties: props,
       returnReferences: refs,
       returnMetadata: metadata,
       includeVectors: vectors,
       cancellationToken: ct
   );
   ```

2. **String-Based Property Selection** - Not type-safe:
   ```csharp
   returnProperties: new[] { "name", "price", "description" }  // typo-prone
   ```

3. **Manual Sort Construction** - Could be more fluent:
   ```csharp
   var sort = new Sort { Property = "price", Ascending = false };
   ```

4. **No Fluent Query Building** - Everything is method parameters

---

## What Maps Well to LINQ Patterns

These operations have clear SQL/LINQ equivalents:

| Operation | LINQ | Current Weaviate API | Could Enhance? |
|-----------|------|---------------------|----------------|
| **Filtering** | `.Where(x => x.Price > 100)` | `Filter.Property("price").GreaterThan(100)` | ✅ Already good |
| **Sorting** | `.OrderBy(x => x.Price)` | `new Sort { Property = "price" }` | ✅ Could be more fluent |
| **Pagination** | `.Take(10).Skip(20)` | `limit: 10, offset: 20` | ✅ Could use LINQ methods |
| **Projection** | `.Select(x => new { x.Name, x.Price })` | `returnProperties: new[] { "name", "price" }` | ✅ Could be type-safe |
| **Aggregation** | `.Count()` | `collection.Count()` | ✅ Already good |

---

## What Does NOT Map to LINQ

These are fundamentally vector database operations with no SQL equivalent:

| Operation | Why LINQ Doesn't Fit |
|-----------|---------------------|
| **Vector Similarity** (`NearText`, `NearVector`) | No concept of "closeness" in SQL WHERE clauses |
| **BM25 Search** | Full-text relevance scoring, not equality/comparison |
| **Hybrid Search** | Combines vector + text with fusion algorithms |
| **Reranking** | Vector-specific result reordering |
| **Distance/Certainty** | Vector similarity metrics |
| **AutoCut** | Vector-specific result limiting |

**Recommendation**: Keep these as explicit named methods, don't try to make them LINQ-like.

---

## Proposed Enhancements

### 1. Fluent Query Builder Pattern

Instead of many parameters, use a builder pattern:

```csharp
// ❌ Current: Too many parameters
var results = await query.FetchObjects(
    limit: 100,
    filters: filter,
    sort: sortByPrice,
    returnProperties: new[] { "name", "price" },
    returnMetadata: MetadataQuery.All,
    cancellationToken: ct
);

// ✅ Proposed: Fluent builder
var results = await collection.Query
    .Where(Filter.Property("price").GreaterThan(100))
    .OrderBy("price")
    .Take(100)
    .Select(new[] { "name", "price" })
    .IncludeMetadata(MetadataQuery.All)
    .ExecuteAsync(ct);
```

### 2. Type-Safe Property Projections

Use expressions for compile-time safety:

```csharp
// ❌ Current: String-based, error-prone
returnProperties: new[] { "name", "price", "descriptin" }  // typo!

// ✅ Proposed: Expression-based
.Select(x => new { x.Name, x.Price, x.Description })

// Implementation uses expression trees to extract property names
```

### 3. Fluent Sort Builder

Make sorting more intuitive:

```csharp
// ❌ Current
var sort = new Sort { Property = "price", Ascending = false };

// ✅ Proposed: Fluent
.OrderByDescending("price")
.ThenBy("name")

// ✅ Or type-safe:
.OrderByDescending(x => x.Price)
.ThenBy(x => x.Name)
```

### 4. Pagination LINQ Methods

Add familiar LINQ methods:

```csharp
// ❌ Current
limit: 100, offset: 20

// ✅ Proposed
.Skip(20)
.Take(100)
```

### 5. Parameter Objects for Complex Scenarios

Group related parameters:

```csharp
// For vector search, keep explicit methods but use parameter objects
await collection.Query
    .NearText("search query", new NearTextOptions
    {
        Certainty = 0.7f,
        Limit = 100,
        TargetVector = "semantic",
        Filters = filter,
        Rerank = rerankConfig
    })
    .Select(x => new { x.Name, x.Description })
    .ExecuteAsync();
```

### 6. Query Builder for Complex Queries

```csharp
// Proposed: Unified query builder
var queryBuilder = collection.Query
    .Where(Filter.Property("category").Equal("electronics")
         & Filter.Property("price").LessThan(1000))
    .OrderBy("price")
    .Take(50);

// Execute as basic fetch
var results = await queryBuilder.ExecuteAsync();

// Or upgrade to vector search
var semanticResults = await queryBuilder
    .NearText("affordable laptops")
    .WithCertainty(0.7f)
    .ExecuteAsync();

// Or hybrid search
var hybridResults = await queryBuilder
    .Hybrid("gaming laptop", new HybridOptions { Alpha = 0.5f })
    .ExecuteAsync();
```

---

## Implementation Approach

### Phase 1: Non-Breaking Additions

Add new fluent API alongside existing methods:

```csharp
// Existing API continues to work
await query.FetchObjects(limit: 100, filters: filter);

// New fluent API available as alternative
await collection.Query
    .Where(filter)
    .Take(100)
    .ExecuteAsync();
```

### Phase 2: Query Builder Class

```csharp
public class QueryBuilder<T>
{
    private Filter? _filter;
    private List<Sort> _sorts = new();
    private uint? _limit;
    private uint? _offset;
    private OneOrManyOf<string>? _returnProperties;
    private MetadataQuery? _returnMetadata;

    public QueryBuilder<T> Where(Filter filter)
    {
        _filter = _filter == null ? filter : _filter & filter;
        return this;
    }

    public QueryBuilder<T> OrderBy(string property)
    {
        _sorts.Add(new Sort { Property = property, Ascending = true });
        return this;
    }

    public QueryBuilder<T> OrderByDescending(string property)
    {
        _sorts.Add(new Sort { Property = property, Ascending = false });
        return this;
    }

    public QueryBuilder<T> Take(uint limit)
    {
        _limit = limit;
        return this;
    }

    public QueryBuilder<T> Skip(uint offset)
    {
        _offset = offset;
        return this;
    }

    public QueryBuilder<T> Select(params string[] properties)
    {
        _returnProperties = properties;
        return this;
    }

    // Type-safe version using expressions
    public QueryBuilder<T> Select<TProjection>(Expression<Func<T, TProjection>> selector)
    {
        var properties = ExtractPropertyNames(selector);
        _returnProperties = properties;
        return this;
    }

    public QueryBuilder<T> IncludeMetadata(MetadataQuery metadata)
    {
        _returnMetadata = metadata;
        return this;
    }

    // Execute as basic fetch
    public async Task<WeaviateResult> ExecuteAsync(CancellationToken ct = default)
    {
        return await _queryClient.FetchObjects(
            limit: _limit,
            offset: _offset,
            filters: _filter,
            sort: _sorts,
            returnProperties: _returnProperties,
            returnMetadata: _returnMetadata,
            cancellationToken: ct
        );
    }

    // Upgrade to vector search
    public VectorQueryBuilder<T> NearText(string text, float? certainty = null)
    {
        return new VectorQueryBuilder<T>(this, new NearTextConfig(text, certainty));
    }

    public VectorQueryBuilder<T> NearVector(Vectors vector, float? distance = null)
    {
        return new VectorQueryBuilder<T>(this, new NearVectorConfig(vector, distance));
    }

    public HybridQueryBuilder<T> Hybrid(string query, HybridOptions? options = null)
    {
        return new HybridQueryBuilder<T>(this, query, options);
    }
}
```

### Phase 3: Type-Safe Projections

```csharp
private static string[] ExtractPropertyNames<T, TProjection>(
    Expression<Func<T, TProjection>> selector)
{
    // For new { x.Name, x.Price }
    if (selector.Body is NewExpression newExpr)
    {
        return newExpr.Arguments
            .OfType<MemberExpression>()
            .Select(m => m.Member.Name.Decapitalize())
            .ToArray();
    }

    // For x => x.Name
    if (selector.Body is MemberExpression memberExpr)
    {
        return new[] { memberExpr.Member.Name.Decapitalize() };
    }

    throw new ArgumentException("Invalid projection expression");
}
```

---

## Benefits

1. **More Idiomatic C#** - Familiar LINQ-style methods where they make sense
2. **Type Safety** - Compile-time errors for property typos
3. **Better Discoverability** - IntelliSense guides developers
4. **Less Ceremony** - Fewer parameters to pass
5. **Composable Queries** - Build queries step by step
6. **Backward Compatible** - Existing code continues to work
7. **Respects Vector DB Nature** - Doesn't force SQL patterns on vector operations

---

## What We're NOT Proposing

1. ❌ **IQueryable Provider** - Too complex, doesn't fit vector operations well
2. ❌ **LINQ Query Syntax** - `from x in collection where x.Price > 100 select x` doesn't map to vector searches
3. ❌ **Entity Framework Integration** - EF Core is for CRUD, vector DBs are for search
4. ❌ **Automatic Expression Translation** - Complex, brittle, not worth it
5. ❌ **Hiding Vector Operations** - Keep `NearText()`, `NearVector()` explicit and clear

---

## Example: Before and After

### Current API

```csharp
var collection = client.Collections.Use<Product>("Product");

var filter = Filter.Property("category").Equal("electronics")
           & Filter.Property("price").LessThan(1000);

var sort = new Sort { Property = "price", Ascending = true };

var results = await collection.Query.FetchObjects(
    limit: 50,
    filters: filter,
    sort: sort,
    returnProperties: new[] { "name", "price", "description" },
    returnMetadata: MetadataQuery.All,
    cancellationToken: cancellationToken
);

foreach (var obj in results.Objects)
{
    var product = obj.As<Product>();
    Console.WriteLine($"{product.Name}: ${product.Price}");
}
```

### Proposed API (Option 1: String-based, simpler)

```csharp
var collection = client.Collections.Use<Product>("Product");

var results = await collection.Query
    .Where(Filter.Property("category").Equal("electronics")
         & Filter.Property("price").LessThan(1000))
    .OrderBy("price")
    .Take(50)
    .Select("name", "price", "description")
    .IncludeMetadata(MetadataQuery.All)
    .ExecuteAsync(cancellationToken);

foreach (var obj in results)
{
    var product = obj.As<Product>();
    Console.WriteLine($"{product.Name}: ${product.Price}");
}
```

### Proposed API (Option 2: Type-safe, more advanced)

```csharp
var collection = client.Collections.Use<Product>("Product");

var results = await collection.Query
    .Where(x => x.Category == "electronics" && x.Price < 1000)
    .OrderBy(x => x.Price)
    .Take(50)
    .Select(x => new { x.Name, x.Price, x.Description })
    .IncludeMetadata(MetadataQuery.All)
    .ExecuteAsync(cancellationToken);

foreach (var product in results)
{
    Console.WriteLine($"{product.Name}: ${product.Price}");
}
```

### Vector Search with Proposed API

```csharp
// Still explicit about vector operations, but cleaner parameter passing
var results = await collection.Query
    .Where(Filter.Property("category").Equal("electronics"))
    .OrderBy("price")
    .Take(50)
    .NearText("affordable laptop", certainty: 0.7f)
    .Select(x => new { x.Name, x.Price, x.Description })
    .ExecuteAsync();
```

---

## Next Steps

1. **Validate Approach** - Get feedback on this proposal
2. **Choose Option** - String-based (simpler) or type-safe (more advanced)?
3. **Implement QueryBuilder** - Core fluent API
4. **Add Extension Methods** - `.Where()`, `.OrderBy()`, `.Take()`, etc.
5. **Add Type-Safe Projections** - Expression tree parsing (if chosen)
6. **Write Tests** - Comprehensive test coverage
7. **Update Documentation** - Examples and migration guide
8. **Iterate Based on Feedback** - Refine based on real usage

---

## Open Questions

1. **How far should type-safety go?**
   - Option A: Keep string-based (simpler, works today)
   - Option B: Full expression trees (complex, better IntelliSense)

2. **Should we support LINQ query syntax?**
   ```csharp
   from p in collection.Query
   where p.Price < 1000
   select new { p.Name, p.Price }
   ```
   - Pros: Familiar to C# developers
   - Cons: Doesn't fit vector operations, significant complexity

3. **IAsyncEnumerable integration?**
   ```csharp
   await foreach (var product in collection.Query.Where(...).AsAsyncEnumerable())
   {
       // Process
   }
   ```

4. **How to handle the "upgrade" from basic to vector queries?**
   - Current proposal: Start with `Query` builder, call `.NearText()` to upgrade
   - Alternative: Separate entry points?

---

## Conclusion

The Weaviate C# client is already quite good. The main improvements should focus on:

1. ✅ **Fluent query building** - Reduce parameter overload
2. ✅ **Type-safe projections** - Catch typos at compile time
3. ✅ **Familiar LINQ methods** - `.Where()`, `.OrderBy()`, `.Take()` where they fit
4. ✅ **Parameter objects** - Group related options
5. ❌ **NOT IQueryable** - Too complex, doesn't fit vector paradigm
6. ❌ **NOT hiding vector operations** - Keep them explicit and discoverable

This approach makes the API more idiomatic C# while respecting that vector databases are fundamentally different from SQL databases.

# Microsoft.Extensions.VectorData Integration

The `Weaviate.Client.VectorData` package provides a first-class implementation of the
[Microsoft.Extensions.VectorData](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.vectordata)
abstractions for Weaviate. This enables Weaviate to participate in the standard .NET AI ecosystem —
including [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) — using a
vendor-neutral interface.

## Table of Contents

- [Why VectorData?](#why-vectordata)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Defining Record Types](#defining-record-types)
- [Dependency Injection](#dependency-injection)
- [Collection Management](#collection-management)
- [CRUD Operations](#crud-operations)
- [Vector Search](#vector-search)
- [Hybrid Search](#hybrid-search)
- [Filtering](#filtering)
- [Schema Mapping Reference](#schema-mapping-reference)
- [Multi-Tenancy and Consistency Level](#multi-tenancy-and-consistency-level)
- [Dynamic Collections](#dynamic-collections)
- [Limitations](#limitations)

---

## Why VectorData?

`Microsoft.Extensions.VectorData` is a vendor-neutral abstraction layer for .NET vector databases.
By targeting its interfaces (`VectorStore`, `VectorStoreCollection<TKey, TRecord>`) instead of
Weaviate-specific types, your application can:

- **Switch vector databases** by swapping the registered implementation without changing application
  code.
- **Use Semantic Kernel** plug-ins and memory connectors that accept `VectorStore` directly.
- **Write portable integration tests** against an in-memory or mock store.

Use this package when you want portability. Use the core `Weaviate.Client` package directly when
you need Weaviate-specific features such as `NearText`, generative search, cross-references, or
fine-grained BM25 control.

---

## Installation

Install both the core client and the VectorData integration package:

```bash
dotnet add package Weaviate.Client
dotnet add package Weaviate.Client.VectorData
```

Or via `.csproj`:

```xml
<PackageReference Include="Weaviate.Client" Version="1.0.0" />
<PackageReference Include="Weaviate.Client.VectorData" Version="1.0.0" />
```

---

## Quick Start

```csharp
using Microsoft.Extensions.VectorData;
using Weaviate.Client;
using Weaviate.Client.VectorData;

// 1. Define a record type
public class Article
{
    [VectorStoreKey]
    public Guid Id { get; set; }

    [VectorStoreData(IsIndexed = true, IsFullTextIndexed = true)]
    public string Title { get; set; } = "";

    [VectorStoreData]
    public string Body { get; set; } = "";

    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public float[] Embedding { get; set; } = [];
}

// 2. Connect and wrap
var weaviateClient = await Connect.Cloud("my-cluster.weaviate.cloud", "api-key");
var store = new WeaviateVectorStore(weaviateClient);

// 3. Get a collection and ensure it exists
var articles = store.GetCollection<Guid, Article>("Article");
await articles.EnsureCollectionExistsAsync();

// 4. Upsert records
await articles.UpsertAsync(new Article
{
    Id = Guid.NewGuid(),
    Title = "Vector databases explained",
    Body = "...",
    Embedding = new float[1536] // supply a real embedding here
});

// 5. Search
await foreach (var result in articles.SearchAsync(myQueryEmbedding, top: 5))
{
    Console.WriteLine($"[{result.Score:F4}] {result.Record.Title}");
}
```

---

## Defining Record Types

Use attributes from `Microsoft.Extensions.VectorData` to describe your record schema.

### `[VectorStoreKey]`

Marks the property used as the Weaviate object UUID. Supported CLR types: `Guid` and `string`
(strings are parsed as UUIDs at runtime).

```csharp
[VectorStoreKey]
public Guid Id { get; set; }
```

### `[VectorStoreData]`

Marks a regular data property. Optional parameters control indexing:

| Parameter | Effect |
|-----------|--------|
| `IsIndexed = true` | Enables filterable (inverted) index — required for filter operations |
| `IsFullTextIndexed = true` | Enables BM25 text search index |
| `StorageName = "..."` | Override the Weaviate property name (defaults to camelCase of the CLR name) |

```csharp
[VectorStoreData(IsIndexed = true)]
public string Genre { get; set; } = "";

[VectorStoreData(IsFullTextIndexed = true)]
public string Description { get; set; } = "";
```

### `[VectorStoreVector]`

Marks a vector property. The first constructor argument is the vector dimension.

```csharp
[VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
public float[] Embedding { get; set; } = [];
```

Supported vector CLR types: `float[]`, `ReadOnlyMemory<float>`, `double[]`.

Multiple named vectors on a single record are supported — each becomes a separate Weaviate named
vector config:

```csharp
[VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
public float[] TitleEmbedding { get; set; } = [];

[VectorStoreVector(768, DistanceFunction = DistanceFunction.DotProductSimilarity)]
public float[] BodyEmbedding { get; set; } = [];
```

---

## Dependency Injection

### Register the vector store

```csharp
using Weaviate.Client.DependencyInjection;
using Weaviate.Client.VectorData.DependencyInjection;

// Register the core Weaviate client first
builder.Services.AddWeaviate(options =>
{
    options.RestEndpoint = "localhost";
});

// Then register the VectorStore wrapper
builder.Services.AddWeaviateVectorStore();
```

`AddWeaviateVectorStore` registers both `WeaviateVectorStore` and `VectorStore` as singletons, so
you can inject either type.

#### Per-collection options factory

Use `CollectionOptionsFactory` to configure tenant or consistency level per collection:

```csharp
builder.Services.AddWeaviateVectorStore(opts =>
{
    opts.CollectionOptionsFactory = collectionName => new WeaviateVectorStoreCollectionOptions
    {
        Tenant = "tenant-a",
        ConsistencyLevel = ConsistencyLevels.QUORUM,
    };
});
```

### Register a specific collection

To inject a specific typed collection directly:

```csharp
builder.Services.AddWeaviateVectorStoreCollection<Guid, Article>(
    collectionName: "Article",
    configure: opts =>
    {
        opts.ConsistencyLevel = ConsistencyLevels.ONE;
    });
```

This registers `WeaviateVectorStoreCollection<Guid, Article>` and
`VectorStoreCollection<Guid, Article>` as singletons.

### Inject in your service

```csharp
public class ArticleService
{
    private readonly VectorStoreCollection<Guid, Article> _articles;

    public ArticleService(VectorStoreCollection<Guid, Article> articles)
    {
        _articles = articles;
    }
}
```

---

## Collection Management

```csharp
var store = new WeaviateVectorStore(client);

// List all collection names
await foreach (var name in store.ListCollectionNamesAsync())
    Console.WriteLine(name);

// Check existence
bool exists = await store.CollectionExistsAsync("Article");

// Create if missing (derives schema from TRecord attributes or provided definition)
var articles = store.GetCollection<Guid, Article>("Article");
await articles.EnsureCollectionExistsAsync();

// Delete if present
await articles.EnsureCollectionDeletedAsync();
```

`EnsureCollectionExistsAsync` auto-creates the Weaviate collection using the schema derived from
either the record type's VectorData attributes or an explicit `VectorStoreCollectionDefinition`.

---

## CRUD Operations

### Upsert

```csharp
// Single record
await articles.UpsertAsync(article);

// Multiple records (sequential — see Limitations)
await articles.UpsertAsync(new[] { article1, article2, article3 });
```

Upsert uses insert-then-replace semantics: it attempts an insert and falls back to a full replace
if the object already exists. There is no native batch replace API in Weaviate, so multi-record
upsert iterates individually.

### Get by key

```csharp
// Single
var article = await articles.GetAsync(myGuid);
var articleWithVectors = await articles.GetAsync(myGuid, new RecordRetrievalOptions { IncludeVectors = true });

// Multiple
await foreach (var a in articles.GetAsync(new[] { id1, id2, id3 }))
    Console.WriteLine(a.Title);
```

### Get by filter

```csharp
await foreach (var a in articles.GetAsync(
    filter: x => x.Genre == "Science Fiction",
    top: 20))
{
    Console.WriteLine(a.Title);
}
```

### Delete

```csharp
// Single
await articles.DeleteAsync(myGuid);

// Multiple — uses Weaviate's batch delete internally
await articles.DeleteAsync(new[] { id1, id2, id3 });
```

---

## Vector Search

```csharp
float[] queryEmbedding = /* your embedding */;

await foreach (var result in articles.SearchAsync(queryEmbedding, top: 10))
{
    Console.WriteLine($"Score: {result.Score:F4} — {result.Record.Title}");
}
```

### Search options

```csharp
await foreach (var result in articles.SearchAsync(
    queryEmbedding,
    top: 10,
    options: new VectorSearchOptions<Article>
    {
        Filter = x => x.Genre == "Science Fiction" && x.Title != null,
        Skip = 20,                     // pagination offset
        IncludeVectors = false,        // omit vector data from results
        ScoreThreshold = 0.8,          // maximum distance (closer = lower distance in cosine)
    }))
{ ... }
```

### Targeting a named vector

When your record has multiple vector properties, use `VectorProperty` to specify which one to
search against:

```csharp
await foreach (var result in articles.SearchAsync(
    queryEmbedding,
    top: 5,
    options: new VectorSearchOptions<Article>
    {
        VectorProperty = x => x.BodyEmbedding,
    }))
{ ... }
```

If there is exactly one vector property, it is selected automatically.

### Supported input types

| Type | Notes |
|------|-------|
| `float[]` | Native format, no conversion |
| `ReadOnlyMemory<float>` | Converted to `float[]` |
| `double[]` | Each element cast to `float` |

---

## Hybrid Search

Hybrid search combines vector similarity with keyword (BM25) scoring:

```csharp
await foreach (var result in articles.HybridSearchAsync(
    searchValue: queryEmbedding,
    keywords: new[] { "vector", "database" },
    top: 10))
{
    Console.WriteLine($"Score: {result.Score:F4} — {result.Record.Title}");
}
```

Options mirror `SearchAsync`: `Filter`, `Skip`, `IncludeVectors`, `VectorProperty`.

> **Note:** BM25 scoring requires `IsFullTextIndexed = true` on the relevant data property.

---

## Filtering

Filters in `GetAsync` and `SearchAsync` use standard LINQ expression syntax. The filter translator
converts these to Weaviate's native filter format at runtime.

### Supported operators

| Expression | Weaviate equivalent |
|---|---|
| `x.Prop == value` | Equal |
| `x.Prop != value` | NotEqual |
| `x.Prop > value` | GreaterThan |
| `x.Prop >= value` | GreaterThanOrEqual |
| `x.Prop < value` | LessThan |
| `x.Prop <= value` | LessThanOrEqual |
| `x.Prop == null` | IsNull(true) |
| `x.Prop != null` | IsNull(false) |
| `expr1 && expr2` | AllOf |
| `expr1 \|\| expr2` | AnyOf |
| `!expr` | Not |
| `x.Tags.Contains(value)` | ContainsAny |

### Examples

```csharp
// Equality
filter: x => x.Genre == "Fantasy"

// Range
filter: x => x.Year >= 2020 && x.Year < 2025

// Null check
filter: x => x.Subtitle != null

// Captured variable
string targetGenre = "Science Fiction";
filter: x => x.Genre == targetGenre

// Tag membership
filter: x => x.Tags.Contains("recommended")

// Compound
filter: x => x.Genre == "Fantasy" || (x.Year >= 2020 && x.Rating > 4.5)
```

> **Important:** Properties used in filters must have `IsIndexed = true` (or `IndexFilterable =
> true` in an explicit definition) for the filter to work correctly.

---

## Schema Mapping Reference

### CLR types → Weaviate DataType

| CLR Type | Weaviate DataType |
|---|---|
| `string` | Text |
| `int`, `long`, `short`, `byte` | Int |
| `float`, `double`, `decimal` | Number |
| `bool` | Bool |
| `DateTime`, `DateTimeOffset` | Date |
| `Guid` | Uuid |
| `string[]`, `List<string>` | TextArray |
| `int[]`, `List<int>`, `long[]`, `List<long>` | IntArray |
| `float[]`, `List<float>`, `double[]`, `List<double>` | NumberArray |
| `bool[]`, `List<bool>` | BoolArray |

Nullable variants (e.g. `int?`) are automatically unwrapped.

### Distance functions

| `DistanceFunction` constant | Weaviate distance |
|---|---|
| `CosineSimilarity` / `CosineDistance` | Cosine (default) |
| `DotProductSimilarity` / `NegativeDotProductSimilarity` | Dot |
| `EuclideanDistance` / `EuclideanSquaredDistance` | L2-Squared |
| `HammingDistance` | Hamming |
| `ManhattanDistance` | **Not supported** — throws `NotSupportedException` |
| *(none / null)* | Cosine |

### Vector index kinds

| `IndexKind` constant | Weaviate index |
|---|---|
| `Hnsw` / *(null)* | HNSW (default) |
| `Flat` | Flat (brute-force) |

---

## Multi-Tenancy and Consistency Level

Use `WeaviateVectorStoreCollectionOptions` to configure per-collection tenant pinning and
consistency level:

```csharp
// Direct construction
var options = new WeaviateVectorStoreCollectionOptions
{
    Tenant = "tenant-a",
    ConsistencyLevel = ConsistencyLevels.QUORUM,
};

var collection = new WeaviateVectorStoreCollection<Guid, Article>(
    client, "Article", options: options);
```

Or via DI, using the per-collection factory:

```csharp
builder.Services.AddWeaviateVectorStore(opts =>
{
    opts.CollectionOptionsFactory = name => name switch
    {
        "Article" => new WeaviateVectorStoreCollectionOptions { Tenant = "tenant-a" },
        _         => new WeaviateVectorStoreCollectionOptions(),
    };
});
```

> **Note:** The tenant and consistency level are applied to the underlying `CollectionClient` on
> first use and do not change during the collection's lifetime. Construct a new collection instance
> to switch tenants at runtime.

---

## Dynamic Collections

For scenarios where the record schema is not known at compile time, use a
`Dictionary<string, object?>` record with an explicit `VectorStoreCollectionDefinition`:

```csharp
var definition = new VectorStoreCollectionDefinition
{
    Properties = new List<VectorStoreProperty>
    {
        new VectorStoreKeyProperty("Id", typeof(Guid)),
        new VectorStoreDataProperty("Title", typeof(string)) { IsIndexed = true },
        new VectorStoreVectorProperty("Embedding", typeof(float[]))
        {
            Dimensions = 1536,
            DistanceFunction = DistanceFunction.CosineSimilarity,
        },
    }
};

var collection = store.GetDynamicCollection("Article", definition);

// Upsert using dictionary
await collection.UpsertAsync(new Dictionary<string, object?>
{
    ["Key"]       = Guid.NewGuid(),
    ["title"]     = "My article",
    ["embedding"] = new float[1536],
});
```

The `"Key"` entry in the dictionary maps to the Weaviate UUID.

---

## Limitations

- **`ManhattanDistance`** is not supported by Weaviate and will throw `NotSupportedException` at
  schema creation time.
- **Bulk upsert is sequential**: `UpsertAsync(IEnumerable<TRecord>)` inserts records one by one.
  For high-throughput ingestion, use the core client's batch API
  ([BATCH_API_USAGE.md](BATCH_API_USAGE.md)) instead.
- **No cross-references**: Weaviate cross-reference properties are not supported through the
  VectorData abstraction.
- **No NearText / generative search**: These Weaviate-specific features require the core client.
  Use `VectorStore.GetService<WeaviateClient>()` or inject `WeaviateClient` directly for these
  use cases.
- **Key type must be `Guid` or `string`**: Other key types will throw `NotSupportedException` at
  collection construction time. String keys are parsed as GUIDs.
- **`VectorSearchOptions.OldFilter` is not supported**: Use the `Filter` LINQ expression property
  instead.

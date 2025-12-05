# Weaviate.Client.Orm

A declarative ORM layer for the Weaviate C# client, providing attribute-based schema definition and type-safe LINQ-style queries.

## Status

🚧 **Under Development** - Core schema building functionality implemented. Query builder and object mapping coming soon.

## Features

### ✅ Implemented
- Attribute-based collection schema definition
- Property configuration with indexing options
- Named vector configuration (47+ vectorizer types supported)
- Reference definitions
- Nested object support
- Inverted index configuration
- Schema builder with automatic property name conversion (PascalCase → camelCase)

### 🚧 In Progress
- Type-safe query builder with LINQ-style API
- Expression tree to Filter conversion
- Object mapper with automatic vector/reference handling
- Complete extension methods for data operations

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

- `[WeaviateCollection]` - Define collection name and description
- `[InvertedIndex]` - Configure inverted index settings

### Property-Level Attributes

- `[Property]` - Define Weaviate property with data type
- `[Index]` - Configure filtering, searching, and range filters
- `[Tokenization]` - Specify tokenization strategy for text properties
- `[NestedType]` - Define nested object structure for Object/ObjectArray types

### Vector Configuration

- `[Vector<TVectorizer>]` - Define named vector with vectorizer configuration
  - Property name becomes vector name
  - Property type determines single (`float[]`) vs multi-vector (`float[,]`)
  - Supports all 47+ Weaviate vectorizer types

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

## Examples

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

## Roadmap

- [ ] Query builder with LINQ-style API
- [ ] Automatic object mapping for insert/retrieve
- [ ] Vector property population on retrieval
- [ ] Reference expansion support
- [ ] Expression tree to Filter conversion
- [ ] Complete test coverage
- [ ] Documentation and examples

## Dependencies

- `Weaviate.Client` - Core Weaviate client (referenced as project dependency)
- `Humanizer.Core` - String transformations (PascalCase → camelCase)

## License

Same as Weaviate C# client - Apache 2.0

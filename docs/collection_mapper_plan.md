# Weaviate.Client.CollectionMapper - Implementation Plan

## Overview

This document outlines the complete implementation plan for the Weaviate C# WOMP layer. The WOMP provides a declarative, attribute-based approach to defining collection schemas and type-safe LINQ-style query capabilities.

## Goals

1. **Declarative Schema Definition**: Use C# attributes to define collection schemas
2. **Type-Safe Queries**: LINQ-style fluent API with compile-time safety
3. **Automatic Mapping**: Seamless conversion between C# objects and Weaviate objects
4. **Vector Property Support**: Named vectors as first-class properties
5. **Reference Handling**: Type-safe cross-references between collections
6. **Zero Breaking Changes**: Build entirely on top of existing `Weaviate.Client` without modifications

## Architecture

### Project Structure

```
src/
  Weaviate.Client/                    # Existing - NO CHANGES
  Weaviate.Client.CollectionMapper/                # NEW - All WOMP code
    ├── Attributes/
    │   ├── WeaviateCollectionAttribute.cs
    │   ├── PropertyAttribute.cs
    │   ├── VectorAttribute.cs
    │   ├── ReferenceAttribute.cs
    │   ├── IndexAttribute.cs
    │   ├── TokenizationAttribute.cs
    │   ├── InvertedIndexAttribute.cs
    │   └── NestedTypeAttribute.cs
    ├── Schema/
    │   ├── CollectionSchemaBuilder.cs
    │   ├── PropertyMapper.cs
    │   └── VectorConfigBuilder.cs
    ├── Query/
    │   ├── CollectionMapperQueryClient.cs
    │   ├── ExpressionToFilterConverter.cs
    │   └── QueryBuilder.cs
    ├── Mapping/
    │   ├── CollectionMapperObjectMapper.cs
    │   ├── VectorMapper.cs
    │   └── ReferenceMapper.cs
    ├── Extensions/
    │   ├── WeaviateClientExtensions.cs
    │   └── CollectionClientExtensions.cs
    └── Internal/
        └── PropertyHelper.cs
```

### Dependencies

**New Dependencies:**
- `Humanizer.Core` (2.14.1) - For string transformations (PascalCase → camelCase)

**Built-in (No Package Required):**
- `System.Linq.Expressions` - Expression tree parsing
- `System.ComponentModel.DataAnnotations` - Validation attributes
- `System.Reflection` - Attribute inspection

## Implementation Phases

### Phase 1: Project Setup and Attributes

#### Step 1.1: Create Project File
**File:** `src/Weaviate.Client.CollectionMapper/Weaviate.Client.CollectionMapper.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Weaviate.Client\Weaviate.Client.csproj" />
    <PackageReference Include="Humanizer.Core" Version="2.14.1" />
  </ItemGroup>
</Project>
```

#### Step 1.2: Define Collection-Level Attributes
**File:** `src/Weaviate.Client.CollectionMapper/Attributes/WeaviateCollectionAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class WeaviateCollectionAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

**File:** `src/Weaviate.Client.CollectionMapper/Attributes/InvertedIndexAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class InvertedIndexAttribute : Attribute
{
    public bool IndexTimestamps { get; set; }
    public bool IndexNullState { get; set; }
    public bool IndexPropertyLength { get; set; }
    public int CleanupIntervalSeconds { get; set; } = 60;
}
```

#### Step 1.3: Define Property-Level Attributes
**File:** `src/Weaviate.Client.CollectionMapper/Attributes/PropertyAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PropertyAttribute : Attribute
{
    public DataType DataType { get; }
    public string? Description { get; set; }

    public PropertyAttribute(DataType dataType)
    {
        DataType = dataType;
    }
}
```

**File:** `src/Weaviate.Client.CollectionMapper/Attributes/IndexAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class IndexAttribute : Attribute
{
    public bool Filterable { get; set; }
    public bool Searchable { get; set; }
    public bool RangeFilters { get; set; }
}
```

**File:** `src/Weaviate.Client.CollectionMapper/Attributes/TokenizationAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TokenizationAttribute : Attribute
{
    public PropertyTokenization Tokenization { get; }

    public TokenizationAttribute(PropertyTokenization tokenization)
    {
        Tokenization = tokenization;
    }
}
```

#### Step 1.4: Define Vector Attribute (Generic)
**File:** `src/Weaviate.Client.CollectionMapper/Attributes/VectorAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class VectorAttribute<TVectorizer> : Attribute
    where TVectorizer : VectorizerConfig
{
    // Common properties all vectorizers support
    public string[]? SourceProperties { get; set; }
    public bool VectorizeCollectionName { get; set; }

    // Text2Vec specific properties
    public string? Model { get; set; }
    public int? Dimensions { get; set; }
    public string? BaseURL { get; set; }

    // Multi2Vec specific properties
    public string[]? TextFields { get; set; }
    public string[]? ImageFields { get; set; }
    public string[]? VideoFields { get; set; }

    // Ref2Vec specific properties
    public string[]? ReferenceProperties { get; set; }

    // For complex configurations that don't fit in attributes
    public Type? ConfigBuilder { get; set; }
}

// Non-generic base for reflection
public abstract class VectorAttributeBase : Attribute
{
    public abstract Type VectorizerType { get; }
}
```

**File:** `src/Weaviate.Client.CollectionMapper/Attributes/VectorIndexAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class VectorIndexAttribute<TIndexType> : Attribute
    where TIndexType : VectorIndexConfig
{
    // HNSW specific
    public VectorDistance? Distance { get; set; }
    public int? EfConstruction { get; set; }
    public int? MaxConnections { get; set; }

    // For complex configurations
    public Type? ConfigBuilder { get; set; }
}
```

#### Step 1.5: Define Reference Attribute
**File:** `src/Weaviate.Client.CollectionMapper/Attributes/ReferenceAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ReferenceAttribute : Attribute
{
    public string TargetCollection { get; }
    public string? Description { get; set; }

    public ReferenceAttribute(string targetCollection)
    {
        TargetCollection = targetCollection;
    }
}
```

#### Step 1.6: Define Nested Type Attribute
**File:** `src/Weaviate.Client.CollectionMapper/Attributes/NestedTypeAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NestedTypeAttribute : Attribute
{
    public Type NestedType { get; }

    public NestedTypeAttribute(Type nestedType)
    {
        NestedType = nestedType;
    }
}
```

### Phase 2: Schema Building

#### Step 2.1: Property Helper Utilities
**File:** `src/Weaviate.Client.CollectionMapper/Internal/PropertyHelper.cs`

```csharp
internal static class PropertyHelper
{
    /// <summary>
    /// Convert C# property name to Weaviate property name (camelCase)
    /// Uses Humanizer for consistent transformation
    /// </summary>
    public static string ToCamelCase(string propertyName)
    {
        return propertyName.Camelize();
    }

    /// <summary>
    /// Extract property name from lambda expression: x => x.Property
    /// </summary>
    public static string GetPropertyName<T, TProp>(Expression<Func<T, TProp>> selector)
    {
        if (selector.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        // Handle conversions: x => (object)x.Property
        if (selector.Body is UnaryExpression unary &&
            unary.Operand is MemberExpression unaryMember)
        {
            return unaryMember.Member.Name;
        }

        throw new ArgumentException("Expression must be a property access", nameof(selector));
    }

    /// <summary>
    /// Extract nested property path: x => x.Category.Name -> "category.name"
    /// </summary>
    public static string GetNestedPropertyPath(MemberExpression member)
    {
        var parts = new Stack<string>();
        var current = member;

        while (current != null)
        {
            parts.Push(ToCamelCase(current.Member.Name));

            if (current.Expression is MemberExpression parent)
                current = parent;
            else
                break;
        }

        return string.Join(".", parts);
    }
}
```

#### Step 2.2: Collection Schema Builder
**File:** `src/Weaviate.Client.CollectionMapper/Schema/CollectionSchemaBuilder.cs`

**Responsibilities:**
1. Scan class for attributes
2. Build `CollectionConfig` from attributes
3. Convert properties to `Property[]`
4. Build vector configurations from vector properties
5. Extract references

**Key Methods:**
- `FromClass<T>()` - Main entry point
- `BuildProperties<T>()` - Create Property array
- `BuildVectorConfig<T>()` - Create VectorConfigList from vector properties
- `BuildReferences<T>()` - Extract reference definitions
- `BuildInvertedIndexConfig<T>()` - Build inverted index config

#### Step 2.3: Vector Config Builder
**File:** `src/Weaviate.Client.CollectionMapper/Schema/VectorConfigBuilder.cs`

**Responsibilities:**
1. Create vectorizer instances from generic type parameter
2. Map attribute properties to vectorizer properties
3. Handle source properties
4. Build vector index configuration
5. Support custom config builders

**Key Methods:**
- `BuildVectorConfig(PropertyInfo property)` - Main builder
- `CreateVectorizer(Type vectorizerType, VectorAttributeBase attr)` - Instantiate vectorizer
- `MapAttributeToVectorizer(Attribute attr, VectorizerConfig vectorizer)` - Property mapping
- `BuildVectorIndexConfig(PropertyInfo property)` - Index config from attributes

### Phase 3: Query Builder

#### Step 3.1: Expression to Filter Converter
**File:** `src/Weaviate.Client.CollectionMapper/Query/ExpressionToFilterConverter.cs`

**Responsibilities:**
1. Parse C# expression trees
2. Convert to Weaviate `Filter` objects
3. Handle binary expressions (&&, ||, ==, >, <, etc.)
4. Handle method calls (.Contains, .ContainsAny, etc.)
5. Support nested property access

**Key Methods:**
- `Convert<T>(Expression<Func<T, bool>> predicate)` - Main entry point
- `ConvertExpression(Expression expr)` - Recursive converter
- `ConvertBinaryExpression(BinaryExpression binary)` - Handle comparisons
- `ConvertMethodCall(MethodCallExpression method)` - Handle method calls
- `GetValue(Expression expr)` - Extract constant values

**Supported Operations:**
- Equality: `x => x.Name == "test"`
- Comparisons: `x => x.Size > 100`, `x => x.Size <= 50`
- Logical: `x => x.A > 1 && x.B < 10`, `x => x.A == 1 || x.B == 2`
- Contains: `x => x.Name.Contains("sub")`
- ContainsAny: `x => x.Tags.ContainsAny(["a", "b"])`
- Nested: `x => x.Category.Name == "Tech"`

#### Step 3.2: WOMP Query Client
**File:** `src/Weaviate.Client.CollectionMapper/Query/CollectionMapperQueryClient.cs`

**Responsibilities:**
1. Fluent query builder API
2. Accumulate query parameters (filters, limits, sorts, etc.)
3. Type-safe property selection for vectors and references
4. Execute queries via underlying `TypedQueryClient<T>`
5. Map results with vectors and references

**Key Methods:**
- `Where(Expression<Func<T, bool>> predicate)` - Add filter
- `NearText(string text, Expression<Func<T, object>>? vector)` - Near text search
- `NearVector(float[] vector, Expression<Func<T, object>>? target)` - Near vector search
- `Hybrid(string query, Expression<Func<T, object>>? vector, float? alpha)` - Hybrid search
- `WithVectors(params Expression<Func<T, object>>[] vectors)` - Include vectors
- `WithReferences(params Expression<Func<T, object>>[] refs)` - Include references
- `Limit(uint limit)` - Set limit
- `Sort<TProp>(Expression<Func<T, TProp>> property, bool descending)` - Sort
- `ExecuteAsync(CancellationToken ct)` - Execute and return typed results

**Internal State:**
```csharp
private Filter? _filter;
private uint? _limit;
private List<string> _includeVectors;
private List<string> _includeReferences;
private AutoArray<Sort>? _sort;
private NearSearchMode _searchMode;
private object? _searchTarget;
private string? _targetVector;
```

### Phase 4: Object Mapping

#### Step 4.1: WOMP Object Mapper
**File:** `src/Weaviate.Client.CollectionMapper/Mapping/CollectionMapperObjectMapper.cs`

**Responsibilities:**
1. Convert C# objects to `WeaviateObject`
2. Convert `WeaviateObject` back to C# objects
3. Handle vector properties
4. Handle reference properties
5. Use existing `ObjectHelper` for regular properties

**Key Methods:**
- `ToWeaviateObject<T>(T obj)` - C# → WeaviateObject
  - Extract regular properties via `ObjectHelper.BuildDataTransferObject()`
  - Extract vectors from vector properties → `Vectors` dictionary
  - Extract references from reference properties → `References` dictionary

- `FromWeaviateObject<T>(WeaviateObject obj)` - WeaviateObject → C#
  - Deserialize properties via `ObjectHelper.UnmarshallProperties()`
  - Populate vector properties from `Vectors` dictionary
  - Populate reference properties from `References` dictionary

#### Step 4.2: Vector Mapper
**File:** `src/Weaviate.Client.CollectionMapper/Mapping/VectorMapper.cs`

**Responsibilities:**
1. Extract vectors from C# objects
2. Inject vectors into C# objects
3. Handle both `float[]` (single vector) and `float[,]` (multi-vector)

**Key Methods:**
- `ExtractVectors<T>(T obj)` - Get all vectors from object
- `InjectVectors<T>(T obj, Vectors vectors)` - Set vectors on object

#### Step 4.3: Reference Mapper
**File:** `src/Weaviate.Client.CollectionMapper/Mapping/ReferenceMapper.cs`

**Responsibilities:**
1. Convert C# objects to reference beacons
2. Convert reference beacons back to C# objects
3. Handle single references and multi-references (List<T>)
4. Support reference expansion

**Key Methods:**
- `ExtractReferences<T>(T obj)` - Get references from object
- `InjectReferences<T>(T obj, IDictionary<string, IList<WeaviateObject>> refs)` - Populate references
- `ConvertToBeacon(object refObj, string targetCollection)` - Object → Beacon
- `ConvertFromBeacon<TRef>(WeaviateObject beacon)` - Beacon → Object

### Phase 5: Extension Methods

#### Step 5.1: Client Extensions
**File:** `src/Weaviate.Client.CollectionMapper/Extensions/WeaviateClientExtensions.cs`

```csharp
public static class WeaviateClientExtensions
{
    /// <summary>
    /// Create collection from class with attributes
    /// </summary>
    public static async Task<CollectionClient> CreateFromClass<T>(
        this ICollections collections,
        CancellationToken ct = default)
        where T : class
    {
        var config = CollectionSchemaBuilder.FromClass<T>();
        return await collections.Create(config, ct);
    }
}
```

#### Step 5.2: Collection Client Extensions
**File:** `src/Weaviate.Client.CollectionMapper/Extensions/CollectionClientExtensions.cs`

```csharp
public static class CollectionClientExtensions
{
    /// <summary>
    /// Get WOMP query client for type-safe queries
    /// </summary>
    public static CollectionMapperQueryClient<T> Query<T>(
        this CollectionClient collection)
        where T : class, new()
    {
        return new CollectionMapperQueryClient<T>(collection);
    }

    /// <summary>
    /// Insert typed object with automatic vector/reference mapping
    /// </summary>
    public static async Task<Guid> Insert<T>(
        this DataClient dataClient,
        T obj,
        CancellationToken ct = default)
        where T : class
    {
        var weaviateObj = CollectionMapperObjectMapper.ToWeaviateObject(obj);

        return await dataClient.Insert(
            weaviateObj.Properties,
            vectors: weaviateObj.Vectors.Count > 0 ? weaviateObj.Vectors : null,
            references: weaviateObj.References.Count > 0
                ? ConvertReferencesToBeacons(weaviateObj.References)
                : null,
            cancellationToken: ct
        );
    }
}
```

### Phase 6: Documentation and Examples

#### Step 6.1: Create README
**File:** `src/Weaviate.Client.CollectionMapper/README.md`

Include:
- Quick start guide
- Attribute reference
- Query examples
- Complete usage examples

#### Step 6.2: Create Example Project
**File:** `src/Weaviate.Client.CollectionMapper.Examples/Program.cs`

Include examples of:
- Simple collection creation
- Multi-vector collections
- References between collections
- Complex queries
- Nested objects

## Testing Strategy

### Unit Tests
**Project:** `src/Weaviate.Client.CollectionMapper.Tests/`

Test suites:
1. `SchemaBuilderTests` - Attribute → CollectionConfig conversion
2. `ExpressionConverterTests` - Expression → Filter conversion
3. `ObjectMapperTests` - Object mapping with vectors/references
4. `QueryBuilderTests` - Query builder API
5. `PropertyHelperTests` - Property name conversions

### Integration Tests
**Project:** `src/Weaviate.Client.Tests.Integration/CollectionMapper/`

Test scenarios:
1. Create collection from class
2. Insert objects with vectors
3. Query with filters
4. Near text/vector searches
5. Reference expansion
6. Multi-vector support

## Example Usage

### Basic Example

```csharp
using Weaviate.Client.CollectionMapper;
using Weaviate.Client.CollectionMapper.Attributes;
using Weaviate.Client.CollectionMapper.Extensions;

// 1. Define model
[WeaviateCollection("Articles")]
public class Article
{
    [Property(DataType.Text)]
    [Index(Filterable = true, Searchable = true)]
    public string Title { get; set; }

    [Property(DataType.Text)]
    public string Content { get; set; }

    [Property(DataType.Int)]
    [Index(Filterable = true)]
    public int WordCount { get; set; }

    [Vector<Text2VecOpenAI>(
        Model = "ada-002",
        SourceProperties = [nameof(Title), nameof(Content)]
    )]
    public float[]? Embedding { get; set; }

    [Reference("Category")]
    public Category? Category { get; set; }
}

// 2. Create collection
var collection = await client.Collections.CreateFromClass<Article>();

// 3. Insert data
await collection.Data.Insert(new Article
{
    Title = "Hello World",
    Content = "Content here",
    WordCount = 100,
    Category = techCategory
});

// 4. Type-safe queries
var results = await collection.Query<Article>()
    .Where(a => a.WordCount > 50)
    .NearText("technology", vector: a => a.Embedding)
    .WithReferences(a => a.Category)
    .WithVectors(a => a.Embedding)
    .Limit(10)
    .ExecuteAsync();

foreach (var article in results)
{
    Console.WriteLine($"{article.Title} - {article.Category?.Name}");
    Console.WriteLine($"Vector: {article.Embedding?.Length} dimensions");
}
```

## Implementation Order

1. ✅ Create `Weaviate.Client.CollectionMapper.csproj`
2. ✅ Implement all attribute classes
3. ✅ Implement `PropertyHelper` utilities
4. ✅ Implement `CollectionSchemaBuilder`
5. ✅ Implement `VectorConfigBuilder`
6. ✅ Implement `ExpressionToFilterConverter`
7. ✅ Implement `CollectionMapperQueryClient`
8. ✅ Implement `CollectionMapperObjectMapper`
9. ✅ Implement `VectorMapper` and `ReferenceMapper`
10. ✅ Implement extension methods
11. ✅ Write unit tests
12. ✅ Write integration tests
13. ✅ Create documentation and examples
14. ✅ Test end-to-end scenarios

## Success Criteria

- [ ] All attributes defined and documented
- [ ] Schema builder creates valid `CollectionConfig` from attributes
- [ ] Expression converter handles all common filter scenarios
- [ ] Query builder provides fluent, type-safe API
- [ ] Object mapper correctly handles vectors and references
- [ ] Extension methods integrate cleanly with existing client
- [ ] No breaking changes to `Weaviate.Client`
- [ ] All tests passing
- [ ] Documentation complete with examples

## Future Enhancements (v2)

1. **Source Generators** - Compile-time schema validation
2. **Custom Config Builders** - Complex vectorizer configurations
3. **Validation Attributes** - Integrate with DataAnnotations
4. **Migration Support** - Schema evolution helpers
5. **IQueryable Provider** - Full LINQ support with `.ToListAsync()`
6. **Batch Operations** - Bulk insert with WOMP
7. **Change Tracking** - Update only modified properties
8. **Lazy Loading** - Lazy reference loading

## Notes

- Keep all WOMP code in separate namespace to avoid pollution
- Use Humanizer consistently for all string transformations
- Leverage existing `TypedQueryClient<T>` and `ObjectHelper`
- Maintain backward compatibility with raw client API
- Prioritize developer experience and type safety

# Property System Documentation

## Overview

The Property system in the Weaviate C# client provides a type-safe way to define and manage collection properties and references. This document explains the architecture, usage patterns, and implementation details.

## Table of Contents

- [Core Concepts](#core-concepts)
- [DataType Enum](#datatype-enum)
- [Property Model](#property-model)
- [Reference Model](#reference-model)
- [Usage Patterns](#usage-patterns)
- [Serialization](#serialization)
- [Type Inference](#type-inference)
- [Validation](#validation)
- [Advanced Topics](#advanced-topics)

## Core Concepts

### Property vs Reference

The Property system distinguishes between two fundamental concepts:

- **Property**: A data field with a specific data type (e.g., text, int, boolean)
- **Reference**: A relationship to one or more target collections

```csharp
// Property - has a single DataType enum value
var titleProperty = new Property
{
    Name = "title",
    DataType = DataType.Text,
    IndexSearchable = true
};

// Reference - has a list of target collection names
var authorReference = new Reference(
    name: "author",
    targetCollection: "Author",
    description: "The article's author"
);
```

### Key Architectural Decisions

1. **Property uses enum**: Type-safe `DataType` enum for compile-time checking
2. **Reference uses strings**: Dynamic `IList<string>` for target collection names
3. **Separate serialization paths**: Properties and References serialize differently
4. **Name transformation**: Property/Reference names are automatically decapitalized

## DataType Enum

### Definition

The `DataType` enum defines all supported Weaviate data types with `[EnumMember]` attributes for wire format compatibility:

```csharp
public enum DataType
{
    [System.Runtime.Serialization.EnumMember(Value = "unknown")]
    Unknown,

    // Text types
    [System.Runtime.Serialization.EnumMember(Value = "text")]
    Text,
    [System.Runtime.Serialization.EnumMember(Value = "text[]")]
    TextArray,

    // Numeric types
    [System.Runtime.Serialization.EnumMember(Value = "int")]
    Int,
    [System.Runtime.Serialization.EnumMember(Value = "int[]")]
    IntArray,
    [System.Runtime.Serialization.EnumMember(Value = "number")]
    Number,
    [System.Runtime.Serialization.EnumMember(Value = "number[]")]
    NumberArray,

    // Boolean types
    [System.Runtime.Serialization.EnumMember(Value = "boolean")]
    Bool,
    [System.Runtime.Serialization.EnumMember(Value = "boolean[]")]
    BoolArray,

    // Date types
    [System.Runtime.Serialization.EnumMember(Value = "date")]
    Date,
    [System.Runtime.Serialization.EnumMember(Value = "date[]")]
    DateArray,

    // UUID types
    [System.Runtime.Serialization.EnumMember(Value = "uuid")]
    Uuid,
    [System.Runtime.Serialization.EnumMember(Value = "uuid[]")]
    UuidArray,

    // Special types
    [System.Runtime.Serialization.EnumMember(Value = "geoCoordinates")]
    GeoCoordinate,
    [System.Runtime.Serialization.EnumMember(Value = "blob")]
    Blob,
    [System.Runtime.Serialization.EnumMember(Value = "phoneNumber")]
    PhoneNumber,

    // Object types
    [System.Runtime.Serialization.EnumMember(Value = "object")]
    Object,
    [System.Runtime.Serialization.EnumMember(Value = "object[]")]
    ObjectArray
}
```

### Extension Methods

Two extension methods handle conversion between enum and string representations:

```csharp
// Convert enum to wire format string
DataType.Text.ToEnumMemberValue()  // Returns "text"
DataType.IntArray.ToEnumMemberValue()  // Returns "int[]"

// Convert string to enum
"text".ToDataTypeEnum()  // Returns DataType.Text
"boolean[]".ToDataTypeEnum()  // Returns DataType.BoolArray
"invalid".ToDataTypeEnum()  // Returns null
```

### C# Type Mapping

The `PropertyHelper` class provides automatic type mapping from C# types to DataType enum values:

| C# Type | DataType |
|---------|----------|
| `string` | `DataType.Text` |
| `int`, `uint`, `short`, `ushort`, `long`, `ulong` | `DataType.Int` |
| `float`, `double`, `decimal` | `DataType.Number` |
| `bool` | `DataType.Bool` |
| `DateTime` | `DataType.Date` |
| `Guid` | `DataType.Uuid` |
| `GeoCoordinate` | `DataType.GeoCoordinate` |
| `PhoneNumber` | `DataType.PhoneNumber` |
| `byte[]`, `sbyte[]` | `DataType.Blob` |
| `string[]`, `List<string>` | `DataType.TextArray` |
| `int[]`, `List<int>` | `DataType.IntArray` |
| `bool[]`, `List<bool>` | `DataType.BoolArray` |
| `DateTime[]`, `List<DateTime>` | `DataType.DateArray` |
| `Guid[]`, `List<Guid>` | `DataType.UuidArray` |
| `float[]`, `double[]`, `decimal[]` | `DataType.NumberArray` |
| Custom class | `DataType.Object` |
| Custom class array/list | `DataType.ObjectArray` |
| Enums | `DataType.Text` (serialized as string) |

## Property Model

### Structure

```csharp
public record Property : IEquatable<Property>
{
    // Required properties
    public required string Name { get; set; }  // Auto-decapitalized
    public required DataType DataType { get; set; }

    // Optional properties
    public string? Description { get; set; }
    public bool? IndexFilterable { get; set; }
    public bool? IndexRangeFilters { get; set; }
    public bool? IndexSearchable { get; set; }
    public PropertyTokenization? PropertyTokenization { get; set; }
    public Property[]? NestedProperties { get; set; }  // For object types

    // Constructor
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public Property(string name, DataType dataType)
    {
        Name = name;  // Goes through setter for decapitalization
        DataType = dataType;
    }
}
```

### Name Transformation

The `Name` property automatically decapitalizes the first letter:

```csharp
var property = new Property { Name = "Title", DataType = DataType.Text };
Console.WriteLine(property.Name);  // Output: "title"

var property2 = new Property("AuthorName", DataType.Text);
Console.WriteLine(property2.Name);  // Output: "authorName"
```

This ensures Weaviate naming conventions are followed (properties start with lowercase).

### Initialization Patterns

Two initialization patterns are supported:

```csharp
// Pattern 1: Constructor + object initializer
var property1 = new Property("title", DataType.Text)
{
    Description = "Article title",
    IndexSearchable = true,
    IndexFilterable = true
};

// Pattern 2: Object initializer only
var property2 = new Property
{
    Name = "title",
    DataType = DataType.Text,
    Description = "Article title",
    IndexSearchable = true,
    IndexFilterable = true
};
```

Both patterns ensure the `Name` property goes through its setter for decapitalization.

### Static Factory Methods

The `Property` class provides static factory methods for all data types:

```csharp
// Simple text property
var title = Property.Text("title");

// Integer property with indexing
var count = Property.Int("count", indexFilterable: true);

// Number array property
var scores = Property.NumberArray("scores");

// Object property with nested properties
var address = Property.Object(
    "address",
    subProperties:
    [
        Property.Text("street"),
        Property.Text("city"),
        Property.Text("zipCode")
    ]
);

// All available factory methods:
Property.Text(name, ...)
Property.TextArray(name, ...)
Property.Int(name, ...)
Property.IntArray(name, ...)
Property.Bool(name, ...)
Property.BoolArray(name, ...)
Property.Number(name, ...)
Property.NumberArray(name, ...)
Property.Date(name, ...)
Property.DateArray(name, ...)
Property.Uuid(name, ...)
Property.UuidArray(name, ...)
Property.GeoCoordinate(name, ...)
Property.Blob(name, ...)
Property.PhoneNumber(name, ...)
Property.Object(name, ...)
Property.ObjectArray(name, ...)
```

### Generic Type-Based Factory

For type-safe property creation based on C# types:

```csharp
// Infers DataType.Text from string
var title = Property<string>.New("title");

// Infers DataType.Int from int
var age = Property<int>.New("age");

// Infers DataType.DateArray from DateTime[]
var dates = Property<DateTime[]>.New("importantDates");

// Infers DataType.Object from custom class
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}
var address = Property<Address>.New("address");
```

### Nested Properties (Objects)

For `Object` and `ObjectArray` types, you can define nested properties:

```csharp
var personProperty = Property.Object(
    "person",
    description: "A person object",
    subProperties:
    [
        Property.Text("firstName"),
        Property.Text("lastName"),
        Property.Int("age"),
        Property.Object(
            "address",
            subProperties:
            [
                Property.Text("street"),
                Property.Text("city"),
                Property.Text("country")
            ]
        )
    ]
);
```

### Property Equality

Properties implement `IEquatable<Property>` and compare all fields:

```csharp
var prop1 = new Property("title", DataType.Text) { IndexSearchable = true };
var prop2 = new Property("title", DataType.Text) { IndexSearchable = true };
var prop3 = new Property("title", DataType.Text) { IndexSearchable = false };

Console.WriteLine(prop1.Equals(prop2));  // true
Console.WriteLine(prop1 == prop2);       // true (record equality)
Console.WriteLine(prop1.Equals(prop3));  // false (different IndexSearchable)
```

Nested properties are compared recursively using `SequenceEqual`.

## Reference Model

### Structure

```csharp
// Public interface for polymorphic serialization
internal interface IReferenceBase
{
    string Name { get; }
    IList<string> TargetCollections { get; }
    string? Description { get; }
}

// Reference record
public record Reference(string Name, string TargetCollection, string? Description = null)
    : IReferenceBase
{
    public string? Description { get; set; } = Description;

    // Explicit interface implementation
    IList<string> IReferenceBase.TargetCollections { get; } = [TargetCollection];
}
```

### Usage

```csharp
// Simple reference to one collection
var author = new Reference(
    name: "author",
    targetCollection: "Author",
    description: "The article's author"
);

// Access properties
Console.WriteLine(author.Name);             // "author"
Console.WriteLine(author.TargetCollection); // "Author"

// Via interface for serialization
IReferenceBase refBase = author;
Console.WriteLine(refBase.TargetCollections[0]);  // "Author"
```

### Static Factory Method

```csharp
// Alternative creation using Property.Reference
var category = Property.Reference(
    name: "category",
    targetCollection: "Category",
    description: "Article category"
);
```

### Design Rationale

References use `IList<string>` for target collections (accessed via interface) because:
1. Collection names are dynamic (defined by users)
2. Wire format uses string arrays
3. No benefit to compile-time type checking of collection names

The `IReferenceBase` interface provides a unified way to serialize Properties and References:
- Properties serialize using `DataType.ToEnumMemberValue()`
- References serialize using `TargetCollections` directly

## Usage Patterns

### Manual Property Definition

```csharp
using Weaviate.Client.Models;

// Define collection properties
var properties = new[]
{
    Property.Text("title", indexSearchable: true),
    Property.Text("content", indexSearchable: true),
    Property.Int("wordCount", indexFilterable: true),
    Property.Date("publishedAt", indexFilterable: true),
    Property.TextArray("tags"),
    Property.Bool("featured")
};

var references = new[]
{
    new Reference("author", "Author", "Article author"),
    new Reference("category", "Category", "Article category")
};

// Use in collection creation
await client.Collections.CreateAsync(new CollectionConfig
{
    Name = "Article",
    Properties = properties,
    References = references
});
```

### Automatic Property Extraction from Classes

The `Property.FromClass<T>()` method automatically extracts properties from C# classes:

```csharp
public class Article
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int WordCount { get; set; }
    public DateTime PublishedAt { get; set; }
    public List<string> Tags { get; set; }
    public bool Featured { get; set; }
}

// Automatically generates properties based on class structure
var properties = Property.FromClass<Article>();

// Equivalent to:
// [
//     new Property { Name = "title", DataType = DataType.Text },
//     new Property { Name = "content", DataType = DataType.Text },
//     new Property { Name = "wordCount", DataType = DataType.Int },
//     new Property { Name = "publishedAt", DataType = DataType.Date },
//     new Property { Name = "tags", DataType = DataType.TextArray },
//     new Property { Name = "featured", DataType = DataType.Bool }
// ]
```

### Nested Object Extraction

For complex types with nested objects:

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Address HomeAddress { get; set; }  // Nested object
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

// Extract with nested properties (maxDepth = 1)
var properties = Property.FromClass<Person>(maxDepth: 1);

// Result:
// [
//     { Name = "firstName", DataType = DataType.Text },
//     { Name = "lastName", DataType = DataType.Text },
//     {
//         Name = "homeAddress",
//         DataType = DataType.Object,
//         NestedProperties = [
//             { Name = "street", DataType = DataType.Text },
//             { Name = "city", DataType = DataType.Text },
//             { Name = "country", DataType = DataType.Text }
//         ]
//     }
// ]
```

The `maxDepth` parameter controls how many levels of nesting to extract:
- `maxDepth: 0` - Only top-level properties, nested objects excluded
- `maxDepth: 1` - One level of nesting (default)
- `maxDepth: 2` - Two levels of nesting
- And so on...

### Combining Manual and Automatic

```csharp
// Start with automatic extraction
var properties = Property.FromClass<Article>().ToList();

// Add custom indexing configuration
properties.Add(Property.Text(
    "searchableTitle",
    indexSearchable: true,
    indexFilterable: true,
    tokenization: PropertyTokenization.Word
));

// Add references (not extracted from classes)
var references = new[]
{
    new Reference("author", "Author"),
    new Reference("category", "Category")
};
```

## Serialization

### Property Serialization (ToDto)

Properties serialize to the Weaviate REST DTO format:

```csharp
// Internal model
var property = new Property
{
    Name = "title",
    DataType = DataType.Text,
    Description = "Article title",
    IndexSearchable = true,
    IndexFilterable = true
};

// Serialized to DTO
var dto = property.ToDto();
// Result:
// {
//     "name": "title",
//     "dataType": ["text"],  // Enum converted to string in array
//     "description": "Article title",
//     "indexSearchable": true,
//     "indexFilterable": true
// }
```

### Reference Serialization (ToDto)

References serialize similarly but use the target collections list:

```csharp
var reference = new Reference("author", "Author", "Article author");

var dto = reference.ToDto();
// Result:
// {
//     "name": "author",
//     "dataType": ["Author"],  // Collection name as string
//     "description": "Article author"
// }
```

### Deserialization (ToModel / ToReference)

The DTO can deserialize back to either Property or Reference based on the dataType content:

```csharp
// DTO from server
var dto = new Rest.Dto.Property
{
    Name = "title",
    DataType = ["text"],
    Description = "Article title",
    IndexSearchable = true
};

// Deserialize to Property (lowercase dataType)
var property = dto.ToModel();
// Result: Property with DataType = DataType.Text

// DTO for reference
var refDto = new Rest.Dto.Property
{
    Name = "author",
    DataType = ["Author"],  // Uppercase = collection name
    Description = "Article author"
};

// Deserialize to Reference
var reference = refDto.ToReference();
// Result: Reference with TargetCollections = ["Author"]
```

### Separation Logic

The client distinguishes Properties from References based on the first character of the dataType string:

```csharp
// In CollectionConfig conversion:
References = collection?.Properties
    ?.Where(p => p.DataType?.Any(t => char.IsUpper(t.First())) ?? false)
    .Select(p => p.ToReference())
    .ToArray() ?? [];

Properties = collection?.Properties
    ?.Where(p => p.DataType?.All(t => char.IsLower(t.First())) ?? false)
    .Select(p => p.ToModel())
    .ToArray() ?? [];
```

- **Uppercase first character** → Reference (e.g., "Author", "Category")
- **Lowercase first character** → Property (e.g., "text", "int", "boolean[]")

## Type Inference

### PropertyHelper Class

The `PropertyHelper` class provides automatic type inference from C# types to DataType enum values:

```csharp
// Get DataType for any C# type
DataType stringType = PropertyHelper.DataTypeForType(typeof(string));
// Returns: DataType.Text

DataType intListType = PropertyHelper.DataTypeForType(typeof(List<int>));
// Returns: DataType.IntArray

DataType customType = PropertyHelper.DataTypeForType(typeof(MyClass));
// Returns: DataType.Object
```

### Special Type Handling

#### Nullable Types
```csharp
DataType nullableInt = PropertyHelper.DataTypeForType(typeof(int?));
// Returns: DataType.Int (nullable wrapper removed)

DataType nullableDateTime = PropertyHelper.DataTypeForType(typeof(DateTime?));
// Returns: DataType.Date
```

#### Enums
```csharp
public enum Status { Active, Inactive, Pending }

DataType enumType = PropertyHelper.DataTypeForType(typeof(Status));
// Returns: DataType.Text (enums serialize as strings)

DataType enumArrayType = PropertyHelper.DataTypeForType(typeof(Status[]));
// Returns: DataType.TextArray
```

#### Collections
```csharp
// Arrays
PropertyHelper.DataTypeForType(typeof(string[]));  // DataType.TextArray
PropertyHelper.DataTypeForType(typeof(int[]));     // DataType.IntArray

// Lists
PropertyHelper.DataTypeForType(typeof(List<string>));  // DataType.TextArray
PropertyHelper.DataTypeForType(typeof(List<int>));     // DataType.IntArray

// IEnumerable
PropertyHelper.DataTypeForType(typeof(IEnumerable<bool>));  // DataType.BoolArray

// HashSet
PropertyHelper.DataTypeForType(typeof(HashSet<Guid>));  // DataType.UuidArray
```

#### Byte Arrays (Blob)
```csharp
PropertyHelper.DataTypeForType(typeof(byte[]));   // DataType.Blob
PropertyHelper.DataTypeForType(typeof(sbyte[]));  // DataType.Blob
```

### PropertyFactory Delegate

The `PropertyFactory` delegate provides a functional way to create properties:

```csharp
public delegate Property PropertyFactory(
    string name,
    string? description = null,
    bool? indexFilterable = null,
    bool? indexRangeFilters = null,
    bool? indexSearchable = null,
    PropertyTokenization? tokenization = null,
    Property[]? subProperties = null
);

// Get factory for a specific type
PropertyFactory textFactory = PropertyHelper.ForType(typeof(string));
var title = textFactory("title", description: "Article title", indexSearchable: true);

// Static factories use this pattern internally
Property.Text("title") // Uses PropertyHelper.Factory(DataType.Text)
```

## Validation

### TypeValidator

The `TypeValidator` class validates C# types against Weaviate schema properties:

```csharp
// Validate a C# class against a collection schema
public class Article
{
    public string Title { get; set; }  // Must match "title" property
    public int WordCount { get; set; }  // Must match "wordCount" property
}

// Schema from Weaviate
var schemaProperties = new[]
{
    new Property { Name = "title", DataType = DataType.Text },
    new Property { Name = "wordCount", DataType = DataType.Int }
};

// Validation happens automatically when using typed client
var collection = client.Collections.Get<Article>("Article");
// TypeValidator ensures Article properties match schema
```

### Type Compatibility Rules

The validator checks:

1. **Property Name Matching** (case-insensitive after decapitalization)
   ```csharp
   // C# property: Title
   // Schema property: title
   // ✅ Match (after decapitalization)
   ```

2. **DataType Compatibility**
   ```csharp
   // C# type: string → Expected: DataType.Text
   // Schema: DataType.Text
   // ✅ Compatible

   // C# type: int → Expected: DataType.Int
   // Schema: DataType.Number
   // ❌ Incompatible (different types)
   ```

3. **Enum-to-Int Special Case**
   ```csharp
   // C# type: enum → Expected: DataType.Text (enum serializes as string)
   // Schema: DataType.Int
   // ✅ Compatible (enums can be stored as ints in Weaviate)
   ```

4. **Nested Object Validation**
   ```csharp
   // C# has nested Address property
   // Schema must have object type with nested properties
   // Recursively validates nested properties
   ```

### Validation Errors

When validation fails, a `WeaviateClientException` is thrown with details:

```csharp
try
{
    var collection = client.Collections.Get<Article>("Article");
}
catch (WeaviateClientException ex)
{
    // Message includes:
    // - Mismatched property names
    // - Type incompatibilities
    // - Missing required properties
}
```

## Advanced Topics

### PropertyTokenization

The `PropertyTokenization` enum controls how text properties are tokenized for search:

```csharp
public enum PropertyTokenization
{
    Word = 0,        // Split on non-alphanumeric characters
    Lowercase = 1,   // Word tokenization + lowercase
    Whitespace = 2,  // Split on whitespace only
    Field = 3,       // Treat entire field as single token
    Trigram = 4,     // Character trigrams for fuzzy search
    Gse = 5,         // Chinese text segmentation
    Kagome_kr = 6,   // Korean text segmentation
    Kagome_ja = 7,   // Japanese text segmentation
    Gse_ch = 8       // Chinese text segmentation (alternative)
}

// Usage:
var property = Property.Text(
    "title",
    indexSearchable: true,
    tokenization: PropertyTokenization.Word
);
```

### Index Configuration

Properties support several indexing options:

```csharp
var property = new Property
{
    Name = "title",
    DataType = DataType.Text,

    // Enable/disable filtering support
    IndexFilterable = true,  // Allow WHERE filters

    // Enable/disable range filters (>, <, >=, <=)
    IndexRangeFilters = true,  // Allow range queries

    // Enable/disable full-text search
    IndexSearchable = true,  // Allow BM25/keyword search

    // Control tokenization (when IndexSearchable = true)
    PropertyTokenization = PropertyTokenization.Word
};
```

#### Index Configuration Guidelines

- **IndexFilterable**: Set to `true` for properties used in filters (`where name = "value"`)
- **IndexRangeFilters**: Set to `true` for numeric/date range queries (`where count > 10`)
- **IndexSearchable**: Set to `true` for text properties used in BM25/keyword search
- **PropertyTokenization**: Only applies when `IndexSearchable = true`

Example configurations:

```csharp
// ID field - filter only, no search
Property.Text("id", indexFilterable: true, indexSearchable: false)

// Title - both filterable and searchable
Property.Text("title",
    indexFilterable: true,
    indexSearchable: true,
    tokenization: PropertyTokenization.Word
)

// Counter - filterable with range support
Property.Int("count",
    indexFilterable: true,
    indexRangeFilters: true
)

// Timestamp - filterable with range support
Property.Date("createdAt",
    indexFilterable: true,
    indexRangeFilters: true
)

// Tags - searchable only
Property.TextArray("tags",
    indexSearchable: true,
    tokenization: PropertyTokenization.Lowercase
)
```

### Circular Reference Detection

When using `Property.FromClass<T>()` with nested objects, the system prevents infinite recursion:

```csharp
public class Person
{
    public string Name { get; set; }
    public Person? Parent { get; set; }  // Circular reference!
}

// Extracts with maxDepth to prevent infinite loop
var properties = Property.FromClass<Person>(maxDepth: 2);
// Only extracts Person.Parent.Parent (2 levels deep), then stops
```

The `seenTypes` dictionary tracks visited types to prevent processing the same type multiple times in a single path.

### Custom Property Converters

For advanced scenarios, you can implement custom property converters:

```csharp
// PropertyConverterRegistry tracks converters by DataType string
// This is used during serialization/deserialization

// Custom converter example (internal use)
var registry = new PropertyConverterRegistry();
registry.Register("custom-type", new CustomPropertyConverter());
```

This is typically used internally by the client and not exposed for direct use.

### Property Equality and Hashing

Properties implement proper equality and hash code generation:

```csharp
var prop1 = Property.Text("title");
var prop2 = Property.Text("title");

// Equality checks all fields
Console.WriteLine(prop1.Equals(prop2));  // true
Console.WriteLine(prop1 == prop2);       // true (record equality)

// Hash code based on all fields
var set = new HashSet<Property> { prop1, prop2 };
Console.WriteLine(set.Count);  // 1 (duplicates removed)
```

Hash code generation includes:
- Name
- DataType
- Description
- All index settings
- PropertyTokenization
- NestedProperties (recursively)

## Best Practices

### 1. Use Type Inference When Possible

```csharp
// ✅ Preferred: Type-safe automatic inference
var properties = Property.FromClass<Article>();

// ⚠️ Less preferred: Manual definition (more verbose, error-prone)
var properties = new[]
{
    Property.Text("title"),
    Property.Int("wordCount"),
    // ...
};
```

### 2. Configure Indexes Based on Usage

```csharp
// ✅ Correct: Index configuration matches query patterns
Property.Text("title",
    indexSearchable: true,      // Used in BM25 search
    indexFilterable: true       // Used in WHERE filters
)

// ❌ Incorrect: Over-indexing (wastes space)
Property.Blob("image",
    indexSearchable: true,      // Blobs can't be searched!
    indexFilterable: true       // Rarely filtered
)
```

### 3. Use References for Relationships

```csharp
// ✅ Correct: Use Reference for relationships
new Reference("author", "Author")

// ❌ Incorrect: Don't use string property for references
Property.Text("authorId")  // Loses relationship semantics
```

### 4. Leverage Static Factories

```csharp
// ✅ Preferred: Clean, readable
var title = Property.Text("title", indexSearchable: true);

// ⚠️ Less preferred: More verbose
var title = new Property
{
    Name = "title",
    DataType = DataType.Text,
    IndexSearchable = true
};
```

### 5. Use maxDepth for Complex Nesting

```csharp
// ✅ Correct: Control nesting depth
var properties = Property.FromClass<ComplexType>(maxDepth: 2);

// ⚠️ Warning: May extract too much or too little
var properties = Property.FromClass<ComplexType>();  // Default maxDepth: 1
```

### 6. Name Properties Consistently

```csharp
// ✅ Correct: PascalCase in C#, auto-decapitalized
public class Article
{
    public string Title { get; set; }        // Becomes "title"
    public int WordCount { get; set; }       // Becomes "wordCount"
}

// ⚠️ Less ideal: Manual lowercase (inconsistent)
public class Article
{
    public string title { get; set; }        // Already lowercase
}
```

The client automatically handles decapitalization, so follow C# naming conventions.

## Examples

### Complete Collection Setup

```csharp
using Weaviate.Client;
using Weaviate.Client.Models;

// Define model
public class BlogPost
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PublishedAt { get; set; }
    public int Views { get; set; }
    public List<string> Tags { get; set; }
    public bool Featured { get; set; }
}

// Create collection with automatic property extraction
var collection = await client.Collections.CreateAsync(new CollectionConfig
{
    Name = "BlogPost",

    // Automatically extract properties from BlogPost class
    Properties = Property.FromClass<BlogPost>(),

    // Add references manually
    References = new[]
    {
        new Reference("author", "Author", "Blog post author"),
        new Reference("category", "Category", "Blog post category")
    }
});
```

### Manual Configuration with Indexing

```csharp
var collection = await client.Collections.CreateAsync(new CollectionConfig
{
    Name = "Product",

    Properties = new[]
    {
        // Searchable title
        Property.Text("name",
            description: "Product name",
            indexSearchable: true,
            indexFilterable: true,
            tokenization: PropertyTokenization.Word
        ),

        // Searchable description
        Property.Text("description",
            description: "Product description",
            indexSearchable: true,
            tokenization: PropertyTokenization.Word
        ),

        // Filterable SKU
        Property.Text("sku",
            description: "Stock keeping unit",
            indexFilterable: true,
            indexSearchable: false
        ),

        // Range-filterable price
        Property.Number("price",
            description: "Product price",
            indexFilterable: true,
            indexRangeFilters: true
        ),

        // Range-filterable stock
        Property.Int("stockQuantity",
            description: "Available quantity",
            indexFilterable: true,
            indexRangeFilters: true
        ),

        // Filterable category
        Property.TextArray("categories",
            description: "Product categories",
            indexFilterable: true
        )
    },

    References = new[]
    {
        new Reference("manufacturer", "Manufacturer"),
        new Reference("supplier", "Supplier")
    }
});
```

### Complex Nested Objects

```csharp
public class Order
{
    public string OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; }
    public List<LineItem> Items { get; set; }
}

public class Customer
{
    public string Name { get; set; }
    public string Email { get; set; }
    public Address ShippingAddress { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

public class LineItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

// Extract with 3 levels of nesting
// Level 0: Order
// Level 1: Customer, Items[]
// Level 2: ShippingAddress, (LineItem properties)
// Level 3: (Address properties)
var properties = Property.FromClass<Order>(maxDepth: 3);

// Result structure:
// - orderId: text
// - orderDate: date
// - customer: object
//   - name: text
//   - email: text
//   - shippingAddress: object
//     - street: text
//     - city: text
//     - country: text
// - items: object[]
//   - productName: text
//   - quantity: int
//   - price: number
```

## Migration Guide

If upgrading from a version that used string-based DataType:

### Before (String-Based)

```csharp
// Old string-based approach
new Property
{
    Name = "title",
    DataType = { "text" },  // Collection of strings
    IndexSearchable = true
}

// Old static constants
if (property.DataType.Contains(DataType.Text))  // String comparison
```

### After (Enum-Based)

```csharp
// New enum-based approach
new Property
{
    Name = "title",
    DataType = DataType.Text,  // Single enum value
    IndexSearchable = true
}

// New enum comparison
if (property.DataType == DataType.Text)  // Type-safe comparison
```

### Breaking Changes

1. `Property.DataType` changed from `IList<string>` to `DataType` enum
2. Constructor signature changed from `Property(string name, string dataType)` to `Property(string name, DataType dataType)`
3. No implicit conversion between Reference and Property

### Migration Steps

1. Update Property instantiations:
   ```csharp
   // Before:
   new Property { Name = "title", DataType = ["text"] }

   // After:
   new Property { Name = "title", DataType = DataType.Text }
   ```

2. Update comparisons:
   ```csharp
   // Before:
   if (property.DataType.Contains("text"))
   if (property.DataType[0] == DataType.Text)

   // After:
   if (property.DataType == DataType.Text)
   ```

3. Static factories remain unchanged:
   ```csharp
   // Still works:
   Property.Text("title")
   Property<string>.New("title")
   ```

## Conclusion

The Property system provides a robust, type-safe way to define collection schemas in the Weaviate C# client. Key takeaways:

- **Type Safety**: Enum-based DataType prevents typos and provides compile-time checking
- **Flexibility**: Manual definition or automatic extraction from C# classes
- **Clear Semantics**: Distinct Property (single type) and Reference (relationships) models
- **Wire Compatibility**: EnumMember attributes ensure correct REST API serialization
- **Automatic Type Inference**: PropertyHelper maps C# types to Weaviate types
- **Validation**: TypeValidator ensures C# classes match Weaviate schemas

For questions or issues, please refer to the main Weaviate documentation or file an issue in the GitHub repository.

// Example usage of Weaviate.Client.CollectionMapper
// This file shows how to use the ORM layer - not meant to be compiled on its own

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor

using Weaviate.Client.CollectionMapper.Attributes;
using Weaviate.Client.CollectionMapper.Extensions;
using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Examples;

/// <summary>
/// Example 1: Simple collection with text vectorization
/// </summary>
[WeaviateCollection("Articles", Description = "Blog articles")]
[InvertedIndex(IndexTimestamps = true)]
public class Article
{
    [Property(DataType.Text)]
    [Index(Filterable = true, Searchable = true)]
    [Tokenization(PropertyTokenization.Word)]
    public string Title { get; set; }

    [Property(DataType.Text)]
    [Index(Searchable = true)]
    public string Content { get; set; }

    [Property(DataType.Int)]
    [Index(Filterable = true, RangeFilters = true)]
    public int WordCount { get; set; }

    [Property(DataType.Date)]
    [Index(Filterable = true)]
    public DateTime PublishedAt { get; set; }

    // Named vector - property name becomes vector name
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-ada-002",
        SourceProperties = [nameof(Title), nameof(Content)]
    )]
    public float[]? TitleContentEmbedding { get; set; }

    // Reference to Category collection
    [Reference("Category")]
    public Category? Category { get; set; }
}

/// <summary>
/// Example 2: Simple reference target with automatic type inference
/// </summary>
[WeaviateCollection("Category")]
public class Category
{
    // Data types are automatically inferred from C# types when not specified
    [Property]
    [Index(Filterable = true)]
    public string Name { get; set; }

    [Property(Description = "Category description")]
    public string Description { get; set; }

    [Property]
    public int ItemCount { get; set; }

    [Property]
    public DateTime CreatedAt { get; set; }

    [Property]
    public bool IsActive { get; set; }
}

/// <summary>
/// Example 3: Multi-vector collection for e-commerce
/// </summary>
[WeaviateCollection("Products")]
public class Product
{
    [Property(DataType.Text)]
    [Index(Filterable = true, Searchable = true)]
    public string Name { get; set; }

    [Property(DataType.Text)]
    [Index(Searchable = true)]
    public string Description { get; set; }

    [Property(DataType.Number)]
    [Index(Filterable = true, RangeFilters = true)]
    public decimal Price { get; set; }

    [Property(DataType.Blob)]
    public byte[]? ProductImage { get; set; }

    // Text-only vector for keyword search
    [Vector<Vectorizer.Text2VecOpenAI>(
        Model = "text-embedding-ada-002",
        SourceProperties = [nameof(Name), nameof(Description)]
    )]
    public float[]? TextEmbedding { get; set; }

    // Multi-modal vector combining text and image
    [Vector<Vectorizer.Multi2VecClip>(
        TextFields = [nameof(Name), nameof(Description)],
        ImageFields = [nameof(ProductImage)]
    )]
    public float[]? MultiModalEmbedding { get; set; }

    // User-provided custom embedding
    [Vector<Vectorizer.SelfProvided>()]
    public float[]? CustomEmbedding { get; set; }
}

/// <summary>
/// Example 4: Nested objects
/// The nested type is automatically inferred from the property type.
/// [NestedType] attribute is optional and only needed if you want to override the inferred type.
/// </summary>
[WeaviateCollection("BlogPost")]
public class BlogPost
{
    [Property(DataType.Text)]
    public string Title { get; set; }

    [Property(DataType.Text)]
    public string Content { get; set; }

    // Nested object - type inferred from property type (Author)
    [Property(DataType.Object)]
    public Author Author { get; set; }

    // Nested array - type inferred from List<T> (Comment)
    [Property(DataType.ObjectArray)]
    public List<Comment> Comments { get; set; }

    [Vector<Vectorizer.Text2VecTransformers>(SourceProperties = [nameof(Title), nameof(Content)])]
    public float[]? ContentEmbedding { get; set; }
}

// Nested type - no [WeaviateCollection] needed
public class Author
{
    [Property(DataType.Text)]
    public string Name { get; set; }

    [Property(DataType.Text)]
    public string Email { get; set; }

    [Property(DataType.Text)]
    public string? Bio { get; set; }
}

// Nested type for comments
public class Comment
{
    [Property(DataType.Text)]
    public string Text { get; set; }

    [Property(DataType.Text)]
    public string AuthorName { get; set; }

    [Property(DataType.Date)]
    public DateTime PostedAt { get; set; }
}

/// <summary>
/// Example 5: Multi-reference support
/// </summary>
[WeaviateCollection("ResearchPaper")]
public class ResearchPaper
{
    [Property(DataType.Text)]
    public string Title { get; set; }

    [Property(DataType.Text)]
    public string Abstract { get; set; }

    // Single reference
    [Reference("Category")]
    public Category? PrimaryCategory { get; set; }

    // Multi-reference - list of related papers
    [Reference("ResearchPaper")]
    public List<ResearchPaper>? Citations { get; set; }

    // ID-only reference (just the Guid, not the full object)
    [Reference("Author")]
    public Guid? PrimaryAuthorId { get; set; }
}

/// <summary>
/// Example usage code
/// </summary>
public static class UsageExamples
{
    public static async Task CreateCollections()
    {
        // Placeholder - WeaviateConfig needs to be imported
        WeaviateClient client = null!;

        // Create collection from class attributes
        var articleCollection = await client.Collections.CreateFromClass<Article>();
        var categoryCollection = await client.Collections.CreateFromClass<Category>();
        var productCollection = await client.Collections.CreateFromClass<Product>();
        var blogPostCollection = await client.Collections.CreateFromClass<BlogPost>();
        var userCollection = await client.Collections.CreateFromClass<User>();

        // Collections are now created with full schema!
        // - All properties configured with automatic type inference
        // - Vectors configured with correct vectorizers
        // - References set up
        // - Indexing enabled
        // - Nested objects structured
    }

    // Future: Insert with automatic mapping (not yet implemented)
    public static async Task InsertData()
    {
        WeaviateClient client = null!;
        var collection = await client.Collections.CreateFromClass<Article>();

        // TODO: Implement this in Phase 3
        // await collection.Data.Insert(new Article {
        //     Title = "Hello World",
        //     Content = "This is my first article",
        //     WordCount = 100,
        //     PublishedAt = DateTime.Now,
        //     Category = techCategory
        // });
    }

    // Future: Type-safe queries (not yet implemented)
    public static async Task QueryData()
    {
        WeaviateClient client = null!;
        var collection = await client.Collections.CreateFromClass<Article>();

        // TODO: Implement this in Phase 2
        // var results = await collection.Query<Article>()
        //     .Where(a => a.WordCount > 100)
        //     .ToListAsync();
    }
}

/// <summary>
/// Example 6: Automatic type inference from C# types
/// </summary>
[WeaviateCollection("Users")]
public class User
{
    // All types are automatically inferred - no need to specify DataType
    [Property]
    public string Username { get; set; }

    [Property]
    public string Email { get; set; }

    [Property]
    public int Age { get; set; }

    [Property]
    public bool IsActive { get; set; }

    [Property]
    public double Rating { get; set; }

    [Property]
    public DateTime CreatedAt { get; set; }

    [Property]
    public Guid UserId { get; set; }

    // Array types are also inferred
    [Property]
    public List<string> Tags { get; set; }

    [Property]
    public int[] Scores { get; set; }

    [Property]
    public List<DateTime> LoginDates { get; set; }

    // You can still specify DataType explicitly when needed
    [Property(DataType.PhoneNumber)]
    public string Phone { get; set; }

    [Vector<Vectorizer.Text2VecOpenAI>(SourceProperties = [nameof(Username), nameof(Email)])]
    public float[]? UserEmbedding { get; set; }
}

#pragma warning restore CS8618

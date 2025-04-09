namespace Weaviate.Client.Rest.Models;

public class PropertyBase
{
    protected PropertyBase() { }

    // Data type of the property (required). If it starts with a capital (for example Person), may be a reference to another type.
    public required string[] DataType { get; set; }

    // Description of the property.
    public string? Description { get; set; }

    // The name of the property (required). Multiple words should be concatenated in camelCase, e.g. `nameOfAuthor`.
    public required string Name { get; set; }

    // Whether to include this property in the filterable, Roaring Bitmap index. If `false`, this property cannot be used in `where` filters. <br/><br/>Note: Unrelated to vectorization behavior.
    public bool? IndexFilterable { get; set; }

    // Optional. Should this property be indexed in the inverted index. Defaults to true. Applicable only to properties of data type text and text[]. If you choose false, you will not be able to use this property in bm25 or hybrid search. This property has no affect on vectorization decisions done by modules
    public bool? IndexSearchable { get; set; }

    // Whether to include this property in the filterable, range-based Roaring Bitmap index. Provides better performance for range queries compared to filterable index in large datasets. Applicable only to properties of data type int, number, date.
    public bool? IndexRangeFilters { get; set; }

    // Determines tokenization of the property as separate words or whole field. Optional. Applies to text and text[] data types. Allowed values are `word` (default; splits on any non-alphanumerical, lowercases), `lowercase` (splits on white spaces, lowercases), `whitespace` (splits on white spaces), `field` (trims). Not supported for remaining data types
    // Enum: [word lowercase whitespace field trigram gse kagome_kr kagome_ja]
    public string? Tokenization { get; set; }

    // The properties of the nested object(s). Applies to object and object[] data types.
    public NestedProperty[]? NestedProperties { get; set; }
}

public class NestedProperty : PropertyBase { }

public class Property : Property<object> { }

// For the end-user API, keep Properties and References separated.
// When API calls combine/split as required.
public class Property<TModuleConfig> : PropertyBase
{
    // (Deprecated). Whether to include this property in the inverted index. If `false`, this property cannot be used in `where` filters, `bm25` or `hybrid` search. <br/><br/>Unrelated to vectorization behavior (deprecated as of v1.19; use indexFilterable or/and indexSearchable instead)
    public bool? IndexInverted { get; set; }

    // Configuration specific to modules this Weaviate instance has installed
    public TModuleConfig? ModuleConfig { get; set; }
}

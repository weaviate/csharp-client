

namespace Weaviate.Client.Models;

public static class DataType
{
    public static string Text { get; } = "text";
    public static string Int { get; } = "int";
    public static string Reference(string property) { return $"{property[0].ToString().ToUpper()}{property.Substring(1)}"; }
}

public class Property
{
    public required string Name { get; set; }
    public IList<string> DataType { get; set; } = new List<string>();
    public string? Description { get; set; }
    public bool? IndexFilterable { get; set; }
    public bool? IndexInverted { get; set; }
    public bool? IndexRangeFilters { get; set; }
    public bool? IndexSearchable { get; set; }

}

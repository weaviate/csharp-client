

namespace Weaviate.Client.Models;

public static class DataType
{
    public static string Text { get; } = "text";
    public static string Int { get; } = "int";
    public static string Reference(string property) => property.Capitalize();
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

    public static Property Text(string name)
    {
        return new Property
        {
            Name = name,
            DataType = { Models.DataType.Text },
        };
    }

    public static Property Int(string name)
    {
        return new Property
        {
            Name = name,
            DataType = { Models.DataType.Int },
        };
    }
}

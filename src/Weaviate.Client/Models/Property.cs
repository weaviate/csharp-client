
namespace Weaviate.Client.Models;

public interface IDataType
{
}

public class DataType<TData> : IDataType { }

public class Property
{
    public string? Name { get; set; }
    public IList<IDataType> DataType { get; set; } = new List<IDataType>();
    public string? Description { get; set; }
    public bool? IndexFilterable { get; set; }
    public bool? IndexInverted { get; set; }
    public bool? IndexRangeFilters { get; set; }
    public bool? IndexSearchable { get; set; }

}

namespace Weaviate.Client.Models;

public record Sort
{
    private Sort() { }

    internal V1.SortBy InternalSort { get; init; } = new V1.SortBy();

    public static Sort ByCreationTime(bool ascending = true)
    {
        var s = ByProperty("_creationTimeUnix");
        return ascending ? s.Ascending() : s.Descending();
    }

    public static Sort ByProperty(string name)
    {
        return new Sort().ByProperty(name);
    }

    public Sort Ascending()
    {
        InternalSort.Ascending = true;
        return this;
    }

    public Sort Descending()
    {
        InternalSort.Ascending = false;
        return this;
    }
}

public static partial class SortExtensions
{
    public static Sort ByProperty(this Sort sort, string name)
    {
        sort.InternalSort.Path.Add(name);
        return sort;
    }
}

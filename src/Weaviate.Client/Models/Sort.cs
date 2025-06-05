namespace Weaviate.Client.Models;

public record Sort
{
    private Sort() { }

    internal V1.SortBy InternalSort { get; init; } = new V1.SortBy();

    public static Sort ByCreationTime(bool ascending = true)
    {
        var s = new Sort().ByProperty("_creationTimeUnix");
        return ascending ? s.Ascending() : s.Descending();
    }

    public Sort ByProperty(string name)
    {
        InternalSort.Path.Add(name);
        return this;
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

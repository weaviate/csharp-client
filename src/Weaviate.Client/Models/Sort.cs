namespace Weaviate.Client.Models;

using V1 = Grpc.Protobuf.V1;

/// <summary>
/// The sort
/// </summary>
public record Sort
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Sort"/> class
    /// </summary>
    private Sort() { }

    /// <summary>
    /// Gets or inits the value of the internal sort
    /// </summary>
    internal V1.SortBy InternalSort { get; init; } = new V1.SortBy();

    /// <summary>
    /// Bys the creation time using the specified ascending
    /// </summary>
    /// <param name="ascending">The ascending</param>
    /// <returns>The sort</returns>
    public static Sort ByCreationTime(bool ascending = true)
    {
        var s = ByProperty("_creationTimeUnix");
        return ascending ? s.Ascending() : s.Descending();
    }

    /// <summary>
    /// Bys the property using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The sort</returns>
    public static Sort ByProperty(string name)
    {
        return new Sort().ByProperty(name);
    }

    /// <summary>
    /// Ascendings this instance
    /// </summary>
    /// <returns>The sort</returns>
    public Sort Ascending()
    {
        InternalSort.Ascending = true;
        return this;
    }

    /// <summary>
    /// Descendings this instance
    /// </summary>
    /// <returns>The sort</returns>
    public Sort Descending()
    {
        InternalSort.Ascending = false;
        return this;
    }
}

/// <summary>
/// The sort extensions class
/// </summary>
public static partial class SortExtensions
{
    /// <summary>
    /// Bys the property using the specified sort
    /// </summary>
    /// <param name="sort">The sort</param>
    /// <param name="name">The name</param>
    /// <returns>The sort</returns>
    public static Sort ByProperty(this Sort sort, string name)
    {
        sort.InternalSort.Path.Add(name);
        return sort;
    }
}

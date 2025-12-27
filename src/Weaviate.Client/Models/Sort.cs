namespace Weaviate.Client.Models;

using V1 = Grpc.Protobuf.V1;

/// <summary>
/// Represents a sort specification for ordering query results.
/// </summary>
/// <remarks>
/// Use <see cref="ByProperty"/> to create a sort by a specific property,
/// or <see cref="ByCreationTime"/> to sort by object creation time.
/// Chain with <see cref="Ascending"/> or <see cref="Descending"/> to specify sort direction.
/// </remarks>
/// <example>
/// <code>
/// var sort = Sort.ByProperty("age").Descending();
/// var sortByCreation = Sort.ByCreationTime(ascending: false);
/// </code>
/// </example>
public record Sort
{
    private Sort() { }

    internal V1.SortBy InternalSort { get; init; } = new V1.SortBy();

    /// <summary>
    /// Creates a sort specification for ordering by object creation time.
    /// </summary>
    /// <param name="ascending">If true, sorts in ascending order (oldest first); otherwise descending (newest first). Defaults to true.</param>
    /// <returns>A <see cref="Sort"/> instance configured to sort by creation time.</returns>
    public static Sort ByCreationTime(bool ascending = true)
    {
        var s = ByProperty("_creationTimeUnix");
        return ascending ? s.Ascending() : s.Descending();
    }

    /// <summary>
    /// Creates a sort specification for ordering by a specified property.
    /// </summary>
    /// <param name="name">The name of the property to sort by.</param>
    /// <returns>A <see cref="Sort"/> instance. Chain with <see cref="Ascending"/> or <see cref="Descending"/> to specify direction.</returns>
    public static Sort ByProperty(string name)
    {
        return new Sort().ByProperty(name);
    }

    /// <summary>
    /// Configures this sort to use ascending order.
    /// </summary>
    /// <returns>This <see cref="Sort"/> instance for method chaining.</returns>
    public Sort Ascending()
    {
        InternalSort.Ascending = true;
        return this;
    }

    /// <summary>
    /// Configures this sort to use descending order.
    /// </summary>
    /// <returns>This <see cref="Sort"/> instance for method chaining.</returns>
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

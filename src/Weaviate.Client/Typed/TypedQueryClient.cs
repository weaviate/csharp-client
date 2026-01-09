namespace Weaviate.Client.Typed;

/// <summary>
/// Strongly-typed wrapper for QueryClient that returns typed results.
/// All query methods return WeaviateObject&lt;T&gt; instead of untyped WeaviateObject.
/// </summary>
/// <typeparam name="T">The C# type to deserialize object properties into.</typeparam>
public partial class TypedQueryClient<T>
    where T : class, new()
{
    private readonly QueryClient _queryClient;

    /// <summary>
    /// Initializes a new instance of the TypedQueryClient class.
    /// </summary>
    /// <param name="queryClient">The underlying QueryClient to wrap.</param>
    public TypedQueryClient(QueryClient queryClient)
    {
        ArgumentNullException.ThrowIfNull(queryClient);

        _queryClient = queryClient;
    }
}

namespace Weaviate.Client.Typed;

/// <summary>
/// Strongly-typed wrapper for GenerateClient that returns typed results.
/// All generative methods return GenerativeWeaviateResult&lt;T&gt; or GenerativeGroupByResult&lt;T&gt; instead of untyped results.
/// </summary>
/// <typeparam name="T">The C# type to deserialize object properties into.</typeparam>
public partial class TypedGenerateClient<T>
    where T : class, new()
{
    private readonly GenerateClient _generateClient;

    /// <summary>
    /// Initializes a new instance of the TypedGenerateClient class.
    /// </summary>
    /// <param name="generateClient">The underlying GenerateClient to wrap.</param>
    public TypedGenerateClient(GenerateClient generateClient)
    {
        ArgumentNullException.ThrowIfNull(generateClient);

        _generateClient = generateClient;
    }
}

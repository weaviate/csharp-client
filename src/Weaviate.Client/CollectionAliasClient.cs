using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient
{
    public CollectionAliasClient Alias => new(this);
}

/// <summary>
/// Client for managing aliases scoped to a specific collection.
/// Provides collection-scoped alias operations by delegating to the AliasClient with the collection name fixed.
/// </summary>
public partial class CollectionAliasClient : BaseCollectionClient
{
    internal CollectionAliasClient(CollectionClient collectionClient)
        : base(collectionClient) { }

    /// <summary>
    /// Create a new alias pointing to this collection
    /// </summary>
    /// <param name="aliasName">The name of the alias to create</param>
    /// <returns>The created alias</returns>
    public async Task<Alias> Add(string aliasName)
    {
        var alias = new Alias(aliasName, CollectionName);
        return await Client.Alias.Add(alias);
    }

    /// <summary>
    /// List all aliases pointing to this collection
    /// </summary>
    /// <returns>Enumerable of all aliases pointing to this collection</returns>
    public async Task<IEnumerable<Alias>> List()
    {
        return await Client.Alias.List(CollectionName);
    }

    /// <summary>
    /// Claim an existing alias by updating it to point to this collection
    /// </summary>
    /// <param name="aliasName">The name of the alias to claim</param>
    /// <returns>The updated alias</returns>
    public async Task<Alias> Claim(string aliasName)
    {
        return await Client.Alias.Update(aliasName, CollectionName);
    }
}

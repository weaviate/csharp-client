using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing collection aliases at the Weaviate instance level.
/// </summary>
public class AliasClient
{
    private readonly WeaviateClient _client;

    internal AliasClient(WeaviateClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Get an alias by name
    /// </summary>
    /// <param name="aliasName">The name of the alias to retrieve</param>
    /// <returns>The alias with its target collection</returns>
    public async Task<Alias> Get(string aliasName)
    {
        var dto = await _client.RestClient.AliasGet(aliasName);
        return ToModel(dto);
    }

    /// <summary>
    /// Create a new alias pointing to a collection
    /// </summary>
    /// <param name="alias">The alias to create</param>
    /// <returns>The created alias</returns>
    public async Task<Alias> Add(Alias alias)
    {
        var dto = ToDto(alias);
        var result = await _client.RestClient.CollectionAliasesPost(dto);
        return ToModel(result);
    }

    /// <summary>
    /// List all aliases, optionally filtered by collection name
    /// </summary>
    /// <param name="collectionName">Optional collection name to filter aliases</param>
    /// <returns>Enumerable of all aliases (or aliases pointing to the specified collection)</returns>
    public async Task<IEnumerable<Alias>> List(string? collectionName = null)
    {
        var dtos = await _client.RestClient.CollectionAliasesGet(collectionName);
        return dtos.Select(ToModel);
    }

    /// <summary>
    /// Update an alias to point to a different collection
    /// </summary>
    /// <param name="aliasName">The name of the alias to update</param>
    /// <param name="targetCollection">The new target collection name</param>
    /// <returns>The updated alias</returns>
    public async Task<Alias> Update(string aliasName, string targetCollection)
    {
        var dto = await _client.RestClient.AliasPut(aliasName, targetCollection);
        return ToModel(dto);
    }

    /// <summary>
    /// Delete an alias
    /// </summary>
    /// <param name="aliasName">The name of the alias to delete</param>
    public async Task Delete(string aliasName)
    {
        await _client.RestClient.AliasDelete(aliasName);
    }

    private static Alias ToModel(Rest.Dto.Alias dto)
    {
        return new Alias(dto.Alias1 ?? string.Empty, dto.Class ?? string.Empty);
    }

    private static Rest.Dto.Alias ToDto(Alias model)
    {
        return new Rest.Dto.Alias { Alias1 = model.Name, Class = model.TargetClass };
    }
}

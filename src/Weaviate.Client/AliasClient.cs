using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing collection aliases at the Weaviate instance level.
/// </summary>
public class AliasClient
{
    private readonly WeaviateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AliasClient"/> class
    /// </summary>
    /// <param name="client">The client</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal AliasClient(WeaviateClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Get an alias by name
    /// </summary>
    /// <param name="aliasName">The name of the alias to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The alias with its target collection</returns>
    public async Task<Alias?> Get(string aliasName, CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        var dto = await _client.RestClient.AliasGet(aliasName, cancellationToken);
        return dto != null ? ToModel(dto) : null;
    }

    /// <summary>
    /// Create a new alias pointing to a collection
    /// </summary>
    /// <param name="alias">The alias to create</param>
    /// <param name="targetCollection">The target collection name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The created alias</returns>
    public async Task<Alias> Create(
        string alias,
        string targetCollection,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var dto = ToDto(new Alias(alias, targetCollection));
        var result = await _client.RestClient.CollectionAliasesPost(dto, cancellationToken);
        return ToModel(result);
    }

    /// <summary>
    /// List all aliases, optionally filtered by collection name
    /// </summary>
    /// <param name="collectionName">Optional collection name to filter aliases</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Enumerable of all aliases (or aliases pointing to the specified collection)</returns>
    public async Task<IEnumerable<Alias>> List(
        string? collectionName = null,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var dtos = await _client.RestClient.CollectionAliasesGet(collectionName, cancellationToken);
        return dtos.Select(ToModel);
    }

    /// <summary>
    /// Update an alias to point to a different collection
    /// </summary>
    /// <param name="aliasName">The name of the alias to update</param>
    /// <param name="targetCollection">The new target collection name</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The updated alias</returns>
    public async Task<Alias> Update(
        string aliasName,
        string targetCollection,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var dto = await _client.RestClient.AliasPut(aliasName, targetCollection, cancellationToken);
        return ToModel(dto);
    }

    /// <summary>
    /// Delete an alias
    /// </summary>
    /// <param name="aliasName">The name of the alias to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    public async Task<bool> Delete(string aliasName, CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        return await _client.RestClient.AliasDelete(aliasName, cancellationToken);
    }

    /// <summary>
    /// Returns the model using the specified dto
    /// </summary>
    /// <param name="dto">The dto</param>
    /// <returns>The alias</returns>
    private static Alias ToModel(Rest.Dto.Alias dto)
    {
        return new Alias(dto.Alias1 ?? string.Empty, dto.Class ?? string.Empty);
    }

    /// <summary>
    /// Returns the dto using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <returns>The rest dto alias</returns>
    private static Rest.Dto.Alias ToDto(Alias model)
    {
        return new Rest.Dto.Alias { Alias1 = model.Name, Class = model.TargetCollection };
    }
}

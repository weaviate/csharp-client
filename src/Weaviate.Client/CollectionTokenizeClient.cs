using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Exposes the per-property <c>/v1/schema/{className}/properties/{propertyName}/tokenize</c>
/// endpoint for a specific collection. Requires Weaviate server version 1.37.0 or later.
/// </summary>
public sealed class CollectionTokenizeClient
{
    private readonly WeaviateClient _client;
    private readonly string _collectionName;

    internal CollectionTokenizeClient(WeaviateClient client, string collectionName)
    {
        _client = client;
        _collectionName = collectionName;
    }

    /// <summary>
    /// Tokenizes <paramref name="text"/> using the tokenization method configured on
    /// property <paramref name="propertyName"/> of this collection.
    /// </summary>
    /// <param name="propertyName">The name of the property whose tokenization to apply.</param>
    /// <param name="text">The text to tokenize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tokenization result.</returns>
    /// <exception cref="WeaviateVersionMismatchException">
    /// Thrown when the connected server version is below 1.37.0.
    /// </exception>
    [RequiresWeaviateVersion(1, 37, 0)]
    public async Task<TokenizeResult> Property(
        string propertyName,
        string text,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(text);

        await _client.EnsureVersion<CollectionTokenizeClient>();

        var response =
            await _client.RestClient.TokenizeProperty(
                _collectionName,
                propertyName,
                new Rest.Dto.PropertyTokenizeRequest { Text = text },
                cancellationToken
            )
            ?? throw new WeaviateClientException(
                new InvalidOperationException(
                    "Tokenize property endpoint returned an empty response."
                )
            );

        return response.ToModel();
    }
}

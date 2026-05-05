using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Exposes the <c>/v1/tokenize</c> endpoint for inspecting how text is tokenized
/// with a given tokenization method and analyzer configuration.
/// Requires Weaviate server version 1.37.0 or later.
/// </summary>
public sealed class TokenizeClient
{
    private readonly WeaviateClient _client;

    internal TokenizeClient(WeaviateClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Tokenizes <paramref name="text"/> using the given <paramref name="tokenization"/> strategy.
    /// Returns the indexed and query token forms produced by the server.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    /// <param name="tokenization">The tokenization method to apply.</param>
    /// <param name="analyzerConfig">Optional text analyzer configuration (e.g. ASCII folding, stopword preset name).</param>
    /// <param name="stopwords">
    /// Optional one-off stopword block applied to this request. Mirrors the collection-level
    /// <c>invertedIndexConfig.stopwords</c> shape (preset + additions + removals).
    /// Mutually exclusive with <paramref name="stopwordPresets"/>.
    /// </param>
    /// <param name="stopwordPresets">
    /// Optional named stopword catalog. Each key is a preset name that can be referenced by
    /// <see cref="TextAnalyzerConfig.StopwordPreset"/>; each value is a plain list of stopword strings.
    /// Matches the collection-level <c>invertedIndexConfig.stopwordPresets</c> shape.
    /// Mutually exclusive with <paramref name="stopwords"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when both <paramref name="stopwords"/> and <paramref name="stopwordPresets"/> are supplied.
    /// </exception>
    /// <exception cref="WeaviateVersionMismatchException">
    /// Thrown when the connected server version is below 1.37.0.
    /// </exception>
    [RequiresWeaviateVersion(1, 37, 0)]
    public async Task<TokenizeResult> Text(
        string text,
        PropertyTokenization tokenization,
        TextAnalyzerConfig? analyzerConfig = null,
        StopwordConfig? stopwords = null,
        IDictionary<string, IList<string>>? stopwordPresets = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(text);
        if (stopwords is not null && stopwordPresets is not null)
            throw new ArgumentException(
                "stopwords and stopwordPresets are mutually exclusive; pass only one.",
                nameof(stopwords)
            );

        await _client.EnsureVersion<TokenizeClient>();

        var request = new Rest.Dto.TokenizeRequest
        {
            Text = text,
            Tokenization = tokenization.ToDto(),
            AnalyzerConfig = analyzerConfig.ToDto(),
            Stopwords = stopwords?.ToDto(),
            StopwordPresets = stopwordPresets,
        };

        var response =
            await _client.RestClient.Tokenize(request, cancellationToken)
            ?? throw new WeaviateClientException(
                new InvalidOperationException("Tokenize endpoint returned an empty response.")
            );

        return response.ToModel();
    }
}

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
    /// Returns the indexed and query forms produced by the server, plus the analyzer/stopword
    /// configurations that were applied.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    /// <param name="tokenization">The tokenization method to apply.</param>
    /// <param name="analyzerConfig">Optional text analyzer configuration (e.g. ASCII folding, stopword preset).</param>
    /// <param name="stopwordPresets">
    /// Optional named stopword configurations. Each key is a preset name that can be referenced by
    /// <see cref="TokenizeAnalyzerConfig.StopwordPreset"/>. Each value is a <see cref="StopwordConfig"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="WeaviateVersionMismatchException">
    /// Thrown when the connected server version is below 1.37.0.
    /// </exception>
    [RequiresWeaviateVersion(1, 37, 0)]
    public async Task<TokenizeResult> Text(
        string text,
        PropertyTokenization tokenization,
        TokenizeAnalyzerConfig? analyzerConfig = null,
        IDictionary<string, StopwordConfig>? stopwordPresets = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(text);

        await _client.EnsureVersion<TokenizeClient>();

        var request = new Rest.Dto.TokenizeRequest
        {
            Text = text,
            Tokenization = tokenization.ToDto(),
            AnalyzerConfig = analyzerConfig.ToDto(),
            StopwordPresets = stopwordPresets?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDto()
            ),
        };

        var response =
            await _client.RestClient.Tokenize(request, cancellationToken)
            ?? throw new WeaviateClientException(
                new InvalidOperationException("Tokenize endpoint returned an empty response.")
            );

        return response.ToModel();
    }
}

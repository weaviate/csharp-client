using System.Collections.Immutable;

namespace Weaviate.Client.Models;

/// <summary>
/// ASCII-folding configuration: enables accent/diacritic folding, with an
/// optional list of characters to exclude. When set on
/// <see cref="TextAnalyzerConfig.AsciiFold"/>, folding is applied; when
/// null, folding is disabled.
/// </summary>
/// <param name="Ignore">
/// Optional list of characters that should be excluded from ASCII folding.
/// </param>
public sealed record AsciiFoldConfig(IReadOnlyList<string>? Ignore = null);

/// <summary>
/// Optional text-analyzer configuration for the tokenize endpoint.
/// Mirrors the server's <c>TextAnalyzerConfig</c>.
/// </summary>
public sealed record TextAnalyzerConfig
{
    /// <summary>
    /// ASCII-folding configuration. When non-null, accent/diacritic marks are
    /// folded to their base characters (e.g. 'école' → 'ecole'), except for
    /// characters listed in <see cref="AsciiFoldConfig.Ignore"/>.
    /// When null, folding is disabled.
    /// </summary>
    public AsciiFoldConfig? AsciiFold { get; init; }

    /// <summary>
    /// Stopword preset name. May be a built-in preset (<c>"en"</c>, <c>"none"</c>)
    /// or the name of a custom preset provided via the
    /// <c>stopwordPresets</c> dictionary on <see cref="TokenizeClient.Text"/>.
    /// </summary>
    public string? StopwordPreset { get; init; }
}

/// <summary>
/// Result of a tokenize request.
/// </summary>
public sealed record TokenizeResult
{
    /// <summary>
    /// Tokens as they are stored in the inverted index.
    /// </summary>
    public ImmutableList<string> Indexed { get; init; } = [];

    /// <summary>
    /// Tokens as they are used for query matching (after stopword removal, etc.).
    /// </summary>
    public ImmutableList<string> Query { get; init; } = [];
}

/// <summary>
/// Mapping helpers between public tokenize models and generated DTOs.
/// </summary>
internal static class TokenizeMapping
{
    internal static Rest.Dto.TokenizeRequestTokenization ToDto(this PropertyTokenization value) =>
        (Rest.Dto.TokenizeRequestTokenization)(int)value;

    internal static Rest.Dto.TextAnalyzerConfig? ToDto(this TextAnalyzerConfig? config) =>
        config is null
            ? null
            : new Rest.Dto.TextAnalyzerConfig
            {
                AsciiFold = config.AsciiFold is not null ? true : null,
                AsciiFoldIgnore = config.AsciiFold?.Ignore is { Count: > 0 } ignore
                    ? [.. ignore]
                    : null,
                StopwordPreset = config.StopwordPreset,
            };

    internal static TextAnalyzerConfig? ToModel(this Rest.Dto.TextAnalyzerConfig? dto) =>
        dto is null
            ? null
            : new TextAnalyzerConfig
            {
                AsciiFold =
                    dto.AsciiFold == true
                        ? new AsciiFoldConfig(
                            dto.AsciiFoldIgnore is { Count: > 0 } ignore ? [.. ignore] : null
                        )
                        : null,
                StopwordPreset = dto.StopwordPreset,
            };

    internal static Rest.Dto.StopwordConfig ToDto(this StopwordConfig config) =>
        new()
        {
            Preset = config.Preset.ToEnumMemberString(),
            Additions = config.Additions.Count > 0 ? [.. config.Additions] : null,
            Removals = config.Removals.Count > 0 ? [.. config.Removals] : null,
        };

    internal static StopwordConfig? ToModel(this Rest.Dto.StopwordConfig? dto) =>
        dto is null
            ? null
            : new StopwordConfig
            {
                Preset = string.IsNullOrEmpty(dto.Preset)
                    ? StopwordConfig.Presets.None
                    : dto.Preset.FromEnumMemberString<StopwordConfig.Presets>(),
                Additions = dto.Additions?.ToImmutableList() ?? [],
                Removals = dto.Removals?.ToImmutableList() ?? [],
            };

    internal static TokenizeResult ToModel(this Rest.Dto.TokenizeResponse dto) =>
        new()
        {
            Indexed = dto.Indexed?.ToImmutableList() ?? [],
            Query = dto.Query?.ToImmutableList() ?? [],
        };
}

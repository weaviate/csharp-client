using System.Collections.Immutable;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// Integration tests for the <c>/v1/tokenize</c> and
/// <c>/v1/schema/{className}/properties/{propertyName}/tokenize</c> endpoints.
/// Requires Weaviate server version 1.37.0 or later.
/// </summary>
[Collection("TestTokenize")]
public class TestTokenize : IntegrationTests
{
    // -----------------------------------------------------------------------
    // Serialization
    // -----------------------------------------------------------------------

    public static TheoryData<PropertyTokenization, string, string[]> TokenizationCases =>
        new()
        {
            {
                PropertyTokenization.Word,
                "The quick brown fox",
                new[] { "the", "quick", "brown", "fox" }
            },
            {
                PropertyTokenization.Lowercase,
                "Hello World Test",
                new[] { "hello", "world", "test" }
            },
            {
                PropertyTokenization.Whitespace,
                "Hello World Test",
                new[] { "Hello", "World", "Test" }
            },
            { PropertyTokenization.Field, "  Hello World  ", new[] { "Hello World" } },
            { PropertyTokenization.Trigram, "Hello", new[] { "hel", "ell", "llo" } },
        };

    [Theory]
    [MemberData(nameof(TokenizationCases))]
    public async Task Tokenization_Enum(
        PropertyTokenization tokenization,
        string text,
        string[] expectedTokens
    )
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var result = await _weaviate.Tokenize.Text(
            text,
            tokenization,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(tokenization, result.Tokenization);
        Assert.Equal(expectedTokens, result.Indexed);
        Assert.Equal(expectedTokens, result.Query);
    }

    [Fact]
    public async Task NoAnalyzerConfig()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var result = await _weaviate.Tokenize.Text(
            "hello world",
            PropertyTokenization.Word,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(PropertyTokenization.Word, result.Tokenization);
        Assert.Equal(new[] { "hello", "world" }, result.Indexed);
        Assert.Null(result.AnalyzerConfig);
    }

    [Fact]
    public async Task AsciiFold()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var cfg = new TextAnalyzerConfig { AsciiFold = new AsciiFoldConfig() };
        var result = await _weaviate.Tokenize.Text(
            "L'école est fermée",
            PropertyTokenization.Word,
            analyzerConfig: cfg,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(new[] { "l", "ecole", "est", "fermee" }, result.Indexed);
    }

    [Fact]
    public async Task AsciiFold_WithIgnore()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var cfg = new TextAnalyzerConfig { AsciiFold = new AsciiFoldConfig(Ignore: ["é"]) };
        var result = await _weaviate.Tokenize.Text(
            "L'école est fermée",
            PropertyTokenization.Word,
            analyzerConfig: cfg,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(new[] { "l", "école", "est", "fermée" }, result.Indexed);
    }

    [Fact]
    public async Task StopwordPreset_String()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var cfg = new TextAnalyzerConfig { StopwordPreset = "en" };
        var result = await _weaviate.Tokenize.Text(
            "The quick brown fox",
            PropertyTokenization.Word,
            analyzerConfig: cfg,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.DoesNotContain("the", result.Query);
        Assert.Contains("quick", result.Query);
    }

    [Fact]
    public async Task Combined_AsciiFold_Stopwords()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var cfg = new TextAnalyzerConfig
        {
            AsciiFold = new AsciiFoldConfig(Ignore: ["é"]),
            StopwordPreset = "en",
        };
        var result = await _weaviate.Tokenize.Text(
            "The école est fermée",
            PropertyTokenization.Word,
            analyzerConfig: cfg,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(new[] { "the", "école", "est", "fermée" }, result.Indexed);
        Assert.DoesNotContain("the", result.Query);
        Assert.Contains("école", result.Query);
    }

    [Fact]
    public async Task CustomPreset_Additions()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var cfg = new TextAnalyzerConfig { StopwordPreset = "custom" };
        var presets = new Dictionary<string, StopwordConfig>
        {
            ["custom"] = new StopwordConfig
            {
                Preset = StopwordConfig.Presets.None,
                Additions = ["test"],
            },
        };

        var result = await _weaviate.Tokenize.Text(
            "hello world test",
            PropertyTokenization.Word,
            analyzerConfig: cfg,
            stopwordPresets: presets,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(new[] { "hello", "world", "test" }, result.Indexed);
        Assert.Equal(new[] { "hello", "world" }, result.Query);
    }

    [Fact]
    public async Task CustomPreset_BaseAndRemovals()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var cfg = new TextAnalyzerConfig { StopwordPreset = "en-no-the" };
        var presets = new Dictionary<string, StopwordConfig>
        {
            ["en-no-the"] = new StopwordConfig
            {
                Preset = StopwordConfig.Presets.EN,
                Removals = ["the"],
            },
        };

        var result = await _weaviate.Tokenize.Text(
            "the quick",
            PropertyTokenization.Word,
            analyzerConfig: cfg,
            stopwordPresets: presets,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(new[] { "the", "quick" }, result.Indexed);
        Assert.Equal(new[] { "the", "quick" }, result.Query);
    }

    // -----------------------------------------------------------------------
    // Deserialization
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Result_Types()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var result = await _weaviate.Tokenize.Text(
            "hello",
            PropertyTokenization.Word,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.IsType<TokenizeResult>(result);
        Assert.IsType<ImmutableList<string>>(result.Indexed);
        Assert.IsType<ImmutableList<string>>(result.Query);
    }

    [Fact]
    public async Task AnalyzerConfig_Echoed()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var cfg = new TextAnalyzerConfig
        {
            AsciiFold = new AsciiFoldConfig(Ignore: ["é"]),
            StopwordPreset = "en",
        };
        var result = await _weaviate.Tokenize.Text(
            "L'école",
            PropertyTokenization.Word,
            analyzerConfig: cfg,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(result.AnalyzerConfig);
        Assert.NotNull(result.AnalyzerConfig!.AsciiFold);
        Assert.Equal(new[] { "é" }, result.AnalyzerConfig.AsciiFold!.Ignore);
        Assert.Equal("en", result.AnalyzerConfig.StopwordPreset);
    }

    [Fact]
    public async Task AnalyzerConfig_None()
    {
        RequireVersion<TokenizeClient>(nameof(TokenizeClient.Text));

        var result = await _weaviate.Tokenize.Text(
            "hello",
            PropertyTokenization.Word,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Null(result.AnalyzerConfig);
    }

    [Fact]
    public async Task PropertyTokenize_Field()
    {
        RequireVersion<CollectionTokenizeClient>(nameof(CollectionTokenizeClient.Property));

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "tag",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Field,
                },
            ]
        );

        var result = await collection.Tokenize.Property(
            "tag",
            "  Hello World  ",
            TestContext.Current.CancellationToken
        );

        Assert.Equal(PropertyTokenization.Field, result.Tokenization);
        Assert.Equal(new[] { "Hello World" }, result.Indexed);
    }
}

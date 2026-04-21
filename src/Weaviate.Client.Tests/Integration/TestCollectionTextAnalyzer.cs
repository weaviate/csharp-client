using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// Integration tests for property-level <c>textAnalyzer</c> configuration and
/// collection-level <c>stopwordPresets</c>. Requires Weaviate ≥ 1.37.0.
/// Ports <c>integration/test_collection_config.py</c> from weaviate-python-client PR #2006.
/// </summary>
[Collection("TestCollectionTextAnalyzer")]
public class TestCollectionTextAnalyzer : IntegrationTests
{
    private const string MinVersion = "1.37.0";

    // -----------------------------------------------------------------------
    // Collection-level stopwordPresets
    // -----------------------------------------------------------------------

    [Fact]
    public async Task StopwordPresets_AppliedAndRoundTripped()
    {
        RequireVersion(MinVersion, message: "stopwordPresets requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title_fr",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig { StopwordPreset = "fr" },
                },
                new Property
                {
                    Name = "title_en",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig { StopwordPreset = "en" },
                },
                new Property
                {
                    Name = "plain",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                },
            ],
            invertedIndexConfig: new()
            {
                StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["fr"] = new List<string> { "le", "la", "les" },
                },
            }
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);

        Assert.NotNull(config.InvertedIndexConfig);
        Assert.NotNull(config.InvertedIndexConfig!.StopwordPresets);
        Assert.Equal(
            new[] { "le", "la", "les" },
            config.InvertedIndexConfig.StopwordPresets!["fr"]
        );

        var titleFr = config.Properties.Single(p => p.Name == "title_fr");
        var titleEn = config.Properties.Single(p => p.Name == "title_en");
        var plain = config.Properties.Single(p => p.Name == "plain");

        Assert.NotNull(titleFr.TextAnalyzer);
        Assert.Equal("fr", titleFr.TextAnalyzer!.StopwordPreset);
        Assert.NotNull(titleEn.TextAnalyzer);
        Assert.Equal("en", titleEn.TextAnalyzer!.StopwordPreset);
        Assert.Null(plain.TextAnalyzer);
    }

    [Fact]
    public async Task StopwordPresets_Update_ReplacesPreset()
    {
        RequireVersion(MinVersion, message: "stopwordPresets requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title_fr",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig { StopwordPreset = "fr" },
                },
            ],
            invertedIndexConfig: new()
            {
                StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["fr"] = new List<string> { "le" },
                },
            }
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.Equal(new[] { "le" }, config.InvertedIndexConfig!.StopwordPresets!["fr"]);

        await collection.Config.Update(
            c =>
            {
                c.InvertedIndexConfig.StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["fr"] = new List<string> { "la" },
                };
            },
            TestContext.Current.CancellationToken
        );

        config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.Equal(new[] { "la" }, config.InvertedIndexConfig!.StopwordPresets!["fr"]);
    }

    [Fact]
    public async Task StopwordPresets_RemoveInUse_RejectedByServer()
    {
        RequireVersion(MinVersion, message: "stopwordPresets requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title_fr",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig { StopwordPreset = "fr" },
                },
            ],
            invertedIndexConfig: new()
            {
                StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["fr"] = new List<string> { "le", "la", "les" },
                },
            }
        );

        await Assert.ThrowsAnyAsync<WeaviateClientException>(async () =>
        {
            await collection.Config.Update(
                c =>
                {
                    c.InvertedIndexConfig.StopwordPresets = new Dictionary<string, IList<string>>();
                },
                TestContext.Current.CancellationToken
            );
        });

        // The original preset must survive the rejected update.
        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.Equal(
            new[] { "le", "la", "les" },
            config.InvertedIndexConfig!.StopwordPresets!["fr"]
        );
    }

    [Fact]
    public async Task StopwordPresets_RemoveUnused_Allowed()
    {
        RequireVersion(MinVersion, message: "stopwordPresets requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig { StopwordPreset = "fr" },
                },
            ],
            invertedIndexConfig: new()
            {
                StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["fr"] = new List<string> { "le", "la", "les" },
                    ["es"] = new List<string> { "el", "la", "los" },
                },
            }
        );

        // Drop only 'es' (unused). 'fr' is still referenced by title.
        await collection.Config.Update(
            c =>
            {
                c.InvertedIndexConfig.StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["fr"] = new List<string> { "le", "la", "les" },
                };
            },
            TestContext.Current.CancellationToken
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.Equal(
            new[] { "le", "la", "les" },
            config.InvertedIndexConfig!.StopwordPresets!["fr"]
        );
        Assert.False(config.InvertedIndexConfig.StopwordPresets.ContainsKey("es"));
    }

    [Fact]
    public async Task StopwordPresets_RemoveReferencedByNested_RejectedByServer()
    {
        RequireVersion(MinVersion, message: "stopwordPresets requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "doc",
                    DataType = DataType.Object,
                    NestedProperties =
                    [
                        new Property
                        {
                            Name = "body",
                            DataType = DataType.Text,
                            PropertyTokenization = PropertyTokenization.Word,
                            TextAnalyzer = new TextAnalyzerConfig { StopwordPreset = "fr" },
                        },
                    ],
                },
            ],
            invertedIndexConfig: new()
            {
                StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["fr"] = new List<string> { "le", "la", "les" },
                },
            }
        );

        await Assert.ThrowsAnyAsync<WeaviateClientException>(async () =>
        {
            await collection.Config.Update(
                c =>
                {
                    c.InvertedIndexConfig.StopwordPresets = new Dictionary<string, IList<string>>();
                },
                TestContext.Current.CancellationToken
            );
        });

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.Equal(
            new[] { "le", "la", "les" },
            config.InvertedIndexConfig!.StopwordPresets!["fr"]
        );
    }

    [Fact]
    public async Task UserDefinedStopwordPreset_OverridesBuiltin()
    {
        RequireVersion(MinVersion, message: "stopwordPresets requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig { StopwordPreset = "en" },
                },
            ],
            invertedIndexConfig: new()
            {
                StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["en"] = new List<string> { "hello" },
                },
            }
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.Equal(new[] { "hello" }, config.InvertedIndexConfig!.StopwordPresets!["en"]);

        var title = config.Properties.Single(p => p.Name == "title");
        Assert.NotNull(title.TextAnalyzer);
        Assert.Equal("en", title.TextAnalyzer!.StopwordPreset);
    }

    // -----------------------------------------------------------------------
    // Property-level TextAnalyzer
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TextAnalyzer_CombinedAsciiFoldAndStopwordPreset()
    {
        RequireVersion(MinVersion, message: "textAnalyzer requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig
                    {
                        AsciiFold = new AsciiFoldConfig(),
                        StopwordPreset = "en",
                    },
                },
            ]
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        var title = config.Properties.Single(p => p.Name == "title");

        Assert.NotNull(title.TextAnalyzer);
        Assert.NotNull(title.TextAnalyzer!.AsciiFold);
        Assert.Equal("en", title.TextAnalyzer.StopwordPreset);
    }

    [Fact]
    public async Task TextAnalyzer_AsciiFoldIgnore_RoundTrips()
    {
        RequireVersion(MinVersion, message: "textAnalyzer requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig
                    {
                        AsciiFold = new AsciiFoldConfig(Ignore: ["é"]),
                    },
                },
            ]
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        var title = config.Properties.Single(p => p.Name == "title");

        Assert.NotNull(title.TextAnalyzer);
        Assert.NotNull(title.TextAnalyzer!.AsciiFold);
        Assert.Equal(new[] { "é" }, title.TextAnalyzer.AsciiFold!.Ignore);
    }

    [Fact]
    public async Task TextAnalyzer_FullRoundTrip_FromDictStyleConfig()
    {
        RequireVersion(MinVersion, message: "textAnalyzer requires Weaviate >= 1.37.0");

        var collection = await CollectionFactory(
            properties:
            [
                new Property
                {
                    Name = "title",
                    DataType = DataType.Text,
                    PropertyTokenization = PropertyTokenization.Word,
                    TextAnalyzer = new TextAnalyzerConfig
                    {
                        AsciiFold = new AsciiFoldConfig(Ignore: ["é"]),
                        StopwordPreset = "fr",
                    },
                },
            ],
            invertedIndexConfig: new()
            {
                Stopwords = new StopwordConfig
                {
                    Preset = StopwordConfig.Presets.EN,
                    Additions = ["a"],
                    Removals = ["the"],
                },
                StopwordPresets = new Dictionary<string, IList<string>>
                {
                    ["fr"] = new List<string> { "le", "la", "les" },
                },
            }
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);

        Assert.Equal(StopwordConfig.Presets.EN, config.InvertedIndexConfig!.Stopwords!.Preset);
        Assert.Equal(new[] { "the" }, config.InvertedIndexConfig.Stopwords.Removals);
        Assert.Equal(
            new[] { "le", "la", "les" },
            config.InvertedIndexConfig.StopwordPresets!["fr"]
        );

        var title = config.Properties.Single(p => p.Name == "title");
        Assert.NotNull(title.TextAnalyzer);
        Assert.Equal("fr", title.TextAnalyzer!.StopwordPreset);
        Assert.NotNull(title.TextAnalyzer.AsciiFold);
        Assert.Equal(new[] { "é" }, title.TextAnalyzer.AsciiFold!.Ignore);
    }

    // -----------------------------------------------------------------------
    // Version-gate
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Property_TextAnalyzer_RaisesOnOldServer()
    {
        if (ServerVersionIsInRange(MinVersion))
        {
            Assert.Skip(
                $"Version gate only applies to Weaviate < {MinVersion}. Current: {_weaviate.WeaviateVersion}"
            );
        }

        await Assert.ThrowsAsync<WeaviateVersionMismatchException>(async () =>
        {
            await CollectionFactory(
                properties:
                [
                    new Property
                    {
                        Name = "title",
                        DataType = DataType.Text,
                        PropertyTokenization = PropertyTokenization.Word,
                        TextAnalyzer = new TextAnalyzerConfig { AsciiFold = new AsciiFoldConfig() },
                    },
                ]
            );
        });
    }

    [Fact]
    public async Task InvertedIndexConfig_StopwordPresets_RaisesOnOldServer()
    {
        if (ServerVersionIsInRange(MinVersion))
        {
            Assert.Skip(
                $"Version gate only applies to Weaviate < {MinVersion}. Current: {_weaviate.WeaviateVersion}"
            );
        }

        await Assert.ThrowsAsync<WeaviateVersionMismatchException>(async () =>
        {
            await CollectionFactory(
                invertedIndexConfig: new()
                {
                    StopwordPresets = new Dictionary<string, IList<string>>
                    {
                        ["fr"] = new List<string> { "le", "la" },
                    },
                }
            );
        });
    }
}

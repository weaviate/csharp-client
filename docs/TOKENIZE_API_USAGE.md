# Tokenize API Usage Guide

> **Version Requirement:**
> The tokenize endpoints require Weaviate **v1.37.0** or newer. Calls against earlier versions throw `WeaviateVersionMismatchException`.

This guide covers the Weaviate C# client's tokenize API — a pair of endpoints that let you inspect how the server would tokenize a piece of text, either with an ad-hoc tokenization strategy or using the one already configured on a collection property.

## Table of Contents

- [Overview](#overview)
- [Tokenization Methods](#tokenization-methods)
- [Ad-hoc Tokenization (`client.Tokenize.Text`)](#ad-hoc-tokenization-clienttokenizetext)
- [Property-scoped Tokenization (`collection.Tokenize.Property`)](#property-scoped-tokenization-collectiontokenizeproperty)
- [Analyzer Configuration](#analyzer-configuration)
- [Stopwords](#stopwords)
- [Result Shape](#result-shape)
- [Property-level Text Analyzer (schema)](#property-level-text-analyzer-schema)
- [Collection-level Stopword Presets (schema)](#collection-level-stopword-presets-schema)
- [Common Patterns](#common-patterns)

## Overview

The tokenize API exposes two REST endpoints:

| Method | Endpoint | Use when… |
|---|---|---|
| `client.Tokenize.Text(...)` | `POST /v1/tokenize` | You want to preview tokenization for arbitrary text with any method/config — no collection required. |
| `collection.Tokenize.Property(...)` | `POST /v1/schema/{class}/properties/{prop}/tokenize` | You want to tokenize text *exactly as it would be indexed* by a specific property of an existing collection. |

Both return a `TokenizeResult` containing two token lists:

- **`Indexed`** — tokens as they are stored in the inverted index.
- **`Query`** — tokens as they are used for query matching (after stopword removal, etc.).

These differ when stopwords are configured: a stopword like `"the"` is still indexed (so `BM25` can count it), but dropped from `Query` so it doesn't inflate match scores.

## Tokenization Methods

The `PropertyTokenization` enum covers all nine server-supported strategies:

| Method | Input | Output (`Indexed`) |
|---|---|---|
| `Word` | `"The quick brown fox"` | `["the", "quick", "brown", "fox"]` |
| `Lowercase` | `"Hello World Test"` | `["hello", "world", "test"]` |
| `Whitespace` | `"Hello World Test"` | `["Hello", "World", "Test"]` |
| `Field` | `"  Hello World  "` | `["Hello World"]` *(entire field, trimmed)* |
| `Trigram` | `"Hello"` | `["hel", "ell", "llo"]` |
| `Gse` | Chinese/Japanese | Requires `ENABLE_TOKENIZER_GSE=true` on the server |
| `Gse_ch` | Chinese-only GSE | Requires `ENABLE_TOKENIZER_GSE_CH=true` |
| `Kagome_ja` | Japanese | Requires `ENABLE_TOKENIZER_KAGOME_JA=true` |
| `Kagome_kr` | Korean | Requires `ENABLE_TOKENIZER_KAGOME_KR=true` |

## Ad-hoc Tokenization (`client.Tokenize.Text`)

The simplest call takes only a text and a tokenization method:

```csharp
using Weaviate.Client.Models;

var result = await client.Tokenize.Text(
    text: "The quick brown fox",
    tokenization: PropertyTokenization.Word
);

Console.WriteLine(string.Join(", ", result.Indexed));
// the, quick, brown, fox
```

Signature:

```csharp
Task<TokenizeResult> Tokenize.Text(
    string text,
    PropertyTokenization tokenization,
    TextAnalyzerConfig? analyzerConfig = null,
    StopwordConfig? stopwords = null,
    IDictionary<string, IList<string>>? stopwordPresets = null,
    CancellationToken cancellationToken = default
);
```

`stopwords` and `stopwordPresets` are mutually exclusive — passing both throws `ArgumentException`.

## Property-scoped Tokenization (`collection.Tokenize.Property`)

When you want to see how a specific property would tokenize text — using that property's configured tokenization — use the collection-scoped variant:

```csharp
var collection = client.Collections.Use("Article");

var result = await collection.Tokenize.Property(
    propertyName: "title",
    text: "  Hello World  "
);

Console.WriteLine(string.Join(", ", result.Indexed)); // Hello World
```

The server uses the property's configured tokenization method and any analyzer config attached to the property — you don't pass either yourself.

## Analyzer Configuration

`TextAnalyzerConfig` controls two optional analyzer stages: **ASCII folding** and **stopword removal**.

### ASCII Folding

`AsciiFoldConfig` is a nullable record — `null` means folding is disabled, non-`null` means it's enabled. The `Ignore` list lets you exempt specific characters from folding.

```csharp
var cfg = new TextAnalyzerConfig
{
    AsciiFold = new AsciiFoldConfig(), // folding enabled, nothing ignored
};

var result = await client.Tokenize.Text(
    "L'école est fermée",
    PropertyTokenization.Word,
    analyzerConfig: cfg
);
// result.Indexed == ["l", "ecole", "est", "fermee"]
```

Ignore a specific character:

```csharp
var cfg = new TextAnalyzerConfig
{
    AsciiFold = new AsciiFoldConfig(Ignore: ["é"]),
};

var result = await client.Tokenize.Text(
    "L'école est fermée",
    PropertyTokenization.Word,
    analyzerConfig: cfg
);
// result.Indexed == ["l", "école", "est", "fermée"]
```

> **Tip:** Modeling `AsciiFold` as a nullable record makes the "ignore without fold" state unrepresentable — you can't accidentally pass `Ignore` without enabling folding.

### Stopwords

Use a built-in preset (`"en"`, `"none"`) via the `StopwordPreset` field:

```csharp
var cfg = new TextAnalyzerConfig { StopwordPreset = "en" };

var result = await client.Tokenize.Text(
    "The quick brown fox",
    PropertyTokenization.Word,
    analyzerConfig: cfg
);

// result.Indexed  → ["the", "quick", "brown", "fox"]     (all tokens kept in index)
// result.Query    → ["quick", "brown", "fox"]            ("the" removed for queries)
```

## Stopwords

There are two ways to feed stopwords into a tokenize call:

1. **`stopwordPresets`** — a `name → word-list` dictionary. Each value is a flat list of stopwords for that preset. `TextAnalyzerConfig.StopwordPreset` then references one by name. A preset name that matches a built-in (`"en"`, `"none"`) replaces the built-in for this call.
2. **`stopwords`** — a one-off `StopwordConfig` (`preset` + `additions` + `removals`) applied directly. Mirrors the collection-level `invertedIndexConfig.stopwords` shape.

The two parameters are **mutually exclusive** — pass one or the other.

### Custom named preset

```csharp
var cfg = new TextAnalyzerConfig { StopwordPreset = "custom" };

var presets = new Dictionary<string, IList<string>>
{
    ["custom"] = new[] { "test" },
};

var result = await client.Tokenize.Text(
    "hello world test",
    PropertyTokenization.Word,
    analyzerConfig: cfg,
    stopwordPresets: presets
);

// result.Indexed → ["hello", "world", "test"]
// result.Query   → ["hello", "world"]          ("test" dropped)
```

### One-off `stopwords` block

Use `stopwords` when you want a base preset plus tweaks for a single call without defining a named preset:

```csharp
var result = await client.Tokenize.Text(
    "the quick",
    PropertyTokenization.Word,
    stopwords: new StopwordConfig
    {
        Preset = StopwordConfig.Presets.EN,
        Removals = ["the"],
    }
);

// "the" was removed from the EN base, so it survives in both lists:
// result.Indexed → ["the", "quick"]
// result.Query   → ["the", "quick"]
```

### Combining folding and stopwords

```csharp
var cfg = new TextAnalyzerConfig
{
    AsciiFold = new AsciiFoldConfig(Ignore: ["é"]),
    StopwordPreset = "en",
};

var result = await client.Tokenize.Text(
    "The école est fermée",
    PropertyTokenization.Word,
    analyzerConfig: cfg
);

// result.Indexed → ["the", "école", "est", "fermee"]
// result.Query   → ["école", "est", "fermee"]         ("the" dropped)
```

## Result Shape

`TokenizeResult` is a sealed record with two members:

| Member | Type | Description |
|---|---|---|
| `Indexed` | `ImmutableList<string>` | Tokens as stored in the inverted index. |
| `Query` | `ImmutableList<string>` | Tokens used at query time (after stopword removal). |

The two lists differ when stopwords are configured: stopwords stay in `Indexed` (so BM25 can count document length) but are dropped from `Query` so they don't inflate match scores.

## Property-level Text Analyzer (schema)

Beyond the ad-hoc tokenize endpoint, Weaviate 1.37.0 also lets you pin analyzer options directly on a property at **collection-creation time**. The same `TextAnalyzerConfig` record is reused: whatever you would pass to `client.Tokenize.Text(...)` can also be attached to a property so every value indexed through that property gets the same treatment.

```csharp
await client.Collections.Create(new CollectionCreateParams
{
    Name = "Article",
    Properties =
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
    ],
});
```

Nested properties (object / object-array) accept `TextAnalyzer` too — they are `Property` records themselves, so the same field is available on every depth.

> **Version requirement:** `Property.TextAnalyzer` is only wired up for servers at Weaviate ≥ 1.37.0. `CollectionsClient.Create` performs a preflight version check and throws `WeaviateVersionMismatchException` if the connected server is older, before the schema request is sent.

## Collection-level Stopword Presets (schema)

Named stopword lists live on the collection's inverted-index config. A preset is a `preset-name → word-list` pair; properties reference one by name via `TextAnalyzer.StopwordPreset`.

```csharp
await client.Collections.Create(new CollectionCreateParams
{
    Name = "Article",
    InvertedIndexConfig = new InvertedIndexConfig
    {
        StopwordPresets = new Dictionary<string, IList<string>>
        {
            ["fr"] = new[] { "le", "la", "les" },
            ["custom_en"] = new[] { "foo", "bar" },
        },
    },
    Properties =
    [
        new Property
        {
            Name = "body",
            DataType = DataType.Text,
            PropertyTokenization = PropertyTokenization.Word,
            TextAnalyzer = new TextAnalyzerConfig { StopwordPreset = "fr" },
        },
    ],
});
```

Updating presets on an existing collection goes through the normal update path:

```csharp
await collection.Config.Update(c =>
{
    c.InvertedIndexConfig.StopwordPresets = new Dictionary<string, IList<string>>
    {
        ["fr"] = new[] { "le", "la", "les", "un", "une" },
    };
});
```

Setting `StopwordPresets` replaces the whole preset map on the server. The server rejects removing a preset that is still referenced by a property's `TextAnalyzer.StopwordPreset` — keep preset removals and property-config changes in the same update, or unwire the property first.

> **Version requirement:** Requires Weaviate ≥ 1.37.0. The preflight in `CollectionsClient.Create` also trips on `InvertedIndexConfig.StopwordPresets` before contacting the server.

## Common Patterns

### Previewing a query

Use `collection.Tokenize.Property` to see exactly what tokens the server will match your search against:

```csharp
var tokens = (await collection.Tokenize.Property("title", userQuery)).Query;
// Show tokens in the UI as "searching for: X, Y, Z"
```

### Debugging a BM25 miss

If a search misses a term you expected, tokenize both the query and a sample document with the same property:

```csharp
var queryTokens = (await collection.Tokenize.Property("body", "running")).Query;
var docTokens   = (await collection.Tokenize.Property("body", "I was running")).Indexed;

// If the sets don't intersect, BM25 can't match — check for stemming / stopwords.
```

### Verifying analyzer config round-trip

Pass the analyzer config to `Tokenize.Text` and check the tokens it returns:

```csharp
var cfg = new TextAnalyzerConfig
{
    AsciiFold = new AsciiFoldConfig(Ignore: ["é"]),
    StopwordPreset = "en",
};

var result = await client.Tokenize.Text("L'école", PropertyTokenization.Word, analyzerConfig: cfg);

// AsciiFold is on, but "é" is in Ignore → "école" survives intact.
Debug.Assert(result.Indexed.Contains("école"));
```

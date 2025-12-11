# Generative AI Shortcuts

This document describes the ergonomic shortcuts available in the C# client for working with generative AI operations, reducing boilerplate and improving code readability.

## Overview

The C# client provides two main shortcuts for working with generative AI:

1. **Implicit String Conversions**: Convert strings directly to `SinglePrompt` or `GroupedTask` objects
2. **Provider Enrichment**: Automatically apply a `GenerativeProvider` to prompts without explicit configuration

These features work together to create a more fluent and readable API for generative operations.

## Implicit String Conversions

### SinglePrompt Implicit Conversion

Instead of explicitly constructing `SinglePrompt` objects, you can use strings directly:

#### Before

```csharp
var result = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "artificial intelligence",
        prompt: new SinglePrompt("Summarize this article in one sentence")
    );
```

#### After

```csharp
var result = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "artificial intelligence",
        prompt: "Summarize this article in one sentence"
    );
```

The string is implicitly converted to a `SinglePrompt` object.

### GroupedTask Implicit Conversion

Similarly, `GroupedTask` accepts string implicit conversions:

#### Before

```csharp
var result = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "machine learning",
        groupBy: new GroupByRequest("category") { NumberOfGroups = 5 },
        groupedTask: new GroupedTask("Create a summary for each category")
    );
```

#### After

```csharp
var result = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "machine learning",
        groupBy: new GroupByRequest("category") { NumberOfGroups = 5 },
        groupedTask: "Create a summary for each category"
    );
```

## Provider Enrichment

### The `provider` Parameter

All generative methods now accept an optional `provider` parameter. When supplied, this provider is automatically applied to any prompts that don't already have a provider configured.

This eliminates the need to set `Provider` on each prompt individually.

#### Before

```csharp
var openai = new Providers.OpenAI 
{ 
    Model = "gpt-4",
    Temperature = 0.7
};

var result = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "quantum computing",
        prompt: new SinglePrompt("Explain this topic") 
        { 
            Provider = openai 
        }
    );
```

#### After

```csharp
var openai = new Providers.OpenAI 
{ 
    Model = "gpt-4",
    Temperature = 0.7
};

var result = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "quantum computing",
        prompt: "Explain this topic",
        provider: openai
    );
```

### Combining Both Shortcuts

The real power comes from combining both shortcuts:

#### Before

```csharp
var anthropic = new Providers.Anthropic 
{ 
    Model = "claude-3-opus-20240229",
    MaxTokens = 2048
};

var result = await client
    .Collections.Use("Article")
    .Generate
    .BM25(
        query: "large language models",
        prompt: new SinglePrompt("Create a detailed analysis") 
        { 
            Provider = anthropic 
        }
    );
```

#### After

```csharp
var anthropic = new Providers.Anthropic 
{ 
    Model = "claude-3-opus-20240229",
    MaxTokens = 2048
};

var result = await client
    .Collections.Use("Article")
    .Generate
    .BM25(
        query: "large language models",
        prompt: "Create a detailed analysis",
        provider: anthropic
    );
```

### Provider Precedence

If a prompt already has a provider set, the `provider` parameter **will not override it**:

```csharp
var openai = new Providers.OpenAI { Model = "gpt-4" };
var cohere = new Providers.Cohere { Model = "command" };

// This prompt explicitly uses Cohere
var promptWithProvider = new SinglePrompt("Test") { Provider = cohere };

var result = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "test",
        prompt: promptWithProvider,
        provider: openai  // This will NOT override cohere
    );

// The prompt will use Cohere, not OpenAI
```

## Complete Examples

### Example 1: Simple Question Answering

```csharp
using Weaviate.Client;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Generative;

var client = new WeaviateClient();

var openai = new Providers.OpenAI 
{ 
    Model = "gpt-4o",
    Temperature = 0.3
};

var result = await client
    .Collections.Use("Documentation")
    .Generate
    .NearText(
        text: "vector databases",
        limit: 5,
        prompt: "Based on the search results, explain what vector databases are used for",
        provider: openai
    );

foreach (var obj in result.Objects)
{
    Console.WriteLine($"Generated: {obj.Generated}");
}
```

### Example 2: Grouped Summaries

```csharp
var mistral = new Providers.Mistral 
{ 
    Model = "mistral-large-latest",
    Temperature = 0.5
};

var result = await client
    .Collections.Use("Article")
    .Generate
    .BM25(
        query: "climate change",
        groupBy: new GroupByRequest("category") { NumberOfGroups = 3 },
        groupedTask: "Summarize the key points from articles in this category",
        provider: mistral
    );

foreach (var group in result.Groups)
{
    Console.WriteLine($"Category: {group.GroupedBy.Value}");
    Console.WriteLine($"Summary: {group.Generated}");
}
```

### Example 3: Multiple Operations with Same Provider

```csharp
var anthropic = new Providers.Anthropic 
{ 
    Model = "claude-3-sonnet-20240229",
    MaxTokens = 1024
};

// Use the same provider across multiple operations
var summary = await client
    .Collections.Use("Article")
    .Generate
    .FetchObjectByID(
        id: articleId,
        prompt: "Provide a concise summary",
        provider: anthropic
    );

var analysis = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "related topics",
        prompt: "Analyze the relationships between these articles",
        provider: anthropic
    );
```

### Example 4: Hybrid Search with Generation

```csharp
var google = new Providers.Google 
{ 
    Model = "gemini-pro",
    Temperature = 0.8
};

var result = await client
    .Collections.Use("Product")
    .Generate
    .Hybrid(
        query: "wireless headphones",
        alpha: 0.5f,
        limit: 10,
        prompt: "Compare the features and create a buying guide",
        provider: google
    );
```

## Provider Configuration Options

The C# client supports many generative AI providers. Each has its own configuration options:

### OpenAI

```csharp
var provider = new Providers.OpenAI 
{
    Model = "gpt-4o",
    Temperature = 0.7,
    MaxTokens = 2048,
    TopP = 0.9,
    FrequencyPenalty = 0.0,
    PresencePenalty = 0.0
};
```

### Anthropic

```csharp
var provider = new Providers.Anthropic 
{
    Model = "claude-3-opus-20240229",
    MaxTokens = 4096,
    Temperature = 0.7,
    TopK = 40,
    TopP = 0.9
};
```

### Cohere

```csharp
var provider = new Providers.Cohere 
{
    Model = "command",
    MaxTokens = 2048,
    Temperature = 0.7,
    K = 50,
    P = 0.75
};
```

### Google

```csharp
var provider = new Providers.Google 
{
    Model = "gemini-pro",
    MaxTokens = 2048,
    Temperature = 0.7,
    TopK = 40,
    TopP = 0.9
};
```

### Mistral

```csharp
var provider = new Providers.Mistral 
{
    Model = "mistral-large-latest",
    MaxTokens = 2048,
    Temperature = 0.7,
    TopP = 0.9
};
```

### Ollama (Local)

```csharp
var provider = new Providers.Ollama 
{
    Model = "llama2",
    ApiEndpoint = "http://localhost:11434",
    Temperature = 0.7
};
```

### AWS Bedrock

```csharp
var provider = new Providers.AWS 
{
    Model = "anthropic.claude-v2",
    Region = "us-east-1",
    Service = "bedrock",
    Temperature = 0.7,
    MaxTokens = 2048
};
```

## Advanced: Setting Provider on Prompts

If you need fine-grained control, you can still set the provider directly on prompt objects:

```csharp
var openai = new Providers.OpenAI { Model = "gpt-4" };
var anthropic = new Providers.Anthropic { Model = "claude-3" };

// Different providers for different prompts
SinglePrompt summaryPrompt = "Summarize this";
summaryPrompt.Provider = openai;

GroupedTask analysisTask = "Analyze the groups";
analysisTask.Provider = anthropic;

var result = await client
    .Collections.Use("Article")
    .Generate
    .FetchObjects(
        limit: 10,
        prompt: summaryPrompt  // Uses OpenAI
    );
```

## Debugging

Both `SinglePrompt` and `GroupedTask` support a `Debug` flag for troubleshooting:

```csharp
SinglePrompt prompt = "Explain this concept";
prompt.Debug = true;

var result = await client
    .Collections.Use("Article")
    .Generate
    .NearText(
        text: "quantum mechanics",
        prompt: prompt,
        provider: new Providers.OpenAI { Model = "gpt-4" }
    );
```

When debug mode is enabled, additional information about the generative request may be included in the response (depending on the Weaviate server version).

## Migration Guide

If you have existing code using the old patterns, migration is straightforward:

### Pattern 1: Remove Explicit Prompt Constructors

```csharp
// Old
prompt: new SinglePrompt("your prompt text")

// New
prompt: "your prompt text"
```

### Pattern 2: Move Provider to Parameter

```csharp
// Old
var prompt = new SinglePrompt("your prompt") { Provider = myProvider };
// ... use prompt

// New
prompt: "your prompt",
provider: myProvider
```

### Pattern 3: Combine Both Changes

```csharp
// Old
var provider = new Providers.OpenAI { Model = "gpt-4" };
var prompt = new SinglePrompt("Explain this") { Provider = provider };
var result = await client.Collections.Use("Article").Generate.NearText(
    text: "AI",
    prompt: prompt
);

// New
var provider = new Providers.OpenAI { Model = "gpt-4" };
var result = await client.Collections.Use("Article").Generate.NearText(
    text: "AI",
    prompt: "Explain this",
    provider: provider
);
```

## Summary

These shortcuts make generative AI code in the C# client:

- **More Concise**: Fewer object constructions and property assignments
- **More Readable**: The intent is clearer without ceremony
- **More Maintainable**: Less boilerplate means less code to maintain
- **Backward Compatible**: Existing code continues to work unchanged

The shortcuts are available on all `GenerateClient` methods including:

- `FetchObjects` (with and without grouping)
- `FetchObjectByID`
- `FetchObjectsByIDs`
- `NearText` (with and without grouping)
- `NearVector` (with and without grouping)
- `BM25` (with and without grouping)
- `Hybrid` (with and without grouping, with multiple vector options)

For more information on generative AI in Weaviate, see the [official documentation](https://weaviate.io/developers/weaviate/modules/reader-generator-modules/generative-openai).

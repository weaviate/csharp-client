using Google.Protobuf;
using Weaviate.Client.Grpc;
using Weaviate.Client.Grpc.Protobuf.V1;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Generative;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for generative shortcuts: implicit string conversions and provider enrichment.
/// Tests verify end-to-end behavior through GenerateClient with mocked gRPC responses.
/// </summary>
[Collection("Unit Tests")]
public class GenerativeShortcutsTests
{
    #region Implicit Conversion Tests

    [Fact]
    public void SinglePrompt_ImplicitConversionFromString_Works()
    {
        // Arrange
        string promptText = "Summarize this document";

        // Act
        SinglePrompt prompt = promptText;

        // Assert
        Assert.NotNull(prompt);
        Assert.Equal(promptText, prompt.Prompt);
    }

    [Fact]
    public void GroupedTask_ImplicitConversionFromString_Works()
    {
        // Arrange
        string taskText = "Create a summary for each group";

        // Act
        GroupedTask task = taskText;

        // Assert
        Assert.NotNull(task);
        Assert.Equal(taskText, task.Task);
    }

    #endregion

    #region Provider Enrichment Integration Tests

    [Fact]
    public async Task GenerateClient_FetchObjects_WithStringPromptAndProvider_EnrichesPrompt()
    {
        // Arrange
        var provider = new Providers.OpenAI { Model = "gpt-4", Temperature = 0.7 };
        SearchRequest? capturedRequest = null;

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: async (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    // Capture and decode the request
                    var content = await request.Content!.ReadAsByteArrayAsync(ct);
                    capturedRequest = DecodeGrpcRequest<SearchRequest>(content);

                    // Return empty search reply
                    var reply = new SearchReply { Collection = "TestCollection" };
                    return Helpers.CreateGrpcResponse(reply);
                }
                return null;
            }
        );

        var grpcClient = new WeaviateGrpcClient(channel);
        var client = new WeaviateClient(grpcClient: grpcClient);

        // Act
        await client
            .Collections.Use("TestCollection")
            .Generate.FetchObjects(
                limit: 10,
                prompt: "Summarize this",
                provider: provider,
                cancellationToken: TestContext.Current.CancellationToken
            );

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Generative);
        Assert.NotNull(capturedRequest.Generative.Single);
        Assert.Equal("Summarize this", capturedRequest.Generative.Single.Prompt);
        Assert.Single(capturedRequest.Generative.Single.Queries);
        var providerQuery = capturedRequest.Generative.Single.Queries[0];
        Assert.NotNull(providerQuery.Openai);
        Assert.Equal("gpt-4", providerQuery.Openai.Model);
        Assert.Equal(0.7f, providerQuery.Openai.Temperature);
    }

    [Fact]
    public async Task GenerateClient_FetchObjects_WithStringGroupedTaskAndProvider_EnrichesTask()
    {
        // Arrange
        var provider = new Providers.Anthropic
        {
            Model = "claude-3-opus-20240229",
            MaxTokens = 2048,
        };
        SearchRequest? capturedRequest = null;

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: async (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    var content = await request.Content!.ReadAsByteArrayAsync(ct);
                    capturedRequest = DecodeGrpcRequest<SearchRequest>(content);

                    var reply = new SearchReply { Collection = "TestCollection" };
                    return Helpers.CreateGrpcResponse(reply);
                }
                return null;
            }
        );

        var grpcClient = new WeaviateGrpcClient(channel);
        var client = new WeaviateClient(grpcClient: grpcClient);

        // Act
        await client
            .Collections.Use("TestCollection")
            .Generate.FetchObjects(
                new GroupByRequest("category") { NumberOfGroups = 5 },
                limit: 10,
                groupedTask: "Summarize by category",
                provider: provider,
                cancellationToken: TestContext.Current.CancellationToken
            );

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Generative);
        Assert.NotNull(capturedRequest.Generative.Grouped);
        Assert.Equal("Summarize by category", capturedRequest.Generative.Grouped.Task);
        Assert.Single(capturedRequest.Generative.Grouped.Queries);
        var providerQuery = capturedRequest.Generative.Grouped.Queries[0];
        Assert.NotNull(providerQuery.Anthropic);
        Assert.Equal("claude-3-opus-20240229", providerQuery.Anthropic.Model);
        Assert.Equal(2048, providerQuery.Anthropic.MaxTokens);
    }

    [Fact]
    public async Task GenerateClient_NearText_WithStringPromptAndProvider_EnrichesPrompt()
    {
        // Arrange
        var provider = new Providers.Cohere { Model = "command", MaxTokens = 1024 };
        SearchRequest? capturedRequest = null;

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: async (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    var content = await request.Content!.ReadAsByteArrayAsync(ct);
                    capturedRequest = DecodeGrpcRequest<SearchRequest>(content);

                    var reply = new SearchReply { Collection = "TestCollection" };
                    return Helpers.CreateGrpcResponse(reply);
                }
                return null;
            }
        );

        var grpcClient = new WeaviateGrpcClient(channel);
        var client = new WeaviateClient(grpcClient: grpcClient);

        // Act
        await client
            .Collections.Use("TestCollection")
            .Generate.NearText(
                text: "artificial intelligence",
                limit: 5,
                prompt: "Explain this concept",
                provider: provider,
                cancellationToken: TestContext.Current.CancellationToken
            );

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Generative);
        Assert.NotNull(capturedRequest.Generative.Single);
        Assert.Equal("Explain this concept", capturedRequest.Generative.Single.Prompt);
        Assert.Single(capturedRequest.Generative.Single.Queries);
        var providerQuery = capturedRequest.Generative.Single.Queries[0];
        Assert.NotNull(providerQuery.Cohere);
        Assert.Equal("command", providerQuery.Cohere.Model);
        Assert.Equal(1024, providerQuery.Cohere.MaxTokens);
    }

    [Fact]
    public async Task GenerateClient_PromptWithExistingProvider_NotOverriddenByParameter()
    {
        // Arrange
        var openaiProvider = new Providers.OpenAI { Model = "gpt-4" };
        var cohereProvider = new Providers.Cohere { Model = "command" };

        // Create prompt with explicit OpenAI provider
        var promptWithProvider = new SinglePrompt("Test prompt") { Provider = openaiProvider };

        SearchRequest? capturedRequest = null;

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: async (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    var content = await request.Content!.ReadAsByteArrayAsync(ct);
                    capturedRequest = DecodeGrpcRequest<SearchRequest>(content);

                    var reply = new SearchReply { Collection = "TestCollection" };
                    return Helpers.CreateGrpcResponse(reply);
                }
                return null;
            }
        );

        var grpcClient = new WeaviateGrpcClient(channel);
        var client = new WeaviateClient(grpcClient: grpcClient);

        // Act - pass Cohere as provider parameter, but prompt already has OpenAI
        await client
            .Collections.Use("TestCollection")
            .Generate.FetchObjects(
                limit: 10,
                prompt: promptWithProvider,
                provider: cohereProvider,
                cancellationToken: TestContext.Current.CancellationToken
            );

        // Assert - should use OpenAI (from prompt), not Cohere (from parameter)
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Generative);
        Assert.NotNull(capturedRequest.Generative.Single);
        Assert.Single(capturedRequest.Generative.Single.Queries);
        var providerQuery = capturedRequest.Generative.Single.Queries[0];
        Assert.NotNull(providerQuery.Openai);
        Assert.Equal("gpt-4", providerQuery.Openai.Model);
        Assert.Null(providerQuery.Cohere);
    }

    [Fact]
    public async Task GenerateClient_PromptWithoutProvider_EnrichedByParameter()
    {
        // Arrange
        var provider = new Providers.Mistral { Model = "mistral-large-latest", Temperature = 0.5 };

        // Create prompt without provider
        var promptWithoutProvider = new SinglePrompt("Test prompt");

        SearchRequest? capturedRequest = null;

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: async (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains("/weaviate.v1.Weaviate/Search"))
                {
                    var content = await request.Content!.ReadAsByteArrayAsync(ct);
                    capturedRequest = DecodeGrpcRequest<SearchRequest>(content);

                    var reply = new SearchReply { Collection = "TestCollection" };
                    return Helpers.CreateGrpcResponse(reply);
                }
                return null;
            }
        );

        var grpcClient = new WeaviateGrpcClient(channel);
        var client = new WeaviateClient(grpcClient: grpcClient);

        // Act - pass provider parameter with prompt that has no provider
        await client
            .Collections.Use("TestCollection")
            .Generate.FetchObjects(
                limit: 10,
                prompt: promptWithoutProvider,
                provider: provider,
                cancellationToken: TestContext.Current.CancellationToken
            );

        // Assert - should use Mistral from parameter
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Generative);
        Assert.NotNull(capturedRequest.Generative.Single);
        Assert.Single(capturedRequest.Generative.Single.Queries);
        var providerQuery = capturedRequest.Generative.Single.Queries[0];
        Assert.NotNull(providerQuery.Mistral);
        Assert.Equal("mistral-large-latest", providerQuery.Mistral.Model);
        Assert.Equal(0.5f, providerQuery.Mistral.Temperature);
    }

    #endregion

    #region Helper Methods

    private static T DecodeGrpcRequest<T>(byte[] content)
        where T : IMessage<T>, new()
    {
        // gRPC wire format: 1 byte compressed flag + 4 bytes length + message bytes
        var messageBytes = content.Skip(5).ToArray();
        var parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(messageBytes);
    }

    #endregion
}

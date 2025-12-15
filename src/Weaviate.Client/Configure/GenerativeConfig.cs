using Weaviate.Client.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

public class GenerativeConfigFactory
{
    internal GenerativeConfigFactory() { }
#pragma warning disable CA1822 // Mark members as static
    public IGenerativeConfig Custom(string type, object? config = null) =>
        new GenerativeConfig.Custom { Type = type, Config = config };

    public IGenerativeConfig AWS(
        string region,
        string service,
        string? model = null,
        string? endpoint = null,
        string? targetModel = null,
        string? targetVariant = null,
        int? maxTokenCount = null,
        int? maxTokensToSample = null,
        string[]? stopSequences = null,
        double? temperature = null,
        double? topP = null,
        double? topK = null
    ) =>
        new GenerativeConfig.AWS
        {
            Region = region,
            Service = service,
            Model = model,
            Endpoint = endpoint,
            TargetModel = targetModel,
            TargetVariant = targetVariant,
            MaxTokenCount = maxTokenCount,
            MaxTokensToSample = maxTokensToSample,
            StopSequences = stopSequences,
            Temperature = temperature,
            TopP = topP,
            TopK = topK,
        };

    public IGenerativeConfig Anthropic(
        string? baseURL = null,
        int? maxTokens = null,
        string? model = null,
        string[]? stopSequences = null,
        double? temperature = null,
        int? topK = null,
        double? topP = null
    ) =>
        new GenerativeConfig.Anthropic
        {
            BaseURL = baseURL,
            MaxTokens = maxTokens,
            Model = model,
            StopSequences = stopSequences,
            Temperature = temperature,
            TopK = topK,
            TopP = topP,
        };

    public IGenerativeConfig Anyscale(
        string? baseURL = null,
        string? model = null,
        double? temperature = null
    ) =>
        new GenerativeConfig.Anyscale
        {
            BaseURL = baseURL,
            Model = model,
            Temperature = temperature,
        };

    public IGenerativeConfig Cohere(
        int? k = null,
        string? model = null,
        string? baseURL = null,
        int? maxTokens = null,
        string[]? stopSequences = null,
        double? temperature = null
    ) =>
        new GenerativeConfig.Cohere
        {
            K = k,
            Model = model,
            BaseURL = baseURL,
            MaxTokens = maxTokens,
            StopSequences = stopSequences,
            Temperature = temperature,
        };

    public IGenerativeConfig Databricks(
        string endpoint,
        int? maxTokens = null,
        double? temperature = null,
        int? topK = null,
        double? topP = null
    ) =>
        new GenerativeConfig.Databricks
        {
            Endpoint = endpoint,
            MaxTokens = maxTokens,
            Temperature = temperature,
            TopK = topK,
            TopP = topP,
        };

    public IGenerativeConfig FriendliAI(
        string? baseURL = null,
        int? maxTokens = null,
        string? model = null,
        double? temperature = null
    ) =>
        new GenerativeConfig.FriendliAI
        {
            BaseURL = baseURL,
            MaxTokens = maxTokens,
            Model = model,
            Temperature = temperature,
        };

    public IGenerativeConfig Mistral(
        string? baseURL = null,
        int? maxTokens = null,
        string? model = null,
        double? temperature = null
    ) =>
        new GenerativeConfig.Mistral
        {
            BaseURL = baseURL,
            MaxTokens = maxTokens,
            Model = model,
            Temperature = temperature,
        };

    public IGenerativeConfig Nvidia(
        string? baseURL = null,
        int? maxTokens = null,
        string? model = null,
        double? temperature = null,
        double? topP = null
    ) =>
        new GenerativeConfig.Nvidia
        {
            BaseURL = baseURL,
            MaxTokens = maxTokens,
            Model = model,
            Temperature = temperature,
            TopP = topP,
        };

    public IGenerativeConfig Ollama(string? apiEndpoint = null, string? model = null) =>
        new GenerativeConfig.Ollama { ApiEndpoint = apiEndpoint, Model = model };

    public IGenerativeConfig OpenAI(
        string? model = null,
        string? baseURL = null,
        int? frequencyPenalty = null,
        int? maxTokens = null,
        int? presencePenalty = null,
        double? temperature = null,
        double? topP = null,
        string? apiVersion = null,
        string? reasoningEffort = null,
        string? verbosity = null
    ) =>
        new GenerativeConfig.OpenAI
        {
            Model = model,
            BaseURL = baseURL,
            FrequencyPenalty = frequencyPenalty,
            MaxTokens = maxTokens,
            PresencePenalty = presencePenalty,
            Temperature = temperature,
            TopP = topP,
            ApiVersion = apiVersion,
            ReasoningEffort = reasoningEffort,
            Verbosity = verbosity,
        };

    public IGenerativeConfig AzureOpenAI(
        string resourceName,
        string deploymentId,
        string? model = null,
        string? baseURL = null,
        int? frequencyPenalty = null,
        int? maxTokens = null,
        int? presencePenalty = null,
        double? temperature = null,
        double? topP = null,
        string? apiVersion = null,
        string? reasoningEffort = null,
        string? verbosity = null
    ) =>
        new GenerativeConfig.AzureOpenAI
        {
            ResourceName = resourceName,
            DeploymentId = deploymentId,
            Model = model,
            BaseURL = baseURL,
            FrequencyPenalty = frequencyPenalty,
            MaxTokens = maxTokens,
            PresencePenalty = presencePenalty,
            Temperature = temperature,
            TopP = topP,
            ApiVersion = apiVersion,
            ReasoningEffort = reasoningEffort,
            Verbosity = verbosity,
        };

    public IGenerativeConfig GoogleVertex(
        string? apiEndpoint = null,
        int? maxOutputTokens = null,
        string? model = null,
        string? projectId = null,
        string? endpointId = null,
        string? region = null,
        double? temperature = null,
        int? topK = null,
        double? topP = null
    ) =>
        new GenerativeConfig.GoogleVertex
        {
            ApiEndpoint = apiEndpoint,
            MaxOutputTokens = maxOutputTokens,
            Model = model,
            ProjectId = projectId,
            EndpointId = endpointId,
            Region = region,
            Temperature = temperature,
            TopK = topK,
            TopP = topP,
        };

    public IGenerativeConfig GoogleGemini(
        int? maxOutputTokens = null,
        string? model = null,
        double? temperature = null,
        int? topK = null,
        double? topP = null
    ) =>
        new GenerativeConfig.GoogleGemini
        {
            MaxOutputTokens = maxOutputTokens,
            Model = model,
            Temperature = temperature,
            TopK = topK,
            TopP = topP,
        };

    public IGenerativeConfig XAI(
        string? baseURL = null,
        int? maxTokens = null,
        string? model = null,
        double? temperature = null,
        double? topP = null
    ) =>
        new GenerativeConfig.XAI
        {
            BaseURL = baseURL,
            MaxTokens = maxTokens,
            Model = model,
            Temperature = temperature,
            TopP = topP,
        };

    public IGenerativeConfig ContextualAI(
        string? model = null,
        double? temperature = null,
        double? topP = null,
        long? maxNewTokens = null,
        string? systemPrompt = null,
        bool? avoidCommentary = null,
        string[]? knowledge = null
    ) =>
        new GenerativeConfig.ContextualAI
        {
            Model = model,
            Temperature = temperature,
            TopP = topP,
            MaxNewTokens = maxNewTokens,
            SystemPrompt = systemPrompt,
            AvoidCommentary = avoidCommentary,
            Knowledge = knowledge,
        };
#pragma warning restore CA1822 // Mark members as static
}

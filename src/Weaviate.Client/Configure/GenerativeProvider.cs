using Weaviate.Client.Models.Generative;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

public class GenerativeProviderFactory
{
    internal GenerativeProviderFactory() { }

#pragma warning disable CA1822 // Mark members as static

    public Providers.Anthropic Anthropic(
        string? baseUrl = null,
        long? maxTokens = null,
        string? model = null,
        double? temperature = null,
        long? topK = null,
        double? topP = null,
        List<string>? stopSequences = null,
        List<string>? images = null,
        List<string>? imageProperties = null
    ) =>
        new()
        {
            BaseUrl = baseUrl,
            MaxTokens = maxTokens,
            Model = model,
            Temperature = temperature,
            TopK = topK,
            TopP = topP,
            StopSequences = stopSequences,
            Images = images,
            ImageProperties = imageProperties,
        };

    public Providers.Anyscale Anyscale(
        string? baseUrl = null,
        string? model = null,
        double? temperature = null
    ) =>
        new()
        {
            BaseUrl = baseUrl,
            Model = model,
            Temperature = temperature,
        };

    public Providers.AWSBedrock AWSBedrock(
        string? model = null,
        double? temperature = null,
        string? region = null,
        string? endpoint = null,
        List<string>? images = null,
        List<string>? imageProperties = null,
        long? maxTokens = null
    ) =>
        new()
        {
            Model = model,
            Temperature = temperature,
            Region = region,
            Endpoint = endpoint,
            Images = images,
            ImageProperties = imageProperties,
            MaxTokens = maxTokens,
        };

    public Providers.AWSSagemaker AWSSagemaker(
        double? temperature = null,
        string? region = null,
        string? endpoint = null,
        string? targetModel = null,
        string? targetVariant = null,
        List<string>? images = null,
        List<string>? imageProperties = null,
        long? maxTokens = null
    ) =>
        new()
        {
            Temperature = temperature,
            Region = region,
            Endpoint = endpoint,
            TargetModel = targetModel,
            TargetVariant = targetVariant,
            Images = images,
            ImageProperties = imageProperties,
            MaxTokens = maxTokens,
        };

    public Providers.AzureOpenAI AzureOpenAI(
        double? frequencyPenalty = null,
        long? maxTokens = null,
        string? model = null,
        long? n = null,
        double? presencePenalty = null,
        List<string>? stop = null,
        double? temperature = null,
        double? topP = null,
        string? baseUrl = null,
        string? apiVersion = null,
        string? resourceName = null,
        string? deploymentId = null,
        bool? isAzure = null,
        List<string>? images = null,
        List<string>? imageProperties = null,
        Providers.AzureOpenAI.ReasoningEffortLevel? reasoningEffort = null,
        Providers.AzureOpenAI.VerbosityLevel? verbosity = null
    ) =>
        new()
        {
            FrequencyPenalty = frequencyPenalty,
            MaxTokens = maxTokens,
            Model = model,
            N = n,
            PresencePenalty = presencePenalty,
            Stop = stop,
            Temperature = temperature,
            TopP = topP,
            BaseUrl = baseUrl,
            ApiVersion = apiVersion,
            ResourceName = resourceName,
            DeploymentId = deploymentId,
            IsAzure = isAzure,
            Images = images,
            ImageProperties = imageProperties,
            ReasoningEffort = reasoningEffort,
            Verbosity = verbosity,
        };

    public Providers.Cohere Cohere(
        string? baseUrl = null,
        double? frequencyPenalty = null,
        long? maxTokens = null,
        string? model = null,
        long? k = null,
        double? p = null,
        double? presencePenalty = null,
        List<string>? stopSequences = null,
        double? temperature = null,
        List<string>? images = null,
        List<string>? imageProperties = null
    ) =>
        new()
        {
            BaseUrl = baseUrl,
            FrequencyPenalty = frequencyPenalty,
            MaxTokens = maxTokens,
            Model = model,
            K = k,
            P = p,
            PresencePenalty = presencePenalty,
            StopSequences = stopSequences,
            Temperature = temperature,
            Images = images,
            ImageProperties = imageProperties,
        };

    public Providers.ContextualAI ContextualAI(
        string? model = null,
        double? temperature = null,
        double? topP = null,
        long? maxNewTokens = null,
        string? systemPrompt = null,
        bool? avoidCommentary = null,
        string[]? knowledge = null
    ) =>
        new()
        {
            Model = model,
            Temperature = temperature,
            TopP = topP,
            MaxNewTokens = maxNewTokens,
            SystemPrompt = systemPrompt,
            AvoidCommentary = avoidCommentary,
            Knowledge = knowledge,
        };

    public Providers.Databricks Databricks(
        string? endpoint = null,
        string? model = null,
        double? frequencyPenalty = null,
        bool? logProbs = null,
        long? topLogProbs = null,
        long? maxTokens = null,
        long? n = null,
        double? presencePenalty = null,
        List<string>? stop = null,
        double? temperature = null,
        double? topP = null
    ) =>
        new()
        {
            Endpoint = endpoint,
            Model = model,
            FrequencyPenalty = frequencyPenalty,
            LogProbs = logProbs,
            TopLogProbs = topLogProbs,
            MaxTokens = maxTokens,
            N = n,
            PresencePenalty = presencePenalty,
            Stop = stop,
            Temperature = temperature,
            TopP = topP,
        };

    public Providers.Dummy Dummy() => new();

    public Providers.FriendliAI FriendliAI(
        string? baseUrl = null,
        string? model = null,
        long? maxTokens = null,
        double? temperature = null,
        long? n = null,
        double? topP = null
    ) =>
        new()
        {
            BaseUrl = baseUrl,
            Model = model,
            MaxTokens = maxTokens,
            Temperature = temperature,
            N = n,
            TopP = topP,
        };

    public Providers.GoogleGemini GoogleGemini(
        double? frequencyPenalty = null,
        long? maxTokens = null,
        string? model = null,
        double? presencePenalty = null,
        double? temperature = null,
        long? topK = null,
        double? topP = null,
        List<string>? stopSequences = null,
        string? apiEndpoint = null,
        string? projectId = null,
        string? endpointId = null,
        string? region = null,
        List<string>? images = null,
        List<string>? imageProperties = null
    ) =>
        new()
        {
            FrequencyPenalty = frequencyPenalty,
            MaxTokens = maxTokens,
            Model = model,
            PresencePenalty = presencePenalty,
            Temperature = temperature,
            TopK = topK,
            TopP = topP,
            StopSequences = stopSequences,
            ApiEndpoint = apiEndpoint,
            ProjectId = projectId,
            EndpointId = endpointId,
            Region = region,
            Images = images,
            ImageProperties = imageProperties,
        };

    public Providers.GoogleVertex GoogleVertex(
        double? frequencyPenalty = null,
        long? maxTokens = null,
        string? model = null,
        double? presencePenalty = null,
        double? temperature = null,
        long? topK = null,
        double? topP = null,
        List<string>? stopSequences = null,
        string? apiEndpoint = null,
        string? projectId = null,
        string? endpointId = null,
        string? region = null,
        List<string>? images = null,
        List<string>? imageProperties = null
    ) =>
        new()
        {
            FrequencyPenalty = frequencyPenalty,
            MaxTokens = maxTokens,
            Model = model,
            PresencePenalty = presencePenalty,
            Temperature = temperature,
            TopK = topK,
            TopP = topP,
            StopSequences = stopSequences,
            ApiEndpoint = apiEndpoint,
            ProjectId = projectId,
            EndpointId = endpointId,
            Region = region,
            Images = images,
            ImageProperties = imageProperties,
        };

    public Providers.Mistral Mistral(
        string? baseUrl = null,
        long? maxTokens = null,
        string? model = null,
        double? temperature = null,
        double? topP = null
    ) =>
        new()
        {
            BaseUrl = baseUrl,
            MaxTokens = maxTokens,
            Model = model,
            Temperature = temperature,
            TopP = topP,
        };

    public Providers.Nvidia Nvidia(
        string? baseUrl = null,
        string? model = null,
        double? temperature = null,
        double? topP = null,
        long? maxTokens = null
    ) =>
        new()
        {
            BaseUrl = baseUrl,
            Model = model,
            Temperature = temperature,
            TopP = topP,
            MaxTokens = maxTokens,
        };

    public Providers.Ollama Ollama(
        string? apiEndpoint = null,
        string? model = null,
        double? temperature = null,
        List<string>? images = null,
        List<string>? imageProperties = null
    ) =>
        new()
        {
            ApiEndpoint = apiEndpoint,
            Model = model,
            Temperature = temperature,
            Images = images,
            ImageProperties = imageProperties,
        };

    public Providers.OpenAI OpenAI(
        double? frequencyPenalty = null,
        long? maxTokens = null,
        string? model = null,
        long? n = null,
        double? presencePenalty = null,
        List<string>? stop = null,
        double? temperature = null,
        double? topP = null,
        string? baseUrl = null,
        string? apiVersion = null,
        string? resourceName = null,
        string? deploymentId = null,
        bool? isAzure = null,
        List<string>? images = null,
        List<string>? imageProperties = null,
        Providers.OpenAI.ReasoningEffortLevel? reasoningEffort = null,
        Providers.OpenAI.VerbosityLevel? verbosity = null
    ) =>
        new()
        {
            FrequencyPenalty = frequencyPenalty,
            MaxTokens = maxTokens,
            Model = model,
            N = n,
            PresencePenalty = presencePenalty,
            Stop = stop,
            Temperature = temperature,
            TopP = topP,
            BaseUrl = baseUrl,
            ApiVersion = apiVersion,
            ResourceName = resourceName,
            DeploymentId = deploymentId,
            IsAzure = isAzure,
            Images = images,
            ImageProperties = imageProperties,
            ReasoningEffort = reasoningEffort,
            Verbosity = verbosity,
        };

    public Providers.XAI XAI(
        string? baseUrl = null,
        string? model = null,
        double? temperature = null,
        double? topP = null,
        long? maxTokens = null,
        List<string>? images = null,
        List<string>? imageProperties = null
    ) =>
        new()
        {
            BaseUrl = baseUrl,
            Model = model,
            Temperature = temperature,
            TopP = topP,
            MaxTokens = maxTokens,
            Images = images,
            ImageProperties = imageProperties,
        };
#pragma warning restore CA1822 // Mark members as static
}

using Weaviate.Client.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// A factory for creating generative configurations.
/// </summary>
public class GenerativeConfigFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeConfigFactory"/> class
    /// </summary>
    internal GenerativeConfigFactory() { }
#pragma warning disable CA1822 // Mark members as static
    /// <summary>
    /// Create a custom generative configuration.
    /// </summary>
    /// <param name="type">The type of the generative module.</param>
    /// <param name="config">The configuration of the generative module.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
    public IGenerativeConfig Custom(string type, object? config = null) =>
        new GenerativeConfig.Custom { Type = type, Config = config ?? new { } };

    /// <summary>
    /// Create a generative configuration for AWS Bedrock.
    /// </summary>
    /// <param name="region">The AWS region.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="stopSequences">The stop sequences.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topP">The top P.</param>
    /// <param name="topK">The top K.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
    public IGenerativeConfig AWSBedrock(
        string region,
        string model,
        int? maxTokens = null,
        string[]? stopSequences = null,
        double? temperature = null,
        double? topP = null,
        double? topK = null
    ) =>
        new GenerativeConfig.AWS
        {
            Region = region,
            Service = "bedrock",
            Model = model,
            Endpoint = null,
            TargetModel = null,
            TargetVariant = null,
            MaxTokens = maxTokens,
            StopSequences = stopSequences,
            Temperature = temperature,
            TopP = topP,
            TopK = topK,
        };

    /// <summary>
    /// Create a generative configuration for AWS Sagemaker.
    /// </summary>
    /// <param name="region">The AWS region.</param>
    /// <param name="endpoint">The Sagemaker endpoint.</param>
    /// <param name="targetModel">The target model.</param>
    /// <param name="targetVariant">The target variant.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="stopSequences">The stop sequences.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topP">The top P.</param>
    /// <param name="topK">The top K.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
    public IGenerativeConfig AWSSagemaker(
        string region,
        string endpoint,
        string? targetModel = null,
        string? targetVariant = null,
        int? maxTokens = null,
        string[]? stopSequences = null,
        double? temperature = null,
        double? topP = null,
        double? topK = null
    ) =>
        new GenerativeConfig.AWS
        {
            Region = region,
            Service = "sagemaker",
            Model = null,
            Endpoint = endpoint,
            TargetModel = targetModel,
            TargetVariant = targetVariant,
            MaxTokens = maxTokens,
            StopSequences = stopSequences,
            Temperature = temperature,
            TopP = topP,
            TopK = topK,
        };

    /// <summary>
    /// Create a generative configuration for Anthropic.
    /// </summary>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="stopSequences">The stop sequences.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topK">The top K.</param>
    /// <param name="topP">The top P.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Anyscale.
    /// </summary>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="temperature">The temperature.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Cohere.
    /// </summary>
    /// <param name="k">The k value.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="stopSequences">The stop sequences.</param>
    /// <param name="temperature">The temperature.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Databricks.
    /// </summary>
    /// <param name="endpoint">The Databricks endpoint.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topK">The top K.</param>
    /// <param name="topP">The top P.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for FriendliAI.
    /// </summary>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="temperature">The temperature.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Mistral.
    /// </summary>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="temperature">The temperature.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Nvidia.
    /// </summary>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topP">The top P.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Ollama.
    /// </summary>
    /// <param name="apiEndpoint">The API endpoint.</param>
    /// <param name="model">The model to use.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
    public IGenerativeConfig Ollama(string? apiEndpoint = null, string? model = null) =>
        new GenerativeConfig.Ollama { ApiEndpoint = apiEndpoint, Model = model };

    /// <summary>
    /// Create a generative configuration for OpenAI.
    /// </summary>
    /// <param name="model">The model to use.</param>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="frequencyPenalty">The frequency penalty.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="presencePenalty">The presence penalty.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topP">The top P.</param>
    /// <param name="apiVersion">The API version.</param>
    /// <param name="reasoningEffort">The reasoning effort.</param>
    /// <param name="verbosity">The verbosity.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Azure OpenAI.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <param name="deploymentId">The deployment ID.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="frequencyPenalty">The frequency penalty.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="presencePenalty">The presence penalty.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topP">The top P.</param>
    /// <param name="apiVersion">The API version.</param>
    /// <param name="reasoningEffort">The reasoning effort.</param>
    /// <param name="verbosity">The verbosity.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Google Vertex.
    /// </summary>
    /// <param name="apiEndpoint">The API endpoint.</param>
    /// <param name="maxOutputTokens">The maximum number of output tokens.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="endpointId">The endpoint ID.</param>
    /// <param name="region">The region.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topK">The top K.</param>
    /// <param name="topP">The top P.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Google Gemini.
    /// </summary>
    /// <param name="maxOutputTokens">The maximum number of output tokens.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topK">The top K.</param>
    /// <param name="topP">The top P.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for XAI.
    /// </summary>
    /// <param name="baseURL">The base URL.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topP">The top P.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

    /// <summary>
    /// Create a generative configuration for Contextual AI.
    /// </summary>
    /// <param name="model">The model to use.</param>
    /// <param name="temperature">The temperature.</param>
    /// <param name="topP">The top P.</param>
    /// <param name="maxNewTokens">The maximum number of new tokens.</param>
    /// <param name="systemPrompt">The system prompt.</param>
    /// <param name="avoidCommentary">Whether to avoid commentary.</param>
    /// <param name="knowledge">The knowledge.</param>
    /// <returns>A <see cref="IGenerativeConfig"/> instance.</returns>
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

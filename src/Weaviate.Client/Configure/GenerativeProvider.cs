using Weaviate.Client.Models.Generative;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Weaviate.Client;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// The generative provider factory class
/// </summary>
public class GenerativeProviderFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeProviderFactory"/> class
    /// </summary>
    internal GenerativeProviderFactory() { }

#pragma warning disable CA1822 // Mark members as static

    /// <summary>
    /// Anthropics the base url
    /// </summary>
    /// <param name="baseUrl">The base url</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="model">The model</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topK">The top</param>
    /// <param name="topP">The top</param>
    /// <param name="stopSequences">The stop sequences</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <returns>The providers anthropic</returns>
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

    /// <summary>
    /// Anyscales the base url
    /// </summary>
    /// <param name="baseUrl">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="temperature">The temperature</param>
    /// <returns>The providers anyscale</returns>
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

    /// <summary>
    /// Awses the bedrock using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="region">The region</param>
    /// <param name="endpoint">The endpoint</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <returns>The providers aws bedrock</returns>
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

    /// <summary>
    /// Awses the sagemaker using the specified temperature
    /// </summary>
    /// <param name="temperature">The temperature</param>
    /// <param name="region">The region</param>
    /// <param name="endpoint">The endpoint</param>
    /// <param name="targetModel">The target model</param>
    /// <param name="targetVariant">The target variant</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <returns>The providers aws sagemaker</returns>
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

    /// <summary>
    /// Azures the open ai using the specified frequency penalty
    /// </summary>
    /// <param name="frequencyPenalty">The frequency penalty</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="model">The model</param>
    /// <param name="n">The </param>
    /// <param name="presencePenalty">The presence penalty</param>
    /// <param name="stop">The stop</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topP">The top</param>
    /// <param name="baseUrl">The base url</param>
    /// <param name="apiVersion">The api version</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="deploymentId">The deployment id</param>
    /// <param name="isAzure">The is azure</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <param name="reasoningEffort">The reasoning effort</param>
    /// <param name="verbosity">The verbosity</param>
    /// <returns>The providers azure open ai</returns>
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

    /// <summary>
    /// Coheres the base url
    /// </summary>
    /// <param name="baseUrl">The base url</param>
    /// <param name="frequencyPenalty">The frequency penalty</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="model">The model</param>
    /// <param name="k">The </param>
    /// <param name="p">The </param>
    /// <param name="presencePenalty">The presence penalty</param>
    /// <param name="stopSequences">The stop sequences</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <returns>The providers cohere</returns>
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

    /// <summary>
    /// Contextuals the ai using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topP">The top</param>
    /// <param name="maxNewTokens">The max new tokens</param>
    /// <param name="systemPrompt">The system prompt</param>
    /// <param name="avoidCommentary">The avoid commentary</param>
    /// <param name="knowledge">The knowledge</param>
    /// <returns>The providers contextual ai</returns>
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

    /// <summary>
    /// Databrickses the endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint</param>
    /// <param name="model">The model</param>
    /// <param name="frequencyPenalty">The frequency penalty</param>
    /// <param name="logProbs">The log probs</param>
    /// <param name="topLogProbs">The top log probs</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="n">The </param>
    /// <param name="presencePenalty">The presence penalty</param>
    /// <param name="stop">The stop</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topP">The top</param>
    /// <returns>The providers databricks</returns>
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

    /// <summary>
    /// Dummies this instance
    /// </summary>
    /// <returns>The providers dummy</returns>
    public Providers.Dummy Dummy() => new();

    /// <summary>
    /// Friendlis the ai using the specified base url
    /// </summary>
    /// <param name="baseUrl">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="n">The </param>
    /// <param name="topP">The top</param>
    /// <returns>The providers friendli ai</returns>
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

    /// <summary>
    /// Googles the gemini using the specified frequency penalty
    /// </summary>
    /// <param name="frequencyPenalty">The frequency penalty</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="model">The model</param>
    /// <param name="presencePenalty">The presence penalty</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topK">The top</param>
    /// <param name="topP">The top</param>
    /// <param name="stopSequences">The stop sequences</param>
    /// <param name="apiEndpoint">The api endpoint</param>
    /// <param name="projectId">The project id</param>
    /// <param name="endpointId">The endpoint id</param>
    /// <param name="region">The region</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <returns>The providers google gemini</returns>
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

    /// <summary>
    /// Googles the vertex using the specified frequency penalty
    /// </summary>
    /// <param name="frequencyPenalty">The frequency penalty</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="model">The model</param>
    /// <param name="presencePenalty">The presence penalty</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topK">The top</param>
    /// <param name="topP">The top</param>
    /// <param name="stopSequences">The stop sequences</param>
    /// <param name="apiEndpoint">The api endpoint</param>
    /// <param name="projectId">The project id</param>
    /// <param name="endpointId">The endpoint id</param>
    /// <param name="region">The region</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <returns>The providers google vertex</returns>
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

    /// <summary>
    /// Mistrals the base url
    /// </summary>
    /// <param name="baseUrl">The base url</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="model">The model</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topP">The top</param>
    /// <returns>The providers mistral</returns>
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

    /// <summary>
    /// Nvidias the base url
    /// </summary>
    /// <param name="baseUrl">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topP">The top</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <returns>The providers nvidia</returns>
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

    /// <summary>
    /// Ollamas the api endpoint
    /// </summary>
    /// <param name="apiEndpoint">The api endpoint</param>
    /// <param name="model">The model</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <returns>The providers ollama</returns>
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

    /// <summary>
    /// Opens the ai using the specified frequency penalty
    /// </summary>
    /// <param name="frequencyPenalty">The frequency penalty</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="model">The model</param>
    /// <param name="n">The </param>
    /// <param name="presencePenalty">The presence penalty</param>
    /// <param name="stop">The stop</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topP">The top</param>
    /// <param name="baseUrl">The base url</param>
    /// <param name="apiVersion">The api version</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="deploymentId">The deployment id</param>
    /// <param name="isAzure">The is azure</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <param name="reasoningEffort">The reasoning effort</param>
    /// <param name="verbosity">The verbosity</param>
    /// <returns>The providers open ai</returns>
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

    /// <summary>
    /// Xais the base url
    /// </summary>
    /// <param name="baseUrl">The base url</param>
    /// <param name="model">The model</param>
    /// <param name="temperature">The temperature</param>
    /// <param name="topP">The top</param>
    /// <param name="maxTokens">The max tokens</param>
    /// <param name="images">The images</param>
    /// <param name="imageProperties">The image properties</param>
    /// <returns>The providers xai</returns>
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

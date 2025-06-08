using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Models.Vectorizers;

/// <summary>
/// Factory class for creating VectorizerConfig instances based on type string and dynamic parameters.
/// </summary>
public static class VectorizerConfigFactory
{
    private static readonly Dictionary<string, Type> _configTypes = new()
    {
        { "none", typeof(NoneConfig) },
        { "img2vec-neural", typeof(Img2VecNeuralConfig) },
        { "multi2vec-clip", typeof(Multi2VecClipConfig) },
        { "multi2vec-cohere", typeof(Multi2VecCohereConfig) },
        { "multi2vec-bind", typeof(Multi2VecBindConfig) },
        { "multi2vec-palm", typeof(Multi2VecGoogleConfig) },
        { "multi2vec-jinaai", typeof(Multi2VecJinaAIConfig) },
        { "multi2vec-voyageai", typeof(Multi2VecVoyageAIConfig) },
        { "ref2vec-centroid", typeof(Ref2VecCentroidConfig) },
        { "text2vec-aws", typeof(Text2VecAWSConfig) },
        { "text2vec-azure-openai", typeof(Text2VecAzureOpenAIConfig) },
        { "text2vec-cohere", typeof(Text2VecCohereConfig) },
        { "text2vec-contextionary", typeof(Text2VecContextionaryConfig) },
        { "text2vec-databricks", typeof(Text2VecDatabricksConfig) },
        { "text2vec-gpt4all", typeof(Text2VecGPT4AllConfig) },
        { "text2vec-huggingface", typeof(Text2VecHuggingFaceConfig) },
        { "text2vec-jinaai", typeof(Text2VecJinaAIConfig) },
        { "text2vec-nvidia", typeof(Text2VecNvidiaConfig) },
        { "text2vec-mistral", typeof(Text2VecMistralConfig) },
        { "text2vec-ollama", typeof(Text2VecOllamaConfig) },
        { "text2vec-openai", typeof(Text2VecOpenAIConfig) },
        { "text2vec-palm", typeof(Text2VecGoogleConfig) },
        { "text2vec-transformers", typeof(Text2VecTransformersConfig) },
        { "text2vec-voyageai", typeof(Text2VecVoyageAIConfig) },
        { "text2vec-weaviate", typeof(Text2VecWeaviateConfig) },
    };

    /// <summary>
    /// Creates a VectorizerConfig instance based on the specified type and parameters.
    /// </summary>
    /// <param name="type">The vectorizer type string (e.g., "text2vec-openai", "multi2vec-clip")</param>
    /// <param name="parameters">Dynamic object containing the configuration parameters</param>
    /// <returns>A VectorizerConfig instance of the appropriate type</returns>
    /// <exception cref="ArgumentException">Thrown when the type is not supported</exception>
    /// <exception cref="InvalidOperationException">Thrown when the object cannot be created with the provided parameters</exception>
    public static VectorizerConfig Create(string type, object? parameters)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be null or empty", nameof(type));

        if (!_configTypes.TryGetValue(type, out var configType))
            throw new ArgumentException($"Unsupported vectorizer type: {type}", nameof(type));

        if (type == "none")
        {
            return new NoneConfig();
        }

        try
        {
            // Configure JsonSerializerOptions for better compatibility
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() },
            };

            // Convert dynamic object to JSON and then deserialize to the target type
            string json = JsonSerializer.Serialize(parameters, options);
            var config = JsonSerializer.Deserialize(json, configType, options) as VectorizerConfig;

            if (config == null)
                throw new InvalidOperationException(
                    $"Failed to create instance of {configType.Name}"
                );

            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to create {configType.Name} with provided parameters. "
                    + $"Please ensure all required parameters are provided and have correct types. "
                    + $"Error: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Creates a VectorizerConfig instance based on the specified type and parameters using reflection.
    /// This method provides more detailed error information but may be slower than the JSON approach.
    /// </summary>
    /// <param name="type">The vectorizer type string</param>
    /// <param name="parameters">Dynamic object containing the configuration parameters</param>
    /// <returns>A VectorizerConfig instance of the appropriate type</returns>
    public static VectorizerConfig CreateWithReflection(string type, object? parameters)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be null or empty", nameof(type));

        if (!_configTypes.TryGetValue(type, out var configType))
            throw new ArgumentException($"Unsupported vectorizer type: {type}", nameof(type));

        try
        {
            // Get constructor parameters
            var constructors = configType.GetConstructors();
            var constructor = constructors
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();
            if (constructor == null)
                throw new InvalidOperationException($"No constructors found for {configType.Name}");

            var constructorParams = constructor.GetParameters();

            // Convert dynamic to dictionary for easier parameter mapping
            Dictionary<string, object?> paramDict = ConvertDynamicToDictionary(parameters);

            // Prepare constructor arguments
            var args = new object?[constructorParams.Length];
            for (int i = 0; i < constructorParams.Length; i++)
            {
                var param = constructorParams[i];
                var paramName = param.Name;

                if (paramName == null)
                    throw new InvalidOperationException(
                        $"Parameter name is null for constructor parameter at index {i} in {configType.Name}"
                    );

                // Try to find matching parameter (case-insensitive)
                var matchingKey = paramDict.Keys.FirstOrDefault(k =>
                    string.Equals(k, paramName, StringComparison.OrdinalIgnoreCase)
                );

                if (matchingKey != null)
                {
                    var value = paramDict[matchingKey];
                    args[i] = ConvertValue(value, param.ParameterType);
                }
                else if (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
                else if (
                    param.ParameterType.IsValueType
                    && Nullable.GetUnderlyingType(param.ParameterType) == null
                )
                {
                    throw new InvalidOperationException(
                        $"Required parameter '{paramName}' not provided for {configType.Name}"
                    );
                }
                else
                {
                    args[i] = null;
                }
            }

            return (VectorizerConfig)Activator.CreateInstance(configType, args)!;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException(
                $"Failed to create {configType.Name} with provided parameters: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Gets all supported vectorizer types.
    /// </summary>
    /// <returns>An enumerable of supported type strings</returns>
    public static IEnumerable<string> GetSupportedTypes()
    {
        return _configTypes.Keys;
    }

    /// <summary>
    /// Checks if a vectorizer type is supported.
    /// </summary>
    /// <param name="type">The type string to check</param>
    /// <returns>True if the type is supported, false otherwise</returns>
    public static bool IsTypeSupported(string? type)
    {
        return !string.IsNullOrWhiteSpace(type) && _configTypes.ContainsKey(type);
    }

    private static Dictionary<string, object?> ConvertDynamicToDictionary(dynamic? obj)
    {
        if (obj is Dictionary<string, object?> dict)
            return dict;

        var result = new Dictionary<string, object?>();

        if (obj != null)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                result[property.Name] = property.GetValue(obj);
            }
        }

        return result;
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            if (value == null)
                return null;
            targetType = underlyingType;
        }

        // Handle arrays
        if (targetType.IsArray && value is IEnumerable<object> enumerable)
        {
            var elementType = targetType.GetElementType();
            if (elementType == null)
                throw new InvalidOperationException(
                    $"Unable to determine element type for array {targetType.Name}"
                );

            var list = enumerable.Select(item => ConvertValue(item, elementType)).ToArray();
            var array = Array.CreateInstance(elementType, list.Length);
            Array.Copy(list, array, list.Length);
            return array;
        }

        // Handle primitive type conversions
        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch (JsonException)
        {
            // Try JSON serialization/deserialization for complex types
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            string json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize(json, targetType, options);
        }
    }
}

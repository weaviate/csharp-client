using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Models.Vectorizers;

/// <summary>
/// Factory class for creating VectorizerConfig instances based on type string and dynamic parameters.
/// </summary>
internal static class VectorizerConfigFactory
{
    private static readonly Dictionary<string, Type> _configTypes = new()
    {
        { "none", typeof(Vectorizer.None) },
        { "img2vec-neural", typeof(Vectorizer.Img2VecNeural) },
        { "multi2vec-clip", typeof(Vectorizer.Multi2VecClip) },
        { "multi2vec-cohere", typeof(Vectorizer.Multi2VecCohere) },
        { "multi2vec-bind", typeof(Vectorizer.Multi2VecBind) },
        { "multi2vec-palm", typeof(Vectorizer.Multi2VecGoogle) },
        { "multi2vec-jinaai", typeof(Vectorizer.Multi2VecJinaAI) },
        { "multi2vec-voyageai", typeof(Vectorizer.Multi2VecVoyageAI) },
        { "ref2vec-centroid", typeof(Vectorizer.Ref2VecCentroid) },
        { "text2vec-aws", typeof(Vectorizer.Text2VecAWS) },
        { "text2vec-azure-openai", typeof(Vectorizer.Text2VecAzureOpenAI) },
        { "text2vec-cohere", typeof(Vectorizer.Text2VecCohere) },
        { "text2vec-contextionary", typeof(Vectorizer.Text2VecContextionary) },
        { "text2vec-databricks", typeof(Vectorizer.Text2VecDatabricks) },
        { "text2vec-gpt4all", typeof(Vectorizer.Text2VecGPT4All) },
        { "text2vec-huggingface", typeof(Vectorizer.Text2VecHuggingFace) },
        { "text2vec-jinaai", typeof(Vectorizer.Text2VecJinaAI) },
        { "text2vec-nvidia", typeof(Vectorizer.Text2VecNvidia) },
        { "text2vec-mistral", typeof(Vectorizer.Text2VecMistral) },
        { "text2vec-ollama", typeof(Vectorizer.Text2VecOllama) },
        { "text2vec-openai", typeof(Vectorizer.Text2VecOpenAI) },
        { "text2vec-palm", typeof(Vectorizer.Text2VecGoogle) },
        { "text2vec-transformers", typeof(Vectorizer.Text2VecTransformers) },
        { "text2vec-voyageai", typeof(Vectorizer.Text2VecVoyageAI) },
        { "text2vec-weaviate", typeof(Vectorizer.Text2VecWeaviate) },
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
            return new Vectorizer.None();
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

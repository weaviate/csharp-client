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
        { "none", typeof(Vectorizer.SelfProvided) },
        { "img2vec-neural", typeof(Vectorizer.Img2VecNeural) },
        { "multi2vec-clip", typeof(Vectorizer.Multi2VecClip) },
        { "multi2vec-cohere", typeof(Vectorizer.Multi2VecCohere) },
        { "multi2vec-bind", typeof(Vectorizer.Multi2VecBind) },
        { "multi2vec-google", typeof(Vectorizer.Multi2VecGoogle) },
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
        { "text2vec-google", typeof(Vectorizer.Text2VecGoogle) },
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
            return new Vectorizer.SelfProvided();
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

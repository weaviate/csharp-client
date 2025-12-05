using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Models.Vectorizers;

/// <summary>
/// Factory class for creating VectorizerConfig instances based on type string and dynamic parameters.
/// </summary>
internal static class VectorizerConfigFactory
{
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

        if (!VectorizerRegistry.TryGetVectorizer(type, out var info))
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

            if (JsonSerializer.Deserialize(json, info.Type, options) is not VectorizerConfig config)
                throw new InvalidOperationException(
                    $"Failed to create instance of {info.Type.Name}"
                );

            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to create {info.Type.Name} with provided parameters. "
                    + $"Please ensure all required parameters are provided and have correct types. "
                    + $"Error: {ex.Message}",
                ex
            );
        }
    }
}

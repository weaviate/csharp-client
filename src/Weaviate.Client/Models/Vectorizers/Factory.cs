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
        { Vectorizer.SelfProvided.IdentifierValue, typeof(Vectorizer.SelfProvided) },
        { Vectorizer.Img2VecNeural.IdentifierValue, typeof(Vectorizer.Img2VecNeural) },
        { Vectorizer.Multi2VecClip.IdentifierValue, typeof(Vectorizer.Multi2VecClip) },
        { Vectorizer.Multi2VecCohere.IdentifierValue, typeof(Vectorizer.Multi2VecCohere) },
        { Vectorizer.Multi2VecBind.IdentifierValue, typeof(Vectorizer.Multi2VecBind) },
        { Vectorizer.Multi2VecGoogle.IdentifierValue, typeof(Vectorizer.Multi2VecGoogle) },
        { Vectorizer.Multi2VecJinaAI.IdentifierValue, typeof(Vectorizer.Multi2VecJinaAI) },
        { Vectorizer.Multi2MultiVecJinaAI.IdentifierValue, typeof(Vectorizer.Multi2MultiVecJinaAI) },
        { Vectorizer.Multi2VecVoyageAI.IdentifierValue, typeof(Vectorizer.Multi2VecVoyageAI) },
        { Vectorizer.Ref2VecCentroid.IdentifierValue, typeof(Vectorizer.Ref2VecCentroid) },
        { Vectorizer.Text2VecAWS.IdentifierValue, typeof(Vectorizer.Text2VecAWS) },
        { Vectorizer.Text2VecAzureOpenAI.IdentifierValue, typeof(Vectorizer.Text2VecAzureOpenAI) },
        { Vectorizer.Text2VecCohere.IdentifierValue, typeof(Vectorizer.Text2VecCohere) },
        { Vectorizer.Text2VecDatabricks.IdentifierValue, typeof(Vectorizer.Text2VecDatabricks) },
        { Vectorizer.Text2VecHuggingFace.IdentifierValue, typeof(Vectorizer.Text2VecHuggingFace) },
        { Vectorizer.Text2VecJinaAI.IdentifierValue, typeof(Vectorizer.Text2VecJinaAI) },
        { Vectorizer.Text2MultiVecJinaAI.IdentifierValue, typeof(Vectorizer.Text2MultiVecJinaAI) },
        { Vectorizer.Text2VecNvidia.IdentifierValue, typeof(Vectorizer.Text2VecNvidia) },
        { Vectorizer.Text2VecMistral.IdentifierValue, typeof(Vectorizer.Text2VecMistral) },
        { Vectorizer.Text2VecModel2Vec.IdentifierValue, typeof(Vectorizer.Text2VecModel2Vec) },
        { Vectorizer.Text2VecOllama.IdentifierValue, typeof(Vectorizer.Text2VecOllama) },
        { Vectorizer.Text2VecOpenAI.IdentifierValue, typeof(Vectorizer.Text2VecOpenAI) },
        { Vectorizer.Text2VecGoogle.IdentifierValue, typeof(Vectorizer.Text2VecGoogle) },
        {
            Vectorizer.Text2VecTransformers.IdentifierValue,
            typeof(Vectorizer.Text2VecTransformers)
        },
        { Vectorizer.Text2VecVoyageAI.IdentifierValue, typeof(Vectorizer.Text2VecVoyageAI) },
        { Vectorizer.Text2VecWeaviate.IdentifierValue, typeof(Vectorizer.Text2VecWeaviate) },
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

        if (type == Vectorizer.SelfProvided.IdentifierValue)
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
}

using System.Reflection;
using Weaviate.Client.CollectionMapper.Attributes;
using Weaviate.Client.CollectionMapper.Internal;
using Weaviate.Client.Models;
using PropertyHelper = Weaviate.Client.CollectionMapper.Internal.PropertyHelper;

namespace Weaviate.Client.CollectionMapper.Mapping;

/// <summary>
/// Maps vector properties to and from Weaviate's Vectors dictionary.
/// Handles automatic extraction and injection of named vectors.
/// </summary>
internal static class VectorMapper
{
    /// <summary>
    /// Extracts vectors from an object's properties and returns a Vectors dictionary.
    /// Only includes vectors that have non-null values.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="obj">The object to extract vectors from.</param>
    /// <returns>A Vectors instance containing all non-null vectors, or null if no vectors found.</returns>
    public static Vectors? ExtractVectors<T>(T obj)
        where T : class
    {
        var type = typeof(T);
        var vectorProps = type.GetProperties()
            .Where(p => p.GetCustomAttribute<VectorAttributeBase>() != null)
            .ToList();

        if (vectorProps.Count == 0)
            return null;

        var vectors = new Vectors();

        foreach (var prop in vectorProps)
        {
            var vectorName = PropertyHelper.ToCamelCase(prop.Name);
            var value = prop.GetValue(obj);

            if (value == null)
                continue;

            // Handle float[] (single vector)
            if (prop.PropertyType == typeof(float[]) || prop.PropertyType == typeof(float?[]))
            {
                var floatArray = value as float[];
                if (floatArray != null && floatArray.Length > 0)
                {
                    vectors[vectorName] = floatArray;
                }
            }
            // Handle float[,] (multi-vector - not commonly used, but supported)
            else if (prop.PropertyType == typeof(float[,]))
            {
                var multiVector = value as float[,];
                if (multiVector != null && multiVector.GetLength(0) > 0)
                {
                    vectors[vectorName] = multiVector;
                }
            }
        }

        return vectors.Count > 0 ? vectors : null;
    }

    /// <summary>
    /// Injects vectors from a Vectors dictionary into an object's properties.
    /// Only populates properties that have a corresponding vector in the dictionary.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="obj">The object to inject vectors into.</param>
    /// <param name="vectors">The Vectors dictionary from Weaviate.</param>
    public static void InjectVectors<T>(T obj, Vectors? vectors)
        where T : class
    {
        if (vectors == null || vectors.Count == 0)
            return;

        var type = typeof(T);
        var vectorProps = type.GetProperties()
            .Where(p => p.GetCustomAttribute<VectorAttributeBase>() != null && p.CanWrite)
            .ToList();

        foreach (var prop in vectorProps)
        {
            var vectorName = PropertyHelper.ToCamelCase(prop.Name);

            if (!vectors.TryGetValue(vectorName, out var vectorValue))
                continue;

            try
            {
                // Handle float[] - use implicit conversion operator
                if (prop.PropertyType == typeof(float[]) || prop.PropertyType == typeof(float?[]))
                {
                    float[] floatArray = vectorValue; // Uses Vector's implicit operator
                    prop.SetValue(obj, floatArray);
                }
                // Handle float[,] - use implicit conversion operator
                else if (prop.PropertyType == typeof(float[,]))
                {
                    float[,] multiVector = vectorValue; // Uses Vector's implicit operator
                    prop.SetValue(obj, multiVector);
                }
            }
            catch
            {
                // If conversion fails, skip this vector
                continue;
            }
        }
    }

    /// <summary>
    /// Gets the names of all vector properties on a type (in camelCase).
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <returns>List of vector property names in camelCase.</returns>
    public static List<string> GetVectorPropertyNames<T>()
        where T : class
    {
        var type = typeof(T);
        return type.GetProperties()
            .Where(p => p.GetCustomAttribute<VectorAttributeBase>() != null)
            .Select(p => PropertyHelper.ToCamelCase(p.Name))
            .ToList();
    }

    /// <summary>
    /// Checks if a type has any vector properties.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <returns>True if the type has vector properties, false otherwise.</returns>
    public static bool HasVectorProperties<T>()
        where T : class
    {
        var type = typeof(T);
        return type.GetProperties().Any(p => p.GetCustomAttribute<VectorAttributeBase>() != null);
    }
}

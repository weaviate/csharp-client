using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.CollectionMapper.Mapping;

/// <summary>
/// Provides bidirectional mapping between C# objects and Weaviate objects.
/// Handles automatic extraction and injection of vectors and references.
/// </summary>
public static class CollectionMapperObjectMapper
{
    /// <summary>
    /// Populates a C# object from a WeaviateObject, including vectors and references.
    /// Uses the typed WeaviateObject's existing deserialization, then injects vectors and references.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="weaviateObject">The WeaviateObject to convert from.</param>
    /// <returns>A fully populated C# object with properties, vectors, and references.</returns>
    public static T FromWeaviateObject<T>(WeaviateObject<T> weaviateObject)
        where T : class, new()
    {
        // Get the typed object (this handles basic property deserialization via existing infrastructure)
        var obj = weaviateObject.Object;

        // Inject vectors if present
        if (weaviateObject.Vectors != null && weaviateObject.Vectors.Count > 0)
        {
            VectorMapper.InjectVectors(obj, weaviateObject.Vectors);
        }

        // Inject references if present
        if (weaviateObject.References != null && weaviateObject.References.Count > 0)
        {
            ReferenceMapper.InjectReferences(obj, weaviateObject.References);
        }

        return obj;
    }

    /// <summary>
    /// Converts a collection of WeaviateObject&lt;T&gt; to a collection of C# objects with vectors and references.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="weaviateObjects">The collection of WeaviateObjects.</param>
    /// <returns>Collection of fully populated C# objects.</returns>
    public static IEnumerable<T> FromWeaviateObjects<T>(
        IEnumerable<WeaviateObject<T>> weaviateObjects
    )
        where T : class, new()
    {
        return weaviateObjects.Select(FromWeaviateObject);
    }

    /// <summary>
    /// Checks if a type has any vector or reference properties that need special mapping.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <returns>True if the type has vectors or references, false otherwise.</returns>
    public static bool RequiresMapping<T>()
        where T : class
    {
        return VectorMapper.HasVectorProperties<T>()
            || ReferenceMapper.GetReferencePropertyNames<T>().Count > 0;
    }
}

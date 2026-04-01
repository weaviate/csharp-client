using Weaviate.Client.Models;

namespace Weaviate.Client.VectorData.Mapping;

/// <summary>
/// Maps between a user's record type and Weaviate's storage format.
/// </summary>
/// <typeparam name="TRecord">The user's record type.</typeparam>
internal interface IWeaviateRecordMapper<TRecord>
{
    /// <summary>
    /// Maps a record to Weaviate storage format.
    /// </summary>
    /// <param name="record">The record to map.</param>
    /// <returns>A tuple of the key (UUID), properties dictionary, and optional vectors.</returns>
    (Guid? Key, IDictionary<string, object?> Properties, Vectors? Vectors) MapToWeaviate(
        TRecord record
    );

    /// <summary>
    /// Maps a Weaviate object back to the record type.
    /// </summary>
    /// <param name="weaviateObject">The Weaviate object to map.</param>
    /// <returns>The mapped record.</returns>
    TRecord MapFromWeaviate(WeaviateObject weaviateObject);

    /// <summary>
    /// Gets the Weaviate storage property names for data properties.
    /// </summary>
    IReadOnlyList<string> GetStoragePropertyNames();

    /// <summary>
    /// Gets the Weaviate storage vector names.
    /// </summary>
    IReadOnlyList<string> GetVectorPropertyNames();
}

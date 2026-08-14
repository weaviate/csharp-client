namespace Weaviate.Client.Models;

/// <summary>
/// Normalises the per-modality field name lists of the multi2vec configurations.
/// </summary>
/// <remarks>
/// The server's <c>ValidateMultiModal</c> rejects any modality key that is present but empty
/// (<c>must contain at least one &lt;name&gt; field name in &lt;name&gt;Fields</c>), and rejects a
/// null value as well (<c>&lt;name&gt;Fields must be an array</c>). A modality the caller left
/// empty therefore has to be absent from the payload, not serialized as <c>[]</c>. Mapping it to
/// null achieves that: the REST serializer runs with
/// <see cref="System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull"/>, so a null
/// field list emits no key at all. This mirrors the empty-collection-is-null convention already
/// used by <see cref="VectorizerConfig.SourceProperties"/>.
/// </remarks>
internal static class ModalityFields
{
    /// <summary>
    /// Returns the field names of <paramref name="fields"/>, or null when the modality carries no
    /// fields. Applies the same <c>Count: &gt; 0</c> guard that
    /// <see cref="Vectorizer.VectorizerWeights.FromWeightedFields"/> applies to the matching weight
    /// array, so an empty modality drops out of both the field list and the weights together.
    /// </summary>
    /// <param name="fields">The weighted fields supplied for one modality</param>
    /// <returns>The field names, or null when there are none</returns>
    internal static string[]? OrNull(WeightedFields? fields) =>
        fields is { Count: > 0 } ? fields.FieldNames : null;

    /// <summary>
    /// Returns <paramref name="fields"/>, or null when the modality carries no fields. An empty
    /// array is as unusable to the server as an empty <see cref="WeightedFields"/>, so the plain
    /// string-array overloads normalise it the same way.
    /// </summary>
    /// <param name="fields">The field names supplied for one modality</param>
    /// <returns>The field names, or null when there are none</returns>
    internal static string[]? OrNull(string[]? fields) => fields is { Length: > 0 } ? fields : null;
}

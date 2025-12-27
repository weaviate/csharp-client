namespace Weaviate.Client.Models;

/// <summary>
/// Specifies how to adjust semantic search results by moving them towards or away from concepts or objects.
/// </summary>
/// <remarks>
/// Move operations allow you to fine-tune semantic search results by shifting the query vector
/// in the embedding space. This is useful for emphasizing certain concepts (MoveTo) or
/// de-emphasizing others (MoveAwayFrom).
/// </remarks>
/// <example>
/// <code>
/// // Move results towards "positive sentiment" concepts
/// var moveTo = new Move(force: 0.5f, concepts: new[] { "happy", "positive", "good" });
///
/// // Move results away from specific objects
/// var moveAway = new Move(force: 0.3f, objects: new[] { badExampleId });
///
/// // Combine both in a search
/// // .NearText("product review", moveTo: moveTo, moveAway: moveAway)
/// </code>
/// </example>
public record Move
{
    /// <summary>
    /// Gets the force (strength) of the move operation, typically between 0.0 and 1.0.
    /// </summary>
    /// <remarks>
    /// Higher values apply stronger movement in the embedding space.
    /// Values typically range from 0.0 (no effect) to 1.0 (maximum effect).
    /// </remarks>
    public float Force { get; }

    /// <summary>
    /// Gets the object IDs to move towards or away from.
    /// </summary>
    public Guid[]? Objects { get; }

    /// <summary>
    /// Gets the text concepts to move towards or away from.
    /// </summary>
    public string[]? Concepts { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Move"/> class with object IDs.
    /// </summary>
    /// <param name="force">The strength of the move operation (typically 0.0 to 1.0).</param>
    /// <param name="objects">Object IDs to use as anchors for the move.</param>
    public Move(float force, AutoArray<Guid> objects)
    {
        Force = force;
        Objects = [.. objects];
        Concepts = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Move"/> class with text concepts.
    /// </summary>
    /// <param name="force">The strength of the move operation (typically 0.0 to 1.0).</param>
    /// <param name="concepts">Text concepts to use as anchors for the move.</param>
    public Move(float force, AutoArray<string> concepts)
    {
        Force = force;
        Objects = null;
        Concepts = [.. concepts];
    }
}

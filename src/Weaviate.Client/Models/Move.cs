namespace Weaviate.Client.Models;

/// <summary>
/// The move
/// </summary>
public record Move
{
    /// <summary>
    /// Gets the value of the force
    /// </summary>
    public float Force { get; }

    /// <summary>
    /// Gets the value of the objects
    /// </summary>
    public Guid[]? Objects { get; }

    /// <summary>
    /// Gets the value of the concepts
    /// </summary>
    public string[]? Concepts { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Move"/> class
    /// </summary>
    /// <param name="uuids">The objects</param>
    /// <param name="force">The force</param>
    public Move(AutoArray<Guid> uuids, float force)
    {
        Force = force;
        Objects = [.. uuids];
        Concepts = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Move"/> class
    /// </summary>
    /// <param name="concepts">The concepts</param>
    /// <param name="force">The force</param>
    public Move(AutoArray<string> concepts, float force)
    {
        Force = force;
        Objects = null;
        Concepts = [.. concepts];
    }
}

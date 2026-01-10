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
    /// <param name="objects">The objects</param>
    /// <param name="force">The force</param>
    public Move(AutoArray<Guid> objects, float force)
    {
        Force = force;
        this.Objects = [.. objects];
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

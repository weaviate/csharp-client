namespace Weaviate.Client.Models;

public record Move
{
    public float Force { get; }
    public Guid[]? Objects { get; }
    public string[]? Concepts { get; }

    public Move(float force, AutoArray<Guid> objects)
    {
        Force = force;
        Objects = [.. objects];
        Concepts = null;
    }

    public Move(float force, AutoArray<string> concepts)
    {
        Force = force;
        Objects = null;
        Concepts = [.. concepts];
    }
}

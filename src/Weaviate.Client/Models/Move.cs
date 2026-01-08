namespace Weaviate.Client.Models;

public record Move
{
    public float Force { get; }
    public Guid[]? Objects { get; }
    public string[]? Concepts { get; }

    public Move(AutoArray<Guid> objects, float force)
    {
        Force = force;
        Objects = [.. objects];
        Concepts = null;
    }

    public Move(AutoArray<string> concepts, float force)
    {
        Force = force;
        Objects = null;
        Concepts = [.. concepts];
    }
}

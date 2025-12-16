namespace Weaviate.Client.Models;

public record Move
{
    public float Force { get; }
    public Guid[]? Objects { get; }
    public OneOrManyOf<string>? Concepts { get; }

    public Move(float force, Guid[] objects)
    {
        Force = force;
        Objects = objects;
        Concepts = null;
    }

    public Move(float force, OneOrManyOf<string> concepts)
    {
        Force = force;
        Objects = null;
        Concepts = concepts;
    }
}

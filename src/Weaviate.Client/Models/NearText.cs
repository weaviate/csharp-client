namespace Weaviate.Client.Models;

public record Move
{
    public float Force { get; }
    public Guid[]? Objects { get; }
    public OneOrManyOf<string>? Concepts { get; }

    public Move(float force, Guid[]? objects = null, OneOrManyOf<string>? concepts = null)
    {
        if (objects is null && (concepts == null || !concepts.Any()))
        {
            throw new ArgumentException("Either objects or concepts need to be given");
        }

        Force = force;
        Objects = objects;
        Concepts = concepts;
    }
}

namespace Weaviate.Client.Models;

public record Move
{
    public float Force { get; }
    public Guid? Objects { get; }
    public string? Concepts { get; }

    public Move(float force, Guid? objects = null, string? concepts = null)
    {
        if (objects is null && string.IsNullOrEmpty(concepts))
        {
            throw new ArgumentException("Either objects or concepts need to be given");
        }

        Force = force;
        Objects = objects;
        Concepts = concepts;
    }
}

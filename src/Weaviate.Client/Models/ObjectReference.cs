namespace Weaviate.Client.Models;

public record ObjectReference(string Name, IEnumerable<Guid> TargetID)
{
    public static implicit operator ObjectReference((string Name, Guid[] TargetID) value)
    {
        return new ObjectReference(value.Name, value.TargetID);
    }

    public static implicit operator ObjectReference((string Name, Guid TargetID) value)
    {
        return new ObjectReference(value.Name, [value.TargetID]);
    }

    public static implicit operator (string Name, Guid[] ID)(ObjectReference value) =>
        (value.Name, value.TargetID.ToArray());

    public static implicit operator List<ObjectReference>(ObjectReference value) => [value];
}

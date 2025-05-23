namespace Weaviate.Client.Models;

public record ObjectReference(string Name, Guid TargetID)
{
    public static implicit operator ObjectReference((string Name, Guid TargetID) value)
    {
        return new ObjectReference(value.Name, value.TargetID);
    }

    public static implicit operator (string Name, Guid Id)(ObjectReference value) =>
        (value.Name, value.TargetID);

    public static implicit operator List<ObjectReference>(ObjectReference value) =>
        [(value.Name, value.TargetID)];
}

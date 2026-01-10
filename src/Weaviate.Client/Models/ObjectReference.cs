namespace Weaviate.Client.Models;

/// <summary>
/// The object reference
/// </summary>
public record ObjectReference(string Name, IEnumerable<Guid> TargetID)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectReference"/> class
    /// </summary>
    /// <param name="Name">The name</param>
    /// <param name="TargetID">The target id</param>
    public ObjectReference(string Name, params Guid[] TargetID)
        : this(Name, (IEnumerable<Guid>)TargetID) { }

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

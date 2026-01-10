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

    /// <summary>
    /// Implicitly converts a tuple to an ObjectReference
    /// </summary>
    /// <param name="value">The tuple containing name and target IDs</param>
    public static implicit operator ObjectReference((string Name, Guid[] TargetID) value)
    {
        return new ObjectReference(value.Name, value.TargetID);
    }

    /// <summary>
    /// Implicitly converts a tuple to an ObjectReference
    /// </summary>
    /// <param name="value">The tuple containing name and target ID</param>
    public static implicit operator ObjectReference((string Name, Guid TargetID) value)
    {
        return new ObjectReference(value.Name, [value.TargetID]);
    }

    /// <summary>
    /// Implicitly converts an ObjectReference to a tuple
    /// </summary>
    /// <param name="value">The object reference</param>
    public static implicit operator (string Name, Guid[] ID)(ObjectReference value) =>
        (value.Name, value.TargetID.ToArray());

    /// <summary>
    /// Implicitly converts an ObjectReference to a List
    /// </summary>
    /// <param name="value">The object reference</param>
    public static implicit operator List<ObjectReference>(ObjectReference value) => [value];
}

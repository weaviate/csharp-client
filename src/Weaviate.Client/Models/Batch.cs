namespace Weaviate.Client.Models;

public record BatchInsertRequest<TData>(
    TData Data,
    Guid? ID = null,
    Vectors? Vectors = null,
    IEnumerable<ObjectReference>? References = null,
    string? Tenant = null
);

public record BatchInsertResponse(int Index, Guid? ID = null, WeaviateException? Error = null);

public class DeleteManyResult
{
    public long Failed { get; set; }
    public long Matches { get; set; }
    public long Successful { get; set; }
    public IEnumerable<DeleteManyObjectResult> Objects { get; set; } = [];
}

public class DeleteManyObjectResult
{
    public Guid Uuid { get; set; }
    public bool Successful { get; set; }
    public string? Error { get; set; }
}

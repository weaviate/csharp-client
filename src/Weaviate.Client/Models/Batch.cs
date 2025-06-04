namespace Weaviate.Client.Models;

public record BatchInsertRequest<TData>(
    TData Data,
    Guid? ID = null,
    NamedVectors? Vectors = null,
    IEnumerable<ObjectReference>? References = null,
    string? Tenant = null
);

public record BatchInsertResponse(int Index, Guid? ID = null, WeaviateException? Error = null);

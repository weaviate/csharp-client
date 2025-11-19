using System.Collections;

namespace Weaviate.Client.Models;

public record BatchInsertRequest<TData>(
    TData Data,
    Guid? ID = null,
    Vectors? Vectors = null,
    IEnumerable<ObjectReference>? References = null,
    string? Tenant = null
);

public static class BatchInsertRequest
{
    public static BatchInsertRequest<TData> Create<TData>(
        TData data,
        Guid? id = null,
        Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    )
    {
        return new BatchInsertRequest<TData>(data, id, vectors, references, tenant);
    }

    public static BatchInsertRequest<TData>[] Create<TData>(params TData[] data)
    {
        return data.Select(d => new BatchInsertRequest<TData>(d)).ToArray();
    }

    public static BatchInsertRequest<TData>[] Create<TData>(params (TData data, Guid id)[] requests)
    {
        return requests.Select(r => new BatchInsertRequest<TData>(r.data, r.id)).ToArray();
    }

    public static BatchInsertRequest<TData>[] Create<TData>(
        params (TData data, IEnumerable<ObjectReference>? references)[] requests
    )
    {
        return requests
            .Select(r => new BatchInsertRequest<TData>(r.data, References: r.references))
            .ToArray();
    }

    public static BatchInsertRequest<TData>[] Create<TData>(
        params (TData data, Vectors vectors)[] requests
    )
    {
        return requests
            .Select(r => new BatchInsertRequest<TData>(r.data, Vectors: r.vectors))
            .ToArray();
    }
}

public record BatchInsertResponseEntry(int Index, Guid? ID = null, WeaviateException? Error = null);

public record BatchInsertResponse : IEnumerable<BatchInsertResponseEntry>
{
    internal BatchInsertResponse(List<BatchInsertResponseEntry> entries)
    {
        Objects = entries;
    }

    public IEnumerable<BatchInsertResponseEntry> Objects { get; internal set; } =
        new List<BatchInsertResponseEntry>();

    public IEnumerator<BatchInsertResponseEntry> GetEnumerator()
    {
        return Objects.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<WeaviateException> Errors =>
        Objects.Where(o => o.Error is not null).Select(o => o.Error!);

    public bool HasErrors => Errors.Any();

    public int Count => Objects.Count();
}

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

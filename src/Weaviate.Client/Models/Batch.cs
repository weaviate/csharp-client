using System.Collections;

namespace Weaviate.Client.Models;

/// <summary>
/// Represents a request to insert an object as part of a batch operation.
/// </summary>
/// <param name="Data">The object data to insert. Can be a strongly-typed object or anonymous type.</param>
/// <param name="ID">Optional ID for the object. If not provided, Weaviate will generate one.</param>
/// <param name="Vectors">Optional vector data for the object. Used when providing custom vectors.</param>
/// <param name="References">Optional cross-references to other objects.</param>
/// <remarks>
/// Use the static <see cref="Create(object, Guid?, Vectors?, IEnumerable{ObjectReference}?)"/> methods
/// to create batch insert requests from various data formats.
/// </remarks>
public record BatchInsertRequest(
    object Data,
    Guid? ID = null,
    Vectors? Vectors = null,
    IEnumerable<ObjectReference>? References = null
)
{
    public static BatchInsertRequest Create(
        object data,
        Guid? id = null,
        Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null
    )
    {
        return new BatchInsertRequest(data, id, vectors, references);
    }

    public static BatchInsertRequest[] Create(IEnumerable<object> data)
    {
        return data.Select(d => new BatchInsertRequest(d)).ToArray();
    }

    public static BatchInsertRequest[] Create(IEnumerable<(object data, Guid id)> requests)
    {
        return requests.Select(r => new BatchInsertRequest(r.data, r.id)).ToArray();
    }

    public static BatchInsertRequest[] Create(
        IEnumerable<(object data, IEnumerable<ObjectReference>? references)> requests
    )
    {
        return requests
            .Select(r => new BatchInsertRequest(r.data, References: r.references))
            .ToArray();
    }

    public static BatchInsertRequest[] Create(IEnumerable<(object data, Vectors vectors)> requests)
    {
        return requests.Select(r => new BatchInsertRequest(r.data, Vectors: r.vectors)).ToArray();
    }
}

/// <summary>
/// Represents the result of a single object insertion in a batch operation.
/// </summary>
/// <param name="Index">The index of this object in the batch request.</param>
/// <param name="ID">The ID of the inserted object, if successful.</param>
/// <param name="Error">The error that occurred during insertion, if any.</param>
public record BatchInsertResponseEntry(int Index, Guid? ID = null, WeaviateException? Error = null);

/// <summary>
/// Represents the response from a batch insert operation, containing results for all inserted objects.
/// </summary>
/// <remarks>
/// This class is enumerable and provides access to individual <see cref="BatchInsertResponseEntry"/> instances.
/// Use <see cref="HasErrors"/> to check if any insertions failed, and <see cref="Errors"/> to retrieve error details.
/// </remarks>
public record BatchInsertResponse : IEnumerable<BatchInsertResponseEntry>
{
    internal BatchInsertResponse(List<BatchInsertResponseEntry> entries)
    {
        Objects = entries;
    }

    /// <summary>
    /// Gets the collection of insertion results for all objects in the batch.
    /// </summary>
    public IEnumerable<BatchInsertResponseEntry> Objects { get; internal set; } =
        new List<BatchInsertResponseEntry>();

    /// <summary>
    /// Returns an enumerator that iterates through the batch insertion results.
    /// </summary>
    /// <returns>An enumerator for the results.</returns>
    public IEnumerator<BatchInsertResponseEntry> GetEnumerator()
    {
        return Objects.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets all errors that occurred during the batch insertion.
    /// </summary>
    public IEnumerable<WeaviateException> Errors =>
        Objects.Where(o => o.Error is not null).Select(o => o.Error!);

    /// <summary>
    /// Gets a value indicating whether any errors occurred during the batch insertion.
    /// </summary>
    public bool HasErrors => Errors.Any();

    /// <summary>
    /// Gets the total number of objects in the batch response.
    /// </summary>
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

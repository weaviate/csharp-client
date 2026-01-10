using System.Collections;

namespace Weaviate.Client.Models;

/// <summary>
/// The batch insert request
/// </summary>
public record BatchInsertRequest(
    object Data,
    Guid? UUID = null,
    Vectors? Vectors = null,
    IEnumerable<ObjectReference>? References = null
)
{
    /// <summary>
    /// Creates the data
    /// </summary>
    /// <param name="data">The data</param>
    /// <param name="uuid">The uuid</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="references">The references</param>
    /// <returns>The batch insert request</returns>
    public static BatchInsertRequest Create(
        object data,
        Guid? uuid = null,
        Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null
    )
    {
        return new BatchInsertRequest(data, uuid, vectors, references);
    }

    /// <summary>
    /// Creates the data
    /// </summary>
    /// <param name="data">The data</param>
    /// <returns>The batch insert request array</returns>
    public static BatchInsertRequest[] Create(IEnumerable<object> data)
    {
        return data.Select(d => new BatchInsertRequest(d)).ToArray();
    }

    /// <summary>
    /// Creates the requests
    /// </summary>
    /// <param name="requests">The requests</param>
    /// <returns>The batch insert request array</returns>
    public static BatchInsertRequest[] Create(IEnumerable<(object data, Guid uuid)> requests)
    {
        return requests.Select(r => new BatchInsertRequest(r.data, r.uuid)).ToArray();
    }

    /// <summary>
    /// Creates the requests
    /// </summary>
    /// <param name="requests">The requests</param>
    /// <returns>The batch insert request array</returns>
    public static BatchInsertRequest[] Create(
        IEnumerable<(object data, IEnumerable<ObjectReference>? references)> requests
    )
    {
        return requests
            .Select(r => new BatchInsertRequest(r.data, References: r.references))
            .ToArray();
    }

    /// <summary>
    /// Creates the requests
    /// </summary>
    /// <param name="requests">The requests</param>
    /// <returns>The batch insert request array</returns>
    public static BatchInsertRequest[] Create(IEnumerable<(object data, Vectors vectors)> requests)
    {
        return requests.Select(r => new BatchInsertRequest(r.data, Vectors: r.vectors)).ToArray();
    }
}

/// <summary>
/// The batch insert response entry
/// </summary>
public record BatchInsertResponseEntry(
    int Index,
    Guid? UUID = null,
    WeaviateException? Error = null
);

/// <summary>
/// The batch insert response
/// </summary>
public record BatchInsertResponse : IEnumerable<BatchInsertResponseEntry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchInsertResponse"/> class
    /// </summary>
    /// <param name="entries">The entries</param>
    internal BatchInsertResponse(List<BatchInsertResponseEntry> entries)
    {
        Objects = entries;
    }

    /// <summary>
    /// Gets or sets the value of the objects
    /// </summary>
    public IEnumerable<BatchInsertResponseEntry> Objects { get; internal set; } =
        new List<BatchInsertResponseEntry>();

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of batch insert response entry</returns>
    public IEnumerator<BatchInsertResponseEntry> GetEnumerator()
    {
        return Objects.GetEnumerator();
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets the value of the errors
    /// </summary>
    public IEnumerable<WeaviateException> Errors =>
        Objects.Where(o => o.Error is not null).Select(o => o.Error!);

    /// <summary>
    /// Gets the value of the has errors
    /// </summary>
    public bool HasErrors => Errors.Any();

    /// <summary>
    /// Gets the value of the count
    /// </summary>
    public int Count => Objects.Count();
}

/// <summary>
/// The delete many result class
/// </summary>
public class DeleteManyResult
{
    /// <summary>
    /// Gets or sets the value of the failed
    /// </summary>
    public long Failed { get; set; }

    /// <summary>
    /// Gets or sets the value of the matches
    /// </summary>
    public long Matches { get; set; }

    /// <summary>
    /// Gets or sets the value of the successful
    /// </summary>
    public long Successful { get; set; }

    /// <summary>
    /// Gets or sets the value of the objects
    /// </summary>
    public IEnumerable<DeleteManyObjectResult> Objects { get; set; } = [];
}

/// <summary>
/// The delete many object result class
/// </summary>
public class DeleteManyObjectResult
{
    /// <summary>
    /// Gets or sets the value of the uuid
    /// </summary>
    public Guid Uuid { get; set; }

    /// <summary>
    /// Gets or sets the value of the successful
    /// </summary>
    public bool Successful { get; set; }

    /// <summary>
    /// Gets or sets the value of the error
    /// </summary>
    public string? Error { get; set; }
}

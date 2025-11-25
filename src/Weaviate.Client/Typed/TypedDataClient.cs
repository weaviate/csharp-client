using System.Collections;
using Weaviate.Client.Models;

namespace Weaviate.Client.Typed;

/// <summary>
/// Strongly-typed wrapper for DataClient that provides type-safe CRUD operations.
/// All data parameters accept T instead of object, and are automatically serialized.
/// </summary>
/// <typeparam name="T">The C# type representing objects in this collection.</typeparam>
public class TypedDataClient<T>
    where T : class, new()
{
    private readonly DataClient _dataClient;

    internal TypedDataClient(DataClient dataClient)
    {
        _dataClient = dataClient;
    }

    /// <summary>
    /// Inserts a strongly-typed object into the collection.
    /// </summary>
    public async Task<Guid> Insert(
        T data,
        Guid? id = null,
        Models.Vectors? vectors = null,
        OneOrManyOf<ObjectReference>? references = null,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.Insert(
            data,
            id,
            vectors,
            references,
            tenant,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Replaces a strongly-typed object in the collection.
    /// </summary>
    public async Task Replace(
        Guid id,
        T data,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        await _dataClient.Replace(id, data, vectors, references, tenant, cancellationToken);
    }

    /// <summary>
    /// Inserts multiple strongly-typed objects in a batch operation.
    /// </summary>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<T> data,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.InsertMany(
            (IEnumerable)data,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Inserts multiple strongly-typed objects with explicit IDs.
    /// </summary>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<(T data, Guid id)> requests,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.InsertMany(
            requests.Select(r => ((object)r.data, r.id)),
            cancellationToken
        );
    }

    /// <summary>
    /// Inserts multiple strongly-typed objects with vectors.
    /// </summary>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<(T data, Models.Vectors vectors)> requests,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.InsertMany(
            requests.Select(r => ((object)r.data, r.vectors)),
            cancellationToken
        );
    }

    /// <summary>
    /// Inserts multiple strongly-typed objects with references.
    /// </summary>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<(T data, IEnumerable<ObjectReference>? references)> requests,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.InsertMany(
            requests.Select(r => ((object)r.data, r.references)),
            cancellationToken
        );
    }

    /// <summary>
    /// Inserts multiple batches of strongly-typed objects.
    /// </summary>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<BatchInsertRequest[]> requestBatches,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.InsertMany(requestBatches, cancellationToken);
    }

    /// <summary>
    /// Inserts multiple strongly-typed objects using BatchInsertRequest.
    /// </summary>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<BatchInsertRequest> requests,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.InsertMany(requests, cancellationToken);
    }

    /// <summary>
    /// Deletes an object by its ID.
    /// </summary>
    public async Task DeleteByID(Guid id, CancellationToken cancellationToken = default)
    {
        await _dataClient.DeleteByID(id, cancellationToken);
    }

    /// <summary>
    /// Deletes multiple objects matching a filter.
    /// </summary>
    public async Task<DeleteManyResult> DeleteMany(
        Filter where,
        bool dryRun = false,
        bool verbose = false,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.DeleteMany(where, dryRun, verbose, tenant, cancellationToken);
    }

    /// <summary>
    /// Adds a reference between objects.
    /// </summary>
    public async Task ReferenceAdd(
        DataReference reference,
        CancellationToken cancellationToken = default
    )
    {
        await _dataClient.ReferenceAdd(reference, cancellationToken);
    }

    /// <summary>
    /// Adds a reference between objects.
    /// </summary>
    public async Task ReferenceAdd(
        Guid from,
        string fromProperty,
        Guid to,
        CancellationToken cancellationToken = default
    )
    {
        await _dataClient.ReferenceAdd(from, fromProperty, to, cancellationToken);
    }

    /// <summary>
    /// Adds multiple references in a batch operation.
    /// </summary>
    public async Task<BatchReferenceReturn> ReferenceAddMany(
        DataReference[] references,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.ReferenceAddMany(references, cancellationToken);
    }

    /// <summary>
    /// Replaces all references for a property with new references.
    /// </summary>
    public async Task ReferenceReplace(
        Guid from,
        string fromProperty,
        Guid[] to,
        CancellationToken cancellationToken = default
    )
    {
        await _dataClient.ReferenceReplace(from, fromProperty, to, cancellationToken);
    }

    /// <summary>
    /// Deletes a reference between objects.
    /// </summary>
    public async Task ReferenceDelete(
        Guid from,
        string fromProperty,
        Guid to,
        CancellationToken cancellationToken = default
    )
    {
        await _dataClient.ReferenceDelete(from, fromProperty, to, cancellationToken);
    }
}

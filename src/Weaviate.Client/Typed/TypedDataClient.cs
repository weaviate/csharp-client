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
        ArgumentNullException.ThrowIfNull(dataClient);

        _dataClient = dataClient;
    }

    /// <summary>
    /// Inserts a strongly-typed object into the collection.
    /// </summary>
    public async Task<Guid> Insert(
        T properties,
        Guid? uuid = null,
        Models.Vectors? vectors = null,
        OneOrManyOf<ObjectReference>? references = null,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.Insert(
            properties,
            uuid,
            vectors,
            references,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Updates a strongly-typed object in the collection.
    /// </summary>
    public async Task Update(
        Guid uuid,
        T properties,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        CancellationToken cancellationToken = default
    )
    {
        await _dataClient.Update(uuid, properties, vectors, references, cancellationToken);
    }

    /// <summary>
    /// Replaces a strongly-typed object in the collection.
    /// </summary>
    public async Task Replace(
        Guid uuid,
        T properties,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        CancellationToken cancellationToken = default
    )
    {
        await _dataClient.Replace(uuid, properties, vectors, references, cancellationToken);
    }

    /// <summary>
    /// Inserts multiple strongly-typed objects in a batch operation.
    /// </summary>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<T> properties,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.InsertMany(
            (IEnumerable)properties,
            cancellationToken: cancellationToken
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
    public async Task DeleteByID(Guid uuid, CancellationToken cancellationToken = default)
    {
        await _dataClient.DeleteByID(uuid, cancellationToken);
    }

    /// <summary>
    /// Deletes multiple objects matching a filter.
    /// </summary>
    public async Task<DeleteManyResult> DeleteMany(
        Filter where,
        bool dryRun = false,
        bool verbose = false,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataClient.DeleteMany(where, dryRun, verbose, cancellationToken);
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

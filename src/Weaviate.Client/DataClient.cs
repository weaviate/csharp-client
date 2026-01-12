using System.Collections;
using System.Collections.Frozen;
using Weaviate.Client.Internal;
using Weaviate.Client.Models;
using Weaviate.Client.Rest.Dto;
using Weaviate.Client.Validation;

namespace Weaviate.Client;

/// <summary>
/// The data client class
/// </summary>
public class DataClient
{
    /// <summary>
    /// The collection client
    /// </summary>
    private readonly CollectionClient _collectionClient;

    /// <summary>
    /// Gets the value of the  client
    /// </summary>
    private WeaviateClient _client => _collectionClient.Client;

    /// <summary>
    /// Gets the value of the  collectionname
    /// </summary>
    private string _collectionName => _collectionClient.Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataClient"/> class
    /// </summary>
    /// <param name="collectionClient">The collection client</param>
    internal DataClient(CollectionClient collectionClient)
    {
        _collectionClient = collectionClient;
    }

    /// <summary>
    /// Creates a cancellation token with data-specific timeout configuration.
    /// Uses InsertTimeout if configured, falls back to DefaultTimeout, then to WeaviateDefaults.InsertTimeout.
    /// </summary>
    private CancellationToken CreateTimeoutCancellationToken(CancellationToken userToken = default)
    {
        var effectiveTimeout =
            _client.Configuration.InsertTimeout
            ?? _client.Configuration.DefaultTimeout
            ?? WeaviateDefaults.InsertTimeout;
        return TimeoutHelper.GetCancellationToken(effectiveTimeout, userToken);
    }

    /// <summary>
    /// Vectorses the to dto using the specified vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <returns>The result</returns>
    public IDictionary<string, object>? VectorsToDto(Models.Vectors? vectors)
    {
        if (vectors == null || vectors.Count == 0)
            return null;

        var result = new Dictionary<string, object>();

        foreach (var vector in vectors)
        {
            result[vector.Key] = vector.Value;
        }

        return result;
    }

    /// <summary>
    /// Inserts the properties
    /// </summary>
    /// <param name="properties">The properties</param>
    /// <param name="uuid">The uuid</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="references">The references</param>
    /// <param name="validate">The validate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>A task containing the guid</returns>
    public async Task<Guid> Insert(
        object properties,
        Guid? uuid = null,
        Models.Vectors? vectors = null,
        AutoArray<ObjectReference>? references = null,
        bool validate = false,
        CancellationToken cancellationToken = default
    )
    {
        if (validate)
        {
            var schema = await _collectionClient.Config.GetCachedConfig(
                cancellationToken: cancellationToken
            );
            var validationResult = TypeValidator.Default.ValidateType(
                properties.GetType(),
                schema!
            );
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Object of type '{properties.GetType().Name}' does not conform to schema of collection '{_collectionName}':\n"
                        + validationResult.GetDetailedMessage()
                );
            }
        }
        var propDict = Internal.ObjectHelper.BuildDataTransferObject(properties);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = Internal.ObjectHelper.MakeBeacons(kvp.TargetID);
        }

        var dto = new Rest.Dto.Object()
        {
            Id = uuid ?? Guid.NewGuid(),
            Class = _collectionName,
            Properties = propDict,
            Vectors = VectorsToDto(vectors),
            Tenant = _collectionClient.Tenant,
        };

        var response = await _client.RestClient.ObjectInsert(
            dto,
            CreateTimeoutCancellationToken(cancellationToken)
        );

        return response.Id!.Value;
    }

    /// <summary>
    /// Updates the uuid
    /// </summary>
    /// <param name="uuid">The uuid</param>
    /// <param name="properties">The properties</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="references">The references</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task Update(
        Guid uuid,
        object properties,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        CancellationToken cancellationToken = default
    )
    {
        var propDict = Internal.ObjectHelper.BuildDataTransferObject(properties);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = Internal.ObjectHelper.MakeBeacons(kvp.TargetID);
        }

        var dto = new Rest.Dto.Object()
        {
            Id = uuid,
            Class = _collectionName,
            Properties = propDict,
            Vectors = VectorsToDto(vectors),
            Tenant = _collectionClient.Tenant,
        };

        await _client.RestClient.ObjectUpdate(
            _collectionName,
            dto,
            CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// Replaces the uuid
    /// </summary>
    /// <param name="uuid">The uuid</param>
    /// <param name="properties">The properties</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="references">The references</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task Replace(
        Guid uuid,
        object properties,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        CancellationToken cancellationToken = default
    )
    {
        var propDict = Internal.ObjectHelper.BuildDataTransferObject(properties);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = Internal.ObjectHelper.MakeBeacons(kvp.TargetID);
        }

        var dto = new Rest.Dto.Object()
        {
            Id = uuid,
            Class = _collectionName,
            Properties = propDict,
            Vectors = VectorsToDto(vectors),
            Tenant = _collectionClient.Tenant,
        };

        await _client.RestClient.ObjectReplace(
            _collectionName,
            dto,
            CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// Inserts the many using the specified properties
    /// </summary>
    /// <param name="properties">The properties</param>
    /// <param name="validate">The validate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>A task containing the batch insert response</returns>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable properties,
        bool validate = false,
        CancellationToken cancellationToken = default
    )
    {
        var objects = properties.Cast<object>().ToList();
        if (validate)
        {
            var schema = await _collectionClient.Config.GetCachedConfig(
                cancellationToken: cancellationToken
            );
            foreach (var obj in objects)
            {
                var validationResult = TypeValidator.Default.ValidateType(obj.GetType(), schema!);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException(
                        $"Object of type '{obj.GetType().Name}' does not conform to schema of collection '{_collectionName}':\n"
                            + validationResult.GetDetailedMessage()
                    );
                }
            }
        }
        return await InsertMany(
            objects.Select(r => BatchInsertRequest.Create(r)),
            cancellationToken
        );
    }

    /// <summary>
    /// Inserts the many using the specified request batches
    /// </summary>
    /// <param name="requestBatches">The request batches</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the batch insert response</returns>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<BatchInsertRequest[]> requestBatches,
        CancellationToken cancellationToken = default
    )
    {
        var results = new List<BatchInsertResponseEntry>();

        foreach (var batch in requestBatches)
        {
            var batchResults = await InsertMany(batch, cancellationToken);
            results.AddRange(batchResults);
        }

        return new BatchInsertResponse(results);
    }

    /// <summary>
    /// Inserts the many using the specified requests
    /// </summary>
    /// <param name="requests">The requests</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the batch insert response</returns>
    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<BatchInsertRequest> requests,
        CancellationToken cancellationToken = default
    )
    {
        var objects = requests
            .Select(
                (r, idx) =>
                {
                    var o = new Grpc.Protobuf.V1.BatchObject
                    {
                        Collection = _collectionName,
                        Uuid = (r.UUID ?? Guid.NewGuid()).ToString(),
                        Properties = Internal.ObjectHelper.BuildBatchProperties(r.Data),
                        Tenant = _collectionClient.Tenant,
                    };

                    if (r.References?.Any() ?? false)
                    {
                        foreach (var reference in r.References!)
                        {
                            var strp = new Grpc.Protobuf.V1.BatchObject.Types.SingleTargetRefProps()
                            {
                                PropName = reference.Name,
                                Uuids = { reference.TargetID.Select(id => id.ToString()) },
                            };

                            o.Properties.SingleTargetRefProps.Add(strp);
                        }
                    }

                    if (r.Vectors != null)
                    {
                        o.Vectors.AddRange(
                            r.Vectors.Select(kvp =>
                            {
                                var v = kvp.Value;
                                return new Grpc.Protobuf.V1.Vectors
                                {
                                    Name = kvp.Key,
                                    VectorBytes = v.ToByteString(),
                                    Type = v.IsMultiVector
                                        ? Grpc.Protobuf.V1.Vectors.Types.VectorType.MultiFp32
                                        : Grpc.Protobuf.V1.Vectors.Types.VectorType.SingleFp32,
                                };
                            })
                        );
                    }

                    return new { Index = idx, BatchObject = o };
                }
            )
            .ToList();

        var inserts = await _client.GrpcClient.InsertMany(
            objects.Select(o => o.BatchObject),
            CreateTimeoutCancellationToken(cancellationToken)
        );

        var dictErr = inserts.Errors.ToFrozenDictionary(kv => kv.Index, kv => kv.Error);
        var dictUuid = objects
            .Select(o => new { o.Index, o.BatchObject.Uuid })
            .Where(o => !dictErr.ContainsKey(o.Index))
            .ToDictionary(kv => kv.Index, kv => Guid.Parse(kv.Uuid));

        var results = new List<BatchInsertResponseEntry>();

        foreach (int r in Enumerable.Range(0, objects.Count()))
        {
            results.Add(
                new BatchInsertResponseEntry(
                    Index: r,
                    dictUuid.TryGetValue(r, out Guid uuid) ? uuid : (Guid?)null,
                    dictErr.TryGetValue(r, out string? error)
                        ? new WeaviateClientException(error)
                        : null
                )
            );
        }

        return new BatchInsertResponse(results);
    }

    /// <summary>
    /// Deletes the by id using the specified uuid
    /// </summary>
    /// <param name="uuid">The uuid</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task DeleteByID(Guid uuid, CancellationToken cancellationToken = default)
    {
        await _client.RestClient.DeleteObject(
            _collectionName,
            uuid,
            _collectionClient.Tenant,
            CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// References the add using the specified reference
    /// </summary>
    /// <param name="reference">The reference</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task ReferenceAdd(
        DataReference reference,
        CancellationToken cancellationToken = default
    )
    {
        await _client.RestClient.ReferenceAdd(
            _collectionName,
            reference.From,
            reference.FromProperty,
            reference.To.Single(),
            _collectionClient.Tenant,
            CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// References the add using the specified from
    /// </summary>
    /// <param name="from">The from</param>
    /// <param name="fromProperty">The from property</param>
    /// <param name="to">The to</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task ReferenceAdd(
        Guid from,
        string fromProperty,
        Guid to,
        CancellationToken cancellationToken = default
    )
    {
        await _client.RestClient.ReferenceAdd(
            _collectionName,
            from,
            fromProperty,
            to,
            _collectionClient.Tenant,
            CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// References the add many using the specified references
    /// </summary>
    /// <param name="references">The references</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the batch reference return</returns>
    public async Task<BatchReferenceReturn> ReferenceAddMany(
        DataReference[] references,
        CancellationToken cancellationToken = default
    )
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var result = await _client.RestClient.ReferenceAddMany(
            _collectionName,
            references,
            _collectionClient.Tenant,
            _collectionClient.ConsistencyLevel,
            CreateTimeoutCancellationToken(cancellationToken)
        );

        stopwatch.Stop();
        var elapsedSeconds = (float)stopwatch.Elapsed.TotalSeconds;

        var errorsByIndex = result
            .Select((r, idx) => new { r.Result, Index = idx })
            .Where(r => (r.Result?.Status ?? ResultStatus.SUCCESS) == ResultStatus.FAILED)
            .ToDictionary(
                entry => entry.Index,
                entry =>
                {
                    var errors = entry.Result?.Errors?.Error ?? Enumerable.Empty<Error>();

                    return errors
                        .Where(e => e.Message is not null)
                        .Select(e => new WeaviateServerException(e.Message!))
                        .Cast<WeaviateException>()
                        .ToArray();
                }
            );

        return new BatchReferenceReturn(elapsedSeconds, errorsByIndex);
    }

    /// <summary>
    /// References the replace using the specified from
    /// </summary>
    /// <param name="from">The from</param>
    /// <param name="fromProperty">The from property</param>
    /// <param name="to">The to</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task ReferenceReplace(
        Guid from,
        string fromProperty,
        Guid[] to,
        CancellationToken cancellationToken = default
    )
    {
        await _client.RestClient.ReferenceReplace(
            _collectionName,
            from,
            fromProperty,
            to,
            _collectionClient.Tenant,
            CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// References the delete using the specified from
    /// </summary>
    /// <param name="from">The from</param>
    /// <param name="fromProperty">The from property</param>
    /// <param name="to">The to</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task ReferenceDelete(
        Guid from,
        string fromProperty,
        Guid to,
        CancellationToken cancellationToken = default
    )
    {
        await _client.RestClient.ReferenceDelete(
            _collectionName,
            from,
            fromProperty,
            to,
            _collectionClient.Tenant,
            CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// Deletes the many using the specified where
    /// </summary>
    /// <param name="where">The where</param>
    /// <param name="dryRun">The dry run</param>
    /// <param name="verbose">The verbose</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The result</returns>
    public async Task<DeleteManyResult> DeleteMany(
        Filter where,
        bool dryRun = false,
        bool verbose = false,
        CancellationToken cancellationToken = default
    )
    {
        var reply = await _client.GrpcClient.DeleteMany(
            _collectionName,
            where,
            dryRun,
            verbose,
            _collectionClient.Tenant,
            _collectionClient.ConsistencyLevel,
            CreateTimeoutCancellationToken(cancellationToken)
        );

        var result = new DeleteManyResult
        {
            Failed = reply.Failed,
            Matches = reply.Matches,
            Successful = reply.Successful,
            Objects = reply.Objects.Select(o => new DeleteManyObjectResult
            {
                Error = string.IsNullOrEmpty(o.Error) ? null : o.Error,
                Successful = o.Successful,
                Uuid = Internal.ObjectHelper.GuidFromByteString(o.Uuid),
            }),
        };

        return result;
    }
}

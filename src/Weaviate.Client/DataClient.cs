using System.Collections;
using System.Collections.Frozen;
using Weaviate.Client.Models;
using Weaviate.Client.Rest.Dto;
using Weaviate.Client.Validation;

namespace Weaviate.Client;

public class DataClient
{
    private readonly CollectionClient _collectionClient;

    private WeaviateClient _client => _collectionClient.Client;
    private string _collectionName => _collectionClient.Name;

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
            _client.InsertTimeout ?? _client.DefaultTimeout ?? WeaviateDefaults.InsertTimeout;
        return TimeoutHelper.GetCancellationToken(effectiveTimeout, userToken);
    }

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

    public async Task<Guid> Insert(
        object data,
        Guid? id = null,
        Models.Vectors? vectors = null,
        OneOrManyOf<ObjectReference>? references = null,
        bool validate = false,
        CancellationToken cancellationToken = default
    )
    {
        if (validate)
        {
            var schema = await _collectionClient.Config.GetCachedConfig();
            var validationResult = TypeValidator.Default.ValidateType(data.GetType(), schema!);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Object of type '{data.GetType().Name}' does not conform to schema of collection '{_collectionName}':\n"
                        + validationResult.GetDetailedMessage()
                );
            }
        }
        var propDict = ObjectHelper.BuildDataTransferObject(data);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = ObjectHelper.MakeBeacons(kvp.TargetID);
        }

        var dto = new Rest.Dto.Object()
        {
            Id = id ?? Guid.NewGuid(),
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

    public async Task Update(
        Guid id,
        object data,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        CancellationToken cancellationToken = default
    )
    {
        var propDict = ObjectHelper.BuildDataTransferObject(data);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = ObjectHelper.MakeBeacons(kvp.TargetID);
        }

        var dto = new Rest.Dto.Object()
        {
            Id = id,
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

    public async Task Replace(
        Guid id,
        object data,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        CancellationToken cancellationToken = default
    )
    {
        var propDict = ObjectHelper.BuildDataTransferObject(data);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = ObjectHelper.MakeBeacons(kvp.TargetID);
        }

        var dto = new Rest.Dto.Object()
        {
            Id = id,
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

    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable data,
        bool validate = false,
        CancellationToken cancellationToken = default
    )
    {
        var objects = data.Cast<object>().ToList();
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

    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<(object, Guid id)> requests,
        CancellationToken cancellationToken = default
    ) => await InsertMany(BatchInsertRequest.Create(requests), cancellationToken);

    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<(object, Models.Vectors vectors)> requests,
        CancellationToken cancellationToken = default
    ) => await InsertMany(BatchInsertRequest.Create(requests), cancellationToken);

    public async Task<BatchInsertResponse> InsertMany(
        IEnumerable<(object data, IEnumerable<ObjectReference>? references)> requests,
        CancellationToken cancellationToken = default
    ) => await InsertMany(BatchInsertRequest.Create(requests), cancellationToken);

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
                        Uuid = (r.ID ?? Guid.NewGuid()).ToString(),
                        Properties = ObjectHelper.BuildBatchProperties(r.Data),
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
                            r.Vectors.Select(v => new Grpc.Protobuf.V1.Vectors
                            {
                                Name = v.Key,
                                VectorBytes = v.Value.ToByteString(),
                                Type = v.Value.IsMultiVector
                                    ? Grpc.Protobuf.V1.Vectors.Types.VectorType.MultiFp32
                                    : Grpc.Protobuf.V1.Vectors.Types.VectorType.SingleFp32,
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

    public async Task DeleteByID(Guid id, CancellationToken cancellationToken = default)
    {
        await _client.RestClient.DeleteObject(
            _collectionName,
            id,
            _collectionClient.Tenant,
            CreateTimeoutCancellationToken(cancellationToken)
        );
    }

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
                Uuid = ObjectHelper.GuidFromByteString(o.Uuid),
            }),
        };

        return result;
    }
}

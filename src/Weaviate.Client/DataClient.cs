using System.Collections.Frozen;
using System.Diagnostics;
using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client;

public class DataClient<TData>
{
    private readonly CollectionClient<TData> _collectionClient;

    private WeaviateClient _client => _collectionClient.Client;
    private string _collectionName => _collectionClient.Name;

    internal DataClient(CollectionClient<TData> collectionClient)
    {
        _collectionClient = collectionClient;
    }

    public async Task<Guid> Insert(
        TData data,
        Guid? id = null,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    )
    {
        var propDict = ObjectHelper.BuildDataTransferObject(data);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = ObjectHelper.MakeBeacons(kvp.TargetID);
        }

        var dtoVectors =
            vectors?.Count == 0
                ? null
                : Rest.Dto.Vectors.FromJson(JsonSerializer.Serialize(vectors ?? []));

        var dto = new Rest.Dto.Object()
        {
            Id = id ?? Guid.NewGuid(),
            Class = _collectionName,
            Properties = propDict,
            Vectors = dtoVectors,
            Tenant = tenant ?? _collectionClient.Tenant,
        };

        var response = await _client.RestClient.ObjectInsert(dto);

        return response.Id!.Value;
    }

    public async Task Replace(
        Guid id,
        TData data,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    )
    {
        var propDict = ObjectHelper.BuildDataTransferObject(data);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = ObjectHelper.MakeBeacons(kvp.TargetID);
        }

        var dtoVectors =
            vectors?.Count == 0
                ? null
                : Rest.Dto.Vectors.FromJson(JsonSerializer.Serialize(vectors ?? []));

        var dto = new Rest.Dto.Object()
        {
            Id = id,
            Class = _collectionName,
            Properties = propDict,
            Vectors = dtoVectors,
            Tenant = tenant ?? _collectionClient.Tenant,
        };

        var response = await _client.RestClient.ObjectReplace(_collectionName, dto);
    }

    public delegate void InsertDelegate(
        TData data,
        Guid? id = null,
        Models.Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    );

    public async Task<IEnumerable<BatchInsertResponse>> InsertMany(params TData[] data)
    {
        return await InsertMany(add =>
        {
            foreach (var o in data)
            {
                add(o);
            }
        });
    }

    public async Task<IEnumerable<BatchInsertResponse>> InsertMany(
        params BatchInsertRequest<TData>[] requests
    )
    {
        var objects = requests
            .Select(
                (r, idx) =>
                {
                    var o = new V1.BatchObject
                    {
                        Collection = _collectionName,
                        Uuid = (r.ID ?? Guid.NewGuid()).ToString(),
                        Properties = ObjectHelper.BuildBatchProperties(r.Data),
                        Tenant = r.Tenant ?? _collectionClient.Tenant,
                    };

                    if (r.References?.Any() ?? false)
                    {
                        foreach (var reference in r.References!)
                        {
                            var strp = new Weaviate.V1.BatchObject.Types.SingleTargetRefProps()
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
                            r.Vectors.Select(v => new V1.Vectors
                            {
                                Name = v.Key,
                                VectorBytes = v.Value.ToByteString(),
                                Type = typeof(System.Collections.IEnumerable).IsAssignableFrom(
                                    v.Value.ValueType
                                )
                                    ? V1.Vectors.Types.VectorType.MultiFp32
                                    : V1.Vectors.Types.VectorType.SingleFp32,
                            })
                        );
                    }

                    return new { Index = idx, BatchObject = o };
                }
            )
            .ToList();

        var inserts = await _client.GrpcClient.InsertMany(objects.Select(o => o.BatchObject));

        var dictErr = inserts.Errors.ToFrozenDictionary(kv => kv.Index, kv => kv.Error);
        var dictUuid = objects
            .Select(o => new { o.Index, o.BatchObject.Uuid })
            .Where(o => !dictErr.ContainsKey(o.Index))
            .ToDictionary(kv => kv.Index, kv => Guid.Parse(kv.Uuid));

        var results = new List<BatchInsertResponse>();

        foreach (int r in Enumerable.Range(0, objects.Count))
        {
            results.Add(
                new BatchInsertResponse(
                    Index: r,
                    dictUuid.TryGetValue(r, out Guid uuid) ? uuid : (Guid?)null,
                    dictErr.TryGetValue(r, out string? error) ? new WeaviateException(error) : null
                )
            );
        }

        return results;
    }

    public async Task<IEnumerable<BatchInsertResponse>> InsertMany(
        params Action<InsertDelegate>[] inserterList
    )
    {
        var responses = new List<BatchInsertResponse>();

        foreach (var inserter in inserterList)
        {
            IList<BatchInsertRequest<TData>> requests = [];

            InsertDelegate _inserter = (
                TData data,
                Guid? id = null,
                Models.Vectors? vectors = null,
                IEnumerable<ObjectReference>? references = null,
                string? tenant = null
            ) =>
            {
                requests.Add(
                    new BatchInsertRequest<TData>(
                        data,
                        id,
                        vectors,
                        references,
                        tenant ?? _collectionClient.Tenant
                    )
                );
            };

            inserter(_inserter);

            var response = await InsertMany([.. requests]);

            responses.AddRange(
                [
                    .. response.Select(r => new BatchInsertResponse(
                        r.Index + responses.Count,
                        r.ID,
                        r.Error
                    )),
                ]
            );
        }

        return responses;
    }

    public async Task Delete(Guid id)
    {
        await _client.RestClient.DeleteObject(_collectionName, id, _collectionClient.Tenant);
    }

    public async Task ReferenceAdd(DataReference reference)
    {
        await _client.RestClient.ReferenceAdd(
            _collectionName,
            reference.From,
            reference.FromProperty,
            reference.To.Single(),
            _collectionClient.Tenant
        );
    }

    public async Task ReferenceAdd(Guid from, string fromProperty, Guid to)
    {
        await _client.RestClient.ReferenceAdd(
            _collectionName,
            from,
            fromProperty,
            to,
            _collectionClient.Tenant
        );
    }

    public async Task<BatchReferenceReturn> ReferenceAddMany(params DataReference[] references)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await _client.RestClient.ReferenceAddMany(
            _collectionName,
            references,
            _collectionClient.ConsistencyLevel
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

                    return errors.Select(e => new WeaviateException(e.Message)).ToArray();
                }
            );

        return new BatchReferenceReturn(elapsedSeconds, errorsByIndex);
    }

    public async Task ReferenceReplace(Guid from, string fromProperty, Guid[] to)
    {
        await _client.RestClient.ReferenceReplace(
            _collectionName,
            from,
            fromProperty,
            to,
            _collectionClient.Tenant
        );
    }

    public async Task ReferenceDelete(Guid from, string fromProperty, Guid to)
    {
        await _client.RestClient.ReferenceDelete(
            _collectionName,
            from,
            fromProperty,
            to,
            _collectionClient.Tenant
        );
    }

    public async Task<DeleteManyResult> DeleteMany(
        Filter where,
        bool dryRun = false,
        bool verbose = false,
        string? tenant = null
    )
    {
        var reply = await _client.GrpcClient.DeleteMany(
            _collectionName,
            where,
            dryRun,
            verbose,
            tenant
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

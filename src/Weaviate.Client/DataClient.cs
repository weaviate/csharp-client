using System.Collections.Frozen;
using System.Dynamic;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
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

    public static IDictionary<string, string>[] MakeBeacons(params Guid[] guids)
    {
        return
        [
            .. guids.Select(uuid => new Dictionary<string, string>
            {
                { "beacon", $"weaviate://localhost/{uuid}" },
            }),
        ];
    }

    internal static IDictionary<string, object?> BuildDynamicObject(object? data)
    {
        var obj = new ExpandoObject();
        var propDict = obj as IDictionary<string, object?>;

        if (data is null)
        {
            return propDict;
        }

        foreach (var propertyInfo in data.GetType().GetProperties())
        {
            if (!propertyInfo.CanRead)
                continue; // skip non-readable properties

            var value = propertyInfo.GetValue(data);

            if (value is null)
            {
                continue;
            }
            else if (propertyInfo.PropertyType.IsNativeType())
            {
                propDict[propertyInfo.Name] = value;
            }
            else
            {
                propDict[propertyInfo.Name] = BuildDynamicObject(value); // recursive call
            }
        }

        return obj;
    }

    // Helper method to convert C# objects to protobuf Values
    public static Value ConvertToValue(object obj)
    {
        return obj switch
        {
            null => Value.ForNull(),
            bool b => Value.ForBool(b),
            int i => Value.ForNumber(i),
            long l => Value.ForNumber(l),
            float f => Value.ForNumber(f),
            double d => Value.ForNumber(d),
            decimal dec => Value.ForNumber((double)dec),
            string s => Value.ForString(s),
            DateTime dt => Value.ForString(dt.ToUniversalTime().ToString("o")),
            Guid uuid => Value.ForString(uuid.ToString()),
            // Dictionary<string, object> dict => Value.ForStruct(CreateStructFromDictionary(dict)),
            // IEnumerable<object> enumerable => CreateListValue(enumerable),
            _ => throw new ArgumentException($"Unsupported type: {obj.GetType()}"),
        };
    }

    private V1.BatchObject.Types.Properties BuildBatchProperties<TProps>(TProps data)
    {
        var props = new V1.BatchObject.Types.Properties();

        if (data is null)
        {
            return props;
        }

        Google.Protobuf.WellKnownTypes.Struct? nonRefProps = null;

        foreach (var propertyInfo in data.GetType().GetProperties())
        {
            if (!propertyInfo.CanRead)
                continue; // skip non-readable properties

            var value = propertyInfo.GetValue(data);

            if (value is null)
            {
                continue;
            }

            if (propertyInfo.PropertyType.IsNativeType())
            {
                nonRefProps ??= new();

                nonRefProps.Fields.Add(propertyInfo.Name, ConvertToValue(value));
            }
        }

        props.NonRefProperties = nonRefProps;

        return props;
    }

    public async Task<Guid> Insert(
        TData data,
        Guid? id = null,
        NamedVectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    )
    {
        var propDict = BuildDynamicObject(data);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = MakeBeacons(kvp.TargetID);
        }

        var dtoVectors =
            vectors?.Count == 0 ? null : Vectors.FromJson(JsonSerializer.Serialize(vectors));

        var dto = new Rest.Dto.Object()
        {
            Id = id ?? Guid.NewGuid(),
            Class = _collectionName,
            Properties = propDict,
            Vectors = dtoVectors,
            Tenant = tenant,
        };

        var response = await _client.RestClient.ObjectInsert(_collectionName, dto);

        return response.Id!.Value;
    }

    public delegate void InsertDelegate(
        TData data,
        Guid? id = null,
        NamedVectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    );

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
                        Properties = BuildBatchProperties(r.Data),
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
                NamedVectors? vectors = null,
                IEnumerable<ObjectReference>? references = null,
                string? tenant = null
            ) =>
            {
                requests.Add(new BatchInsertRequest<TData>(data, id, vectors, references, tenant));
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
        await _client.RestClient.DeleteObject(_collectionName, id);
    }

    public async Task ReferenceAdd(Guid from, string fromProperty, Guid to)
    {
        await _client.RestClient.ReferenceAdd(_collectionName, from, fromProperty, to);
    }

    public async Task ReferenceReplace(Guid from, string fromProperty, Guid[] to)
    {
        await _client.RestClient.ReferenceReplace(_collectionName, from, fromProperty, to);
    }

    public async Task ReferenceDelete(Guid from, string fromProperty, Guid to)
    {
        await _client.RestClient.ReferenceDelete(_collectionName, from, fromProperty, to);
    }
}

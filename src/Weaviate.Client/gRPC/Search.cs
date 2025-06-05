using Google.Protobuf.Collections;
using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public partial class WeaviateGrpcClient
{
    internal SearchRequest BaseSearchRequest(
        string collection,
        Filters? filter = null,
        IEnumerable<SortBy>? sort = null,
        uint? limit = null,
        GroupByRequest? groupBy = null,
        MetadataQuery? metadata = null,
        IList<QueryReference>? reference = null,
        string[]? fields = null
    )
    {
        var metadataRequest = new MetadataRequest()
        {
            Uuid = true,
            Vector = metadata?.Vector ?? false,
            LastUpdateTimeUnix = metadata?.LastUpdateTime ?? false,
            CreationTimeUnix = metadata?.CreationTime ?? false,
            Certainty = metadata?.Certainty ?? false,
            Distance = metadata?.Distance ?? false,
            Score = metadata?.Score ?? false,
            ExplainScore = metadata?.ExplainScore ?? false,
            IsConsistent = metadata?.IsConsistent ?? false,
        };

        metadataRequest.Vectors.AddRange(metadata?.Vectors.ToArray() ?? []);

        var request = new SearchRequest()
        {
            Collection = collection,
            Filters = filter,
#pragma warning disable CS0612 // Type or member is obsolete
            Uses123Api = true,
            Uses125Api = true,
#pragma warning restore CS0612 // Type or member is obsolete
            Uses127Api = true,
            Limit = limit ?? 0,
            GroupBy = groupBy is not null
                ? new GroupBy()
                {
                    Path = { groupBy.PropertyName },
                    NumberOfGroups = Convert.ToInt32(groupBy.NumberOfGroups),
                    ObjectsPerGroup = Convert.ToInt32(groupBy.ObjectsPerGroup),
                }
                : null,
            Metadata = metadataRequest,
            Properties = MakePropsRequest(fields, reference),
        };

        if (sort is not null)
        {
            request.SortBy.AddRange(sort);
        }

        return request;
    }

    private PropertiesRequest? MakePropsRequest(string[]? fields, IList<QueryReference>? reference)
    {
        if (fields is null && reference is null)
            return null;

        var req = new PropertiesRequest();

        if (fields is not null)
        {
            req.NonRefProperties.AddRange(fields);
        }
        else
        {
            req.ReturnAllNonrefProperties = true;
        }

        foreach (var r in reference ?? [])
        {
            if (reference is not null)
            {
                req.RefProperties.Add(MakeRefPropsRequest(r));
            }
        }

        return req;
    }

    private RefPropertiesRequest? MakeRefPropsRequest(QueryReference? reference)
    {
        if (reference is null)
            return null;

        return new RefPropertiesRequest()
        {
            Metadata = new MetadataRequest()
            {
                Uuid = true,
                LastUpdateTimeUnix = reference.Metadata?.LastUpdateTime ?? false,
                CreationTimeUnix = reference.Metadata?.CreationTime ?? false,
                Certainty = reference.Metadata?.Certainty ?? false,
                Distance = reference.Metadata?.Distance ?? false,
                Score = reference.Metadata?.Score ?? false,
                ExplainScore = reference.Metadata?.ExplainScore ?? false,
                IsConsistent = reference.Metadata?.IsConsistent ?? false,
            },
            Properties = MakePropsRequest(reference.Fields, reference.References),
            ReferenceProperty = reference.LinkOn,
        };
    }

    private static Metadata BuildMetadataFromResult(MetadataResult metadata)
    {
        return new Metadata
        {
            LastUpdateTime = metadata.LastUpdateTimeUnixPresent
                ? DateTimeOffset.FromUnixTimeMilliseconds(metadata.LastUpdateTimeUnix).UtcDateTime
                : null,
            CreationTime = metadata.CreationTimeUnixPresent
                ? DateTimeOffset.FromUnixTimeMilliseconds(metadata.CreationTimeUnix).UtcDateTime
                : null,
            Certainty = metadata.CertaintyPresent ? metadata.Certainty : null,
            Distance = metadata.DistancePresent ? metadata.Distance : null,
            Score = metadata.ScorePresent ? metadata.Score : null,
            ExplainScore = metadata.ExplainScorePresent ? metadata.ExplainScore : null,
            IsConsistent = metadata.IsConsistentPresent ? metadata.IsConsistent : null,
        };
    }

    private static NamedVectors BuildVectorsFromResult(RepeatedField<Vectors> vectors)
    {
        var result = new NamedVectors();

        foreach (var vector in vectors)
        {
            var vectorData = new NamedVector(vector.VectorBytes.FromByteString<float>());
            result.Add(vector.Name, vectorData);
        }

        return result;
    }

    private static GroupByObject BuildGroupByObjectFromResult(
        string collection,
        string groupName,
        SearchResult obj
    )
    {
        var metadata = obj.Metadata;
        var properties = obj.Properties;

        return new GroupByObject(BuildObjectFromResult(collection, metadata, properties))
        {
            BelongsToGroup = groupName,
        };
    }

    private static WeaviateObject BuildObjectFromResult(
        string collection,
        MetadataResult metadata,
        PropertiesResult properties
    )
    {
        return new WeaviateObject
        {
            ID = !string.IsNullOrEmpty(metadata.Id) ? Guid.Parse(metadata.Id) : Guid.Empty,
            Collection = collection,
            Vectors = BuildVectorsFromResult(metadata.Vectors),
            Properties = MakeNonRefs(properties.NonRefProps),
            References = properties.RefPropsRequested
                ? MakeRefs(properties.RefProps)
                : new Dictionary<string, IList<WeaviateObject>>(),
            Metadata = BuildMetadataFromResult(metadata),
        };
    }

    private static WeaviateResult BuildResult(string collection, SearchReply reply)
    {
        return new WeaviateResult
        {
            Objects = reply.Results.Any()
                ? reply.Results.Select(r =>
                    BuildObjectFromResult(collection, r.Metadata, r.Properties)
                )
                : [],
        };
    }

    private static IDictionary<string, IList<WeaviateObject>> MakeRefs(
        RepeatedField<RefPropertiesResult> refProps
    )
    {
        var result = new Dictionary<string, IList<WeaviateObject>>();

        foreach (var refProp in refProps)
        {
            result[refProp.PropName] = refProp
                .Properties.Select(p => BuildObjectFromResult(p.TargetCollection, p.Metadata, p))
                .ToList();
        }

        return result;
    }

    private static Models.GroupByResult BuildGroupByResult(string collection, SearchReply reply)
    {
        if (!reply.GroupByResults.Any())
        {
            return (new List<GroupByObject>(), new Dictionary<string, WeaviateGroup>());
        }

        var groups = reply.GroupByResults.ToDictionary(
            k => k.Name,
            v => new WeaviateGroup
            {
                Name = v.Name,
                Objects = v
                    .Objects.Select(obj => BuildGroupByObjectFromResult(collection, v.Name, obj))
                    .ToArray(),
            }
        );

        var objects = groups.Values.SelectMany(g => g.Objects).ToArray();

        return (objects, groups);
    }

    private static void BuildNearText(
        string query,
        double? distance,
        double? certainty,
        SearchRequest request,
        Move? moveTo,
        Move? moveAway
    )
    {
        request.NearText = new NearTextSearch
        {
            Query = { query },
            // Targets = null,
            // VectorForTargets = { },
        };

        if (moveTo is not null)
        {
            var uuids = moveTo.Objects is null
                ? []
                : (new Guid?[] { moveTo.Objects }).Select(x => x.ToString());
            var concepts = moveTo.Concepts is null ? new string[] { } : [moveTo.Concepts];
            request.NearText.MoveTo = new NearTextSearch.Types.Move
            {
                Uuids = { uuids },
                Concepts = { concepts },
                Force = moveTo.Force,
            };
        }

        if (moveAway is not null)
        {
            var uuids = moveAway.Objects is null
                ? []
                : (new Guid?[] { moveAway.Objects }).Select(x => x.ToString());
            var concepts = moveAway.Concepts is null ? new string[] { } : [moveAway.Concepts];
            request.NearText.MoveAway = new NearTextSearch.Types.Move
            {
                Uuids = { uuids },
                Concepts = { concepts },
                Force = moveAway.Force,
            };
        }

        if (distance is not null)
        {
            request.NearText.Distance = distance.Value;
        }

        if (certainty.HasValue)
        {
            request.NearText.Certainty = certainty.Value;
        }
    }

    private static void BuildNearVector(
        float[] vector,
        float? distance,
        float? certainty,
        SearchRequest request
    )
    {
        request.NearVector = new NearVector
        {
            Vectors =
            {
                new Vectors
                {
                    Name = "default",
                    Type = Vectors.Types.VectorType.SingleFp32,
                    VectorBytes = vector.ToByteString(),
                },
            },
            // Targets = null,
            // VectorForTargets = { },
        };

        if (distance.HasValue)
        {
            request.NearVector.Distance = distance.Value;
        }

        if (certainty.HasValue)
        {
            request.NearVector.Certainty = certainty.Value;
        }
    }

    private void BuildBM25(SearchRequest request, string query, string[]? properties = null)
    {
        request.Bm25Search = new BM25() { Query = query };

        if (properties is not null)
        {
            request.Bm25Search.Properties.AddRange(properties);
        }
    }

    internal async Task<WeaviateResult> FetchObjects(
        string collection,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var req = BaseSearchRequest(
            collection,
            filter?.InternalFilter,
            sort?.Select(s => s.InternalSort),
            limit,
            fields: fields,
            metadata: metadata,
            reference: reference
        );

        SearchReply? reply = await _grpcClient.SearchAsync(req);

        return BuildResult(collection, reply);
    }

    public async Task<WeaviateResult> SearchNearVector(
        string collection,
        float[] vector,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: limit,
            fields: fields,
            metadata: metadata,
            reference: reference
        );

        BuildNearVector(vector, distance, certainty, request);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildResult(collection, reply);
    }

    internal async Task<WeaviateResult> SearchNearText(
        string collection,
        string query,
        float? distance,
        float? certainty,
        uint? limit,
        Move? moveTo,
        Move? moveAway,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: limit,
            fields: fields,
            metadata: metadata,
            reference: reference
        );

        BuildNearText(query, distance, certainty, request, moveTo, moveAway);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildResult(collection, reply);
    }

    public async Task<Models.GroupByResult> SearchNearVector(
        string collection,
        float[] vector,
        GroupByRequest groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: limit,
            groupBy: groupBy,
            fields: fields,
            metadata: metadata,
            reference: reference
        );

        BuildNearVector(vector, distance, certainty, request);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildGroupByResult(collection, reply);
    }

    internal async Task<Models.GroupByResult> SearchNearText(
        string collection,
        string query,
        GroupByRequest groupBy,
        float? distance,
        float? certainty,
        uint? limit,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: limit,
            groupBy: groupBy,
            fields: fields,
            metadata: metadata,
            reference: reference
        );

        BuildNearText(query, distance, certainty, request, moveTo: null, moveAway: null);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildGroupByResult(collection, reply);
    }

    internal async Task<WeaviateResult> SearchBM25(
        string collection,
        string query,
        string[]? searchFields,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: null,
            groupBy: null,
            fields: fields,
            metadata: metadata,
            reference: reference
        );

        BuildBM25(request, query, properties: searchFields);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildResult(collection, reply);
    }
}

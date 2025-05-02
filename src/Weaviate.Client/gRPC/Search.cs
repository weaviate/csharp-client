using Weaviate.Client.Models;
using Weaviate.V1;

using WeaviateObject = Weaviate.Client.Models.WeaviateObject;

namespace Weaviate.Client.Grpc;

public partial class WeaviateGrpcClient
{
    internal SearchRequest BaseSearchRequest(string collection, Filters? filter = null, uint? limit = null, GroupByConstraint? groupBy = null)
    {
        return new SearchRequest()
        {
            Collection = collection,
            Filters = filter,
            Uses123Api = true,
            Uses125Api = true,
            Uses127Api = true,
            Limit = limit ?? 0,
            GroupBy = groupBy is not null ? new GroupBy()
            {
                Path = { groupBy.PropertyName },
                NumberOfGroups = Convert.ToInt32(groupBy.NumberOfGroups),
                ObjectsPerGroup = Convert.ToInt32(groupBy.ObjectsPerGroup),
            } : null,
            Metadata = new MetadataRequest()
            {
                Uuid = true,
                Vector = true,
                Vectors = { "default" }
            }
        };
    }

    private static IEnumerable<WeaviateObject> BuildResult(string collection, SearchReply reply)
    {
        if (!reply.Results.Any())
        {
            return [];
        }

        return reply.Results.Select(result => new Models.WeaviateObject(collection)
        {
            ID = Guid.Parse(result.Metadata.Id),
            Vector = result.Metadata.Vector,
            Vectors = result.Metadata.Vectors.ToDictionary(v => v.Name, v => (IList<float>)v.VectorBytes.FromByteString<float>().ToList()),
            Data = buildObjectFromProperties(result.Properties.NonRefProps),
        });
    }

    private static Models.GroupByResult BuildGroupByResult(string collection, SearchReply reply)
    {
        if (!reply.GroupByResults.Any())
        {
            return (new List<WeaviateGroupByObject>(), new Dictionary<string, WeaviateGroup>());
        }

        var groups = reply.GroupByResults.ToDictionary(k => k.Name, v => new WeaviateGroup()
        {
            Name = v.Name,
            Objects = v.Objects.Select(obj => new WeaviateGroupByObject(collection)
            {
                ID = Guid.Parse(obj.Metadata.Id),
                Vector = obj.Metadata.Vector,
                Vectors = obj.Metadata.Vectors.ToDictionary(v => v.Name, v => (IList<float>)v.VectorBytes.FromByteString<float>().ToList()),
                Data = buildObjectFromProperties(obj.Properties.NonRefProps),
                BelongsToGroup = v.Name,
            }).ToArray()
        });

        var objects = groups.Values.SelectMany(g => g.Objects).ToList();

        return (objects, groups);
    }

    private static void BuildNearText(string query, double? distance, double? certainty, SearchRequest request, Move? moveTo, Move? moveAway)
    {
        request.NearText = new NearTextSearch
        {
            Query = { query },
            // Targets = null,
            // VectorForTargets = { },
        };

        if (moveTo is not null)
        {
            var uuids = moveTo.Objects is null ? [] : (new Guid?[] { moveTo.Objects }).Select(x => x.ToString());
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
            var uuids = moveAway.Objects is null ? [] : (new Guid?[] { moveAway.Objects }).Select(x => x.ToString());
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

    private static void BuildNearVector(float[] vector, float? distance, float? certainty, SearchRequest request)
    {
        request.NearVector = new NearVector
        {
            Vector = { vector },
            Vectors = {
                    new Vectors {
                        Name = "default",
                        Type = Vectors.Types.VectorType.SingleFp32,
                        VectorBytes = vector.ToByteString(),
                    }
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

    internal async Task<IEnumerable<WeaviateObject>> FetchObjects(string collection, Filters? filter = null, uint? limit = null)
    {
        var req = BaseSearchRequest(collection, filter, limit);

        SearchReply? reply = await _grpcClient.SearchAsync(req);

        return BuildResult(collection, reply);
    }

    public async Task<IEnumerable<WeaviateObject>> SearchNearVector(string collection, float[] vector, float? distance = null, float? certainty = null, uint? limit = null)
    {
        var request = BaseSearchRequest(collection, filter: null, limit: limit);

        BuildNearVector(vector, distance, certainty, request);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildResult(collection, reply);
    }

    internal async Task<IEnumerable<WeaviateObject>> SearchNearText(string collection, string query, float? distance, float? certainty, uint? limit, Move? moveTo, Move? moveAway)
    {
        var request = BaseSearchRequest(collection, filter: null, limit: limit);

        BuildNearText(query, distance, certainty, request, moveTo, moveAway);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildResult(collection, reply);
    }

    public async Task<Models.GroupByResult> SearchNearVector(string collection, float[] vector, GroupByConstraint groupBy, float? distance = null, float? certainty = null, uint? limit = null)
    {
        var request = BaseSearchRequest(collection, filter: null, limit: limit, groupBy: groupBy);

        BuildNearVector(vector, distance, certainty, request);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildGroupByResult(collection, reply);
    }

    internal async Task<Models.GroupByResult> SearchNearText(string collection, string query, GroupByConstraint groupBy, float? distance, float? certainty, uint? limit)
    {
        var request = BaseSearchRequest(collection, filter: null, limit: limit, groupBy: groupBy);

        BuildNearText(query, distance, certainty, request, moveTo: null, moveAway: null);

        SearchReply? reply = await _grpcClient.SearchAsync(request);

        return BuildGroupByResult(collection, reply);
    }
}

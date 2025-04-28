using Google.Protobuf;
using Weaviate.Client.Rest.Dto;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public class GroupByConstraint
{
    public required string PropertyName { get; set; }
    public uint NumberOfGroups { get; set; }
    public uint ObjectsPerGroup { get; set; }
}

public partial class WeaviateGrpcClient
{
    internal SearchRequest BaseSearchRequest(string collection, Filters? filter = null, uint? limit = null)
    {
        return new SearchRequest()
        {
            Collection = collection,
            Filters = filter,
            Uses123Api = true,
            Uses125Api = true,
            Uses127Api = true,
            Limit = limit ?? 0,
            Metadata = new MetadataRequest()
            {
                Uuid = true,
                Vector = true,
                Vectors = { "default" }
            }
        };
    }

    internal async Task<IList<WeaviateObject>> FetchObjects(string collection, Filters? filter = null, uint? limit = null)
    {
        var req = BaseSearchRequest(collection, filter, limit);

        SearchReply? reply = await _grpcClient.SearchAsync(req);

        if (!reply.Results.Any())
        {
            return [];
        }

        return reply.Results.Select(result => new WeaviateObject
        {
            ID = Guid.Parse(result.Metadata.Id),
            Vector = result.Metadata.Vector,
            Vectors = result.Metadata.Vectors.ToDictionary(v => v.Name, v =>
            {
                using (var ms = new MemoryStream(v.VectorBytes.ToByteArray()))
                {
                    return ms.FromStream<float>().ToList().AsEnumerable();
                }
            }),
            Properties = buildObjectFromProperties(result.Properties.NonRefProps),
        }).ToList();
    }

    public string SearchNearText(string text, int? limit = null)
    {
        return "";
    }

    // TODO Find a way to make IntelliSense know that it's either Distance or Certainty, but not both.
    public async Task<IEnumerable<WeaviateObject>> SearchNearVector(string collection, float[] vector, float? distance = null, float? certainty = null, uint? limit = null)
    {
        var request = BaseSearchRequest(collection, filter: null, limit: limit);

        var vectorStream = vector.ToStream();
        var vectorBytes = ByteString.FromStream(stream: vectorStream);
        vectorStream.Dispose();

        request.NearVector = new NearVector
        {
            Vector = { vector },
            Vectors = {
                    new Vectors {
                        Name = "default",
                        Type = Vectors.Types.VectorType.SingleFp32,
                        VectorBytes = vectorBytes,
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


        SearchReply? reply = await _grpcClient.SearchAsync(request);

        if (!reply.Results.Any())
        {
            return [];
        }

        return reply.Results.Select(result => new WeaviateObject
        {
            ID = Guid.Parse(result.Metadata.Id),
            Vector = result.Metadata.Vector,
            Vectors = result.Metadata.Vectors.ToDictionary(v => v.Name, v =>
            {
                using (var ms = new MemoryStream(v.VectorBytes.ToByteArray()))
                {
                    return ms.FromStream<float>().ToList().AsEnumerable();
                }
            }),
            Properties = buildObjectFromProperties(result.Properties.NonRefProps),
        }).ToList();
    }

    public async Task<(IEnumerable<WeaviateGroupByObject>, IDictionary<string, WeaviateGroup>)> SearchNearVectorWithGroupBy(string collection, float[] vector, GroupByConstraint groupBy, float? distance = null, float? certainty = null, uint? limit = null)
    {
        var request = BaseSearchRequest(collection, filter: null, limit: limit);

        var vectorStream = vector.ToStream();
        var vectorBytes = ByteString.FromStream(stream: vectorStream);
        vectorStream.Dispose();

        request.GroupBy = new GroupBy()
        {
            Path = { groupBy.PropertyName },
            NumberOfGroups = Convert.ToInt32(groupBy.NumberOfGroups),
            ObjectsPerGroup = Convert.ToInt32(groupBy.ObjectsPerGroup),
        };

        request.NearVector = new NearVector
        {
            Vector = { vector },
            Vectors = {
                    new Vectors {
                        Name = "default",
                        Type = Vectors.Types.VectorType.SingleFp32,
                        VectorBytes = vectorBytes,
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


        SearchReply? reply = await _grpcClient.SearchAsync(request);

        if (!reply.GroupByResults.Any())
        {
            return (new List<WeaviateGroupByObject>(), new Dictionary<string, WeaviateGroup>());
        }

        var groups = reply.GroupByResults.ToDictionary(k => k.Name, v => new WeaviateGroup()
        {
            Name = v.Name,
            Objects = v.Objects.Select(obj => new WeaviateGroupByObject
            {
                ID = Guid.Parse(obj.Metadata.Id),
                Vector = obj.Metadata.Vector,
                Vectors = obj.Metadata.Vectors.ToDictionary(v => v.Name, v =>
                {
                    using (var ms = new MemoryStream(v.VectorBytes.ToByteArray()))
                    {
                        return ms.FromStream<float>().ToList().AsEnumerable();
                    }
                }),
                Properties = buildObjectFromProperties(obj.Properties.NonRefProps),
                BelongsToGroup = v.Name,
            }).ToArray()
        });

        var objects = groups.Values.SelectMany(g => g.Objects).ToList();


        return (objects, groups);
    }
}
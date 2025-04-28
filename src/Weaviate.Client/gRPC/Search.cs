using Google.Protobuf;
using Weaviate.Client.Rest.Dto;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

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
            Id = Guid.Parse(result.Metadata.Id),
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
            Id = Guid.Parse(result.Metadata.Id),
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
}
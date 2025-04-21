namespace Weaviate.Client.Grpc;

using Weaviate.Client.Rest.Dto;
using Weaviate.V1;

public partial class WeaviateGrpcClient
{
    public async Task<IList<WeaviateObject>> FetchObjects(string collection, Filters? filter = null, uint? limit = null)
    {
        var req = new SearchRequest()
        {
            Collection = collection,
            Filters = filter,
            Uses123Api = true,
            Uses125Api = true,
            Uses127Api = true,
            Limit = limit ?? 0
        };

        req.Metadata = new MetadataRequest()
        {
            Uuid = true,
        };

        SearchReply? reply = await _grpcClient.SearchAsync(req);

        if (!reply.Results.Any())
        {
            return [];
        }

        return reply.Results
            .Select(result => new WeaviateObject
            {
                Id = Guid.Parse(result.Metadata.Id),
                Properties = buildObjectFromProperties(result.Properties.NonRefProps),
            }).ToList();
    }

    public string SearchNearText(string text, int? limit = null)
    {
        return "";
    }

    public string SearchNearVector(string collection, float[] vector, int? limit = null, float? distance = null)
    {
        var request = new SearchRequest()
        {
            // For NV search only use vectors from -1.0 to 1.0
            Collection = collection,
            NearVector = new NearVector()
            {

            }
        };

        return "";
    }
}
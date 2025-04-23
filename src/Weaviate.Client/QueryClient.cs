using Weaviate.Client.Models;

namespace Weaviate.Client;

public class QueryClient<TData>
{
    private readonly CollectionClient<TData> _collectionClient;

    private WeaviateClient _client => _collectionClient.Client;

    public QueryClient(CollectionClient<TData> collectionClient)
    {
        _collectionClient = collectionClient;
    }

    public async Task<object> NearText(string text, int? limit = null)
    {
        return await Task.FromResult(new object { });
    }

    // public async IAsyncEnumerable<WeaviateObject<TData>> NearVector(float[] vector, uint? limit, string[]? fields, string[]? metadata)
    // {
    //     await foreach (var r in NearVector<TData>(vector, limit: limit, fields: fields, metadata: metadata))
    //         yield return r;
    // }

    public async IAsyncEnumerable<Rest.Dto.WeaviateObject> NearVector(float[] vector, float? distance = null, float? certainty = null, uint? limit = null, string[]? fields = null, string[]? metadata = null)
    {
        var results =
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                distance: distance,
                certainty: certainty,
                limit: limit
            );
        foreach (var r in results)
        {
            yield return r;
        }
    }
}
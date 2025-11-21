using System.Net;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    /// <summary>
    /// Checks if the Weaviate instance reports it is live (process is running).
    /// </summary>
    /// <returns>true if live endpoint returns 200 OK, false otherwise.</returns>
    internal async Task<bool> LiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var path = WeaviateEndpoints.WellKnownLive();
            var response = await _httpClient.GetAsync(path, cancellationToken);
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the Weaviate instance is ready (all internal services initialized).
    /// </summary>
    /// <returns>true if ready endpoint returns 200 OK, false otherwise.</returns>
    internal async Task<bool> ReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var path = WeaviateEndpoints.WellKnownReady();
            var response = await _httpClient.GetAsync(path, cancellationToken);
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }
}

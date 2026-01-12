using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client class
/// </summary>
internal partial class WeaviateRestClient
{
    // Tenants API
    /// <summary>
    /// Tenantses the add using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="tenants">The tenants</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of rest dto tenant</returns>
    internal async Task<IEnumerable<Dto.Tenant>> TenantsAdd(
        string collectionName,
        Dto.Tenant[] tenants,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.CollectionTenants(collectionName);

        var response = await _httpClient.PostAsJsonAsync(
            path,
            tenants,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "tenants add",
            ResourceType.Tenant
        );
        var result = await response.DecodeAsync<IEnumerable<Dto.Tenant>>(cancellationToken);
        return result ?? Enumerable.Empty<Dto.Tenant>();
    }

    /// <summary>
    /// Tenants the update using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="tenants">The tenants</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentNullException">Tenant names cannot be null.</exception>
    /// <returns>A task containing an enumerable of rest dto tenant</returns>
    internal async Task<IEnumerable<Dto.Tenant>> TenantUpdate(
        string collectionName,
        Dto.Tenant[] tenants,
        CancellationToken cancellationToken = default
    )
    {
        if (tenants.Any(t => t.Name is null))
        {
            throw new ArgumentNullException(nameof(tenants), "Tenant names cannot be null.");
        }

        var path = WeaviateEndpoints.CollectionTenants(collectionName);
        var response = await _httpClient.PutAsJsonAsync(
            path,
            tenants,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.Conflict,
                // HttpStatusCode.InternalServerError,
            ],
            "tenant update",
            ResourceType.Tenant
        );

        var result = await response.DecodeAsync<Dto.Tenant[]>(cancellationToken);

        return result ?? Enumerable.Empty<Dto.Tenant>();
    }

    /// <summary>
    /// Tenantses the delete using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="tenantNames">The tenant names</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentException">Tenant names cannot be null or empty. </exception>
    internal async Task TenantsDelete(
        string collectionName,
        IEnumerable<string> tenantNames,
        CancellationToken cancellationToken = default
    )
    {
        if (tenantNames.Any(name => string.IsNullOrWhiteSpace(name)))
        {
            throw new ArgumentException(
                "Tenant names cannot be null or empty.",
                nameof(tenantNames)
            );
        }

        var path = WeaviateEndpoints.CollectionTenants(collectionName);
        var request = new HttpRequestMessage(HttpMethod.Delete, path)
        {
            Content = JsonContent.Create(tenantNames, options: RestJsonSerializerOptions),
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);

        await response.ManageStatusCode([HttpStatusCode.OK], "tenants delete", ResourceType.Tenant);
    }
}

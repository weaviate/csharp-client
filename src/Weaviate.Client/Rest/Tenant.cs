using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    // Tenants API
    internal async Task<IEnumerable<Rest.Dto.Tenant>> TenantsAdd(
        string collectionName,
        Rest.Dto.Tenant[] tenants,
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
        var result = await response.DecodeAsync<IEnumerable<Rest.Dto.Tenant>>(cancellationToken);
        return result ?? Enumerable.Empty<Rest.Dto.Tenant>();
    }

    internal async Task<IEnumerable<Rest.Dto.Tenant>> TenantUpdate(
        string collectionName,
        Rest.Dto.Tenant[] tenants,
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

        var result = await response.DecodeAsync<Rest.Dto.Tenant[]>(cancellationToken);

        return result ?? Enumerable.Empty<Rest.Dto.Tenant>();
    }

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

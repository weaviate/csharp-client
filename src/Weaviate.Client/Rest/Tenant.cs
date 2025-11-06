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
        params Rest.Dto.Tenant[] tenants
    )
    {
        var path = WeaviateEndpoints.CollectionTenants(collectionName);

        var response = await _httpClient.PostAsJsonAsync(
            path,
            tenants,
            options: RestJsonSerializerOptions
        );
        await response.EnsureExpectedStatusCodeAsync([200], "tenants add");
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<Rest.Dto.Tenant>>(
            options: RestJsonSerializerOptions
        );
        return result ?? Enumerable.Empty<Rest.Dto.Tenant>();
    }

    internal async Task<IEnumerable<Rest.Dto.Tenant>> TenantUpdate(
        string collectionName,
        params Rest.Dto.Tenant[] tenants
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
            options: RestJsonSerializerOptions
        );
        await response.EnsureExpectedStatusCodeAsync([200], "tenant update");

        var result = await response.Content.ReadFromJsonAsync<Rest.Dto.Tenant[]>(
            options: RestJsonSerializerOptions
        );

        return result ?? Enumerable.Empty<Rest.Dto.Tenant>();
    }

    internal async Task TenantsDelete(string collectionName, IEnumerable<string> tenantNames)
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
        var response = await _httpClient.SendAsync(request);
        await response.EnsureExpectedStatusCodeAsync([200], "tenants delete");
    }
}

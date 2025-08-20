using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    internal async Task<IEnumerable<V1.Tenant>> TenantsGet(string name, string[] tenantNames)
    {
        var request = new TenantsGetRequest { Collection = name };

        if (tenantNames.Length > 0)
        {
            request.Names = new TenantNames { Values = { tenantNames } };
        }

        TenantsGetReply reply = await _grpcClient.TenantsGetAsync(
            request,
            headers: _defaultHeaders
        );

        return reply.Tenants;
    }
}

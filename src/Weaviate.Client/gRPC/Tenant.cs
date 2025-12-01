namespace Weaviate.Client.Grpc;

using Protobuf.V1;

internal partial class WeaviateGrpcClient
{
    internal async Task<IEnumerable<Tenant>> TenantsGet(
        string name,
        string[] tenantNames,
        CancellationToken cancellationToken = default
    )
    {
        var request = new TenantsGetRequest { Collection = name };

        if (tenantNames.Length > 0)
        {
            request.Names = new TenantNames { Values = { tenantNames } };
        }

        TenantsGetReply reply = await _grpcClient.TenantsGetAsync(
            request,
            CreateCallOptions(cancellationToken)
        );

        return reply.Tenants;
    }
}

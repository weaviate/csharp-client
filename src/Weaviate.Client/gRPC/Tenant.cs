namespace Weaviate.Client.Grpc;

using Protobuf.V1;

/// <summary>
/// The weaviate grpc client class
/// </summary>
internal partial class WeaviateGrpcClient
{
    /// <summary>
    /// Tenantses the get using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="tenantNames">The tenant names</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of tenant</returns>
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

        try
        {
            TenantsGetReply reply = await _grpcClient.TenantsGetAsync(
                request,
                CreateCallOptions(cancellationToken)
            );

            return reply.Tenants;
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            // Use centralized exception mapping helper
            throw Internal.ExceptionHelper.MapGrpcException(ex, "Tenants get request failed");
        }
    }
}

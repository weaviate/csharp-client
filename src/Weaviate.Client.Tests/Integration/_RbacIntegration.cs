namespace Weaviate.Client.Tests.Integration;

using Microsoft.Extensions.Configuration;
using Weaviate.Client;

/// <summary>
/// Base class for RBAC integration tests. Connects to the RBAC-enabled Weaviate instance
/// whose ports are controlled by WV_RBAC_HTTP_PORT / WV_RBAC_GRPC_PORT environment variables
/// (defaulting to 8092 / 50063 to match the local docker-compose RBAC service).
/// Use WV_RBAC_HTTP_PORT / WV_RBAC_GRPC_PORT to point these tests at the proxy RBAC endpoint.
/// </summary>
public abstract class RbacIntegrationTests : IntegrationTests
{
    /// <summary>The API key for the built-in admin user on the RBAC server.</summary>
    protected const string ADMIN_API_KEY = "admin-key";

    /// <inheritdoc />
    public override ushort RestPort => _configuration.GetValue<ushort>("WV_RBAC_HTTP_PORT", 8092);

    /// <inheritdoc />
    public override ushort GrpcPort => _configuration.GetValue<ushort>("WV_RBAC_GRPC_PORT", 50063);

    /// <inheritdoc />
    public override ICredentials? Credentials => Auth.ApiKey(ADMIN_API_KEY);
}

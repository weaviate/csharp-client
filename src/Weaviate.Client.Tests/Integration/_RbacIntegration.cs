namespace Weaviate.Client.Tests.Integration;

using Microsoft.Extensions.Configuration;
using Weaviate.Client;

/// <summary>
/// Base class for RBAC integration tests. Connects to the RBAC-enabled Weaviate instance.
/// Configurable via WV_TEST_RBAC_HOST, WV_TEST_RBAC_REST_PORT, WV_TEST_RBAC_GRPC_PORT
/// (defaulting to localhost:8092 / 50063).
/// </summary>
public abstract class RbacIntegrationTests : IntegrationTests
{
    /// <summary>The API key for the built-in admin user on the RBAC server.</summary>
    protected const string ADMIN_API_KEY = "admin-key";

    /// <inheritdoc />
    public override string RestHost => _configuration.GetValue<string>("WV_TEST_RBAC_HOST") ?? "localhost";

    /// <inheritdoc />
    public override ushort RestPort => _configuration.GetValue<ushort>("WV_TEST_RBAC_REST_PORT", 8092);

    /// <inheritdoc />
    public override ushort GrpcPort => _configuration.GetValue<ushort>("WV_TEST_RBAC_GRPC_PORT", 50063);

    /// <inheritdoc />
    public override ICredentials? Credentials => Auth.ApiKey(ADMIN_API_KEY);
}

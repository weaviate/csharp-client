namespace Weaviate.Client.Tests.Integration;

using System.Linq;
using Weaviate.Client;
using Xunit;

/// <summary>
/// RBAC Groups integration tests (Rest:8092 gRPC:50063). Groups functionality is limited; these
/// tests mainly exercise listing groups and fetching their role assignments.
/// </summary>
public class TestRbacGroups : IntegrationTests
{
    public override ushort RestPort => 8092;
    public override ushort GrpcPort => 50063;
    private const string ADMIN_API_KEY = "admin-key";

    public override ICredentials? Credentials => Auth.ApiKey(ADMIN_API_KEY);

    [Fact, Trait("Category", "RBAC")]
    public async Task ListGroups()
    {
        RequireVersion("1.30.0");
        var groups = await _weaviate.Groups.List("oidc");
        // Enumeration should not throw; emptiness is acceptable depending on environment configuration.
        _ = groups.ToList();
    }

    [Fact, Trait("Category", "RBAC")]
    public async Task GetGroupRoles()
    {
        RequireVersion("1.30.0");
        var groupId = $"/test-group-{Random.Shared.Next(1, 10000)}";
        var roles = await _weaviate.Groups.Roles(groupId, "oidc");
        _ = roles.ToList(); // Accept empty; presence depends on external identity provider configuration.
    }
}

using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public class RbacTests
{
    [Fact]
    public async Task UsersClient_MapsDtoToModel()
    {
        var now = DateTimeOffset.UtcNow;
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        handler.AddJsonResponse(
            new object[]
            {
                new
                {
                    roles = new[] { "reader", "writer" },
                    userId = "alice",
                    dbUserType = "db_user",
                    active = true,
                    createdAt = now,
                    lastUsedAt = now,
                    apiKeyFirstLetters = "abc",
                },
                new
                {
                    roles = Array.Empty<string>(),
                    userId = "env-user",
                    dbUserType = "db_env_user",
                    active = false,
                    createdAt = now,
                    lastUsedAt = (DateTimeOffset?)null,
                    apiKeyFirstLetters = (string?)null,
                },
            },
            expectedEndpoint: "/v1/users/db"
        );

        // Invoke list (consume first queued response)
        var users = await client.Users.Db.List();
        var list = users.ToList();
        Assert.Equal(2, list.Count);
        var alice = list.First(u => u.UserId == "alice");
        Assert.True(alice.Active);
        Assert.Equal(DatabaseUserType.DbUser, alice.DbUserType);
        Assert.Equal("abc", alice.ApiKeyFirstLetters);
        var env = list.First(u => u.UserId == "env-user");
        Assert.Equal(DatabaseUserType.DbEnvUser, env.DbUserType);
        Assert.False(env.Active);
    }

    [Fact]
    public async Task RolesClient_MapsPermissions()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        handler.AddJsonResponse(
            new object[]
            {
                new
                {
                    name = "reader",
                    permissions = new object[] { new { action = "read_roles" } },
                },
                new
                {
                    name = "writer",
                    permissions = new object[] { new { action = "update_roles" } },
                },
            },
            expectedEndpoint: "/v1/authz/roles"
        );

        var roles = (await client.Roles.ListAll()).ToList();
        Assert.Equal(2, roles.Count);
        Assert.Contains(
            roles,
            r =>
                r.Name == "reader"
                && r.Permissions.Any(p => p.Action == RbacPermissionAction.ReadRoles)
        );
    }
}

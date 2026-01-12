using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The rbac tests class
/// </summary>
public class RbacTests
{
    /// <summary>
    /// Tests that users client maps dto to model
    /// </summary>
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
        var users = await client.Users.Db.List(
            cancellationToken: TestContext.Current.CancellationToken
        );
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

    /// <summary>
    /// Tests that roles client maps permissions
    /// </summary>
    [Fact]
    public async Task RolesClient_MapsPermissions()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        handler.AddJsonResponse(
            new[]
            {
                new Rest.Dto.Role
                {
                    Name = "reader",
                    Permissions = new[]
                    {
                        new Rest.Dto.Permission
                        {
                            Roles = new Rest.Dto.Roles
                            {
                                Role = "reader",
                                Scope = Weaviate.Client.Rest.Dto.RolesScope.Match,
                            },
                            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_roles,
                        },
                    },
                },
                new Rest.Dto.Role
                {
                    Name = "writer",
                    Permissions = new[]
                    {
                        new Rest.Dto.Permission
                        {
                            Roles = new Rest.Dto.Roles
                            {
                                Role = "writer",
                                Scope = Weaviate.Client.Rest.Dto.RolesScope.Match,
                            },
                            Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_roles,
                        },
                    },
                },
            },
            expectedEndpoint: "/v1/authz/roles"
        );

        var roles = (await client.Roles.ListAll(TestContext.Current.CancellationToken)).ToList();
        Assert.Equal(2, roles.Count);
        Assert.Contains(
            roles,
            r => r.Name == "reader" && r.Permissions.Any(p => p is Permissions.Roles { Read: true })
        );
    }
}

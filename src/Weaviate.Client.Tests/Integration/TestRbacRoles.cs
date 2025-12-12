namespace Weaviate.Client.Tests.Integration;

using System;
using System.Linq;
using System.Threading.Tasks;
using Weaviate.Client.Models;
using Xunit;

[Trait("Category", "RBAC")]
public class TestRbacRoles : IntegrationTests
{
    public override ushort RestPort => 8092;
    public override ushort GrpcPort => 50063;
    private const string ADMIN_API_KEY = "admin-key";
    public override ICredentials? Credentials => Auth.ApiKey(ADMIN_API_KEY);

    private static string MakeRoleName(string suffix) =>
        $"test-role-{suffix}-{Random.Shared.Next(1, 1_000_000)}";

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        RequireVersion("1.30.0");
    }

    [Fact]
    public async Task ListRoles()
    {
        RequireVersion("1.30.0");
        var roles = (await _weaviate.Roles.ListAll(TestContext.Current.CancellationToken)).ToList();
        Assert.NotEmpty(roles);
        Assert.Contains(roles, r => r.Name == "viewer");
    }

    [Fact]
    public async Task CreateRoleWithPermissions()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("create");
        try
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
            var created = await _weaviate.Roles.Create(
                roleName,
                [new Permissions.Roles(roleName, null) { Read = true }],
                TestContext.Current.CancellationToken
            );
            Assert.NotNull(created);
            Assert.Equal(roleName, created.Name);
            Assert.Single(created.Permissions);
            Assert.Contains(created.Permissions, p => p is Permissions.Roles { Read: true });

            var fetched = await _weaviate.Roles.Get(
                roleName,
                TestContext.Current.CancellationToken
            );
            Assert.NotNull(fetched);
            Assert.Equal(roleName, fetched!.Name);
            Assert.Single(fetched.Permissions);
            Assert.Contains(fetched.Permissions, p => p is Permissions.Roles { Read: true });
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task CreateRoleConflict()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("conflict");
        try
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
            await _weaviate.Roles.Create(
                roleName,
                Array.Empty<PermissionScope>(),
                TestContext.Current.CancellationToken
            );
            await Assert.ThrowsAsync<WeaviateConflictException>(async () =>
                await _weaviate.Roles.Create(
                    roleName,
                    Array.Empty<PermissionScope>(),
                    TestContext.Current.CancellationToken
                )
            );
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task GetRole()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("get");
        try
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
            await _weaviate.Roles.Create(
                roleName,
                [new Permissions.Roles(roleName) { Read = true, Create = true }],
                TestContext.Current.CancellationToken
            );
            var role = await _weaviate.Roles.Get(roleName, TestContext.Current.CancellationToken);
            Assert.NotNull(role);
            Assert.Equal(roleName, role!.Name);
            Assert.NotEmpty(role.Permissions);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task DeleteRole()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("delete");
        await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        var role = await _weaviate.Roles.Create(
            roleName,
            Array.Empty<PermissionScope>(),
            TestContext.Current.CancellationToken
        );
        Assert.NotNull(role);

        await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<WeaviateNotFoundException>(async () =>
            await _weaviate.Roles.Get(roleName, TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task AddPermissionsToExisting()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("add-perms");
        try
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
            await _weaviate.Roles.Create(
                roleName,
                [new Permissions.Roles(roleName) { Read = true }],
                TestContext.Current.CancellationToken
            );
            var updated = await _weaviate.Roles.AddPermissions(
                roleName,
                [new Permissions.Roles(roleName) { Create = true }],
                TestContext.Current.CancellationToken
            );
            Assert.NotNull(updated);
            Assert.Single(updated.Permissions);
            Assert.Contains(updated.Permissions, p => p is Permissions.Roles { Read: true });
            Assert.Contains(updated.Permissions, p => p is Permissions.Roles { Create: true });
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task RemovePermissionsFromExisting()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("remove-perms");
        try
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
            await _weaviate.Roles.Create(
                roleName,
                new[]
                {
                    new Permissions.Roles(roleName) { Read = true, Create = true },
                },
                TestContext.Current.CancellationToken
            );
            var updated = await _weaviate.Roles.RemovePermissions(
                roleName,
                [new Permissions.Roles(roleName) { Create = true }],
                TestContext.Current.CancellationToken
            );
            Assert.NotNull(updated);
            Assert.Single(updated.Permissions);
            Assert.Contains(updated.Permissions, p => p is Permissions.Roles { Read: true });
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task HasPermission()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("has-perm");
        try
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
            await _weaviate.Roles.Create(
                roleName,
                [new Permissions.Roles(roleName) { Read = true }],
                TestContext.Current.CancellationToken
            );
            var has = await _weaviate.Roles.HasPermission(
                roleName,
                new Permissions.Roles(roleName) { Read = true },
                TestContext.Current.CancellationToken
            );
            Assert.True(has);
            var hasOther = await _weaviate.Roles.HasPermission(
                roleName,
                new Permissions.Roles(roleName) { Create = true },
                TestContext.Current.CancellationToken
            );
            Assert.False(hasOther);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task HasPermissionNonExistentRole()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("has-perm-missing");

        // Server returns 200 with false for non-existent roles (lenient behavior)
        // Does not throw exception - returns false instead
        var has = await _weaviate.Roles.HasPermission(
            roleName,
            new Permissions.Roles(roleName) { Read = true },
            TestContext.Current.CancellationToken
        );
        Assert.False(has);
    }

    [Fact]
    public async Task UserAssignments()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("user-assign");
        var userName = $"test-user-{Random.Shared.Next(1, 10000)}";
        try
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);

            await _weaviate.Roles.Create(
                roleName,
                Array.Empty<PermissionScope>(),
                TestContext.Current.CancellationToken
            );

            try
            {
                await _weaviate.Users.Db.Delete(userName, TestContext.Current.CancellationToken);
            }
            catch (WeaviateNotFoundException) { }

            await _weaviate.Users.Db.Create(
                userName,
                cancellationToken: TestContext.Current.CancellationToken
            );

            await _weaviate.Users.Db.AssignRoles(
                userName,
                new[] { roleName },
                TestContext.Current.CancellationToken
            );
            var assignments = (
                await _weaviate.Roles.GetUserAssignments(
                    roleName,
                    TestContext.Current.CancellationToken
                )
            ).ToList();
            Assert.Contains(assignments, a => a.UserId == userName);

            await _weaviate.Users.Db.RevokeRoles(
                userName,
                new[] { roleName },
                TestContext.Current.CancellationToken
            );
            var after = (
                await _weaviate.Roles.GetUserAssignments(
                    roleName,
                    TestContext.Current.CancellationToken
                )
            ).ToList();
            Assert.DoesNotContain(after, a => a.UserId == userName);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);

            await _weaviate.Users.Db.Delete(userName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task GetUserRoles()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("user-roles");
        var userName = $"test-user-{Random.Shared.Next(1, 10000)}";
        try
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);

            await _weaviate.Roles.Create(
                roleName,
                PermissionScope.Empty(),
                TestContext.Current.CancellationToken
            );

            try
            {
                await _weaviate.Users.Db.Delete(userName, TestContext.Current.CancellationToken);
            }
            catch (WeaviateNotFoundException) { }

            await _weaviate.Users.Db.Create(
                userName,
                cancellationToken: TestContext.Current.CancellationToken
            );
            await _weaviate.Users.Db.AssignRoles(
                userName,
                new[] { roleName },
                cancellationToken: TestContext.Current.CancellationToken
            );
            var roles = (
                await _weaviate.Users.Db.GetRoles(
                    userName,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            ).ToList();
            Assert.NotEmpty(roles);
            Assert.Contains(roles, r => r.Name == roleName);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);

            await _weaviate.Users.Db.Delete(userName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Test_RoleWithDataPermissions_MergesCorrectly()
    {
        RequireVersion("1.30.0");
        var randomUserName = $"multi-perm-user-{Random.Shared.Next(1, 10000)}";
        var roleName = $"multi-perm-role-{Random.Shared.Next(1, 10000)}";
        var collection = "Cats";
        try
        {
            // Create user
            await _weaviate.Users.Db.Create(
                randomUserName,
                cancellationToken: TestContext.Current.CancellationToken
            );

            // Create role with multiple Data permissions
            var permissions = new[]
            {
                new Permissions.Data(collection)
                {
                    Read = true,
                    Update = true,
                    Delete = true,
                },
            };
            await _weaviate.Roles.Create(
                roleName,
                permissions,
                TestContext.Current.CancellationToken
            );

            // Assign role to user
            await _weaviate.Users.Db.AssignRoles(
                randomUserName,
                new[] { roleName },
                cancellationToken: TestContext.Current.CancellationToken
            );

            // Get user and check merged permissions
            var roles = await _weaviate.Users.Db.GetRoles(
                randomUserName,
                includeFullRoles: true,
                cancellationToken: TestContext.Current.CancellationToken
            );

            var role = Assert.Single(roles);

            Assert.Equal(roleName, role.Name);
            Assert.NotNull(role);

            var dataPerm = role
                .Permissions.OfType<Permissions.Data>()
                .FirstOrDefault(p => p.Resource.Collection == collection);
            Assert.NotNull(dataPerm);
            Assert.True(dataPerm.Read);
            Assert.True(dataPerm.Update);
            Assert.True(dataPerm.Delete);
        }
        finally
        {
            await _weaviate.Users.Db.Delete(randomUserName, TestContext.Current.CancellationToken);
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Test_RoleWithMultipleDataPermissions_MergesCorrectly()
    {
        RequireVersion("1.30.0");
        var randomUserName = $"multi-perm-user-{Random.Shared.Next(1, 10000)}";
        var roleName = $"multi-perm-role-{Random.Shared.Next(1, 10000)}";
        var collection = "Cats";
        try
        {
            // Create user
            await _weaviate.Users.Db.Create(
                randomUserName,
                cancellationToken: TestContext.Current.CancellationToken
            );

            // Create role with multiple Data permissions
            var permissions = new[]
            {
                new Permissions.Data(collection) { Read = true },
                new Permissions.Data(collection) { Update = true },
                new Permissions.Data(collection, "*") { Delete = true },
            };
            await _weaviate.Roles.Create(
                roleName,
                permissions,
                TestContext.Current.CancellationToken
            );

            // Assign role to user
            await _weaviate.Users.Db.AssignRoles(
                randomUserName,
                new[] { roleName },
                TestContext.Current.CancellationToken
            );

            // Get user and check merged permissions
            var roles = await _weaviate.Users.Db.GetRoles(
                randomUserName,
                includeFullRoles: true,
                cancellationToken: TestContext.Current.CancellationToken
            );

            var role = Assert.Single(roles);

            Assert.Equal(roleName, role.Name);
            Assert.NotNull(role);

            var dataPerm = role
                .Permissions.OfType<Permissions.Data>()
                .FirstOrDefault(p => p.Resource.Collection == collection);
            Assert.NotNull(dataPerm);
            Assert.True(dataPerm.Read);
            Assert.True(dataPerm.Update);
            Assert.True(dataPerm.Delete);
        }
        finally
        {
            await _weaviate.Users.Db.Delete(randomUserName, TestContext.Current.CancellationToken);
            await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);
        }
    }
}

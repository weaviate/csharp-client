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

    public TestRbacRoles()
    {
        RequireVersion("1.30.0");
    }

    [Fact]
    public async Task ListRoles()
    {
        RequireVersion("1.30.0");
        var roles = (await _weaviate.Roles.List()).ToList();
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
            await _weaviate.Roles.Delete(roleName);
            var created = await _weaviate.Roles.Create(
                roleName,
                new[] { new PermissionInfo(RbacPermissionAction.ReadRoles) }
            );
            Assert.True(created);
            var fetched = await _weaviate.Roles.Get(roleName);
            Assert.NotNull(fetched);
            Assert.Equal(roleName, fetched!.Name);
            Assert.Single(fetched.Permissions);
            Assert.Equal("read_roles", fetched.Permissions.First().Action);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName);
        }
    }

    [Fact]
    public async Task GetRole()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("get");
        try
        {
            await _weaviate.Roles.Delete(roleName);
            await _weaviate.Roles.Create(
                roleName,
                new[] { new PermissionInfo(RbacPermissionAction.ReadRoles) }
            );
            var role = await _weaviate.Roles.Get(roleName);
            Assert.NotNull(role);
            Assert.Equal(roleName, role!.Name);
            Assert.NotEmpty(role.Permissions);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName);
        }
    }

    [Fact]
    public async Task DeleteRole()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("delete");
        await _weaviate.Roles.Delete(roleName);
        await _weaviate.Roles.Create(roleName, Array.Empty<PermissionInfo>());
        var deleted = await _weaviate.Roles.Delete(roleName);
        Assert.True(deleted);
        var role = await _weaviate.Roles.Get(roleName);
        Assert.Null(role);
    }

    [Fact]
    public async Task AddPermissionsToExisting()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("add-perms");
        try
        {
            await _weaviate.Roles.Delete(roleName);
            await _weaviate.Roles.Create(
                roleName,
                new[] { new PermissionInfo(RbacPermissionAction.ReadRoles) }
            );
            await _weaviate.Roles.AddPermissions(
                roleName,
                new[] { new PermissionInfo(RbacPermissionAction.CreateRoles) }
            );
            var role = await _weaviate.Roles.Get(roleName);
            Assert.NotNull(role);
            Assert.Equal(2, role!.Permissions.Count());
            Assert.Contains(role.Permissions, p => p.Action == "read_roles");
            Assert.Contains(role.Permissions, p => p.Action == "create_roles");
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName);
        }
    }

    [Fact]
    public async Task RemovePermissionsFromExisting()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("remove-perms");
        try
        {
            await _weaviate.Roles.Delete(roleName);
            await _weaviate.Roles.Create(
                roleName,
                new[]
                {
                    new PermissionInfo(RbacPermissionAction.ReadRoles),
                    new PermissionInfo(RbacPermissionAction.CreateRoles),
                }
            );
            await _weaviate.Roles.RemovePermissions(
                roleName,
                new[] { new PermissionInfo(RbacPermissionAction.CreateRoles) }
            );
            var role = await _weaviate.Roles.Get(roleName);
            Assert.NotNull(role);
            Assert.Single(role!.Permissions);
            Assert.Equal("read_roles", role.Permissions.First().Action);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName);
        }
    }

    [Fact]
    public async Task HasPermission()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("has-perm");
        try
        {
            await _weaviate.Roles.Delete(roleName);
            await _weaviate.Roles.Create(
                roleName,
                new[] { new PermissionInfo(RbacPermissionAction.ReadRoles) }
            );
            var has = await _weaviate.Roles.HasPermission(
                roleName,
                new PermissionInfo(RbacPermissionAction.ReadRoles)
            );
            Assert.True(has);
            var hasOther = await _weaviate.Roles.HasPermission(
                roleName,
                new PermissionInfo(RbacPermissionAction.CreateRoles)
            );
            Assert.False(hasOther);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName);
        }
    }

    [Fact]
    public async Task UserAssignments()
    {
        RequireVersion("1.30.0");
        var roleName = MakeRoleName("user-assign");
        var userName = $"test-user-{Random.Shared.Next(1, 10000)}";
        try
        {
            await _weaviate.Roles.Delete(roleName);
            await _weaviate.Roles.Create(roleName, Array.Empty<PermissionInfo>());
            await _weaviate.Users.Delete(userName);
            await _weaviate.Users.Create(userName);
            await _weaviate.Roles.AssignToUser(userName, "db", new[] { roleName });
            var assignments = (await _weaviate.Roles.GetUserAssignments(roleName)).ToList();
            Assert.Contains(assignments, a => a.UserId == userName);
            await _weaviate.Roles.RevokeFromUser(userName, "db", new[] { roleName });
            var after = (await _weaviate.Roles.GetUserAssignments(roleName)).ToList();
            Assert.DoesNotContain(after, a => a.UserId == userName);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName);
            await _weaviate.Users.Delete(userName);
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
            await _weaviate.Roles.Delete(roleName);
            await _weaviate.Roles.Create(roleName, Array.Empty<PermissionInfo>());
            await _weaviate.Users.Delete(userName);
            await _weaviate.Users.Create(userName);
            await _weaviate.Roles.AssignToUser(userName, "db", new[] { roleName });
            var roles = (await _weaviate.Roles.RolesForUser(userName, "db")).ToList();
            Assert.NotEmpty(roles);
            Assert.Contains(roles, r => r.Name == roleName);
        }
        finally
        {
            await _weaviate.Roles.Delete(roleName);
            await _weaviate.Users.Delete(userName);
        }
    }
}

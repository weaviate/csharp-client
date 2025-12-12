using System.Net;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public class PermissionInfoTests
{
    [Fact]
    public async Task RolesClient_CreateRole_PipelineMapsPermissionInfo()
    {
        // Arrange: mock client & queue server response for role creation.
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        var roleName = $"role-{Guid.NewGuid():N}";
        var roleResponse = new Rest.Dto.Role
        {
            Name = roleName,
            Permissions = new Rest.Dto.Permission[]
            {
                new Rest.Dto.Permission
                {
                    Roles = new Rest.Dto.Roles
                    {
                        Role = roleName,
                        Scope = Rest.Dto.RolesScope.Match,
                    },
                    Action = Rest.Dto.PermissionAction.Read_roles,
                },
            },
        };
        // Response to POST /v1/authz/roles (201 Created)
        handler.AddJsonResponse(
            new { },
            expectedEndpoint: "/v1/authz/roles",
            statusCode: HttpStatusCode.Created
        );
        // Follow-up GET /v1/authz/roles/{id} response (200 OK)
        handler.AddJsonResponse(
            roleResponse,
            expectedEndpoint: $"/v1/authz/roles/{roleName}",
            statusCode: HttpStatusCode.OK
        );

        // Act: create role
        var created = await client.Roles.Create(
            roleName,
            new[] { new Permissions.Roles(roleName) { Read = true } },
            TestContext.Current.CancellationToken
        );

        // Assert: request body contained correct action string.
        var postRequest = handler.Requests.First(r => r.Method == HttpMethod.Post);
        var requestJson = await postRequest.Content!.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        using var doc = System.Text.Json.JsonDocument.Parse(requestJson);
        var permissionsEl = doc.RootElement.GetProperty("permissions");
        Assert.True(
            permissionsEl
                .EnumerateArray()
                .Any(p => p.GetProperty("action").GetString() == "read_roles"),
            $"Expected request JSON to contain permissions action=read_roles. Body was: {requestJson}"
        );

        // Assert: returned model mapped both raw and enum values.
        var perm = Assert.Single(created.Permissions);
        Assert.True(perm is Permissions.Roles { Read: true });
    }

    [Fact]
    public async Task RolesClient_GetRole_WithFutureAction_ThrowsWeaviateClientException()
    {
        // When server returns an unknown permission action, deserialization should fail
        // with a helpful WeaviateClientException instead of raw JsonException.
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        var roleName = $"role-{Guid.NewGuid():N}";

        handler.AddJsonResponse(
            new
            {
                name = roleName,
                permissions = new object[] { new { action = "future_new_action" } },
            },
            expectedEndpoint: $"/v1/authz/roles/{roleName}",
            statusCode: HttpStatusCode.OK
        );

        // Act & Assert: should throw WeaviateClientException with upgrade guidance.
        var ex = await Assert.ThrowsAsync<WeaviateClientException>(async () =>
            await client.Roles.Get(roleName, TestContext.Current.CancellationToken)
        );
        Assert.Contains("future_new_action", ex.Message);
        Assert.Contains("PermissionAction", ex.Message);
        Assert.Contains("client", ex.Message.ToLower());
    }

    [Fact]
    public void PermissionInfo_ToModel_MapsAllResourceScopes()
    {
        // Backups
        var backupsDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Manage_backups,
            Backups = new Weaviate.Client.Rest.Dto.Backups { Collection = "backup-collection" },
        };
        var backupsModel = Permissions.Parse([backupsDto]).Single();
        Assert.True(backupsModel is Permissions.Backups);
        Assert.Equal("backup-collection", ((Permissions.Backups)backupsModel).Resource.Collection);

        // Data
        var dataDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_data,
            Data = new Weaviate.Client.Rest.Dto.Data
            {
                Collection = "data-collection",
                Tenant = "tenant1",
                Object = "object1",
            },
        };
        var dataModel = Permissions.Parse([dataDto]).Single();
        Assert.True(dataModel is Permissions.Data);
        var dataRes = ((Permissions.Data)dataModel).Resource;
        Assert.Equal("data-collection", dataRes.Collection);
        Assert.Equal("tenant1", dataRes.Tenant);

        // Nodes
        var nodesDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_nodes,
            Nodes = new Weaviate.Client.Rest.Dto.Nodes
            {
                Collection = "nodes-collection",
                Verbosity = Weaviate.Client.Rest.Dto.NodesVerbosity.Verbose,
            },
        };
        var nodesModel = Permissions.Parse([nodesDto]).Single();
        Assert.True(nodesModel is Permissions.Nodes);
        var nodesRes = ((Permissions.Nodes)nodesModel).Resource;
        Assert.Equal("nodes-collection", nodesRes.Collection);
        Assert.Equal(NodeVerbosity.Verbose, nodesRes.Verbosity);

        // Users
        var usersDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_users,
            Users = new Weaviate.Client.Rest.Dto.Users { Users1 = "userA" },
        };
        var usersModel = Permissions.Parse([usersDto]).Single();
        Assert.True(usersModel is Permissions.Users);
        Assert.Equal("userA", ((Permissions.Users)usersModel).Resource.Users);

        // Groups
        var groupsDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_groups,
            Groups = new Weaviate.Client.Rest.Dto.Groups
            {
                Group = "groupA",
                GroupType = Weaviate.Client.Rest.Dto.GroupType.Oidc,
            },
        };
        var groupsModel = Permissions.Parse([groupsDto]).Single();
        Assert.True(groupsModel is Permissions.Groups);
        var groupsRes = ((Permissions.Groups)groupsModel).Resource;
        Assert.Equal("groupA", groupsRes.Group);
        Assert.Equal(RbacGroupType.Oidc, groupsRes.GroupType);

        // Tenants
        var tenantsDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_tenants,
            Tenants = new Weaviate.Client.Rest.Dto.Tenants
            {
                Collection = "tenant-collection",
                Tenant = "tenant2",
            },
        };
        var tenantsModel = Permissions.Parse([tenantsDto]).Single();
        Assert.True(tenantsModel is Permissions.Tenants);
        var tenantsRes = ((Permissions.Tenants)tenantsModel).Resource;
        Assert.Equal("tenant-collection", tenantsRes.Collection);
        Assert.Equal("tenant2", tenantsRes.Tenant);

        // Roles
        var rolesDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_roles,
            Roles = new Weaviate.Client.Rest.Dto.Roles
            {
                Role = "roleA",
                Scope = Weaviate.Client.Rest.Dto.RolesScope.Match,
            },
        };
        var rolesModel = Permissions.Parse([rolesDto]).Single();
        Assert.True(rolesModel is Permissions.Roles);
        var rolesRes = ((Permissions.Roles)rolesModel).Resource;
        Assert.Equal("roleA", rolesRes.Role);
        Assert.Equal(RolesScope.Match, rolesRes.Scope);

        // Collections
        var collectionsDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_collections,
            Collections = new Weaviate.Client.Rest.Dto.Collections { Collection = "collA" },
        };
        var collectionsModel = Permissions.Parse([collectionsDto]).Single();
        Assert.True(collectionsModel is Permissions.Collections);
        Assert.Equal("collA", ((Permissions.Collections)collectionsModel).Resource.Collection);

        // Replicate
        var replicateDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_replicate,
            Replicate = new Weaviate.Client.Rest.Dto.Replicate
            {
                Collection = "replicate-collection",
                Shard = "shardA",
            },
        };
        var replicateModel = Permissions.Parse([replicateDto]).Single();
        Assert.True(replicateModel is Permissions.Replicate);
        var replicateRes = ((Permissions.Replicate)replicateModel).Resource;
        Assert.Equal("replicate-collection", replicateRes.Collection);
        Assert.Equal("shardA", replicateRes.Shard);

        // Aliases
        var aliasesDto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_aliases,
            Aliases = new Weaviate.Client.Rest.Dto.Aliases
            {
                Collection = "alias-collection",
                Alias = "aliasA",
            },
        };
        var aliasesModel = Permissions.Parse([aliasesDto]).Single();
        Assert.True(aliasesModel is Permissions.Alias);
        var aliasesRes = ((Permissions.Alias)aliasesModel).Resource;
        Assert.Equal("alias-collection", aliasesRes.Collection);
        Assert.Equal("aliasA", aliasesRes.Alias);
    }
}

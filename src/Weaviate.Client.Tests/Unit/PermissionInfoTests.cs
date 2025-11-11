using System.Net;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public class PermissionInfoTests
{
    [Fact]
    public void CustomAction_ConstructorSetsCustomEnum()
    {
        var info = new PermissionInfo("future_new_action");
        Assert.Equal("future_new_action", info.ActionRaw);
        Assert.Equal(RbacPermissionAction.Custom, info.Action);
    }

    [Fact]
    public async Task RolesClient_CreateRole_PipelineMapsPermissionInfo()
    {
        // Arrange: mock client & queue server response for role creation.
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();
        var roleName = $"role-{Guid.NewGuid():N}";
        var roleResponse = new
        {
            name = roleName,
            permissions = new object[] { new { action = "read_roles" } },
        };
        // Response to POST /v1/authz/roles (201 Created)
        handler.AddJsonResponse(
            roleResponse,
            expectedEndpoint: "/v1/authz/roles",
            statusCode: HttpStatusCode.Created
        );
        // Follow-up GET /v1/authz/roles/{id} response (200 OK)
        handler.AddJsonResponse(
            roleResponse,
            expectedEndpoint: $"/v1/authz/roles/{roleName}",
            statusCode: HttpStatusCode.OK
        );

        // Act: create role with known enum permission.
        var created = await client.Roles.Create(
            roleName,
            new[] { new Permissions.Roles(new RolesResource(roleName, null)) { Read = true } }
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
    public async Task RolesClient_UnknownString_MapsToCustom()
    {
        // NOTE: Due to strict enum deserialization in the generated REST DTOs, an unknown
        // server action string currently causes a JsonException. We cannot pass the pipeline end-to-end
        // until the server action list is regenerated. This test documents forward-compat mapping logic
        // by emulating the post-deserialization step: taking a raw action string and producing a PermissionInfo
        // with Action=Custom.
        const string futureAction = "future_new_action";
        var info = new PermissionInfo(futureAction);
        Assert.Equal(RbacPermissionAction.Custom, info.Action);
        Assert.Equal(futureAction, info.ActionRaw);

        await Task.CompletedTask;
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
            await client.Roles.Get(roleName)
        );
        Assert.Contains("future_new_action", ex.Message);
        Assert.Contains("PermissionAction", ex.Message);
        Assert.Contains("client", ex.Message.ToLower());
    }

    [Fact]
    public void PermissionInfo_ToModel_MapsAllResourceScopes()
    {
        var dto = new Weaviate.Client.Rest.Dto.Permission
        {
            Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_data,
            Backups = new Weaviate.Client.Rest.Dto.Backups { Collection = "backup-collection" },
            Data = new Weaviate.Client.Rest.Dto.Data
            {
                Collection = "data-collection",
                Tenant = "tenant1",
                Object = "object1",
            },
            Nodes = new Weaviate.Client.Rest.Dto.Nodes
            {
                Collection = "nodes-collection",
                Verbosity = Weaviate.Client.Rest.Dto.NodesVerbosity.Verbose,
            },
            Users = new Weaviate.Client.Rest.Dto.Users { Users1 = "userA" },
            Groups = new Weaviate.Client.Rest.Dto.Groups
            {
                Group = "groupA",
                GroupType = Weaviate.Client.Rest.Dto.GroupType.Oidc,
            },
            Tenants = new Weaviate.Client.Rest.Dto.Tenants
            {
                Collection = "tenant-collection",
                Tenant = "tenant2",
            },
            Roles = new Weaviate.Client.Rest.Dto.Roles
            {
                Role = "roleA",
                Scope = Weaviate.Client.Rest.Dto.RolesScope.Match,
            },
            Collections = new Weaviate.Client.Rest.Dto.Collections { Collection = "collA" },
            Replicate = new Weaviate.Client.Rest.Dto.Replicate
            {
                Collection = "replicate-collection",
                Shard = "shardA",
            },
            Aliases = new Weaviate.Client.Rest.Dto.Aliases
            {
                Collection = "alias-collection",
                Alias = "aliasA",
            },
        };

        var model = dto.ToModel();
        Assert.Equal("read_data", model.ActionRaw);
        Assert.Equal(RbacPermissionAction.ReadData, model.Action);
        Assert.NotNull(model.Resources);
        var r = model.Resources!;
        Assert.Equal("backup-collection", r.Backups?.Collection);
        Assert.Equal("data-collection", r.Data?.Collection);
        Assert.Equal("tenant1", r.Data?.Tenant);
        Assert.Equal("object1", r.Data?.Object);
        Assert.Equal("nodes-collection", r.Nodes?.Collection);
        Assert.Equal("Verbose", r.Nodes?.Verbosity);
        Assert.Equal("userA", r.Users?.Users);
        Assert.Equal("groupA", r.Groups?.Group);
        Assert.Equal("Oidc", r.Groups?.GroupType);
        Assert.Equal("tenant-collection", r.Tenants?.Collection);
        Assert.Equal("tenant2", r.Tenants?.Tenant);
        Assert.Equal("roleA", r.Roles?.Role);
        Assert.Equal("Match", r.Roles?.Scope);
        Assert.Equal("collA", r.Collections?.Collection);
        Assert.Equal("replicate-collection", r.Replicate?.Collection);
        Assert.Equal("shardA", r.Replicate?.Shard);
        Assert.Equal("alias-collection", r.Aliases?.Collection);
        Assert.Equal("aliasA", r.Aliases?.Alias);
    }
}

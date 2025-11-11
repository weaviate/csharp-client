using Weaviate.Client;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class PermissionsScopeTests
{
    [Fact]
    public void Alias_GroupBy_CorrectlyAggregatesPermissions()
    {
        var infos = new List<PermissionInfo>
        {
            new(
                RbacPermissionAction.CreateAliases,
                new PermissionResource(Aliases: new AliasesResource("colA", "alias1"))
            ),
            new(
                RbacPermissionAction.ReadAliases,
                new PermissionResource(Aliases: new AliasesResource("colA", "alias1"))
            ),
            new(
                RbacPermissionAction.DeleteAliases,
                new PermissionResource(Aliases: new AliasesResource("colA", "alias1"))
            ),
            new(
                RbacPermissionAction.UpdateAliases,
                new PermissionResource(Aliases: new AliasesResource("colA", "alias2"))
            ),
            new(
                RbacPermissionAction.ReadAliases,
                new PermissionResource(Aliases: new AliasesResource("colA", "alias2"))
            ),
            new(
                RbacPermissionAction.CreateAliases,
                new PermissionResource(Aliases: new AliasesResource("colB", "alias3"))
            ),
        };

        var aliases = Permissions.Alias.Parse(infos).Cast<Permissions.Alias>().ToList();

        Assert.Equal(3, aliases.Count);

        var alias1 = aliases.Single(a =>
            a.Resource.Collection == "colA" && a.Resource.Alias == "alias1"
        );
        Assert.True(alias1.Create);
        Assert.True(alias1.Read);
        Assert.True(alias1.Delete);
        Assert.False(alias1.Update);

        var alias2 = aliases.Single(a =>
            a.Resource.Collection == "colA" && a.Resource.Alias == "alias2"
        );
        Assert.False(alias2.Create);
        Assert.True(alias2.Read);
        Assert.True(alias2.Update);
        Assert.False(alias2.Delete);

        var alias3 = aliases.Single(a =>
            a.Resource.Collection == "colB" && a.Resource.Alias == "alias3"
        );
        Assert.True(alias3.Create);
        Assert.False(alias3.Read);
        Assert.False(alias3.Update);
        Assert.False(alias3.Delete);
    }

    [Fact]
    public void Cluster_Aggregates_ReadPermission()
    {
        var infos = new List<PermissionInfo> { new(RbacPermissionAction.ReadCluster, null) };
        var clusters = Permissions.Cluster.Parse(infos).Cast<Permissions.Cluster>().ToList();
        Assert.Single(clusters);
        Assert.True(clusters[0].Read);
    }

    [Fact]
    public void Nodes_Aggregates_ReadPermission()
    {
        var infos = new List<PermissionInfo>
        {
            new(
                RbacPermissionAction.ReadNodes,
                new PermissionResource(Nodes: new NodesResource("colA", "verbose"))
            ),
        };
        var nodes = Permissions.Nodes.Parse(infos).Cast<Permissions.Nodes>().ToList();
        Assert.Single(nodes);
        Assert.True(nodes[0].Read);
    }

    [Fact]
    public void Roles_Aggregates_AllActions()
    {
        var resource = new RolesResource("scopeA", "roleA");
        var infos = new List<PermissionInfo>
        {
            new(RbacPermissionAction.CreateRoles, new PermissionResource(Roles: resource)),
            new(RbacPermissionAction.ReadRoles, new PermissionResource(Roles: resource)),
            new(RbacPermissionAction.UpdateRoles, new PermissionResource(Roles: resource)),
            new(RbacPermissionAction.DeleteRoles, new PermissionResource(Roles: resource)),
        };
        var roles = Permissions.Roles.Parse(infos).Cast<Permissions.Roles>().ToList();
        Assert.Single(roles);
        Assert.True(roles[0].Create);
        Assert.True(roles[0].Read);
        Assert.True(roles[0].Update);
        Assert.True(roles[0].Delete);
    }

    [Fact]
    public void Users_Aggregates_AllActions()
    {
        var resource = new UsersResource("userA");
        var infos = new List<PermissionInfo>
        {
            new(RbacPermissionAction.CreateUsers, new PermissionResource(Users: resource)),
            new(RbacPermissionAction.ReadUsers, new PermissionResource(Users: resource)),
            new(RbacPermissionAction.UpdateUsers, new PermissionResource(Users: resource)),
            new(RbacPermissionAction.DeleteUsers, new PermissionResource(Users: resource)),
            new(RbacPermissionAction.AssignAndRevokeUsers, new PermissionResource(Users: resource)),
        };
        var users = Permissions.Users.Parse(infos).Cast<Permissions.Users>().ToList();
        Assert.Single(users);
        Assert.True(users[0].Create);
        Assert.True(users[0].Read);
        Assert.True(users[0].Update);
        Assert.True(users[0].Delete);
        Assert.True(users[0].AssignAndRevoke);
    }

    [Fact]
    public void Tenants_Aggregates_AllActions()
    {
        var resource = new TenantsResource("tenantA", "scopeA");
        var infos = new List<PermissionInfo>
        {
            new(RbacPermissionAction.CreateTenants, new PermissionResource(Tenants: resource)),
            new(RbacPermissionAction.ReadTenants, new PermissionResource(Tenants: resource)),
            new(RbacPermissionAction.UpdateTenants, new PermissionResource(Tenants: resource)),
            new(RbacPermissionAction.DeleteTenants, new PermissionResource(Tenants: resource)),
        };
        var tenants = Permissions.Tenants.Parse(infos).Cast<Permissions.Tenants>().ToList();
        Assert.Single(tenants);
        Assert.True(tenants[0].Create);
        Assert.True(tenants[0].Read);
        Assert.True(tenants[0].Update);
        Assert.True(tenants[0].Delete);
    }

    [Fact]
    public void Groups_Aggregates_AllActions()
    {
        var resource = new GroupsResource("typeA", "groupA");
        var infos = new List<PermissionInfo>
        {
            new PermissionInfo(
                RbacPermissionAction.AssignAndRevokeGroups,
                new PermissionResource(Groups: resource)
            ),
            new PermissionInfo(
                RbacPermissionAction.ReadGroups,
                new PermissionResource(Groups: resource)
            ),
        };
        var groups = Permissions.Groups.Parse(infos).Cast<Permissions.Groups>().ToList();
        Assert.Single(groups);
        Assert.True(groups[0].AssignAndRevoke);
        Assert.True(groups[0].Read);
    }

    [Fact]
    public void Replicate_Aggregates_AllActions()
    {
        var resource = new ReplicateResource("shardA", "replicateA");
        var infos = new List<PermissionInfo>
        {
            new PermissionInfo(
                RbacPermissionAction.CreateReplicate,
                new PermissionResource(Replicate: resource)
            ),
            new PermissionInfo(
                RbacPermissionAction.ReadReplicate,
                new PermissionResource(Replicate: resource)
            ),
            new PermissionInfo(
                RbacPermissionAction.UpdateReplicate,
                new PermissionResource(Replicate: resource)
            ),
            new PermissionInfo(
                RbacPermissionAction.DeleteReplicate,
                new PermissionResource(Replicate: resource)
            ),
        };
        var replicate = Permissions.Replicate.Parse(infos).Cast<Permissions.Replicate>().ToList();
        Assert.Single(replicate);
        Assert.True(replicate[0].Create);
        Assert.True(replicate[0].Read);
        Assert.True(replicate[0].Update);
        Assert.True(replicate[0].Delete);
    }

    [Fact]
    public void Collections_Aggregates_AllActions()
    {
        var resource = new CollectionsResource("collectionA");
        var infos = new List<PermissionInfo>
        {
            new(
                RbacPermissionAction.CreateCollections,
                new PermissionResource(Collections: resource)
            ),
            new(
                RbacPermissionAction.ReadCollections,
                new PermissionResource(Collections: resource)
            ),
            new(
                RbacPermissionAction.UpdateCollections,
                new PermissionResource(Collections: resource)
            ),
            new(
                RbacPermissionAction.DeleteCollections,
                new PermissionResource(Collections: resource)
            ),
        };
        var collections = Permissions
            .Collections.Parse(infos)
            .Cast<Permissions.Collections>()
            .ToList();
        Assert.Single(collections);
        Assert.True(collections[0].Create);
        Assert.True(collections[0].Read);
        Assert.True(collections[0].Update);
        Assert.True(collections[0].Delete);
    }

    [Fact]
    public void Backups_Aggregates_ManageBackupsOnly()
    {
        var resource = new BackupsResource("backup1");
        var infos = new List<PermissionInfo>
        {
            new(RbacPermissionAction.ManageBackups, new PermissionResource(Backups: resource)),
        };
        var backups = Permissions.Backups.Parse(infos).Cast<Permissions.Backups>().ToList();
        Assert.Single(backups);
        Assert.True(backups[0].Manage);
    }

    [Fact]
    public void AllPermissionActions_AreMentioned()
    {
        // This test ensures every valid permission action is covered in the suite
        var allActions = Enum.GetValues(typeof(RbacPermissionAction))
            .Cast<RbacPermissionAction>()
            .ToList();
        // Exclude deprecated or custom actions if present
        allActions.RemoveAll(a =>
            a.ToEnumMemberString() == "custom"
            || a.ToEnumMemberString() == "create_backup"
            || a.ToEnumMemberString() == "read_backup"
            || a.ToEnumMemberString() == "update_backup"
            || a.ToEnumMemberString() == "delete_backup"
        );
        // List of actions covered by tests
        var testedActions = new HashSet<string>
        {
            "manage_backups",
            "read_cluster",
            "create_data",
            "read_data",
            "update_data",
            "delete_data",
            "read_nodes",
            "create_roles",
            "read_roles",
            "update_roles",
            "delete_roles",
            "create_collections",
            "read_collections",
            "update_collections",
            "delete_collections",
            "assign_and_revoke_users",
            "create_users",
            "read_users",
            "update_users",
            "delete_users",
            "create_tenants",
            "read_tenants",
            "update_tenants",
            "delete_tenants",
            "create_replicate",
            "read_replicate",
            "update_replicate",
            "delete_replicate",
            "create_aliases",
            "read_aliases",
            "update_aliases",
            "delete_aliases",
            "assign_and_revoke_groups",
            "read_groups",
        };
        foreach (var action in allActions)
        {
            Assert.Contains(action.ToEnumMemberString(), testedActions);
        }
    }

    [Fact]
    public void Data_Aggregates_AllActions()
    {
        var resource = new DataResource("colA", null, null);
        var infos = new List<PermissionInfo>
        {
            new(RbacPermissionAction.CreateData, new PermissionResource(Data: resource)),
            new(RbacPermissionAction.ReadData, new PermissionResource(Data: resource)),
            new(RbacPermissionAction.UpdateData, new PermissionResource(Data: resource)),
            new(RbacPermissionAction.DeleteData, new PermissionResource(Data: resource)),
        };
        var data = Permissions.Data.Parse(infos).Cast<Permissions.Data>().ToList();
        Assert.Single(data);
        Assert.True(data[0].Create);
        Assert.True(data[0].Read);
        Assert.True(data[0].Update);
        Assert.True(data[0].Delete);
    }
}

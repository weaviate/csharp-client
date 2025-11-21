using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class PermissionsScopeTests
{
    [Fact]
    public void Alias_GroupBy_CorrectlyAggregatesPermissions()
    {
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_aliases,
                Aliases = new Weaviate.Client.Rest.Dto.Aliases
                {
                    Collection = "colA",
                    Alias = "alias1",
                },
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_aliases,
                Aliases = new Weaviate.Client.Rest.Dto.Aliases
                {
                    Collection = "colA",
                    Alias = "alias1",
                },
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Delete_aliases,
                Aliases = new Weaviate.Client.Rest.Dto.Aliases
                {
                    Collection = "colA",
                    Alias = "alias1",
                },
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Update_aliases,
                Aliases = new Weaviate.Client.Rest.Dto.Aliases
                {
                    Collection = "colA",
                    Alias = "alias2",
                },
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_aliases,
                Aliases = new Weaviate.Client.Rest.Dto.Aliases
                {
                    Collection = "colA",
                    Alias = "alias2",
                },
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_aliases,
                Aliases = new Weaviate.Client.Rest.Dto.Aliases
                {
                    Collection = "colB",
                    Alias = "alias3",
                },
            },
        };

        var aliases = Permissions.Alias.Parse(permissions).Cast<Permissions.Alias>().ToList();

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
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new() { Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_cluster },
        };
        var clusters = Permissions.Cluster.Parse(permissions).Cast<Permissions.Cluster>().ToList();
        Assert.Single(clusters);
        Assert.True(clusters[0].Read);
    }

    [Fact]
    public void Nodes_Aggregates_ReadPermission()
    {
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_nodes,
                Nodes = new Weaviate.Client.Rest.Dto.Nodes
                {
                    Collection = "colA",
                    Verbosity = Weaviate.Client.Rest.Dto.NodesVerbosity.Verbose,
                },
            },
        };
        var nodes = Permissions.Nodes.Parse(permissions).Cast<Permissions.Nodes>().ToList();
        Assert.Single(nodes);
        Assert.True(nodes[0].Read);
    }

    [Fact]
    public void Roles_Aggregates_AllActions()
    {
        var resource = new Weaviate.Client.Rest.Dto.Roles
        {
            Role = "roleA",
            Scope = Weaviate.Client.Rest.Dto.RolesScope.Match,
        };
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_roles,
                Roles = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_roles,
                Roles = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Update_roles,
                Roles = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Delete_roles,
                Roles = resource,
            },
        };
        var roles = Permissions.Roles.Parse(permissions).Cast<Permissions.Roles>().ToList();
        Assert.Single(roles);
        Assert.True(roles[0].Create);
        Assert.True(roles[0].Read);
        Assert.True(roles[0].Update);
        Assert.True(roles[0].Delete);
    }

    [Fact]
    public void Users_Aggregates_AllActions()
    {
        var resource = new Weaviate.Client.Rest.Dto.Users { Users1 = "userA" };
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_users,
                Users = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_users,
                Users = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Update_users,
                Users = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Delete_users,
                Users = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Assign_and_revoke_users,
                Users = resource,
            },
        };
        var users = Permissions.Users.Parse(permissions).Cast<Permissions.Users>().ToList();
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
        var resource = new Weaviate.Client.Rest.Dto.Tenants
        {
            Collection = "tenantA",
            Tenant = "scopeA",
        };
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_tenants,
                Tenants = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_tenants,
                Tenants = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Update_tenants,
                Tenants = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Delete_tenants,
                Tenants = resource,
            },
        };
        var tenants = Permissions.Tenants.Parse(permissions).Cast<Permissions.Tenants>().ToList();
        Assert.Single(tenants);
        Assert.True(tenants[0].Create);
        Assert.True(tenants[0].Read);
        Assert.True(tenants[0].Update);
        Assert.True(tenants[0].Delete);
    }

    [Fact]
    public void Groups_Aggregates_AllActions()
    {
        var resource = new Weaviate.Client.Rest.Dto.Groups
        {
            Group = "groupA",
            GroupType = Weaviate.Client.Rest.Dto.GroupType.Oidc,
        };
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Assign_and_revoke_groups,
                Groups = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_groups,
                Groups = resource,
            },
        };
        var groups = Permissions.Groups.Parse(permissions).Cast<Permissions.Groups>().ToList();
        Assert.Single(groups);
        Assert.True(groups[0].AssignAndRevoke);
        Assert.True(groups[0].Read);
    }

    [Fact]
    public void Replicate_Aggregates_AllActions()
    {
        var resource = new Weaviate.Client.Rest.Dto.Replicate
        {
            Collection = "shardA",
            Shard = "replicateA",
        };
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_replicate,
                Replicate = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_replicate,
                Replicate = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Update_replicate,
                Replicate = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Delete_replicate,
                Replicate = resource,
            },
        };
        var replicate = Permissions
            .Replicate.Parse(permissions)
            .Cast<Permissions.Replicate>()
            .ToList();
        Assert.Single(replicate);
        Assert.True(replicate[0].Create);
        Assert.True(replicate[0].Read);
        Assert.True(replicate[0].Update);
        Assert.True(replicate[0].Delete);
    }

    [Fact]
    public void Collections_Aggregates_AllActions()
    {
        var resource = new Weaviate.Client.Rest.Dto.Collections { Collection = "collectionA" };
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_collections,
                Collections = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_collections,
                Collections = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Update_collections,
                Collections = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Delete_collections,
                Collections = resource,
            },
        };
        var collections = Permissions
            .Collections.Parse(permissions)
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
        var resource = new Weaviate.Client.Rest.Dto.Backups { Collection = "backup1" };
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Manage_backups,
                Backups = resource,
            },
        };
        var backups = Permissions.Backups.Parse(permissions).Cast<Permissions.Backups>().ToList();
        Assert.Single(backups);
        Assert.True(backups[0].Manage);
    }

    [Fact]
    public void AllPermissionActions_AreMentioned()
    {
        // This test ensures every valid permission action is covered in the suite
        var allActions = Enum.GetValues(typeof(Weaviate.Client.Rest.Dto.PermissionAction))
            .Cast<Weaviate.Client.Rest.Dto.PermissionAction>()
            .ToList();
        // Exclude deprecated or custom actions if present
        allActions.RemoveAll(a =>
            a.ToString() == "Custom"
            || a.ToString() == "Create_backup"
            || a.ToString() == "Read_backup"
            || a.ToString() == "Update_backup"
            || a.ToString() == "Delete_backup"
        );
        // List of actions covered by tests
        var testedActions = new HashSet<string>
        {
            "Manage_backups",
            "Read_cluster",
            "Create_data",
            "Read_data",
            "Update_data",
            "Delete_data",
            "Read_nodes",
            "Create_roles",
            "Read_roles",
            "Update_roles",
            "Delete_roles",
            "Create_collections",
            "Read_collections",
            "Update_collections",
            "Delete_collections",
            "Assign_and_revoke_users",
            "Create_users",
            "Read_users",
            "Update_users",
            "Delete_users",
            "Create_tenants",
            "Read_tenants",
            "Update_tenants",
            "Delete_tenants",
            "Create_replicate",
            "Read_replicate",
            "Update_replicate",
            "Delete_replicate",
            "Create_aliases",
            "Read_aliases",
            "Update_aliases",
            "Delete_aliases",
            "Assign_and_revoke_groups",
            "Read_groups",
        };
        foreach (var action in allActions)
        {
            Assert.Contains(action.ToString(), testedActions);
        }
    }

    [Fact]
    public void Data_Aggregates_AllActions()
    {
        var resource = new Weaviate.Client.Rest.Dto.Data { Collection = "colA" };
        var permissions = new List<Weaviate.Client.Rest.Dto.Permission>
        {
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Create_data,
                Data = resource,
            },
            new() { Action = Weaviate.Client.Rest.Dto.PermissionAction.Read_data, Data = resource },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Update_data,
                Data = resource,
            },
            new()
            {
                Action = Weaviate.Client.Rest.Dto.PermissionAction.Delete_data,
                Data = resource,
            },
        };
        var data = Permissions.Data.Parse(permissions).Cast<Permissions.Data>().ToList();
        Assert.Single(data);
        Assert.True(data[0].Create);
        Assert.True(data[0].Read);
        Assert.True(data[0].Update);
        Assert.True(data[0].Delete);
    }
}

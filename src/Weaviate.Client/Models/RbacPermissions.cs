namespace Weaviate.Client.Models;

using System.Linq;

public abstract class PermissionScope
{
    public static IEnumerable<PermissionScope> Empty() => Array.Empty<PermissionScope>();

    internal abstract IEnumerable<Rest.Dto.Permission> ToDto();
}

public static class Permissions
{
    public class Alias : PermissionScope
    {
        public AliasesResource Resource { get; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        public Alias(string? collection, string? alias)
            : this(new AliasesResource(collection, alias)) { }

        Alias(AliasesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Create_aliases, Allowed: Create),
                (Action: Rest.Dto.PermissionAction.Read_aliases, Allowed: Read),
                (Action: Rest.Dto.PermissionAction.Update_aliases, Allowed: Update),
                (Action: Rest.Dto.PermissionAction.Delete_aliases, Allowed: Delete),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Aliases = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Aliases != null)
                .GroupBy(i => i.Aliases!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Data : PermissionScope
    {
        public DataResource Resource { get; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        public Data(string? collection, string? tenant = "*")
            : this(new DataResource(collection, tenant)) { }

        Data(DataResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Create_data, Allowed: Create),
                (Action: Rest.Dto.PermissionAction.Read_data, Allowed: Read),
                (Action: Rest.Dto.PermissionAction.Update_data, Allowed: Update),
                (Action: Rest.Dto.PermissionAction.Delete_data, Allowed: Delete),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Data = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Data != null)
                .GroupBy(i => i.Data!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Backups : PermissionScope
    {
        public BackupsResource Resource { get; }
        public bool Manage { get; set; }

        public Backups(string? collection)
            : this(new BackupsResource(collection)) { }

        Backups(BackupsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Manage_backups, Allowed: Manage),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Backups = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Backups != null)
                .GroupBy(i => i.Backups!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Cluster : PermissionScope
    {
        public bool Read { get; set; }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            if (Read)
                yield return new Rest.Dto.Permission()
                {
                    Action = Rest.Dto.PermissionAction.Read_cluster,
                };
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            var cluster = new Cluster()
            {
                Read = infos.Any(i => i.Action == Rest.Dto.PermissionAction.Read_cluster),
            };

            if (!cluster.Read)
            {
                return [];
            }

            return [cluster];
        }
    }

    public class Nodes : PermissionScope
    {
        public NodesResource Resource { get; }

        public Nodes(string? collection, NodeVerbosity? verbosity)
            : this(new NodesResource(collection, verbosity)) { }

        Nodes(NodesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Read { get; set; }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Read_nodes, Allowed: Read),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Nodes = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Nodes != null)
                .GroupBy(i => i.Nodes!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Roles : PermissionScope
    {
        public RolesResource Resource { get; }

        public Roles(string? name, RolesScope? scope = null)
            : this(new RolesResource(name, scope)) { }

        Roles(RolesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Create_roles, Allowed: Create),
                (Action: Rest.Dto.PermissionAction.Read_roles, Allowed: Read),
                (Action: Rest.Dto.PermissionAction.Update_roles, Allowed: Update),
                (Action: Rest.Dto.PermissionAction.Delete_roles, Allowed: Delete),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Roles = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Roles != null)
                .GroupBy(i => i.Roles!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Users : PermissionScope
    {
        public UsersResource Resource { get; }

        public Users()
            : this(new UsersResource(null)) { }

        public Users(string? name)
            : this(new UsersResource(name)) { }

        Users(UsersResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool AssignAndRevoke { get; set; }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Create_users, Allowed: Create),
                (Action: Rest.Dto.PermissionAction.Read_users, Allowed: Read),
                (Action: Rest.Dto.PermissionAction.Update_users, Allowed: Update),
                (Action: Rest.Dto.PermissionAction.Delete_users, Allowed: Delete),
                (
                    Action: Rest.Dto.PermissionAction.Assign_and_revoke_users,
                    Allowed: AssignAndRevoke
                ),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Users = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Users != null)
                .GroupBy(i => i.Users!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Tenants : PermissionScope
    {
        public TenantsResource Resource { get; }

        public Tenants(string? collection, string? tenant)
            : this(new TenantsResource(collection, tenant)) { }

        Tenants(TenantsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Create_tenants, Allowed: Create),
                (Action: Rest.Dto.PermissionAction.Read_tenants, Allowed: Read),
                (Action: Rest.Dto.PermissionAction.Update_tenants, Allowed: Update),
                (Action: Rest.Dto.PermissionAction.Delete_tenants, Allowed: Delete),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Tenants = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Tenants != null)
                .GroupBy(i => i.Tenants!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Groups : PermissionScope
    {
        public GroupsResource Resource { get; }

        public Groups(string? group, RbacGroupType? groupType)
            : this(new GroupsResource(group, groupType)) { }

        Groups(GroupsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool AssignAndRevoke { get; set; }
        public bool Read { get; set; }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (
                    Action: Rest.Dto.PermissionAction.Assign_and_revoke_groups,
                    Allowed: AssignAndRevoke
                ),
                (Action: Rest.Dto.PermissionAction.Read_groups, Allowed: Read),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Groups = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Groups != null)
                .GroupBy(i => i.Groups!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Replicate : PermissionScope
    {
        public ReplicateResource Resource { get; }

        public Replicate(string? collection, string? shard)
            : this(new ReplicateResource(collection, shard)) { }

        Replicate(ReplicateResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Create_replicate, Allowed: Create),
                (Action: Rest.Dto.PermissionAction.Read_replicate, Allowed: Read),
                (Action: Rest.Dto.PermissionAction.Update_replicate, Allowed: Update),
                (Action: Rest.Dto.PermissionAction.Delete_replicate, Allowed: Delete),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Replicate = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Replicate != null)
                .GroupBy(i => i.Replicate!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    public class Collections : PermissionScope
    {
        public CollectionsResource Resource { get; }

        public Collections(string? collection)
            : this(new CollectionsResource(collection)) { }

        Collections(CollectionsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Create_collections, Allowed: Create),
                (Action: Rest.Dto.PermissionAction.Read_collections, Allowed: Read),
                (Action: Rest.Dto.PermissionAction.Update_collections, Allowed: Update),
                (Action: Rest.Dto.PermissionAction.Delete_collections, Allowed: Delete),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission
                {
                    Collections = Resource.ToDto(),
                    Action = p.Action,
                });
        }

        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Collections != null)
                .GroupBy(i => i.Collections!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
    {
        var scopes = new List<PermissionScope>();
        scopes.AddRange(Alias.Parse(infos));
        scopes.AddRange(Data.Parse(infos));
        scopes.AddRange(Backups.Parse(infos));
        scopes.AddRange(Cluster.Parse(infos));
        scopes.AddRange(Nodes.Parse(infos));
        scopes.AddRange(Roles.Parse(infos));
        scopes.AddRange(Users.Parse(infos));
        scopes.AddRange(Tenants.Parse(infos));
        scopes.AddRange(Groups.Parse(infos));
        scopes.AddRange(Replicate.Parse(infos));
        scopes.AddRange(Collections.Parse(infos));
        return scopes;
    }
}

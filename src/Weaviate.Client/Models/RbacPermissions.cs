namespace Weaviate.Client.Models;

using System.Linq;

public abstract class PermissionScope
{
    public static IEnumerable<PermissionScope> Empty() => Array.Empty<PermissionScope>();

    internal abstract IEnumerable<PermissionInfo> ToDto();
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

        public Alias(string? name, string? collection)
            : this(new AliasesResource(name, collection)) { }

        public Alias(AliasesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Aliases: Resource);
            if (Create)
            {
                yield return new PermissionInfo(RbacPermissionAction.CreateAliases, permResource);
            }
            if (Read)
            {
                yield return new PermissionInfo(RbacPermissionAction.ReadAliases, permResource);
            }
            if (Update)
            {
                yield return new PermissionInfo(RbacPermissionAction.UpdateAliases, permResource);
            }
            if (Delete)
            {
                yield return new PermissionInfo(RbacPermissionAction.DeleteAliases, permResource);
            }
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Aliases != null)
                .GroupBy(i => i.Resources!.Aliases!)
                .Select(group =>
                {
                    var aliasPerm = new Alias(group.Key);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.CreateAliases:
                                aliasPerm.Create = true;
                                break;
                            case RbacPermissionAction.ReadAliases:
                                aliasPerm.Read = true;
                                break;
                            case RbacPermissionAction.UpdateAliases:
                                aliasPerm.Update = true;
                                break;
                            case RbacPermissionAction.DeleteAliases:
                                aliasPerm.Delete = true;
                                break;
                        }
                    }
                    return (PermissionScope)aliasPerm;
                })
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

        public Data(string? name, string? collection, string? @object)
            : this(new DataResource(name, collection, @object)) { }

        public Data(DataResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Data: Resource);
            if (Create)
            {
                yield return new PermissionInfo(RbacPermissionAction.CreateData, permResource);
            }
            if (Read)
            {
                yield return new PermissionInfo(RbacPermissionAction.ReadData, permResource);
            }
            if (Update)
            {
                yield return new PermissionInfo(RbacPermissionAction.UpdateData, permResource);
            }
            if (Delete)
            {
                yield return new PermissionInfo(RbacPermissionAction.DeleteData, permResource);
            }
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Data != null)
                .GroupBy(i => i.Resources!.Data!)
                .Select(group =>
                {
                    var dataPerm = new Data(group.Key);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.CreateData:
                                dataPerm.Create = true;
                                break;
                            case RbacPermissionAction.ReadData:
                                dataPerm.Read = true;
                                break;
                            case RbacPermissionAction.UpdateData:
                                dataPerm.Update = true;
                                break;
                            case RbacPermissionAction.DeleteData:
                                dataPerm.Delete = true;
                                break;
                        }
                    }
                    return (PermissionScope)dataPerm;
                })
                .ToList();
        }
    }

    public class Backups : PermissionScope
    {
        public BackupsResource Resource { get; }
        public bool Manage { get; set; }

        public Backups(string? collection)
            : this(new BackupsResource(collection)) { }

        public Backups(BackupsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Backups: Resource);
            if (Manage)
            {
                yield return new PermissionInfo(RbacPermissionAction.ManageBackups, permResource);
            }
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Backups != null)
                .GroupBy(i => i.Resources!.Backups!)
                .Select(group =>
                {
                    var backupPerm = new Backups(group.Key);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.ManageBackups:
                                backupPerm.Manage = true;
                                break;
                        }
                    }
                    return (PermissionScope)backupPerm;
                })
                .ToList();
        }
    }

    public class Cluster : PermissionScope
    {
        public bool Read { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadCluster);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            var cluster = new Cluster();
            foreach (var info in infos)
            {
                switch (info.Action)
                {
                    case RbacPermissionAction.ReadCluster:
                        cluster.Read = true;
                        return new List<PermissionScope> { cluster };
                }
            }

            return new();
        }
    }

    public class Nodes : PermissionScope
    {
        public NodesResource Resource { get; }

        public Nodes(string? collection, string? verbosity)
            : this(new NodesResource(collection, verbosity)) { }

        public Nodes(NodesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Read { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Nodes: Resource);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadNodes, permResource);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Nodes != null)
                .GroupBy(i => i.Resources!.Nodes!)
                .Select(group =>
                {
                    var nodesPerm = new Nodes(group.Key);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.ReadNodes:
                                nodesPerm.Read = true;
                                break;
                        }
                    }
                    return (PermissionScope)nodesPerm;
                })
                .ToList();
        }
    }

    public class Roles : PermissionScope
    {
        public RolesResource Resource { get; }

        public Roles(string? name, string? scope = null)
            : this(new RolesResource(name, scope)) { }

        public Roles(RolesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Roles: Resource);
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateRoles, permResource);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadRoles, permResource);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateRoles, permResource);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteRoles, permResource);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Roles != null)
                .GroupBy(i => i.Resources!.Roles!)
                .Select(group =>
                {
                    var roles = new Roles(group.Key!);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.CreateRoles:
                                roles.Create = true;
                                break;
                            case RbacPermissionAction.ReadRoles:
                                roles.Read = true;
                                break;
                            case RbacPermissionAction.UpdateRoles:
                                roles.Update = true;
                                break;
                            case RbacPermissionAction.DeleteRoles:
                                roles.Delete = true;
                                break;
                        }
                    }
                    return (PermissionScope)roles;
                })
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

        public Users(UsersResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool AssignAndRevoke { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Users: Resource);
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateUsers, permResource);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadUsers, permResource);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateUsers, permResource);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteUsers, permResource);
            if (AssignAndRevoke)
                yield return new PermissionInfo(
                    RbacPermissionAction.AssignAndRevokeUsers,
                    permResource
                );
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Users != null)
                .GroupBy(i => i.Resources!.Users!)
                .Select(group =>
                {
                    var users = new Users(group.Key!);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.CreateUsers:
                                users.Create = true;
                                break;
                            case RbacPermissionAction.ReadUsers:
                                users.Read = true;
                                break;
                            case RbacPermissionAction.UpdateUsers:
                                users.Update = true;
                                break;
                            case RbacPermissionAction.DeleteUsers:
                                users.Delete = true;
                                break;
                            case RbacPermissionAction.AssignAndRevokeUsers:
                                users.AssignAndRevoke = true;
                                break;
                        }
                    }
                    return (PermissionScope)users;
                })
                .ToList();
        }
    }

    public class Tenants : PermissionScope
    {
        public TenantsResource Resource { get; }

        public Tenants(string? collection, string? tenant)
            : this(new TenantsResource(collection, tenant)) { }

        public Tenants(TenantsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Tenants: Resource);
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateTenants, permResource);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadTenants, permResource);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateTenants, permResource);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteTenants, permResource);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Tenants != null)
                .GroupBy(i => i.Resources!.Tenants!)
                .Select(group =>
                {
                    var tenants = new Tenants(group.Key!);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.CreateTenants:
                                tenants.Create = true;
                                break;
                            case RbacPermissionAction.ReadTenants:
                                tenants.Read = true;
                                break;
                            case RbacPermissionAction.UpdateTenants:
                                tenants.Update = true;
                                break;
                            case RbacPermissionAction.DeleteTenants:
                                tenants.Delete = true;
                                break;
                        }
                    }
                    return (PermissionScope)tenants;
                })
                .ToList();
        }
    }

    public class Groups : PermissionScope
    {
        public GroupsResource Resource { get; }

        public Groups(string? group, string? groupType)
            : this(new GroupsResource(group, groupType)) { }

        public Groups(GroupsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool AssignAndRevoke { get; set; }
        public bool Read { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Groups: Resource);
            if (AssignAndRevoke)
                yield return new PermissionInfo(
                    RbacPermissionAction.AssignAndRevokeGroups,
                    permResource
                );
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadGroups, permResource);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Groups != null)
                .GroupBy(i => i.Resources!.Groups!)
                .Select(group =>
                {
                    var groupsPerm = new Groups(group.Key!);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.AssignAndRevokeGroups:
                                groupsPerm.AssignAndRevoke = true;
                                break;
                            case RbacPermissionAction.ReadGroups:
                                groupsPerm.Read = true;
                                break;
                        }
                    }
                    return (PermissionScope)groupsPerm;
                })
                .ToList();
        }
    }

    public class Replicate : PermissionScope
    {
        public ReplicateResource Resource { get; }

        public Replicate(string? collection, string? shard)
            : this(new ReplicateResource(collection, shard)) { }

        public Replicate(ReplicateResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Replicate: Resource);
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateReplicate, permResource);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadReplicate, permResource);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateReplicate, permResource);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteReplicate, permResource);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Replicate != null)
                .GroupBy(i => i.Resources!.Replicate!)
                .Select(group =>
                {
                    var replicate = new Replicate(group.Key);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.CreateReplicate:
                                replicate.Create = true;
                                break;
                            case RbacPermissionAction.ReadReplicate:
                                replicate.Read = true;
                                break;
                            case RbacPermissionAction.UpdateReplicate:
                                replicate.Update = true;
                                break;
                            case RbacPermissionAction.DeleteReplicate:
                                replicate.Delete = true;
                                break;
                        }
                    }
                    return (PermissionScope)replicate;
                })
                .ToList();
        }
    }

    public class Collections : PermissionScope
    {
        public CollectionsResource Resource { get; }

        public Collections(string? collection)
            : this(new CollectionsResource(collection)) { }

        public Collections(CollectionsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            var permResource = new PermissionResource(Collections: Resource);
            if (Create)
                yield return new PermissionInfo(
                    RbacPermissionAction.CreateCollections,
                    permResource
                );
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadCollections, permResource);
            if (Update)
                yield return new PermissionInfo(
                    RbacPermissionAction.UpdateCollections,
                    permResource
                );
            if (Delete)
                yield return new PermissionInfo(
                    RbacPermissionAction.DeleteCollections,
                    permResource
                );
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            return infos
                .Where(i => i.Resources?.Collections != null)
                .GroupBy(i => i.Resources!.Collections!)
                .Select(group =>
                {
                    var collections = new Collections(group.Key!);
                    foreach (var info in group)
                    {
                        switch (info.Action)
                        {
                            case RbacPermissionAction.CreateCollections:
                                collections.Create = true;
                                break;
                            case RbacPermissionAction.ReadCollections:
                                collections.Read = true;
                                break;
                            case RbacPermissionAction.UpdateCollections:
                                collections.Update = true;
                                break;
                            case RbacPermissionAction.DeleteCollections:
                                collections.Delete = true;
                                break;
                        }
                    }
                    return (PermissionScope)collections;
                })
                .ToList();
        }
    }

    internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
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

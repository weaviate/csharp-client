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
                        break;
                }
            }
            return cluster.Read
                ? new List<PermissionScope> { cluster }
                : new List<PermissionScope>();
        }
    }

    public class Nodes : PermissionScope
    {
        public NodesResource Resource { get; }

        public Nodes(NodesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool Read { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadNodes);
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

        public Roles()
            : this(new RolesResource(string.Empty, null)) { }

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
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateRoles);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadRoles);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateRoles);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteRoles);
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
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateUsers);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadUsers);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateUsers);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteUsers);
            if (AssignAndRevoke)
                yield return new PermissionInfo(RbacPermissionAction.AssignAndRevokeUsers);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            var users = new Users();
            foreach (var info in infos)
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
            return (
                users.Create || users.Read || users.Update || users.Delete || users.AssignAndRevoke
            )
                ? new List<PermissionScope> { users }
                : new List<PermissionScope>();
        }
    }

    public class Tenants : PermissionScope
    {
        public TenantsResource Resource { get; }

        public Tenants()
            : this(new TenantsResource(null, null)) { }

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
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateTenants);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadTenants);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateTenants);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteTenants);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            var tenants = new Tenants();
            foreach (var info in infos)
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
            return (tenants.Create || tenants.Read || tenants.Update || tenants.Delete)
                ? new List<PermissionScope> { tenants }
                : new List<PermissionScope>();
        }
    }

    public class Groups : PermissionScope
    {
        public GroupsResource Resource { get; }

        public Groups()
            : this(new GroupsResource(null, null)) { }

        public Groups(GroupsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public bool AssignAndRevoke { get; set; }
        public bool Read { get; set; }

        internal override IEnumerable<PermissionInfo> ToDto()
        {
            if (AssignAndRevoke)
                yield return new PermissionInfo(RbacPermissionAction.AssignAndRevokeGroups);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadGroups);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            var groups = new Groups();
            foreach (var info in infos)
            {
                switch (info.Action)
                {
                    case RbacPermissionAction.AssignAndRevokeGroups:
                        groups.AssignAndRevoke = true;
                        break;
                    case RbacPermissionAction.ReadGroups:
                        groups.Read = true;
                        break;
                }
            }
            return (groups.AssignAndRevoke || groups.Read)
                ? new List<PermissionScope> { groups }
                : new List<PermissionScope>();
        }
    }

    public class Replicate : PermissionScope
    {
        public ReplicateResource Resource { get; }

        public Replicate()
            : this(new ReplicateResource(null, null)) { }

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
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateReplicate);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadReplicate);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateReplicate);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteReplicate);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            var replicate = new Replicate();
            foreach (var info in infos)
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
            return (replicate.Create || replicate.Read || replicate.Update || replicate.Delete)
                ? new List<PermissionScope> { replicate }
                : new List<PermissionScope>();
        }
    }

    public class Collections : PermissionScope
    {
        public CollectionsResource Resource { get; }

        public Collections()
            : this(new CollectionsResource(null)) { }

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
            if (Create)
                yield return new PermissionInfo(RbacPermissionAction.CreateCollections);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadCollections);
            if (Update)
                yield return new PermissionInfo(RbacPermissionAction.UpdateCollections);
            if (Delete)
                yield return new PermissionInfo(RbacPermissionAction.DeleteCollections);
        }

        internal static List<PermissionScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            var collections = new Collections();
            foreach (var info in infos)
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
            return (
                collections.Create || collections.Read || collections.Update || collections.Delete
            )
                ? new List<PermissionScope> { collections }
                : new List<PermissionScope>();
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

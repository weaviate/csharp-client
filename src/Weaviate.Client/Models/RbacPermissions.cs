namespace Weaviate.Client.Models;

using System.Linq;

/// <summary>
/// The permission scope class
/// </summary>
public abstract class PermissionScope
{
    /// <summary>
    /// Empties
    /// </summary>
    /// <returns>An enumerable of permission scope</returns>
    public static IEnumerable<PermissionScope> Empty() => Array.Empty<PermissionScope>();

    /// <summary>
    /// Returns the dto
    /// </summary>
    /// <returns>An enumerable of rest dto permission</returns>
    internal abstract IEnumerable<Rest.Dto.Permission> ToDto();
}

/// <summary>
/// The permissions class
/// </summary>
public static class Permissions
{
    /// <summary>
    /// The alias class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Alias : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public AliasesResource Resource { get; }

        /// <summary>
        /// Gets or sets the value of the create
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the value of the update
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets the value of the delete
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alias"/> class
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <param name="alias">The alias</param>
        public Alias(string? collection, string? alias)
            : this(new AliasesResource(collection, alias)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alias"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Alias(AliasesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Aliases != null)
                .GroupBy(i => i.Aliases!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The data class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Data : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public DataResource Resource { get; }

        /// <summary>
        /// Gets or sets the value of the create
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the value of the update
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets the value of the delete
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> class
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <param name="tenant">The tenant</param>
        public Data(string? collection, string? tenant = "*")
            : this(new DataResource(collection, tenant)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Data(DataResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Data != null)
                .GroupBy(i => i.Data!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The backups class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Backups : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public BackupsResource Resource { get; }

        /// <summary>
        /// Gets or sets the value of the manage
        /// </summary>
        public bool Manage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Backups"/> class
        /// </summary>
        /// <param name="collection">The collection</param>
        public Backups(string? collection)
            : this(new BackupsResource(collection)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Backups"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Backups(BackupsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Backups != null)
                .GroupBy(i => i.Backups!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The mcp class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Mcp : PermissionScope
    {
        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the value of the create
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the value of the update
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            var permissions = new[]
            {
                (Action: Rest.Dto.PermissionAction.Read_mcp, Allowed: Read),
                (Action: Rest.Dto.PermissionAction.Create_mcp, Allowed: Create),
                (Action: Rest.Dto.PermissionAction.Update_mcp, Allowed: Update),
            };

            return permissions
                .Where(p => p.Allowed)
                .Select(p => new Rest.Dto.Permission { Action = p.Action });
        }

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            var actions = infos
                .Where(i =>
                    i.Action == Rest.Dto.PermissionAction.Read_mcp
                    || i.Action == Rest.Dto.PermissionAction.Create_mcp
                    || i.Action == Rest.Dto.PermissionAction.Update_mcp
                )
                .Select(i => i.Action)
                .ToHashSet();

            if (actions.Count == 0)
                return [];

            return
            [
                new Mcp
                {
                    Read = actions.Contains(Rest.Dto.PermissionAction.Read_mcp),
                    Create = actions.Contains(Rest.Dto.PermissionAction.Create_mcp),
                    Update = actions.Contains(Rest.Dto.PermissionAction.Update_mcp),
                },
            ];
        }
    }

    /// <summary>
    /// The cluster class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Cluster : PermissionScope
    {
        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
        internal override IEnumerable<Rest.Dto.Permission> ToDto()
        {
            if (Read)
                yield return new Rest.Dto.Permission()
                {
                    Action = Rest.Dto.PermissionAction.Read_cluster,
                };
        }

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
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

    /// <summary>
    /// The nodes class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Nodes : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public NodesResource Resource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nodes"/> class
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <param name="verbosity">The verbosity</param>
        public Nodes(string? collection, NodeVerbosity? verbosity)
            : this(new NodesResource(collection, verbosity)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nodes"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Nodes(NodesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Nodes != null)
                .GroupBy(i => i.Nodes!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The roles class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Roles : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public RolesResource Resource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Roles"/> class
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="scope">The scope</param>
        public Roles(string? name, RolesScope? scope = null)
            : this(new RolesResource(name, scope)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Roles"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Roles(RolesResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Gets or sets the value of the create
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the value of the update
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets the value of the delete
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Roles != null)
                .GroupBy(i => i.Roles!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The users class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Users : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public UsersResource Resource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Users"/> class
        /// </summary>
        public Users()
            : this(new UsersResource(null)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Users"/> class
        /// </summary>
        /// <param name="name">The name</param>
        public Users(string? name)
            : this(new UsersResource(name)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Users"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Users(UsersResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Gets or sets the value of the create
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the value of the update
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets the value of the delete
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Gets or sets the value of the assign and revoke
        /// </summary>
        public bool AssignAndRevoke { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Users != null)
                .GroupBy(i => i.Users!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The tenants class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Tenants : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public TenantsResource Resource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenants"/> class
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <param name="tenant">The tenant</param>
        public Tenants(string? collection, string? tenant)
            : this(new TenantsResource(collection, tenant)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenants"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Tenants(TenantsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Gets or sets the value of the create
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the value of the update
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets the value of the delete
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Tenants != null)
                .GroupBy(i => i.Tenants!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The groups class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Groups : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public GroupsResource Resource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Groups"/> class
        /// </summary>
        /// <param name="group">The group</param>
        /// <param name="groupType">The group type</param>
        public Groups(string? group, RbacGroupType? groupType)
            : this(new GroupsResource(group, groupType)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Groups"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Groups(GroupsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Gets or sets the value of the assign and revoke
        /// </summary>
        public bool AssignAndRevoke { get; set; }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Groups != null)
                .GroupBy(i => i.Groups!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The replicate class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Replicate : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public ReplicateResource Resource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replicate"/> class
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <param name="shard">The shard</param>
        public Replicate(string? collection, string? shard)
            : this(new ReplicateResource(collection, shard)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replicate"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Replicate(ReplicateResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Gets or sets the value of the create
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the value of the update
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets the value of the delete
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Replicate != null)
                .GroupBy(i => i.Replicate!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// The collections class
    /// </summary>
    /// <seealso cref="PermissionScope"/>
    public class Collections : PermissionScope
    {
        /// <summary>
        /// Gets the value of the resource
        /// </summary>
        public CollectionsResource Resource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Collections"/> class
        /// </summary>
        /// <param name="collection">The collection</param>
        public Collections(string? collection)
            : this(new CollectionsResource(collection)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Collections"/> class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <exception cref="ArgumentNullException"></exception>
        Collections(CollectionsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// Gets or sets the value of the create
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Gets or sets the value of the read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the value of the update
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets or sets the value of the delete
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Returns the dto
        /// </summary>
        /// <returns>An enumerable of rest dto permission</returns>
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

        /// <summary>
        /// Parses the infos
        /// </summary>
        /// <param name="infos">The infos</param>
        /// <returns>A list of permission scope</returns>
        internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
        {
            return infos
                .Where(i => i.Collections != null)
                .GroupBy(i => i.Collections!)
                .Select(group => group.Key.ToModel(group.AsEnumerable()))
                .ToList();
        }
    }

    /// <summary>
    /// Parses the infos
    /// </summary>
    /// <param name="infos">The infos</param>
    /// <returns>The scopes</returns>
    internal static List<PermissionScope> Parse(IEnumerable<Rest.Dto.Permission> infos)
    {
        var scopes = new List<PermissionScope>();
        scopes.AddRange(Alias.Parse(infos));
        scopes.AddRange(Data.Parse(infos));
        scopes.AddRange(Backups.Parse(infos));
        scopes.AddRange(Mcp.Parse(infos));
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

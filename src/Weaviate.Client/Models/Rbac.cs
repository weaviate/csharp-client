namespace Weaviate.Client.Models;

using System.Collections;
using System.Linq;
using System.Runtime.Serialization;

/// <summary>
/// Database user types (subset of underlying DTO enum values)
/// </summary>
public enum DatabaseUserType
{
    DbUser,
    DbEnvUser,
}

/// <summary>
/// RBAC user types for role assignment endpoints.
/// </summary>
public enum RbacUserType
{
    [EnumMember(Value = "db")]
    Database,

    [EnumMember(Value = "oidc")]
    Oidc,
}

/// <summary>
/// RBAC group types for role assignment endpoints.
/// </summary>
public enum RbacGroupType
{
    [EnumMember(Value = "oidc")]
    Oidc,
}

/// <summary>
/// Represents a database user returned by the Users API.
/// </summary>
public record DatabaseUser(
    string UserId,
    bool Active,
    DatabaseUserType DbUserType,
    System.DateTimeOffset? CreatedAt,
    System.DateTimeOffset? LastUsedAt,
    string? ApiKeyFirstLetters,
    IEnumerable<string> Roles
);

/// <summary>
/// Represents the current authenticated user's info (own-info endpoint).
/// </summary>
public record CurrentUserInfo(
    string Username,
    IEnumerable<RoleInfo> Roles,
    IEnumerable<string>? Groups
);

/// <summary>
/// Simplified role representation.
/// </summary>
public record RoleInfo(string Name, IEnumerable<PermissionInfo> Permissions);

/// <summary>
/// Simplified permission representation exposing only the action; full resource scoping is available via Raw.
/// Provides convenience constructor accepting an <see cref="RbacPermissionAction"/> enum value.
/// </summary>
public record PermissionInfo
{
    /// <summary>
    /// The original action string as received from / sent to the REST API.
    /// Always preserved, even if the action is unknown to this client version.
    /// </summary>
    public string ActionRaw { get; }

    /// <summary>
    /// Lazily parsed enum value for the action. If the raw string is not recognized it will be <see cref="RbacPermissionAction.Custom"/>.
    /// Consumers can rely on this to branch logic; when <c>Custom</c>, fall back to inspecting <see cref="ActionRaw"/>.
    /// </summary>
    public RbacPermissionAction Action =>
        ActionRaw.FromEnumMemberString<RbacPermissionAction>(RbacPermissionAction.Custom);

    /// <summary>
    /// The resource scope for this permission, if any. Mirrors the REST DTO structure.
    /// </summary>
    public PermissionResource? Resources { get; }

    /// <summary>
    /// Creates a permission from a raw action string and optional resources. Stores the raw string; Action is resolved lazily.
    /// </summary>
    public PermissionInfo(string actionRaw, PermissionResource? resources = null)
    {
        ActionRaw = actionRaw ?? string.Empty;
        Resources = resources;
    }

    /// <summary>
    /// Convenience constructor from enum; stores its wire value in <see cref="ActionRaw"/>.
    /// </summary>
    public PermissionInfo(RbacPermissionAction action, PermissionResource? resources = null)
        : this(action.ToEnumMemberString(), resources) { }
}

/// <summary>
/// Role assignment for a user.
/// </summary>
public record UserRoleAssignment(string UserId, RbacUserType UserType);

/// <summary>
/// Role assignment for a group.
/// </summary>
public record GroupRoleAssignment(string GroupId, RbacGroupType GroupType);

public abstract class PermissionsScope : IEnumerable<PermissionInfo>
{
    public abstract IEnumerator<PermissionInfo> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Permissions
{
    public class Alias : PermissionsScope
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

        public override IEnumerator<PermissionInfo> GetEnumerator()
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

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                    return (PermissionsScope)aliasPerm;
                })
                .ToList();
        }
    }

    public class Data : PermissionsScope
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

        public override IEnumerator<PermissionInfo> GetEnumerator()
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

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                    return (PermissionsScope)dataPerm;
                })
                .ToList();
        }
    }

    public class Backups : PermissionsScope
    {
        public BackupsResource Resource { get; }
        public bool Manage { get; set; }

        public Backups(BackupsResource resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public override IEnumerator<PermissionInfo> GetEnumerator()
        {
            var permResource = new PermissionResource(Backups: Resource);
            if (Manage)
            {
                yield return new PermissionInfo(RbacPermissionAction.ManageBackups, permResource);
            }
        }

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                    return (PermissionsScope)backupPerm;
                })
                .ToList();
        }
    }

    public class Cluster : PermissionsScope
    {
        public bool Read { get; set; }

        public override IEnumerator<PermissionInfo> GetEnumerator()
        {
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadCluster);
        }

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                ? new List<PermissionsScope> { cluster }
                : new List<PermissionsScope>();
        }
    }

    public class Nodes : PermissionsScope
    {
        public bool Read { get; set; }

        public override IEnumerator<PermissionInfo> GetEnumerator()
        {
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadNodes);
        }

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            var nodes = new Nodes();
            foreach (var info in infos)
            {
                switch (info.Action)
                {
                    case RbacPermissionAction.ReadNodes:
                        nodes.Read = true;
                        break;
                }
            }
            return nodes.Read ? new List<PermissionsScope> { nodes } : new List<PermissionsScope>();
        }
    }

    public class Roles : PermissionsScope
    {
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        public override IEnumerator<PermissionInfo> GetEnumerator()
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

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
        {
            var roles = new Roles();
            foreach (var info in infos)
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
            return (roles.Create || roles.Read || roles.Update || roles.Delete)
                ? new List<PermissionsScope> { roles }
                : new List<PermissionsScope>();
        }
    }

    public class Users : PermissionsScope
    {
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool AssignAndRevoke { get; set; }

        public override IEnumerator<PermissionInfo> GetEnumerator()
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

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                ? new List<PermissionsScope> { users }
                : new List<PermissionsScope>();
        }
    }

    public class Tenants : PermissionsScope
    {
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        public override IEnumerator<PermissionInfo> GetEnumerator()
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

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                ? new List<PermissionsScope> { tenants }
                : new List<PermissionsScope>();
        }
    }

    public class Groups : PermissionsScope
    {
        public bool AssignAndRevoke { get; set; }
        public bool Read { get; set; }

        public override IEnumerator<PermissionInfo> GetEnumerator()
        {
            if (AssignAndRevoke)
                yield return new PermissionInfo(RbacPermissionAction.AssignAndRevokeGroups);
            if (Read)
                yield return new PermissionInfo(RbacPermissionAction.ReadGroups);
        }

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                ? new List<PermissionsScope> { groups }
                : new List<PermissionsScope>();
        }
    }

    public class Replicate : PermissionsScope
    {
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        public override IEnumerator<PermissionInfo> GetEnumerator()
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

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                ? new List<PermissionsScope> { replicate }
                : new List<PermissionsScope>();
        }
    }

    public class Collections : PermissionsScope
    {
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        public override IEnumerator<PermissionInfo> GetEnumerator()
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

        public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
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
                ? new List<PermissionsScope> { collections }
                : new List<PermissionsScope>();
        }
    }

    public static List<PermissionsScope> Parse(IEnumerable<PermissionInfo> infos)
    {
        var scopes = new List<PermissionsScope>();
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

namespace Weaviate.Client.Models;

using System.Runtime.Serialization;

/// <summary>
/// Public enum representing RBAC permission action strings. Mirrors internal REST DTO PermissionAction.
/// </summary>
public enum RbacPermissionAction
{
    [EnumMember(Value = "manage_backups")]
    ManageBackups,

    [EnumMember(Value = "read_cluster")]
    ReadCluster,

    [EnumMember(Value = "create_data")]
    CreateData,

    [EnumMember(Value = "read_data")]
    ReadData,

    [EnumMember(Value = "update_data")]
    UpdateData,

    [EnumMember(Value = "delete_data")]
    DeleteData,

    [EnumMember(Value = "read_nodes")]
    ReadNodes,

    [EnumMember(Value = "create_roles")]
    CreateRoles,

    [EnumMember(Value = "read_roles")]
    ReadRoles,

    [EnumMember(Value = "update_roles")]
    UpdateRoles,

    [EnumMember(Value = "delete_roles")]
    DeleteRoles,

    [EnumMember(Value = "create_collections")]
    CreateCollections,

    [EnumMember(Value = "read_collections")]
    ReadCollections,

    [EnumMember(Value = "update_collections")]
    UpdateCollections,

    [EnumMember(Value = "delete_collections")]
    DeleteCollections,

    [EnumMember(Value = "assign_and_revoke_users")]
    AssignAndRevokeUsers,

    [EnumMember(Value = "create_users")]
    CreateUsers,

    [EnumMember(Value = "read_users")]
    ReadUsers,

    [EnumMember(Value = "update_users")]
    UpdateUsers,

    [EnumMember(Value = "delete_users")]
    DeleteUsers,

    [EnumMember(Value = "create_tenants")]
    CreateTenants,

    [EnumMember(Value = "read_tenants")]
    ReadTenants,

    [EnumMember(Value = "update_tenants")]
    UpdateTenants,

    [EnumMember(Value = "delete_tenants")]
    DeleteTenants,

    [EnumMember(Value = "create_replicate")]
    CreateReplicate,

    [EnumMember(Value = "read_replicate")]
    ReadReplicate,

    [EnumMember(Value = "update_replicate")]
    UpdateReplicate,

    [EnumMember(Value = "delete_replicate")]
    DeleteReplicate,

    [EnumMember(Value = "create_aliases")]
    CreateAliases,

    [EnumMember(Value = "read_aliases")]
    ReadAliases,

    [EnumMember(Value = "update_aliases")]
    UpdateAliases,

    [EnumMember(Value = "delete_aliases")]
    DeleteAliases,

    [EnumMember(Value = "assign_and_revoke_groups")]
    AssignAndRevokeGroups,

    [EnumMember(Value = "read_groups")]
    ReadGroups,
}

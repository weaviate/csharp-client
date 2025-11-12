
namespace Weaviate.Client.Rest.Dto;

// RBAC Permission mapping
internal partial class Permission
{
    /// <summary>
    /// Converts a REST DTO Permission to a strongly-typed PermissionInfo, handling enum conversion and default values.
    /// Throws InvalidEnumWireFormatException for invalid enum values.
    /// </summary>
    internal Models.PermissionInfo ToModel()
    {
        // Handle NodeVerbosity conversion and default
        var nodeVerbosity = Nodes?.Verbosity?.ToEquivalentEnum<Models.NodeVerbosity>();
        var groupType = this.Groups?.GroupType?.ToEquivalentEnum<Models.RbacGroupType>();

        // Handle default values for resources (OpenAPI: '*' for collection, etc.)
        var resources = new Models.PermissionResource(
            Backups is not null ? new Models.BackupsResource(Backups.Collection) : null,
            Data is not null ? new Models.DataResource(Data.Collection, Data.Tenant, Data.Object) : null,
            Nodes is not null ? new Models.NodesResource(Nodes.Collection, nodeVerbosity) : null,
            Users is not null ? new Models.UsersResource(Users.Users1) : null,
            Groups is not null ? new Models.GroupsResource(Groups.Group, groupType) : null,
            Tenants is not null ? new Models.TenantsResource(Tenants.Collection, Tenants.Tenant) : null,
            Roles is not null ? new Models.RolesResource(Roles.Role, Roles.Scope?.ToEquivalentEnum<Models.RolesScope>()) : null,
            Collections is not null ? new Models.CollectionsResource(Collections.Collection) : null,
            Replicate is not null ? new Models.ReplicateResource(Replicate.Collection, Replicate.Shard) : null,
            Aliases is not null ? new Models.AliasesResource(Aliases.Collection, Aliases.Alias) : null
        );
        // Action conversion and validation
        var actionString = Action.ToEquivalentEnum<Models.RbacPermissionAction>();

        return new Models.PermissionInfo(actionString, resources);
    }
}

// Role assignment mapping for user
internal static class RoleUserAssignmentExtensions
{
    public static Models.UserRoleAssignment ToModel(this UserTypeOutput userType, string userId)
    {
        return new Models.UserRoleAssignment(userId, userType switch
        {
            UserTypeOutput.Db_user => Models.RbacUserType.Database,
            UserTypeOutput.Db_env_user => Models.RbacUserType.Database, // treat env user as database
            UserTypeOutput.Oidc => Models.RbacUserType.Oidc,
            _ => throw new ArgumentOutOfRangeException(nameof(userType), $"Unknown UserTypeOutput: {userType}")
        });
    }
}

// Role assignment mapping for group
internal static class RoleGroupAssignmentExtensions
{
    public static Models.GroupRoleAssignment ToModel(this GroupType groupType, string groupId)
    {
        return new Models.GroupRoleAssignment(groupId, groupType switch
        {
            GroupType.Oidc => Models.RbacGroupType.Oidc,
            _ => throw new ArgumentOutOfRangeException(nameof(groupType), $"Unknown GroupType: {groupType}")
        });
    }
}

// RBAC Role mapping
internal partial class Role
{
    /// <summary>
    /// Converts a REST DTO Role to a strongly-typed RoleInfo, parsing permissions and handling errors.
    /// </summary>
    public Models.RoleInfo ToModel()
    {
        return new Models.RoleInfo(
            Name ?? string.Empty,
            Models.Permissions.Parse((Permissions ?? []).Select(p => p.ToModel()))
        );
    }
}

internal partial class NestedProperty
{
    public Models.Property ToModel()
    {
        return new Models.Property
        {
            Name = Name ?? string.Empty,
            DataType = DataType?.ToList() ?? new List<string>(),
            Description = Description,
            IndexFilterable = IndexFilterable,
            IndexSearchable = IndexSearchable,
            IndexRangeFilters = IndexRangeFilters,
            PropertyTokenization = (Models.PropertyTokenization?)Tokenization,
            NestedProperties = NestedProperties?.Select(np => np.ToModel()).ToArray(),
        };
    }
}

internal partial class Property {
    public  Models.Property ToModel()
    {
        return new Models.Property
        {
            Name = Name ?? string.Empty,
            DataType = DataType?.ToList() ?? new List<string>(),
            Description = Description,
            IndexFilterable = IndexFilterable,
#pragma warning disable CS0612 // Type or member is obsolete
            IndexInverted = IndexInverted,
#pragma warning restore CS0612 // Type or member is obsolete
            IndexRangeFilters = IndexRangeFilters,
            IndexSearchable = IndexSearchable,
            PropertyTokenization = (Models.PropertyTokenization?)Tokenization,
            NestedProperties = NestedProperties?.Select(np => np.ToModel()).ToArray(),
        };
    }
}

internal partial class GeoCoordinates
{
    public Models.GeoCoordinate ToModel()
    {
        return new Models.GeoCoordinate(Latitude!.Value, Longitude!.Value);
    }
}

internal partial class PhoneNumber
{
    public Models.PhoneNumber ToModel()
    {
        return new Models.PhoneNumber(Input!)
        {
            DefaultCountry = DefaultCountry,
            CountryCode = CountryCode is null ? null :Convert.ToUInt64(CountryCode),
            National = National is null ? null : Convert.ToUInt64(National),
            InternationalFormatted = InternationalFormatted,
            NationalFormatted = NationalFormatted,
            Valid = Valid,
        };
    }
}

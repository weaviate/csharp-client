
namespace Weaviate.Client.Rest.Dto;

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
internal partial record Role
{
    /// <summary>
    /// Converts a REST DTO Role to a strongly-typed RoleInfo, parsing permissions and handling errors.
    /// </summary>
    public Models.RoleInfo ToModel()
    {
        return new Models.RoleInfo(
            Name ?? string.Empty,
            Models.Permissions.Parse(Permissions ?? [])
        );
    }
}

internal partial record NestedProperty
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

internal partial record Property {
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

internal partial record GeoCoordinates
{
    public Models.GeoCoordinate ToModel()
    {
        return new Models.GeoCoordinate(Latitude!.Value, Longitude!.Value);
    }
}

internal partial record PhoneNumber
{
    public Models.PhoneNumber ToModel()
    {
        return new Models.PhoneNumber(Input!)
        {
            DefaultCountry = DefaultCountry,
            CountryCode = CountryCode is null ? null : Convert.ToUInt64(CountryCode),
            National = National is null ? null : Convert.ToUInt64(National),
            InternationalFormatted = InternationalFormatted,
            NationalFormatted = NationalFormatted,
            Valid = Valid,
        };
    }
}

// Shard status mapping
internal partial record ShardStatusGetResponse
{
    /// <summary>
    /// Converts a REST DTO ShardStatusGetResponse to a ShardInfo model.
    /// </summary>
    public Models.ShardInfo ToModel()
    {
        return new Models.ShardInfo
        {
            Name = Name ?? string.Empty,
            Status = Status ?? string.Empty,
            VectorQueueSize = VectorQueueSize,
        };
    }
}

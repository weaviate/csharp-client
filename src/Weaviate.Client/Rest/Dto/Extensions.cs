
namespace Weaviate.Client.Rest.Dto;

// Role assignment mapping for user
/// <summary>
/// The role user assignment extensions class
/// </summary>
internal static class RoleUserAssignmentExtensions
{
    /// <summary>
    /// Returns the model using the specified user type
    /// </summary>
    /// <param name="userType">The user type</param>
    /// <param name="userId">The user id</param>
    /// <exception cref="ArgumentOutOfRangeException">Unknown UserTypeOutput: {userType}</exception>
    /// <returns>The models user role assignment</returns>
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
/// <summary>
/// The role group assignment extensions class
/// </summary>
internal static class RoleGroupAssignmentExtensions
{
    /// <summary>
    /// Returns the model using the specified group type
    /// </summary>
    /// <param name="groupType">The group type</param>
    /// <param name="groupId">The group id</param>
    /// <exception cref="ArgumentOutOfRangeException">Unknown GroupType: {groupType}</exception>
    /// <returns>The models group role assignment</returns>
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
/// <summary>
/// The role
/// </summary>
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

/// <summary>
/// The nested property
/// </summary>
internal partial record NestedProperty
{
    /// <summary>
    /// Returns the model
    /// </summary>
    /// <returns>The models property</returns>
    public Models.Property ToModel()
    {
        return new Models.Property
        {
            Name = Name ?? string.Empty,
            DataType = DataType?.FirstOrDefault()?.FromEnumMemberString<Models.DataType>() ?? Models.DataType.Unknown,
            Description = Description,
            IndexFilterable = IndexFilterable,
            IndexSearchable = IndexSearchable,
            IndexRangeFilters = IndexRangeFilters,
            PropertyTokenization = (Models.PropertyTokenization?)Tokenization,
            NestedProperties = NestedProperties?.Select(np => np.ToModel()).ToArray(),
            TextAnalyzer = Weaviate.Client.Models.TokenizeMapping.ToModel(TextAnalyzer),
        };
    }
}

/// <summary>
/// The property
/// </summary>
internal partial record Property
{
    /// <summary>
    /// Returns the model
    /// </summary>
    /// <returns>The models property</returns>
    public Models.Property ToModel()
    {
        // Extract skip and vectorizePropertyName from moduleConfig
        bool skipVectorization = false;
        bool vectorizePropertyName = true;

        if (ModuleConfig != null && ModuleConfig.Count > 0)
        {
            // Take the first vectorizer's settings (usually there's only one)
            var firstVectorizerConfig = ModuleConfig.Values.FirstOrDefault();
            if (firstVectorizerConfig is System.Text.Json.JsonElement jsonElement)
            {
                if (jsonElement.TryGetProperty("skip", out var skipProp))
                {
                    skipVectorization = skipProp.GetBoolean();
                }
                if (jsonElement.TryGetProperty("vectorizePropertyName", out var vecPropNameProp))
                {
                    vectorizePropertyName = vecPropNameProp.GetBoolean();
                }
            }
            else if (firstVectorizerConfig is IDictionary<string, object> dict)
            {
                if (dict.TryGetValue("skip", out var skipObj))
                {
                    skipVectorization = Convert.ToBoolean(skipObj);
                }
                if (dict.TryGetValue("vectorizePropertyName", out var vecPropNameObj))
                {
                    vectorizePropertyName = Convert.ToBoolean(vecPropNameObj);
                }
            }
        }

        return new Models.Property
        {
            Name = Name ?? string.Empty,
            DataType = DataType?.FirstOrDefault()?.FromEnumMemberString<Models.DataType>() ?? Models.DataType.Unknown,
            Description = Description,
            IndexFilterable = IndexFilterable,
#pragma warning disable CS0612 // Type or member is obsolete
            IndexInverted = IndexInverted,
#pragma warning restore CS0612 // Type or member is obsolete
            IndexRangeFilters = IndexRangeFilters,
            IndexSearchable = IndexSearchable,
            PropertyTokenization = (Models.PropertyTokenization?)Tokenization,
            NestedProperties = NestedProperties?.Select(np => np.ToModel()).ToArray(),
            SkipVectorization = skipVectorization,
            VectorizePropertyName = vectorizePropertyName,
            TextAnalyzer = Weaviate.Client.Models.TokenizeMapping.ToModel(TextAnalyzer),
        };
    }

        /// <summary>
        /// Returns the reference model
        /// </summary>
        /// <returns>The models reference</returns>
        public Models.Reference ToReferenceModel()
        {
            var targetCollection = DataType?.FirstOrDefault() ?? string.Empty;
            return new Models.Reference(Name ?? string.Empty, targetCollection, Description);
        }

}

/// <summary>
/// The geo coordinates
/// </summary>
internal partial record GeoCoordinates
{
    /// <summary>
    /// Returns the model
    /// </summary>
    /// <returns>The models geo coordinate</returns>
    public Models.GeoCoordinate ToModel()
    {
        return new Models.GeoCoordinate(Latitude!.Value, Longitude!.Value);
    }
}

/// <summary>
/// The phone number
/// </summary>
internal partial record PhoneNumber
{
    /// <summary>
    /// Returns the model
    /// </summary>
    /// <returns>The models phone number</returns>
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
/// <summary>
/// The shard status get response
/// </summary>
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
            Status = Models.ShardStatusExtensions.ParseStatus(Status),
            VectorQueueSize = VectorQueueSize,
        };
    }
}

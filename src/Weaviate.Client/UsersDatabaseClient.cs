using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing database users - CRUD operations, lifecycle management, and role assignments.
/// Accessed via <see cref="UsersClient.Db"/>.
/// </summary>
public class UsersDatabaseClient
{
    private readonly WeaviateClient _client;
    private const RbacUserType UserType = RbacUserType.Database;

    internal UsersDatabaseClient(WeaviateClient client) => _client = client;

    /// <summary>
    /// Lists all database users.
    /// </summary>
    public async Task<IEnumerable<DatabaseUser>> List(bool? includeLastUsedTime = null)
    {
        var users = await _client.RestClient.UsersDbList(includeLastUsedTime);
        return users.Select(ToModel);
    }

    /// <summary>
    /// Gets a specific database user by ID.
    /// </summary>
    public async Task<DatabaseUser?> Get(string userId, bool? includeLastUsedTime = null)
    {
        var dto = await _client.RestClient.UserDbGet(userId, includeLastUsedTime);
        return dto is null ? null : ToModel(dto);
    }

    /// <summary>
    /// Creates a new database user and returns their API key.
    /// </summary>
    public async Task<string> Create(
        string userId,
        bool? import = null,
        DateTimeOffset? createTime = null
    )
    {
        var apiKey = await _client.RestClient.UserDbCreate(userId, import, createTime);
        return apiKey.Apikey;
    }

    /// <summary>
    /// Deletes a database user.
    /// </summary>
    public Task<bool> Delete(string userId) => _client.RestClient.UserDbDelete(userId);

    /// <summary>
    /// Rotates the API key for a database user.
    /// </summary>
    public async Task<string> RotateApiKey(string userId)
    {
        var apiKey = await _client.RestClient.UserDbRotateKey(userId);

        return apiKey.Apikey;
    }

    /// <summary>
    /// Activates a database user.
    /// </summary>
    public Task<bool> Activate(string userId) => _client.RestClient.UserDbActivate(userId);

    /// <summary>
    /// Deactivates a database user, optionally revoking their API key.
    /// </summary>
    public Task<bool> Deactivate(string userId, bool? revokeKey = null) =>
        _client.RestClient.UserDbDeactivate(userId, revokeKey);

    /// <summary>
    /// Assigns roles to a database user.
    /// </summary>
    public Task AssignRoles(string userId, params string[] roles) =>
        _client.RestClient.UserAssignRoles(userId, UserType.ToEnumMemberString(), roles);

    /// <summary>
    /// Revokes roles from a database user.
    /// </summary>
    public Task RevokeRoles(string userId, params string[] roles) =>
        _client.RestClient.UserRevokeRoles(userId, UserType.ToEnumMemberString(), roles);

    /// <summary>
    /// Gets all roles assigned to a database user.
    /// </summary>
    public async Task<IEnumerable<RoleInfo>> GetRoles(string userId, bool? includeFullRoles = null)
    {
        var roles = await _client.RestClient.UserRolesGet(
            userId,
            UserType.ToEnumMemberString(),
            includeFullRoles
        );
        return roles.Select(r => r.ToModel());
    }

    private static DatabaseUser ToModel(Rest.Dto.DBUserInfo dto)
    {
        var type = dto.DbUserType switch
        {
            Rest.Dto.DBUserInfoDbUserType.Db_env_user => DatabaseUserType.DbEnvUser,
            _ => DatabaseUserType.DbUser,
        };
        return new DatabaseUser(
            dto.UserId ?? string.Empty,
            dto.Active,
            type,
            dto.CreatedAt,
            dto.LastUsedAt,
            dto.ApiKeyFirstLetters,
            dto.Roles ?? []
        );
    }
}

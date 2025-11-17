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
    public async Task<IEnumerable<DatabaseUser>> List(
        bool? includeLastUsedTime = null,
        CancellationToken cancellationToken = default
    )
    {
        var users = await _client.RestClient.UsersDbList(includeLastUsedTime, cancellationToken);
        return users.Select(ToModel);
    }

    /// <summary>
    /// Gets a specific database user by ID.
    /// </summary>
    public async Task<DatabaseUser?> Get(
        string userId,
        bool? includeLastUsedTime = null,
        CancellationToken cancellationToken = default
    )
    {
        var dto = await _client.RestClient.UserDbGet(
            userId,
            includeLastUsedTime,
            cancellationToken
        );
        return dto is null ? null : ToModel(dto);
    }

    /// <summary>
    /// Creates a new database user and returns their API key.
    /// </summary>
    public async Task<string> Create(
        string userId,
        bool? import = null,
        DateTimeOffset? createTime = null,
        CancellationToken cancellationToken = default
    )
    {
        var apiKey = await _client.RestClient.UserDbCreate(userId, import, createTime);
        return apiKey.Apikey;
    }

    /// <summary>
    /// Deletes a database user.
    /// </summary>
    public Task<bool> Delete(string userId, CancellationToken cancellationToken = default) =>
        _client.RestClient.UserDbDelete(userId, cancellationToken);

    /// <summary>
    /// Rotates the API key for a database user.
    /// </summary>
    public async Task<string> RotateApiKey(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var apiKey = await _client.RestClient.UserDbRotateKey(userId, cancellationToken);

        return apiKey.Apikey;
    }

    /// <summary>
    /// Activates a database user.
    /// </summary>
    public Task<bool> Activate(string userId, CancellationToken cancellationToken = default) =>
        _client.RestClient.UserDbActivate(userId, cancellationToken);

    /// <summary>
    /// Deactivates a database user, optionally revoking their API key.
    /// </summary>
    public Task<bool> Deactivate(
        string userId,
        bool? revokeKey = null,
        CancellationToken cancellationToken = default
    ) => _client.RestClient.UserDbDeactivate(userId, revokeKey, cancellationToken);

    /// <summary>
    /// Assigns roles to a database user.
    /// </summary>
    public Task AssignRoles(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    ) =>
        _client.RestClient.UserAssignRoles(
            userId,
            UserType.ToEnumMemberString(),
            roles,
            cancellationToken
        );

    /// <summary>
    /// Revokes roles from a database user.
    /// </summary>
    public Task RevokeRoles(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    ) =>
        _client.RestClient.UserRevokeRoles(
            userId,
            UserType.ToEnumMemberString(),
            roles,
            cancellationToken
        );

    /// <summary>
    /// Gets all roles assigned to a database user.
    /// </summary>
    public async Task<IEnumerable<RoleInfo>> GetRoles(
        string userId,
        bool? includeFullRoles = null,
        CancellationToken cancellationToken = default
    )
    {
        var roles = await _client.RestClient.UserRolesGet(
            userId,
            UserType.ToEnumMemberString(),
            includeFullRoles,
            cancellationToken
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

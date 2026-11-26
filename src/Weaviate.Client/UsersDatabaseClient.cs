using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing database users - CRUD operations, lifecycle management, and role assignments.
/// Accessed via <see cref="UsersClient.Db"/>.
/// </summary>
public class UsersDatabaseClient
{
    /// <summary>
    /// The client
    /// </summary>
    private readonly WeaviateClient _client;

    /// <summary>
    /// The database
    /// </summary>
    private const RbacUserType UserType = RbacUserType.Database;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersDatabaseClient"/> class
    /// </summary>
    /// <param name="client">The client</param>
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
    public async Task<string> Create(string userId, CancellationToken cancellationToken = default)
    {
        var apiKey = await _client.RestClient.UserDbCreate(
            userId,
            cancellationToken: cancellationToken
        );
        return apiKey.Apikey;
    }

    /// <summary>
    /// Creates a new database user and returns their API key.
    /// </summary>
    /// <param name="userId">The ID of the user to create.</param>
    /// <param name="expiresAt">When the user's API key expires. Omit, or pass null, for a key that never expires. Requires Weaviate 1.41.0+.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="WeaviateVersionMismatchException">
    /// Thrown when <paramref name="expiresAt"/> is set and the server version is below 1.41.0.
    /// </exception>
    public async Task<string> Create(
        string userId,
        DateTimeOffset? expiresAt,
        CancellationToken cancellationToken = default
    )
    {
        if (expiresAt is not null)
            await EnsureUserExpirationSupported();

        var apiKey = await _client.RestClient.UserDbCreate(userId, expiresAt, cancellationToken);
        return apiKey.Apikey;
    }

    /// <summary>
    /// Set the expiration of a 'db_user' user's API key. Requires Weaviate 1.41.0+.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="expiresAt">When the API key expires, or null for a key that never expires.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="WeaviateVersionMismatchException">
    /// Thrown when the server version is below 1.41.0.
    /// </exception>
    [RequiresWeaviateVersion(1, 41, 0)]
    public async Task UpdateUserExpiration(
        string userId,
        DateTimeOffset? expiresAt,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureVersion<UsersDatabaseClient>();

        await _client.RestClient.UserDbSetExpiration(userId, expiresAt, cancellationToken);
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

    /// <summary>The first server version that supports DB user API key expiration.</summary>
    private static readonly Version UserExpirationMinimumVersion = new(1, 41, 0);

    /// <summary>Throws when the connected server predates DB user API key expiration.</summary>
    private async Task EnsureUserExpirationSupported()
    {
        await _client.EnsureInitializedAsync();

        var serverVersion = _client.WeaviateVersion;
        if (serverVersion is null)
            return;

        if (serverVersion < UserExpirationMinimumVersion)
        {
            throw new WeaviateVersionMismatchException(
                "expiresAt",
                UserExpirationMinimumVersion,
                serverVersion
            );
        }
    }

    /// <summary>Maps the server's never-expires values (absent or Go zero time) to null.</summary>
    internal static DateTimeOffset? ExpiresAtOrNull(DateTimeOffset? value) =>
        value == default(DateTimeOffset) ? null : value;

    /// <summary>
    /// Returns the model using the specified dto
    /// </summary>
    /// <param name="dto">The dto</param>
    /// <returns>The database user</returns>
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
        )
        {
            ExpiresAt = ExpiresAtOrNull(dto.ExpiresAt),
        };
    }
}

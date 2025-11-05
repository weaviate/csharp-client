using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing database users (RBAC Users API).
/// </summary>
public class UsersClient
{
    private readonly WeaviateClient _client;

    internal UsersClient(WeaviateClient client) => _client = client;

    public async Task<CurrentUserInfo?> OwnInfo()
    {
        var dto = await _client.RestClient.UserOwnInfoGet();
        if (dto is null)
            return null;
        return new CurrentUserInfo(
            dto.Username ?? string.Empty,
            (dto.Roles ?? []).Select(r => new RoleInfo(
                r.Name ?? string.Empty,
                (r.Permissions ?? []).Select(p => new PermissionInfo(
                    p.Action.ToEnumMemberString() ?? string.Empty
                ))
            )),
            dto.Groups
        );
    }

    public async Task<IEnumerable<DatabaseUser>> List(bool? includeLastUsedTime = null)
    {
        var users = await _client.RestClient.UsersDbList(includeLastUsedTime);
        return users.Select(ToModel);
    }

    public async Task<DatabaseUser?> Get(string userId, bool? includeLastUsedTime = null)
    {
        var dto = await _client.RestClient.UserDbGet(userId, includeLastUsedTime);
        return dto is null ? null : ToModel(dto);
    }

    public async Task<string> Create(
        string userId,
        bool? import = null,
        DateTimeOffset? createTime = null
    )
    {
        var apiKey = await _client.RestClient.UserDbCreate(userId, import, createTime);
        return apiKey.Apikey;
    }

    public Task<bool> Delete(string userId) => _client.RestClient.UserDbDelete(userId);

    public async Task<string?> RotateApiKey(string userId)
    {
        var apiKey = await _client.RestClient.UserDbRotateKey(userId);
        return apiKey?.Apikey;
    }

    public Task<bool> Activate(string userId) => _client.RestClient.UserDbActivate(userId);

    public Task<bool> Deactivate(string userId, bool? revokeKey = null) =>
        _client.RestClient.UserDbDeactivate(userId, revokeKey);

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

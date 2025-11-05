using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

public partial class WeaviateRestClient
{
    internal async Task<Dto.UserOwnInfo?> UserOwnInfoGet()
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.UsersOwnInfo());
        var status = await response.EnsureExpectedStatusCodeAsync(
            [200, 401, 403, 500],
            "get own user info"
        );
        return status == HttpStatusCode.OK
            ? await response.Content.ReadFromJsonAsync<Dto.UserOwnInfo>(RestJsonSerializerOptions)
            : null;
    }

    internal async Task<IEnumerable<Dto.DBUserInfo>> UsersDbList(bool? includeLastUsedTime = null)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.UsersDb(includeLastUsedTime));
        await response.EnsureExpectedStatusCodeAsync([200], "list db users");
        var users = await response.Content.ReadFromJsonAsync<IEnumerable<Dto.DBUserInfo>>(
            RestJsonSerializerOptions
        );
        return users ?? Array.Empty<Dto.DBUserInfo>();
    }

    internal async Task<Dto.DBUserInfo?> UserDbGet(string userId, bool? includeLastUsedTime = null)
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.UserDb(userId, includeLastUsedTime)
        );
        var status = await response.EnsureExpectedStatusCodeAsync([200, 404], "get db user");
        return status == HttpStatusCode.OK
            ? await response.Content.ReadFromJsonAsync<Dto.DBUserInfo>(RestJsonSerializerOptions)
            : null;
    }

    internal async Task<Dto.UserApiKey> UserDbCreate(
        string userId,
        bool? import = null,
        DateTimeOffset? createTime = null
    )
    {
        object? body = null;
        if (import.HasValue || createTime.HasValue)
        {
            body = new
            {
                import = import ?? false,
                createTime = createTime?.UtcDateTime.ToString("o"),
            };
        }
        var response = body is not null
            ? await _httpClient.PostAsJsonAsync(
                WeaviateEndpoints.UserDb(userId),
                body,
                options: RestJsonSerializerOptions
            )
            : await _httpClient.PostAsync(WeaviateEndpoints.UserDb(userId), null);
        await response.EnsureExpectedStatusCodeAsync([201], "create db user");
        return await response.Content.ReadFromJsonAsync<Dto.UserApiKey>(RestJsonSerializerOptions)
            ?? throw new WeaviateRestClientException();
    }

    internal async Task<bool> UserDbDelete(string userId)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.UserDb(userId));
        var status = await response.EnsureExpectedStatusCodeAsync([204, 404], "delete db user");
        return status == HttpStatusCode.NoContent;
    }

    internal async Task<Dto.UserApiKey?> UserDbRotateKey(string userId)
    {
        var response = await _httpClient.PostAsync(WeaviateEndpoints.UserDbRotateKey(userId), null);
        var status = await response.EnsureExpectedStatusCodeAsync(
            [200, 404],
            "rotate user api key"
        );
        return status == HttpStatusCode.OK
            ? await response.Content.ReadFromJsonAsync<Dto.UserApiKey>(RestJsonSerializerOptions)
            : null;
    }

    internal async Task<bool> UserDbActivate(string userId)
    {
        var response = await _httpClient.PostAsync(WeaviateEndpoints.UserDbActivate(userId), null);
        var status = await response.EnsureExpectedStatusCodeAsync([200, 404, 409], "activate user");
        return status == HttpStatusCode.OK;
    }

    internal async Task<bool> UserDbDeactivate(string userId, bool? revokeKey = null)
    {
        object? body = revokeKey.HasValue ? new { revoke_key = revokeKey.Value } : null;
        HttpResponseMessage response;
        if (body is not null)
        {
            response = await _httpClient.PostAsJsonAsync(
                WeaviateEndpoints.UserDbDeactivate(userId),
                body,
                options: RestJsonSerializerOptions
            );
        }
        else
        {
            response = await _httpClient.PostAsync(
                WeaviateEndpoints.UserDbDeactivate(userId),
                null
            );
        }
        var status = await response.EnsureExpectedStatusCodeAsync(
            [200, 404, 409],
            "deactivate user"
        );
        return status == HttpStatusCode.OK;
    }
}

using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    internal async Task<Dto.UserOwnInfo?> UserOwnInfoGet(
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.UsersOwnInfo(),
            cancellationToken
        );

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.InternalServerError,
            ],
            "get own user info",
            ResourceType.User
        );

        return response.StatusCode == HttpStatusCode.OK
            ? await response.DecodeAsync<Dto.UserOwnInfo>(cancellationToken)
            : null;
    }

    internal async Task<IEnumerable<Dto.DBUserInfo>> UsersDbList(
        bool? includeLastUsedTime = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.UsersDb(includeLastUsedTime),
            cancellationToken
        );

        await response.ManageStatusCode([HttpStatusCode.OK], "list db users", ResourceType.User);

        var users = await response.DecodeAsync<IEnumerable<Dto.DBUserInfo>>(cancellationToken);
        return users ?? Array.Empty<Dto.DBUserInfo>();
    }

    internal async Task<Dto.DBUserInfo> UserDbGet(
        string userId,
        bool? includeLastUsedTime = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.UserDb(userId, includeLastUsedTime),
            cancellationToken
        );

        await response.ManageStatusCode([HttpStatusCode.OK], "get db user", ResourceType.User);

        return await response.DecodeAsync<Dto.DBUserInfo>(cancellationToken);
    }

    internal async Task<Dto.UserApiKey> UserDbCreate(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsync(
            WeaviateEndpoints.UserDb(userId),
            null,
            cancellationToken
        );

        await response.ManageStatusCode(
            [HttpStatusCode.Created],
            "create db user",
            ResourceType.User
        );

        return await response.DecodeAsync<Dto.UserApiKey>(cancellationToken);
    }

    internal async Task<bool> UserDbDelete(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.UserDb(userId),
            cancellationToken
        );

        await response.ManageStatusCode(
            [HttpStatusCode.NoContent, HttpStatusCode.NotFound],
            "delete db user",
            ResourceType.User
        );

        return response.StatusCode == HttpStatusCode.NoContent;
    }

    internal async Task<Dto.UserApiKey> UserDbRotateKey(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsync(
            WeaviateEndpoints.UserDbRotateKey(userId),
            null,
            cancellationToken
        );

        await response.ManageStatusCode(
            [HttpStatusCode.OK],
            "rotate user api key",
            ResourceType.User
        );

        return await response.DecodeAsync<Dto.UserApiKey>(cancellationToken);
    }

    internal async Task<bool> UserDbActivate(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsync(
            WeaviateEndpoints.UserDbActivate(userId),
            null,
            cancellationToken
        );

        await response.ManageStatusCode(
            [HttpStatusCode.OK, HttpStatusCode.Conflict],
            "activate user",
            ResourceType.User
        );

        return response.StatusCode == HttpStatusCode.OK;
    }

    internal async Task<bool> UserDbDeactivate(
        string userId,
        bool? revokeKey = null,
        CancellationToken cancellationToken = default
    )
    {
        object? body = revokeKey.HasValue ? new { revoke_key = revokeKey.Value } : null;
        HttpResponseMessage response;
        if (body is not null)
        {
            response = await _httpClient.PostAsJsonAsync(
                WeaviateEndpoints.UserDbDeactivate(userId),
                body,
                options: RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            response = await _httpClient.PostAsync(
                WeaviateEndpoints.UserDbDeactivate(userId),
                null,
                cancellationToken
            );
        }
        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                HttpStatusCode.Conflict,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.NotFound,
                // HttpStatusCode.InternalServerError,
            ],
            "deactivate user",
            ResourceType.User
        );

        return response.StatusCode == HttpStatusCode.OK;
    }
}

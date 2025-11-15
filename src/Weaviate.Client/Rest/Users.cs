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
        var status = await response.EnsureExpectedStatusCodeAsync(
            [200, 401, 403, 500],
            "get own user info"
        );
        return status == HttpStatusCode.OK
            ? await response.Content.ReadFromJsonAsync<Dto.UserOwnInfo>(
                RestJsonSerializerOptions,
                cancellationToken
            )
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
        await response.EnsureExpectedStatusCodeAsync([200], "list db users");
        var users = await response.Content.ReadFromJsonAsync<IEnumerable<Dto.DBUserInfo>>(
            RestJsonSerializerOptions,
            cancellationToken
        );
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
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "get db user");
            return await response.Content.ReadFromJsonAsync<Dto.DBUserInfo>(
                    RestJsonSerializerOptions,
                    cancellationToken
                ) ?? throw new WeaviateRestClientException();
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.User);
        }
    }

    internal async Task<Dto.UserApiKey> UserDbCreate(
        string userId,
        bool? import = null,
        DateTimeOffset? createTime = null,
        CancellationToken cancellationToken = default
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
                options: RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            )
            : await _httpClient.PostAsync(
                WeaviateEndpoints.UserDb(userId),
                null,
                cancellationToken
            );
        await response.EnsureExpectedStatusCodeAsync([201], "create db user");
        return await response.Content.ReadFromJsonAsync<Dto.UserApiKey>(
                RestJsonSerializerOptions,
                cancellationToken
            ) ?? throw new WeaviateRestClientException();
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
        await response.EnsureExpectedStatusCodeAsync([204, 404], "delete db user");

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
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "rotate user api key");
            return await response.Content.ReadFromJsonAsync<Dto.UserApiKey>(
                    RestJsonSerializerOptions,
                    cancellationToken
                ) ?? throw new WeaviateRestClientException();
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.User);
        }
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

        await response.EnsureExpectedStatusCodeAsync([200, 409], "activate user");

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

        await response.EnsureExpectedStatusCodeAsync([200, 409], "deactivate user");

        return response.StatusCode == HttpStatusCode.OK;
    }
}

using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest.Dto;

// These replace generated types until the next OpenAPI spec resync (Weaviate 1.41).

/// <summary>
/// Request body for POST /users/db/{user_id}.
/// </summary>
internal record UserCreateRequest
{
    /// <summary>
    /// When the user's API key expires.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? ExpiresAt { get; init; }
}

/// <summary>
/// Request body for PUT /users/db/{user_id}/expiration.
/// </summary>
internal record UserExpirationRequest
{
    /// <summary>
    /// When the user's API key expires.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Set to <c>true</c> to remove the expiration.
    /// </summary>
    [JsonPropertyName("neverExpires")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? NeverExpires { get; init; }
}

internal partial record DBUserInfo
{
    /// <summary>
    /// When the user's API key expires. Absent or Go zero time when it never expires.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }
}

internal partial record UserOwnInfo
{
    /// <summary>
    /// When the user's API key expires. Absent or Go zero time when it never expires.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }
}

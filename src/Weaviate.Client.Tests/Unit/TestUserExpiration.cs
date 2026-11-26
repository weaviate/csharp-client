using System.Net;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Tests for DB user API key expiration (Weaviate 1.41+).
/// </summary>
public class UserExpirationTests
{
    private static readonly DateTimeOffset ExpiresAt = new(
        2026,
        12,
        1,
        12,
        0,
        0,
        TimeSpan.FromHours(2)
    );

    /// <summary>
    /// Create with an expiration sends expiresAt.
    /// </summary>
    [Fact]
    public async Task Create_WithExpiration_SendsExpiresAt()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(serverVersion: "1.41.0");
        handler.AddResponse(
            new MockHttpResponse
            {
                StatusCode = HttpStatusCode.Created,
                Content = """{"apikey":"key"}""",
            }
        );
        var ct = TestContext.Current.CancellationToken;

        var apiKey = await client.Users.Db.Create("alice", ExpiresAt, ct);

        Assert.Equal("key", apiKey);
        handler.LastRequest!.ShouldHaveMethod(HttpMethod.Post).ShouldHavePath("/v1/users/db/alice");
        Assert.Equal(
            """{"expiresAt":"2026-12-01T12:00:00+02:00"}""",
            await handler.LastRequest!.Content!.ReadAsStringAsync(ct)
        );
    }

    /// <summary>
    /// UpdateUserExpiration sends expiresAt, or neverExpires for null.
    /// </summary>
    [Theory]
    [InlineData(true, """{"expiresAt":"2026-12-01T12:00:00+02:00"}""")]
    [InlineData(false, """{"neverExpires":true}""")]
    public async Task UpdateUserExpiration_SendsBody(bool withExpiration, string expectedBody)
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(serverVersion: "1.41.0");
        handler.AddResponse(MockResponses.Ok());
        var ct = TestContext.Current.CancellationToken;

        await client.Users.Db.UpdateUserExpiration("alice", withExpiration ? ExpiresAt : null, ct);

        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Put)
            .ShouldHavePath("/v1/users/db/alice/expiration");
        Assert.Equal(expectedBody, await handler.LastRequest!.Content!.ReadAsStringAsync(ct));
    }

    /// <summary>
    /// expiresAt on a DB user and on the own user parses to the instant; absent and Go zero time parse to null.
    /// </summary>
    [Theory]
    [InlineData(""","expiresAt":"2026-12-01T10:00:00Z" """, true)]
    [InlineData("", false)]
    [InlineData(""","expiresAt":"0001-01-01T00:00:00Z" """, false)]
    public async Task ExpiresAt_ParsesFromDbUserAndOwnUser(string expiresAtMember, bool expected)
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(serverVersion: "1.41.0");
        handler.AddResponse(
            new MockHttpResponse
            {
                Content =
                    $$"""{"userId":"alice","roles":[],"dbUserType":"db_user","active":true{{expiresAtMember}}}""",
            }
        );
        handler.AddResponse(
            new MockHttpResponse { Content = $$"""{"username":"alice"{{expiresAtMember}}}""" }
        );
        var ct = TestContext.Current.CancellationToken;
        DateTimeOffset? expectedValue = expected ? ExpiresAt : null;

        var user = await client.Users.Db.Get("alice", cancellationToken: ct);
        var ownInfo = await client.Users.OwnInfo(ct);

        Assert.Equal(expectedValue, user!.ExpiresAt);
        Assert.Equal(expectedValue, ownInfo!.ExpiresAt);
    }

    /// <summary>
    /// Below 1.41.0, an expiration is refused before any request is sent.
    /// </summary>
    [Fact]
    public async Task Expiration_OnServerBelow1_41_ThrowsVersionMismatchException()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(serverVersion: "1.40.7");
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<WeaviateVersionMismatchException>(() =>
            client.Users.Db.Create("alice", ExpiresAt, ct)
        );
        await Assert.ThrowsAsync<WeaviateVersionMismatchException>(() =>
            client.Users.Db.UpdateUserExpiration("alice", null, ct)
        );
        Assert.Empty(handler.Requests);
    }
}

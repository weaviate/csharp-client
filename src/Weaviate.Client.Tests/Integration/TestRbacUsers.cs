namespace Weaviate.Client.Tests.Integration;

using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using Xunit;

/// <summary>
/// Integration tests for Users client operations against RBAC-enabled Weaviate.
/// Requires Weaviate with RBAC running on port defined in 8092/50063.
/// </summary>
[Trait("Category", "RBAC")]
public class TestRbacUsers : IntegrationTests
{
    public override ushort RestPort => 8092;
    public override ushort GrpcPort => 50063;

    private const string ADMIN_API_KEY = "admin-key";

    public override ICredentials? Credentials => Auth.ApiKey(ADMIN_API_KEY);

    public TestRbacUsers() => RequireVersion("1.30.0");

    [Fact]
    public async Task Test_OwnUser()
    {
        RequireVersion("1.28.0");
        var user = await _weaviate.Users.OwnInfo();

        Assert.NotNull(user);
        Assert.Equal("admin-user", user.Username);
        Assert.NotEmpty(user.Roles);
    }

    [Fact]
    public async Task Test_ListUsers()
    {
        RequireVersion("1.30.0");
        var users = await _weaviate.Users.Db.List();
        var userList = users.ToList();

        Assert.NotEmpty(userList);
        Assert.Contains(userList, u => u.UserId == "admin-user");
    }

    [Fact]
    public async Task Test_CreateUserAndGet()
    {
        RequireVersion("1.30.0");
        var randomUserName = $"new-user-{Random.Shared.Next(1, 10000)}";

        try
        {
            // Create user
            var apiKey = await _weaviate.Users.Db.Create(randomUserName);
            Assert.NotNull(apiKey);
            Assert.NotEmpty(apiKey);

            // Get user
            var user = await _weaviate.Users.Db.Get(randomUserName);
            Assert.NotNull(user);
            Assert.Equal(randomUserName, user.UserId);
            Assert.Equal(DatabaseUserType.DbUser, user.DbUserType);
            Assert.True(user.Active);

            // Verify we can connect with the new user's API key
            var newUserClient = Connect.Local(
                hostname: "localhost",
                restPort: RestPort,
                grpcPort: GrpcPort,
                credentials: Auth.ApiKey(apiKey),
                httpMessageHandler: _httpMessageHandler
            );
            var ownInfo = await newUserClient.Users.OwnInfo();
            Assert.NotNull(ownInfo);
            Assert.Equal(randomUserName, ownInfo!.Username);
        }
        finally
        {
            // Cleanup
            await _weaviate.Users.Db.Delete(randomUserName);
        }
    }

    [Fact]
    public async Task Test_DeleteUser()
    {
        RequireVersion("1.30.0");
        var randomUserName = $"delete-user-{Random.Shared.Next(1, 10_000)}";

        // Create user
        await _weaviate.Users.Db.Create(randomUserName);

        // Delete user
        await _weaviate.Users.Db.Delete(randomUserName);
        // No exception means success

        // Verify delete of non-existent user throws exception
        await Assert.ThrowsAsync<WeaviateNotFoundException>(async () =>
        {
            await _weaviate.Users.Db.Delete(randomUserName);
        });
    }

    [Fact]
    public async Task Test_RotateUserKey()
    {
        RequireVersion("1.30.0");
        var randomUserName = $"rotate-user-{Random.Shared.Next(1, 10_000)}";

        try
        {
            // Create user
            var apiKeyOld = await _weaviate.Users.Db.Create(randomUserName);

            // Verify old key works
            var oldKeyClient = Connect.Local(
                hostname: "localhost",
                restPort: RestPort,
                grpcPort: GrpcPort,
                credentials: Auth.ApiKey(apiKeyOld),
                httpMessageHandler: _httpMessageHandler
            );
            var user = await oldKeyClient.Users.OwnInfo();
            Assert.NotNull(user);
            Assert.Equal(randomUserName, user!.Username);

            // Rotate key
            var apiKeyNew = await _weaviate.Users.Db.RotateApiKey(randomUserName);
            Assert.NotNull(apiKeyNew);
            Assert.NotEmpty(apiKeyNew);
            Assert.NotEqual(apiKeyOld, apiKeyNew);

            // Verify new key works
            var newKeyClient = Connect.Local(
                hostname: "localhost",
                restPort: RestPort,
                grpcPort: GrpcPort,
                credentials: Auth.ApiKey(apiKeyNew),
                httpMessageHandler: _httpMessageHandler
            );
            var userAfterRotate = await newKeyClient.Users.OwnInfo();
            Assert.NotNull(userAfterRotate);
            Assert.Equal(randomUserName, userAfterRotate!.Username);
        }
        finally
        {
            await _weaviate.Users.Db.Delete(randomUserName);
        }
    }

    [Fact]
    public async Task Test_ActivateDeactivate()
    {
        RequireVersion("1.30.0");
        var randomUserName = $"activate-user-{Random.Shared.Next(1, 10000)}";

        try
        {
            // Create user
            await _weaviate.Users.Db.Create(randomUserName);

            // Deactivate
            await _weaviate.Users.Db.Deactivate(randomUserName);
            // Not throwing means success

            // Second deactivation should throw a conflict exception
            await Assert.ThrowsAsync<WeaviateConflictException>(async () =>
            {
                await _weaviate.Users.Db.Deactivate(randomUserName);
            });

            // Verify user is inactive
            var user = await _weaviate.Users.Db.Get(randomUserName);
            Assert.NotNull(user);
            Assert.False(user!.Active);

            // Activate
            await _weaviate.Users.Db.Activate(randomUserName);

            // Second activation should throw a conflict exception
            await Assert.ThrowsAsync<WeaviateConflictException>(async () =>
            {
                await _weaviate.Users.Db.Activate(randomUserName);
            });

            // Verify user is active
            user = await _weaviate.Users.Db.Get(randomUserName);
            Assert.NotNull(user);
            Assert.True(user!.Active);
        }
        finally
        {
            await _weaviate.Users.Db.Delete(randomUserName);
        }
    }

    [Fact]
    public async Task Test_DeactivateAndRevokeKey()
    {
        RequireVersion("1.30.0");
        var randomUserName = $"revoke-user-{Random.Shared.Next(1, 10000)}";

        try
        {
            // Create user
            var apiKeyOld = await _weaviate.Users.Db.Create(randomUserName);

            // Deactivate with revoke_key=true
            await _weaviate.Users.Db.Deactivate(randomUserName, revokeKey: true);

            // Old key should not work anymore (capture exception from client construction + call)
            await Assert.ThrowsAnyAsync<WeaviateException>(async () =>
            {
                var oldKeyClient = Connect.Local(
                    hostname: "localhost",
                    restPort: RestPort,
                    grpcPort: GrpcPort,
                    credentials: Auth.ApiKey(apiKeyOld),
                    httpMessageHandler: _httpMessageHandler
                );
                await oldKeyClient.Users.OwnInfo();
            });

            // Re-activate
            await _weaviate.Users.Db.Activate(randomUserName);

            // Old key still shouldn't work (revoked)
            await Assert.ThrowsAnyAsync<WeaviateException>(async () =>
            {
                var oldKeyClient = Connect.Local(
                    hostname: "localhost",
                    restPort: RestPort,
                    grpcPort: GrpcPort,
                    credentials: Auth.ApiKey(apiKeyOld),
                    httpMessageHandler: _httpMessageHandler
                );
                await oldKeyClient.Users.OwnInfo();
            });

            // Rotate to get new key
            var apiKeyNew = await _weaviate.Users.Db.RotateApiKey(randomUserName);

            // New key should work
            var newKeyClient = Connect.Local(
                hostname: "localhost",
                restPort: RestPort,
                grpcPort: GrpcPort,
                credentials: Auth.ApiKey(apiKeyNew ?? string.Empty),
                httpMessageHandler: _httpMessageHandler
            );
            var user = await newKeyClient.Users.OwnInfo();
            Assert.NotNull(user);
            Assert.Equal(randomUserName, user!.Username);
        }
        finally
        {
            await _weaviate.Users.Db.Delete(randomUserName);
        }
    }
}

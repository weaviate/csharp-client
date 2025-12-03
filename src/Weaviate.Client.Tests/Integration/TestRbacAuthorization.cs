namespace Weaviate.Client.Tests.Integration;

using System.Linq;
using Weaviate.Client;
using Weaviate.Client.Models;
using Xunit;

/// <summary>
/// RBAC Groups integration tests (Rest:8092 gRPC:50063). Authorization checks for various operations.
/// </summary>
public class TestRbacAuthorization : IntegrationTests
{
    public override ushort RestPort => 8092;
    public override ushort GrpcPort => 50063;
    private const string ADMIN_API_KEY = "admin-key";

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        RequireVersion("1.32.0");
    }

    public override ICredentials? Credentials => Auth.ApiKey(ADMIN_API_KEY);

    [Fact, Trait("Category", "RBAC")]
    public async Task TestAuthorizationFailure()
    {
        // Generate random names for collection and user
        var collectionName = $"AuthorizationTest";
        var userId = Helpers.GenerateUniqueIdentifier("user");

        // Create collection
        var collectionConfig = new CollectionConfig
        {
            Name = collectionName,
            Properties = [Property.Text("name")],
        };

        var client = await CollectionFactory<object>(collectionConfig);

        var roleName = Helpers.GenerateUniqueIdentifier("read-only-role");

        await _weaviate.Roles.Delete(roleName, TestContext.Current.CancellationToken);

        // Create a role with only read permission for this collection
        var readOnlyRole = await _weaviate.Roles.Create(
            roleName,
            [
                new Permissions.Collections(collectionName) { Read = true },
                new Permissions.Data(collectionName, null, null) { Read = true },
            ],
            TestContext.Current.CancellationToken
        );

        // Create a user and assign the read-only role
        var apiKey = await _weaviate.Users.Db.Create(
            userId,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await _weaviate.Users.Db.AssignRoles(
            userId,
            new[] { readOnlyRole.Name },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Create a new client with the user's API key
        var userClient = await new WeaviateClientBuilder()
            .WithRestEndpoint("localhost")
            .WithRestPort(RestPort)
            .WithGrpcEndpoint("localhost")
            .WithGrpcPort(GrpcPort)
            .WithCredentials(Auth.ApiKey(apiKey))
            .BuildAsync();

        var userCollection = userClient.Collections.Use(collectionName);

        // Try to insert data and assert that authorization exception is thrown
        await Assert.ThrowsAsync<WeaviateAuthorizationException>(async () =>
        {
            await userCollection.Data.Insert(
                new { name = "should fail" },
                cancellationToken: TestContext.Current.CancellationToken
            );
        });
    }
}

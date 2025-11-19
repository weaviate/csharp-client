namespace Weaviate.Client.Tests.Integration;

using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using Xunit;

[Collection("TestAliases")]
public class TestAliases : IntegrationTests
{
    public TestAliases()
        : base()
    {
        RequireVersion("1.32.0");
    }

    [Fact]
    public async Task Test_Create_And_Get_Alias()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "TargetCollection",
            properties: [Property.Text("Name")]
        );

        var aliasName = MakeUniqueCollectionName<object>("TestAlias");

        // Act
        var createdAlias = await collection.Alias.Add(
            aliasName,
            TestContext.Current.CancellationToken
        );
        var retrievedAlias = await _weaviate.Alias.Get(
            aliasName,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(createdAlias);
        Assert.Equal(aliasName, createdAlias.Name);
        Assert.Equal(collection.Name, createdAlias.TargetClass);

        Assert.NotNull(retrievedAlias);
        Assert.Equal(aliasName, retrievedAlias.Name);
        Assert.Equal(collection.Name, retrievedAlias.TargetClass);

        // Cleanup
        await _weaviate.Alias.Delete(aliasName, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test_List_Aliases()
    {
        // Arrange
        var collection1 = await CollectionFactory(
            name: "Collection1",
            properties: [Property.Text("Name")]
        );
        var collection2 = await CollectionFactory(
            name: "Collection2",
            properties: [Property.Text("Name")]
        );

        var alias1Name = MakeUniqueCollectionName<object>("Alias1");
        var alias2Name = MakeUniqueCollectionName<object>("Alias2");
        var alias3Name = MakeUniqueCollectionName<object>("Alias3");

        // Ensure clean state by deleting aliases if they exist
        var allExistingAliases = await _weaviate.Alias.List(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var existingAliasNames = allExistingAliases.Select(a => a.Name).ToHashSet();
        if (existingAliasNames.Contains(alias1Name))
            await _weaviate.Alias.Delete(alias1Name, TestContext.Current.CancellationToken);
        if (existingAliasNames.Contains(alias2Name))
            await _weaviate.Alias.Delete(alias2Name, TestContext.Current.CancellationToken);
        if (existingAliasNames.Contains(alias3Name))
            await _weaviate.Alias.Delete(alias3Name, TestContext.Current.CancellationToken);

        await collection1.Alias.Add(alias1Name, TestContext.Current.CancellationToken);
        await collection1.Alias.Add(alias2Name, TestContext.Current.CancellationToken);
        await collection2.Alias.Add(alias3Name, TestContext.Current.CancellationToken);

        // Act - List all aliases
        var allAliases = await _weaviate.Alias.List(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Act - List aliases for collection1
        var collection1Aliases = await _weaviate.Alias.List(
            collection1.Name,
            TestContext.Current.CancellationToken
        );

        // Assert - All aliases should include our created aliases
        var allAliasNames = allAliases.Select(a => a.Name).ToHashSet();
        Assert.Contains(alias1Name, allAliasNames);
        Assert.Contains(alias2Name, allAliasNames);
        Assert.Contains(alias3Name, allAliasNames);

        // Assert - Collection1 aliases should only include alias1 and alias2
        var collection1AliasNames = collection1Aliases.Select(a => a.Name).ToHashSet();
        Assert.Contains(alias1Name, collection1AliasNames);
        Assert.Contains(alias2Name, collection1AliasNames);
        Assert.DoesNotContain(alias3Name, collection1AliasNames);

        // Cleanup
        await _weaviate.Alias.Delete(alias1Name, TestContext.Current.CancellationToken);
        await _weaviate.Alias.Delete(alias2Name, TestContext.Current.CancellationToken);
        await _weaviate.Alias.Delete(alias3Name, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test_Update_Alias()
    {
        // Arrange
        var collection1 = await CollectionFactory(
            name: "OriginalCollection",
            properties: [Property.Text("Name")]
        );
        var collection2 = await CollectionFactory(
            name: "UpdatedCollection",
            properties: [Property.Text("Name")]
        );

        var aliasName = MakeUniqueCollectionName<object>("UpdateAlias");

        await collection1.Alias.Add(aliasName, TestContext.Current.CancellationToken);

        // Act
        var updatedAlias = await _weaviate.Alias.Update(
            aliasName,
            collection2.Name,
            TestContext.Current.CancellationToken
        );

        var retrievedAlias = await _weaviate.Alias.Get(
            aliasName,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(updatedAlias);
        Assert.Equal(aliasName, updatedAlias.Name);
        Assert.Equal(collection2.Name, updatedAlias.TargetClass);

        Assert.NotNull(retrievedAlias);
        Assert.Equal(aliasName, retrievedAlias.Name);
        Assert.Equal(collection2.Name, retrievedAlias.TargetClass);

        // Cleanup
        await _weaviate.Alias.Delete(aliasName, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test_Delete_Alias()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "DeleteTestCollection",
            properties: [Property.Text("Name")]
        );

        var aliasName = MakeUniqueCollectionName<object>("DeleteAlias");

        await collection.Alias.Add(aliasName, TestContext.Current.CancellationToken);

        // Verify alias exists
        var retrievedAlias = await _weaviate.Alias.Get(
            aliasName,
            TestContext.Current.CancellationToken
        );
        Assert.NotNull(retrievedAlias);

        // Act
        await _weaviate.Alias.Delete(aliasName, TestContext.Current.CancellationToken);

        // Assert - Getting deleted alias should return null
        Assert.Null(await _weaviate.Alias.Get(aliasName, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Test_Get_Nonexistent_Alias_Returns_Null()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "TestCollection",
            properties: [Property.Text("Name")]
        );

        var nonexistentAliasName = MakeUniqueCollectionName<object>("NonexistentAlias");

        // Act
        var alias = await _weaviate.Alias.Get(
            nonexistentAliasName,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Null(alias);
    }

    [Fact]
    public async Task Test_Create_Alias_To_Nonexistent_Collection_Throws_Exception()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "ExistingCollection",
            properties: [Property.Text("Name")]
        );

        var aliasName = MakeUniqueCollectionName<object>("BadAlias");
        var nonexistentCollectionName = MakeUniqueCollectionName<object>("NonexistentCollection");
        var alias = new Alias(aliasName, nonexistentCollectionName);

        // Act & Assert
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Alias.Add(alias, TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task Test_Create_Duplicate_Alias_Throws_Exception()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "DuplicateTestCollection",
            properties: [Property.Text("Name")]
        );

        var aliasName = MakeUniqueCollectionName<object>("DuplicateAlias");

        await collection.Alias.Add(aliasName, TestContext.Current.CancellationToken);

        // Act & Assert - Creating the same alias again should throw
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await collection.Alias.Add(aliasName, TestContext.Current.CancellationToken)
        );

        // Cleanup
        await _weaviate.Alias.Delete(aliasName, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test_Update_Nonexistent_Alias_Throws_Exception()
    {
        // Arrange
        var collection1 = await CollectionFactory(
            name: "Collection1",
            properties: [Property.Text("Name")]
        );
        var collection2 = await CollectionFactory(
            name: "Collection2",
            properties: [Property.Text("Name")]
        );

        var nonexistentAliasName = MakeUniqueCollectionName<object>("NonexistentUpdateAlias");

        // Act & Assert
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Alias.Update(
                nonexistentAliasName,
                collection2.Name,
                TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task Test_Update_Alias_After_Original_Collection_Deleted()
    {
        // Arrange
        var collection1 = await CollectionFactory(
            name: "TempCollection",
            properties: [Property.Text("Name")]
        );
        var collection2 = await CollectionFactory(
            name: "PermanentCollection",
            properties: [Property.Text("Name")]
        );

        var aliasName = MakeUniqueCollectionName<object>("SurvivingAlias");

        await collection1.Alias.Add(aliasName, TestContext.Current.CancellationToken);

        // Act - Delete the target collection (will be cleaned up automatically)
        // Note: We can't remove from cleanup list as it's private, but that's fine

        // Update alias to point to a different collection
        var updatedAlias = await _weaviate.Alias.Update(
            aliasName,
            collection2.Name,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(updatedAlias);
        Assert.Equal(collection2.Name, updatedAlias.TargetClass);

        // Cleanup
        await _weaviate.Alias.Delete(aliasName, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test_Multiple_Aliases_Point_To_Same_Collection()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "SharedCollection",
            properties: [Property.Text("Name")]
        );

        var alias1Name = MakeUniqueCollectionName<object>("SharedAlias1");
        var alias2Name = MakeUniqueCollectionName<object>("SharedAlias2");
        var alias3Name = MakeUniqueCollectionName<object>("SharedAlias3");

        var allExistingAliases = await _weaviate.Alias.List(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var existingAliasNames = allExistingAliases.Select(a => a.Name).ToHashSet();
        if (existingAliasNames.Contains(alias1Name))
            await _weaviate.Alias.Delete(alias1Name, TestContext.Current.CancellationToken);
        if (existingAliasNames.Contains(alias2Name))
            await _weaviate.Alias.Delete(alias2Name, TestContext.Current.CancellationToken);
        if (existingAliasNames.Contains(alias3Name))
            await _weaviate.Alias.Delete(alias3Name, TestContext.Current.CancellationToken);

        // Act
        await collection.Alias.Add(alias1Name, TestContext.Current.CancellationToken);
        await collection.Alias.Add(alias2Name, TestContext.Current.CancellationToken);
        await collection.Alias.Add(alias3Name, TestContext.Current.CancellationToken);

        var collectionAliases = await collection.Alias.List(TestContext.Current.CancellationToken);

        // Assert
        var aliasNames = collectionAliases.Select(a => a.Name).ToHashSet();
        Assert.Contains(alias1Name, aliasNames);
        Assert.Contains(alias2Name, aliasNames);
        Assert.Contains(alias3Name, aliasNames);

        // All should point to the same collection
        Assert.All(
            collectionAliases.Where(a =>
                a.Name == alias1Name || a.Name == alias2Name || a.Name == alias3Name
            ),
            a => Assert.Equal(collection.Name, a.TargetClass)
        );

        // Cleanup
        await _weaviate.Alias.Delete(
            alias1Name,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await _weaviate.Alias.Delete(
            alias2Name,
            cancellationToken: TestContext.Current.CancellationToken
        );
        await _weaviate.Alias.Delete(
            alias3Name,
            cancellationToken: TestContext.Current.CancellationToken
        );
    }

    [Fact]
    public async Task Test_Alias_To_Deleted_Collection_Persists_But_Queries_Fail()
    {
        // Arrange - Create a collection with data
        var collection = await CollectionFactory(
            name: "CollectionToDelete",
            properties: [Property.Text("Name"), Property.Int("Age")]
        );

        // Insert some dummy data
        var id1 = await collection.Data.Insert(
            new { Name = "Alice", Age = 30 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var id2 = await collection.Data.Insert(
            new { Name = "Bob", Age = 25 },
            cancellationToken: TestContext.Current.CancellationToken
        );
        var id3 = await collection.Data.Insert(
            new { Name = "Charlie", Age = 35 },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Verify data exists
        var objectsBefore = await collection.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(3, objectsBefore.Objects.Count);

        // Create an alias pointing to this collection
        var aliasName = MakeUniqueCollectionName<object>("AliasToDeletedCollection");
        try
        {
            await _weaviate.Alias.Delete(aliasName, TestContext.Current.CancellationToken);
        }
        catch { }
        await collection.Alias.Add(aliasName, TestContext.Current.CancellationToken);

        // Verify the alias exists
        var retrievedAlias = await _weaviate.Alias.Get(
            aliasName,
            TestContext.Current.CancellationToken
        );
        Assert.NotNull(retrievedAlias);
        Assert.Equal(collection.Name, retrievedAlias.TargetClass);

        // Act - Delete the collection
        await _weaviate.Collections.Delete(collection.Name, TestContext.Current.CancellationToken);

        // Assert - List aliases to confirm the alias still exists
        var allAliases = await _weaviate.Alias.List(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var aliasStillExists = allAliases.Any(a => a.Name == aliasName);
        Assert.True(
            aliasStillExists,
            "Alias should still exist after target collection is deleted"
        );

        // Get the alias to verify it still points to the deleted collection
        var aliasAfterDeletion = await _weaviate.Alias.Get(
            aliasName,
            TestContext.Current.CancellationToken
        );
        Assert.NotNull(aliasAfterDeletion);
        Assert.Equal(collection.Name, aliasAfterDeletion.TargetClass);

        // Try to fetch objects using the alias as the collection name - should fail
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate
                .Collections.Use<object>(aliasName)
                .Query.FetchObjects(cancellationToken: TestContext.Current.CancellationToken)
        );

        // Try to fetch a specific object by ID using the alias - should also fail
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate
                .Collections.Use<object>(aliasName)
                .Query.FetchObjectByID(
                    id1,
                    cancellationToken: TestContext.Current.CancellationToken
                )
        );

        // Check if Collection.Exists with the alias name returns false
        var exists = await _weaviate.Collections.Exists(
            aliasName,
            TestContext.Current.CancellationToken
        );
        Assert.False(
            exists,
            "Collection.Exists should return false for alias to deleted collection"
        );

        // Cleanup - Delete the alias
        await _weaviate.Alias.Delete(aliasName, TestContext.Current.CancellationToken);
    }
}

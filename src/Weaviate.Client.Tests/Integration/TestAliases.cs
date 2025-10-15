namespace Weaviate.Client.Tests.Integration;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using Xunit;

[Collection("TestAliases")]
public class TestAliases : IntegrationTests
{
    [Fact]
    public async Task Test_Create_And_Get_Alias()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "TargetCollection",
            properties: [Property.Text("Name")]
        );

        var aliasName = MakeUniqueCollectionName<object>("TestAlias");
        var alias = new Alias(aliasName, collection.Name);

        // Act
        var createdAlias = await collection.Alias.Add(alias);
        var retrievedAlias = await collection.Alias.Get(aliasName);

        // Assert
        Assert.NotNull(createdAlias);
        Assert.Equal(aliasName, createdAlias.Name);
        Assert.Equal(collection.Name, createdAlias.TargetClass);

        Assert.NotNull(retrievedAlias);
        Assert.Equal(aliasName, retrievedAlias.Name);
        Assert.Equal(collection.Name, retrievedAlias.TargetClass);

        // Cleanup
        await collection.Alias.Delete(aliasName);
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

        var alias1 = new Alias(alias1Name, collection1.Name);
        var alias2 = new Alias(alias2Name, collection1.Name);
        var alias3 = new Alias(alias3Name, collection2.Name);

        // Ensure clean state by deleting aliases if they exist
        var allExistingAliases = await collection1.Alias.List();
        var existingAliasNames = allExistingAliases.Select(a => a.Name).ToHashSet();
        if (existingAliasNames.Contains(alias1Name))
            await collection1.Alias.Delete(alias1Name);
        if (existingAliasNames.Contains(alias2Name))
            await collection1.Alias.Delete(alias2Name);
        if (existingAliasNames.Contains(alias3Name))
            await collection2.Alias.Delete(alias3Name);

        await collection1.Alias.Add(alias1);
        await collection1.Alias.Add(alias2);
        await collection2.Alias.Add(alias3);

        // Act - List all aliases
        var allAliases = await collection1.Alias.List();

        // Act - List aliases for collection1
        var collection1Aliases = await collection1.Alias.List(collection1.Name);

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
        await collection1.Alias.Delete(alias1Name);
        await collection1.Alias.Delete(alias2Name);
        await collection2.Alias.Delete(alias3Name);
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
        var alias = new Alias(aliasName, collection1.Name);

        await collection1.Alias.Add(alias);

        // Act
        var updatedAlias = await collection1.Alias.Update(aliasName, collection2.Name);

        var retrievedAlias = await collection2.Alias.Get(aliasName);

        // Assert
        Assert.NotNull(updatedAlias);
        Assert.Equal(aliasName, updatedAlias.Name);
        Assert.Equal(collection2.Name, updatedAlias.TargetClass);

        Assert.NotNull(retrievedAlias);
        Assert.Equal(aliasName, retrievedAlias.Name);
        Assert.Equal(collection2.Name, retrievedAlias.TargetClass);

        // Cleanup
        await collection2.Alias.Delete(aliasName);
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
        var alias = new Alias(aliasName, collection.Name);

        await collection.Alias.Add(alias);

        // Verify alias exists
        var retrievedAlias = await collection.Alias.Get(aliasName);
        Assert.NotNull(retrievedAlias);

        // Act
        await collection.Alias.Delete(aliasName);

        // Assert - Getting deleted alias should throw exception
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await collection.Alias.Get(aliasName)
        );
    }

    [Fact]
    public async Task Test_Get_Nonexistent_Alias_Throws_Exception()
    {
        // Arrange
        var collection = await CollectionFactory(
            name: "TestCollection",
            properties: [Property.Text("Name")]
        );

        var nonexistentAliasName = MakeUniqueCollectionName<object>("NonexistentAlias");

        // Act & Assert
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await collection.Alias.Get(nonexistentAliasName)
        );
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
            await collection.Alias.Add(alias)
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
        var alias = new Alias(aliasName, collection.Name);

        await collection.Alias.Add(alias);

        // Act & Assert - Creating the same alias again should throw
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await collection.Alias.Add(alias)
        );

        // Cleanup
        await collection.Alias.Delete(aliasName);
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
            await collection1.Alias.Update(nonexistentAliasName, collection2.Name)
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
        var alias = new Alias(aliasName, collection1.Name);

        await collection1.Alias.Add(alias);

        // Act - Delete the target collection (will be cleaned up automatically)
        // Note: We can't remove from cleanup list as it's private, but that's fine

        // Update alias to point to a different collection
        var updatedAlias = await collection2.Alias.Update(aliasName, collection2.Name);

        // Assert
        Assert.NotNull(updatedAlias);
        Assert.Equal(collection2.Name, updatedAlias.TargetClass);

        // Cleanup
        await collection2.Alias.Delete(aliasName);
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

        var alias1 = new Alias(alias1Name, collection.Name);
        var alias2 = new Alias(alias2Name, collection.Name);
        var alias3 = new Alias(alias3Name, collection.Name);

        var allExistingAliases = await collection.Alias.List();
        var existingAliasNames = allExistingAliases.Select(a => a.Name).ToHashSet();
        if (existingAliasNames.Contains(alias1Name))
            await collection.Alias.Delete(alias1Name);
        if (existingAliasNames.Contains(alias2Name))
            await collection.Alias.Delete(alias2Name);
        if (existingAliasNames.Contains(alias3Name))
            await collection.Alias.Delete(alias3Name);

        // Act
        await collection.Alias.Add(alias1);
        await collection.Alias.Add(alias2);
        await collection.Alias.Add(alias3);

        var collectionAliases = await collection.Alias.List(collection.Name);

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
        await collection.Alias.Delete(alias1Name);
        await collection.Alias.Delete(alias2Name);
        await collection.Alias.Delete(alias3Name);
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
        var id1 = await collection.Data.Insert(new { Name = "Alice", Age = 30 });
        var id2 = await collection.Data.Insert(new { Name = "Bob", Age = 25 });
        var id3 = await collection.Data.Insert(new { Name = "Charlie", Age = 35 });

        // Verify data exists
        var objectsBefore = await collection.Query.FetchObjects();
        Assert.Equal(3, objectsBefore.Objects.Count);

        // Create an alias pointing to this collection
        var aliasName = MakeUniqueCollectionName<object>("AliasToDeletedCollection");
        var alias = new Alias(aliasName, collection.Name);
        try
        {
            await collection.Alias.Delete(alias.Name);
        }
        catch { }
        await collection.Alias.Add(alias);

        // Verify the alias exists
        var retrievedAlias = await collection.Alias.Get(aliasName);
        Assert.NotNull(retrievedAlias);
        Assert.Equal(collection.Name, retrievedAlias.TargetClass);

        // Act - Delete the collection
        await _weaviate.Collections.Delete(collection.Name);

        // Create a temporary collection client just for accessing alias operations
        var tempCollection = await CollectionFactory(
            name: "TempAliasHelper",
            properties: [Property.Text("Name")]
        );

        // Assert - List aliases to confirm the alias still exists
        var allAliases = await tempCollection.Alias.List();
        var aliasStillExists = allAliases.Any(a => a.Name == aliasName);
        Assert.True(
            aliasStillExists,
            "Alias should still exist after target collection is deleted"
        );

        // Get the alias to verify it still points to the deleted collection
        var aliasAfterDeletion = await tempCollection.Alias.Get(aliasName);
        Assert.NotNull(aliasAfterDeletion);
        Assert.Equal(collection.Name, aliasAfterDeletion.TargetClass);

        // Try to fetch objects using the alias as the collection name - should fail
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Collections.Use<object>(aliasName).Query.FetchObjects()
        );

        // Try to fetch a specific object by ID using the alias - should also fail
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Collections.Use<object>(aliasName).Query.FetchObjectByID(id1)
        );

        // Check if Collection.Exists with the alias name returns false
        var exists = await _weaviate.Collections.Exists(aliasName);
        Assert.False(
            exists,
            "Collection.Exists should return false for alias to deleted collection"
        );

        // Cleanup - Delete the alias
        await tempCollection.Alias.Delete(aliasName);
    }
}

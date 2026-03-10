namespace Weaviate.Client.Tests.Integration;

using Weaviate.Client.Models;

/// <summary>
/// The test collection shards class
/// </summary>
/// <seealso cref="IntegrationTests"/>
public class TestCollectionShards : IntegrationTests
{
    /// <summary>
    /// The test data class
    /// </summary>
    private class TestData { }

    /// <summary>
    /// Tests that test should get collection shards
    /// </summary>
    [Fact]
    public async Task Test_Should_Get_Collection_Shards()
    {
        // Create a collection - it will have at least one shard
        var collection = await CollectionFactory<TestData>(
            name: "ShardsTest",
            properties: [Property.Text("Name")]
        );

        // Act: Get all shards
        var shards = await collection.Config.GetShards(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(shards);
        Assert.NotEmpty(shards);
        Assert.All(
            shards,
            shard =>
            {
                Assert.NotNull(shard.Name);
                Assert.NotEmpty(shard.Name);
                // Status is an enum, so it always has a value
                Assert.True(Enum.IsDefined(typeof(ShardStatus), shard.Status));
            }
        );
    }

    /// <summary>
    /// Tests that test should get specific shard
    /// </summary>
    [Fact]
    public async Task Test_Should_Get_Specific_Shard()
    {
        // Arrange: Create collection and get the first shard name
        var collection = await CollectionFactory<TestData>(
            name: "SpecificShardTest",
            properties: [Property.Text("Name")]
        );

        var allShards = await collection.Config.GetShards(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotEmpty(allShards);

        var firstShardName = allShards[0].Name;

        // Act: Get the specific shard
        var shard = await collection.Config.GetShard(
            firstShardName,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(shard);
        Assert.Equal(firstShardName, shard.Name);
        Assert.True(Enum.IsDefined(typeof(ShardStatus), shard.Status));
    }

    /// <summary>
    /// Tests that test should update single shard status to read only
    /// </summary>
    [Fact]
    public async Task Test_Should_Update_Single_Shard_Status_To_ReadOnly()
    {
        // Arrange: Create collection and get the first shard name
        var collection = await CollectionFactory<TestData>(
            name: "UpdateShardTest",
            properties: [Property.Text("Name")]
        );

        var allShards = await collection.Config.GetShards(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotEmpty(allShards);

        var firstShardName = allShards[0].Name;

        // Act: Update shard status to READONLY
        var updatedShards = await collection.Config.UpdateShardStatus(
            ShardStatus.ReadOnly,
            firstShardName,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(updatedShards);
        Assert.Single(updatedShards);
        Assert.Equal(firstShardName, updatedShards[0].Name);
        Assert.Equal(ShardStatus.ReadOnly, updatedShards[0].Status);

        // Cleanup: Set it back to READY
        await collection.Config.UpdateShardStatus(
            ShardStatus.Ready,
            firstShardName,
            TestContext.Current.CancellationToken
        );
    }

    /// <summary>
    /// Tests that test should update shard status back to ready
    /// </summary>
    [Fact]
    public async Task Test_Should_Update_Shard_Status_Back_To_Ready()
    {
        // Arrange: Create collection, get shard, and set it to READONLY
        var collection = await CollectionFactory<TestData>(
            name: "UpdateShardBackTest",
            properties: [Property.Text("Name")]
        );

        var allShards = await collection.Config.GetShards(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotEmpty(allShards);

        var firstShardName = allShards[0].Name;

        // Set to READONLY first
        await collection.Config.UpdateShardStatus(
            ShardStatus.ReadOnly,
            firstShardName,
            TestContext.Current.CancellationToken
        );

        // Act: Update back to READY
        var updatedShards = await collection.Config.UpdateShardStatus(
            ShardStatus.Ready,
            firstShardName,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(updatedShards);
        Assert.Single(updatedShards);
        Assert.Equal(firstShardName, updatedShards[0].Name);
        Assert.Equal(ShardStatus.Ready, updatedShards[0].Status);
    }

    /// <summary>
    /// Tests that test should update multiple shards with params
    /// </summary>
    [Fact]
    public async Task Test_Should_Update_Multiple_Shards_With_Params()
    {
        // Arrange: Create collection with multiple shards if possible
        var collection = await CollectionFactory<TestData>(
            name: "MultipleShardTest",
            properties: [Property.Text("Name")],
            shardingConfig: new ShardingConfig { DesiredCount = 2 }
        );

        var allShards = await collection.Config.GetShards(
            cancellationToken: TestContext.Current.CancellationToken
        );

        // If we only have one shard, just test with that one
        var shardNames = allShards.Select(s => s.Name).ToArray();

        // Act: Update all shards to READONLY
        var updatedShards = await collection.Config.UpdateShardStatus(
            ShardStatus.ReadOnly,
            shardNames,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(updatedShards);
        Assert.Equal(shardNames.Length, updatedShards.Count);
        Assert.All(updatedShards, shard => Assert.Equal(ShardStatus.ReadOnly, shard.Status));

        // Cleanup: Set them all back to READY
        await collection.Config.UpdateShardStatus(
            ShardStatus.Ready,
            shardNames,
            TestContext.Current.CancellationToken
        );
    }

    /// <summary>
    /// Tests that test should throw when no shard names provided
    /// </summary>
    [Fact]
    public async Task Test_Should_Throw_When_No_Shard_Names_Provided()
    {
        // Arrange
        var collection = await CollectionFactory<TestData>(
            name: "NoShardNameTest",
            properties: [Property.Text("Name")]
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await collection.Config.UpdateShardStatus(
                ShardStatus.Ready,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    /// <summary>
    /// Tests that test should throw when shard name is empty
    /// </summary>
    [Fact]
    public async Task Test_Should_Throw_When_Shard_Name_Is_Empty()
    {
        // Arrange
        var collection = await CollectionFactory<TestData>(
            name: "EmptyShardNameTest",
            properties: [Property.Text("Name")]
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await collection.Config.GetShard(
                "",
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }
}

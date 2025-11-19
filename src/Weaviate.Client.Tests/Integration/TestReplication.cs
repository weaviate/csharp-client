namespace Weaviate.Client.Tests.Integration;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

[Collection("TestReplication")]
[CollectionDefinition("TestReplication", DisableParallelization = true)]
public class TestReplication : IntegrationTests
{
    public TestReplication()
        : base()
    {
        // Replication operations are available starting from version 1.32.0
        RequireVersion("1.32.0");
    }

    // Use dedicated ports for replication test suite to avoid clashes with other running instances
    public override ushort RestPort => 8087;
    public override ushort GrpcPort => 50058;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        // Cleanup any existing replication operations before each test
        await CleanupReplicationOperations();
    }

    /// <summary>
    /// Clean up any existing replication operations before starting tests
    /// </summary>
    private async Task CleanupReplicationOperations()
    {
        try
        {
            await _weaviate.Cluster.Replications.DeleteAll();
            // Wait a bit for cleanup to complete
            await Task.Delay(500);
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    private async Task WaitForCollectionReady(string collectionName, int timeoutMs = 30000)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                if (await _weaviate.Collections.Exists(collectionName))
                {
                    return;
                }
            }
            catch
            {
                // ignore transient errors
            }
            await Task.Delay(300);
        }
        throw new TimeoutException($"Collection '{collectionName}' not ready after {timeoutMs}ms");
    }

    private static async Task<T> RetryAsync<T>(
        Func<Task<T>> action,
        int attempts = 12,
        int delayMs = 500
    )
    {
        for (int i = 0; i < attempts; i++)
        {
            try
            {
                return await action();
            }
            catch when (i < attempts - 1)
            {
                await Task.Delay(delayMs);
            }
        }
        // last attempt (will throw if it fails)
        return await action();
    }

    [Fact]
    public async Task Test_Replicate_And_Get()
    {
        // Create a collection with replication factor > 1
        var collection = await CollectionFactory(
            properties: [Property.Text("content")],
            replicationConfig: new ReplicationConfig { Factor = 1 }
        );

        var collectionName = collection.Name;

        // Ensure schema has propagated before querying nodes
        await WaitForCollectionReady(collectionName);

        // Retrieve node + shard info with retry (may lag right after creation)
        var nodes = await RetryAsync(async () =>
        {
            var n = await _weaviate.Cluster.Nodes.ListVerbose(collection: collectionName);
            if (n.Length < 2 || n[0].Shards is null || n[0].Shards.Length == 0)
            {
                throw new InvalidOperationException("Shard info not yet available");
            }
            return n;
        });

        Assert.True(nodes.Length >= 2, "Expected at least 2 nodes in cluster");
        Assert.NotNull(nodes[0].Shards);
        Assert.NotEmpty(nodes[0].Shards);

        var sourceNode = nodes[0].Name;
        var targetNode = nodes[1].Name;
        var shard = nodes[0].Shards[0].Name;

        // Start replication
        var operation = await _weaviate.Cluster.Replicate(
            new ReplicateRequest(
                Collection: collectionName,
                Shard: shard,
                SourceNode: sourceNode,
                TargetNode: targetNode,
                Type: ReplicationType.Copy
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(operation);
        Assert.NotEqual(Guid.Empty, operation.Current.Id);

        // Test Get operation
        var retrieved = await _weaviate.Cluster.Replications.Get(
            operation.Current.Id,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(retrieved);
        Assert.Equal(collectionName, retrieved.Collection);
        Assert.Equal(shard, retrieved.Shard);
        Assert.Equal(sourceNode, retrieved.SourceNode);
        Assert.Equal(targetNode, retrieved.TargetNode);
        Assert.Equal(ReplicationType.Copy, retrieved.Type);

        // Test Get with history
        var retrievedWithHistory = await _weaviate.Cluster.Replications.Get(
            operation.Current.Id,
            includeHistory: true,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(retrievedWithHistory);
        Assert.NotNull(retrievedWithHistory.StatusHistory);

        // Cleanup
        await _weaviate.Cluster.Replications.Cancel(
            operation.Current.Id,
            TestContext.Current.CancellationToken
        );
        await _weaviate.Cluster.Replications.Delete(
            operation.Current.Id,
            TestContext.Current.CancellationToken
        );
    }

    [Fact]
    public async Task Test_Replicate_And_Cancel()
    {
        // Create a collection with replication factor > 1
        var collection = await CollectionFactory(
            properties: [Property.Text("content")],
            replicationConfig: new ReplicationConfig { Factor = 1 }
        );

        var collectionName = collection.Name;

        await WaitForCollectionReady(collectionName);
        var nodes = await RetryAsync(async () =>
        {
            var n = await _weaviate.Cluster.Nodes.ListVerbose(collection: collectionName);
            if (n.Length < 2 || n[0].Shards is null || n[0].Shards.Length == 0)
            {
                throw new InvalidOperationException("Shard info not yet available");
            }
            return n;
        });

        Assert.True(nodes.Length >= 2, "Expected at least 2 nodes in cluster");
        Assert.NotNull(nodes[0].Shards);
        Assert.NotEmpty(nodes[0].Shards);

        var sourceNode = nodes[0].Name;
        var targetNode = nodes[1].Name;
        var shard = nodes[0].Shards[0].Name;

        // Start replication
        var operation = await _weaviate.Cluster.Replicate(
            new ReplicateRequest(
                Collection: collectionName,
                Shard: shard,
                SourceNode: sourceNode,
                TargetNode: targetNode,
                Type: ReplicationType.Copy
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(operation);

        // Cancel the operation and wait until tracker observes completion (Cancelled or Ready)
        await operation.CancelSync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(
            operation.IsCancelled,
            "Operation should report cancelled state after WaitForCompletion"
        );

        // Cleanup
        await _weaviate.Cluster.Replications.Delete(
            operation.Current.Id,
            TestContext.Current.CancellationToken
        );
    }

    [Fact]
    public async Task Test_Replicate_And_Delete()
    {
        // Create a collection with replication factor > 1
        var collection = await CollectionFactory(
            properties: [Property.Text("content")],
            replicationConfig: new ReplicationConfig { Factor = 1 }
        );

        var collectionName = collection.Name;

        await WaitForCollectionReady(collectionName);
        var nodes = await RetryAsync(async () =>
        {
            var n = await _weaviate.Cluster.Nodes.ListVerbose(collection: collectionName);
            if (n.Length < 2 || n[0].Shards is null || n[0].Shards.Length == 0)
            {
                throw new InvalidOperationException("Shard info not yet available");
            }
            return n;
        });

        Assert.True(nodes.Length >= 2, "Expected at least 2 nodes in cluster");
        Assert.NotNull(nodes[0].Shards);
        Assert.NotEmpty(nodes[0].Shards);

        var sourceNode = nodes[0].Name;
        var targetNode = nodes[1].Name;
        var shard = nodes[0].Shards[0].Name;

        // Start replication
        var operation = await _weaviate.Cluster.Replicate(
            new ReplicateRequest(
                Collection: collectionName,
                Shard: shard,
                SourceNode: sourceNode,
                TargetNode: targetNode,
                Type: ReplicationType.Copy
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(operation);
        var operationId = operation.Current.Id;

        // Cancel first via tracker then wait for cancellation completion
        await operation.CancelSync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(operation.IsCancelled, "Operation should be cancelled before deletion");

        // Delete the operation
        await _weaviate.Cluster.Replications.Delete(
            operationId,
            TestContext.Current.CancellationToken
        );

        // Verify it's deleted - poll until it's actually gone (deletion is asynchronous)
        var deleted = false;
        for (int i = 0; i < 20; i++) // Try for up to 10 seconds
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);
            try
            {
                await _weaviate.Cluster.Replications.Get(
                    operationId,
                    cancellationToken: TestContext.Current.CancellationToken
                );
            }
            catch (WeaviateNotFoundException)
            {
                deleted = true;
                break;
            }
        }
        Assert.True(deleted, "Operation should be deleted after waiting");
    }

    [Fact]
    public async Task Test_List_And_Query_Operations()
    {
        // Create a collection with replication factor > 1
        var collection = await CollectionFactory(
            properties: [Property.Text("content")],
            replicationConfig: new ReplicationConfig { Factor = 1 }
        );

        var collectionName = collection.Name;

        await WaitForCollectionReady(collectionName);
        var nodes = await RetryAsync(async () =>
        {
            var n = await _weaviate.Cluster.Nodes.ListVerbose(collection: collectionName);
            if (n.Length < 2 || n[0].Shards is null || n[0].Shards.Length == 0)
            {
                throw new InvalidOperationException("Shard info not yet available");
            }
            return n;
        });

        Assert.True(nodes.Length >= 2, "Expected at least 2 nodes in cluster");
        Assert.NotNull(nodes[0].Shards);
        Assert.NotEmpty(nodes[0].Shards);

        var sourceNode = nodes[0].Name;
        var targetNode = nodes[1].Name;
        var shard = nodes[0].Shards[0].Name;

        // Start replication
        var operation = await _weaviate.Cluster.Replicate(
            new ReplicateRequest(
                Collection: collectionName,
                Shard: shard,
                SourceNode: sourceNode,
                TargetNode: targetNode,
                Type: ReplicationType.Copy
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(operation);

        // Test ListAll
        var allOperations = await _weaviate.Cluster.Replications.ListAll(
            TestContext.Current.CancellationToken
        );
        Assert.NotNull(allOperations);
        Assert.Contains(allOperations, op => op.Id == operation.Current.Id);

        // Test List with filters
        var collectionFiltered = await _weaviate.Cluster.Replications.List(
            collection: collectionName,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(collectionFiltered);
        Assert.Contains(collectionFiltered, op => op.Id == operation.Current.Id);

        var shardFiltered = await _weaviate.Cluster.Replications.List(
            collection: collectionName,
            shard: shard,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(shardFiltered);
        Assert.Contains(shardFiltered, op => op.Id == operation.Current.Id);

        var targetFiltered = await _weaviate.Cluster.Replications.List(
            targetNode: targetNode,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(targetFiltered);

        // Cleanup
        await _weaviate.Cluster.Replications.Cancel(
            operation.Current.Id,
            TestContext.Current.CancellationToken
        );
        await _weaviate.Cluster.Replications.Delete(
            operation.Current.Id,
            TestContext.Current.CancellationToken
        );
    }

    [Fact]
    public async Task Test_DeleteAll_Operations()
    {
        // Create a collection with replication factor > 1
        var collection = await CollectionFactory(
            properties: [Property.Text("content")],
            replicationConfig: new ReplicationConfig { Factor = 1 }
        );

        var collectionName = collection.Name;

        await WaitForCollectionReady(collectionName);
        var nodes = await RetryAsync(async () =>
        {
            var n = await _weaviate.Cluster.Nodes.ListVerbose(collection: collectionName);
            if (n.Length < 2 || n[0].Shards is null || n[0].Shards.Length == 0)
            {
                throw new InvalidOperationException("Shard info not yet available");
            }
            return n;
        });

        Assert.True(nodes.Length >= 2, "Expected at least 2 nodes in cluster");
        Assert.NotNull(nodes[0].Shards);
        Assert.NotEmpty(nodes[0].Shards);

        var sourceNode = nodes[0].Name;
        var targetNode = nodes[1].Name;
        var shard = nodes[0].Shards[0].Name;

        // Start a couple of replications
        var op1 = await _weaviate.Cluster.Replicate(
            new ReplicateRequest(
                Collection: collectionName,
                Shard: shard,
                SourceNode: sourceNode,
                TargetNode: targetNode,
                Type: ReplicationType.Copy
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(op1);

        // Delete all operations
        await _weaviate.Cluster.Replications.DeleteAll(TestContext.Current.CancellationToken);

        // Verify all deleted - poll until actually gone (deletion is asynchronous)
        var allDeleted = false;
        for (int i = 0; i < 20; i++) // Try for up to 10 seconds
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);
            var operations = await _weaviate.Cluster.Replications.ListAll(
                TestContext.Current.CancellationToken
            );
            if (!operations.Any())
            {
                allDeleted = true;
                break;
            }
        }
        Assert.True(allDeleted, "All operations should be deleted after waiting");
    }

    [Fact]
    public async Task Test_Wait_For_Successful_Completion()
    {
        // Create a collection with replication factor > 1
        var collection = await CollectionFactory(
            properties: [Property.Text("content")],
            replicationConfig: new ReplicationConfig { Factor = 1 }
        );

        var collectionName = collection.Name;

        await WaitForCollectionReady(collectionName);
        var nodes = await RetryAsync(async () =>
        {
            var n = await _weaviate.Cluster.Nodes.ListVerbose(collection: collectionName);
            if (n.Length < 2 || n[0].Shards is null || n[0].Shards.Length == 0)
            {
                throw new InvalidOperationException("Shard info not yet available");
            }
            return n;
        });

        Assert.True(nodes.Length >= 2, "Expected at least 2 nodes in cluster");
        Assert.NotNull(nodes[0].Shards);
        Assert.NotEmpty(nodes[0].Shards);

        var sourceNode = nodes[0].Name;
        var targetNode = nodes[1].Name;
        var shard = nodes[0].Shards[0].Name;

        // Start replication
        var operation = await _weaviate.Cluster.Replicate(
            new ReplicateRequest(
                Collection: collectionName,
                Shard: shard,
                SourceNode: sourceNode,
                TargetNode: targetNode,
                Type: ReplicationType.Copy
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(operation);

        // Wait for successful completion
        var finalOp = await operation.WaitForCompletion(
            TimeSpan.FromSeconds(60),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(finalOp);
        Assert.True(operation.IsCompleted, "Operation should be completed");
        Assert.True(operation.IsSuccessful, "Operation should complete successfully (READY state)");
        Assert.Equal(ReplicationOperationState.Ready, operation.Current.Status.State);

        // Cleanup
        await _weaviate.Cluster.Replications.Delete(
            operation.Current.Id,
            TestContext.Current.CancellationToken
        );
    }

    [Fact]
    public async Task Test_External_Cancellation_Detected_By_Tracker()
    {
        // Create a collection with replication factor > 1
        var collection = await CollectionFactory(
            properties: [Property.Text("content")],
            replicationConfig: new ReplicationConfig { Factor = 1 }
        );

        var collectionName = collection.Name;

        await WaitForCollectionReady(collectionName);
        var nodes = await RetryAsync(async () =>
        {
            var n = await _weaviate.Cluster.Nodes.ListVerbose(collection: collectionName);
            if (n.Length < 2 || n[0].Shards is null || n[0].Shards.Length == 0)
            {
                throw new InvalidOperationException("Shard info not yet available");
            }
            return n;
        });

        Assert.True(nodes.Length >= 2, "Expected at least 2 nodes in cluster");
        Assert.NotNull(nodes[0].Shards);
        Assert.NotEmpty(nodes[0].Shards);

        var sourceNode = nodes[0].Name;
        var targetNode = nodes[1].Name;
        var shard = nodes[0].Shards[0].Name;

        // Start replication
        var operation = await _weaviate.Cluster.Replicate(
            new ReplicateRequest(
                Collection: collectionName,
                Shard: shard,
                SourceNode: sourceNode,
                TargetNode: targetNode,
                Type: ReplicationType.Copy
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(operation);
        var operationId = operation.Current.Id;

        // Spawn a task that waits for completion (simulating one thread)
        var waitTask = Task.Run(async () =>
            await operation.WaitForCompletion(
                TimeSpan.FromSeconds(60),
                TestContext.Current.CancellationToken
            )
        );

        // Give the wait task a moment to start
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Cancel via direct REST client call (simulating external/concurrent cancellation)
        await _weaviate.Cluster.Replications.Cancel(
            operationId,
            TestContext.Current.CancellationToken
        );

        // Wait for the tracker to detect the cancellation
        // The background refresh should pick it up within ~500ms
        var completed = await waitTask;

        Assert.NotNull(completed);
        Assert.True(operation.IsCancelled, "Tracker should detect external cancellation");
        Assert.Equal(ReplicationOperationState.Cancelled, completed.Status.State);

        // Cleanup
        await _weaviate.Cluster.Replications.Delete(
            operationId,
            TestContext.Current.CancellationToken
        );
    }

    [Fact]
    public async Task Test_Replication_Operation_Tracker_Properties()
    {
        // Create a collection with replication factor > 1
        var collection = await CollectionFactory(
            properties: [Property.Text("content")],
            replicationConfig: new ReplicationConfig { Factor = 1 }
        );

        var collectionName = collection.Name;

        await WaitForCollectionReady(collectionName);
        var nodes = await RetryAsync(async () =>
        {
            var n = await _weaviate.Cluster.Nodes.ListVerbose(collection: collectionName);
            if (n.Length < 2 || n[0].Shards is null || n[0].Shards.Length == 0)
            {
                throw new InvalidOperationException("Shard info not yet available");
            }
            return n;
        });

        Assert.True(nodes.Length >= 2, "Expected at least 2 nodes in cluster");
        Assert.NotNull(nodes[0].Shards);
        Assert.NotEmpty(nodes[0].Shards);

        var sourceNode = nodes[0].Name;
        var targetNode = nodes[1].Name;
        var shard = nodes[0].Shards[0].Name;

        // Start replication
        var operation = await _weaviate.Cluster.Replicate(
            new ReplicateRequest(
                Collection: collectionName,
                Shard: shard,
                SourceNode: sourceNode,
                TargetNode: targetNode,
                Type: ReplicationType.Copy
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(operation);
        Assert.NotNull(operation.Current);
        Assert.Equal(collectionName, operation.Current.Collection);
        Assert.Equal(shard, operation.Current.Shard);
        Assert.Equal(sourceNode, operation.Current.SourceNode);
        Assert.Equal(targetNode, operation.Current.TargetNode);
        Assert.Equal(ReplicationType.Copy, operation.Current.Type);

        // Test RefreshStatus
        await operation.RefreshStatus(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(operation.Current);

        // Cleanup
        await operation.CancelSync(cancellationToken: TestContext.Current.CancellationToken);
        // Allow READY as race (if completed before cancellation effect), but prefer Cancelled
        Assert.True(
            operation.IsCancelled || operation.IsSuccessful,
            "Operation should be cancelled or have completed successfully"
        );
        await _weaviate.Cluster.Replications.Delete(
            operation.Current.Id,
            TestContext.Current.CancellationToken
        );
    }
}

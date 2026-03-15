using Weaviate.Client.Batch;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// The server-side batch tests class
/// </summary>
/// <seealso cref="IntegrationTests"/>
[Collection("ServerSideBatchTests")]
public class ServerSideBatchTests : IntegrationTests
{
    /// <summary>
    /// Tests basic server-side batch insert
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_BasicInsert_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing Server-Side Batch Insert",
            properties: [Property<string>.New("Name"), Property<int>.New("Age")]
        );

        var testData = Enumerable
            .Range(0, 50)
            .Select(i => BatchInsertRequest.Create(new { Name = $"Person{i}", Age = 20 + i }))
            .ToList();

        // Act
        await using var batch = await collection.Batch.StartBatch(
            null,
            TestContext.Current.CancellationToken
        );
        var handles = new List<TaskHandle>();

        foreach (var item in testData)
        {
            var handle = await batch.Add(item, TestContext.Current.CancellationToken);
            handles.Add(handle);
        }

        await batch.Close(TestContext.Current.CancellationToken);

        // Assert
        var results = await Task.WhenAll(handles.Select(h => h.Result));
        Assert.All(results, r => Assert.True(r.Success));

        // Verify objects were inserted
        var count = 0;
        await foreach (
            var obj in collection.Iterator(cancellationToken: TestContext.Current.CancellationToken)
        )
        {
            count++;
        }
        Assert.Equal(50, count);
    }

    /// <summary>
    /// Tests server-side batch with acknowledgments
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_CheckAcks_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing Server-Side Batch Acks",
            properties: [Property<string>.New("Data")]
        );

        var testData = Enumerable
            .Range(0, 10)
            .Select(i => BatchInsertRequest.Create(new { Data = $"Item{i}" }))
            .ToList();

        // Act
        await using var batch = await collection.Batch.StartBatch(
            null,
            TestContext.Current.CancellationToken
        );
        var handles = new List<TaskHandle>();

        foreach (var item in testData)
        {
            handles.Add(await batch.Add(item, TestContext.Current.CancellationToken));
        }

        await batch.Close(TestContext.Current.CancellationToken);

        // Assert - all items should be acknowledged
        var acks = await Task.WhenAll(handles.Select(h => h.IsAcked));
        Assert.All(acks, ack => Assert.True(ack));
    }

    /// <summary>
    /// Tests server-side batch with custom batch size
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_CustomBatchSize_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing Server-Side Batch Custom Size",
            properties: [Property<string>.New("Name")]
        );

        var options = new BatchOptions { BatchSize = 25 };
        var testData = Enumerable
            .Range(0, 100)
            .Select(i => BatchInsertRequest.Create(new { Name = $"Item{i}" }))
            .ToList();

        // Act
        await using var batch = await collection.Batch.StartBatch(
            options,
            TestContext.Current.CancellationToken
        );
        var handles = new List<TaskHandle>();

        foreach (var item in testData)
        {
            handles.Add(await batch.Add(item, TestContext.Current.CancellationToken));
        }

        await batch.Close(TestContext.Current.CancellationToken);

        // Assert
        var results = await Task.WhenAll(handles.Select(h => h.Result));
        Assert.Equal(100, results.Length);
        Assert.All(results, r => Assert.True(r.Success));
    }

    /// <summary>
    /// Tests server-side batch with vectors
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_WithVectors_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing Server-Side Batch with Vectors",
            properties: [Property<string>.New("Content")],
            vectorConfig: Configure.Vector(v => v.SelfProvided())
        );

        var testData = Enumerable
            .Range(0, 20)
            .Select(i =>
            {
                var vector = Enumerable.Range(0, 128).Select(j => (float)j / 128).ToArray();
                return BatchInsertRequest.Create(
                    new { Content = $"Document{i}" },
                    vectors: new Vectors(vector)
                );
            })
            .ToList();

        // Act
        await using var batch = await collection.Batch.StartBatch(
            null,
            TestContext.Current.CancellationToken
        );
        var handles = new List<TaskHandle>();

        foreach (var item in testData)
        {
            handles.Add(await batch.Add(item, TestContext.Current.CancellationToken));
        }

        await batch.Close(TestContext.Current.CancellationToken);

        // Assert
        var results = await Task.WhenAll(handles.Select(h => h.Result));
        Assert.All(results, r => Assert.True(r.Success));

        // Verify objects with vectors were inserted
        var count = 0;
        await foreach (
            var obj in collection.Iterator(cancellationToken: TestContext.Current.CancellationToken)
        )
        {
            count++;
        }
        Assert.Equal(20, count);
    }

    /// <summary>
    /// Tests server-side batch retry functionality
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_Retry_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing Server-Side Batch Retry",
            properties: [Property<string>.New("Value")]
        );

        await using var batch = await collection.Batch.StartBatch(
            null,
            TestContext.Current.CancellationToken
        );

        var item = BatchInsertRequest.Create(new { Value = "TestRetry" });
        var handle = await batch.Add(item, TestContext.Current.CancellationToken);

        // Wait for the first attempt to complete
        await handle.Result;

        // Act - retry the operation
        var retryHandle = await batch.Retry(handle, TestContext.Current.CancellationToken);

        // Await the retry result before closing the batch to ensure it is processed
        var completedTask = await Task.WhenAny(
            retryHandle.Result,
            Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken)
        );
        if (completedTask != retryHandle.Result)
        {
            throw new TimeoutException("Retry result was not received within 10 seconds.");
        }

        var batchResult = await retryHandle.Result;

        await batch.Close(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(batchResult.Success);
        Assert.Equal(1, retryHandle.TimesRetried);
    }

    /// <summary>
    /// Tests server-side batch state management
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_StateManagement_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing Server-Side Batch State",
            properties: [Property<string>.New("Data")]
        );

        // Act & Assert
        await using var batch = await collection.Batch.StartBatch(
            null,
            TestContext.Current.CancellationToken
        );

        // After StartBatchAsync, state should be InFlight (stream is started)
        Assert.Equal(BatchState.InFlight, batch.State);

        var item = BatchInsertRequest.Create(new { Data = "Test" });
        await batch.Add(item, TestContext.Current.CancellationToken);

        await batch.Close(TestContext.Current.CancellationToken);

        Assert.Equal(BatchState.Closed, batch.State);

        // Attempting to add after close should throw
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await batch.Add(item, TestContext.Current.CancellationToken)
        );
    }

    /// <summary>
    /// Tests InsertManyAsync with IEnumerable of BatchInsertRequest
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_InsertManyEnumerable_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing InsertManyAsync with IEnumerable",
            properties: [Property<string>.New("Name"), Property<int>.New("Value")]
        );

        var testData = Enumerable
            .Range(0, 100)
            .Select(i => BatchInsertRequest.Create(new { Name = $"Item{i}", Value = i }))
            .ToList();

        // Act
        var response = await collection.Batch.InsertMany(
            testData,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(response);
        Assert.Equal(100, response.Objects.Count());
        Assert.All(response.Objects, e => Assert.Null(e.Error));
        Assert.All(response.Objects, e => Assert.NotNull(e.UUID));

        // Verify all objects were inserted
        var count = 0;
        await foreach (
            var obj in collection.Iterator(cancellationToken: TestContext.Current.CancellationToken)
        )
        {
            count++;
        }
        Assert.Equal(100, count);
    }

    /// <summary>
    /// Tests InsertManyAsync with POCOs (not BatchInsertRequest)
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_InsertManyPoco_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing InsertManyAsync with POCOs",
            properties: [Property<string>.New("Title"), Property<string>.New("Description")]
        );

        var testData = Enumerable
            .Range(0, 30)
            .Select(i => new { Title = $"Document{i}", Description = $"Description for doc {i}" })
            .ToList();

        // Act
        var response = await collection.Batch.InsertMany(
            testData,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(response);
        Assert.Equal(30, response.Objects.Count());
        Assert.All(response.Objects, e => Assert.Null(e.Error));

        // Verify all objects were inserted
        var count = 0;
        await foreach (
            var obj in collection.Iterator(cancellationToken: TestContext.Current.CancellationToken)
        )
        {
            count++;
        }
        Assert.Equal(30, count);
    }

    /// <summary>
    /// Tests InsertManyAsync with custom UUIDs
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_InsertManyWithCustomUuids_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing InsertManyAsync with custom UUIDs",
            properties: [Property<string>.New("Name")]
        );

        var expectedUuids = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        var testData = expectedUuids
            .Select((uuid, i) => BatchInsertRequest.Create(new { Name = $"Item{i}" }, uuid: uuid))
            .ToList();

        // Act
        var response = await collection.Batch.InsertMany(
            testData,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Objects.Count());
        Assert.All(response.Objects, e => Assert.Null(e.Error));

        // Verify UUIDs match what we requested
        var returnedUuids = response.Objects.Select(e => e.UUID).ToHashSet();
        foreach (var expectedUuid in expectedUuids)
        {
            Assert.Contains(expectedUuid, returnedUuids);
        }
    }

    /// <summary>
    /// Tests InsertManyAsync with IAsyncEnumerable
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_InsertManyAsyncEnumerable_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing InsertManyAsync with IAsyncEnumerable",
            properties: [Property<string>.New("Data")]
        );

        async IAsyncEnumerable<BatchInsertRequest> GenerateData()
        {
            for (var i = 0; i < 50; i++)
            {
                yield return BatchInsertRequest.Create(new { Data = $"AsyncItem{i}" });
                await Task.Yield(); // Simulate async work
            }
        }

        // Act
        var response = await collection.Batch.InsertMany(
            GenerateData(),
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(response);
        Assert.Equal(50, response.Objects.Count());
        Assert.All(response.Objects, e => Assert.Null(e.Error));
    }

    /// <summary>
    /// Tests InsertManyAsync with BatchOptions
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_InsertManyWithOptions_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing InsertManyAsync with options",
            properties: [Property<string>.New("Name")]
        );

        var options = new BatchOptions { BatchSize = 10, MaxRetries = 5 };

        var testData = Enumerable
            .Range(0, 50)
            .Select(i => BatchInsertRequest.Create(new { Name = $"Item{i}" }))
            .ToList();

        // Act
        var response = await collection.Batch.InsertMany(
            testData,
            options,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(response);
        Assert.Equal(50, response.Objects.Count());
        Assert.All(response.Objects, e => Assert.Null(e.Error));
    }

    /// <summary>
    /// Tests that InsertManyAsync returns proper UUIDs in handles
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_TaskHandleUuid_MatchesInserted()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing TaskHandle UUID",
            properties: [Property<string>.New("Name")]
        );

        // Act
        await using var batch = await collection.Batch.StartBatch(
            ct: TestContext.Current.CancellationToken
        );

        var request = BatchInsertRequest.Create(new { Name = "TestUuid" });
        var handle = await batch.Add(request, TestContext.Current.CancellationToken);

        await batch.Close(TestContext.Current.CancellationToken);

        // Assert
        var result = await handle.Result;
        Assert.True(result.Success);
        Assert.NotNull(handle.Uuid);

        // Verify the object exists with that UUID
        var obj = await collection.Query.FetchObjectByID(
            handle.Uuid!.Value,
            returnProperties: new[] { "Name" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(obj);
    }

    /// <summary>
    /// Tests InsertManyAsync with consistency level option
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ServerSideBatch_WithConsistencyLevel_Success()
    {
        // Arrange
        var collection = await CollectionFactory(
            description: "Testing consistency level",
            properties: [Property<string>.New("Name")]
        );

        var options = new BatchOptions { ConsistencyLevel = ConsistencyLevels.All };

        var testData = Enumerable
            .Range(0, 10)
            .Select(i => BatchInsertRequest.Create(new { Name = $"Item{i}" }))
            .ToList();

        // Act
        var response = await collection.Batch.InsertMany(
            testData,
            options,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Objects.Count());
        Assert.All(response.Objects, e => Assert.Null(e.Error));
    }
}

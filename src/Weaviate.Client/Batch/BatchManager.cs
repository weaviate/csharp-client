using System.Threading.Channels;
using Weaviate.Client.Models;

namespace Weaviate.Client.Batch
{
    /// <summary>
    /// Manages batch operations for a collection.
    /// </summary>
    public class BatchManager
    {
        /// <summary>
        /// Minimum Weaviate version for server-side batch support.
        /// </summary>
        private static readonly Version MinBatchVersion = new(1, 36, 0);

        /// <summary>
        /// Gets the value of the  client
        /// </summary>
        private WeaviateClient _client => _collectionClient.Client;

        private readonly CollectionClient _collectionClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchManager"/> class.
        /// </summary>
        /// <param name="collectionClient">The collection client</param>
        public BatchManager(CollectionClient collectionClient)
        {
            _collectionClient = collectionClient;
        }

        /// <summary>
        /// Starts a new batch operation.
        /// </summary>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A batch context for adding objects</returns>
        public async Task<BatchContext> StartBatch(
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            var grpcClient = _client.GrpcClient;
            var collectionName = _collectionClient.Name;
            var tenant = _collectionClient.Tenant;

            var context = new BatchContext(grpcClient, collectionName, tenant, options);
            await context.Start(ct);

            return context;
        }

        /// <summary>
        /// Inserts multiple objects using server-side batching.
        /// </summary>
        /// <param name="requests">The batch insert requests</param>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A batch insert response with results for each object</returns>
        [RequiresWeaviateVersion(1, 36, 0)]
        public async Task<BatchInsertResponse> InsertMany(
            IEnumerable<BatchInsertRequest> requests,
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            var results = new List<BatchInsertResponseEntry>();
            await using var batch = await StartBatch(options, ct);
            var handles = new List<(int Index, TaskHandle Handle)>();
            var index = 0;
            foreach (var req in requests)
            {
                var handle = await batch.Add(req, ct);
                handles.Add((index++, handle));
            }
            await batch.Close(ct);
            foreach (var (i, handle) in handles)
            {
                var result = await handle.Result;
                results.Add(
                    new BatchInsertResponseEntry(
                        i,
                        handle.Uuid,
                        result.Success
                            ? null
                            : new WeaviateException(result.ErrorMessage ?? "Unknown error")
                    )
                );
            }
            return new BatchInsertResponse(results);
        }

        /// <summary>
        /// Inserts multiple objects of type T by wrapping them as BatchInsertRequest and delegating.
        /// </summary>
        /// <typeparam name="T">The type of objects to insert</typeparam>
        /// <param name="items">The items to insert</param>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A batch insert response with results for each object</returns>
        [RequiresWeaviateVersion(1, 36, 0)]
        public async Task<BatchInsertResponse> InsertMany<T>(
            IEnumerable<T> items,
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            var requests = items.Select(i =>
                i is BatchInsertRequest bir ? bir : new BatchInsertRequest(i!)
            );
            return await InsertMany(requests, options, ct);
        }

        /// <summary>
        /// Inserts multiple objects using server-side batching from an async enumerable.
        /// </summary>
        /// <param name="requests">The async enumerable of batch insert requests</param>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A batch insert response with results for each object</returns>
        [RequiresWeaviateVersion(1, 36, 0)]
        public async Task<BatchInsertResponse> InsertMany(
            IAsyncEnumerable<BatchInsertRequest> requests,
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            var results = new List<BatchInsertResponseEntry>();
            await using var batch = await StartBatch(options, ct);
            var handles = new List<(int Index, TaskHandle Handle)>();
            var index = 0;

            await foreach (var req in requests.WithCancellation(ct))
            {
                var handle = await batch.Add(req, ct);
                handles.Add((index++, handle));
            }

            await batch.Close(ct);

            foreach (var (i, handle) in handles)
            {
                var result = await handle.Result;
                results.Add(
                    new BatchInsertResponseEntry(
                        i,
                        handle.Uuid,
                        result.Success
                            ? null
                            : new WeaviateException(result.ErrorMessage ?? "Unknown error")
                    )
                );
            }

            return new BatchInsertResponse(results);
        }

        /// <summary>
        /// Inserts multiple objects of type T from an async enumerable by wrapping them as BatchInsertRequest.
        /// </summary>
        /// <typeparam name="T">The type of objects to insert</typeparam>
        /// <param name="items">The async enumerable of items to insert</param>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A batch insert response with results for each object</returns>
        [RequiresWeaviateVersion(1, 36, 0)]
        public async Task<BatchInsertResponse> InsertMany<T>(
            IAsyncEnumerable<T> items,
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            async IAsyncEnumerable<BatchInsertRequest> ConvertToRequests()
            {
                await foreach (var item in items.WithCancellation(ct))
                {
                    yield return item is BatchInsertRequest bir
                        ? bir
                        : new BatchInsertRequest(item!);
                }
            }

            return await InsertMany(ConvertToRequests(), options, ct);
        }

        /// <summary>
        /// Inserts multiple objects using server-side batching from a channel reader with streaming results.
        /// Results are written to the output channel as soon as each batch operation completes.
        /// </summary>
        /// <param name="channelReader">The channel reader providing batch insert requests</param>
        /// <param name="resultWriter">The channel writer to stream results to</param>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A task that completes when all items have been processed</returns>
        [RequiresWeaviateVersion(1, 36, 0)]
        public async Task InsertMany(
            ChannelReader<BatchInsertRequest> channelReader,
            ChannelWriter<BatchInsertResponseEntry> resultWriter,
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            await using var batch = await StartBatch(options, ct);
            var pendingResults = new List<Task<(int Index, TaskHandle Handle)>>();
            var index = 0;

            // Read and submit all requests
            await foreach (var req in channelReader.ReadAllAsync(ct))
            {
                var currentIndex = index++;
                var handle = await batch.Add(req, ct);

                // Create a task that waits for the result and writes it immediately
                var resultTask = Task.Run(
                    async () =>
                    {
                        var result = await handle.Result;
                        var entry = new BatchInsertResponseEntry(
                            currentIndex,
                            handle.Uuid,
                            result.Success
                                ? null
                                : new WeaviateException(result.ErrorMessage ?? "Unknown error")
                        );
                        await resultWriter.WriteAsync(entry, ct);
                        return (currentIndex, handle);
                    },
                    ct
                );

                pendingResults.Add(resultTask);
            }

            await batch.Close(ct);

            // Wait for all results to be written
            await Task.WhenAll(pendingResults);
        }

        /// <summary>
        /// Inserts multiple objects using server-side batching from a channel reader.
        /// Accumulates all results and returns them in a BatchInsertResponse.
        /// </summary>
        /// <param name="channelReader">The channel reader providing batch insert requests</param>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A batch insert response with results for each object</returns>
        [RequiresWeaviateVersion(1, 36, 0)]
        public async Task<BatchInsertResponse> InsertMany(
            ChannelReader<BatchInsertRequest> channelReader,
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            var resultChannel = Channel.CreateUnbounded<BatchInsertResponseEntry>();
            var results = new List<BatchInsertResponseEntry>();

            // Background task to collect results
            var collectTask = Task.Run(
                async () =>
                {
                    await foreach (var entry in resultChannel.Reader.ReadAllAsync(ct))
                    {
                        results.Add(entry);
                    }
                },
                ct
            );

            // Stream results to the channel
            await InsertMany(channelReader, resultChannel.Writer, options, ct);
            resultChannel.Writer.Complete();

            // Wait for collection to finish
            await collectTask;

            // Sort by index to maintain order
            results.Sort((a, b) => a.Index.CompareTo(b.Index));

            return new BatchInsertResponse(results);
        }

        /// <summary>
        /// Inserts multiple objects of type T from a channel reader with streaming results.
        /// Results are written to the output channel as soon as each batch operation completes.
        /// </summary>
        /// <typeparam name="T">The type of objects to insert</typeparam>
        /// <param name="channelReader">The channel reader providing items to insert</param>
        /// <param name="resultWriter">The channel writer to stream results to</param>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A task that completes when all items have been processed</returns>
        [RequiresWeaviateVersion(1, 36, 0)]
        public async Task InsertMany<T>(
            ChannelReader<T> channelReader,
            ChannelWriter<BatchInsertResponseEntry> resultWriter,
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            var requestChannel = Channel.CreateUnbounded<BatchInsertRequest>();

            // Background task to convert T to BatchInsertRequest
            var convertTask = Task.Run(
                async () =>
                {
                    try
                    {
                        await foreach (var item in channelReader.ReadAllAsync(ct))
                        {
                            var request = item is BatchInsertRequest bir
                                ? bir
                                : new BatchInsertRequest(item!);
                            await requestChannel.Writer.WriteAsync(request, ct);
                        }
                    }
                    finally
                    {
                        requestChannel.Writer.Complete();
                    }
                },
                ct
            );

            // Stream results
            await InsertMany(requestChannel.Reader, resultWriter, options, ct);

            // Ensure conversion is complete
            await convertTask;
        }

        /// <summary>
        /// Inserts multiple objects of type T from a channel reader by wrapping them as BatchInsertRequest.
        /// Accumulates all results and returns them in a BatchInsertResponse.
        /// </summary>
        /// <typeparam name="T">The type of objects to insert</typeparam>
        /// <param name="channelReader">The channel reader providing items to insert</param>
        /// <param name="options">Optional batch configuration</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A batch insert response with results for each object</returns>
        [RequiresWeaviateVersion(1, 36, 0)]
        public async Task<BatchInsertResponse> InsertMany<T>(
            ChannelReader<T> channelReader,
            BatchOptions? options = null,
            CancellationToken ct = default
        )
        {
            await _client.EnsureVersion<BatchManager>();
            var requestChannel = Channel.CreateUnbounded<BatchInsertRequest>();

            // Background task to convert T to BatchInsertRequest
            var convertTask = Task.Run(
                async () =>
                {
                    try
                    {
                        await foreach (var item in channelReader.ReadAllAsync(ct))
                        {
                            var request = item is BatchInsertRequest bir
                                ? bir
                                : new BatchInsertRequest(item!);
                            await requestChannel.Writer.WriteAsync(request, ct);
                        }
                    }
                    finally
                    {
                        requestChannel.Writer.Complete();
                    }
                },
                ct
            );

            // Accumulate results
            var response = await InsertMany(requestChannel.Reader, options, ct);

            // Ensure conversion is complete
            await convertTask;

            return response;
        }
    }
}

using Weaviate.Client.Internal;
using Weaviate.Client.Models;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Grpc
{
    /// <summary>
    /// Context for batch stream operations.
    /// </summary>
    internal readonly struct BatchStreamContext
    {
        public string Collection { get; init; }
        public string? Tenant { get; init; }
    }

    /// <summary>
    /// Wraps the gRPC batch stream to abstract away protobuf types.
    /// </summary>
    internal class BatchStreamWrapper : IAsyncDisposable
    {
        private readonly global::Grpc.Core.AsyncDuplexStreamingCall<
            V1.BatchStreamRequest,
            V1.BatchStreamReply
        > _stream;

        private bool _isDisposed;

        internal BatchStreamWrapper(
            global::Grpc.Core.AsyncDuplexStreamingCall<
                V1.BatchStreamRequest,
                V1.BatchStreamReply
            > stream
        )
        {
            _stream = stream;
        }

        /// <summary>
        /// Reads the next message from the response stream.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The next message, or null if the stream has ended</returns>
        internal async Task<BatchStreamMessage?> ReadNextAsync(CancellationToken ct)
        {
            try
            {
                if (!await _stream.ResponseStream.MoveNext(ct))
                    return null;
            }
            catch (global::Grpc.Core.RpcException ex)
                when (ex.StatusCode == global::Grpc.Core.StatusCode.Cancelled)
            {
                // Stream was cancelled
                return null;
            }

            var reply = _stream.ResponseStream.Current;

            return reply.MessageCase switch
            {
                V1.BatchStreamReply.MessageOneofCase.Started => new BatchStreamStarted(),
                V1.BatchStreamReply.MessageOneofCase.ShuttingDown => new BatchStreamShuttingDown(),
                V1.BatchStreamReply.MessageOneofCase.Backoff => new BatchStreamBackoff
                {
                    BatchSize = reply.Backoff.BatchSize,
                },
                V1.BatchStreamReply.MessageOneofCase.OutOfMemory => new BatchStreamOutOfMemory
                {
                    UUIDs = reply.OutOfMemory.Uuids.ToList(),
                    Beacons = reply.OutOfMemory.Beacons.ToList(),
                    WaitTimeSeconds = reply.OutOfMemory.WaitTime,
                },
                V1.BatchStreamReply.MessageOneofCase.Acks => new BatchStreamAcks
                {
                    UUIDs = reply.Acks.Uuids.ToList(),
                    Beacons = reply.Acks.Beacons.ToList(),
                },
                V1.BatchStreamReply.MessageOneofCase.Results => new BatchStreamResults
                {
                    Errors = reply
                        .Results.Errors.Select(e => new BatchStreamError
                        {
                            UUID =
                                e.DetailCase
                                == V1.BatchStreamReply
                                    .Types
                                    .Results
                                    .Types
                                    .Error
                                    .DetailOneofCase
                                    .Uuid
                                    ? Guid.Parse(e.Uuid)
                                    : null,
                            Beacon =
                                e.DetailCase
                                == V1.BatchStreamReply
                                    .Types
                                    .Results
                                    .Types
                                    .Error
                                    .DetailOneofCase
                                    .Beacon
                                    ? e.Beacon
                                    : null,
                            Error = e.Error_,
                        })
                        .ToList(),
                    Successes = reply
                        .Results.Successes.Select(s => new BatchStreamSuccess
                        {
                            UUID =
                                s.DetailCase
                                == V1.BatchStreamReply
                                    .Types
                                    .Results
                                    .Types
                                    .Success
                                    .DetailOneofCase
                                    .Uuid
                                    ? Guid.Parse(s.Uuid)
                                    : null,
                            Beacon =
                                s.DetailCase
                                == V1.BatchStreamReply
                                    .Types
                                    .Results
                                    .Types
                                    .Success
                                    .DetailOneofCase
                                    .Beacon
                                    ? s.Beacon
                                    : null,
                        })
                        .ToList(),
                },
                V1.BatchStreamReply.MessageOneofCase.None => null,
                _ => throw new InvalidOperationException(
                    $"Unexpected batch stream message type: {reply.MessageCase}"
                ),
            };
        }

        /// <summary>
        /// Sends a batch of objects to the server.
        /// </summary>
        /// <param name="requests">The batch requests (must have UUID filled in)</param>
        /// <param name="context">The collection/tenant context</param>
        /// <param name="ct">Cancellation token</param>
        internal async Task SendObjectsAsync(
            IEnumerable<BatchInsertRequest> requests,
            BatchStreamContext context,
            CancellationToken ct
        )
        {
            var dataRequest = new V1.BatchStreamRequest
            {
                Data = new V1.BatchStreamRequest.Types.Data
                {
                    Objects = new V1.BatchStreamRequest.Types.Data.Types.Objects(),
                },
            };

            foreach (var request in requests)
            {
                var batchObj = ConvertToBatchObject(request, context);
                dataRequest.Data.Objects.Values.Add(batchObj);
            }

            await _stream.RequestStream.WriteAsync(dataRequest, ct);
        }

        /// <summary>
        /// Sends a batch of references to the server.
        /// </summary>
        /// <param name="references">The references to send</param>
        /// <param name="context">The collection/tenant context</param>
        /// <param name="ct">Cancellation token</param>
        internal async Task SendReferencesAsync(
            IEnumerable<DataReference> references,
            BatchStreamContext context,
            CancellationToken ct
        )
        {
            var dataRequest = new V1.BatchStreamRequest
            {
                Data = new V1.BatchStreamRequest.Types.Data
                {
                    References = new V1.BatchStreamRequest.Types.Data.Types.References(),
                },
            };

            foreach (var r in references)
            foreach (var toUuid in r.To)
            {
                dataRequest.Data.References.Values.Add(
                    new V1.BatchReference
                    {
                        Name = r.FromProperty,
                        FromCollection = r.FromCollection ?? context.Collection,
                        FromUuid = r.From.ToString(),
                        ToCollection = r.ToCollection ?? string.Empty,
                        ToUuid = toUuid.ToString(),
                        Tenant = context.Tenant ?? string.Empty,
                    }
                );
            }

            await _stream.RequestStream.WriteAsync(dataRequest, ct);
        }

        /// <summary>
        /// Sends a stop message to the server and completes the request stream.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        internal async Task SendStopAsync(CancellationToken ct)
        {
            try
            {
                var stopRequest = new V1.BatchStreamRequest
                {
                    Stop = new V1.BatchStreamRequest.Types.Stop(),
                };
                await _stream.RequestStream.WriteAsync(stopRequest, ct);
                await _stream.RequestStream.CompleteAsync();
            }
            catch (global::Grpc.Core.RpcException ex)
                when (ex.StatusCode == global::Grpc.Core.StatusCode.Cancelled
                    || ex.StatusCode == global::Grpc.Core.StatusCode.DeadlineExceeded
                )
            {
                // Server already closed the stream, ignore to avoid hanging
            }
            catch (System.Net.Http.HttpProtocolException)
            {
                // HTTP/2 server reset the stream, ignore to avoid hanging
            }
        }

        /// <summary>
        /// Converts a BatchInsertRequest to a protobuf BatchObject.
        /// </summary>
        private static V1.BatchObject ConvertToBatchObject(
            BatchInsertRequest request,
            BatchStreamContext context
        )
        {
            var batchObj = new V1.BatchObject
            {
                Collection = context.Collection,
                Uuid = request.UUID!.Value.ToString(),
                Tenant = context.Tenant ?? string.Empty,
            };

            // Build properties from data
            if (request.Data != null)
            {
                batchObj.Properties = ObjectHelper.BuildBatchProperties(request.Data);
            }

            // Add references if present
            if (request.References != null)
            {
                foreach (var reference in request.References)
                {
                    var strp = new V1.BatchObject.Types.SingleTargetRefProps
                    {
                        PropName = reference.Name,
                        Uuids = { reference.TargetID.Select(id => id.ToString()) },
                    };
                    batchObj.Properties ??= new V1.BatchObject.Types.Properties();
                    batchObj.Properties.SingleTargetRefProps.Add(strp);
                }
            }

            // Add vectors if present
            if (request.Vectors != null)
            {
                batchObj.Vectors.AddRange(
                    request.Vectors.Select(kvp =>
                    {
                        var v = kvp.Value;
                        return new V1.Vectors
                        {
                            Name = kvp.Key,
                            VectorBytes = v.ToByteString(),
                            Type = v.IsMultiVector
                                ? V1.Vectors.Types.VectorType.MultiFp32
                                : V1.Vectors.Types.VectorType.SingleFp32,
                        };
                    })
                );
            }

            return batchObj;
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _stream.Dispose();
            _isDisposed = true;
            await Task.CompletedTask;
        }
    }
}

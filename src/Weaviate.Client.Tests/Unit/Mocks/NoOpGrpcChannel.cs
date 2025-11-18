using Grpc.Net.Client;

namespace Weaviate.Client.Tests.Unit.Mocks;

/// <summary>
/// Factory for creating a no-op GrpcChannel that doesn't make any network calls.
/// Used for unit testing gRPC client instantiation without requiring a running server.
/// Automatically handles health check requests by returning a SERVING status.
/// </summary>
internal static class NoOpGrpcChannel
{
    private static readonly Uri NoOpAddress = new("http://localhost:50051");

    /// <summary>
    /// Creates a GrpcChannel with a handler that returns successful health check responses
    /// and allows optional custom handling for other requests.
    /// </summary>
    /// <param name="customHandler">Optional custom handler for non-health-check requests.</param>
    public static GrpcChannel Create(
        Func<HttpRequestMessage, HttpResponseMessage?>? customHandler = null,
        Func<
            HttpRequestMessage,
            CancellationToken,
            Task<HttpResponseMessage?>
        >? customAsyncHandler = null
    )
    {
        return GrpcChannel.ForAddress(
            NoOpAddress,
            new GrpcChannelOptions
            {
                HttpHandler = new NoOpHttpHandler(customHandler, customAsyncHandler),
            }
        );
    }

    /// <summary>
    /// An HttpMessageHandler that returns mock gRPC responses without making network calls.
    /// Automatically handles health check requests and allows custom handling for other requests.
    /// </summary>
    private class NoOpHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage?>? _customHandler;
        private readonly Func<
            HttpRequestMessage,
            CancellationToken,
            Task<HttpResponseMessage?>
        >? _customAsyncHandler;

        public NoOpHttpHandler(
            Func<HttpRequestMessage, HttpResponseMessage?>? customHandler = null,
            Func<
                HttpRequestMessage,
                CancellationToken,
                Task<HttpResponseMessage?>
            >? customAsyncHandler = null
        )
        {
            _customHandler = customHandler;
            _customAsyncHandler = customAsyncHandler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            // First check async handler (supports cancellation-aware delays)
            if (_customAsyncHandler != null)
            {
                var asyncResponse = await _customAsyncHandler(request, cancellationToken);
                if (asyncResponse != null)
                {
                    return asyncResponse;
                }
            }

            // Then check custom sync handler
            if (_customHandler != null)
            {
                var customResponse = _customHandler(request);
                if (customResponse != null)
                {
                    return customResponse;
                }
            }

            // Check if this is a health check request
            if (IsHealthCheckRequest(request))
            {
                return CreateServingHealthCheckResponse();
            }

            // For any other gRPC call, throw an exception
            throw new InvalidOperationException(
                $"No mock response configured for {request.Method} {request.RequestUri}"
            );
        }

        private static bool IsHealthCheckRequest(HttpRequestMessage request)
        {
            return request.RequestUri?.PathAndQuery.Contains("/grpc.health.v1.Health/Check")
                == true;
        }

        private static HttpResponseMessage CreateServingHealthCheckResponse()
        {
            var healthCheckResponse = new global::Grpc.Health.V1.HealthCheckResponse
            {
                Status = global::Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.Serving,
            };

            return Helpers.CreateGrpcResponse(healthCheckResponse);
        }
    }
}

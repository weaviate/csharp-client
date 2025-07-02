using System.Collections;
using System.Dynamic;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public partial class WeaviateGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    internal Metadata? _defaultHeaders = null;
    private readonly V1.Weaviate.WeaviateClient _grpcClient;

    AsyncAuthInterceptor _AuthInterceptorFactory(string apiKey)
    {
        return (
            async (context, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {apiKey}");
                await Task.CompletedTask;
            }
        );
    }

    public WeaviateGrpcClient(Uri grpcUri, string? apiKey = null, string? wcdHost = null)
    {
        var options = new GrpcChannelOptions();

        if (apiKey != null)
        {
            var credentials = CallCredentials.FromInterceptor(_AuthInterceptorFactory(apiKey));
            options.Credentials = ChannelCredentials.Create(new SslCredentials(), credentials);
        }

        _channel = GrpcChannel.ForAddress(grpcUri, options);
        var healthClient = new Health.HealthClient(_channel);
        var request = new HealthCheckRequest();
        try
        {
            var response = healthClient.Check(request);

            // Check if service is serving
            if (response.Status != HealthCheckResponse.Types.ServingStatus.Serving)
            {
                throw new WeaviateException(
                    "GRPC health checkk failed and "
                        + grpcUri.AbsoluteUri
                        + " is not reachable. Please check if the Weaviate instance is running and accessible. Details: "
                        + response.Status
                );
            }
        }
        catch (RpcException ex)
        {
            // Handle gRPC specific exceptions
            throw new WeaviateException(
                "GRPC health checkk failed and "
                    + grpcUri.AbsoluteUri
                    + " is not reachable. Please check if the Weaviate instance is running and accessible. Details:"
                    + ex.Status.Detail
            );
        }
        // Create default headers
        if (!string.IsNullOrEmpty(wcdHost))
        {
            _defaultHeaders = new Metadata { { "X-Weaviate-Cluster-URL", wcdHost } };
        }
        _grpcClient = new V1.Weaviate.WeaviateClient(_channel);
    }

    private static IList MakeListValue(ListValue list)
    {
        switch (list.KindCase)
        {
            case ListValue.KindOneofCase.BoolValues:
                return list.BoolValues.Values.ToArray();
            case ListValue.KindOneofCase.ObjectValues:
                return list.ObjectValues.Values.Select(v => MakeNonRefs(v)).ToArray();
            case ListValue.KindOneofCase.DateValues:
                return list.DateValues.Values.Select(v => DateTime.Parse(v)).ToArray();
            case ListValue.KindOneofCase.UuidValues:
                return list.UuidValues.Values.Select(v => Guid.Parse(v)).ToArray();
            case ListValue.KindOneofCase.TextValues:
                return list.TextValues.Values;
            case ListValue.KindOneofCase.IntValues:
                return list.IntValues.Values.FromByteString<long>().ToArray();
            case ListValue.KindOneofCase.NumberValues:
                return list.NumberValues.Values.FromByteString<double>().ToArray();
            case ListValue.KindOneofCase.None:
            default:
                return new List<object> { };
        }
    }

    private static ExpandoObject MakeNonRefs(Properties result)
    {
        var eoBase = new ExpandoObject();

        if (result is null)
        {
            return eoBase;
        }

        var eo = eoBase as IDictionary<string, object>;

        foreach (var r in result.Fields)
        {
            Value.KindOneofCase kind = r.Value.KindCase;
            switch (kind)
            {
                case Value.KindOneofCase.None:
                case Value.KindOneofCase.NullValue:
                    continue;
                case Value.KindOneofCase.NumberValue:
                    eo[r.Key] = r.Value.NumberValue;
                    break;
                case Value.KindOneofCase.StringValue:
#pragma warning disable CS0612 // Type or member is obsolete
                    eo[r.Key] = r.Value.StringValue;
#pragma warning restore CS0612 // Type or member is obsolete
                    break;
                case Value.KindOneofCase.BoolValue:
                    eo[r.Key] = r.Value.BoolValue;
                    break;
                case Value.KindOneofCase.ObjectValue:
                    eo[r.Key] = MakeNonRefs(r.Value.ObjectValue) ?? new object { };
                    break;
                case Value.KindOneofCase.ListValue:
                    eo[r.Key] = MakeListValue(r.Value.ListValue);
                    break;
                case Value.KindOneofCase.DateValue:
                    eo[r.Key] = r.Value.DateValue; // TODO Parse date here?
                    break;
                case Value.KindOneofCase.UuidValue:
                    eo[r.Key] = Guid.Parse(r.Value.UuidValue);
                    break;
                case Value.KindOneofCase.IntValue:
                    eo[r.Key] = r.Value.IntValue;
                    break;
                case Value.KindOneofCase.GeoValue:
                    eo[r.Key] = new Models.GeoCoordinate(
                        r.Value.GeoValue.Latitude,
                        r.Value.GeoValue.Longitude
                    );
                    break;
                case Value.KindOneofCase.BlobValue:
                    eo[r.Key] = r.Value.BlobValue;
                    break;
                case Value.KindOneofCase.PhoneValue:
                    eo[r.Key] = r.Value.PhoneValue;
                    break;
                case Value.KindOneofCase.TextValue:
                    eo[r.Key] = r.Value.TextValue;
                    break;
            }
        }

        return eoBase;
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}

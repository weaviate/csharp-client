using System.Collections;
using System.Dynamic;
using System.Text.Json;
using Grpc.Net.Client;
using Weaviate.Client.Rest.Dto;
using Weaviate.V1;
using static Weaviate.V1.Value;

namespace Weaviate.Client.Grpc;

public partial class WeaviateGrpcClient : IDisposable
{
    private readonly WeaviateClient _client;
    private readonly GrpcChannel _channel;
    private readonly V1.Weaviate.WeaviateClient _grpcClient;

    public WeaviateGrpcClient(WeaviateClient client)
    {
        _client = client;

        var ub = new UriBuilder(client.Configuration.Host);

        ub.Port = client.Configuration.GrpcPort;

        _channel = GrpcChannel.ForAddress(ub.Uri);
        _grpcClient = new V1.Weaviate.WeaviateClient(_channel);
    }

    private static IList buildListFromListValue(ListValue list)
    {
        switch (list.KindCase)
        {
            case ListValue.KindOneofCase.BoolValues:
                return list.BoolValues.Values;
            case ListValue.KindOneofCase.ObjectValues:
                return list.ObjectValues.Values.Select(v => buildObjectFromProperties(v)).ToList();
            case ListValue.KindOneofCase.DateValues:
                return list.DateValues.Values; // TODO Parse dates here?
            case ListValue.KindOneofCase.UuidValues:
                return list.UuidValues.Values.Select(v => Guid.Parse(v)).ToList();
            case ListValue.KindOneofCase.TextValues:
                return list.TextValues.Values;
            case ListValue.KindOneofCase.IntValues: // TODO Decode list.IntValues according to docs
            case ListValue.KindOneofCase.NumberValues: // TODO Decode list.NumberValues according to docs
            case ListValue.KindOneofCase.None:
            default:
                return new List<object> { };
        }
    }

    private static object? buildObjectFromProperties(Properties result)
    {
        var eo = new ExpandoObject() as IDictionary<string, object>;

        foreach (var r in result.Fields)
        {
            KindOneofCase kind = r.Value.KindCase;
            switch (kind)
            {
                case KindOneofCase.None:
                case KindOneofCase.NullValue:
                    continue;
                case KindOneofCase.NumberValue:
                    eo[r.Key] = r.Value.NumberValue;
                    break;
                case KindOneofCase.StringValue:
                    eo[r.Key] = r.Value.StringValue;
                    break;
                case KindOneofCase.BoolValue:
                    eo[r.Key] = r.Value.BoolValue;
                    break;
                case KindOneofCase.ObjectValue:
                    eo[r.Key] = buildObjectFromProperties(r.Value.ObjectValue) ?? new object { };
                    break;
                case KindOneofCase.ListValue:
                    eo[r.Key] = buildListFromListValue(r.Value.ListValue);
                    break;
                case KindOneofCase.DateValue:
                    eo[r.Key] = r.Value.DateValue; // TODO Parse date here?
                    break;
                case KindOneofCase.UuidValue:
                    eo[r.Key] = Guid.Parse(r.Value.UuidValue);
                    break;
                case KindOneofCase.IntValue:
                    eo[r.Key] = r.Value.IntValue;
                    break;
                case KindOneofCase.GeoValue:
                    eo[r.Key] = r.Value.GeoValue.ToString();
                    break;
                case KindOneofCase.BlobValue:
                    eo[r.Key] = r.Value.BlobValue;
                    break;
                case KindOneofCase.PhoneValue:
                    eo[r.Key] = r.Value.PhoneValue;
                    break;
                case KindOneofCase.TextValue:
                    eo[r.Key] = r.Value.TextValue;
                    break;
            }
        }

        return eo;
    }

    public void Dispose()
    {
        // throw new NotImplementedException();
    }
}
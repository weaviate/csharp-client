using System.Collections;
using System.Dynamic;
using Grpc.Net.Client;
using Weaviate.V1;

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
                return list.ObjectValues.Values.Select(v => MakeNonRefs(v)).ToList();
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
                    eo[r.Key] = r.Value.StringValue;
                    break;
                case Value.KindOneofCase.BoolValue:
                    eo[r.Key] = r.Value.BoolValue;
                    break;
                case Value.KindOneofCase.ObjectValue:
                    eo[r.Key] = MakeNonRefs(r.Value.ObjectValue) ?? new object { };
                    break;
                case Value.KindOneofCase.ListValue:
                    eo[r.Key] = buildListFromListValue(r.Value.ListValue);
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
                    eo[r.Key] = r.Value.GeoValue.ToString();
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
        // throw new NotImplementedException();
    }
}
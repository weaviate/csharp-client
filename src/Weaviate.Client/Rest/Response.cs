using System.Collections.ObjectModel;
using System.Net;

namespace Weaviate.Client.Rest;

public class Response
{
    public object? Body { get; }

    public IReadOnlyDictionary<string, string>? Headers { get; internal set; }

    public HttpStatusCode StatusCode { get; }

    public string? ContentType { get; }

}
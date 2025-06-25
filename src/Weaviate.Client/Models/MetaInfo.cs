namespace Weaviate.Client.Models;

public struct MetaInfo
{
    public string Hostname { get; set; }
    public string Version { get; set; }
    public Dictionary<string, object> Modules { get; set; }
    public int GrpcMaxMessageSize { get; set; }
}

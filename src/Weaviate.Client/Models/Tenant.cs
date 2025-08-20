namespace Weaviate.Client.Models;

public enum TenantActivityStatus
{
    [System.Runtime.Serialization.EnumMember(Value = "unspecified")]
    Unspecified,

    [System.Runtime.Serialization.EnumMember(Value = "active")]
    Active,

    [System.Runtime.Serialization.EnumMember(Value = "inactive")]
    Inactive,

    [System.Runtime.Serialization.EnumMember(Value = "offloaded")]
    Offloaded,

    [System.Runtime.Serialization.EnumMember(Value = "offloading")]
    Offloading,

    [System.Runtime.Serialization.EnumMember(Value = "onloading")]
    Onloading,

    [System.Runtime.Serialization.EnumMember(Value = "hot")]
    Hot,

    [System.Runtime.Serialization.EnumMember(Value = "cold")]
    Cold,

    [System.Runtime.Serialization.EnumMember(Value = "frozen")]
    Frozen,

    [System.Runtime.Serialization.EnumMember(Value = "freezing")]
    Freezing,

    [System.Runtime.Serialization.EnumMember(Value = "unfreezing")]
    Unfreezing,
}

public record Tenant
{
    public required string Name { get; init; }
    public TenantActivityStatus Status { get; init; } = TenantActivityStatus.Hot;

    public Tenant() { }

    public Tenant(string name, TenantActivityStatus status)
    {
        Name = name;
        Status = status;
    }

    public Tenant(string name, string status)
        : this(name, ParseStatus(status)) { }

    private static TenantActivityStatus ParseStatus(string status)
    {
        if (Enum.TryParse<TenantActivityStatus>(status, true, out var parsed))
        {
            return parsed;
        }
        throw new ArgumentException($"Invalid TenantStatus: {status}");
    }

    internal static Tenant FromGrpc(V1.Tenant t)
    {
        return new Tenant { Name = t.Name, Status = MapGrpcStatus(t.ActivityStatus) };
    }

    private static TenantActivityStatus MapGrpcStatus(V1.TenantActivityStatus grpcStatus)
    {
        return grpcStatus switch
        {
            V1.TenantActivityStatus.Unspecified => TenantActivityStatus.Unspecified,
            V1.TenantActivityStatus.Hot => TenantActivityStatus.Active,
            V1.TenantActivityStatus.Cold => TenantActivityStatus.Inactive,
            V1.TenantActivityStatus.Frozen => TenantActivityStatus.Offloaded,
            V1.TenantActivityStatus.Unfreezing => TenantActivityStatus.Onloading,
            V1.TenantActivityStatus.Freezing => TenantActivityStatus.Offloading,
            V1.TenantActivityStatus.Active => TenantActivityStatus.Active,
            V1.TenantActivityStatus.Inactive => TenantActivityStatus.Inactive,
            V1.TenantActivityStatus.Offloaded => TenantActivityStatus.Offloaded,
            V1.TenantActivityStatus.Offloading => TenantActivityStatus.Offloading,
            V1.TenantActivityStatus.Onloading => TenantActivityStatus.Onloading,
            _ => throw new ArgumentOutOfRangeException(nameof(grpcStatus), grpcStatus, null),
        };
    }
}

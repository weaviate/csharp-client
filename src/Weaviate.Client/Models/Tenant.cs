namespace Weaviate.Client.Models;

using V1 = Grpc.Protobuf.V1;

/// <summary>
/// The tenant activity status enum
/// </summary>
public enum TenantActivityStatus
{
    /// <summary>
    /// The unspecified tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "unspecified")]
    Unspecified,

    /// <summary>
    /// The active tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "active")]
    Active,

    /// <summary>
    /// The inactive tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "inactive")]
    Inactive,

    /// <summary>
    /// The offloaded tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "offloaded")]
    Offloaded,

    /// <summary>
    /// The offloading tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "offloading")]
    Offloading,

    /// <summary>
    /// The onloading tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "onloading")]
    Onloading,

    /// <summary>
    /// The hot tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "hot")]
    Hot,

    /// <summary>
    /// The cold tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "cold")]
    Cold,

    /// <summary>
    /// The frozen tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "frozen")]
    Frozen,

    /// <summary>
    /// The freezing tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "freezing")]
    Freezing,

    /// <summary>
    /// The unfreezing tenant activity status
    /// </summary>
    [System.Runtime.Serialization.EnumMember(Value = "unfreezing")]
    Unfreezing,
}

/// <summary>
/// The tenant
/// </summary>
public record Tenant
{
    /// <summary>
    /// Gets or inits the value of the name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or inits the value of the status
    /// </summary>
    public TenantActivityStatus Status { get; init; } = TenantActivityStatus.Hot;

    public static implicit operator Tenant(string name) => new() { Name = name };

    /// <summary>
    /// Initializes a new instance of the <see cref="Tenant"/> class
    /// </summary>
    public Tenant() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tenant"/> class
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="status">The status</param>
    public Tenant(string name, TenantActivityStatus status)
    {
        Name = name;
        Status = status;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tenant"/> class
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="status">The status</param>
    public Tenant(string name, string status)
        : this(name, ParseStatus(status)) { }

    /// <summary>
    /// Parses the status using the specified status
    /// </summary>
    /// <param name="status">The status</param>
    /// <exception cref="ArgumentException">Invalid TenantStatus: {status}</exception>
    /// <returns>The tenant activity status</returns>
    private static TenantActivityStatus ParseStatus(string status)
    {
        if (Enum.TryParse<TenantActivityStatus>(status, true, out var parsed))
        {
            return parsed;
        }
        throw new ArgumentException($"Invalid TenantStatus: {status}");
    }

    /// <summary>
    /// Creates the grpc using the specified t
    /// </summary>
    /// <param name="t">The </param>
    /// <returns>The tenant</returns>
    internal static Tenant FromGrpc(V1.Tenant t)
    {
        return new Tenant { Name = t.Name, Status = MapGrpcStatus(t.ActivityStatus) };
    }

    /// <summary>
    /// Maps the grpc status using the specified grpc status
    /// </summary>
    /// <param name="grpcStatus">The grpc status</param>
    /// <exception cref="ArgumentOutOfRangeException">null</exception>
    /// <returns>The tenant activity status</returns>
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

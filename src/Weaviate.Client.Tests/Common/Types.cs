using System.Runtime.Serialization;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Common;

public enum TestStatus
{
    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "inactive")]
    Inactive,

    [EnumMember(Value = "pending")]
    Pending,

    // No EnumMember attribute for this one
    Archived,
}

public record TestNestedProperties
{
    public string? TestText { get; set; }
    public int? TestInt { get; set; }
    public TestNestedProperties? TestObject { get; set; }
}

public record TestProperties
{
    public string? TestText { get; set; }
    public string[]? TestTextArray { get; set; }
    public int? TestInt { get; set; }
    public int[]? TestIntArray { get; set; }
    public bool? TestBool { get; set; }
    public bool[]? TestBoolArray { get; set; }
    public double? TestNumber { get; set; }
    public double[]? TestNumberArray { get; set; }
    public DateTime? TestDate { get; set; }
    public DateTime[]? TestDateArray { get; set; }
    public Guid? TestUuid { get; set; }
    public Guid[]? TestUuidArray { get; set; }
    public GeoCoordinate? TestGeo { get; set; }
    public byte[]? TestBlob { get; set; }
    public PhoneNumber? TestPhone { get; set; }
    public TestNestedProperties? TestObject { get; set; }
    public TestNestedProperties[]? TestObjectArray { get; set; }
    public TestStatus? TestEnum { get; set; }
}

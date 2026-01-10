using System.Runtime.Serialization;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Common;

/// <summary>
/// The test status enum
/// </summary>
public enum TestStatus
{
    /// <summary>
    /// The active test status
    /// </summary>
    [EnumMember(Value = "active")]
    Active,

    /// <summary>
    /// The inactive test status
    /// </summary>
    [EnumMember(Value = "inactive")]
    Inactive,

    /// <summary>
    /// The pending test status
    /// </summary>
    [EnumMember(Value = "pending")]
    Pending,

    // No EnumMember attribute for this one
    /// <summary>
    /// The archived test status
    /// </summary>
    Archived,
}

/// <summary>
/// The test nested properties
/// </summary>
public record TestNestedProperties
{
    /// <summary>
    /// Gets or sets the value of the test text
    /// </summary>
    public string? TestText { get; set; }

    /// <summary>
    /// Gets or sets the value of the test int
    /// </summary>
    public int? TestInt { get; set; }

    /// <summary>
    /// Gets or sets the value of the test object
    /// </summary>
    public TestNestedProperties? TestObject { get; set; }
}

/// <summary>
/// The test properties
/// </summary>
public record TestProperties
{
    /// <summary>
    /// Gets or sets the value of the test text
    /// </summary>
    public string? TestText { get; set; }

    /// <summary>
    /// Gets or sets the value of the test text array
    /// </summary>
    public string[]? TestTextArray { get; set; }

    /// <summary>
    /// Gets or sets the value of the test int
    /// </summary>
    public int? TestInt { get; set; }

    /// <summary>
    /// Gets or sets the value of the test int array
    /// </summary>
    public int[]? TestIntArray { get; set; }

    /// <summary>
    /// Gets or sets the value of the test bool
    /// </summary>
    public bool? TestBool { get; set; }

    /// <summary>
    /// Gets or sets the value of the test bool array
    /// </summary>
    public bool[]? TestBoolArray { get; set; }

    /// <summary>
    /// Gets or sets the value of the test number
    /// </summary>
    public double? TestNumber { get; set; }

    /// <summary>
    /// Gets or sets the value of the test number array
    /// </summary>
    public double[]? TestNumberArray { get; set; }

    /// <summary>
    /// Gets or sets the value of the test date
    /// </summary>
    public DateTime? TestDate { get; set; }

    /// <summary>
    /// Gets or sets the value of the test date array
    /// </summary>
    public DateTime[]? TestDateArray { get; set; }

    /// <summary>
    /// Gets or sets the value of the test uuid
    /// </summary>
    public Guid? TestUuid { get; set; }

    /// <summary>
    /// Gets or sets the value of the test uuid array
    /// </summary>
    public Guid[]? TestUuidArray { get; set; }

    /// <summary>
    /// Gets or sets the value of the test geo
    /// </summary>
    public GeoCoordinate? TestGeo { get; set; }

    /// <summary>
    /// Gets or sets the value of the test blob
    /// </summary>
    public byte[]? TestBlob { get; set; }

    /// <summary>
    /// Gets or sets the value of the test phone
    /// </summary>
    public PhoneNumber? TestPhone { get; set; }

    /// <summary>
    /// Gets or sets the value of the test object
    /// </summary>
    public TestNestedProperties? TestObject { get; set; }

    /// <summary>
    /// Gets or sets the value of the test object array
    /// </summary>
    public TestNestedProperties[]? TestObjectArray { get; set; }

    /// <summary>
    /// Gets or sets the value of the test enum
    /// </summary>
    public TestStatus? TestEnum { get; set; }
}

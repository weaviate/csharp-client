using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
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
        // // public byte[]? TestBlob { get; set; }
        // // public PhoneNumber? TestPhone { get; set; }
        // // public object? TestObject { get; set; }
        // // public object? TestObjectArray { get; set; }
    }

    [Fact]
    public async Task AllPropertiesSaveRetrieve()
    {
        Property[] props =
        [
            Property.Text("testText"),
            Property.TextArray("testTextArray"),
            Property.Int("testInt"),
            Property.IntArray("testIntArray"),
            Property.Bool("testBool"),
            Property.BoolArray("testBoolArray"),
            Property.Number("testNumber"),
            Property.NumberArray("testNumberArray"),
            Property.Date("testDate"),
            Property.DateArray("testDateArray"),
            Property.Uuid("testUuid"),
            Property.UuidArray("testUuidArray"),
            Property.GeoCoordinate("testGeo"),
            // Property.Blob("testBlob"),
            // Property.PhoneNumber("testPhone"),
            // Property.Object("testObject"),
            // Property.ObjectArray("testObjectArray"),
        ];

        // 1. Create collection
        var c = await CollectionFactory(
            description: "Testing collection properties",
            properties: props
        );

        // 2. Create an object with values for all properties
        var testData = new TestProperties
        {
            TestText = "dummyText",
            TestTextArray = new[] { "dummyTextArray1", "dummyTextArray2" },
            TestInt = 123,
            TestIntArray = new[] { 1, 2, 3 },
            TestBool = true,
            TestBoolArray = new[] { true, false },
            TestNumber = 456.789,
            TestNumberArray = new[] { 4.5, 6.7 },
            TestDate = DateTime.Now.AddDays(-1),
            TestDateArray = new[] { DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3) },
            TestUuid = Guid.NewGuid(),
            TestUuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
            TestGeo = new GeoCoordinate(12.345f, 67.890f),
        };

        var id = await c.Data.Insert(testData);

        // 3. Retrieve the object and confirm all properties match
        var retrieved = await c.Query.FetchObjectByID(id);

        var obj = retrieved.Objects.First();

        var concreteObj = obj.As<TestProperties>();

        Assert.Equivalent(testData, concreteObj);
    }
}

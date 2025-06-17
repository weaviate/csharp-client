using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

[Collection("PropertiesTests")]
public partial class PropertyTests : IntegrationTests
{
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
        var obj = await c.Query.FetchObjectByID(id);

        var concreteObj = obj?.As<TestProperties>();

        Assert.Equivalent(testData, concreteObj);
    }

    [Fact]
    public async Task Test_BatchInsert_WithArrays()
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
        var testData = new[]
        {
            new TestProperties
            {
                TestText = "dummyText1",
                TestTextArray = new[] { "dummyTextArray11", "dummyTextArray21" },
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
            },
            new TestProperties
            {
                TestText = "dummyText",
                TestTextArray = new[] { "dummyTextArray1", "dummyTextArray2" },
                TestInt = 456,
                TestIntArray = new[] { 4, 5, 6 },
                TestBool = true,
                TestBoolArray = new bool[] { },
                TestNumber = 789.987,
                TestNumberArray = new[] { 5.6, 7.8 },
                TestDate = DateTime.Now.AddDays(-2),
                TestDateArray = new[] { DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-4) },
                TestUuid = Guid.NewGuid(),
                TestUuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
                TestGeo = new GeoCoordinate(23.456f, 78.910f),
            },
            new TestProperties
            {
                TestText = "dummyText",
                TestTextArray = new[] { "dummyTextArray1", "dummyTextArray2" },
                TestInt = 345,
                TestIntArray = new[] { 3, 4, 6 },
                TestBool = true,
                TestNumber = 567.897,
                TestNumberArray = new[] { 6.7, 8.9 },
                TestDate = DateTime.Now.AddDays(+1),
                TestDateArray = new[] { DateTime.Now.AddDays(+2), DateTime.Now.AddDays(+3) },
                TestUuid = Guid.NewGuid(),
                TestUuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
                TestGeo = new GeoCoordinate(34.567f, 98.765f),
            },
        };

        var response = await c.Data.InsertMany(batcher =>
        {
            testData.ToList().ForEach(d => batcher(d));
        });

        // 3. Retrieve the object and confirm all properties match
        foreach (var r in response)
        {
            var obj = await c.Query.FetchObjectByID(r.ID!.Value);

            Assert.NotNull(obj);

            var concreteObj = obj.As<TestProperties>();

            Assert.Equivalent(testData[r.Index], concreteObj);
        }
    }
}

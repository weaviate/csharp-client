using Weaviate.Client.Models;
using Weaviate.Client.Tests.Common;

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
            // TestPhone = new PhoneNumber(),
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

        var response = await c.Data.InsertMany(BatchInsertRequest.Create<object>(testData));

        // 3. Retrieve the object and confirm all properties match
        foreach (var r in response)
        {
            var obj = await c.Query.FetchObjectByID(r.ID!.Value);

            Assert.NotNull(obj);

            var concreteObj = obj.As<TestProperties>();

            Assert.Equivalent(testData[r.Index], concreteObj);
        }
    }

    [Fact]
    public async Task Test_Properties_Extra_Options()
    {
        Property[] props =
        [
            Property.Text(
                "Name",
                "Some Description",
                true,
                false,
                true,
                PropertyTokenization.Lowercase
            ),
            Property.Text(
                "Nickname",
                "No Description",
                indexFilterable: true,
                indexRangeFilters: false,
                indexSearchable: false,
                tokenization: PropertyTokenization.Lowercase
            ),
            Property.Int(
                "Age",
                "In years",
                indexFilterable: true,
                indexRangeFilters: true,
                indexSearchable: false,
                tokenization: null
            ),
        ];

        // 1. Create collection
        var c = await CollectionFactory(
            description: "Testing collection properties",
            properties: props
        );

        var export = await _weaviate.Collections.Export(c.Name);

        Assert.NotNull(export);
        Assert.Equal(3, export.Properties.Length);

        // var propA = export.Properties.First(p => p.Name == "Name");
        var propA = export.Properties[0];

        Assert.Equal("name", propA.Name);
        Assert.Equal("Some Description", propA.Description);
        Assert.True(propA.IndexFilterable);
        Assert.False(propA.IndexRangeFilters);
        Assert.True(propA.IndexSearchable);
#pragma warning disable CS0612 // Type or member is obsolete
        Assert.Null(propA.IndexInverted);
#pragma warning restore CS0612 // Type or member is obsolete

        // var propB = export.Properties.First(p => p.Name == "Nickname");
        var propB = export.Properties[1];
        Assert.Equal("nickname", propB.Name);
        Assert.True(propB.IndexFilterable);
        Assert.False(propB.IndexRangeFilters);
        Assert.False(propB.IndexSearchable);
#pragma warning disable CS0612 // Type or member is obsolete
        Assert.Null(propB.IndexInverted);
#pragma warning restore CS0612 // Type or member is obsolete

        var propC = export.Properties[2];
        Assert.Equal("age", propC.Name);
        Assert.True(propC.IndexFilterable);
        Assert.True(propC.IndexRangeFilters);
        Assert.False(propC.IndexSearchable);
#pragma warning disable CS0612 // Type or member is obsolete
        Assert.Null(propC.IndexInverted);
#pragma warning restore CS0612 // Type or member is obsolete
    }

    [Fact]
    public async Task Test_FloatingPoint_Precision_SingleValue()
    {
        // Test that floating point values are stored and retrieved with sufficient precision
        // This addresses the issue where 599.99 was stored as 599.989989082349082
        Property[] props =
        [
            Property.Number("price"),
            Property.Number("measurement"),
            Property.Number("ratio"),
        ];

        var c = await CollectionFactory(
            description: "Testing floating point precision",
            properties: props
        );

        // Test various problematic floating point values
        var testCases = new[]
        {
            new
            {
                price = 599.99,
                measurement = 0.1,
                ratio = 1.23456789,
            },
            new
            {
                price = 123.45,
                measurement = 0.01,
                ratio = 9.87654321,
            },
            new
            {
                price = 999.99,
                measurement = 0.001,
                ratio = 3.14159265,
            },
            new
            {
                price = 1234.56,
                measurement = 100.5,
                ratio = 2.71828182,
            },
            new
            {
                price = 0.99,
                measurement = 1000.001,
                ratio = 0.123456789,
            },
        };

        foreach (var testData in testCases)
        {
            var id = await c.Data.Insert(testData);
            var obj = await c.Query.FetchObjectByID(id);

            Assert.NotNull(obj);

            // Verify the values are within acceptable tolerance (6 decimal places)
            var retrievedPrice = (double)obj.Properties["price"]!;
            var retrievedMeasurement = (double)obj.Properties["measurement"]!;
            var retrievedRatio = (double)obj.Properties["ratio"]!;

            // Use a tolerance of 1e-6 for comparison (6 decimal places)
            Assert.True(
                Math.Abs(testData.price - retrievedPrice) < 1e-6,
                $"Price mismatch: expected {testData.price}, got {retrievedPrice}"
            );
            Assert.True(
                Math.Abs(testData.measurement - retrievedMeasurement) < 1e-6,
                $"Measurement mismatch: expected {testData.measurement}, got {retrievedMeasurement}"
            );
            Assert.True(
                Math.Abs(testData.ratio - retrievedRatio) < 1e-6,
                $"Ratio mismatch: expected {testData.ratio}, got {retrievedRatio}"
            );
        }
    }

    [Fact]
    public async Task Test_FloatingPoint_Precision_Arrays()
    {
        // Test that floating point arrays maintain precision
        Property[] props = [Property.NumberArray("prices"), Property.NumberArray("measurements")];

        var c = await CollectionFactory(
            description: "Testing floating point array precision",
            properties: props
        );

        var testData = new
        {
            prices = new[] { 599.99, 123.45, 999.99, 0.99 },
            measurements = new[] { 0.1, 0.01, 0.001, 100.5, 1000.001 },
        };

        var id = await c.Data.Insert(testData);
        var obj = await c.Query.FetchObjectByID(id);

        Assert.NotNull(obj);

        // The properties are returned as double[] arrays
        var pricesArray = obj.Properties["prices"] as double[];
        var measurementsArray = obj.Properties["measurements"] as double[];

        Assert.NotNull(pricesArray);
        Assert.NotNull(measurementsArray);

        // Verify array lengths
        Assert.Equal(testData.prices.Length, pricesArray.Length);
        Assert.Equal(testData.measurements.Length, measurementsArray.Length);

        // Verify each value is within acceptable tolerance
        for (int i = 0; i < testData.prices.Length; i++)
        {
            Assert.True(
                Math.Abs(testData.prices[i] - pricesArray[i]) < 1e-6,
                $"Price[{i}] mismatch: expected {testData.prices[i]}, got {pricesArray[i]}"
            );
        }

        for (int i = 0; i < testData.measurements.Length; i++)
        {
            Assert.True(
                Math.Abs(testData.measurements[i] - measurementsArray[i]) < 1e-6,
                $"Measurement[{i}] mismatch: expected {testData.measurements[i]}, got {measurementsArray[i]}"
            );
        }
    }

    [Fact]
    public async Task Test_FloatingPoint_Precision_BatchInsert()
    {
        // Test floating point precision with batch insert operations
        Property[] props = [Property.Number("amount"), Property.NumberArray("values")];

        var c = await CollectionFactory(
            description: "Testing floating point precision in batch operations",
            properties: props
        );

        var testData = new[]
        {
            new { amount = 599.99, values = new[] { 1.11, 2.22, 3.33 } },
            new { amount = 123.45, values = new[] { 4.44, 5.55, 6.66 } },
            new { amount = 999.99, values = new[] { 7.77, 8.88, 9.99 } },
        };

        var response = await c.Data.InsertMany(BatchInsertRequest.Create<object>(testData));

        // Verify each inserted object
        foreach (var r in response)
        {
            var obj = await c.Query.FetchObjectByID(r.ID!.Value);
            Assert.NotNull(obj);

            var retrievedAmount = (double)obj.Properties["amount"]!;
            var retrievedValues = obj.Properties["values"] as double[];

            Assert.NotNull(retrievedValues);

            Assert.True(
                Math.Abs(testData[r.Index].amount - retrievedAmount) < 1e-6,
                $"Batch[{r.Index}] amount mismatch: expected {testData[r.Index].amount}, got {retrievedAmount}"
            );

            for (int j = 0; j < testData[r.Index].values.Length; j++)
            {
                Assert.True(
                    Math.Abs(testData[r.Index].values[j] - retrievedValues[j]) < 1e-6,
                    $"Batch[{r.Index}] values[{j}] mismatch: expected {testData[r.Index].values[j]}, got {retrievedValues[j]}"
                );
            }
        }
    }

    [Fact]
    public async Task Test_FloatingPoint_EdgeCases()
    {
        // Test edge cases for floating point values
        Property[] props = [Property.Number("value")];

        var c = await CollectionFactory(
            description: "Testing floating point edge cases",
            properties: props
        );

        // Test various edge cases
        var edgeCases = new[]
        {
            0.0,
            -0.0,
            1.0,
            -1.0,
            0.123456789012345, // Many decimal places
            -0.123456789012345,
            1e-10, // Very small positive
            -1e-10, // Very small negative
            1e10, // Very large positive
            -1e10, // Very large negative
            Math.PI,
            Math.E,
        };

        foreach (var testValue in edgeCases)
        {
            var id = await c.Data.Insert(new { value = testValue });
            var obj = await c.Query.FetchObjectByID(id);

            Assert.NotNull(obj);

            var retrievedValue = (double)obj.Properties["value"]!;

            // For very small numbers, use relative tolerance; otherwise use absolute
            var tolerance = Math.Abs(testValue) < 1e-6 ? 1e-15 : 1e-6;
            var diff = Math.Abs(testValue - retrievedValue);

            Assert.True(
                diff < tolerance,
                $"Edge case mismatch: expected {testValue}, got {retrievedValue}, diff {diff}"
            );
        }
    }
}

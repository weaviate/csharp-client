using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The property tests class
/// </summary>
public partial class PropertyTests
{
    /// <summary>
    /// Gets the value of the property cases keys
    /// </summary>
    public static TheoryData<string> PropertyCasesKeys => [.. PropertyCases.Keys];

    /// <summary>
    /// The new
    /// </summary>
    private static Dictionary<string, (PropertyFactory, PropertyFactory)> PropertyCases = new()
    {
        ["Text"] = (Property.Text, Property<string>.New),
        ["TextArray"] = (Property.TextArray, Property<string[]>.New),
        ["Int"] = (Property.Int, Property<int>.New),
        ["IntArray"] = (Property.IntArray, Property<int[]>.New),
        ["Bool"] = (Property.Bool, Property<bool>.New),
        ["BoolArray"] = (Property.BoolArray, Property<bool[]>.New),
        ["Number"] = (Property.Number, Property<double>.New),
        ["NumberArray"] = (Property.NumberArray, Property<double[]>.New),
        ["Date"] = (Property.Date, Property<DateTime>.New),
        ["DateArray"] = (Property.DateArray, Property<DateTime[]>.New),
        ["Uuid"] = (Property.Uuid, Property<Guid>.New),
        ["UuidArray"] = (Property.UuidArray, Property<Guid[]>.New),
        ["Geo"] = (Property.GeoCoordinate, Property<GeoCoordinate>.New),
        ["Phone"] = (Property.PhoneNumber, Property<PhoneNumber>.New),
        ["Blob"] = (Property.Blob, Property<byte[]>.New),
        ["Object"] = (Property.Object, Property<object>.New),
        ["ObjectArray"] = (Property.ObjectArray, Property<object[]>.New),
    };

    /// <summary>
    /// Tests that property data type helpers
    /// </summary>
    /// <param name="key">The key</param>
    [Theory]
    [MemberData(nameof(PropertyCasesKeys))]
    public void Property_DataType_Helpers(string key)
    {
        var (f1, f2) = PropertyCases[key];
        var (p1, p2) = (f1(key), f2(key));

        Assert.Equivalent(p1, p2);
    }

    /// <summary>
    /// Tests that equals should return true for identical properties
    /// </summary>
    [Fact]
    public void Equals_Should_Return_True_For_Identical_Properties()
    {
        // Arrange
        var property1 = new Property("name", DataType.Text)
        {
            Description = "description",
            IndexFilterable = true,
            IndexRangeFilters = false,
            IndexSearchable = true,
            PropertyTokenization = PropertyTokenization.Whitespace,
        };

        var property2 = new Property("name", DataType.Text)
        {
            Description = "description",
            IndexFilterable = true,
            IndexRangeFilters = false,
            IndexSearchable = true,
            PropertyTokenization = PropertyTokenization.Whitespace,
        };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that equals should return false when names differ
    /// </summary>
    [Fact]
    public void Equals_Should_Return_False_When_Names_Differ()
    {
        // Arrange
        var property1 = new Property("name1", DataType.Text);
        var property2 = new Property("name2", DataType.Text);

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that equals should return false when data types differ
    /// </summary>
    [Fact]
    public void Equals_Should_Return_False_When_DataTypes_Differ()
    {
        // Arrange
        var property1 = new Property("prop", DataType.Text);
        var property2 = new Property("prop", DataType.Int);

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that equals should return false when descriptions differ
    /// </summary>
    [Fact]
    public void Equals_Should_Return_False_When_Descriptions_Differ()
    {
        // Arrange
        var property1 = new Property("prop", DataType.Text) { Description = "desc1" };
        var property2 = new Property("prop", DataType.Text) { Description = "desc2" };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that equals should return false when index filterable differs
    /// </summary>
    [Fact]
    public void Equals_Should_Return_False_When_IndexFilterable_Differs()
    {
        // Arrange
        var property1 = new Property("prop", DataType.Text) { IndexFilterable = true };
        var property2 = new Property("prop", DataType.Text) { IndexFilterable = false };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that equals should return false when index range filters differs
    /// </summary>
    [Fact]
    public void Equals_Should_Return_False_When_IndexRangeFilters_Differs()
    {
        // Arrange
        var property1 = new Property("prop", DataType.Text) { IndexRangeFilters = true };
        var property2 = new Property("prop", DataType.Text) { IndexRangeFilters = false };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that equals should return false when index searchable differs
    /// </summary>
    [Fact]
    public void Equals_Should_Return_False_When_IndexSearchable_Differs()
    {
        // Arrange
        var property1 = new Property("prop", DataType.Text) { IndexSearchable = true };
        var property2 = new Property("prop", DataType.Text) { IndexSearchable = false };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that equals should return false when property tokenization differs
    /// </summary>
    [Fact]
    public void Equals_Should_Return_False_When_PropertyTokenization_Differs()
    {
        // Arrange
        var property1 = new Property("prop", DataType.Text)
        {
            PropertyTokenization = PropertyTokenization.Word,
        };
        var property2 = new Property("prop", DataType.Text)
        {
            PropertyTokenization = PropertyTokenization.Lowercase,
        };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// The test class
    /// </summary>
    private class TestClass
    {
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the age
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the value of the is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the value of the created at
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// The test class with array property class
    /// </summary>
    private class TestClassWithArrayProperty
    {
        /// <summary>
        /// Gets or sets the value of the items
        /// </summary>
        public TestClass[] Items { get; set; } = Array.Empty<TestClass>();
    }

    /// <summary>
    /// The nested class
    /// </summary>
    private class NestedClass
    {
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the details
        /// </summary>
        public NestedClass? Details { get; set; } = null;

        /// <summary>
        /// Gets or sets the value of the details array
        /// </summary>
        public NestedClass[]? DetailsArray { get; set; } = null;
    }

    /// <summary>
    /// Tests that from class should generate properties for simple
    /// </summary>
    [Fact]
    public void FromClass_ShouldGeneratePropertiesForSimpleClass()
    {
        // Act
        var properties = Property.FromClass<TestClass>();

        // Assert
        Assert.Equal(4, properties.Length);

        Assert.Contains(properties, p => p.Name == "name" && p.DataType == DataType.Text);
        Assert.Contains(properties, p => p.Name == "age" && p.DataType == DataType.Int);
        Assert.Contains(properties, p => p.Name == "isActive" && p.DataType == DataType.Bool);
        Assert.Contains(properties, p => p.Name == "createdAt" && p.DataType == DataType.Date);
    }

    /// <summary>
    /// Tests that from class should generate properties for object array
    /// </summary>
    [Fact]
    public void FromClass_ShouldGeneratePropertiesForObjectArray()
    {
        // Act
        var properties = Property.FromClass<TestClass[]>();

        // Assert
        Assert.Equal(4, properties.Length);
    }

    /// <summary>
    /// Tests that from class should generate properties for object array property
    /// </summary>
    [Fact]
    public void FromClass_ShouldGeneratePropertiesForObjectArrayProperty()
    {
        // Act
        var properties = Property.FromClass<TestClassWithArrayProperty>();

        // Assert
        Assert.Single(properties);
        Assert.NotNull(properties[0].NestedProperties);
        Assert.Equal(4, properties[0].NestedProperties!.Length);
    }

    /// <summary>
    /// Tests that from class should generate properties for nested
    /// </summary>
    [Fact]
    public void FromClass_ShouldGeneratePropertiesForNestedClass()
    {
        // Act
        var properties = Property.FromClass<NestedClass>();

        // Assert
        Assert.Equal(3, properties.Length);

        Assert.Contains(properties, p => p.Name == "title" && p.DataType == DataType.Text);
        Assert.Contains(properties, p => p.Name == "details" && p.DataType == DataType.Object);
        Assert.Contains(
            properties,
            p => p.Name == "detailsArray" && p.DataType == DataType.ObjectArray
        );
        var detailsProperty = properties.FirstOrDefault(p => p.Name == "details");
        Assert.NotNull(detailsProperty);
        Assert.Equal(DataType.Object, detailsProperty!.DataType);
        Assert.NotNull(detailsProperty!.NestedProperties);
        Assert.NotEmpty(detailsProperty.NestedProperties);
        Assert.Null(detailsProperty.NestedProperties[0].NestedProperties);

        var detailsArrayProperty = properties.FirstOrDefault(p => p.Name == "detailsArray");
        Assert.NotNull(detailsArrayProperty);
        Assert.Equal(DataType.ObjectArray, detailsArrayProperty!.DataType);
        Assert.NotNull(detailsArrayProperty!.NestedProperties);
        Assert.NotEmpty(detailsArrayProperty.NestedProperties);
        Assert.Null(detailsArrayProperty.NestedProperties[0].NestedProperties);
    }

    /// <summary>
    /// The empty class
    /// </summary>
    class EmptyClass { }

    /// <summary>
    /// Tests that from class should handle empty
    /// </summary>
    [Fact]
    public void FromClass_ShouldHandleEmptyClass()
    {
        // Arrange

        // Act
        var properties = Property.FromClass<EmptyClass>();

        // Assert
        Assert.Empty(properties);
    }

    /// <summary>
    /// The unsupported class
    /// </summary>
    class UnsupportedClass
    {
        /// <summary>
        /// Gets or sets the value of the unsupported property
        /// </summary>
        public object UnsupportedProperty { get; set; } = new object();
    }
}

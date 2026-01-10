using Weaviate.Client.Models;
using Weaviate.Client.Validation;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for TypeValidator to ensure proper type validation against collection schemas.
/// </summary>
public class TypeValidatorTests
{
    /// <summary>
    /// Tests that validate type with matching types returns success
    /// </summary>
    [Fact]
    public void ValidateType_WithMatchingTypes_ReturnsSuccess()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property { Name = "title", DataType = DataType.Text },
                new Property { Name = "publishedDate", DataType = DataType.Date },
                new Property { Name = "viewCount", DataType = DataType.Int },
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<ArticleModel>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validate type with type mismatch returns error
    /// </summary>
    [Fact]
    public void ValidateType_WithTypeMismatch_ReturnsError()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property { Name = "viewCount", DataType = DataType.Text }, // Wrong: should be int
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<ArticleModel>(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(ValidationErrorType.TypeMismatch, result.Errors[0].ErrorType);
        Assert.Equal("ViewCount", result.Errors[0].PropertyName);
        Assert.Equal("text", result.Errors[0].ExpectedType);
        Assert.Equal("int", result.Errors[0].ActualType);
    }

    /// <summary>
    /// Tests that validate type with array mismatch returns array mismatch error
    /// </summary>
    [Fact]
    public void ValidateType_WithArrayMismatch_ReturnsArrayMismatchError()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property
                {
                    Name = "tags",
                    DataType = DataType.Text, // Should be text[]
                },
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<ArticleWithTags>(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(ValidationErrorType.ArrayMismatch, result.Errors[0].ErrorType);
    }

    /// <summary>
    /// Tests that validate type with extra property in c sharp returns warning
    /// </summary>
    [Fact]
    public void ValidateType_WithExtraPropertyInCSharp_ReturnsWarning()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property { Name = "title", DataType = DataType.Text },
                // viewCount is in C# type but not in schema
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<ArticleModel>(schema);

        // Assert
        Assert.True(result.IsValid); // Warnings don't fail validation
        Assert.Empty(result.Errors);
        Assert.NotEmpty(result.Warnings);
        Assert.Contains(
            result.Warnings,
            w =>
                w.PropertyName == "ViewCount"
                && w.WarningType == ValidationWarningType.ExtraProperty
        );
    }

    /// <summary>
    /// Tests that validate type with missing property in c sharp returns warning
    /// </summary>
    [Fact]
    public void ValidateType_WithMissingPropertyInCSharp_ReturnsWarning()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property { Name = "title", DataType = DataType.Text },
                new Property { Name = "content", DataType = DataType.Text }, // Not in C# type
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<ArticleModel>(schema);

        // Assert
        Assert.True(result.IsValid); // Warnings don't fail validation
        Assert.Empty(result.Errors);
        Assert.Contains(result.Warnings, w => w.PropertyName == "content");
    }

    /// <summary>
    /// Tests that validate type with all supported types validates correctly
    /// </summary>
    [Fact]
    public void ValidateType_WithAllSupportedTypes_ValidatesCorrectly()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "AllTypes",
            Properties =
            [
                new Property { Name = "textProp", DataType = DataType.Text },
                new Property { Name = "intProp", DataType = DataType.Int },
                new Property { Name = "numberProp", DataType = DataType.Number },
                new Property { Name = "boolProp", DataType = DataType.Bool },
                new Property { Name = "dateProp", DataType = DataType.Date },
                new Property { Name = "uuidProp", DataType = DataType.Uuid },
                new Property { Name = "blobProp", DataType = DataType.Blob },
                new Property { Name = "geoProp", DataType = DataType.GeoCoordinate },
                new Property { Name = "phoneProp", DataType = DataType.PhoneNumber },
                new Property { Name = "textArrayProp", DataType = DataType.TextArray },
                new Property { Name = "intArrayProp", DataType = DataType.IntArray },
                new Property { Name = "objectProp", DataType = DataType.Object },
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<AllTypesModel>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validate type with nested object validates nested properties
    /// </summary>
    [Fact]
    public void ValidateType_WithNestedObject_ValidatesNestedProperties()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "ArticleWithAuthor",
            Properties =
            [
                new Property { Name = "title", DataType = DataType.Text },
                new Property
                {
                    Name = "author",
                    DataType = DataType.Object,
                    NestedProperties =
                    [
                        new Property { Name = "name", DataType = DataType.Text },
                        new Property { Name = "age", DataType = DataType.Int },
                    ],
                },
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<ArticleWithAuthor>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validate type with nested object type mismatch returns error
    /// </summary>
    [Fact]
    public void ValidateType_WithNestedObjectTypeMismatch_ReturnsError()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "ArticleWithAuthor",
            Properties =
            [
                new Property { Name = "title", DataType = DataType.Text },
                new Property
                {
                    Name = "author",
                    DataType = DataType.Object,
                    NestedProperties =
                    [
                        new Property { Name = "name", DataType = DataType.Text },
                        new Property { Name = "age", DataType = DataType.Text }, // Wrong: should be int
                    ],
                },
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<ArticleWithAuthor>(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Age", result.Errors[0].PropertyName);
        Assert.Equal(ValidationErrorType.TypeMismatch, result.Errors[0].ErrorType);
    }

    /// <summary>
    /// Tests that validation result throw if invalid throws when invalid
    /// </summary>
    [Fact]
    public void ValidationResult_ThrowIfInvalid_ThrowsWhenInvalid()
    {
        // Arrange
        var result = ValidationResult.Failed(
            new ValidationError
            {
                PropertyName = "Test",
                Message = "Test error",
                ErrorType = ValidationErrorType.TypeMismatch,
            }
        );

        // Act & Assert
        var exception = Assert.Throws<TypeValidationException>(() => result.ThrowIfInvalid());
        Assert.NotNull(exception.ValidationResult);
        Assert.False(exception.ValidationResult.IsValid);
    }

    /// <summary>
    /// Tests that validation result throw if invalid does not throw when valid
    /// </summary>
    [Fact]
    public void ValidationResult_ThrowIfInvalid_DoesNotThrowWhenValid()
    {
        // Arrange
        var result = ValidationResult.Success();

        // Act & Assert
        result.ThrowIfInvalid(); // Should not throw
    }

    /// <summary>
    /// Tests that validation result get detailed message formats errors and warnings
    /// </summary>
    [Fact]
    public void ValidationResult_GetDetailedMessage_FormatsErrorsAndWarnings()
    {
        // Arrange
        var result = ValidationResult.WithIssues(
            [
                new ValidationError
                {
                    PropertyName = "TestProp",
                    Message = "Type mismatch",
                    ErrorType = ValidationErrorType.TypeMismatch,
                    ExpectedType = "text",
                    ActualType = "int",
                },
            ],
            [
                new ValidationWarning
                {
                    PropertyName = "ExtraProp",
                    Message = "Extra property",
                    WarningType = ValidationWarningType.ExtraProperty,
                },
            ]
        );

        // Act
        var message = result.GetDetailedMessage();

        // Assert
        Assert.Contains("TestProp", message);
        Assert.Contains("Type mismatch", message);
        Assert.Contains("Expected: text", message);
        Assert.Contains("Got: int", message);
        Assert.Contains("ExtraProp", message);
        Assert.Contains("Extra property", message);
    }

    // Test models
    /// <summary>
    /// The article model class
    /// </summary>
    public class ArticleModel
    {
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the value of the published date
        /// </summary>
        public DateTime PublishedDate { get; set; }

        /// <summary>
        /// Gets or sets the value of the view count
        /// </summary>
        public int ViewCount { get; set; }
    }

    /// <summary>
    /// The article with tags class
    /// </summary>
    public class ArticleWithTags
    {
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the value of the tags
        /// </summary>
        public string[]? Tags { get; set; }
    }

    /// <summary>
    /// The all types model class
    /// </summary>
    public class AllTypesModel
    {
        /// <summary>
        /// Gets or sets the value of the text prop
        /// </summary>
        public string? TextProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the int prop
        /// </summary>
        public int IntProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the number prop
        /// </summary>
        public double NumberProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the bool prop
        /// </summary>
        public bool BoolProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the date prop
        /// </summary>
        public DateTime DateProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the uuid prop
        /// </summary>
        public Guid UuidProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the blob prop
        /// </summary>
        public byte[]? BlobProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the geo prop
        /// </summary>
        public GeoCoordinate? GeoProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the phone prop
        /// </summary>
        public PhoneNumber? PhoneProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the text array prop
        /// </summary>
        public string[]? TextArrayProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the int array prop
        /// </summary>
        public int[]? IntArrayProp { get; set; }

        /// <summary>
        /// Gets or sets the value of the object prop
        /// </summary>
        public NestedModel? ObjectProp { get; set; }
    }

    /// <summary>
    /// The article with author class
    /// </summary>
    public class ArticleWithAuthor
    {
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the value of the author
        /// </summary>
        public AuthorModel? Author { get; set; }
    }

    /// <summary>
    /// The author model class
    /// </summary>
    public class AuthorModel
    {
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the age
        /// </summary>
        public int Age { get; set; }
    }

    /// <summary>
    /// The nested model class
    /// </summary>
    public class NestedModel
    {
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string? Name { get; set; }
    }

    // Models for compatibility tests
    /// <summary>
    /// The product float class
    /// </summary>
    private class ProductFloat
    {
        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public float Price { get; set; }
    }

    /// <summary>
    /// The product decimal class
    /// </summary>
    private class ProductDecimal
    {
        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public decimal Price { get; set; }
    }

    /// <summary>
    /// The order long class
    /// </summary>
    private class OrderLong
    {
        /// <summary>
        /// Gets or sets the value of the quantity
        /// </summary>
        public long Quantity { get; set; }
    }

    /// <summary>
    /// The task status enum enum
    /// </summary>
    private enum TaskStatusEnum
    {
        /// <summary>
        /// The pending task status enum
        /// </summary>
        Pending,

        /// <summary>
        /// The in progress task status enum
        /// </summary>
        InProgress,

        /// <summary>
        /// The completed task status enum
        /// </summary>
        Completed,
    }

    /// <summary>
    /// The user task class
    /// </summary>
    private class UserTask
    {
        /// <summary>
        /// Gets or sets the value of the status
        /// </summary>
        public TaskStatusEnum Status { get; set; }
    }

    /// <summary>
    /// Tests that validate type number compatibility is valid
    /// </summary>
    /// <param name="type">The type</param>
    [Theory]
    [InlineData(typeof(ProductFloat))]
    [InlineData(typeof(ProductDecimal))]
    public void ValidateType_NumberCompatibility_IsValid(Type type)
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Product",
            Properties = [new Property { Name = "price", DataType = DataType.Number }],
        };
        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType(type, schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validate type int compatibility is valid
    /// </summary>
    [Fact]
    public void ValidateType_IntCompatibility_IsValid()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Order",
            Properties = [new Property("quantity", DataType.Int)],
        };
        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<OrderLong>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validate type enum compatibility is valid
    /// </summary>
    [Fact]
    public void ValidateType_EnumCompatibility_IsValid()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "UserTask",
            Properties = new[]
            {
                new Property { Name = "status", DataType = DataType.Text },
            },
        };
        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<UserTask>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validate type enum compatibility with integer is valid
    /// </summary>
    [Fact]
    public void ValidateType_EnumCompatibility_WithInteger_IsValid()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "UserTask",
            Properties = new[]
            {
                new Property { Name = "status", DataType = DataType.Int },
            },
        };
        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<UserTask>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

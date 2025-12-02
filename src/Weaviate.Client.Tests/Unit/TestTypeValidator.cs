using Weaviate.Client.Models;
using Weaviate.Client.Validation;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for TypeValidator to ensure proper type validation against collection schemas.
/// </summary>
public class TypeValidatorTests
{
    [Fact]
    public void ValidateType_WithMatchingTypes_ReturnsSuccess()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property { Name = "title", DataType = [DataType.Text] },
                new Property { Name = "publishedDate", DataType = [DataType.Date] },
                new Property { Name = "viewCount", DataType = [DataType.Int] },
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<ArticleModel>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateType_WithTypeMismatch_ReturnsError()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property { Name = "viewCount", DataType = [DataType.Text] }, // Wrong: should be int
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
        Assert.Equal(DataType.Text, result.Errors[0].ExpectedType);
        Assert.Equal(DataType.Int, result.Errors[0].ActualType);
    }

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
                    DataType = [DataType.Text], // Should be text[]
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

    [Fact]
    public void ValidateType_WithExtraPropertyInCSharp_ReturnsWarning()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property { Name = "title", DataType = [DataType.Text] },
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

    [Fact]
    public void ValidateType_WithMissingPropertyInCSharp_ReturnsWarning()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Article",
            Properties =
            [
                new Property { Name = "title", DataType = [DataType.Text] },
                new Property { Name = "content", DataType = [DataType.Text] }, // Not in C# type
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

    [Fact]
    public void ValidateType_WithAllSupportedTypes_ValidatesCorrectly()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "AllTypes",
            Properties =
            [
                new Property { Name = "textProp", DataType = [DataType.Text] },
                new Property { Name = "intProp", DataType = [DataType.Int] },
                new Property { Name = "numberProp", DataType = [DataType.Number] },
                new Property { Name = "boolProp", DataType = [DataType.Bool] },
                new Property { Name = "dateProp", DataType = [DataType.Date] },
                new Property { Name = "uuidProp", DataType = [DataType.Uuid] },
                new Property { Name = "blobProp", DataType = [DataType.Blob] },
                new Property { Name = "geoProp", DataType = [DataType.GeoCoordinate] },
                new Property { Name = "phoneProp", DataType = [DataType.PhoneNumber] },
                new Property { Name = "textArrayProp", DataType = [DataType.TextArray] },
                new Property { Name = "intArrayProp", DataType = [DataType.IntArray] },
                new Property { Name = "objectProp", DataType = [DataType.Object] },
            ],
        };

        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<AllTypesModel>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateType_WithNestedObject_ValidatesNestedProperties()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "ArticleWithAuthor",
            Properties =
            [
                new Property { Name = "title", DataType = [DataType.Text] },
                new Property
                {
                    Name = "author",
                    DataType = [DataType.Object],
                    NestedProperties =
                    [
                        new Property { Name = "name", DataType = [DataType.Text] },
                        new Property { Name = "age", DataType = [DataType.Int] },
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

    [Fact]
    public void ValidateType_WithNestedObjectTypeMismatch_ReturnsError()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "ArticleWithAuthor",
            Properties =
            [
                new Property { Name = "title", DataType = [DataType.Text] },
                new Property
                {
                    Name = "author",
                    DataType = [DataType.Object],
                    NestedProperties =
                    [
                        new Property { Name = "name", DataType = [DataType.Text] },
                        new Property { Name = "age", DataType = [DataType.Text] }, // Wrong: should be int
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

    [Fact]
    public void ValidationResult_ThrowIfInvalid_DoesNotThrowWhenValid()
    {
        // Arrange
        var result = ValidationResult.Success();

        // Act & Assert
        result.ThrowIfInvalid(); // Should not throw
    }

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
    public class ArticleModel
    {
        public string? Title { get; set; }
        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
    }

    public class ArticleWithTags
    {
        public string? Title { get; set; }
        public string[]? Tags { get; set; }
    }

    public class AllTypesModel
    {
        public string? TextProp { get; set; }
        public int IntProp { get; set; }
        public double NumberProp { get; set; }
        public bool BoolProp { get; set; }
        public DateTime DateProp { get; set; }
        public Guid UuidProp { get; set; }
        public byte[]? BlobProp { get; set; }
        public GeoCoordinate? GeoProp { get; set; }
        public PhoneNumber? PhoneProp { get; set; }
        public string[]? TextArrayProp { get; set; }
        public int[]? IntArrayProp { get; set; }
        public NestedModel? ObjectProp { get; set; }
    }

    public class ArticleWithAuthor
    {
        public string? Title { get; set; }
        public AuthorModel? Author { get; set; }
    }

    public class AuthorModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class NestedModel
    {
        public string? Name { get; set; }
    }

    // Models for compatibility tests
    private class ProductFloat
    {
        public float Price { get; set; }
    }

    private class ProductDecimal
    {
        public decimal Price { get; set; }
    }

    private class OrderLong
    {
        public long Quantity { get; set; }
    }

    private enum TaskStatusEnum
    {
        Pending,
        InProgress,
        Completed,
    }

    private class UserTask
    {
        public TaskStatusEnum Status { get; set; }
    }

    [Theory]
    [InlineData(typeof(ProductFloat))]
    [InlineData(typeof(ProductDecimal))]
    public void ValidateType_NumberCompatibility_IsValid(Type type)
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Product",
            Properties = [new Property { Name = "price", DataType = [DataType.Number] }],
        };
        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType(type, schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateType_IntCompatibility_IsValid()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "Order",
            Properties = [new Property { Name = "quantity", DataType = [DataType.Int] }],
        };
        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<OrderLong>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateType_EnumCompatibility_IsValid()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "UserTask",
            Properties = new[]
            {
                new Property
                {
                    Name = "status",
                    DataType = new List<string> { DataType.Text },
                },
            },
        };
        var validator = TypeValidator.Default;

        // Act
        var result = validator.ValidateType<UserTask>(schema);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateType_EnumCompatibility_WithInteger_IsValid()
    {
        // Arrange
        var schema = new CollectionConfig
        {
            Name = "UserTask",
            Properties = new[]
            {
                new Property
                {
                    Name = "status",
                    DataType = new List<string> { DataType.Int },
                },
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

using Weaviate.Client.Models;

namespace Weaviate.Client.Tests;

[Collection("Unit Tests")]
public partial class UnitTests
{
    [Fact]
    public void NamedVectorInitialization()
    {
        // Arrange
        var v1 = new NamedVectors { { "default", 0.1f, 0.2f, 0.3f } };
    }

    [Fact]
    public void MetadataQueryImplicitConversion()
    {
        // Arrange
        var vectors = new string[] { "default" };
        var options = MetadataOptions.Vector;

        // Act
        MetadataQuery q1 = vectors;
        MetadataQuery q2 = options;
        MetadataQuery q3 = (options, vectors);

        // Assert
        Assert.Equal(q1.Vectors, vectors);
        Assert.Equal(q2.Options, options);
        Assert.Equal(q3.Options, options);
        Assert.Equal(q3.Vectors, vectors);
    }

    [Fact]
    public void TestBuildDynamicObject()
    {
        // Arrange
        var review = new[]
        {
            new
            {
                author_username = "kineticandroid",
                content = @"Take the story of Frankenstein's monster.",
                rating = (double?)null,
                movie_id = 162,
                review_id = 162,
            },
            new
            {
                author_username = "r96sk",
                content = @"Very enjoyable.",
                rating = (double?)8.0,
                movie_id = 162,
                review_id = 162,
            },
        };

        // Act
        var obj = review.Select(r => DataClient<dynamic>.BuildDynamicObject(r)).ToList();

        // Assert
        Assert.Equal("kineticandroid", obj[0]["author_username"]);
        Assert.Equal(review[0].content, obj[0]["content"]);
        Assert.False(obj[0].ContainsKey("rating"));
        Assert.Equal(162, obj[0]["movie_id"]);
        Assert.Equal(162, obj[0]["review_id"]);

        Assert.Equal("r96sk", obj[1]["author_username"]);
        Assert.Equal(review[1].content, obj[1]["content"]);
        Assert.Equal(8.0, obj[1]["rating"]);
        Assert.Equal(162, obj[1]["movie_id"]);
        Assert.Equal(162, obj[1]["review_id"]);
    }

    [Theory]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(char), true)]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(ulong), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(double?), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(Object), false)]
    [InlineData(typeof(WeaviateObject), false)]
    public void TestIsNativeTypeCheck(Type type, bool expected)
    {
        // Arrange

        // Act
        var result = type.IsNativeType();

        // Assert
        Assert.Equal(expected, result);
    }
}

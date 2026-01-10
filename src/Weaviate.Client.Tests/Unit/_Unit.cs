using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The misc tests class
/// </summary>
[Collection("Unit Tests")]
public partial class MiscTests
{
    /// <summary>
    /// Tests that metadata query and vector query implicit conversion
    /// </summary>
    [Fact]
    public void MetadataQuery_And_VectorQuery_ImplicitConversion()
    {
        // Arrange
        var vectors = new string[] { "default" };
        var options = MetadataOptions.All;

        // Act
        VectorQuery v1 = vectors;
        VectorQuery v2 = true;
        VectorQuery v3 = false;
        MetadataQuery q1 = options;

        // Assert
        Assert.Equal(vectors, v1.Vectors);
        Assert.NotNull(v2.Vectors);
        Assert.Empty(v2.Vectors!);
        Assert.Null(v3.Vectors);
        Assert.Equal(options, q1.Options);
    }
}

using System.Dynamic;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Common;

namespace Weaviate.Client.Tests.Unit;

[Collection("Unit Tests")]
public partial class MiscTests
{
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
}

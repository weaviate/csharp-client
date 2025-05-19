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
                content = @"Take the story of Frankenstein's monster, remove the hateful creator, and replace the little girl's flowers with a brightly pastel Reagan-era suburb. Though not my personal favorite Tim Burton film, I feel like this one best encapsulates his style and story interests.",
                rating = (double?)null,
                movie_id = 162,
                review_id = 162,
            },
            new
            {
                author_username = "r96sk",
                content = @"Very enjoyable.

It's funny the way we picture things in our minds. I had heard of <em>'Edward Scissorhands'</em> but actually knew very little about it, typified by the fact I was expecting this to be very dark - probably just based on the seeing the cover here and there. It's much sillier than expected, but in a positive way.

I do kinda end up wishing they went down a more dark/creative route, instead of relying on the novelty of having scissors as hands; though, to be fair, they do touch on the deeper side a bit. With that said, I did get a good amount of entertainment seeing this plot unfold. It's weird and wonderful.

Johnny Depp is a great actor and is very good here, mainly via his facial expressions and body language. It's cool to see Winona Ryder involved, someone I've thoroughly enjoyed in more recent times in <em>'Stranger Things'</em>. Alan Arkin and Anthony Michael Hall also appear.

The film looks neat, as I've come to expect from Tim Burton. It has the obvious touch of Bo Welch to it, with the neighbourhood looking not too dissimilar to what Welch would create for 2003's <em>'The Cat in the Hat'</em> - which I, truly, enjoyed.

Undoubtedly worth a watch.",
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
    [InlineData(typeof(bool))]
    [InlineData(typeof(char))]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(float))]
    [InlineData(typeof(double))]
    [InlineData(typeof(double?))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(string))]
    [InlineData(typeof(DateTime))]
    public void TestIsNativeTypeCheck(Type type)
    {
        // Arrange

        // Act
        var result = type.IsNativeType();

        // Assert
        Assert.True(result);
    }
}

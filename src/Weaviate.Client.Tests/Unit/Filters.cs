using System.Reflection;
using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Tests;

public partial class UnitTests
{
    [Theory]
    [InlineData(
        typeof(Filter.TypedGuid),
        new[] { "Equal", "NotEqual", "ContainsAny", "ContainsAll" },
        new[] { "GreaterThan", "GreaterThanEqual", "LessThan", "LessThanEqual" }
    )]
    public async Task TypeSupportedOperations(
        Type t,
        string[] expectedMethodList,
        string[] unexpectedMethodList
    )
    {
        // Arrange
        var methods = new HashSet<string>(expectedMethodList);

        // Act
        var actualMethods = t.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
            )
            .Select(m => m.Name)
            .Distinct()
            .ToHashSet();

        // Assert
        Assert.Subset(actualMethods, methods);
        Assert.Empty(actualMethods.Intersect(unexpectedMethodList));

        await Task.Yield();
    }

    [Fact]
    public void FilterByReferenceDoesNotChangePreviousFilter()
    {
        // Arrange
        var f1 = Filter.Reference("ref");
        var f2 = f1.Reference("ref2").Property("prop").Equal("value");

        // Act
        Filters filter = f2;

        // Assert
        Assert.Equal("ref", filter.Target.SingleTarget.On);
        Assert.Equal("ref2", filter.Target.SingleTarget.Target.SingleTarget.On);

        // CAUTION. f1 and f2 are different objects, but they have the same reference to filter.
        Assert.Equal((Filters)f2, (Filters)f1);
        Assert.NotNull(((Filters)f2).Target.SingleTarget.Target);
        Assert.NotNull(((Filters)f1).Target.SingleTarget.Target);
    }

    [Fact]
    public void FilterByReferenceCreatesProperGrpcMessage_1()
    {
        // Arrange
        var f = Filter.Reference("ref").Property("name").Equal("John");

        var expected = new Filters()
        {
            Target = new FilterTarget()
            {
                SingleTarget = new FilterReferenceSingleTarget()
                {
                    On = "ref",
                    Target = new FilterTarget() { Property = "name" },
                },
            },
            Operator = Filters.Types.Operator.Equal,
            ValueText = "John",
        };

        Assert.Equal(expected, f);
    }

    [Fact]
    public void FilterByReferenceCreatesProperGrpcMessage_2()
    {
        // Arrange
        var f = Filter.Reference("ref").Property("size").GreaterThan(3);

        var expected = new Filters()
        {
            Target = new FilterTarget()
            {
                SingleTarget = new FilterReferenceSingleTarget()
                {
                    On = "ref",
                    Target = new FilterTarget() { Property = "size" },
                },
            },
            Operator = Filters.Types.Operator.GreaterThan,
            ValueInt = 3,
        };

        Assert.Equal(expected, f);
    }

    [Fact]
    public void FilterByReferenceCreatesProperGrpcMessage_3()
    {
        // Arrange
        var f = Filter.Reference("ref").Count.Equal(1);

        var expected = new Filters()
        {
            Target = new FilterTarget() { Count = new FilterReferenceCount { On = "ref" } },
            Operator = Filters.Types.Operator.Equal,
            ValueInt = 1,
        };

        // Act
        // Assert
        Assert.Equal(expected, f);
    }
}

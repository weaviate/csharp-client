using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Tests;

public partial class UnitTests
{
    [Fact]
    public void FilterByReferenceStacksUp()
    {
        // Arrange
        var f1 = Filter.Reference("ref");
        var f2 = f1.Reference("ref2");

        // Act
        Filters filter = f2;

        // Assert
        Assert.Equal("ref", filter.Target.SingleTarget.On);
        Assert.Equal("ref2", filter.Target.SingleTarget.Target.SingleTarget.On);

        // CAUTION. f1 and f2 are different objects, but they have the same reference to filter.
        // TODO: Look for a way to avoid this.
        Assert.NotNull(((Filters)f2).Target.SingleTarget.Target);
        Assert.NotNull(((Filters)f1).Target.SingleTarget.Target);
    }

    [Fact]
    public void FilterByReferenceCreatesProperGrpcMessage()
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
    public void FilterByReferenceCreatesProperGrpcMessage2()
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
}

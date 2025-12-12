using System.Reflection;
using Weaviate.Client.Grpc.Protobuf.V1;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public partial class FilterTests
{
    [Theory]
    [InlineData(
        typeof(TypedGuid),
        new[] { "Equal", "NotEqual", "ContainsAny" },
        new[] { "GreaterThan", "GreaterThanEqual", "LessThan", "LessThanEqual" }
    )]
    public void FilterTypeSupportedOperations(
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
    }

    [Fact]
    public void FilterByReferencChainingChangePreviousFilterBecauseRefs()
    {
        // Arrange
        var f1 = Filter.Reference("ref");
        var f2 = f1.Reference("ref2").Property("prop").IsEqual("value");

        // Act
        Filters filter = f2.InternalFilter;

        // Assert
        Assert.Equal("ref", filter.Target.SingleTarget.On);
        Assert.Equal("ref2", filter.Target.SingleTarget.Target.SingleTarget.On);

        // CAUTION. f1 and f2 are different objects, but they have the same reference to filter.
        Assert.Equal(f2.InternalFilter, f1.InternalFilter);
        Assert.NotNull(f2.InternalFilter.Target.SingleTarget.Target);
        Assert.NotNull(f1.InternalFilter.Target.SingleTarget.Target);
    }

    [Fact]
    public void FilterByReferenceCreatesProperGrpcMessage_1()
    {
        // Arrange
        var f = Filter.Reference("ref").Property("name").IsEqual("John");

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

        Assert.Equal(expected, f.InternalFilter);
    }

    [Fact]
    public void FilterByReferenceCreatesProperGrpcMessage_2()
    {
        // Arrange
        var f = Filter.Reference("ref").Property("size").IsGreaterThan(3);

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

        Assert.Equal(expected, f.InternalFilter);
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
        Assert.Equal(expected, f.InternalFilter);
    }

    [Fact]
    public void FilterRequestCreatesProperGrpcMessage_4()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        var f = Filter.ID.ContainsAny([id]);

        var expected = new Filters()
        {
            Target = new FilterTarget() { Property = "_id" },
            Operator = Filters.Types.Operator.ContainsAny,
            ValueTextArray = new TextArray() { Values = { id.ToString() } },
        };

        // Act
        // Assert
        Assert.Equal(expected, f.InternalFilter);
    }

    [Fact]
    public void Filter_Property_Length()
    {
        // Arrange
        var f = Filter.Property("name").Length().Equal(5);

        var expected = new Filters()
        {
            Target = new FilterTarget() { Property = "len(name)" },
            Operator = Filters.Types.Operator.Equal,
            ValueInt = 5,
        };

        // Act
        // Assert
        Assert.Equal(expected, f.InternalFilter);
    }

    [Fact]
    public void Filter_UUIDs_ContainsAny()
    {
        var uuid2 = Guid.NewGuid();
        var f = Filter.Property("uuids").ContainsAny(new[] { uuid2 });

        var expected = new Filters()
        {
            Target = new FilterTarget() { Property = "uuids" },
            Operator = Filters.Types.Operator.ContainsAny,
            ValueTextArray = new TextArray() { Values = { uuid2.ToString() } },
        };

        // Act
        // Assert
        Assert.Equal(expected, f.InternalFilter);
    }

    [Fact]
    public void Filter_AllOf_And_AnyOf()
    {
        var uuid2 = Guid.NewGuid();
        var f1 = Filter.Property("uuids").ContainsAny(new[] { uuid2 });
        var f2 = Filter.Property("name").Length().Equal(5);

        var expectedAllOf = f1 & f2;
        var expectedAnyOf = f1 | f2;

        // Act
        var fAllOf = Filter.AllOf(f1, f2);
        var fAnyOf = Filter.AnyOf(f1, f2);

        // Assert
        Assert.Equal(expectedAllOf.InternalFilter, fAllOf.InternalFilter);
        Assert.Equal(expectedAnyOf.InternalFilter, fAnyOf.InternalFilter);
    }

    [Fact]
    public void Filter_ContainsNone()
    {
        // Arrange
        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();
        var f1 = Filter.Property("uuids").ContainsNone(new[] { uuid1, uuid2 });

        var expectedF1 = new Filters
        {
            Target = new FilterTarget() { Property = "uuids" },
            Operator = Filters.Types.Operator.ContainsNone,
            ValueTextArray = new TextArray() { Values = { uuid1.ToString(), uuid2.ToString() } },
        };

        // Act

        // Assert
        Assert.Equal(expectedF1, f1.InternalFilter);
    }

    [Fact]
    public void Filter_Not()
    {
        // Arrange
        var uuid1 = Guid.NewGuid();
        var uuid2 = Guid.NewGuid();
        var f1 = Filter.Not(Filter.ID.ContainsAny(new[] { uuid1, uuid2 }));

        var expectedF1 = new Filters
        {
            Filters_ =
            {
                new Filters
                {
                    Target = new FilterTarget() { Property = "_id" },
                    Operator = Filters.Types.Operator.ContainsAny,
                    ValueTextArray = new TextArray()
                    {
                        Values = { uuid1.ToString(), uuid2.ToString() },
                    },
                },
            },
            Operator = Filters.Types.Operator.Not,
        };

        // Act

        // Assert
        Assert.Equal(expectedF1, f1.InternalFilter);
    }
}

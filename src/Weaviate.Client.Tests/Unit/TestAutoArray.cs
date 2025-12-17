using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class TestAutoArray
{
    [Fact]
    public void ImplicitConversion_FromSingleItem_CreatesSingleValue()
    {
        AutoArray<string> result = "test";

        Assert.Single(result);
        Assert.Equal("test", result.First());
    }

    [Fact]
    public void ImplicitConversion_FromArray_PreservesItems()
    {
        var items = new[] { "one", "two", "three" };

        AutoArray<string> result = items;

        Assert.Equal(items, result);
    }

    [Fact]
    public void ImplicitConversion_FromList_PreservesItems()
    {
        var items = new List<int> { 1, 2, 3 };

        AutoArray<int> result = items;

        Assert.Equal(items, result);
    }

    [Fact]
    public void ImplicitConversion_FromEmptyArray_CreatesEmptyCollection()
    {
        AutoArray<string> result = Array.Empty<string>();

        Assert.Empty(result);
    }

    [Fact]
    public void ImplicitConversion_FromNullArray_ReturnsNull()
    {
        string[]? items = null;

        AutoArray<string>? result = items;

        Assert.Null(result);
    }

    [Fact]
    public void ExplicitConversion_ToArray_ReturnsCopy()
    {
        AutoArray<string> source = new[] { "one", "two", "three" };

        var copy = (string[])source;

        Assert.Equal(new[] { "one", "two", "three" }, copy);
    }

    [Fact]
    public void Add_WithSingleItem_AppendsValue()
    {
        AutoArray<string> values = new[] { "one", "two" };

        values.Add("three");

        Assert.Equal(new[] { "one", "two", "three" }, values);
    }

    [Fact]
    public void Add_WithArray_AppendsValues()
    {
        AutoArray<string> values = "original";
        var additional = new[] { "one", "two", "three" };

        values.Add(additional);

        Assert.Equal(new[] { "original", "one", "two", "three" }, values);
    }

    [Fact]
    public void Add_WithNullArray_DoesNothing()
    {
        var original = new[] { "one", "two" };
        AutoArray<string> values = original;

        string[]? nullArray = null;
        values.Add(nullArray!);

        Assert.Equal(original, values);
    }

    [Fact]
    public void Add_WithEmptyArray_MakesNoChanges()
    {
        AutoArray<string> values = new[] { "one", "two" };

        values.Add(Array.Empty<string>());

        Assert.Equal(new[] { "one", "two" }, values);
    }

    [Fact]
    public void Enumerating_GenericInterface_ReturnsSequence()
    {
        var items = new[] { "one", "two", "three" };
        AutoArray<string> values = items;
        var result = new List<string>();

        foreach (var item in values)
        {
            result.Add(item);
        }

        Assert.Equal(items, result);
    }

    [Fact]
    public void Enumerating_NonGenericInterface_ReturnsSequence()
    {
        var items = new[] { 1, 2, 3 };
        AutoArray<int> values = items;
        var result = new List<int>();

        var enumerator = ((System.Collections.IEnumerable)values).GetEnumerator();
        while (enumerator.MoveNext())
        {
            result.Add((int)enumerator.Current);
        }

        Assert.Equal(items, result);
    }

    [Fact]
    public void SupportsComplexTypes()
    {
        var obj1 = new TestClass { Id = 1, Name = "First" };
        var obj2 = new TestClass { Id = 2, Name = "Second" };

        AutoArray<TestClass> values = new[] { obj1, obj2 };

        Assert.Equal(2, values.Count());
        Assert.Contains(obj1, values);
        Assert.Contains(obj2, values);
    }

    [Fact]
    public void ImplicitConversion_PassesThroughMethodParameters()
    {
        var single = AcceptOneOrMany("single");
        var multiple = AcceptOneOrMany(new[] { "one", "two" });

        Assert.Single(single);
        Assert.Equal(2, multiple.Count());
    }

    private static AutoArray<string> AcceptOneOrMany(AutoArray<string> value)
    {
        return value;
    }

    [Fact]
    public void CollectionExpression_WithSingleItem_CreatesSingleItemCollection()
    {
        AutoArray<int> x = [1];

        Assert.Single(x);
        Assert.Equal(new[] { 1 }, x);
    }

    [Fact]
    public void CollectionExpression_WithMultipleItems_CreatesCollection()
    {
        AutoArray<int> y = [1, 2, 3];

        Assert.Equal(3, y.Count());
        Assert.Equal(new[] { 1, 2, 3 }, y);
    }

    [Fact]
    public void CollectionExpression_WithEmptyExpression_CreatesEmptyCollection()
    {
        AutoArray<string> empty = [];

        Assert.Empty(empty);
    }

    [Fact]
    public void CollectionExpression_WithStrings_PreservesValues()
    {
        AutoArray<string> values = ["one", "two", "three"];

        Assert.Equal(new[] { "one", "two", "three" }, values);
    }

    [Fact]
    public void CollectionExpression_WithComplexTypes_WorksCorrectly()
    {
        var obj1 = new TestClass { Id = 1, Name = "First" };
        var obj2 = new TestClass { Id = 2, Name = "Second" };

        AutoArray<TestClass> values = [obj1, obj2];

        Assert.Equal(2, values.Count());
        Assert.Contains(obj1, values);
        Assert.Contains(obj2, values);
    }

    [Fact]
    public void CollectionExpression_CanBeEnumerated()
    {
        AutoArray<int> values = [10, 20, 30];
        var sum = 0;

        foreach (var value in values)
        {
            sum += value;
        }

        Assert.Equal(60, sum);
    }

    [Fact]
    public void CollectionExpression_CanBeConvertedToArray()
    {
        AutoArray<string> values = ["x", "y", "z"];

        var array = (string[])values;

        Assert.Equal(new[] { "x", "y", "z" }, array);
    }

    [Fact]
    public void CollectionExpression_CanBePassedAsParameter()
    {
        var result = AcceptOneOrMany([1, 2, 3]);

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void CollectionExpression_SupportsAddAfterCreation()
    {
        AutoArray<string> values = ["one", "two"];
        values.Add("three");

        Assert.Equal(new[] { "one", "two", "three" }, values);
    }

    [Fact]
    public void CollectionExpression_WithDuplicates_PreservesDuplicates()
    {
        AutoArray<int> values = [1, 2, 1, 3, 2];

        Assert.Equal(5, values.Count());
        Assert.Equal(new[] { 1, 2, 1, 3, 2 }, values);
    }

    [Fact]
    public void ImplicitConversion_AndCollectionExpression_BothWork()
    {
        // Collection expression
        AutoArray<int> fromExpression = [1, 2, 3];

        // Implicit conversion from single item
        AutoArray<int> fromSingle = 1;

        // Implicit conversion from array
        AutoArray<int> fromArray = new[] { 1, 2, 3 };

        Assert.Equal(new[] { 1, 2, 3 }, fromExpression);
        Assert.Equal(new[] { 1 }, fromSingle);
        Assert.Equal(new[] { 1, 2, 3 }, fromArray);
    }

    private static AutoArray<int> AcceptOneOrMany(AutoArray<int> value)
    {
        return value;
    }

    private class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

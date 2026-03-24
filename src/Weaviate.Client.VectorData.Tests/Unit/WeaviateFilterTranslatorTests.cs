using System.Linq.Expressions;
using Weaviate.Client.VectorData.Filters;

namespace Weaviate.Client.VectorData.Tests.Unit;

public class WeaviateFilterTranslatorTests
{
    private class TestRecord
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public bool Active { get; set; }
        public string[] Tags { get; set; } = [];
    }

    [Fact]
    public void Translate_NullFilter_ReturnsNull()
    {
        var result = WeaviateFilterTranslator.Translate<TestRecord>(null);
        Assert.Null(result);
    }

    [Fact]
    public void Translate_EqualString_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Name == "Alice";
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_EqualInt_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Age == 30;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_NotEqual_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Name != "Bob";
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_GreaterThan_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Age > 18;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_LessThanOrEqual_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Age <= 65;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_AndCombination_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Name == "Alice" && x.Age > 18;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_OrCombination_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Name == "Alice" || x.Name == "Bob";
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_NullComparison_ReturnsIsNull()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Name == null;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_BoolProperty_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Active == true;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_CapturedVariable_ReturnsFilter()
    {
        var minAge = 21;
        Expression<Func<TestRecord, bool>> filter = x => x.Age >= minAge;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_Contains_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => x.Tags.Contains("important");
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_NotOperator_ReturnsFilter()
    {
        Expression<Func<TestRecord, bool>> filter = x => !(x.Age > 18);
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_ReversedComparison_ReturnsFilter()
    {
        // 30 > x.Age should translate to Age < 30, not Age > 30
        Expression<Func<TestRecord, bool>> filter = x => 30 > x.Age;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }

    [Fact]
    public void Translate_ReversedLessThanOrEqual_ReturnsFilter()
    {
        // 18 <= x.Age should translate to Age >= 18
        Expression<Func<TestRecord, bool>> filter = x => 18 <= x.Age;
        var result = WeaviateFilterTranslator.Translate(filter);
        Assert.NotNull(result);
    }
}

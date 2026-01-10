using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<Weaviate.Client.Analyzers.AggregatePropertySuffixAnalyzer>;

namespace Weaviate.Client.Analyzers.Tests;

/// <summary>
/// The aggregate property suffix analyzer tests class
/// </summary>
public class AggregatePropertySuffixAnalyzerTests
{
    /// <summary>
    /// The test stub code
    /// </summary>
    private const string TestStubCode =
        @"
using System;
using System.Collections.Generic;

namespace Weaviate.Client.Models
{
    public class AggregateResult { }
}

namespace Weaviate.Client.Models.Typed
{
    using Weaviate.Client.Models;

    public static class TypedResultConverter
    {
        public static AggregateResult<T> ToTyped<T>(this AggregateResult result) where T : new()
        {
            return new AggregateResult<T>();
        }
    }

    public static class MetricsExtractor
    {
        public static string[] FromType<T>() => Array.Empty<string>();
    }

    public class AggregateResult<T> where T : new()
    {
        public T Properties { get; set; } = new T();
    }
}

namespace Weaviate.Client.Models.Aggregate
{
    public class Text { }
    public class Integer { }
    public class Number { }
    public class Boolean { }
    public class Date { }
}

namespace Weaviate.Client.Models.Typed
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TextMetricsAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class IntegerMetricsAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class NumberMetricsAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class BooleanMetricsAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class DateMetricsAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class MetricsForPropertyAttribute : Attribute
    {
        public MetricsForPropertyAttribute(string propertyName) { }
    }
}
";

    #region WEAVIATE005 - Missing Suffix Tests

    /// <summary>
    /// Tests that missing suffix on to typed call reports diagnostic
    /// </summary>
    [Fact]
    public async Task MissingSuffix_OnToTypedCall_ReportsDiagnostic()
    {
        var testCode =
            TestStubCode
            + @"
namespace Test
{
    using Weaviate.Client.Models;
    using Weaviate.Client.Models.Typed;

    public class MyModel
    {
        public int Age { get; set; }
    }

    public class TestClass
    {
        public void TestMethod()
        {
            var result = new AggregateResult();
            var typed = result.ToTyped<MyModel>();
        }
    }
}";

        var expected = VerifyCS
            .Diagnostic(AggregatePropertySuffixAnalyzer.MissingSuffixDiagnosticId)
            .WithArguments("Age", "MyModel")
            .WithSpan("/0/Test0.cs", 81, 40, 81, 47);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Tests that valid suffix as count no diagnostic
    /// </summary>
    [Fact]
    public async Task ValidSuffixAsCount_NoDiagnostic()
    {
        var testCode =
            TestStubCode
            + @"
namespace Test
{
    using Weaviate.Client.Models;
    using Weaviate.Client.Models.Typed;

    public class MyModel
    {
        public long? AgeAsCount { get; set; }
    }

    public class TestClass
    {
        public void TestMethod()
        {
            var result = new AggregateResult();
            var typed = result.ToTyped<MyModel>();
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    #endregion

    #region WEAVIATE006 - Invalid Suffix Type Tests

    /// <summary>
    /// Tests that invalid suffix type count on string reports diagnostic
    /// </summary>
    [Fact]
    public async Task InvalidSuffixType_CountOnString_ReportsDiagnostic()
    {
        var testCode =
            TestStubCode
            + @"
namespace Test
{
    using Weaviate.Client.Models;
    using Weaviate.Client.Models.Typed;

    public class MyModel
    {
        public string NameAsCount { get; set; }
    }

    public class TestClass
    {
        public void TestMethod()
        {
            var result = new AggregateResult();
            var typed = result.ToTyped<MyModel>();
        }
    }
}";

        var expected = VerifyCS
            .Diagnostic(AggregatePropertySuffixAnalyzer.InvalidSuffixTypeDiagnosticId)
            .WithArguments("NameAsCount", "MyModel", "Count", "long or int or double", "string")
            .WithSpan("/0/Test0.cs", 81, 40, 81, 47);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    #endregion

    #region WEAVIATE004 - Wrong Attribute Type Tests

    /// <summary>
    /// Tests that wrong attribute type number metrics on text reports diagnostic
    /// </summary>
    [Fact]
    public async Task WrongAttributeType_NumberMetricsOnText_ReportsDiagnostic()
    {
        var testCode =
            TestStubCode
            + @"
namespace Test
{
    using Weaviate.Client.Models;
    using Weaviate.Client.Models.Typed;
    using Weaviate.Client.Models.Aggregate;

    public class MyModel
    {
        [NumberMetrics]
        public Text Name { get; set; }
    }

    public class TestClass
    {
        public void TestMethod()
        {
            var result = new AggregateResult();
            var typed = result.ToTyped<MyModel>();
        }
    }
}";

        var expected = VerifyCS
            .Diagnostic(AggregatePropertySuffixAnalyzer.WrongAttributeTypeDiagnosticId)
            .WithArguments("Name", "Text", "Number")
            .WithSpan("/0/Test0.cs", 83, 40, 83, 47);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Tests that valid attribute type number metrics on number no diagnostic
    /// </summary>
    [Fact]
    public async Task ValidAttributeType_NumberMetricsOnNumber_NoDiagnostic()
    {
        var testCode =
            TestStubCode
            + @"
namespace Test
{
    using Weaviate.Client.Models;
    using Weaviate.Client.Models.Typed;
    using Weaviate.Client.Models.Aggregate;

    public class MyModel
    {
        [NumberMetrics]
        public Number Price { get; set; }
    }

    public class TestClass
    {
        public void TestMethod()
        {
            var result = new AggregateResult();
            var typed = result.ToTyped<MyModel>();
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    #endregion
}

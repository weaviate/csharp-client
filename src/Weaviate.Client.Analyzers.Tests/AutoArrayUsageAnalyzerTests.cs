using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<Weaviate.Client.Analyzers.AutoArrayUsageAnalyzer>;

namespace Weaviate.Client.Analyzers.Tests;

/// <summary>
/// The auto array usage analyzer tests class
/// </summary>
public class AutoArrayUsageAnalyzerTests
{
    /// <summary>
    /// The auto array source
    /// </summary>
    private const string AutoArraySource =
        @"
namespace Weaviate.Client.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class AutoArray<T> : IEnumerable<T>
    {
        private readonly List<T> _items = new List<T>();

        public AutoArray() { }

        public AutoArray(IEnumerable<T> items)
        {
            _items.AddRange(items);
        }

        public AutoArray(params T[] items)
            : this(items.AsEnumerable()) { }

        public static implicit operator AutoArray<T>(T item) => new AutoArray<T>(new[] { item });

        public static implicit operator AutoArray<T>(T[] items) => new AutoArray<T>(items);

        public static implicit operator AutoArray<T>(List<T> items) => new AutoArray<T>(items);

        public static explicit operator T[](AutoArray<T> list) => list._items.ToArray();

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void Add(params T[] items)
        {
            if (items != null)
                _items.AddRange(items);
        }
    }
}";

    /// <summary>
    /// Tests that field declaration reports diagnostic
    /// </summary>
    [Fact]
    public async Task FieldDeclaration_ReportsDiagnostic()
    {
        var testCode =
            AutoArraySource
            + @"

namespace TestNamespace
{
    using Weaviate.Client.Internal;

    class TestClass
    {
        private AutoArray<string> {|#0:_field|};
    }
}";

        var expected = VerifyCS
            .Diagnostic(AutoArrayUsageAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("field");

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Tests that property declaration reports diagnostic
    /// </summary>
    [Fact]
    public async Task PropertyDeclaration_ReportsDiagnostic()
    {
        var testCode =
            AutoArraySource
            + @"

namespace TestNamespace
{
    using Weaviate.Client.Internal;

    class TestClass
    {
        public AutoArray<string> {|#0:Property|} { get; set; }
    }
}";

        var expected = VerifyCS
            .Diagnostic(AutoArrayUsageAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("property");

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Tests that method parameter no diagnostic
    /// </summary>
    [Fact]
    public async Task MethodParameter_NoDiagnostic()
    {
        var testCode =
            AutoArraySource
            + @"

namespace TestNamespace
{
    using Weaviate.Client.Internal;

    class TestClass
    {
        public void Method(AutoArray<string> parameter)
        {
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Tests that local variable no diagnostic
    /// </summary>
    [Fact]
    public async Task LocalVariable_NoDiagnostic()
    {
        var testCode =
            AutoArraySource
            + @"

namespace TestNamespace
{
    using Weaviate.Client.Internal;

    class TestClass
    {
        public void Method()
        {
            AutoArray<string> localVar = new AutoArray<string>();
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Tests that multiple fields in declaration reports diagnostic for each
    /// </summary>
    [Fact]
    public async Task MultipleFieldsInDeclaration_ReportsDiagnosticForEach()
    {
        var testCode =
            AutoArraySource
            + @"

namespace TestNamespace
{
    using Weaviate.Client.Internal;

    class TestClass
    {
        private AutoArray<string> {|#0:_field1|}, {|#1:_field2|};
    }
}";

        var expected = new[]
        {
            VerifyCS
                .Diagnostic(AutoArrayUsageAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("field"),
            VerifyCS
                .Diagnostic(AutoArrayUsageAnalyzer.DiagnosticId)
                .WithLocation(1)
                .WithArguments("field"),
        };

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Tests that non auto array field no diagnostic
    /// </summary>
    [Fact]
    public async Task NonAutoArrayField_NoDiagnostic()
    {
        var testCode =
            @"
using System.Collections.Generic;

class TestClass
{
    private List<string> _field;
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }
}

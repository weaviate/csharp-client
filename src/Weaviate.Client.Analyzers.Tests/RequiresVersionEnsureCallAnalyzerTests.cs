using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<Weaviate.Client.Analyzers.RequiresVersionEnsureCallAnalyzer>;

namespace Weaviate.Client.Analyzers.Tests;

/// <summary>
/// Tests for WEAVIATE008 — methods decorated with [RequiresWeaviateVersion] must call EnsureVersion&lt;T&gt;().
/// </summary>
public class RequiresVersionEnsureCallAnalyzerTests
{
    /// <summary>
    /// Minimal stubs that reproduce the real types the analyzer looks for.
    /// </summary>
    private const string Stubs =
        @"
using System;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresWeaviateVersionAttribute : Attribute
{
    public RequiresWeaviateVersionAttribute(int major, int minor, int patch = 0) { }
}

public class WeaviateClient
{
    public Task EnsureVersion<T>(string operationName = """") => Task.CompletedTask;
}
";

    /// <summary>
    /// A method with the attribute that correctly calls EnsureVersion produces no diagnostic.
    /// </summary>
    [Fact]
    public async Task WithAttributeAndEnsureVersionCall_NoDiagnostic()
    {
        var testCode =
            Stubs
            + @"
public class CollectionConfigClient
{
    private WeaviateClient _client = new WeaviateClient();

    [RequiresWeaviateVersion(1, 36, 0)]
    public async Task DeletePropertyIndex()
    {
        await _client.EnsureVersion<CollectionConfigClient>();
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// A method with the attribute but no EnsureVersion call produces a diagnostic.
    /// </summary>
    [Fact]
    public async Task WithAttributeButMissingEnsureVersionCall_ReportsDiagnostic()
    {
        var testCode =
            Stubs
            + @"
public class CollectionConfigClient
{
    [RequiresWeaviateVersion(1, 36, 0)]
    public async Task {|#0:DeletePropertyIndex|}()
    {
        await Task.CompletedTask;
    }
}";

        var expected = VerifyCS
            .Diagnostic(RequiresVersionEnsureCallAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("DeletePropertyIndex", "CollectionConfigClient");

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// A method without the attribute does not need to call EnsureVersion.
    /// </summary>
    [Fact]
    public async Task WithoutAttribute_NoDiagnostic()
    {
        var testCode =
            Stubs
            + @"
public class CollectionConfigClient
{
    public async Task SomeRegularMethod()
    {
        await Task.CompletedTask;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// A method using the full attribute type name (RequiresWeaviateVersionAttribute) is also caught.
    /// </summary>
    [Fact]
    public async Task FullAttributeName_WithoutEnsureVersionCall_ReportsDiagnostic()
    {
        var testCode =
            Stubs
            + @"
public class SomeClient
{
    [RequiresWeaviateVersionAttribute(1, 35, 0)]
    public async Task {|#0:VersionedMethod|}()
    {
        await Task.CompletedTask;
    }
}";

        var expected = VerifyCS
            .Diagnostic(RequiresVersionEnsureCallAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("VersionedMethod", "SomeClient");

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// An EnsureVersion call anywhere in the method body (not just the first statement) satisfies the rule.
    /// </summary>
    [Fact]
    public async Task EnsureVersionCallInMiddleOfBody_NoDiagnostic()
    {
        var testCode =
            Stubs
            + @"
public class SomeClient
{
    private WeaviateClient _client = new WeaviateClient();

    [RequiresWeaviateVersion(1, 36, 0)]
    public async Task VersionedMethod(bool condition)
    {
        if (condition)
        {
            await _client.EnsureVersion<SomeClient>();
        }
        await Task.CompletedTask;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Two methods in the same class: one decorated correctly, one missing the call.
    /// Only the offending method should produce a diagnostic.
    /// </summary>
    [Fact]
    public async Task TwoMethods_OnlyDecoratedAndMissingCallReportsDiagnostic()
    {
        var testCode =
            Stubs
            + @"
public class SomeClient
{
    private WeaviateClient _client = new WeaviateClient();

    [RequiresWeaviateVersion(1, 36, 0)]
    public async Task GoodMethod()
    {
        await _client.EnsureVersion<SomeClient>();
    }

    [RequiresWeaviateVersion(1, 36, 0)]
    public async Task {|#0:BadMethod|}()
    {
        await Task.CompletedTask;
    }
}";

        var expected = VerifyCS
            .Diagnostic(RequiresVersionEnsureCallAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("BadMethod", "SomeClient");

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }
}

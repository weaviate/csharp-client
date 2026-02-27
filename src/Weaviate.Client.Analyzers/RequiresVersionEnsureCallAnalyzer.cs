using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Weaviate.Client.Analyzers;

/// <summary>
/// Ensures that any method decorated with <c>[RequiresWeaviateVersion]</c> contains a call
/// to <c>EnsureVersion&lt;T&gt;()</c>, so the minimum server version is enforced at runtime.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RequiresVersionEnsureCallAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic id
    /// </summary>
    public const string DiagnosticId = "WEAVIATE008";

    /// <summary>
    /// The category
    /// </summary>
    private const string Category = "Correctness";

    /// <summary>
    /// The title
    /// </summary>
    private static readonly LocalizableString Title =
        "Method with [RequiresWeaviateVersion] must call EnsureVersion<T>()";

    /// <summary>
    /// The message format
    /// </summary>
    private static readonly LocalizableString MessageFormat =
        "Method '{0}' is decorated with [RequiresWeaviateVersion] but does not call 'EnsureVersion<{1}>()'";

    /// <summary>
    /// The description
    /// </summary>
    private static readonly LocalizableString Description =
        "Methods decorated with [RequiresWeaviateVersion] must call 'await _client.EnsureVersion<TContainingType>()' "
        + "to enforce the minimum server version at runtime.";

    /// <summary>
    /// The rule
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description
    );

    /// <summary>
    /// Gets the value of the supported diagnostics
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    /// <summary>
    /// Initializes the context
    /// </summary>
    /// <param name="context">The context</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    /// <summary>
    /// Analyzes the method declaration using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;

        if (!HasRequiresWeaviateVersionAttribute(method))
            return;

        if (BodyContainsEnsureVersionCall(method))
            return;

        var containingType = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();

        var containingTypeName = containingType?.Identifier.Text ?? "T";

        var diagnostic = Diagnostic.Create(
            Rule,
            method.Identifier.GetLocation(),
            method.Identifier.Text,
            containingTypeName
        );

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Determines whether the method has the RequiresWeaviateVersion attribute.
    /// </summary>
    private static bool HasRequiresWeaviateVersionAttribute(MethodDeclarationSyntax method)
    {
        foreach (var attributeList in method.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                if (name == "RequiresWeaviateVersion" || name == "RequiresWeaviateVersionAttribute")
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if the method body (block or expression body) contains at least one
    /// invocation whose member name is <c>EnsureVersion</c>.
    /// </summary>
    private static bool BodyContainsEnsureVersionCall(MethodDeclarationSyntax method)
    {
        SyntaxNode? body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body is null)
            return false;

        return body.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation =>
            {
                var expression = invocation.Expression;

                // _client.EnsureVersion<T>() — member access
                if (expression is MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess.Name switch
                    {
                        GenericNameSyntax g => g.Identifier.Text == "EnsureVersion",
                        SimpleNameSyntax s => s.Identifier.Text == "EnsureVersion",
                        _ => false,
                    };
                }

                // EnsureVersion<T>() — direct (generic) name
                if (expression is GenericNameSyntax genericName)
                    return genericName.Identifier.Text == "EnsureVersion";

                // EnsureVersion() — bare identifier
                if (expression is IdentifierNameSyntax identifierName)
                    return identifierName.Identifier.Text == "EnsureVersion";

                return false;
            });
    }
}

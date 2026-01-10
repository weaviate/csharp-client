using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Weaviate.Client.Analyzers;

/// <summary>
/// The auto array usage analyzer class
/// </summary>
/// <seealso cref="DiagnosticAnalyzer"/>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoArrayUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic id
    /// </summary>
    public const string DiagnosticId = "WEAVIATE001";

    /// <summary>
    /// The category
    /// </summary>
    private const string Category = "Usage";

    /// <summary>
    /// The title
    /// </summary>
    private static readonly LocalizableString Title =
        "AutoArray<T> should only be used as method parameter";

    /// <summary>
    /// The message format
    /// </summary>
    private static readonly LocalizableString MessageFormat =
        "AutoArray<T> should only be used as a method parameter, not as a {0}";

    /// <summary>
    /// The description
    /// </summary>
    private static readonly LocalizableString Description =
        "AutoArray<T> is designed for flexible method parameters and should not be stored as fields or properties.";

    /// <summary>
    /// The description
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
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

        // Register for field declarations
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);

        // Register for property declarations
        context.RegisterSyntaxNodeAction(
            AnalyzePropertyDeclaration,
            SyntaxKind.PropertyDeclaration
        );
    }

    /// <summary>
    /// Analyzes the field declaration using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

        if (IsAutoArrayType(fieldDeclaration.Declaration.Type, context.SemanticModel))
        {
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    variable.Identifier.GetLocation(),
                    "field"
                );
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    /// <summary>
    /// Analyzes the property declaration using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        if (IsAutoArrayType(propertyDeclaration.Type, context.SemanticModel))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                propertyDeclaration.Identifier.GetLocation(),
                "property"
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Ises the auto array type using the specified type syntax
    /// </summary>
    /// <param name="typeSyntax">The type syntax</param>
    /// <param name="semanticModel">The semantic model</param>
    /// <returns>The bool</returns>
    private static bool IsAutoArrayType(TypeSyntax typeSyntax, SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
        var namedTypeSymbol = typeInfo.Type as INamedTypeSymbol;

        if (namedTypeSymbol == null)
            return false;

        // Check if it's AutoArray<T> - handle both generic and non-generic forms
        if (namedTypeSymbol.IsGenericType)
        {
            var constructedFrom = namedTypeSymbol.ConstructedFrom;
            return constructedFrom.Name == "AutoArray"
                && constructedFrom.ContainingNamespace.ToDisplayString()
                    == "Weaviate.Client.Models";
        }

        return namedTypeSymbol.Name == "AutoArray"
            && namedTypeSymbol.ContainingNamespace.ToDisplayString() == "Weaviate.Client.Models";
    }
}

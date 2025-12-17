using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Weaviate.Client.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoArrayUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "WEAVIATE001";
    private const string Category = "Usage";

    private static readonly LocalizableString Title =
        "AutoArray<T> should only be used as method parameter";
    private static readonly LocalizableString MessageFormat =
        "AutoArray<T> should only be used as a method parameter, not as a {0}";
    private static readonly LocalizableString Description =
        "AutoArray<T> is designed for flexible method parameters and should not be stored as fields or properties.";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

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

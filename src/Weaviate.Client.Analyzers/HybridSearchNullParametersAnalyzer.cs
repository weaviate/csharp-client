using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Weaviate.Client.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HybridSearchNullParametersAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "WEAVIATE002";
    private const string Category = "Usage";

    private static readonly LocalizableString Title =
        "Hybrid search requires at least one of 'query' or 'vectors' parameters";
    private static readonly LocalizableString MessageFormat =
        "At least one of 'query' or 'vectors' must be provided for hybrid search. Both parameters cannot be null.";
    private static readonly LocalizableString Description =
        "Hybrid search methods require either a keyword query (for BM25 search) or vector input (for vector search), or both. Calling Hybrid() with both parameters as null will throw an ArgumentException at runtime.";

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

        // Register for invocation expressions (method calls)
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Get the method symbol
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        // Check if the method is named "Hybrid"
        if (methodSymbol.Name != "Hybrid")
            return;

        // Check if the containing type is one of the Weaviate client types
        if (!IsWeaviateClientType(methodSymbol.ContainingType))
            return;

        // Analyze the arguments
        var argumentList = invocation.ArgumentList;
        if (argumentList == null)
            return;

        // Find the query and vectors arguments
        bool queryIsNull = false;
        bool vectorsIsNull = false;

        foreach (var argument in argumentList.Arguments)
        {
            // Get the parameter this argument is for
            var parameter = DetermineParameter(argument, methodSymbol, context.SemanticModel);
            if (parameter == null)
                continue;

            var parameterName = parameter.Name;

            // Check if this is the query or vectors parameter
            if (parameterName == "query")
            {
                queryIsNull = IsNullLiteral(argument.Expression);
            }
            else if (parameterName == "vectors")
            {
                vectorsIsNull = IsNullLiteral(argument.Expression);
            }
        }

        // If both arguments weren't explicitly provided, check if they have default values
        // If a parameter wasn't found in the arguments, it's using its default value
        var queryParam = methodSymbol.Parameters.FirstOrDefault(p => p.Name == "query");
        var vectorsParam = methodSymbol.Parameters.FirstOrDefault(p => p.Name == "vectors");

        // If query parameter wasn't in the argument list and has a default value of null
        if (
            queryParam != null
            && !argumentList.Arguments.Any(a =>
                DetermineParameter(a, methodSymbol, context.SemanticModel)?.Name == "query"
            )
        )
        {
            if (queryParam.HasExplicitDefaultValue && queryParam.ExplicitDefaultValue == null)
            {
                queryIsNull = true;
            }
        }

        // If vectors parameter wasn't in the argument list and has a default value of null
        if (
            vectorsParam != null
            && !argumentList.Arguments.Any(a =>
                DetermineParameter(a, methodSymbol, context.SemanticModel)?.Name == "vectors"
            )
        )
        {
            if (vectorsParam.HasExplicitDefaultValue && vectorsParam.ExplicitDefaultValue == null)
            {
                vectorsIsNull = true;
            }
        }

        // Report diagnostic if both are null
        if (queryIsNull && vectorsIsNull)
        {
            // Get the location of the method name, not the entire invocation
            var location = invocation.Expression is MemberAccessExpressionSyntax memberAccess
                ? memberAccess.Name.GetLocation()
                : invocation.Expression.GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsWeaviateClientType(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol == null)
            return false;

        var typeName = typeSymbol.Name;
        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

        // Check for QueryClient, GenerateClient, AggregateClient
        if (namespaceName == "Weaviate.Client")
        {
            return typeName == "QueryClient"
                || typeName == "GenerateClient"
                || typeName == "AggregateClient";
        }

        // Check for TypedQueryClient<T>, TypedGenerateClient<T>
        if (namespaceName == "Weaviate.Client.Typed")
        {
            if (typeSymbol.IsGenericType)
            {
                var constructedFrom = typeSymbol.ConstructedFrom;
                return constructedFrom.Name == "TypedQueryClient"
                    || constructedFrom.Name == "TypedGenerateClient";
            }
        }

        return false;
    }

    private static IParameterSymbol? DetermineParameter(
        ArgumentSyntax argument,
        IMethodSymbol methodSymbol,
        SemanticModel semanticModel
    )
    {
        // If the argument has a name colon (e.g., query: "test"), use that
        if (argument.NameColon != null)
        {
            var parameterName = argument.NameColon.Name.Identifier.Text;
            return methodSymbol.Parameters.FirstOrDefault(p => p.Name == parameterName);
        }

        // Otherwise, determine by position
        var argumentList = argument.Parent as ArgumentListSyntax;
        if (argumentList == null)
            return null;

        var index = argumentList.Arguments.IndexOf(argument);
        if (index < 0 || index >= methodSymbol.Parameters.Length)
            return null;

        return methodSymbol.Parameters[index];
    }

    private static bool IsNullLiteral(ExpressionSyntax expression)
    {
        // Check for literal null
        if (expression is LiteralExpressionSyntax literal)
        {
            return literal.Kind() == SyntaxKind.NullLiteralExpression;
        }

        // Check for default(T) where T is a reference type
        if (expression is DefaultExpressionSyntax)
        {
            return true;
        }

        // Check for cast expressions like (string)null
        if (expression is CastExpressionSyntax castExpression)
        {
            return IsNullLiteral(castExpression.Expression);
        }

        // Check for parenthesized expressions like (null)
        if (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            return IsNullLiteral(parenthesized.Expression);
        }

        return false;
    }
}

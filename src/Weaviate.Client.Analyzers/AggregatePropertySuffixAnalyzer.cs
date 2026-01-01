using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Weaviate.Client.Analyzers;

/// <summary>
/// Analyzer that validates aggregate property suffix usage in types used with the ToTyped&lt;T&gt;() extension method
/// for mapping aggregate results to strongly-typed objects.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AggregatePropertySuffixAnalyzer : DiagnosticAnalyzer
{
    public const string MissingSuffixDiagnosticId = "WEAVIATE002";
    public const string InvalidSuffixTypeDiagnosticId = "WEAVIATE003";
    private const string Category = "Usage";

    private static readonly LocalizableString MissingSuffixTitle =
        "Aggregate property missing suffix";
    private static readonly LocalizableString MissingSuffixMessageFormat =
        "Property '{0}' in type '{1}' has a primitive type but no recognized aggregate suffix";
    private static readonly LocalizableString MissingSuffixDescription =
        "Properties in aggregate result types used with ToTyped<T>() must either use full Aggregate types or have a recognized suffix.";

    private static readonly LocalizableString InvalidSuffixTypeTitle =
        "Invalid type for aggregate suffix";
    private static readonly LocalizableString InvalidSuffixTypeMessageFormat =
        "Property '{0}' in type '{1}' with suffix '{2}' should be of type '{3}', not '{4}'";
    private static readonly LocalizableString InvalidSuffixTypeDescription =
        "Properties with aggregate suffixes must use compatible types.";

    private static readonly DiagnosticDescriptor MissingSuffixRule = new DiagnosticDescriptor(
        MissingSuffixDiagnosticId,
        MissingSuffixTitle,
        MissingSuffixMessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: MissingSuffixDescription
    );

    private static readonly DiagnosticDescriptor InvalidSuffixTypeRule = new DiagnosticDescriptor(
        InvalidSuffixTypeDiagnosticId,
        InvalidSuffixTypeTitle,
        InvalidSuffixTypeMessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: InvalidSuffixTypeDescription
    );

    /// <summary>
    /// Recognized suffixes and their expected types.
    /// </summary>
    private static readonly Dictionary<string, string[]> SuffixExpectedTypes = new Dictionary<
        string,
        string[]
    >
    {
        // Numeric suffixes - can be long, int, double, float, decimal
        {
            "Count",
            new[]
            {
                "long",
                "int",
                "double",
                "float",
                "decimal",
                "long?",
                "int?",
                "double?",
                "float?",
                "decimal?",
            }
        },
        {
            "Sum",
            new[]
            {
                "long",
                "int",
                "double",
                "float",
                "decimal",
                "long?",
                "int?",
                "double?",
                "float?",
                "decimal?",
            }
        },
        { "Mean", new[] { "double", "float", "decimal", "double?", "float?", "decimal?" } },
        { "Average", new[] { "double", "float", "decimal", "double?", "float?", "decimal?" } },
        {
            "Min",
            new[]
            {
                "long",
                "int",
                "double",
                "float",
                "decimal",
                "long?",
                "int?",
                "double?",
                "float?",
                "decimal?",
            }
        },
        {
            "Minimum",
            new[]
            {
                "long",
                "int",
                "double",
                "float",
                "decimal",
                "long?",
                "int?",
                "double?",
                "float?",
                "decimal?",
            }
        },
        {
            "Max",
            new[]
            {
                "long",
                "int",
                "double",
                "float",
                "decimal",
                "long?",
                "int?",
                "double?",
                "float?",
                "decimal?",
            }
        },
        {
            "Maximum",
            new[]
            {
                "long",
                "int",
                "double",
                "float",
                "decimal",
                "long?",
                "int?",
                "double?",
                "float?",
                "decimal?",
            }
        },
        { "Median", new[] { "double", "float", "decimal", "double?", "float?", "decimal?" } },
        {
            "Mode",
            new[]
            {
                "long",
                "int",
                "double",
                "float",
                "decimal",
                "long?",
                "int?",
                "double?",
                "float?",
                "decimal?",
            }
        },
        // Boolean-specific suffixes
        { "TotalTrue", new[] { "long", "int", "long?", "int?" } },
        { "TotalFalse", new[] { "long", "int", "long?", "int?" } },
        {
            "PercentageTrue",
            new[] { "double", "float", "decimal", "double?", "float?", "decimal?" }
        },
        {
            "PercentageFalse",
            new[] { "double", "float", "decimal", "double?", "float?", "decimal?" }
        },
        // Text-specific suffixes
        { "TopOccurrence", new[] { "string", "string?" } },
        {
            "TopOccurrences",
            new[]
            {
                "List<Aggregate.TopOccurrence<string>>",
                "IReadOnlyList<Aggregate.TopOccurrence<string>>",
                "IList<Aggregate.TopOccurrence<string>>",
                "IEnumerable<Aggregate.TopOccurrence<string>>",
            }
        },
    };

    /// <summary>
    /// Date-specific suffixes that require DateTime types.
    /// </summary>
    private static readonly HashSet<string> DateSuffixes = new HashSet<string>
    {
        "Minimum",
        "Min",
        "Maximum",
        "Max",
        "Median",
        "Mode",
    };

    /// <summary>
    /// The fully qualified name of the ToTyped extension method container.
    /// </summary>
    private const string TypedResultConverterFullName =
        "Weaviate.Client.Models.Typed.TypedResultConverter";

    /// <summary>
    /// The fully qualified name of the MetricsExtractor class.
    /// </summary>
    private const string MetricsExtractorFullName = "Weaviate.Client.Models.Typed.MetricsExtractor";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingSuffixRule, InvalidSuffixTypeRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for invocation expressions to detect ToTyped<T>() calls
        context.RegisterSyntaxNodeAction(
            AnalyzeInvocationExpression,
            SyntaxKind.InvocationExpression
        );
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a ToTyped<T>() or MetricsExtractor.FromType<T>() call
        TypeSyntax? typeArgument = null;
        if (
            !IsToTypedCall(invocation, context.SemanticModel, out typeArgument)
            && !IsFromTypeCall(invocation, context.SemanticModel, out typeArgument)
        )
            return;

        // Get the type symbol for the type argument
        if (typeArgument == null)
            return;

        var typeInfo = context.SemanticModel.GetTypeInfo(typeArgument);
        var typeSymbol = typeInfo.Type as INamedTypeSymbol;
        if (typeSymbol == null)
            return;

        // Analyze all properties of the type
        AnalyzeTypeProperties(context, typeArgument, typeSymbol);
    }

    private static bool IsToTypedCall(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        out TypeSyntax? typeArgument
    )
    {
        typeArgument = null;

        // Look for member access like result.ToTyped<T>()
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // Check if the method name is ToTyped and has type arguments
            if (memberAccess.Name is GenericNameSyntax genericName)
            {
                if (
                    genericName.Identifier.Text == "ToTyped"
                    && genericName.TypeArgumentList.Arguments.Count == 1
                )
                {
                    // Verify this is actually the TypedResultConverter.ToTyped method
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                    if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                    {
                        var containingType = methodSymbol.ContainingType?.ToDisplayString();
                        if (containingType == TypedResultConverterFullName)
                        {
                            typeArgument = genericName.TypeArgumentList.Arguments[0];
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private static bool IsFromTypeCall(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        out TypeSyntax? typeArgument
    )
    {
        typeArgument = null;

        // Look for MetricsExtractor.FromType<T>() call
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // Check if the method name is FromType and has type arguments
            if (memberAccess.Name is GenericNameSyntax genericName)
            {
                if (
                    genericName.Identifier.Text == "FromType"
                    && genericName.TypeArgumentList.Arguments.Count == 1
                )
                {
                    // Verify this is actually the MetricsExtractor.FromType method
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                    if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                    {
                        var containingType = methodSymbol.ContainingType?.ToDisplayString();
                        if (containingType == MetricsExtractorFullName)
                        {
                            typeArgument = genericName.TypeArgumentList.Arguments[0];
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private static void AnalyzeTypeProperties(
        SyntaxNodeAnalysisContext context,
        TypeSyntax typeArgumentSyntax,
        INamedTypeSymbol typeSymbol
    )
    {
        // Get all public instance properties
        var properties = typeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);

        foreach (var property in properties)
        {
            var propertyType = property.Type;

            // Skip if this is a full Aggregate.* type
            if (IsAggregateType(propertyType))
                continue;

            // Only analyze primitive/value types and string
            if (!IsPrimitiveOrValueType(propertyType))
                continue;

            var propertyName = property.Name;
            var suffix = GetSuffix(propertyName);

            if (suffix == null)
            {
                // Property has a primitive type but no recognized suffix
                var diagnostic = Diagnostic.Create(
                    MissingSuffixRule,
                    typeArgumentSyntax.GetLocation(),
                    propertyName,
                    typeSymbol.Name
                );
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            // Validate the type is appropriate for the suffix
            ValidateSuffixType(
                context,
                typeArgumentSyntax,
                typeSymbol.Name,
                propertyName,
                suffix,
                propertyType
            );
        }
    }

    private static bool IsAggregateType(ITypeSymbol type)
    {
        var displayName = type.ToDisplayString();
        return displayName.StartsWith("Weaviate.Client.Models.Aggregate.", StringComparison.Ordinal)
            || displayName == "Weaviate.Client.Models.Aggregate.Text"
            || displayName == "Weaviate.Client.Models.Aggregate.Integer"
            || displayName == "Weaviate.Client.Models.Aggregate.Number"
            || displayName == "Weaviate.Client.Models.Aggregate.Boolean"
            || displayName == "Weaviate.Client.Models.Aggregate.Date"
            || displayName == "Weaviate.Client.Models.Aggregate.Property";
    }

    private static bool IsPrimitiveOrValueType(ITypeSymbol type)
    {
        // Handle nullable types
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var originalDef = namedType.OriginalDefinition.SpecialType;
            if (originalDef == SpecialType.System_Nullable_T)
            {
                type = namedType.TypeArguments[0];
            }
        }

        return type.SpecialType == SpecialType.System_Int32
            || type.SpecialType == SpecialType.System_Int64
            || type.SpecialType == SpecialType.System_Double
            || type.SpecialType == SpecialType.System_Single
            || type.SpecialType == SpecialType.System_Decimal
            || type.SpecialType == SpecialType.System_String
            || type.SpecialType == SpecialType.System_DateTime
            || type.SpecialType == SpecialType.System_Boolean;
    }

    private static string? GetSuffix(string propertyName)
    {
        foreach (var suffix in SuffixExpectedTypes.Keys)
        {
            if (
                propertyName.Length > suffix.Length
                && propertyName.EndsWith(suffix, StringComparison.Ordinal)
            )
            {
                return suffix;
            }
        }

        return null;
    }

    private static void ValidateSuffixType(
        SyntaxNodeAnalysisContext context,
        TypeSyntax typeArgumentSyntax,
        string typeName,
        string propertyName,
        string suffix,
        ITypeSymbol propertyType
    )
    {
        var typeDisplayName = GetSimpleTypeName(propertyType);

        // DateTime properties with date suffixes are valid
        if (
            DateSuffixes.Contains(suffix)
            && (
                propertyType.SpecialType == SpecialType.System_DateTime
                || IsNullableDateTime(propertyType)
            )
        )
        {
            return; // Valid - DateTime with date suffix
        }

        if (!SuffixExpectedTypes.TryGetValue(suffix, out var expectedTypes))
            return;

        // Check if the type matches any of the expected types
        if (!expectedTypes.Contains(typeDisplayName))
        {
            var diagnostic = Diagnostic.Create(
                InvalidSuffixTypeRule,
                typeArgumentSyntax.GetLocation(),
                propertyName,
                typeName,
                suffix,
                string.Join(" or ", expectedTypes.Take(3)),
                typeDisplayName
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullableDateTime(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var originalDef = namedType.OriginalDefinition.SpecialType;
            if (originalDef == SpecialType.System_Nullable_T)
            {
                return namedType.TypeArguments[0].SpecialType == SpecialType.System_DateTime;
            }
        }

        return false;
    }

    private static string GetSimpleTypeName(ITypeSymbol type)
    {
        // Handle nullable types
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var originalDef = namedType.OriginalDefinition.SpecialType;
            if (originalDef == SpecialType.System_Nullable_T)
            {
                var innerType = namedType.TypeArguments[0];
                return GetSimpleTypeName(innerType) + "?";
            }
        }

        return type.SpecialType switch
        {
            SpecialType.System_Int32 => "int",
            SpecialType.System_Int64 => "long",
            SpecialType.System_Double => "double",
            SpecialType.System_Single => "float",
            SpecialType.System_Decimal => "decimal",
            SpecialType.System_String => "string",
            SpecialType.System_DateTime => "DateTime",
            SpecialType.System_Boolean => "bool",
            _ => type.ToDisplayString(),
        };
    }
}

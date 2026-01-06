using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Weaviate.Client.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class VectorizerFactoryAnalyzer : DiagnosticAnalyzer
{
    public const string MissingPropertyDiagnosticId = "WEAVIATE002";
    public const string MissingWeightFieldDiagnosticId = "WEAVIATE003";

    private const string Category = "Usage";

    private static readonly LocalizableString MissingPropertyTitle =
        "Vectorizer factory method missing property initialization or parameter";
    private static readonly LocalizableString MissingPropertyMessageFormat =
        "Factory method creating '{0}' does not have a way to set property '{1}'. Add a parameter for it or initialize it in the object initializer.";
    private static readonly LocalizableString MissingPropertyDescription =
        "All public properties of a vectorizer config should be initializable through factory method parameters or explicitly initialized in the object initializer to ensure completeness.";

    private static readonly LocalizableString MissingWeightFieldTitle =
        "Vectorizer factory method missing field in Weights calculation";
    private static readonly LocalizableString MissingWeightFieldMessageFormat =
        "Factory method creating '{0}' with WeightedFields should include '{1}' in Weights calculation";
    private static readonly LocalizableString MissingWeightFieldDescription =
        "When a factory method accepts WeightedFields parameters, all of them should be included in the VectorizerWeights.FromWeightedFields() call.";

    private static readonly DiagnosticDescriptor MissingPropertyRule = new DiagnosticDescriptor(
        MissingPropertyDiagnosticId,
        MissingPropertyTitle,
        MissingPropertyMessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: MissingPropertyDescription
    );

    private static readonly DiagnosticDescriptor MissingWeightFieldRule = new DiagnosticDescriptor(
        MissingWeightFieldDiagnosticId,
        MissingWeightFieldTitle,
        MissingWeightFieldMessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: MissingWeightFieldDescription
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingPropertyRule, MissingWeightFieldRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for method declarations in VectorizerFactory classes
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        // Only analyze methods in VectorizerFactory or VectorizerFactoryMulti classes
        var containingClass = methodDeclaration.Parent as ClassDeclarationSyntax;
        if (
            containingClass == null
            || (
                containingClass.Identifier.Text != "VectorizerFactory"
                && containingClass.Identifier.Text != "VectorizerFactoryMulti"
            )
        )
        {
            return;
        }

        // Find the object creation expression
        var objectCreation = methodDeclaration
            .DescendantNodes()
            .OfType<ObjectCreationExpressionSyntax>()
            .FirstOrDefault();

        if (objectCreation == null)
            return;

        var semanticModel = context.SemanticModel;
        var typeInfo = semanticModel.GetTypeInfo(objectCreation);
        var namedType = typeInfo.Type as INamedTypeSymbol;

        if (namedType == null)
            return;

        // Only analyze types in Weaviate.Client.Models.Vectorizer namespace
        if (
            !namedType.ContainingNamespace.ToDisplayString().Contains("Weaviate.Client.Models")
            || !IsVectorizerConfig(namedType)
        )
        {
            return;
        }

        // Check for missing properties
        AnalyzePropertyInitialization(
            context,
            methodDeclaration,
            objectCreation,
            namedType,
            semanticModel
        );

        // Check for missing fields in Weights calculation
        AnalyzeWeightsCalculation(context, methodDeclaration, objectCreation, namedType);
    }

    private static void AnalyzePropertyInitialization(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        ObjectCreationExpressionSyntax objectCreation,
        INamedTypeSymbol namedType,
        SemanticModel semanticModel
    )
    {
        // Get all public settable properties from the vectorizer type (excluding internal Weights)
        var properties = namedType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p =>
                p.DeclaredAccessibility == Accessibility.Public
                && p.SetMethod != null
                && p.Name != "Weights"
            )
            .ToList();

        if (!properties.Any())
            return;

        // Get all initialized properties in the object initializer
        var initializer = objectCreation.Initializer;
        if (initializer == null)
            return;

        var initializedProperties = new System.Collections.Generic.HashSet<string>(
            initializer
                .Expressions.OfType<AssignmentExpressionSyntax>()
                .Select(assignment =>
                {
                    var left = assignment.Left as IdentifierNameSyntax;
                    return left?.Identifier.Text;
                })
                .Where(name => name != null)!
        );

        // Check for missing required properties and optional properties with corresponding parameters
        var methodParameters = methodDeclaration.ParameterList.Parameters;
        var parameterNames = new System.Collections.Generic.HashSet<string>(
            methodParameters.Select(p => p.Identifier.Text.ToLowerInvariant())
        );

        foreach (var property in properties)
        {
            // Skip if already initialized
            if (initializedProperties.Contains(property.Name))
                continue;

            // Check if there's a corresponding parameter for this property
            var propertyNameLower = property.Name.ToLowerInvariant();

            // Convert PascalCase to camelCase for property name
            var camelCasePropertyName =
                property.Name.Length > 0
                    ? char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1)
                    : property.Name;

            var hasCorrespondingParameter =
                parameterNames.Contains(propertyNameLower)
                || parameterNames.Contains(camelCasePropertyName.ToLowerInvariant());

            // Warn if property is not initialized AND either:
            // 1. There's a corresponding parameter (parameter exists but not used), OR
            // 2. There's NO parameter and property isn't initialized (property can't be set via factory)
            // This ensures all properties can be set through the factory method
            var diagnostic = Diagnostic.Create(
                MissingPropertyRule,
                methodDeclaration.Identifier.GetLocation(),
                namedType.Name,
                property.Name
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeWeightsCalculation(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        ObjectCreationExpressionSyntax objectCreation,
        INamedTypeSymbol namedType
    )
    {
        // Find WeightedFields parameters
        var weightedFieldsParams = methodDeclaration
            .ParameterList.Parameters.Where(p =>
                p.Type?.ToString().Contains("WeightedFields") == true
            )
            .Select(p => p.Identifier.Text)
            .ToList();

        if (!weightedFieldsParams.Any())
            return;

        // Check if there's a Weights property assignment
        var initializer = objectCreation.Initializer;
        if (initializer == null)
            return;

        var weightsAssignment = initializer
            .Expressions.OfType<AssignmentExpressionSyntax>()
            .FirstOrDefault(a =>
            {
                var left = a.Left as IdentifierNameSyntax;
                return left?.Identifier.Text == "Weights";
            });

        if (weightsAssignment == null)
            return; // No Weights property set, which is okay for some vectorizers

        // Find the FromWeightedFields invocation
        var invocation = weightsAssignment
            .Right.DescendantNodesAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(inv =>
            {
                var memberAccess = inv.Expression as MemberAccessExpressionSyntax;
                return memberAccess?.Name.Identifier.Text == "FromWeightedFields";
            });

        if (invocation == null)
            return;

        // Get arguments passed to FromWeightedFields
        var passedArguments = new System.Collections.Generic.HashSet<string>(
            invocation
                .ArgumentList.Arguments.Select(arg =>
                {
                    // Handle both positional and named arguments
                    if (arg.NameColon != null)
                    {
                        return arg.NameColon.Name.Identifier.Text;
                    }
                    // For positional args, get the identifier
                    var identifier = arg.Expression as IdentifierNameSyntax;
                    return identifier?.Identifier.Text;
                })
                .Where(name => name != null)!
        );

        // Check if all WeightedFields parameters are included
        foreach (var param in weightedFieldsParams)
        {
            if (!passedArguments.Contains(param))
            {
                var diagnostic = Diagnostic.Create(
                    MissingWeightFieldRule,
                    weightsAssignment.GetLocation(),
                    namedType.Name,
                    param
                );
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsVectorizerConfig(INamedTypeSymbol type)
    {
        // Check if type inherits from VectorizerConfig
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "VectorizerConfig")
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }
}
